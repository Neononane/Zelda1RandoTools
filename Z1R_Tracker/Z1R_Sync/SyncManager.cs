using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;


namespace Z1R_Sync
{
    public static class SyncManager
    {
        private static string _AzureFunctionUrl;

        private static readonly HttpClient _httpClient = new HttpClient();
        private static HubConnection _connection;

        // Delegates for handling different kinds of messages
        private static Action<string, string, string> _onTileChange;
        private static Action<string, string, string> _onSyncMessage;

        private static string lastSentDoorChangePayload = null;

        public static void MarkLastSentDoorChange(string payload)
        {
            lastSentDoorChangePayload = payload;
        }

        public static bool ShouldSuppressDoorChange(string payload)
        {
            return lastSentDoorChangePayload == payload;
        }

        private static string lastSentRoomChangePayload = null;

        public static void MarkLastSentRoomChange(string payload)
        {
            lastSentRoomChangePayload = payload;
        }

        public static bool ShouldSuppressRoomChange(string payload)
        {
            return lastSentRoomChangePayload == payload;
        }
        private static bool _suppressRoomChange = false;

        public static void BeginSuppressingRoomChanges()
        {
            _suppressRoomChange = true;
        }

        public static void EndSuppressingRoomChanges()
        {
            _suppressRoomChange = false;
        }

        public static bool IsSuppressingRoomChanges()
        {
            return _suppressRoomChange;
        }


        public static void Configure(string azureFunctionUrl)
        {
            _AzureFunctionUrl = azureFunctionUrl;
        }

        public static string AzureFunctionUrl => _AzureFunctionUrl;

        public static void SetTileChangeHandler(Action<string, string, string> handler)
        {
            _onTileChange = handler;
        }

        public static void SetSyncMessageHandler(Action<string, string, string> handler)
        {
            _onSyncMessage = handler;
        }

        public static async Task Send(string msgType, string payload, string senderId)
        {
            if (string.IsNullOrEmpty(AzureFunctionUrl))
                throw new InvalidOperationException("AzureFunctionUrl is not set");

            var request = new
            {
                messageType = msgType,
                payload = payload,
                senderId = senderId
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var stopwatch = Stopwatch.StartNew();
                Console.WriteLine($"[Sync] Sending message {msgType} from {senderId} with payload length {json.Length}");

                var response = await _httpClient.PostAsync(AzureFunctionUrl, content);

                stopwatch.Stop();
                Console.WriteLine($"[Sync] Response for {msgType}: {(int)response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms");

                if (!response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Sync] Response content: {responseText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sync] ERROR sending message {msgType}: {ex.Message}");
            }
        }






        private static Dictionary<string, string> _pendingMessages = new Dictionary<string, string>();
        private static System.Timers.Timer _debounceTimer;

        public static void SendModelUpdate(string msgType, object model, string senderId)
        {
            var payload = JsonConvert.SerializeObject(model);
            lock (_pendingMessages)
            {
                _pendingMessages[msgType] = payload;
            }

            if (_debounceTimer == null)
            {
                _debounceTimer = new System.Timers.Timer(250); // 250 ms
                _debounceTimer.Elapsed += async (sender, e) =>
                {
                    Dictionary<string, string> toSend;
                    lock (_pendingMessages)
                    {
                        toSend = new Dictionary<string, string>(_pendingMessages);
                        _pendingMessages.Clear();
                    }

                    foreach (var kvp in toSend)
                    {
                        await Send(kvp.Key, kvp.Value, senderId);
                    }
                };
                _debounceTimer.AutoReset = false;
            }

            _debounceTimer.Stop();
            _debounceTimer.Start();
        }




        public static async Task StartAsync(string negotiateUrl)
        {
            try
            {
                // Step 1: Get SignalR connection info from Azure Function
                //RPT I changed this from Post to Get. We will see
                var response = await _httpClient.GetAsync(negotiateUrl);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<SignalRNegotiation>(json);

                // Step 2: Build SignalR connection
                _connection = new HubConnectionBuilder()
                    .WithUrl(info.url, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(info.accessToken);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // Step 3: Register tile change handler
                _connection.On<object>("ReceiveMapUpdate", payload =>
                {
                    try
                    {
                        var jsonPayload = payload.ToString();
                        var update = JsonConvert.DeserializeObject<MapUpdate>(jsonPayload);
                        _onTileChange?.Invoke(update.tileId, update.iconId, update.senderId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SyncManager] Failed to parse tile update: {ex.Message}");
                    }
                });

                // Step 4: Register structured message handler
                _connection.On<SyncMessage>("ReceiveSyncMessage", message =>
                {
                    try
                    {
                        //var payloadJson = JsonConvert.SerializeObject(message.payload);
                        var payloadJson = message.payload.ToString();
                        _onSyncMessage?.Invoke(message.messageType, payloadJson, message.senderId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SyncManager] Failed to process sync message: {ex.Message}");
                    }
                });

                // Step 5: Connect
                await _connection.StartAsync();
                System.Diagnostics.Debug.WriteLine("[SyncManager] Connected to SignalR hub");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SyncManager] ERROR during startup: {ex.Message}");
                throw;
            }
        }

        // === Supporting Data Structures ===

        private class SignalRNegotiation
        {
            public string url { get; set; }
            public string accessToken { get; set; }
        }

        private class MapUpdate
        {
            public string tileId { get; set; }
            public string iconId { get; set; }
            public string senderId { get; set; }
        }

        public class SyncMessage
        {
            public string messageType { get; set; }
            public string senderId { get; set; }
            public object payload { get; set; }
        }
    }
}
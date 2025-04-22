using System;
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
        private static string AzureFunctionUrl = "https://ztrackersync.azurewebsites.net/api/SyncUpdate";
        private static readonly HttpClient _httpClient = new HttpClient();
        private static HubConnection _connection;

        // Delegates for handling different kinds of messages
        private static Action<string, string, string> _onTileChange;
        private static Action<string, string, string> _onSyncMessage;

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

            var client = new HttpClient();
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
                var response = await client.PostAsync(AzureFunctionUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Sync] Azure Function call failed: {response.StatusCode} - {responseText}");
                }
                else
                {
                    Console.WriteLine($"[Sync] Azure Function POST succeeded for msgType={msgType}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sync] Failed to send message via Azure Function: {ex.Message}");
            }
        }




        public static void SendModelUpdate(string msgType, object model, string senderId)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(model);
                _ = Send(msgType, payload, senderId);  // Fire and forget
                Debug.WriteLine($"[Sync] Sent {msgType} update: {payload}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Sync] Failed to send {msgType} update: {ex.Message}");
            }
        }



        public static async Task StartAsync(string negotiateUrl)
        {
            try
            {
                // Step 1: Get SignalR connection info from Azure Function
                var response = await _httpClient.PostAsync(negotiateUrl, null);
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
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;


namespace Z1R_Sync
{
    public static class SyncManager
    {
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
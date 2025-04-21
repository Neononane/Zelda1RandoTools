using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace Z1R_Sync
{
    public static class SyncManager
    {
        private static HubConnection _connection;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static Action<string, string> _onTileChange;

        public static async Task StartAsync(string negotiateUrl)
        {
            try
            {
                // Step 1: Negotiate with Azure Function to get SignalR connection info
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

                // Step 3: Register handler for incoming messages
                _connection.On<object>("ReceiveMapUpdate", payload =>
                {
                    try
                    {
                        var jsonPayload = payload.ToString();
                        var update = JsonConvert.DeserializeObject<MapUpdate>(jsonPayload);
                        _onTileChange?.Invoke(update.tileId, update.iconId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SyncManager] Failed to parse payload: {ex.Message}");
                    }
                });

                // Step 4: Connect to SignalR hub
                await _connection.StartAsync();
                System.Diagnostics.Debug.WriteLine("[SyncManager] Connected to SignalR hub");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SyncManager] ERROR during startup: {ex.Message}");
                throw;
            }
        }

        public static void SetTileChangeHandler(Action<string, string> handler)
        {
            _onTileChange = handler;
        }

        // Optional: Send outbound messages (e.g., triggered by tile clicks)
        public static async Task RaiseTileChangeAsync(string tileId, string iconId, string senderId)
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                var payload = new MapUpdate { tileId = tileId, iconId = iconId, senderId = senderId };
                await _connection.SendAsync("ReceiveMapUpdate", payload);
            }
        }

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
    }
}

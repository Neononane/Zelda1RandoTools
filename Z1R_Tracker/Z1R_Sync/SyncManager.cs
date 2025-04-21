using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using Newtonsoft.Json;

namespace Z1R_Sync
{
    public static class SyncManager
    {
        private static HubConnection _connection;
        private static Action<string, string> _onTileChange;
        private static HttpClient _httpClient = new HttpClient();

        // Call this from F# at app start
        public static async Task StartAsync(string signalRHubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(signalRHubUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string>("TileChanged", (tileId, iconId) =>
            {
                _onTileChange?.Invoke(tileId, iconId);
            });

            await _connection.StartAsync();
        }

        // Called by F# when a user triggers a state change
        public static async Task RaiseTileChangeAsync(string tileId, string iconId)
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                await _connection.SendAsync("BroadcastTileChange", tileId, iconId);
            }
        }

        // Optional: use this to call a separate Azure Function (e.g., auditing)
        public static async Task SendToFunctionAsync(string url, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(url, content);
        }

        // F# provides a handler for applying remote tile changes
        public static void SetTileChangeHandler(Action<string, string> handler)
        {
            _onTileChange = handler;
        }
    }
}

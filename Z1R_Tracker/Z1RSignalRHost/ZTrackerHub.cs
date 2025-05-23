using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LocalSignalRServer
{
    public class ZTrackerHub : Hub
    {
        public async Task SendUpdate(string messageType, string payloadJson, string senderId)
        {
            await Clients.Others.SendAsync("ReceiveUpdate", messageType, payloadJson, senderId);
        }
    }
}

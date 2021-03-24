using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RepoAnalyser.SignalR.Hubs;
using RepoAnalyser.SignalR.Objects;

namespace RepoAnalyser.SignalR.Helpers
{
    public static class SignalRHelper
    {
        public static Task DirectNotify(this IHubContext<AppHub, IAppHub> hub, string connectionId, string message, SignalRNotificationType type)
        {
            return hub.Clients.Client(connectionId).DirectNotification(connectionId, message, type);
        }
    }
}

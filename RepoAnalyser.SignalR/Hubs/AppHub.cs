using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RepoAnalyser.Objects.Attributes;
using RepoAnalyser.SignalR.Objects;

namespace RepoAnalyser.SignalR.Hubs
{
    [ScrutorIgnore]
    public class AppHub : Hub<IAppHub>
    {
        //Example of calling a specific client to notify 
        //private readonly IHubContext<AppHub, IAppHub> _hub;
        //await _hub.Clients.Clients(connectionId).DirectNotification(connectionId, "About to calculate code owners",
        //    SignalRNotificationType.RepoAnalysisProgressUpdate);

        public Task DirectNotification(string user, string message, SignalRNotificationType type)
        {
            return Clients.User(user).DirectNotification( user, message, type);
        }
    }

    public interface IAppHub
    {
        Task DirectNotification(string user, string message, SignalRNotificationType type);
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.SignalR.Hubs
{
    [ScrutorIgnore]
    public class AppHub : Hub<IAppHub>
    {
        // private readonly IHubContext<AppHub, IAppHub> _appHub; strongly typed DI example
        public Task Test(string message)
        {
            return Clients.All.Test(message);
        }
    }

    public interface IAppHub
    {
        Task Test(string message);
    }
}

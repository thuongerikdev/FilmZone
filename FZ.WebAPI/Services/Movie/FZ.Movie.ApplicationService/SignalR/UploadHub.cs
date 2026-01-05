using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using VimeoDotNet.Models;

namespace FZ.WebAPI.SignalR
{
    public class UploadHub : Hub
    {
        public Task JoinJob(string jobId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, jobId);
    }
}

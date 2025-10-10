using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FriendlyRS1.SignalRChat.Hubs
{
    public class CommentHub : Hub
    {
        public async Task JoinPostGroup(int postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"post_{postId}");
        }

        public async Task LeavePostGroup(int postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post_{postId}");
        }
    }
}

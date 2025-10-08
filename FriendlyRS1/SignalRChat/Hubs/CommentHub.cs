using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FriendlyRS1.SignalRChat.Hubs
{
    public class CommentHub : Hub
    {
        // When a user views a post, they join that post's "room"
        public async Task JoinPostGroup(int postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"post_{postId}");
        }

        // When user leaves the post (e.g., navigates away)
        public async Task LeavePostGroup(int postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post_{postId}");
        }

        // Send comment only to users currently viewing that post
        public async Task SendComment(int postId, string author, string text, string date)
        {
            await Clients.Group($"post_{postId}").SendAsync("ReceiveComment", postId, author, text, date);
        }

        // Optional: Handle comment updates in real time
        public async Task UpdateComment(int postId, int commentId, string text, string date)
        {
            await Clients.Group($"post_{postId}").SendAsync("UpdateComment", postId, commentId, text, date);
        }

        // Optional: Handle comment deletions in real time
        public async Task DeleteComment(int postId, int commentId)
        {
            await Clients.Group($"post_{postId}").SendAsync("DeleteComment", postId, commentId);
        }
    }
}

using DataLayer.EntityModels;
using FriendlyRS1.Repository.RepostorySetup;
using FriendlyRS1.SignalRChat.Hubs;
using FriendlyRS1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FriendlyRS1.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHubContext<CommentHub> _hubContext;

        public CommentController(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IHubContext<CommentHub> hubContext)
        {
            _userManager = userManager;
            this.unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> GetComments(int postId)
        {
            try
            {
                var loggedUser = await _userManager.GetUserAsync(User);
                List<CommentVM> Comments = unitOfWork.Comment.GetCommentsByPostId(postId)
                    .OrderBy(c => c.DateCreated)
                    .Select(c => new CommentVM
                    {
                        Id = c.Id,
                        AuthorId = c.AuthorId,
                        AuthorName = c.Author.ToString(),
                        Text = c.Text,
                        DateCreated = c.DateCreated.ToString("M/d/yyyy, hh:mm tt"),
                        DateUpdated = c.DateUpdated.ToString("M/d/yyyy, hh:mm tt"),
                        IsMe = loggedUser.Id == c.AuthorId
                    })
            .ToList();

                return Ok(new { success = true, comments = Comments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public async Task<IActionResult> AddComment(int postId, string text)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)

                return Unauthorized();
            var userId = Convert.ToInt32(_userManager.GetUserId(User));
            var author = (user.FirstName + " " + user.LastName)?.Trim() ?? "Unknown";

            var comment = new Comment
            {
                PostId = postId,
                Text = text,
                AuthorId = userId,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now
            };

            try
            {
                unitOfWork.Comment.Add(comment);
                unitOfWork.Complete();

                await _hubContext.Clients.Group($"post_{postId}")
                    .SendAsync("ReloadComments", postId);

                return Ok(new
                {
                    success = true,
                    author,
                    text = comment.Text,
                    date = comment.DateCreated.ToString("o"),
                    commentId = comment.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public async Task<IActionResult> UpdateComment(int id, string text)
        {
            var comment = unitOfWork.Comment.Find(id);
            if (comment == null)
                return NotFound();

            comment.Text = text;
            comment.DateUpdated = DateTime.Now;

            try
            {
                unitOfWork.Complete();

                // Broadcast update
                await _hubContext.Clients.Group($"post_{comment.PostId}")
                    .SendAsync("ReloadComments", comment.PostId);

                return Ok(new { success = true, comment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = unitOfWork.Comment.Find(id);
            if (comment == null)
                return NotFound();

            try
            {
                unitOfWork.Comment.Remove(comment);
                unitOfWork.Complete();

                // Broadcast deletion
                await _hubContext.Clients.Group($"post_{comment.PostId}")
                    .SendAsync("ReloadComments", comment.PostId);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

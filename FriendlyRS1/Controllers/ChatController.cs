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

namespace FriendlyRS1.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> hubContext)
        {
            _userManager = userManager;
            this.unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }


        private const int take = 6;

        // GET: Chat
        public async Task<IActionResult> Index(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var chatList = await GetChatList();

            ViewData["ChatList"] = chatList;

            List<ChatVM>? chatMessages = null;

            if (id != null)
            {
                var chatMate = unitOfWork.User.Find(id ?? 0);


                ViewData["ChatMateId"] = chatMate?.Id;
                ViewData["ChatMateName"] = $"{chatMate?.FirstName} {chatMate?.LastName}";
                ViewData["ChatMateProfileImage"] = chatMate?.ProfileImage;
                ViewData["LoggedProfileImage"] = currentUser?.ProfileImage;

                var messages = unitOfWork.Chat.GetAll()
                    .Where(x =>
                        (x.SenderId == currentUser.Id && x.ReceiverId == id) ||
                        (x.SenderId == id && x.ReceiverId == currentUser.Id))
                    .OrderBy(x => x.SentDate)
                    .ToList();

                chatMessages = messages.Select(m => new ChatVM
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    MessageText = m.MessageText,
                    ImageData = m.ImageData,
                    SentDate = m.SentDate,
                    SenderProfileImage = m.Sender.ProfileImage,
                    ReceiverProfileImage = m.Receiver.ProfileImage,
                    SentTimeFormatted = m.SentDate.ToLocalTime().ToString("hh:mm tt"),
                    IsRead = m.IsRead,
                    IsMe = currentUser.Id == m.SenderId,
                }).ToList();

                ViewData["MessageList"] = chatMessages;
            }

            return View();
        }

        public async Task<List<ChatListVM>> GetChatList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var chats = unitOfWork.Chat.GetAll()
                .Where(x => x.SenderId == currentUser.Id || x.ReceiverId == currentUser.Id)
                .OrderByDescending(x => x.SentDate)
                .ToList();

            // Group by the other user and select the last message
            var chatList = chats
                .GroupBy(x => x.SenderId == currentUser.Id ? x.ReceiverId : x.SenderId)
                .Select(g =>
                {
                    var lastMessage = g.OrderByDescending(x => x.SentDate).First();

                    // Determine the other user's ID
                    var otherUserId = lastMessage.SenderId == currentUser.Id
                        ? lastMessage.ReceiverId
                        : lastMessage.SenderId;

                    // Fetch the other user's details using unitOfWork
                    var otherUser = unitOfWork.User.Find(otherUserId);

                    return new ChatListVM
                    {
                        UserId = otherUser?.Id ?? 0,
                        Name = otherUser != null
                            ? otherUser.FirstName + " " + otherUser.LastName
                            : "Unknown User",
                        ProfileImage = otherUser?.ProfileImage,
                        LastMessage = lastMessage.MessageText ?? string.Empty,
                        LastMessageDate = lastMessage.SentDate
                    };
                })
                .OrderByDescending(x => x.LastMessageDate)
                .ToList();

            return chatList;
        }

        public async Task<IActionResult> SearchPeople(string q = "")
        {
            var loggedUser = await _userManager.GetUserAsync(User);

            QueryVM obj = new QueryVM
            {
                LoggedUserId = loggedUser.Id,
                q = q
            };

            return View("SearchPeople", obj);
        }

        public async Task<IActionResult> SendMessage(int ReceiverId, string MessageText)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (sender == null) return Unauthorized();

            var chat = new Chat
            {
                SenderId = sender.Id,
                ReceiverId = ReceiverId,
                MessageText = MessageText,
                ImageData = null,
                SentDate = DateTime.UtcNow,
                IsRead = false
            };

            unitOfWork.Chat.Add(chat);
            unitOfWork.Complete();

            var formattedDate = chat.SentDate.ToLocalTime().ToString("hh:mm tt");

            var senderUserId = sender.Id.ToString();
            var receiverUserId = ReceiverId.ToString();

            await _hubContext.Clients.Users(senderUserId, receiverUserId)
                .SendAsync("ReceiveMessage", new
                {
                    senderId = sender.Id,
                    receiverId = chat.ReceiverId,
                    messageText = chat.MessageText,
                    imageData = chat.ImageData,
                    sentDate = formattedDate
                });

            await _hubContext.Clients.Users(senderUserId, receiverUserId)
                .SendAsync("UpdateChatList");

            return Ok(new { success = true });
        }

        public async Task<IActionResult> GetPeople(int id, string q, int firstItem = 0)
        {
            UserVM model = new UserVM();

            if (!string.IsNullOrEmpty(q))
            {
                List<ApplicationUser> users = unitOfWork.User.GetUsersByName(q, firstItem, take);

                model = new UserVM
                {
                    Users = users.Select(x => new UserVM.Row
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        ProfileImage = x.ProfileImage,
                        IsMe = x.Id == id || id == 0
                    }).ToList()
                };
            }

            if ((model.Users == null || model.Users.Count == 0) && firstItem > take)
            {
                return new EmptyResult();
            }

            return View(model);

        }

        public async Task<IActionResult> GetChatListPartial()
        {
            var chatList = await GetChatList();
            return PartialView("_ChatListPartial", chatList);
        }
    }
}

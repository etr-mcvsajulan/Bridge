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

            if (id != null)
            {
                var messages = unitOfWork.Chat.GetAll()
                    .Where(x =>
                        (x.SenderId == currentUser.Id && x.ReceiverId == id) ||
                        (x.SenderId == id && x.ReceiverId == currentUser.Id))
                    .OrderBy(x => x.SentDate)
                    .ToList();

                var chatMessages = messages.Select(m => new ChatVM
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    MessageText = m.MessageText,
                    ImageData = m.ImageData,
                    SentDate = m.SentDate,
                    SentTimeFormatted = m.SentDate.ToLocalTime().ToString("hh:mm tt"),
                    IsRead = m.IsRead
                }).ToList();

                ViewData["ReceiverId"] = id;
                return PartialView("_ChatPanelPartial", chatMessages);
            }

            return View("Index");
        }

        public async Task<IActionResult> ChatList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            // Sidebar: list of recent chatmates
            var chats = unitOfWork.Chat.GetAll()
                .Where(x => x.SenderId == currentUser.Id || x.ReceiverId == currentUser.Id)
                .OrderByDescending(x => x.SentDate)
                .ToList();

            var chatList = chats
                .GroupBy(x => x.SenderId == currentUser.Id ? x.ReceiverId : x.SenderId)
                .Select(g =>
                {
                    var lastMessage = g.OrderByDescending(x => x.SentDate).First();
                    var chatMateId = lastMessage.SenderId == currentUser.Id
                        ? lastMessage.ReceiverId
                        : lastMessage.SenderId;

                    return new ChatVM
                    {
                        ReceiverId = chatMateId,
                        ReceiverName = lastMessage.Receiver.FirstName.ToString() + " " + lastMessage.Receiver.LastName.ToString(),
                        ReceiverProfileImage = lastMessage.Receiver.ProfileImage,
                        MessageText = lastMessage.MessageText,
                        SentDate = lastMessage.SentDate,
                        SentTimeFormatted = lastMessage.SentDate.ToLocalTime().ToString("hh:mm tt"),
                        IsRead = lastMessage.IsRead
                    };
                }).ToList();

            return PartialView("_LoadChatList", chatList);
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

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatVM model)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (sender == null) return Unauthorized();

            var chat = new Chat
            {
                SenderId = sender.Id,
                ReceiverId = model.ReceiverId,
                MessageText = model.MessageText,
                ImageData = model.ImageData,
                SentDate = DateTime.UtcNow,
                IsRead = false
            };

            unitOfWork.Chat.Add(chat);
            unitOfWork.Complete();

            var formattedDate = chat.SentDate.ToLocalTime().ToString("hh:mm tt");

            // Push to receiver and sender (so both see new message)
            await _hubContext.Clients.Users(
                model.ReceiverId.ToString(),
                sender.Id.ToString()
            ).SendAsync("ReceiveMessage", new
            {
                senderId = sender.Id,
                receiverId = model.ReceiverId,
                messageText = chat.MessageText,
                sentDate = formattedDate
            });

            return Ok();
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
    }
}

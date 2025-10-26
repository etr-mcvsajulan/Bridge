using DataLayer.EntityModels;
using FriendlyRS1.Repository.RepostorySetup;
using FriendlyRS1.SignalRChat.Hubs;
using FriendlyRS1.SignalRChat.Interface;
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
    public class AppointmentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUserConnectionManager _userConnectionManager;

        public AppointmentController(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> hubContext,
            IUserConnectionManager userConnectionManager)
        {
            _userManager = userManager;
            this.unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _userConnectionManager = userConnectionManager;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<List<AppointmentVM>> GetAppointments(int month, int year)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return new List<AppointmentVM>();

            var appointments = unitOfWork.Appointment.GetAppointments(currentUser.Id);

            var appointmentList = appointments
                .Select(a => new AppointmentVM
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = (AppointmentStatus)a.Status,
                    AuthorId = a.AuthorId,
                    ReceiverId = a.ReceiverId,
                    AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : "Unknown",
                    ReceiverName = a.Receiver != null ? $"{a.Receiver.FirstName} {a.Receiver.LastName}" : "Unknown",
                    CreatedAt = a.CreatedAt
                })
                .OrderBy(a => a.StartTime)
                .ToList();

            return appointmentList;
        }


        public async Task<IActionResult> AddAppointment([FromBody] AddAppointmentVM model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            if (model == null) return BadRequest(new { success = false, message = "Invalid appointment data." });

            var appointment = new Appointments
            {
                AuthorId = currentUser.Id,
                ReceiverId = model.ReceiverId,
                Title = model.Title,
                Description = model.Description,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Status = (int)AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            unitOfWork.Appointment.Add(appointment);
            unitOfWork.Complete();

            return Json(new { success = true, message = "Appointment created successfully." });
        }

        public async Task<ConnectionsVM> GetConnections(string searchString, int firstItem = 0)
        {
            var user = await _userManager.GetUserAsync(User);


            var model = new ConnectionsVM();
            model.Connections = unitOfWork.Friendship.
                GetConnections(x => new ConnectionsVM.Connection
                {
                    User1Id = x.User1Id,
                    User2Id = x.User2Id,
                    ActorId = x.ActionUserId,
                    Id = x.Id,
                    User = x.User1Id != user.Id ? x.User1 : x.User2
                }, (int)user.Id, firstItem, 10, searchString);

            model.LoggedUser = user.Id;

            return model;
        }
    }
}

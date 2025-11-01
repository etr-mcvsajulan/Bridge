using DataLayer.EntityModels;
using FriendlyRS1.Repository;
using FriendlyRS1.Repository.RepostorySetup;
using FriendlyRS1.SignalRChat.Hubs;
using FriendlyRS1.SignalRChat.Interface;
using FriendlyRS1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
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
        private readonly IHubContext<NotificationHubUser> _notificationUserHubContext;
        private readonly ApplicationDbContext _context;

        public AppointmentController(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> hubContext,
            IUserConnectionManager userConnectionManager,
            IHubContext<NotificationHubUser> notificationUserHubContext,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            this.unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _userConnectionManager = userConnectionManager;
            _notificationUserHubContext = notificationUserHubContext;
            _context = context;
        }
        public async Task<IActionResult> Index(string? id)
        {
            var loggedUser = await _userManager.GetUserAsync(User);
            DateTime? selectedDate = null;

            if (!string.IsNullOrEmpty(id) && DateTime.TryParse(id, out DateTime parsed))
                selectedDate = parsed;

            // Pass null if no valid date
            ViewBag.SelectedDate = selectedDate?.ToString("yyyy-MM-dd");
            ViewBag.UserId = loggedUser.Id;
            return View();
        }

        public async Task<List<AppointmentVM>> GetAppointments([FromBody] GetAppointmentRequestVM request)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return new List<AppointmentVM>();

            var appointmentList = await _context.Appointments
                .Where(a => (a.AuthorId == currentUser.Id || a.ReceiverId == currentUser.Id)
                    && a.StartTime.Month == request.Month && a.StartTime.Year == request.Year)
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
                    AuthorName = a.Author.FirstName + " " + a.Author.LastName,
                    ReceiverName = a.Receiver.FirstName + " " + a.Receiver.LastName,
                    CreatedAt = a.CreatedAt
                })
                .OrderBy(a => a.StartTime)
                .AsNoTracking()
                .ToListAsync();

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

            BellNotification notification = new BellNotification
            {
                ActorId = currentUser.Id,
                NotificationTypeId = (int)EnumNotificationType.AppointmentCreated,
                NotifierId = model.ReceiverId,
                DateCreated = DateTime.Now,
                RedirectAction = "Index",
                RedirectController = "Appointment",
                RedirectParam = model.StartTime.ToString("yyyy-MM-dd")
            };

            unitOfWork.BellNotification.Add(notification);
            unitOfWork.Appointment.Add(appointment);
            unitOfWork.Complete();

            await SendAppointmentNotification(
                model.StartTime.ToString("yyyy-MM-dd"),
                model.ReceiverId,
                "created an appointment with you."
            );

            return Json(new { success = true, message = "Appointment created successfully." });
        }

        public async Task<IActionResult> AcceptAppointment(int appointmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var appointment = unitOfWork.Appointment.Find(appointmentId);
            if (appointment == null)
                return NotFound(new { success = false, message = "Appointment not found." });

            if (appointment.ReceiverId != currentUser.Id)
                return Forbid();

            appointment.Status = (int)AppointmentStatus.Accepted;

            var payment = new AppointmentPayment
            {
                AppointmentId = appointment.Id,
                TransactionId = unitOfWork.AppointmentPayment.GenerateTransactionId("PAY"),
                ProfessionalFee = 500, 
                ServiceFee = 50,      
                PaymentMethod = "GCash",
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            unitOfWork.AppointmentPayment.Add(payment);
            unitOfWork.Appointment.Update(appointment);
            unitOfWork.Complete();

            // Optionally notify the customer
            BellNotification notification = new BellNotification
            {
                ActorId = currentUser.Id,
                NotificationTypeId = (int)EnumNotificationType.AppointmentAccepted,
                NotifierId = appointment.AuthorId,
                DateCreated = DateTime.Now,
                RedirectAction = "Index",
                RedirectController = "Appointment",
                RedirectParam = appointment.StartTime.ToString("yyyy-MM-dd")
            };
            unitOfWork.BellNotification.Add(notification);
            unitOfWork.Complete();

            await SendAppointmentNotification(
                appointment.StartTime.ToString("yyyy-MM-dd"),
                appointment.AuthorId,
                "has accepted your appointment request."
            );

            return Json(new { success = true, message = "Appointment accepted and payment record created." });
        }

        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var appointment = unitOfWork.Appointment.Find(appointmentId);
            if (appointment == null)
                return NotFound(new { success = false, message = "Appointment not found." });

            appointment.Status = (int)AppointmentStatus.Completed;
            unitOfWork.Appointment.Update(appointment);

            // Notify
            BellNotification notification = new BellNotification
            {
                ActorId = currentUser.Id,
                NotificationTypeId = (int)EnumNotificationType.AppointmentCompleted,
                NotifierId = appointment.AuthorId,
                DateCreated = DateTime.Now,
                RedirectAction = "Index",
                RedirectController = "Appointment",
                RedirectParam = appointment.StartTime.ToString("yyyy-MM-dd")
            };

            unitOfWork.BellNotification.Add(notification);
            unitOfWork.Complete();

            await SendAppointmentNotification(
                appointment.StartTime.ToString("yyyy-MM-dd"),
                appointment.AuthorId,
                "marked your appointment as completed."
            );

            return Json(new { success = true, message = "Appointment marked as completed." });
        }

        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var appointment = unitOfWork.Appointment.Find(appointmentId);
            if (appointment == null)
                return NotFound(new { success = false, message = "Appointment not found." });

            appointment.Status = (int)AppointmentStatus.Cancelled;
            unitOfWork.Appointment.Update(appointment);

            BellNotification notification = new BellNotification
            {
                ActorId = currentUser.Id,
                NotificationTypeId = (int)EnumNotificationType.AppointmentCancelled,
                NotifierId = appointment.AuthorId == currentUser.Id ? appointment.ReceiverId : appointment.AuthorId,
                DateCreated = DateTime.Now,
                RedirectAction = "Index",
                RedirectController = "Appointment",
                RedirectParam = appointment.StartTime.ToString("yyyy-MM-dd")
            };

            unitOfWork.BellNotification.Add(notification);
            unitOfWork.Complete();

            await SendAppointmentNotification(
                appointment.StartTime.ToString("yyyy-MM-dd"),
                notification.NotifierId,
                "cancelled the appointment."
            );

            return Json(new { success = true, message = "Appointment cancelled successfully." });
        }

        public async Task<IActionResult> MarkAsPaid(int appointmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var payment = unitOfWork.AppointmentPayment.Get(x => x.AppointmentId == appointmentId);
            if (payment == null)
                return NotFound(new { success = false, message = "Payment record not found." });

            payment.PaymentStatus = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            unitOfWork.AppointmentPayment.Update(payment);
            unitOfWork.Complete();

            var appointment = unitOfWork.Appointment.Find(appointmentId);

            // Notify
            BellNotification notification = new BellNotification
            {
                ActorId = currentUser.Id,
                NotificationTypeId = (int)EnumNotificationType.AppointmentPaid,
                NotifierId = appointment.ReceiverId == currentUser.Id ? appointment.AuthorId : appointment.ReceiverId,
                DateCreated = DateTime.Now,
                RedirectAction = "Index",
                RedirectController = "Appointment",
                RedirectParam = appointment.StartTime.ToString("yyyy-MM-dd")
            };

            unitOfWork.BellNotification.Add(notification);
            unitOfWork.Complete();

            await SendAppointmentNotification(
                appointment.StartTime.ToString("yyyy-MM-dd"),
                notification.NotifierId,
                "has completed payment for the appointment."
            );

            return Json(new { success = true, message = "Payment marked as paid." });
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

        public async Task<IActionResult> AppointmentDetailsPartial(int appointmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var appointment = unitOfWork.Appointment.GetAppointment(appointmentId); // your method to include Author & Receiver

            if (appointment == null) return NotFound();

            var model = new AppointmentVM
            {
                Id = appointment.Id,
                Title = appointment.Title,
                Description = appointment.Description,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = (AppointmentStatus)appointment.Status,
                AuthorId = appointment.AuthorId,
                ReceiverId = appointment.ReceiverId,
                AuthorName = $"{appointment.Author.FirstName} {appointment.Author.LastName}",
                ReceiverName = $"{appointment.Receiver.FirstName} {appointment.Receiver.LastName}",
                PaymentStatus = appointment?.Payment?.PaymentStatus
            };

            ViewBag.UserId = currentUser.Id;

            return PartialView("_AppointmentDetailsModal", model);
        }

        public async Task<IActionResult> SendAppointmentNotification(string date, int receiverId, string message)
        {
            try
            {
                var loggedUser = await _userManager.GetUserAsync(User);
                var connections = _userConnectionManager.GetUserConnections(receiverId.ToString());

                if (connections != null)
                {
                    foreach (var connectionId in connections)
                    {
                        await _notificationUserHubContext.Clients.Client(connectionId).SendAsync(
                            "sendToUser",
                            loggedUser.ToString(),
                            message,
                            loggedUser.Id,
                            loggedUser?.ProfileImage,
                            $"/Appointment/Index/{date}"
                            );
                    }
                }
            }
            catch { }


            return new EmptyResult();
        }
    }
}

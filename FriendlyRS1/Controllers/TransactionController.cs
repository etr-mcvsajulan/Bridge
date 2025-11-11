using DataLayer.EntityModels;
using FriendlyRS1.Repository;
using FriendlyRS1.Repository.RepostorySetup;
using FriendlyRS1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FriendlyRS1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TransactionController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public TransactionController(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue) startDate = DateTime.Today;
            if (!endDate.HasValue) endDate = DateTime.Today;

            var query = from a in _context.Appointments
                        join p in _context.AppointmentPayment
                            on a.Id equals p.AppointmentId into ap
                        from p in ap.DefaultIfEmpty() // left join
                        where string.IsNullOrEmpty(search) ||
                              (a.Author.FirstName + " " + a.Author.LastName).Contains(search) ||
                              (a.Receiver.FirstName + " " + a.Receiver.LastName).Contains(search)
                        select new AppointmentTransactionVM
                        {
                            Id = a.Id,
                            TransanctionId = p != null ? p.TransactionId : null,
                            AuthorName = a.Author.FirstName + " " + a.Author.LastName,
                            ReceiverName = a.Receiver.FirstName + " " + a.Receiver.LastName,
                            AppointmentStatus = (AppointmentStatus)a.Status,
                            AppointmentStatusString = ((AppointmentStatus)a.Status).ToString(),
                            PaymentStatus = p.PaymentStatus,
                            PaymentStatusString = ((PaymentStatus)p.PaymentStatus).ToString(),
                            PaymentMethod = p.PaymentMethod,
                            CreatedAt = a.CreatedAt
                        };

            // Filter by CreatedAt only if startDate or endDate is provided
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTime endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt >= startDate.Value && x.CreatedAt <= endOfDay);
            }
            else if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate.Value);
            }
            else if (endDate.HasValue)
            {
                DateTime endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt <= endOfDay);
            }


            int totalCount = await query.CountAsync();

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();


            var pager = new Pager(totalCount, page, pageSize);

            ViewBag.Pager = pager;
            ViewBag.Search = search;
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            return View(transactions);
        }

        public async Task<IActionResult> AppointmentDetailsPartial(int appointmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var appointment = _unitOfWork.Appointment.GetAppointment(appointmentId); // your method to include Author & Receiver

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
                PaymentDetails = appointment.Payment == null ? null : new AppointmentPaymentVM
                {
                    Id = appointment.Payment.Id,
                    TransactionId = appointment.Payment.TransactionId,
                    ProfessionalFee = appointment.Payment.ProfessionalFee,
                    ServiceFee = appointment.Payment.ServiceFee,
                    PaymentMethod = appointment.Payment.PaymentMethod,
                    Status = appointment.Payment.PaymentStatus,
                    PaidAt = appointment.Payment.PaidAt
                }
            };

            ViewBag.UserId = currentUser.Id;

            return PartialView("_TransactionDetailsModal", model);
        }
    }
}

using DataLayer.EntityModels;
using FriendlyRS1.Repository.RepostorySetup;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FriendlyRS1.Repository.Repositories
{
    public class AppointmentRepository : Repository<Appointments>
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Appointments> _dbSet;

        public AppointmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Appointments>();
        }

        public List<Appointments> GetAppointments(int userId)
        {
            return _dbSet
                .Include(a => a.Author)
                .Include(a => a.Receiver)
                .Where(a => a.AuthorId == userId || a.ReceiverId == userId)
                .OrderByDescending(a => a.StartTime)
                .AsNoTracking()
                .ToList();
        }

        public Appointments GetAppointment(int id)
        {
            return _dbSet
                .Include(a => a.Author)
                .Include(a => a.Receiver)
                .Include(a => a.Payment)
                .AsNoTracking()
                .FirstOrDefault(a => a.Id == id);
        }
    }
}

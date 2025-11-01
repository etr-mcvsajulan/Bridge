using DataLayer.EntityModels;
using FriendlyRS1.Repository.RepostorySetup;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FriendlyRS1.Repository.Repositories
{
    public class AppointmentPaymentRepository : Repository<AppointmentPayment>
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<AppointmentPayment> _dbSet;

        public AppointmentPaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<AppointmentPayment>();
        }

        public string GenerateTransactionId(string acronym)
        {
            var random = new Random();
            return $"{acronym}{random.Next(1000000000, int.MaxValue).ToString("D10")}";
        }
    }
}

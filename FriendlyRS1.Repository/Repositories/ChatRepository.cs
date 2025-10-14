using DataLayer.EntityModels;
using FriendlyRS1.Repository.RepostorySetup;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendlyRS1.Repository.Repositories
{
    public class ChatRepository: Repository<Chat>
    {
        private ApplicationDbContext _context;
        private DbSet<ChatRepository> _dbSet;
        public ChatRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Chat> GetChatMessagesBetweenUsers(int userId, int partnerId)
        {
            return _context.Chat
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Where(c => (c.SenderId == userId && c.ReceiverId == partnerId)
                         || (c.SenderId == partnerId && c.ReceiverId == userId))
                .OrderBy(c => c.SentDate)
                .AsNoTracking()
                .ToList();
        }

        // Get all chat users that current user has conversations with
        public List<ApplicationUser> GetChatUsers(int userId)
        {
            return _context.Chat
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Where(c => c.SenderId == userId || c.ReceiverId == userId)
                .Select(c => c.SenderId == userId ? c.Receiver : c.Sender)
                .Distinct()
                .AsNoTracking()
                .ToList();
        }
    }
}

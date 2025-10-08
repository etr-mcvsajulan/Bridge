using DataLayer.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendlyRS1.Repository.RepostorySetup;
using Microsoft.EntityFrameworkCore;

namespace FriendlyRS1.Repository.Repositories
{
    public class CommentRepository:Repository<Comment>
    {
        private ApplicationDbContext _context;
        private DbSet<CommentRepository> _dbSet;
        public CommentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Comment> GetCommentsByPostId(int postId, int take = 100, int skip = 0)
        {
            return _context.Comment
                .Include(c => c.Author)               
                .Where(c => c.PostId == postId)        
                .OrderByDescending(c => c.DateCreated)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToList();
        }
    }
}

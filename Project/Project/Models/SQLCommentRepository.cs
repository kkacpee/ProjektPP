using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class SQLCommentRepository : ICommentRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<SQLCommentRepository> logger;

        public SQLCommentRepository(AppDbContext context,
                                    ILogger<SQLCommentRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }
            public Comment Add(Comment comment)
        {
            context.Comments.Add(comment);
            context.SaveChanges();
            return comment;
        }

        public Comment Delete(int id)
        {
            Comment comment = context.Comments.Find(id);
            if (comment != null)
            {
                context.Comments.Remove(comment);
                context.SaveChanges();
            }
            return comment;
        }

        public IEnumerable<Comment> GetAllComments(int id)
        {
            return context.Comments.Where(d => d.PhotoId == id);
        }

        public Comment GetComment(int Id)
        {
            return context.Comments.Find(Id);
        }

        public Comment Update(Comment commentChanges)
        {
            var comment = context.Comments.Attach(commentChanges);
            comment.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return commentChanges;
        }
    }
}

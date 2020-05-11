using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public interface ICommentRepository
    {
        Comment GetComment(int Id);
        IEnumerable<Comment> GetAllComments(int id);
        Comment Add(Comment comment);
        Comment Update(Comment commentChanges);
        Comment Delete(int id);
    }
}

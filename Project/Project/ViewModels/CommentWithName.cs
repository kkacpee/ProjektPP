using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.ViewModels
{
    public class CommentWithName : Comment
    {
        public CommentWithName(Comment com, string userName)
        {
            base.Content = com.Content;
            base.Date = com.Date;
            base.ID = com.ID;
            base.PhotoId = com.PhotoId;
            this.UserName = userName;
        }
        public string UserName { get; set; }
    }
}

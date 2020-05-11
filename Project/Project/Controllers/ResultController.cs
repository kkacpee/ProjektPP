using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    public class ResultController : Controller
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly ISearchRepository _searchRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<IdentityUser> userManager;

        public ResultController(IPhotoRepository photoRepository, 
            ISearchRepository searchRepository, 
            ICommentRepository commentRepository,
             UserManager<IdentityUser> userManager)
        {
            _photoRepository = photoRepository;
            _searchRepository = searchRepository;
            _commentRepository = commentRepository;
            this.userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Search(string phrase)
        {
            Search search = new Search 
            { 
                Phrase = phrase, 
                Date = DateTime.Now 
            };
            _searchRepository.Add(search);
            IEnumerable<Photo> photos = _photoRepository.GetAllPhotos();
            return View(photos.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            Photo photo = _photoRepository.GetPhoto(id);
            List<Comment> coms = _commentRepository.GetAllComments(id).ToList();
            List<CommentWithName> comsWithNames = new List<CommentWithName>();
            foreach(var comm in coms)
            {
                var user = await userManager.FindByIdAsync(comm.UserId);
                comsWithNames.Add(new CommentWithName(comm, user.UserName));
            }
            PhotoCommentsViewModel photoCommentViewMode = new PhotoCommentsViewModel
            {
                ID = photo.ID,
                Url = photo.Url,
                ResX = photo.ResX,
                ResY = photo.ResY,
                PhotoFormat = photo.PhotoFormat,
                Date = photo.Date,
                Comments = comsWithNames
            };
            return View(photoCommentViewMode);
        }

        [HttpPost]
        public async Task<IActionResult> Result(PhotoCommentsViewModel model, string content)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync((User.Identity.Name));
                Comment comment = new Comment
                {
                    Content = content,
                    Date = DateTime.Now,
                    UserId = user.Id,
                    PhotoId = model.ID
                };

                _commentRepository.Add(comment);
            }
            return RedirectToAction("Result",new { id = model.ID });
        }

        [HttpPost]
        public IActionResult DeleteComment(int photoId, int id)
        {
            _commentRepository.Delete(id);
            return RedirectToAction("Result", new { id = photoId });
        }
    }
}
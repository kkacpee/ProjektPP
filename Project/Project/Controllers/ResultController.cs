using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    public class ResultController : Controller
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly ISearchRepository _searchRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IResultRepository _resultRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public ResultController(IPhotoRepository photoRepository, 
            ISearchRepository searchRepository,
            IHistoryRepository historyRepository,
            ICommentRepository commentRepository,
            IResultRepository resultRepository,
             UserManager<IdentityUser> userManager,
             SignInManager<IdentityUser> signInManager)
        {
            _photoRepository = photoRepository;
            _searchRepository = searchRepository;
            _historyRepository = historyRepository;
            _commentRepository = commentRepository;
            _resultRepository = resultRepository;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string phrase)
        {
            var search = new Search
            {
                Phrase = phrase,
                Date = DateTime.Now
            };
            _searchRepository.Add(search);
            if (signInManager.IsSignedIn(User))
            {
                var user = await userManager.FindByNameAsync((User.Identity.Name));
                var history = new History
                {
                    Date = search.Date,
                    SearchId = search.ID,
                    UserId = user.Id
                };
                _historyRepository.Add(history);
            }
            List<Photo> listFromApi = ApiHandler("https://jsonplaceholder.typicode.com/", search);
            if (listFromApi.Any())
                return View(listFromApi);
            IEnumerable <Photo> photos = _photoRepository.GetAllPhotos();
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

        [HttpGet]
        public IActionResult GetJson(string json)
        {
            
            return View();
     
        }

        public List<Photo> ApiHandler(string uri, Search search)
        {
            var outputList = new List<Photo>();
            string str;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                var responseTask = client.GetAsync(search.Phrase);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadAsStringAsync();
                    readJob.Wait();
                    str = readJob.Result;
                    var list = JsonConvert.DeserializeObject<List<Photo>>(str);

                    Result res;
                    foreach (var photo in list)
                    {
                        var returnedPhoto = _photoRepository.Add(photo);
                        outputList.Add(returnedPhoto);
                        res = new Result
                        {
                            PhotoId = returnedPhoto.ID,
                            EngineId = 1,
                            SearchId = search.ID
                        };
                        _resultRepository.Add(res);
                    }
                }
                else
                {
                }

                return outputList;
            }
        }
    }
}
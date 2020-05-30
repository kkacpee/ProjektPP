using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly IScoreRepository _scoreRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public ResultController(IPhotoRepository photoRepository, 
            ISearchRepository searchRepository,
            IHistoryRepository historyRepository,
            ICommentRepository commentRepository,
            IResultRepository resultRepository,
            IScoreRepository scoreRepository,
             UserManager<IdentityUser> userManager,
             SignInManager<IdentityUser> signInManager)
        {
            _photoRepository = photoRepository;
            _searchRepository = searchRepository;
            _historyRepository = historyRepository;
            _commentRepository = commentRepository;
            _resultRepository = resultRepository;
            _scoreRepository = scoreRepository;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string phrase, string google, string pinterest)
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

            var check = ApiTask("https://image-spider-server.herokuapp.com/schedule.json", search);

            return RedirectToAction("Waiting", search);
        }

        public IActionResult Peak(int id)
        {
            var results = _resultRepository.GetResultsForSearch(id);
            List<Photo> photos = new List<Photo>();
            foreach (var result in results)
            {
                var photo = _photoRepository.GetPhoto(result.PhotoId);
                photos.Add(photo);
            }   

            return View("Search", photos);
        }

        [HttpGet]
        public async Task<IActionResult> Result(int id, int? search)
        {
            Result result = null;
            var vote = false;
            List<Result> results = new List<Result>();
            if (search != 0)
            {
                var sch = search ?? default;
                result = _resultRepository.GetAllResults().Where(p => p.PhotoId == id && p.SearchId == sch).FirstOrDefault();
                if(result == null)
                {
                    var phrase = _searchRepository.GetSearch(sch).Phrase;
                    sch = _searchRepository.GetAllSearches().Where(p => p.Phrase == phrase).FirstOrDefault().ID;
                    result = _resultRepository.GetAllResults().Where(p => p.PhotoId == id && p.SearchId == sch).FirstOrDefault();
                }
                var hist = _historyRepository.GetHistory(sch);
                var scores = _scoreRepository.GetAllScores().Where(p => p.UserId == hist.UserId && p.ResultId == result.ID);
                if (scores.Any())
                    vote = true;
            }
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
                Comments = comsWithNames,
                Voted = vote
            };
            var tuple = new Tuple<PhotoCommentsViewModel, Result>(photoCommentViewMode, result);

            return View(tuple);
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

        [HttpPost]
        public IActionResult UpdateComment(int photoId, int id, string content)
        {
            var comment = _commentRepository.GetComment(id);
            comment.Content = content;
            _commentRepository.Update(comment);
            return RedirectToAction("Result", new { id = photoId });
        }

        [HttpGet]
        public IActionResult GetJson(string json)
        {
            return View();
     
        }



        public List<Photo> ApiHandler(string uri, int id)
        {
            var outputList = new List<Photo>();
            string str;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                Dictionary<string, string> pairs = new Dictionary<string, string>();
                pairs.Add("crawlid", id.ToString());
                FormUrlEncodedContent formContent =
                    new FormUrlEncodedContent(pairs);

                var responseTask = client.PostAsync(client.BaseAddress, formContent);
                responseTask.Wait();

               var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadAsStringAsync();
                    readJob.Wait();
                    str = readJob.Result; 
                    if (JObject.Parse(str)["image_count"].Equals("0"))
                        return outputList;
                    str = JObject.Parse(str)["images"].ToString();
                    var list = JsonConvert.DeserializeObject<List<Photo>>(str);

                    Result res;
                    foreach (var photo in list)
                    {
                        photo.ID = 0;
                        Photo returnedPhoto;
                        var control = _photoRepository.FindPhoto(photo.Url);
                        if(control == null)
                        {
                            returnedPhoto = _photoRepository.Add(photo);
                        }
                        else
                        {
                            returnedPhoto = control;

                        }
                        outputList.Add(returnedPhoto);
                        res = new Result
                        {
                            PhotoId = returnedPhoto.ID,
                            EngineId = 1,
                            SearchId = id
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

        public int ApiTask(string uri, Search search)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                //var link = "https://image-spider-server.herokuapp.com/schedule.json";
                //var spider = new Uri(link);
                Dictionary<string, string> pairs = new Dictionary<string, string>();
                pairs.Add("project", "Spiders");
                pairs.Add("spider", "GoogleImages");
                pairs.Add("term", search.Phrase.ToString());
                pairs.Add("amount", "50");
                pairs.Add("crawlid", search.ID.ToString());
                FormUrlEncodedContent formContent =
                    new FormUrlEncodedContent(pairs);

                var responseTask = client.PostAsync(client.BaseAddress, formContent);
                StringContent sc = new StringContent(JsonConvert.SerializeObject(pairs), UnicodeEncoding.UTF8, "application/json");

                //var responseTask = client.PostAsync(client.BaseAddress, sc);
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }

        [HttpGet]
        public IActionResult Waiting(Search search)
        {
            return View(search);
        }

        [HttpPost]
        public IActionResult Waiting(int id)
        {
            var search = _searchRepository.GetSearch(id);
            var searches = _searchRepository.GetAllSearches().Where(p => p.Phrase == search.Phrase && p.ID != id);
            List<Photo> photosFromDB = new List<Photo>();
            foreach (var s in searches)
            {
                var results = _resultRepository.GetResultsForSearch(s.ID);
                foreach (var result in results)
                {
                    var photo = _photoRepository.GetPhoto(result.PhotoId);
                    if (!photosFromDB.Contains(photo))
                    {
                        photosFromDB.Add(photo);
                    }

                }
            }
            List<Photo> listFromApi = new List<Photo>();
            if (searches.Any())
            {
                var lastId = searches.Last().ID;
                listFromApi.AddRange(ApiHandler("https://image-spider-client.herokuapp.com/results/", lastId));
            }
            
            listFromApi.AddRange(ApiHandler("https://image-spider-client.herokuapp.com/results/", id));
            List<Photo> photos = new List<Photo>();
            photos.AddRange(photosFromDB);
            photos.AddRange(listFromApi);
            photos.Reverse();
            photos.Distinct();
            var tuple = new Tuple<List<Photo>, int>(photos, id);
            return View("Search", tuple);
        }

        public IActionResult PhraseResults(string phrase)
        {
            var searches = _searchRepository.GetAllSearches().Where(p => p.Phrase == phrase);
            List<Photo> photos = new List<Photo>();
            foreach(var search in searches)
            {
                var results = _resultRepository.GetResultsForSearch(search.ID);
                foreach(var result in results)
                {
                    var photo = _photoRepository.GetPhoto(result.PhotoId);
                    if (!photos.Contains(photo))
                    {
                        photos.Add(photo);
                    }
                    
                }
            }
            return View(photos);
        }

        public async Task<IActionResult> VoteUp(string name, int resultID)
        {
            var result = _resultRepository.GetResult(resultID);
            var user = await userManager.FindByNameAsync(name);
            var score = new Score
            {
                ResultId = result.ID,
                Grade = 1,
                UserId = user.Id
            };
            _scoreRepository.Add(score);
            UpdateScores(1, result);

            return RedirectToAction("Result", new { id = result.PhotoId, search = result.SearchId });
        }

        public async Task<IActionResult> VoteDown(string name, int resultID)
        {
            var result = _resultRepository.GetResult(resultID);
            var user = await userManager.FindByNameAsync(name);
            var score = new Score
            {
                ResultId = result.ID,
                Grade = -1,
                UserId = user.Id
            };
            _scoreRepository.Add(score);
            UpdateScores(-1,result);

            return RedirectToAction("Result", new { id = result.PhotoId, search = result.SearchId });
        }

        public void UpdateScores(int grade, Result result)
        {
            var search = _searchRepository.GetSearch(result.SearchId);
            var searches = _searchRepository.GetAllSearches().Where(p => p.Phrase == search.Phrase);
            List<Result> results = new List<Result>();
            foreach(var s in searches)
            {
                results.AddRange(_resultRepository.GetResultsForSearch(s.ID));
            }
            results = results.Where(p => p.PhotoId == result.PhotoId).ToList();
            foreach(var res in results)
            {
                res.AvgScore += grade;
                _resultRepository.Update(res);
            }
        }
    }
}
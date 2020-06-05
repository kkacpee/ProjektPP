using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Search(string phrase)
        {
            var name = Request.Form["engine"].ToString();
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
            //else
            //{
            //    var user = await userManager.FindByNameAsync("guest@guest.com");
            //    var history = new History
            //    {
            //        Date = search.Date,
            //        SearchId = search.ID,
            //        UserId = user.Id
            //    };
            //    _historyRepository.Add(history);
            //}

            var check = ApiTask("https://image-spider-server.herokuapp.com/schedule.json",name ,search);

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
            var tuple = new Tuple<List<Photo>, int>(photos, id);
            ViewBag.Clicked = false;
            ViewBag.Checked = false;
            return View("Search", tuple);
        }

        [HttpGet]
        public async Task<IActionResult> Result(int id, int? search)
        {
            Result result = null;
            var vote = false;
            List<Result> results = new List<Result>();
            if (search != null)
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
            else
            {
                result = _resultRepository.GetAllResults().Where(p => p.PhotoId == id).FirstOrDefault();
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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Waiting(Search search)
        {
            return View(search);
        }

        [AllowAnonymous]
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
                listFromApi.AddRange(ApiHandler("https://image-spider-client.herokuapp.com/results", lastId));
            }
            
            listFromApi.AddRange(ApiHandler("https://image-spider-client.herokuapp.com/results", id));
            List<Photo> photos = new List<Photo>();
            photos.AddRange(photosFromDB);
            photos.AddRange(listFromApi);
            photos.Reverse();
            photos.Distinct();
            var tuple = new Tuple<List<Photo>, int>(photos, id);
            ViewBag.Changes = false;
            ViewBag.Clicked = false;
            return View("Search", tuple);
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
                        if (control == null)
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
        public int ApiTask(string uri,string name ,Search search)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                //var link = "https://image-spider-server.herokuapp.com/schedule.json";
                //var spider = new Uri(link);
                Dictionary<string, string> pairs = new Dictionary<string, string>();
                pairs.Add("project", "Spiders");
                if(name == "Pinterest")
                {
                    pairs.Add("spider", "Pinterest");
                }
                else
                {
                    pairs.Add("spider", "GoogleImages");
                }
                
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

        public IActionResult DeleteScore(int scoreId)
        {
            var score = _scoreRepository.GetScore(scoreId);
            var result = _resultRepository.GetResult(score.ResultId);
            var grade = score.Grade * (-1);
            UpdateScores(grade, result);
            _scoreRepository.Delete(scoreId);

            return RedirectToAction("CheckScore", new { resultId = result.ID });
        }

        public async Task<IActionResult> CheckScore(int resultId)
        {
            var result = _resultRepository.GetResult(resultId);
            var results = _resultRepository.GetAllResults().Where(e => e.PhotoId == result.PhotoId);
            var scores = _scoreRepository.GetAllScores();
            var scoreList = new List<Score>();
            foreach(var res in results)
            {
                scoreList.AddRange(scores.Where(e => e.ResultId == res.ID));
            }
            var scoreViewsList = new List<ScoreViewModel>();
            foreach(var score in scoreList)
            {
                result = _resultRepository.GetResult(score.ResultId);
                var search = _searchRepository.GetSearch(result.SearchId);
                var user = await userManager.FindByIdAsync(score.UserId);
                scoreViewsList.Add(new ScoreViewModel
                {
                    ID = score.ID,
                    Phrase = search.Phrase,
                    UserName = user.UserName,
                    Score = score.Grade
                });
            }

            return View(scoreViewsList);
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult CheckN(int searchId)
        //public IActionResult CheckN(Tuple<List<Photo>, int> input)
        {
            //  var list = input.Item1;
            //  var searchId = input.Item2;
            var search = _searchRepository.GetSearch(searchId);
            var searches = _searchRepository.GetAllSearches().Where(p => p.Phrase == search.Phrase);
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
            var isImplementedUri = new Uri("http://15.236.208.227:8133/isImplemented"); //Get
            var checkNUri = new Uri("http://15.236.208.227:8133/checkN"); //Get
            ViewBag.Clicked = true;
            if(IsImplementedHandler(isImplementedUri, search))
            {
                var output = CheckNHandler(checkNUri, search, photosFromDB);
                var tuple = new Tuple<List<Photo>, int>(output, searchId);
                ViewBag.Checked = true;
                return View("Search", tuple);
            }
            else
            {
                var tuple = new Tuple<List<Photo>, int>(photosFromDB, searchId);
                ViewBag.Checked = false;
                return View("Search", tuple);
            }

        }


        public bool IsImplementedHandler(Uri uri, Search search)
        {
            
            using (var client = new HttpClient())
            {
                client.BaseAddress = uri;
                Dictionary<string, string> pairs = new Dictionary<string, string>();
                pairs.Add("label", search.Phrase);

                StringContent sc = new StringContent(JsonConvert.SerializeObject(pairs), UnicodeEncoding.UTF8, "application/json");
                var responseTask = client.PostAsync(client.BaseAddress, sc);
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadAsStringAsync();
                    readJob.Wait();
                    var str = readJob.Result;
                    var value = JObject.Parse(str)["value"];
                    var strValue = value.ToString();
                    if (strValue.Equals("True"))
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<Photo> CheckNHandler(Uri uri, Search search, List<Photo> input)
        {
            var outputList = new List<Photo>();
            var jsonList = new List<Dictionary<string, string>>();
            foreach (var photo in input)
            {
                jsonList.Add(new Dictionary<string, string> {
                    {"label", search.Phrase },
                    {"url", photo.Url }
                });
            }
            using (var client = new HttpClient())
            {
                client.BaseAddress = uri;
                StringContent sc = new StringContent(JsonConvert.SerializeObject(jsonList), UnicodeEncoding.UTF8, "application/json");
                var responseTask = client.PostAsync(client.BaseAddress, sc);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadAsStringAsync();
                    readJob.Wait();
                    var arr = JArray.Parse(readJob.Result);
                    int i = 0;
                    foreach(var photo in input)
                    {
                        if(arr[i].ToString() == "True")
                        {
                            outputList.Add(photo);
                        }
                        i++;
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
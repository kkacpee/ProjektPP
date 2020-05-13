using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.Models;

namespace Project.Controllers
{
    public class HttpController : Controller
    {
        private readonly IPhotoRepository photoRepository;
        private readonly ISearchRepository searchRepository;
        private readonly IResultRepository resultRepository;

        public HttpController(IPhotoRepository photoRepository,
           ISearchRepository searchRepository,
           IResultRepository resultRepository)
        {
            this.photoRepository = photoRepository;
            this.searchRepository = searchRepository;
            this.resultRepository = resultRepository;
        }

        public string Index()
        {
            // IEnumerable<CustomerViewModel> customer = null;
            string str = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
                var responseTask = client.GetAsync("todos/1");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadAsStringAsync();
                    readJob.Wait();
                    str = readJob.Result;

                }
                else
                {

                }

                return str;
            }
        }

        public IActionResult ApiHandler(string uri, string phrase)
        {
            string str = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                var responseTask = client.GetAsync(phrase);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadAsStringAsync();
                    readJob.Wait();
                    str = readJob.Result;
                    var list = JsonConvert.DeserializeObject<List<Photo>>(str);
                    var search = new Search
                    {
                        Phrase = phrase,
                        Date = DateTime.Now
                    };
                    searchRepository.Add(search);
                    Result res;
                    foreach (var photo in list)
                    {
                        var returnedPhoto = photoRepository.Add(photo);
                        res = new Result
                        {
                            PhotoId = returnedPhoto.ID,
                            EngineId = 1,
                            SearchId = search.ID
                        };
                        resultRepository.Add(res);
                    }
                }
                else
                {

                }

                return RedirectToAction("Search", "Result", phrase);
            }
        }
    }
}
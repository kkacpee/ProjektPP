using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;

        public HttpController(ISearchRepository searchRepository,
            IPhotoRepository photoRepository,
           
           IResultRepository resultRepository,
           SignInManager<IdentityUser> signInManager)
        {
            this.photoRepository = photoRepository;
            this.searchRepository = searchRepository;
            this.resultRepository = resultRepository;
            this.signInManager = signInManager;
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

      
    }
}
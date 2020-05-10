using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Controllers
{
    public class ResultController : Controller
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly ISearchRepository _searchRepository;

        public ResultController(IPhotoRepository photoRepository, ISearchRepository searchRepository)
        {
            _photoRepository = photoRepository;
            _searchRepository = searchRepository;
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
            return View(photos);
        }

        [HttpGet]
        public IActionResult Result(int id)
        {
            Photo photo = _photoRepository.GetPhoto(id);
            return View(photo);
        }
    }
}
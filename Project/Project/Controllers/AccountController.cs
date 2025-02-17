﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IHistoryRepository historyRepository;
        private readonly ISearchRepository searchRepository;

        public AccountController(UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager,
            IHistoryRepository historyRepository,
            ISearchRepository searchRepository)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.historyRepository = historyRepository;
            this.searchRepository = searchRepository;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [AcceptVerbs("Get","Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use");
            }
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };
               var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if(signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                        return RedirectToAction("ListUsers", "Administration");
                    }
                    await userManager.AddToRoleAsync(user, "User");
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "home");
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, 
                    isPersistent: model.RememberMe, false);
                if (result.Succeeded)
                {
                    if(!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                var result = await userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    if (User.IsInRole("Admin") && User.Identity.Name != user.UserName)
                    {
                        return RedirectToAction("ListUsers");
                    }
                    else
                    {
                        return RedirectToAction("Logout");
                    }

                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("ListUsers");
            }
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string name)
        {
            ViewBag.Name = name;
            if (User.IsInRole("Admin") || User.Identity.Name == name)
            {
                var user = await userManager.FindByNameAsync(name);
                ViewBag.ID = user.Id;
                var results = historyRepository.GetHistoryForUser(user.Id);
                List<History> list = new List<History>();
                list = results.ToList();
                HistoriesWithNames hisWithName;
                List<HistoriesWithNames> listWithNames = new List<HistoriesWithNames>();
                foreach (var his in list)
                {
                    var usr = await userManager.FindByIdAsync(his.UserId);
                    var srch = searchRepository.GetSearch(his.SearchId);
                    hisWithName = new HistoriesWithNames(his, usr.Email, srch.Phrase);
                    listWithNames.Add(hisWithName);
                }
                listWithNames.Reverse();


                return View(listWithNames);
            }
            return RedirectToAction("AccessDenied");
        }
  

    [HttpPost]
    public async Task<IActionResult> FilteredProfile(string name)
    {
        ViewBag.Name = name;
        if (User.IsInRole("Admin") || User.Identity.Name == name)
        {
            var phraseCheck = Request.Form["phraseCheck"].ToString();
            var phraseSelect = Request.Form["phraseSelect"].ToString();
            var dateCheck = Request.Form["dateCheck"].ToString();
            var dateFrom = Request.Form["dateFrom"].ToString();
            var dateTo = Request.Form["dateTo"].ToString();

            var user = await userManager.FindByNameAsync(name);
            ViewBag.ID = user.Id;
            var results = historyRepository.GetHistoryForUser(user.Id);
            List<History> list = new List<History>();
            list = results.ToList();
            HistoriesWithNames hisWithName;
            List<HistoriesWithNames> listWithNames = new List<HistoriesWithNames>();
            foreach (var his in list)
            {
                var usr = await userManager.FindByIdAsync(his.UserId);
                var srch = searchRepository.GetSearch(his.SearchId);
                hisWithName = new HistoriesWithNames(his, usr.Email, srch.Phrase);
                listWithNames.Add(hisWithName);
            }
            if (phraseCheck.Equals("on"))
            {
                listWithNames = listWithNames.Where(e => e.Phrase == phraseSelect).ToList();
            }
            if (dateCheck.Equals("on"))
            {
                listWithNames = listWithNames.Where(e => e.Date >= Convert.ToDateTime(dateFrom) && e.Date <= Convert.ToDateTime(dateTo)).ToList();
            }

            listWithNames.Reverse();


            return View("Profile", listWithNames);
        }
        return RedirectToAction("AccessDenied");
    }

    }
}
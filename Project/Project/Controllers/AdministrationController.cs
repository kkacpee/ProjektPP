using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ISearchRepository searchRepository;
        private readonly IEngineRepository engineRepository;
        private readonly IResultRepository resultRepository;
        private readonly IHistoryRepository historyRepository;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
                                        UserManager<IdentityUser> userManager,
                                        ISearchRepository searchRepository,
                                        IEngineRepository engineRepository,
                                        IResultRepository resultRepository,
                                        IHistoryRepository historyRepository
                                        )
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.searchRepository = searchRepository;
            this.engineRepository = engineRepository;
            this.resultRepository = resultRepository;
            this.historyRepository = historyRepository;
        }
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };
                IdentityResult result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();
            foreach (var user in userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                model.Add(userRoleViewModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;

                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }


        [HttpGet]
        public IActionResult ListUsers()
        {
            var roles = userManager.Users;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }

            var userClaims = await userManager.GetClaimsAsync(user);
            var userRoles = await userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Claims = userClaims.Select(c => c.Value).ToList(),
                Roles = userRoles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;

                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

       
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                var result = await roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("ListRoles");
            }
        }

        [HttpGet]
        public IActionResult ListEngines()
        {
            var engines = engineRepository.GetAllEngines();
            return View(engines);
        }

        [HttpGet]
        public IActionResult CreateEngine()
        {
            return View();
        }



        [HttpPost]
        public IActionResult CreateEngine(Engine model)
        {
            if (ModelState.IsValid)
            {
                engineRepository.Add(model);
            }
            return RedirectToAction("ListEngines");
        }

        [HttpPost]
        public IActionResult DeleteEngine(int id)
        {
            engineRepository.Delete(id);
            return RedirectToAction("ListEngines");
        }

        [HttpGet]
        public IActionResult EditEngine(int id)
        {
            var model = engineRepository.GetEngine(id);
            return View(model);
        }

        [HttpPost]
        public IActionResult EditEngine(Engine model)
        {
            engineRepository.Update(model);
            return RedirectToAction("ListEngines");
        }

        [HttpGet]
        public IActionResult ListSearches()
        {
            var searches = searchRepository.GetAllSearches();
            List<Search> list = new List<Search>();
            list = searches.ToList();
            list.Reverse();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> ListHistories()
        {
            var results = historyRepository.GetAllHistories();
            List<History> list = new List<History>();
            list = results.ToList();
            HistoriesWithNames hisWithName;
            List<HistoriesWithNames> listWithNames = new List<HistoriesWithNames>();
            foreach(var his in list)
            {
                var usr = await userManager.FindByIdAsync(his.UserId);
                var srch = searchRepository.GetSearch(his.SearchId);
                hisWithName = new HistoriesWithNames(his, usr.Email, srch.Phrase);
                listWithNames.Add(hisWithName);
            }
            listWithNames.Reverse();
            return View(listWithNames);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public IActionResult DeleteHistory(int id, string name)
        {
            historyRepository.Delete(id);
            if (name.Any())
            {
                return RedirectToAction("Profile", "Account", new { name = name });
            }
            return RedirectToAction("ListHistories");
        }


        [HttpGet]
        public JsonResult JsonSearchList()
        {
           
            var searches = searchRepository.GetAllSearches();
            List<Search> list = new List<Search>();
            list = searches.ToList();
            list.Reverse();
            return Json(list);
        }

        [HttpGet]
        public IActionResult ListPhrases()
        {
            var result = searchRepository.GetAllSearches().Select(x => x.Phrase).Distinct();
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> FilteredHistories()
        {
            var userCheck = Request.Form["userCheck"].ToString();
            var userSelect = Request.Form["userSelect"].ToString();
            var phraseCheck = Request.Form["phraseCheck"].ToString();
            var phraseSelect = Request.Form["phraseSelect"].ToString();
            var dateCheck = Request.Form["dateCheck"].ToString();
            var dateFrom = Request.Form["dateFrom"].ToString();
            var dateTo = Request.Form["dateTo"].ToString();


            var results = historyRepository.GetAllHistories();
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

            if (userCheck.Equals("on"))
            {
                listWithNames = listWithNames.Where(e => e.UserName == userSelect).ToList();
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
            return View("ListHistories", listWithNames);
        }

    }
}
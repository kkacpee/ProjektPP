using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Controllers
{
    [AllowAnonymous]
    public class FirstController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IEngineRepository engineRepository;

        public FirstController(RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            IEngineRepository engineRepository)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.engineRepository = engineRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> First()
        {
            IdentityRole identityRole = new IdentityRole
            {
                Name = "Admin"
            };            
            IdentityRole identityRole2 = new IdentityRole
            {
                Name = "User"
            };
            IdentityUser adminUser = new IdentityUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com"
            };
            await roleManager.CreateAsync(identityRole);
            await roleManager.CreateAsync(identityRole2);
            await userManager.CreateAsync(adminUser, "Admin1");
            await userManager.AddToRoleAsync(adminUser, "Admin");

            Engine google = new Engine
            {
                Address = "https://www.google.com",
                Name = "Google",
                Description = "Google engine"
            };
            Engine pinterest = new Engine
            {

                Address = "https://pl.pinterest.com",
                Name = "Pinterest",
                Description = "Pinterest engine"
            };
            engineRepository.Add(google);
            engineRepository.Add(pinterest);

            return RedirectToAction("Index", "Home");
        }
    }
}
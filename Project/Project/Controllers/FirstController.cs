using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers
{
    [AllowAnonymous]
    public class FirstController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public FirstController(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
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
            IdentityResult result = await roleManager.CreateAsync(identityRole);

            IdentityRole identityRole2 = new IdentityRole
            {
                Name = "User"
            };
            IdentityResult result2 = await roleManager.CreateAsync(identityRole2);
            return RedirectToAction("Index", "Home");
        }
    }
}
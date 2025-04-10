using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace nhom6.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        private csdl db= new csdl();

        [HttpPost]

        public JsonResult CheckLogin()
        {
            bool isLoggedIn = User.Identity.IsAuthenticated;
            return Json(new { isLoggedIn });
        }
    }
}
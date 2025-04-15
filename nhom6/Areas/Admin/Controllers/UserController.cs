using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace nhom6.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        private csdl db = new csdl();
        // GET: Admin/User
        public ActionResult UserList()
        {
            var users = db.Users
         .Include(u => u.Role)
         .Select(u => new UserWithOwnerInfo
         {
             UserID = u.userID,
             UserName = u.userName,
             UserEmail = u.userEmail,
             RoleName = u.Role.RoleName,
             OwnerName = u.roleID == 0
                 ? db.Employees.FirstOrDefault(e => e.UserID == u.userID).EmployeeName
                 : db.Customers.FirstOrDefault(c => c.UserID == u.userID).CustomerName,
             RoleID = u.roleID
         }).ToList();

            return View(users);
        }
        public class UserWithOwnerInfo
        {
            public int UserID { get; set; }
            public string UserName { get; set; }
            public string UserEmail { get; set; }
            public string RoleName { get; set; }
            public string OwnerName { get; set; }
            public int RoleID { get; set; }
        }

    }
}
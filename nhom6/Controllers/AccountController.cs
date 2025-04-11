using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using nhom6.ViewModels;
// Đảm bảo đúng namespace chứa csdlEntities và User

namespace nhom6.Controllers
{
    public class AccountController : Controller
    {
        private csdlEntities db = new csdlEntities();

        // GET: /Account/Register
        public ActionResult Register()
        {
            var roles = db.Roles.ToList();

            var viewModel = new RegisterViewModel
            {
                RoleList = roles.Select(r => new SelectListItem
                {
                    Value = r.RoleID.ToString(),
                    Text = r.RoleName,
                    Selected = r.RoleID == 3 // Gán mặc định Customer
                }).ToList(),

                roleID = 3 // Mặc định là Customer
            };

            return View(viewModel);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tài khoản trùng hoàn toàn (userName, email, pass, role)
                var existingUser = db.Users.FirstOrDefault(u =>
                    u.userName == model.userName &&
                    u.userEmail == model.userEmail &&
                    u.userPass == model.userPass &&
                    u.roleID == model.roleID);

                if (existingUser != null)
                {
                    // Đã tồn tại → thông báo và chuyển sang Login
                    TempData["AlreadyRegistered"] = "Bạn đã có tài khoản! Đăng nhập ngay!";
                    return RedirectToAction("Login", "Account");
                }

                // Nếu chỉ trùng username thì báo lỗi ngay
                if (db.Users.Any(u => u.userName == model.userName))
                {
                    ModelState.AddModelError("userName", "Tên người dùng đã tồn tại.");
                }
                else
                {
                    // Tạo mới tài khoản
                    User newUser = new User
                    {
                        userName = model.userName,
                        userPass = model.userPass,
                        userEmail = model.userEmail,
                        roleID = model.roleID
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    ViewBag.Success = "🎉 Đăng ký thành công!";
                    ModelState.Clear(); // Reset form

                    // Reset RoleList cho dropdown
                    model = new RegisterViewModel
                    {
                        RoleList = db.Roles.Select(r => new SelectListItem
                        {
                            Value = r.RoleID.ToString(),
                            Text = r.RoleName,
                            Selected = r.RoleID == 3
                        }).ToList(),
                        roleID = 3
                    };

                    return View(model);
                }
            }

            // Nếu có lỗi, load lại RoleList để dropdown không null
            model.RoleList = db.Roles.Select(r => new SelectListItem
            {
                Value = r.RoleID.ToString(),
                Text = r.RoleName,
                Selected = r.RoleID == model.roleID
            }).ToList();

            return View(model);
        }

        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            Session.Clear(); // 🔥 Rất quan trọng: xóa session cũ trước mỗi lần login

            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u =>
                    u.userName == model.userName &&
                    u.userPass == model.userPass);

                if (user != null)
                {
                    Session["UserName"] = user.userName;
                    Session["RoleID"] = user.roleID;

                    return RedirectToAction("Index", "Home"); // 🎯 Chung cho cả employee và customer
                }

                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear(); // 🧹 Xoá toàn bộ session
            return RedirectToAction("Login", "Account");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using nhom6.ViewModels; // Đảm bảo đúng namespace chứa csdlEntities và User

namespace nhom6.Controllers
{
    public class AccountController : Controller
    {
        private csdlEntities db = new csdlEntities();

        // GET: /Account/Register
        public ActionResult Register()
        {
            var viewModel = new RegisterViewModel
            {
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
                model.roleID = 3; // Bắt buộc là Customer

                // Kiểm tra tài khoản trùng hoàn toàn
                var existingUser = db.Users.FirstOrDefault(u =>
                    u.userName == model.userName &&
                    u.userEmail == model.userEmail &&
                    u.userPass == model.userPass &&
                    u.roleID == model.roleID);

                if (existingUser != null)
                {
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

                    // ✅ Gán thông báo vào TempData để hiện pop-up bên Login
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";

                    return RedirectToAction("Login", "Account");
                }
            }

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
            Session.Clear(); // Xoá session cũ

            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u =>
                    u.userName == model.userName &&
                    u.userPass == model.userPass);

                if (user != null)
                {
                    Session["UserName"] = user.userName;
                    Session["RoleID"] = user.roleID;

                    return RedirectToAction("Index", "Product");
                }

                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}

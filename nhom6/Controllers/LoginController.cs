using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using nhom6.Models;

namespace nhom6.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        private csdl db= new csdl();
        private object objModel;

        [HttpPost]

        public JsonResult CheckLogin()
        {
            bool isLoggedIn = Session["UserID"] != null;
            return Json(new { isLoggedIn });
        }
        // GET: /Account/Register
        public ActionResult Register()
        {
            var viewModel = new RegisterModel
            {
                roleID = 1 // Mặc định là Customer
            };

            return View(viewModel);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                model.roleID = 1; // Bắt buộc là Customer

                // Kiểm tra tài khoản trùng hoàn toàn
                var existingUser = db.Users.FirstOrDefault(u =>
                    u.userName == model.userName &&
                    u.userEmail == model.userEmail &&
                    u.userPass == model.userPass &&
                    u.roleID == model.roleID);

                if (existingUser != null)
                {
                    TempData["AlreadyRegistered"] = "Bạn đã có tài khoản! Đăng nhập ngay!";
                    return RedirectToAction("Login", "Login");
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

                    // Tạo Customer tương ứng
                    Customer customer = new Customer
                    {
                        CustomerName = model.CustomerName,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        UserID = newUser.userID
                    };
                    db.Customers.Add(customer);
                    db.SaveChanges();

                    // Gán thông báo vào TempData để hiện pop-up bên Login
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";

                    return RedirectToAction("Login", "Login");
                }
            }

            return View(model);
        }
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
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
                    Session["UserID"] = user.userID;

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
            return RedirectToAction("Index", "Product");
        }
    }
}
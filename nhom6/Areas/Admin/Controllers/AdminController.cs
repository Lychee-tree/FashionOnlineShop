using nhom6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using System.Data.Entity.Validation;

namespace nhom6.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private csdl1 db = new csdl1();
        // GET: Admin/Admin
        public ActionResult Index()
        {
            return View();
        }

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
             OwnerName = u.roleID == 1
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

        public ActionResult DiscountList()
        {
            var discounts = db.Discounts.ToList();
            return View(discounts);
        }

        // GET: Create Discount
        public ActionResult CreateDiscount()
        {
            return View();
        }

        // POST: Create Discount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDiscount(Discount discount)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Additional validation
                    if (discount.ExpiryDate < discount.ValidityDate)
                    {
                        ModelState.AddModelError("ExpiryDate", "Expiry date must be after validity date");
                        return View(discount);
                    }

                    db.Discounts.Add(discount);
                    db.SaveChanges();
                    return RedirectToAction("DiscountList");
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }

            return View(discount);
        }

        // GET: Edit Discount
        public ActionResult EditDiscount(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discount discount = db.Discounts.Find(id);
            if (discount == null)
            {
                return HttpNotFound();
            }
            return View(discount);
        }

        // POST: Edit Discount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDiscount(Discount discount)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (discount.ExpiryDate < discount.ValidityDate)
                    {
                        ModelState.AddModelError("ExpiryDate", "Expiry date must be after validity date");
                        return View(discount);
                    }

                    db.Entry(discount).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("DiscountList");
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }

            return View(discount);
        }





        //chạy được
        //public ActionResult ListOrder()
        //{
        //    var orders = db.Orders.ToList(); // hiển thị ds đơn hàng
        //    return View(orders);
        //}

        //Thử list order mới
        public ActionResult ListOrder()
        {
            var orders = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.PaidStatu)
                .ToList(); // Load danh sách đơn hàng kèm thông tin liên quan
            return View(orders);
        }

        // Chấp nhận đơn hàng, chạy được
        //public ActionResult Accept(int? id)
        //{
        //    var acceptedOrders = db.Orders.Where(o => o.ShippingStatusID == 4).ToList();
        //    return View(acceptedOrders); // Chuyển hướng đến action hiển thị đơn hàng đã chấp nhận
        //}

        //Thử accept mới
        //public ActionResult AcceptOrder(int id)
        //{
        //    var order = db.Orders.Find(id);
        //    if (order != null)
        //    {
        //        order.ShippingStatusID = 4; // 4: "Đã xác nhận"
        //        db.SaveChanges();
        //        return RedirectToAction("AcceptedOrders");
        //    }
        //    return RedirectToAction("ListOrder");
        //}


        // Chấp nhận đơn hàng và cập nhật trạng thái
        public ActionResult Accept(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                // Giả sử 4 là trạng thái "Đã chấp nhận"
                order.ShippingStatusID = 4;
                db.SaveChanges();
            }
            return RedirectToAction("AcceptedOrders");
        }

        // Hiển thị danh sách đơn hàng đã chấp nhận
        public ActionResult AcceptedOrders()
        {
            var acceptedOrders = db.Orders
                .Where(o => o.ShippingStatusID == 4)
                .Include(o => o.Customer)
                .Include(o => o.PaidStatu)
                .ToList();
            return View(acceptedOrders);
        }

        // Hủy đơn hàng
        //public ActionResult Cancel(int id)
        //{
        //    var order = db.Orders.Find(id);
        //    if (order != null)
        //    {
        //        //order.Status = "Canceled";
        //        db.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}
        // Thêm phương thức xử lý hủy đơn hàng
        [HttpPost]
        public JsonResult Cancel(int id)
        {
            try
            {
                var order = db.Orders.Find(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Xóa các chi tiết đơn hàng trước
                var orderDetails = db.OrderDetails.Where(od => od.OrderID == id).ToList();
                db.OrderDetails.RemoveRange(orderDetails);

                // Xóa đơn hàng
                db.Orders.Remove(order);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Thêm phương thức cập nhật trạng thái đơn hàng
        [HttpPost]
        public JsonResult UpdateOrderStatus(int id, string status)
        {
            try
            {
                var order = db.Orders.Find(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Cập nhật trạng thái thanh toán
                var paidStatus = db.PaidStatus.FirstOrDefault(ps => ps.PaidStatus == status);
                if (paidStatus != null)
                {
                    order.PaidStatusID = paidStatus.PaidStatusID;
                    db.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
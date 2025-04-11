using System.Linq;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using nhom6.Models; // Đảm bảo Models được import
using System.Data.Entity; // For Include() method

namespace nhom6.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private csdl1 db = new csdl1(); // Đảm bảo csdl tồn tại & khớp với Web.config

        //chạy được
        public ActionResult ListOrder()
        {
            var orders = db.Orders.ToList(); // hiển thị ds đơn hàng
            return View(orders);
        }

        //Thử list order mới
        //public ActionResult ListOrder()
        //{
        //    var orders = db.Orders
        //        .Include(o => o.Customer)
        //        .Include(o => o.PaidStatu)
        //        .ToList(); // Load danh sách đơn hàng kèm thông tin liên quan
        //    return View(orders);
        //}

        // Chấp nhận đơn hàng, chạy được
        public ActionResult Accept()
        {
            var acceptedOrders = db.Orders.Where(o => o.ShippingStatusID == 4).ToList();
            return View(acceptedOrders); // Chuyển hướng đến action hiển thị đơn hàng đã chấp nhận
        }

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
        [HttpPost]
        public JsonResult Cancel(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                db.Orders.Remove(order); // Xóa đơn hàng
                db.SaveChanges(); // Lưu thay đổi vào database
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace nhom6.Controllers
{
    public class OrderController : Controller
    {
        private csdl db= new csdl();
        // GET: Order
        public ActionResult OrderList()
        {
            int currentUserId = Convert.ToInt32(Session["UserID"]);
            //int currentUserId = 3;
            // Tìm Customer tương ứng với User đang đăng nhập
            var customerId = db.Customers.FirstOrDefault(c => c.UserID == currentUserId)?.CustomerID;
            var orders = db.Orders
           .Where(o => o.CustomerID == customerId && o.ShippingStatusID !=2 )
           .Include(o=> o.Customer)
           .Include(o => o.Shipping_Status)
           .Include(o => o.PaidStatu)
           .Include(o => o.OrderDetails.Select(od => od.Instock.Product))
           .Include(o => o.OrderDetails.Select(od => od.Instock.Color))
           .Include(o => o.OrderDetails.Select(od => od.Instock.Size))
           .ToList();
            return View(orders);
        }
    }
}
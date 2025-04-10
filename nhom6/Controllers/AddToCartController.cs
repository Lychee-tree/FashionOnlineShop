using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace nhom6.Controllers
{
    public class AddToCartController : Controller
    {
        // GET: AddToCart
        private csdl db = new csdl();

        [HttpPost]
        public JsonResult AddToCart(int productId, int quantity)
        {
                int currentUserId = Convert.ToInt32(User.Identity.GetUserId());
                // Tìm Customer tương ứng với User đang đăng nhập
                var customer = db.Customers.FirstOrDefault(c => c.UserID == currentUserId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khách hàng." });
                }

                var order = db.Orders.FirstOrDefault(o => o.CustomerID == customer.CustomerID && o.ShippingStatusID == 0);

                if (order == null)
                {
                    order = new Order
                    {
                        CustomerID = customer.CustomerID,
                        OrderDate = DateTime.Now,
                        ShippingStatusID = 0,
                    };
                    db.Orders.Add(order);
                    db.SaveChanges();
                }

                var orderDetail = db.OrderDetails.FirstOrDefault(d => d.OrderID == order.OrderID && d.PvID ==productId );

                if (orderDetail != null)
                {
                    orderDetail.Quantity += quantity;
                }
                else
                {
                    var product = db.Products.Find(productId);
                    db.OrderDetails.Add(new OrderDetail
                    {
                        OrderID = order.OrderID,
                        PvID = productId,
                        Quantity = quantity,
                        TotalUnitprice = product.UnitPrice * quantity,
                    });
                }

                db.SaveChanges();

            return Json(new { success = true });
        }

    }
}
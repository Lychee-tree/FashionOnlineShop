using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using nhom6.Models;

namespace nhom6.Controllers
{
    public class AddToCartController : Controller
    {
        // GET: AddToCart
        private csdl db = new csdl();

        [HttpPost]
        public JsonResult AddToCart(int product, int size, int color, int quantity)
        {
            int currentUserId = Convert.ToInt32(Session["UserID"]);
            //int currentUserId = 3;
            // Tìm Customer tương ứng với User đang đăng nhập
            var customer = db.Customers.FirstOrDefault(c => c.UserID == currentUserId)?.CustomerID;
            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách hàng." });
            }
            // Tìm PvID (sản phẩm cụ thể)
            var pvid = db.Instocks.FirstOrDefault(x => x.ProductID == product && x.ColorID == color && x.SizeID == size)?.PvID;
            if (pvid == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại với thuộc tính đã chọn." });

            // Tìm đơn hàng đang mua sắm
            var order = db.Orders.FirstOrDefault(x => x.CustomerID == customer && x.ShippingStatusID == 2);
            if (order == null)
            {
                order = new Order
                {
                    CustomerID = customer.Value,
                    ShippingStatusID = 2,
                    OrderDate = DateTime.Now,
                    PaidStatusID = 1,
                    TotalPrice = 0
                };
                db.Orders.Add(order);
                db.SaveChanges();
            }

            var inStock = db.Instocks.FirstOrDefault(x => x.PvID == pvid);
            // Kiểm tra nếu đã có sản phẩm đó trong đơn, thì cộng số lượng
            var detail = db.OrderDetails.FirstOrDefault(d => d.OrderID == order.OrderID && d.PvID == pvid);
            if (detail != null)
            {
                detail.Quantity += quantity;
                detail.TotalUnitprice = detail.Quantity * inStock.Product.UnitPrice;
            }
            else
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    PvID = pvid.Value,
                    Quantity = quantity,
                    TotalUnitprice = quantity * inStock.Product.UnitPrice
                });
            }

            db.Orders.FirstOrDefault(o => o.OrderID==order.OrderID).TotalPrice = db.OrderDetails
                                                                                .Where(d => d.OrderID == order.OrderID)
                                                                                .Sum(d => (int?)d.TotalUnitprice) ?? 0;

            db.SaveChanges();

            db.Orders.FirstOrDefault(o => o.OrderID == order.OrderID).TotalPrice = db.OrderDetails
                                                                               .Where(d => d.OrderID == order.OrderID)
                                                                               .Sum(d => (int?)d.TotalUnitprice) ?? 0;

            db.SaveChanges();

            int cartCount = db.Orders
            .Where(o => o.CustomerID == customer && o.ShippingStatusID == 2)
            .SelectMany(o => o.OrderDetails)
            .Sum(od => (int?)od.Quantity) ?? 0;



            return Json(new { success = true, cartCount = cartCount });
        }

        [HttpGet]
        public JsonResult GetCartCount()
        {
            int currentUserId = Convert.ToInt32(Session["UserID"]);
            //int currentUserId = 3;
            // Tìm Customer tương ứng với User đang đăng nhập
            var customer = db.Customers.FirstOrDefault(c => c.UserID == currentUserId)?.CustomerID;
            int cartCount = db.Orders
             .Where(o => o.CustomerID == customer && o.ShippingStatusID == 2)
             .SelectMany(o => o.OrderDetails)
             .Sum(od => (int?)od.Quantity) ?? 0;

            return Json(new { success = true, cartCount = cartCount }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Cart()
        {
            int currentUserId = Convert.ToInt32(Session["UserID"]);
            // Tìm Customer tương ứng với User đang đăng nhập
            var customer = db.Customers.FirstOrDefault(c => c.UserID == currentUserId)?.CustomerID;
            //Tìm đơn hàng của customer đó đang ở trạng thái mua sắm
            var order = db.Orders.FirstOrDefault(x => x.CustomerID == customer && x.ShippingStatusID == 2)?.OrderID;
            if (customer == null || order == null)
            {
                return View(new List<CartItemModel>()); // Trả về view trống
            }
            ViewBag.TotalPrice = db.Orders.FirstOrDefault(o=> o.OrderID == order).TotalPrice;
            ViewBag.OrderID = order;
            var orderList = db.OrderDetails.Where(o=>o.OrderID==order)
                                            .Select(od => new CartItemModel
                                            {
                                                pvID = od.PvID,
                                                orderID = od.OrderID,
                                                ProductName = od.Instock.Product.ProductName,
                                                ColorName = od.Instock.Color.ColorName,
                                                SizeName = od.Instock.Size.SizeName,
                                                Quantity = od.Quantity,
                                                UnitPrice = od.Instock.Product.UnitPrice,
                                                TotalUnitPrice = od.TotalUnitprice,
                                                ProductImage = od.Instock.Color.ColorImages.FirstOrDefault(c=>c.ProductID== od.Instock.Product.ProductID).Image // nếu có ảnh
                                            }).ToList();

            return View(orderList);
        }

        [HttpPost]
        public ActionResult RemoveItem(int pvId, int orderId)
        {
            var orderDetail = db.OrderDetails.FirstOrDefault(od => od.PvID == pvId && od.OrderID== orderId);
            if (orderDetail != null)
            {
                db.OrderDetails.Remove(orderDetail);
                db.SaveChanges();

                db.Orders.FirstOrDefault(o => o.OrderID == orderId).TotalPrice = db.OrderDetails
                                                                              .Where(d => d.OrderID == orderId)
                                                                              .Sum(d => (int?)d.TotalUnitprice) ?? 0;

                db.SaveChanges();
            }
            var cartTotal = db.Orders.FirstOrDefault(o => o.OrderID == orderId).TotalPrice;
            return Json(new
            {
                success = true,
                cartTotalFormatted = cartTotal
            });
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int pvId, int orderId, int quantity)
        {
            var orderDetail = db.OrderDetails.FirstOrDefault(od => od.PvID==pvId && od.OrderID == orderId);
           
            if (orderDetail != null)
            {
                // Lấy số lượng tồn kho từ bảng Instock
                var stockQuantity = db.Instocks.FirstOrDefault( i => i.PvID == pvId).Instock1;

                // Nếu số lượng yêu cầu vượt quá tồn kho thì trả lại lỗi
                if (quantity > stockQuantity)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Số lượng vượt quá tồn kho.",
                        currentQuantity = orderDetail.Quantity
                    });
                }

                // Nếu hợp lệ thì cập nhật
                orderDetail.Quantity = quantity;
                orderDetail.TotalUnitprice = quantity * orderDetail.Instock.Product.UnitPrice;

                db.SaveChanges();

                db.Orders.FirstOrDefault(o => o.OrderID == orderId).TotalPrice = db.OrderDetails
                                                                             .Where(d => d.OrderID == orderId)
                                                                             .Sum(d => (int?)d.TotalUnitprice) ?? 0;

                db.SaveChanges();

                var totalUnitPrice = orderDetail.TotalUnitprice;
                var cartTotal = orderDetail.Order.TotalPrice;

                return Json(new
                {
                    success = true,
                    totalUnitPriceFormatted = totalUnitPrice,
                    cartTotalFormatted = cartTotal
                });
            }

            return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
        }

    }
}
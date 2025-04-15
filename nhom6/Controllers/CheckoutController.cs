using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using nhom6.Models;
namespace nhom6.Controllers
{
    public class CheckoutController : Controller
    {
        // GET: Checkout
        private csdl db = new csdl();
        [HttpPost]
        public ActionResult CheckCartStock(int orderId)
        {

            var outOfStockItems = new List<string>();

            var orderDetails = db.OrderDetails.Where(od => od.OrderID == orderId).ToList();

            foreach (var item in orderDetails)
            {
                var instock = db.Instocks.FirstOrDefault(i => i.PvID == item.PvID);
                if (instock == null || instock.Instock1 < item.Quantity)
                {
                    var productName = db.Products
                                        .Where(p => p.ProductID == item.Instock.ProductID)
                                        .Select(p => p.ProductName)
                                        .FirstOrDefault() ?? "Sản phẩm không xác định";
                    var size = db.Sizes
                              .Where(s => s.SizeID == item.Instock.SizeID)
                              .Select(s => s.SizeName)
                              .FirstOrDefault() ?? "Không xác định";
                    var color = db.Colors
                               .Where(c => c.ColorID == item.Instock.ColorID)
                               .Select(c => c.ColorName)
                               .FirstOrDefault() ?? "Không xác định";

                    outOfStockItems.Add($"{productName}(Màu: {color}, Kích thước: {size}): chỉ còn {instock?.Instock1 ?? 0}");
                }
            }

            if (outOfStockItems.Count > 0)
            {
                return Json(new
                {
                    success = false,
                    outOfStockItems
                });
            }

            return Json(new { success = true, orderId = orderId });
        }

        public ActionResult CheckOut(int orderId)
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId && o.ShippingStatusID == 2);

            if (order == null)
            {
                return HttpNotFound();
            }

            var CheckOutView = new CheckOutModel
            {
                OrderID = orderId,
                CustomerName = order.Customer.CustomerName,
                Email = order.Customer.User.userEmail,
                Phone = order.Customer.PhoneNumber,
                Address = order.Customer.Address,
                Note = order.Note,
                TotalPrice = order.TotalPrice,
                OrderDetails = order.OrderDetails.Select(od => new CheckoutItem
                {
                    ProductName = od.Instock.Product.ProductName,
                    Color = od.Instock.Color.ColorName,
                    Size = od.Instock.Size.SizeName,
                    Quantity = od.Quantity,
                    TotalUnitPrice = od.TotalUnitprice
                }).ToList()

            };
            return View(CheckOutView);
        }

        [HttpPost]
        public JsonResult GetDiscount(string discount)
        {
            var discountRow = db.Discounts.FirstOrDefault(d => d.DiscountCode == discount);
            if (discountRow == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không tồn tại." });
            }

            if (discountRow.ExpiryDate != null && discountRow.ExpiryDate < DateTime.Now)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn." });
            }

            int discountValue = discountRow.DiscountValue;
            var discountId = discountRow.DiscountID;
            return Json(new
            {
                success = true,
                discount = discountValue,
                discountId = discountId,
            });
        }

        [HttpPost]
        public JsonResult ConfirmOrder(int orderId,  string address, string phone, string note, int? discountId)
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId && o.ShippingStatusID == 2);
            if (order == null)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });

            var customer = order.Customer;
            if (!string.IsNullOrEmpty(phone)) customer.PhoneNumber = phone;
            if (!string.IsNullOrEmpty(address)) customer.Address = address;
            if (!string.IsNullOrEmpty(note)) order.Note = note;

            if (discountId.HasValue)
            {
                var discount = db.Discounts.Find(discountId.Value);
                if (discount != null)
                {
                    order.DiscountID = discountId;
                    int discountValue = discount.DiscountValue;
                    order.TotalPrice = order.TotalPrice * (100 - discountValue) / 100;
                }
            }
            // Trừ số lượng sản phẩm trong bảng Instock
            var orderDetails = db.OrderDetails.Where(d => d.OrderID == orderId).ToList();
            foreach (var detail in orderDetails)
            {
                var instock = db.Instocks.FirstOrDefault(i =>
                    i.ProductID == detail.Instock.ProductID &&
                    i.ColorID == detail.Instock.ColorID &&
                    i.SizeID == detail.Instock.SizeID);

                if (instock != null)
                {
                    instock.Instock1 -= detail.Quantity;
                    if (instock.Instock1 < 0)
                        instock.Instock1 = 0; // Đảm bảo không âm
                }
            }
            // Chuyển trạng thái đơn sang "chờ xác nhận" 
            order.ShippingStatusID = 3;
            order.OrderDate = DateTime.Now;
            
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
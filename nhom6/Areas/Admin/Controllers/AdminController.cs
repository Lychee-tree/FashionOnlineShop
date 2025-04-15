using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Caching;
using nhom6.Areas.Admin.Models;

namespace nhom6.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private csdl db =new csdl();
        // GET: Admin/Admin
        public ActionResult Index()
        {
            return View();
        }

        //Thử list order mới
        public ActionResult ListOrder()
        {
            var orders = db.Orders
                .Where(o => o.ShippingStatusID == 3)
                .Include(o => o.Customer)
                .Include(o => o.PaidStatu)
                .ToList(); // Load danh sách đơn hàng kèm thông tin liên quan

            return View(orders);
        }

        // Hiển thị danh sách đơn hàng đã chấp nhận
        public ActionResult AcceptedOrders()
        {
            var acceptedOrders = db.Orders
                .Where(o => (o.ShippingStatusID == 4 || o.ShippingStatusID == 5 || o.ShippingStatusID == 6) && o.PaidStatusID == 1)
                .Include(o => o.Customer)
                .Include(o => o.PaidStatu)
                .Include(o => o.Shipping_Status)
                .ToList();

            // Lấy tất cả trạng thái thanh toán từ database
            ViewBag.ShippingStatusList = db.Shipping_Status.ToList(); // Gán danh sách trạng thái
            ViewBag.PaidStatusList = db.PaidStatus.ToList();
            return View(acceptedOrders);
        }




        // Action POST để cập nhật trạng thái giao hàng và hình thức thanh toán
        [HttpPost]
        public JsonResult UpdateOrderStatus(int orderId, int paidStatusId, int shippingStatusId, string shippingCode)
        {
            try
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                order.PaidStatusID = paidStatusId;
                order.ShippingStatusID = shippingStatusId;
                order.ShippingCode = shippingCode?.Trim();

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


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

        //chấp nhận đơn hàng
        [HttpPost]
        public JsonResult Acceptbtn(int id)
        {
            try
            {
                int currentUserId = Convert.ToInt32(Session["UserID"]);

                var employee = db.Employees.FirstOrDefault(c => c.UserID == currentUserId)?.EmployeeID;
                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên." });
                }

                var order = db.Orders.Find(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                order.EmployeeID = employee;
                order.ShippingStatusID = 4; // Đã chấp nhận
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("AcceptedOrders", "Admin", new { area = "Admin" }) // nếu bạn đang ở trong Areas/Admin
                });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult CompletedOrders()
        {
            var orders = db.Orders
               .Where(o => o.ShippingStatusID == 6 && o.PaidStatusID == 2)
               .Include(o => o.Customer)
               .Include(o => o.PaidStatu)
               .Include(o => o.Employee)
               .ToList(); // Load danh sách đơn hàng kèm thông tin liên quan

            return View(orders);
        }

        public ActionResult OrderDetail(int id)
        {
            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Shipping_Status)
                .Include(o => o.PaidStatu)
                .Include(o => o.OrderDetails.Select(od => od.Instock.Product))
                .Include(o => o.OrderDetails.Select(od => od.Instock.Color))
                .Include(o => o.OrderDetails.Select(od => od.Instock.Size))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var viewModel = new OrderDetailViewModel
            {
                OrderId = order.OrderID,
                CustomerName = order.Customer.CustomerName,
                Phone = order.Customer.PhoneNumber,
                TotalPrice = order.TotalPrice,
                DiscountValue = order.Discount?.DiscountValue ?? 0,
                Products = order.OrderDetails.Select(od => new OrderProductDetail
                {
                    ProductName = od.Instock.Product.ProductName,
                    Color = od.Instock.Color.ColorName,
                    Size = od.Instock.Size.SizeName,
                    Quantity = od.Quantity,
                    TotalPrice = (int?) od.Quantity * od.Instock.Product.UnitPrice ?? 0,
                }).ToList()
            };

            return View(viewModel);
        }

    }
}
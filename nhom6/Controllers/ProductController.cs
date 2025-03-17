using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace nhom6.Frontend
{
    public class ProductController : Controller
    {
        private nhom6Entities db = new nhom6Entities();
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Shop()
        {
            var listProduct = db.Products.ToList();
            return View(listProduct);
        }
        public ActionResult ProductDetails(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            // Truy vấn sản phẩm theo id
            var product = db.Products.SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Truy vấn danh sách ảnh của sản phẩm
            var productImages = db.ProductImages.Where(pi => pi.ProductID == id).ToList();
            ViewBag.ProductImages = productImages;

            // Truy vấn danh sách size của sản phẩm
            var sizes = db.Instocks
                          .Where(i => i.ProductID == id)
                          .Select(i => i.Size)
                          .Distinct()
                          .ToList();
            ViewBag.Sizes = sizes;

            // Truy vấn danh mục của sản phẩm
            var category = db.Categories.SingleOrDefault(c => c.categoryID == product.CategoryID);
            ViewBag.CategoryName = category?.categoryName;
            if (category != null)
            {
                // Truyền đường dẫn ảnh size sang view
                ViewBag.CategorySizeImage = category.CategorySizeImage;
            }

            // Truy vấn danh sách ảnh của sản phẩm từ bảng ColorImage
            var colorImages = db.ColorImages
                                .Where(ci => ci.ProductID == id)
                                .GroupBy(ci => ci.ColorID) // Nhóm ảnh theo ColorID
                                .ToDictionary(g => g.Key, g => g.ToList()); // Chuyển thành Dictionary để dễ truy cập

            ViewBag.ColorImages = colorImages;


            // Truy vấn danh sách màu sắc của sản phẩm
            var colors = db.ColorImages
                .Where(ci => ci.ProductID == id)
                .Select(ci => ci.Color)
                .Distinct()
                .ToList();
            ViewBag.Colors = colors;

            // Mặc định chọn ColorID đầu tiên
            var firstColor = colors.FirstOrDefault();
            int firstColorId = firstColor?.ColorID ?? 0;
            ViewBag.SelectedColorId = firstColorId;

            // Lấy ảnh đầu tiên từ ColorImage tương ứng với màu hiện tại
            var firstColorImage = db.ColorImages
                .Where(ci => ci.ColorID == firstColorId && ci.ProductID == id)
                .Select(ci => ci.Image)
                .FirstOrDefault();
            ViewBag.FirstColorImage = firstColorImage;

            // Truy vấn các sản phẩm cùng danh mục
            var relatedProducts = db.Products
                .Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID)
                .Take(4) // Lấy tối đa 4 sản phẩm
                .ToList();
            ViewBag.RelatedProducts = relatedProducts;

            // Kiểm tra tồn kho
            var instock = db.Instocks
                            .Where(i => i.ProductID == id)
                            .Sum(i => i.Instock1); // Tính tổng số lượng tồn kho
            ViewBag.InStock = instock > 0; // Trả về true nếu còn hàng, false nếu hết hàng

            // Trả về view với sản phẩm
            return View(product);
        }
        public ActionResult GetColorImages(int colorId)
        {
            // Lấy danh sách ảnh của ColorID được chọn
            var colorImages = db.ColorImages
                .Where(ci => ci.ColorID == colorId)
                .Select(ci => new { ci.Image })
                .ToList();

            return Json(colorImages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStockQuantity(int productId, int colorId, int sizeId)
        {
            // Truy vấn số lượng tồn kho từ bảng Instock
            var stockQuantity = db.Instocks
                                  .Where(i => i.ProductID == productId && i.ColorID == colorId && i.SizeID == sizeId)
                                  .Sum(i => i.Instock1);

            return Json(new { stock = stockQuantity }, JsonRequestBehavior.AllowGet);
        }
    }
}
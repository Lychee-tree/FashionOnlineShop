using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using nhom6.Models;
using System.Diagnostics;

namespace nhom6.Controllers
{
    public class ProductController : Controller
    {
        private csdl db= new csdl();
        // GET: Product
        public ActionResult Index()
        {
            // Sử dụng Include để load dữ liệu liên quan
            var allProducts = db.Products
                              .Include(p => p.ColorImages.Select(ci => ci.Color))
                              .OrderBy(p => p.ProductID)
                              .ToList();

            // Chuyển đổi sang ViewModel
            var productViewModels = allProducts.Select(p => new ProductViewModel
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                CategoryID = p.CategoryID,
                UnitPrice = p.UnitPrice,
                UnitDescription = p.UnitDescription,
                UnitImage = p.UnitImage,
                Colors = p.ColorImages.Select(ci => new ProductColorViewModel
                {
                    ColorID = ci.Color.ColorID,
                    ColorName = ci.Color.ColorName,
                    ColorCode = ci.Color.ColorCode,
                    Image = ci.Image
                }).ToList()
            }).ToList();

            ViewBag.TopProducts = productViewModels.Where(p => p.CategoryID == 1).Take(8).ToList();
            ViewBag.BottomProducts = productViewModels.Where(p => p.CategoryID == 2).Take(8).ToList();
            ViewBag.OuterwearProducts = productViewModels.Where(p => p.CategoryID == 3).Take(8).ToList();

            return View(productViewModels); // Truyền model chính xác vào View
        }


        public ActionResult Shop()
        {
            var listProduct = db.Products.ToList();
            // Lấy danh sách loại và đếm số lượng sản phẩm theo loại
            ViewBag.Categories = db.Categories
                .Select(c => new CountModel //Object ẩn c có thêm cột productCount
            {
                CategoryID=c.categoryID,
                CategoryName=c.categoryName,
                ProductCount = db.Products.Count(p => p.CategoryID == c.categoryID)
            }).ToList();

            ViewBag.Colors = db.Colors
                .Select(m => new ColorModel
                { ColorID=m.ColorID, 
                  ColorCode= m.ColorCode
                }).ToList();

            ViewBag.Sizes = db.Sizes
                .Select(p => new SizeModel
                { SizeID=p.SizeID, 
                  SizeName=p.SizeName 
                }).ToList();

            return View(listProduct);
        }

        public JsonResult FilterProducts(int? categoryId, int? price, int? color, int? size, int? sort)
        {
            var products = db.Products.AsQueryable();
           

            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryID == categoryId);
                Debug.WriteLine("Đã phân loại xong");
            }

            //lọc theo giá
            if(price.HasValue && price > 0) { 
                switch (price.Value)
                {
                    case 1:
                        products = products.Where(p => p.UnitPrice < 500000);
                        break;
                    case 2:
                        products = products.Where(p => p.UnitPrice >= 500000 && p.UnitPrice < 1000000);
                        break;
                    case 3:
                        products = products.Where(p => p.UnitPrice > 1000000);
                        break;
                }
            }
            //lọc theo màu
            if (color.HasValue && color > 0)
            {
                products = products.Where(p => db.Instocks.Any(i => i.ProductID==p.ProductID && i.ColorID == color));
            }

            //lọc theo size
            if (size.HasValue && size > 0)
            {
                products = products.Where(p => db.Instocks.Any(i => i.ProductID == p.ProductID && i.SizeID == size));
            }
            //sắp xếp sản phẩm
            if (sort.HasValue && sort > 0)
            {
                Debug.WriteLine("Sort Value: " + sort.Value); // Kiểm tra giá trị
                switch (sort.Value)
                {
                    case 1:
                        products = products.OrderBy(p => p.UnitPrice);
                        break;
                    case 2:
                        products = products.OrderByDescending(p => p.UnitPrice);
                        break;
                }
            }

            var result = products.Select(p => new
            {
                p.ProductID,
                p.ProductName,
                p.UnitPrice,
                ImagePath = p.UnitImage
            }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetColorImages(int colorId, int productId)
        {
            // Lấy danh sách ảnh của ColorID được chọn
            var colorImages = db.ColorImages
                .Where(ci => ci.ColorID == colorId && ci.ProductID == productId)
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

        public ActionResult Contact()
        {
            return View();
        }
    }
}
public class ProductViewModel
{
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public int CategoryID { get; set; }
    public int UnitPrice { get; set; }
    public string UnitDescription { get; set; }
    public string UnitImage { get; set; }
    public List<ProductColorViewModel> Colors { get; set; }

}

public class ProductColorViewModel
{
    public int ColorID { get; set; }
    public string ColorName { get; set; }
    public string ColorCode { get; set; }
    public string Image { get; set; }
}
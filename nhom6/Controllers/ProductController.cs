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
            return View();
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
                ImagePath = "/Content/Image/" + p.UnitImage
            }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}

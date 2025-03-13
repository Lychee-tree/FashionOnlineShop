using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using nhom6.Models;

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
            var categories = db.Categories
                .Select(c => new CountModel //Object ẩn c có thêm cột productCount
            {
                CategoryID=c.categoryID,
                CategoryName=c.categoryName,
                ProductCount = db.Products.Count(p => p.CategoryID == c.categoryID)
            }).ToList();

            ViewBag.Categories = categories;

            return View(listProduct);
        }

        public JsonResult FilterProducts(int? categoryId)
        {
            var products = db.Products.AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryID == categoryId);
            }

            var result = products.Select(p => new
            {
                p.ProductName,
                p.UnitPrice,
                ImagePath = "/Content/Image/" + p.UnitImage
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}

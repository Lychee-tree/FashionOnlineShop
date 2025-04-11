using nhom6.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using X.PagedList;


namespace nhom6.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private csdl1 db = new csdl1();
        // GET: Admin/Admin
        public ActionResult Index()
        {
            return View();
        }
        //Tìm kiếm
        public ActionResult list_Product(string searchTerm)
        {
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Sử dụng LIKE chính xác hơn
                products = products.Where(p => DbFunctions.Like(p.ProductName, "%" + searchTerm + "%"));
            }

            ViewBag.CurrentSearch = searchTerm;
            return View(products.ToList());
        }

        //Xem chi tiết

        public ActionResult Detail(int id)
        {
            var product = db.Products.Include("Category").FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        //Xóa sản phẩm
        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                using (var db = new csdl1()) // Đảm bảo DbContext của bạn đúng
                {
                    var product = db.Products.FirstOrDefault(p => p.ProductID == id);
                    if (product == null)
                    {
                        return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                    }

                    db.Products.Remove(product);
                    db.SaveChanges(); // Xóa sản phẩm khỏi database

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        //Edit
        [HttpPost]
        public ActionResult Edit(FormCollection form, HttpPostedFileBase uploadImage)
        {
            int productId = int.Parse(form["ProductID"]);
            var product = db.Products.Find(productId);

            if (product == null) return HttpNotFound();

            // Cập nhật thông tin chính
            product.ProductName = form["ProductName"];
            product.UnitDescription = form["UnitDescription"];
            //product.Product = decimal.Parse(form["TotalUnitprice"]);
            product.CategoryID = int.Parse(form["CategoryID"]);

            // Xử lý ảnh nếu có
            if (uploadImage != null && uploadImage.ContentLength > 0)
            {
                string fileName = Path.GetFileName(uploadImage.FileName);
                string path = Path.Combine(Server.MapPath("~/Content/Image/"), fileName);
                uploadImage.SaveAs(path);
                product.UnitImage = fileName;
            }

            db.Entry(product).State = EntityState.Modified;

            // Xoá dữ liệu tồn kho cũ
            var oldInstocks = db.Instocks.Where(x => x.ProductID == productId);
            db.Instocks.RemoveRange(oldInstocks);
            db.SaveChanges();

            // Thêm dữ liệu tồn kho mới
            var variantIndices = form.GetValues("variantIndex");
            if (variantIndices != null)
            {
                foreach (var i in variantIndices)
                {
                    int colorId = int.Parse(form[$"ColorID_{i}"]);
                    int sizeId = int.Parse(form[$"SizeID_{i}"]);
                    int instock = int.Parse(form[$"Instock_{i}"]);

                    db.Instocks.Add(new Instock
                    {
                        ProductID = productId,
                        ColorID = colorId,
                        SizeID = sizeId,
                        Instock1 = instock
                    });
                }
            }

            db.SaveChanges();
            return RedirectToAction("list_Product");
        }
        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.Colors = new SelectList(db.Colors, "ColorID", "ColorName");
            ViewBag.Sizes = new SelectList(db.Sizes, "SizeID", "SizeName");

            // Lấy danh sách mẫu mã (Color + Size + Instock) theo ProductID
            ViewBag.Instocks = db.Instocks
                .Where(i => i.ProductID == id)
                .Select(i => new ProductVariantsModel
                {
                    ColorID = i.ColorID,
                    SizeID = i.SizeID,
                    Instock = i.Instock1
                }).ToList();
            return View(product);
        }


        //Edit
        //public ActionResult Edit(int id)
        // {
        // var product = db.Products.FirstOrDefault(p => p.ProductID == id);
        // if (product == null)
        // {
        //  return HttpNotFound();
        // }

        // ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);

        // Truy vấn thông tin tồn kho kèm màu, size, hình ảnh
        //  var instocks = (from i in db.Instocks
        //  join c in db.Colors on i.ColorID equals c.ColorID
        // join s in db.Sizes on i.SizeID equals s.SizeID
        // where i.ProductID == id
        // select new CustomStock
        //  {
        //      ProductID = i.ProductID,
        //        ColorID = i.ColorID,
        //         SizeID = i.SizeID,
        //          
        //            ColorName = c.ColorName,
        //             SizeName = s.SizeName
        //          }).ToList();

        // ViewBag.Instocks = instocks;

        // return View(product);
        // }

        // Class tạm để truyền dữ liệu ra View (không cần tạo Model riêng)
        public class CustomStock
        {
            public int ProductID { get; set; }
            public int ColorID { get; set; }
            public int SizeID { get; set; }
            public int Quantity { get; set; }
            public string Image { get; set; }
            public string ColorName { get; set; }
            public string SizeName { get; set; }
        }




        // THÊM SẢN PHẨM MỚI
        [HttpGet]
        public ActionResult AddProduct()
        {
            ViewBag.Colors = new SelectList(db.Colors.ToList(), "ColorID", "ColorName");
            ViewBag.Sizes = new SelectList(db.Sizes.ToList(), "SizeID", "SizeName");
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(FormCollection form, HttpPostedFileBase uploadImage)
        {
            try
            {
                string name = form["ProductName"];
                int categoryId = Convert.ToInt32(form["CategoryID"]);
                decimal price = Convert.ToDecimal(form["UnitPrice"]);
                string desc = form["UnitDescription"];

                // Xử lý lưu ảnh
                string fileName = "";
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    fileName = Path.GetFileName(uploadImage.FileName);
                    string path = Server.MapPath("~/Content/Image/" + fileName);
                    uploadImage.SaveAs(path);
                }

                // Tạo sản phẩm
                var product = new Product
                {
                    ProductName = name,
                    CategoryID = categoryId,
                    UnitPrice = (int)price,
                    UnitDescription = desc,
                    UnitImage = fileName
                };

                db.Products.Add(product);
                db.SaveChanges(); // Lưu để lấy ProductID

                // Xử lý nhiều mẫu mã: Màu + Size + Tồn kho
                int index = 0;
                while (form[$"ColorID_{index}"] != null)
                {
                    var colorId = Convert.ToInt32(form[$"ColorID_{index}"]);
                    var sizeId = Convert.ToInt32(form[$"SizeID_{index}"]);
                    var instock = Convert.ToInt32(form[$"Instock_{index}"]);

                    var variant = new Instock
                    {
                        ProductID = product.ProductID,
                        ColorID = colorId,
                        SizeID = sizeId,
                        Instock1 = instock
                    };
                    db.Instocks.Add(variant);
                    index++;
                }

                db.SaveChanges();
                return RedirectToAction("list_Product");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                ViewBag.Colors = new SelectList(db.Colors.ToList(), "ColorID", "ColorName");
                ViewBag.Sizes = new SelectList(db.Sizes.ToList(), "SizeID", "SizeName");
                return View();
            }
        }



    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using nhom6.Areas.Admin.Models;
using Antlr.Runtime.Misc;
using System.Data.Entity;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Data.Entity.Infrastructure;


namespace nhom6.Areas.Admin.Controllers
{
    public class ProductAdminController : Controller
    {
        private csdl db= new csdl();
        //Tìm kiếm
        public ActionResult list_Product(string searchTerm)
        {
            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.Instocks.Select(i => i.Color))
                .Include(p => p.Instocks.Select(i => i.Size))
                .ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(searchTerm)).ToList();
            }

            return View(products);
        }


        ////Xem chi tiết
        public ActionResult Detail(int id)
        {
            var product = db.Products.Include(p => p.Instocks.Select(i => i.Color)).Include(p => p.Instocks.Select(i => i.Size))
                                     .FirstOrDefault(p => p.ProductID == id);

            return View(product);
        }


        //public ActionResult Detail(int id)
        //{
        //    var product = db.Products.Include("Category").FirstOrDefault(p => p.ProductID == id);
        //    if (product == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(product);
        //}

        //Xóa sản phẩm
        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                var product = db.Products.Find(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
                }

                // Lấy danh sách ảnh để xóa file vật lý
                var productImages = db.ProductImages.Where(p => p.ProductID == id).ToList();
                var colorImages = db.ColorImages.Where(c => c.ProductID == id).ToList();
                var instocks = db.Instocks.Where(i => i.ProductID == id).ToList();

                // Xóa file vật lý trong thư mục
                foreach (var img in productImages)
                {
                    var path = Server.MapPath("~/Content/Image/" + img.Image);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                foreach (var color in colorImages)
                {
                    var path = Server.MapPath("~/Content/Image/" + color.Image);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                if (!string.IsNullOrEmpty(product.UnitImage))
                {   

                    var mainImagePath = Server.MapPath("~/Content/Image/" + product.UnitImage);
                    if (System.IO.File.Exists(mainImagePath))
                        System.IO.File.Delete(mainImagePath);
                }

                // Xóa các bảng phụ dùng RemoveRange
                db.Instocks.RemoveRange(instocks);
                db.ProductImages.RemoveRange(productImages);
                db.ColorImages.RemoveRange(colorImages);

                db.Products.Remove(product);

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private string GetFullErrorMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.Message;
        }


        //Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model, HttpPostedFileBase uploadImage, FormCollection form)
        {
            if (!ModelState.IsValid)
                return View(model);

            var product = db.Products.Find(model.ProductID);
            if (product == null) return HttpNotFound();

            // Cập nhật thông tin sản phẩm
            product.ProductName = model.ProductName;
            product.UnitDescription = model.UnitDescription;
            product.UnitPrice = model.UnitPrice;
            product.CategoryID = model.CategoryID;

            // Cập nhật ảnh nếu có
            if (uploadImage != null && uploadImage.ContentLength > 0)
            {
                var path = Server.MapPath("~/Content/Image/" + product.UnitImage);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                string fileName = Path.GetFileName(uploadImage.FileName);
                string newPath = Path.Combine(Server.MapPath("~/Content/Image/"), fileName);
                uploadImage.SaveAs(newPath);
                product.UnitImage = fileName;
            }

            db.Entry(product).State = EntityState.Modified;

            // Cập nhật tồn kho cũ
            var keys = form.AllKeys.Where(k => k.StartsWith("Instocks[") && k.EndsWith("].Quantity"));
            foreach (var key in keys)
            {
                var pvIDStr = key.Substring(9, key.IndexOf("]") - 9);
                if (int.TryParse(pvIDStr, out int pvID))
                {
                    var instock = db.Instocks.FirstOrDefault(i => i.PvID == pvID);
                    if (instock != null && int.TryParse(form[key], out int quantity))
                    {
                        instock.Instock1 = quantity;
                        db.Entry(instock).State = EntityState.Modified;
                    }
                }
            }

            // Thêm mẫu mã mới
            int index = 0;
            while (true)
            {
                var colorKey = $"ColorID_{index}";
                var sizeKey = $"SizeID_{index}";
                var quantityKey = $"Instock_{index}";

                if (!form.AllKeys.Contains(colorKey)) break;

                int colorId = int.Parse(form[colorKey]);
                int sizeId = int.Parse(form[sizeKey]);
                int quantity = int.Parse(form[quantityKey]);

                // Kiểm tra xem mẫu mã này đã tồn tại chưa
                bool exists = db.Instocks.Any(i => i.ProductID == model.ProductID && i.ColorID == colorId && i.SizeID == sizeId);
                if (exists)
                {
                    if (string.IsNullOrEmpty(model.UnitImage))
                    {
                        var oldProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == model.ProductID);
                        if (oldProduct != null)
                            model.UnitImage = oldProduct.UnitImage;
                    }

                    ModelState.AddModelError("", $"Mẫu mã {index + 1} đã tồn tại.");
                    LoadViewBagsForEdit(model.ProductID, model.CategoryID);
                    return View(model); 
                }

                // Thêm mẫu mã mới
                var newInstock = new Instock
                {
                    ProductID = model.ProductID,
                    ColorID = colorId,
                    SizeID = sizeId,
                    Instock1 = quantity
                };
                db.Instocks.Add(newInstock);

                // Kiểm tra nếu ảnh mẫu mã chưa có thì thêm vào ColorImage
                bool hasColorImage = db.ColorImages.Any(ci => ci.ProductID == model.ProductID && ci.ColorID == colorId);
                if (!hasColorImage)
                {
                    db.ColorImages.Add(new ColorImage
                    {
                        ProductID = model.ProductID,
                        ColorID = colorId,
                        Image = null 
                    });
                }

                index++;
            }


            db.SaveChanges();
            return RedirectToAction("Edit", new { id = model.ProductID });
        }

        private void LoadViewBagsForEdit(int productId, int? selectedCategoryId = null)
        {
            ViewBag.Instocks = db.Instocks
                .Include(i => i.Color)
                .Include(i => i.Size)
                .Where(i => i.ProductID == productId)
                .ToList();

            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName", selectedCategoryId);
        }

        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.Colors= db.Colors.ToList();
            ViewBag.Sizes = db.Sizes.ToList();
            // Lấy danh sách mẫu mã (Color + Size + Instock) theo ProductID
            ViewBag.Instocks = db.Instocks
                .Where(i => i.ProductID == id).ToList();
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
            ViewBag.Colors = db.Colors.ToList();
            ViewBag.Sizes = db.Sizes.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(FormCollection form, HttpPostedFileBase uploadImage, IEnumerable<HttpPostedFileBase> subImages)
        {
            try
            {
                string name = form["ProductName"];
                int categoryId = Convert.ToInt32(form["CategoryID"]);
                decimal price = Convert.ToDecimal(form["UnitPrice"]);
                string desc = form["UnitDescription"];

                // Xử lý ảnh chính
                string fileName = "";
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    fileName = Path.GetFileName(uploadImage.FileName);
                    string path = Server.MapPath("~/Content/Image/" + fileName);
                    uploadImage.SaveAs(path);
                }

                // Tạo sản phẩm mới
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

                // Thêm các mẫu mã (màu + size + tồn kho)
                int index = 0;
                HashSet<int> addedColors = new HashSet<int>();

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
                    addedColors.Add(colorId); // Ghi lại các màu đã dùng
                    index++;
                }

                db.SaveChanges();

                // Lọc trùng
                var distinctColorIds = addedColors.Distinct().ToList();

                foreach (int colorId in distinctColorIds)
                {
                    db.ColorImages.Add(new ColorImage
                    {
                        ProductID = product.ProductID,
                        ColorID = colorId,
                        Image = null // chưa có ảnh
                    });
                }

                db.SaveChanges();

                // Xử lý ảnh phụ (nếu có)
                if (subImages != null)
                {
                    foreach (var img in subImages)
                    {
                        if (img != null && img.ContentLength > 0)
                        {
                            string subFileName = Path.GetFileName(img.FileName);
                            string subPath = Server.MapPath("~/Content/Image/" + subFileName);
                            img.SaveAs(subPath);

                            db.ProductImages.Add(new ProductImage
                            {
                                ProductID = product.ProductID,
                                Image = subFileName
                            });
                        }
                    }
                }
                db.SaveChanges();
                // Chuyển đến trang thêm ảnh cho từng màu
                return RedirectToAction("AddColorImages", new { id = product.ProductID });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                ViewBag.Colors = new SelectList(db.Colors.ToList(), "ColorID", "ColorName");
                ViewBag.Sizes = new SelectList(db.Sizes.ToList(), "SizeID", "SizeName");
                return View();
            }
        }

        public ActionResult AddColorImages(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.ProductName = product.ProductName;
            ViewBag.ProductID = id;

            ViewBag.ColorList = db.ColorImages.Where(c => c.ProductID == id).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddColorImage(FormCollection form, IEnumerable<HttpPostedFileBase> colorImages)
        {
            int productId = int.Parse(form["ProductID"]);
            var colorList = db.ColorImages.Where(c => c.ProductID == productId).ToList();

            int index = 0;
            foreach (var file in colorImages)
            {
                if (file != null && file.ContentLength > 0 && index < colorList.Count)
                {
                    // Lấy đúng ColorID ở vị trí tương ứng
                    int colorId = colorList[index].ColorID;

                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/Image/"), fileName);
                    file.SaveAs(path);

                    var colorImage = db.ColorImages.FirstOrDefault(ci => ci.ProductID == productId && ci.ColorID == colorId);
                    if (colorImage != null)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(colorImage.Image))
                        {
                            string oldPath = Path.Combine(Server.MapPath("~/Content/Image/"), colorImage.Image);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        colorImage.Image = fileName;
                        db.Entry(colorImage).State = EntityState.Modified;
                    }
                }

                index++;
            }

            db.SaveChanges();
            return RedirectToAction("AddColorImages", new { id = productId });
        }

    }
}
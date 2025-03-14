using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public ActionResult listProduct()
        {
            var listProduct = db.Products.ToList();
            return View(listProduct);
        }
    }
}
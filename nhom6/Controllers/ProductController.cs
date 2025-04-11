using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using nhom6.Models;

namespace nhom6.Frontend
{
    public class ProductController : Controller
    {
        private csdl db= new csdl();
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult Shop()
        //{
        //    var listProduct = db.Products.ToList();
        //    return View(listProduct);
        //}
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using X.PagedList;

namespace nhom6.Frontend
{
    public class ProductController : Controller
    {
        private csdl1 db= new csdl1();
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

        public ActionResult Contact()
        {
            return View();
        }
        

    }


}
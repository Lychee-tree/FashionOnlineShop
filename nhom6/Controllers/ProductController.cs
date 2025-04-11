using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Security.Cryptography;
using System.Text;

namespace nhom6.Frontend
{
    public class ProductController : Controller
    {
        private csdlEntities db = new csdlEntities();
        private object objModel;

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

        public ActionResult Login()
        {
            return View();
        }

      
        
        
       

    }
}
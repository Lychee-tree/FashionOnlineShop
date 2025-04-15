using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace nhom6.Areas.Admin.Controllers
{
    public class DiscountController : Controller
    {
        private csdl db = new csdl();
        // GET: Admin/Discount
        public ActionResult DiscountList()
        {
            var discounts = db.Discounts.ToList();
            return View(discounts);
        }

        // GET: Create Discount
        public ActionResult CreateDiscount()
        {
            return View();
        }

        // POST: Create Discount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDiscount(Discount discount)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Additional validation
                    if (discount.ExpiryDate < discount.ValidityDate)
                    {
                        ModelState.AddModelError("ExpiryDate", "Expiry date must be after validity date");
                        return View(discount);
                    }

                    db.Discounts.Add(discount);
                    db.SaveChanges();
                    return RedirectToAction("DiscountList");
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }

            return View(discount);
        }

        // GET: Edit Discount
        public ActionResult EditDiscount(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discount discount = db.Discounts.Find(id);
            if (discount == null)
            {
                return HttpNotFound();
            }
            return View(discount);
        }

        // POST: Edit Discount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDiscount(Discount discount)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (discount.ExpiryDate < discount.ValidityDate)
                    {
                        ModelState.AddModelError("ExpiryDate", "Expiry date must be after validity date");
                        return View(discount);
                    }

                    db.Entry(discount).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("DiscountList");
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }

            return View(discount);
        }
    }
}
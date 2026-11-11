using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MusicStore.Models;

namespace MusicStore.Areas.Admin.Controllers
{
    public class ProductsAdminController : Controller
    {
        private MusicStoreDBModel db = new MusicStoreDBModel();

        [ChildActionOnly]
        public PartialViewResult CategoryMenu()
        {
            var list = db.Categories.ToList();
            return PartialView(list);
        }

        public ActionResult ProductsByCategory(int id)
        {
            var list = db.Products.Where(x => x.CategoryId == id).ToList();
            ViewBag.CategoryName = db.Categories
                             .Where(c => c.CategoryId == id)
                             .Select(c => c.CategoryName)
                             .FirstOrDefault();
            return View(list);
        }

        // GET: Admin/ProductsAdmin
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Admin/ProductsAdmin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/ProductsAdmin/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Admin/ProductsAdmin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return Json(new
                {
                    result = false,
                    error = string.Join("<br/>", errors)
                });
            }
            try
            {
                var f = Request.Files["ImageFile"];

                if (f != null && f.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(f.FileName);

                    string uploadPath =
                        Server.MapPath("~/Images/" + fileName);

                    f.SaveAs(uploadPath);

                    product.ImageUrl = fileName;
                }

                db.Products.Add(product);
                db.SaveChanges();
                return Json(new { result = true, JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                    error = ex.InnerException == null
                            ? ex.Message
                            : ex.InnerException.Message
                });
            }
        }

        // GET: Admin/ProductsAdmin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Admin/ProductsAdmin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            try
            {
                var f = Request.Files["ImageFile"];

                // Nếu có chọn ảnh mới
                if (f != null && f.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(f.FileName);

                    string uploadPath = Server.MapPath("~/Images/" + fileName);

                    f.SaveAs(uploadPath);

                    product.ImageUrl = fileName;
                }
                else
                {
                    product.ImageUrl = db.Products
                                        .Where(p => p.ProductId == product.ProductId)
                                        .Select(p => p.ImageUrl)
                                        .FirstOrDefault();
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new
                {
                    result = true,
                    message = "Update successfully."
                });
            }
            catch
            {
                return Json(new
                {
                    result = false,
                    error = "Update has failed."
                });
            }
        }

        // GET: Admin/ProductsAdmin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/ProductsAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

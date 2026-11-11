using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using MusicStore.Models;

namespace MusicStore.Areas.Admin.Controllers
{
    public class OrdersAdminController : Controller
    {
        private readonly MusicStoreDBModel db = new MusicStoreDBModel();

        public ActionResult Index(string status)
        {
            var orders = db.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.OrderStatus == status);
            }

            ViewBag.CurrentStatus = status;
            return View(orders.ToList());
        }

        public ActionResult Details(int id)
        {
            var order = db.Orders
                          .Include(o => o.User)
                          .Include(o => o.OrderDetails.Select(d => d.Product))
                          .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int orderId, string newStatus)
        {
            var order = db.Orders.Find(orderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            string[] validStatuses = { "Pending", "Shipping", "Delivered", "Cancelled" };
            if (validStatuses.Contains(newStatus))
            {
                order.OrderStatus = newStatus;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Order status updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid status value.";
            }

            return RedirectToAction("Details", new { id = orderId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
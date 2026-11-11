using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MusicStore.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        private MusicStoreDBModel db = new MusicStoreDBModel();

        public ActionResult Index()
        {
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCategories = db.Categories.Count();

            var orders = db.Orders.ToList();

            ViewBag.TotalRevenue = orders.Where(o => o.OrderStatus == "Delivered")
                                         .Sum(o => o.OrderTotalAmount ?? 0);

            ViewBag.PendingOrders = orders.Count(o => o.OrderStatus == "Pending");
            ViewBag.ShippingOrders = orders.Count(o => o.OrderStatus == "Shipping");
            ViewBag.DeliveredOrders = orders.Count(o => o.OrderStatus == "Delivered");
            ViewBag.CancelledOrders = orders.Count(o => o.OrderStatus == "Cancelled");

            var lowStockProducts = db.Products.Where(p => p.StockQuantity < 5).ToList();

            return View(lowStockProducts);
        }
    }
}
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MusicStore.Models;

namespace MusicStore.Controllers
{
    public class OrdersController : Controller
    {
        private MusicStoreDBModel db = new MusicStoreDBModel();

        // GET: Orders/Checkout
        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !db.CartItems.Any(c => c.CartId == cart.CartId))
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống, không thể thanh toán!";
                return RedirectToAction("Index", "ShoppingCarts");
            }

            var cartItems = db.CartItems.Include(c => c.Product).Where(c => c.CartId == cart.CartId).ToList();
            ViewBag.GrandTotal = cartItems.Sum(item => item.Quantity * (item.Product.UnitPrice));

            var user = db.Users.Find(userId);
            ViewBag.UserAddress = user != null ? user.Address : "";

            return View();
        }

        // POST: Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string shippingAddress)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Users");

            int userId = Convert.ToInt32(Session["UserId"]);
            var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);
            var cartItems = db.CartItems.Include(c => c.Product).Where(c => c.CartId == cart.CartId).ToList();

            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng trống.");
                return View();
            }

            decimal grandTotal = cartItems.Sum(item => item.Quantity * (item.Product.UnitPrice));

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = new Order
                    {
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        OrderStatus = "Pending",
                        OrderTotalAmount = grandTotal,
                        ShippingAddress = shippingAddress
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    foreach (var item in cartItems)
                    {
                        if (item.Product.StockQuantity < item.Quantity)
                        {
                            ModelState.AddModelError("", $"Sản phẩm '{item.Product.ProductName}' không đủ hàng trong kho (Còn lại: {item.Product.StockQuantity}).");
                            transaction.Rollback();
                            ViewBag.GrandTotal = grandTotal;
                            return View();
                        }

                        item.Product.StockQuantity -= item.Quantity;
                        db.Entry(item.Product).State = EntityState.Modified;

                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.ProductId,
                            OrderItemQuantity = item.Quantity,             
                            OrderItemUnitPrice = item.Product.UnitPrice    
                        };
                        db.OrderDetails.Add(orderDetail);
                    }

                    db.CartItems.RemoveRange(cartItems);
                    db.SaveChanges();

                    transaction.Commit();

                    Session["CartCount"] = 0;
                    return RedirectToAction("OrderSuccess");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Lỗi xử lý đặt hàng: " + ex.Message);
                    ViewBag.GrandTotal = grandTotal;
                    return View();
                }
            }
        }

        // GET: Orders/OrderSuccess
        public ActionResult OrderSuccess()
        {
            return View();
        }

        // GET: Orders/MyOrders
        public ActionResult MyOrders()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Users");

            int userId = Convert.ToInt32(Session["UserId"]);
            var orders = db.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // GET: Orders/MyOrderDetails/5
        public ActionResult MyOrderDetails(int? id)
        {
            if (id == null) return RedirectToAction("MyOrders");
            if (Session["UserId"] == null) return RedirectToAction("Login", "Users");

            int userId = Convert.ToInt32(Session["UserId"]);

            var order = db.Orders.FirstOrDefault(o => o.OrderId == id && o.UserId == userId);
            if (order == null) return HttpNotFound();

            var details = db.OrderDetails.Include(d => d.Product).Where(d => d.OrderId == id).ToList();
            ViewBag.OrderDetails = details;

            return View(order);
        }
    }
}
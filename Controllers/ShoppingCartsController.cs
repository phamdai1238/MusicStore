using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MusicStore.Models;

namespace MusicStore.Controllers
{
    public class ShoppingCartsController : Controller
    {
        private MusicStoreDBModel db = new MusicStoreDBModel();

        private int GetCurrentUserId()
        {
            if (Session["UserId"] != null)
            {
                return Convert.ToInt32(Session["UserId"]);
            }
            return 1;
        }

        private int GetOrCreateCartId(int userId)
        {
            var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                db.ShoppingCarts.Add(cart);
                db.SaveChanges();
            }
            return cart.CartId;
        }

        //Get cart
        public ActionResult Index()
        {
            int userId = GetCurrentUserId();
            int cartId = GetOrCreateCartId(userId);

            var cartItems = db.CartItems
                              .Include(c => c.Product)
                              .Where(c => c.CartId == cartId)
                              .ToList();

            decimal grandTotal = cartItems.Sum(item => item.Quantity * item.Product.UnitPrice);
            int totalCount = cartItems.Sum(item => item.Quantity);

            ViewBag.GrandTotal = grandTotal;
            ViewBag.TotalCount = totalCount;

            return View(cartItems);
        }

        //Add product to cart
        [HttpPost]
        public ActionResult AddToCart(int id, int qty = 1)
        {
            if (Session["UserId"] == null)
            {
                return Json(new
                {
                    success = false,
                    isNotLoggedIn = true,
                    message = "Vui lòng đăng nhập để thêm vào giỏ hàng!"
                });
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            int userId = GetCurrentUserId();
            int cartId = GetOrCreateCartId(userId);

            var existingItem = db.CartItems.FirstOrDefault(c => c.CartId == cartId && c.ProductId == id);

            if (existingItem != null)
            {
                existingItem.Quantity += qty;
                db.Entry(existingItem).State = EntityState.Modified;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = id,
                    Quantity = qty
                };
                db.CartItems.Add(newItem);
            }

            db.SaveChanges();

            int totalCount = db.CartItems.Where(c => c.CartId == cartId).Sum(c => (int?)c.Quantity) ?? 0;
            Session["CartCount"] = totalCount;
            return Json(new { success = true, cartCount = totalCount });
        }

        
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int qty)
        {
            int userId = GetCurrentUserId();
            int cartId = GetOrCreateCartId(userId);

            var item = db.CartItems.Include(c => c.Product).FirstOrDefault(c => c.CartId == cartId && c.ProductId == id);

            if (item != null)
            {
                if (qty > item.Product.StockQuantity)
                {
                    return Json(new { success = false, message = $"Chỉ còn tối đa {item.Product.StockQuantity} sản phẩm trong kho." });
                }

                if (qty <= 0)
                {
                    db.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity = qty;
                    db.Entry(item).State = EntityState.Modified;
                }
                db.SaveChanges();
            }

            decimal itemTotal = item != null ? (item.Quantity * item.Product.UnitPrice) : 0;

            var allItems = db.CartItems.Include(c => c.Product).Where(c => c.CartId == cartId).ToList();

            decimal grandTotal = allItems.Sum(x => x.Quantity * x.Product.UnitPrice);

            int totalCount = allItems.Sum(x => x.Quantity);

            Session["CartCount"] = totalCount;
            return Json(new
            {
                success = true,
                itemTotalPrice = string.Format("{0:N2}", itemTotal),
                grandTotal = string.Format("{0:N2}", grandTotal),
                cartCount = totalCount
            });
        }

        //Delete product from cart
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            int userId = GetCurrentUserId();
            int cartId = GetOrCreateCartId(userId);

            var item = db.CartItems.FirstOrDefault(c => c.CartId == cartId && c.ProductId == id);
            if (item != null)
            {
                db.CartItems.Remove(item);
                db.SaveChanges();
            }

            var allItems = db.CartItems.Include(c => c.Product).Where(c => c.CartId == cartId).ToList();
            decimal grandTotal = allItems.Sum(x => x.Quantity * x.Product.UnitPrice);
            int totalCount = allItems.Sum(x => x.Quantity);
            Session["CartCount"] = totalCount;
            return Json(new
            {
                success = true,
                grandTotal = string.Format("{0:N2}", grandTotal),
                cartCount = totalCount
            });
        }

        [HttpPost]
        public ActionResult RemoveAllFromCart()
        {
            try
            {
                int userId = GetCurrentUserId();
                int cartId = GetOrCreateCartId(userId);

                var allItems = db.CartItems.Where(c => c.CartId == cartId).ToList();

                if (allItems.Any())
                {
                    db.CartItems.RemoveRange(allItems);
                    db.SaveChanges();
                }

                Session["CartCount"] = 0;

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MusicStore.Models;

namespace MusicStore.Controllers
{
    public class UsersController : Controller
    {
        private MusicStoreDBModel db = new MusicStoreDBModel();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra xem tên tài khoản đã tồn tại chưa
                var isExist = db.Users.Any(u => u.UserName == model.UserName);
                if (isExist)
                {
                    ModelState.AddModelError("UserName", "Tên tài khoản này đã được sử dụng!");
                    return View(model);
                }

                // 2. Mặc định mọi tài khoản tự đăng ký đều là khách hàng (Customer)
                model.Role = "Customer";

                // 3. Lưu vào cơ sở dữ liệu
                db.Users.Add(model);
                db.SaveChanges();

                // Lưu thông báo thành công và chuyển hướng đến trang Đăng nhập
                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Hãy đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // ==========================================
        // KHU VỰC ĐĂNG NHẬP (LOGIN)
        // ==========================================

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string UserName, string password)
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                return View();
            }

            // Tìm kiếm tài khoản trùng khớp trong Database
            var user = db.Users.FirstOrDefault(u => u.UserName == UserName && u.Password == password);

            if (user != null)
            {
                // KHỞI TẠO SESSION KHI ĐĂNG NHẬP THÀNH CÔNG
                Session["UserId"] = user.UserId;
                Session["UserName"] = user.UserName;
                Session["FullName"] = user.FullName;
                Session["Role"] = user.Role; // Lưu vai trò ("Admin" hoặc "Customer")

                // Phân tuyến đường đi dựa vào vai trò
                if (user.Role == "Admin")
                {
                    // Nếu là Admin, dẫn thẳng vào trang Dashboard của vùng Admin
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else
                {
                    // Nếu là khách hàng, quay lại trang chủ mua sắm
                    return RedirectToAction("Index", "Home");
                }
            }

            // Nếu sai tài khoản/mật khẩu
            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }

        // ==========================================
        // KHU VỰC ĐĂNG XUẤT (LOGOUT)
        // ==========================================

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa sạch toàn bộ Session đang lưu trữ
            return RedirectToAction("Index", "Home");
        }

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,UserName,FullName,EmailAddress,Password,PhoneNumber,Address,Role,CreatedDate")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,UserName,FullName,EmailAddress,Password,PhoneNumber,Address,Role,CreatedDate")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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

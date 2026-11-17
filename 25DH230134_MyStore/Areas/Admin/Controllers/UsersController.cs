using _25DH230134_MyStore.Areas.Admin.Filters;
using _25DH230134_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace _25DH230134_MyStore.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class UsersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        private const string RoleAdmin = "A";
        private const int PageSize = 10;

        //Tìm 1 tài khoản Admin theo Username
        private User FindAdmin(string id)
        {
            return db.Users.SingleOrDefault(u => u.Username == id && u.UserRole == RoleAdmin);
        }

        // GET: Admin/Users - Danh sách tài khoản Admin + tìm kiếm + phân trang
        public ActionResult Index(string searchTerm, int? page)
        {
            var users = db.Users.Where(u => u.UserRole == RoleAdmin);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = users.Where(u => u.Username.Contains(searchTerm));
            }
            ViewBag.SearchTerm = searchTerm;
            return View(users.OrderBy(u => u.Username).ToPagedList(page ?? 1, PageSize));
        }

        // GET: Admin/Users/Details/admin
        public ActionResult Details(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = FindAdmin(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password")] User user, string confirmPassword)
        {
            user.UserRole = RoleAdmin;          //Tài khoản tạo ở đây luôn là Admin
            ModelState.Remove("UserRole");      //Form không có ô UserRole nên bỏ kiểm tra trường này

            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Mật khẩu nhập lại không khớp");
            }
            if (db.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
            }

            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã tạo tài khoản Admin \"" + user.Username + "\".";
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/Users/Edit/admin - Đổi mật khẩu
        public ActionResult Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = FindAdmin(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        // POST: Admin/Users/Edit/admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, string newPassword, string confirmPassword)
        {
            var user = FindAdmin(id);
            if (user == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu mới tối thiểu 6 ký tự");
            }
            else if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu nhập lại không khớp");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }
            user.Password = newPassword;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã đổi mật khẩu cho \"" + user.Username + "\".";
            return RedirectToAction("Index");
        }

        // GET: Admin/Users/Delete/admin
        public ActionResult Delete(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = FindAdmin(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        // POST: Admin/Users/Delete/admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var user = FindAdmin(id);
            if (user == null) return HttpNotFound();

            //Không cho tự xóa chính mình, và phải còn ít nhất 1 Admin
            if (user.Username == (Session["AdminUser"] as string))
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }
            if (db.Users.Count(u => u.UserRole == RoleAdmin) <= 1)
            {
                TempData["ErrorMessage"] = "Phải còn ít nhất 1 tài khoản Admin.";
                return RedirectToAction("Index");
            }

            db.Users.Remove(user);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã xóa tài khoản \"" + id + "\".";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
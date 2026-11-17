using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH230134_MyStore.Models;
using _25DH230134_MyStore.Models.ViewModel;

namespace _25DH230134_MyStore.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Account/Login
        public ActionResult Login(string returnUrl)
        {
            if (Session["AdminUser"] != null)
            {
                return RedirectToAction("Index", "HomeAdmin");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Admin/Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //Chỉ cho tài khoản có quyền Admin (UserRole = "A") đăng nhập
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                                                      && u.Password == model.Password
                                                      && u.UserRole == "A");
                if (user != null)
                {
                    Session["AdminUser"] = user.Username;
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "HomeAdmin");
                }
                ModelState.AddModelError("", "Sai tên đăng nhập, mật khẩu hoặc tài khoản không có quyền Admin.");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: Admin/Account/Logout
        public ActionResult Logout()
        {
            Session.Remove("AdminUser");
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
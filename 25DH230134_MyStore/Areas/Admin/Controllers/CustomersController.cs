using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using _25DH230134_MyStore.Areas.Admin.Filters;
using _25DH230134_MyStore.Models;
using _25DH230134_MyStore.Models.ViewModel;
using PagedList;

namespace _25DH230134_MyStore.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        private const int PageSize = 10;

        //Dropdown chọn tài khoản: chỉ lấy tài khoản khách hàng (role C), hiển thị Username
        private SelectList UserSelectList(string selected = null)
        {
            return new SelectList(db.Users.Where(u => u.UserRole == "C"), "Username", "Username", selected);
        }

        // GET: Admin/Customers - Danh sách + tìm kiếm + sắp xếp + phân trang
        public ActionResult Index(string searchTerm, string sortOrder, int? page)
        {
            var customers = db.Customers.Include(c => c.Orders).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                customers = customers.Where(c => c.CustomerName.Contains(searchTerm)
                                              || c.CustomerPhone.Contains(searchTerm)
                                              || c.CustomerEmail.Contains(searchTerm)
                                              || c.Username.Contains(searchTerm));
            }

            switch (sortOrder)
            {
                case "name_desc": customers = customers.OrderByDescending(c => c.CustomerName); break;
                case "orders_desc": customers = customers.OrderByDescending(c => c.Orders.Count()); break;
                case "newest": customers = customers.OrderByDescending(c => c.CustomerID); break;
                default: customers = customers.OrderBy(c => c.CustomerName); break;
            }

            var model = new CustomerSearchVM
            {
                SearchTerm = searchTerm,
                SortOrder = sortOrder,
                Customers = customers.ToPagedList(page ?? 1, PageSize)
            };
            return View(model);
        }

        // GET: Admin/Customers/Details/5 - Thông tin khách hàng + các đơn hàng của khách
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Include(c => c.Orders).SingleOrDefault(c => c.CustomerID == id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Admin/Customers/Create
        public ActionResult Create()
        {
            ViewBag.Username = UserSelectList();
            return View();
        }

        // POST: Admin/Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Username = UserSelectList(customer.Username);
            return View(customer);
        }

        // GET: Admin/Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.Username = UserSelectList(customer.Username);
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Username = UserSelectList(customer.Username);
            return View(customer);
        }

        // GET: Admin/Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Include(c => c.Orders).SingleOrDefault(c => c.CustomerID == id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            //Khách đã có đơn hàng thì không cho xóa (tránh mất lịch sử đơn + lỗi khóa ngoại)
            if (db.Orders.Any(o => o.CustomerID == id))
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng \"" + customer.CustomerName + "\" vì đã có đơn hàng.";
                return RedirectToAction("Index");
            }

            User user = db.Users.Find(customer.Username);
            db.Customers.Remove(customer);   //Xóa bảng con trước
            if (user != null)
            {
                db.Users.Remove(user);       //Rồi xóa tài khoản đăng nhập
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã xóa khách hàng \"" + customer.CustomerName + "\".";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
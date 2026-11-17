using System;
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
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        private const int PageSize = 15; //15 đơn hàng mỗi trang (theo đề)

        // GET: Admin/Orders - Danh sách đơn hàng + tìm kiếm + phân trang
        public ActionResult Index(string customerName, string paymentStatus, DateTime? orderDate, int? customerId, int? page)
        {
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();
            var model = new OrderSearchVM
            {
                CustomerName = customerName,
                PaymentStatus = paymentStatus,
                OrderDate = orderDate,
                CustomerID = customerId
            };

            //Xem đơn hàng của 1 khách hàng cụ thể
            if (customerId.HasValue)
            {
                orders = orders.Where(o => o.CustomerID == customerId.Value);
                model.CustomerFilterName = db.Customers.Find(customerId.Value)?.CustomerName;
            }
            //Tìm theo tên khách hàng
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                orders = orders.Where(o => o.Customer.CustomerName.Contains(customerName));
            }
            //Tìm theo trạng thái thanh toán
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentStatus == paymentStatus);
            }
            //Tìm theo ngày đặt hàng (cả ngày đó, từ 00:00 đến 23:59)
            if (orderDate.HasValue)
            {
                DateTime from = orderDate.Value.Date;
                DateTime to = from.AddDays(1);
                orders = orders.Where(o => o.OrderDate >= from && o.OrderDate < to);
            }

            model.PaymentStatuses = db.Orders.Select(o => o.PaymentStatus).Distinct().OrderBy(s => s).ToList();
            model.Orders = orders.OrderByDescending(o => o.OrderDate).ToPagedList(page ?? 1, PageSize);
            return View(model);
        }

        // GET: Admin/Orders/DetailedOrder/5 - Chi tiết đơn hàng
        public ActionResult DetailedOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Include(o => o.Customer)
                                 .Include("OrderDetails.Product")
                                 .SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Admin/Orders/UpdateStatus/5 - Trang cập nhật trạng thái
        public ActionResult UpdateStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Include(o => o.Customer).SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string orderStatus, string paymentStatus)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            //Chỉ nhận các giá trị nằm trong danh sách cho phép
            if (!OrderSearchVM.OrderStatusOptions.Contains(orderStatus) || !OrderSearchVM.PaymentStatusOptions.Contains(paymentStatus))
            {
                ModelState.AddModelError("", "Trạng thái không hợp lệ.");
                return View(db.Orders.Include(o => o.Customer).Single(o => o.OrderID == id));
            }

            order.OrderStatus = orderStatus;
            order.PaymentStatus = paymentStatus;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã cập nhật trạng thái đơn hàng #" + id + ".";
            return RedirectToAction("DetailedOrder", new { id });
        }

        // GET: Admin/Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Include(o => o.Customer).SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            if (order != null)
            {
                //Xóa chi tiết đơn trước để tránh lỗi khóa ngoại
                var orderDetails = db.OrderDetails.Where(od => od.OrderID == id).ToList();
                db.OrderDetails.RemoveRange(orderDetails);
                db.Orders.Remove(order);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
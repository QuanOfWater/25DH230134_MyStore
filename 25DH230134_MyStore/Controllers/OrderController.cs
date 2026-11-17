using _25DH230134_MyStore.Models;
using _25DH230134_MyStore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH230134_MyStore.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Lấy các sản phẩm được tick chọn trong giỏ (không truyền selectedIds = lấy cả giỏ)
        private List<CartItem> GetSelectedItems(int[] selectedIds)
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null) return new List<CartItem>();
            var items = cart.Items.ToList();
            if (selectedIds != null && selectedIds.Length > 0)
            {
                items = items.Where(i => selectedIds.Contains(i.ProductID)).ToList();
            }
            return items;
        }

        //GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout(int[] selectedIds)
        {
            var items = GetSelectedItems(selectedIds);
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống, vui lòng thêm sản phẩm trước khi đặt hàng!";
                return RedirectToAction("Index", "Home");
            }

            //Lấy thông tin khách hàng đang đăng nhập
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) { return RedirectToAction("Login", "Account"); }

            var model = new CheckoutVM
            {
                CartItems = items,
                TotalAmount = items.Sum(i => i.TotalPrice),
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                ShippingAddress = customer.CustomerAddress, //Địa chỉ mặc định lấy từ bảng Customer
                DeliveryMethod = "Giao trong ngày",
                PaymentMethod = "Tiền mặt"
            };
            return View(model);
        }

        //POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model, int[] selectedIds)
        {
            var items = GetSelectedItems(selectedIds);
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống, vui lòng thêm sản phẩm trước khi đặt hàng!";
                return RedirectToAction("Index", "Home");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) { return RedirectToAction("Login", "Account"); }

            if (!ModelState.IsValid)
            {
                //Form bị lỗi: nạp lại dữ liệu hiển thị rồi trả về form
                model.CartItems = items;
                model.TotalAmount = items.Sum(i => i.TotalPrice);
                model.CustomerName = customer.CustomerName;
                model.CustomerPhone = customer.CustomerPhone;
                return View(model);
            }

            //Trạng thái thanh toán dựa trên phương thức thanh toán
            //Trạng thái thanh toán dựa trên phương thức thanh toán
            string paymentStatus;
            switch (model.PaymentMethod)
            {
                case "Tiền mặt": paymentStatus = "Thanh toán khi nhận hàng"; break;
                case "Chuyển khoản": paymentStatus = "Chờ chuyển khoản"; break;
                case "Ví điện tử": paymentStatus = "Chờ thanh toán qua ví"; break;
                default: paymentStatus = "Chưa thanh toán"; break;
            }

            //Trạng thái đơn hàng: trả tiền mặt thì xử lý luôn, còn lại thì chờ thanh toán
            string orderStatus = model.PaymentMethod == "Tiền mặt" ? "Đang xử lý" : "Chờ thanh toán";

            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = items.Sum(i => i.TotalPrice),
                PaymentStatus = paymentStatus,
                PaymentMethod = model.PaymentMethod,
                DeliveryMethod = model.DeliveryMethod,
                ShippingAddress = model.ShippingAddress,
                OrderStatus = orderStatus,
                //TotalPrice của OrderDetail do SQL tự tính (Quantity * UnitPrice)
                OrderDetails = items.Select(i => new OrderDetail
                {
                    ProductID = i.ProductID,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
            db.Orders.Add(order);
            db.SaveChanges();

            //Chỉ xóa khỏi giỏ những sản phẩm đã mua, sản phẩm không tick vẫn giữ lại
            var cart = Session["Cart"] as Cart;
            if (cart != null)
            {
                foreach (var i in items)
                {
                    cart.RemoveItem(i.ProductID);
                }
                if (!cart.Items.Any()) Session["Cart"] = null;
            }

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        //GET: Order/OrderSuccess/5 - Xác nhận đơn hàng
        [Authorize]
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
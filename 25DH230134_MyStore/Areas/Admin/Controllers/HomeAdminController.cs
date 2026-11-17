using System;
using System.Linq;
using System.Web.Mvc;
using _25DH230134_MyStore.Areas.Admin.Filters;
using _25DH230134_MyStore.Models;
using _25DH230134_MyStore.Models.ViewModel;

namespace _25DH230134_MyStore.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeAdminController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/HomeAdmin - Trang thống kê
        public ActionResult Index(int? year)
        {
            int y = year ?? DateTime.Now.Year;
            //Đơn đã hủy không tính vào doanh thu
            var validOrders = db.Orders.Where(o => o.OrderStatus == null || o.OrderStatus != "Đã hủy");

            var model = new DashboardVM
            {
                Year = y,
                TotalOrders = db.Orders.Count(),
                TotalRevenue = validOrders.Select(o => (decimal?)o.TotalAmount).Sum() ?? 0,
                TotalCustomers = db.Customers.Count(),
                TotalProducts = db.Products.Count()
            };

            //1. Doanh thu 12 tháng của năm y (tháng không có đơn = 0)
            var byMonth = validOrders.Where(o => o.OrderDate.Year == y)
                                     .GroupBy(o => o.OrderDate.Month)
                                     .Select(g => new { Month = g.Key, Total = g.Sum(o => o.TotalAmount) })
                                     .ToList();
            for (int m = 1; m <= 12; m++)
            {
                model.RevenueByMonth.Add(new DashboardVM.ChartItem
                {
                    Label = "T" + m,
                    Value = byMonth.Where(x => x.Month == m).Select(x => x.Total).FirstOrDefault()
                });
            }

            //2. Số đơn theo trạng thái đơn hàng
            model.OrdersByStatus = db.Orders
                .GroupBy(o => o.OrderStatus ?? "Chưa có trạng thái")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList()
                .Select(x => new DashboardVM.ChartItem { Label = x.Status, Value = x.Count })
                .ToList();

            //3. Top 5 sản phẩm bán chạy (theo tổng số lượng)
            model.TopProducts = db.OrderDetails
                .GroupBy(d => d.Product.ProductName)
                .Select(g => new { Name = g.Key, Qty = g.Sum(d => d.Quantity) })
                .OrderByDescending(x => x.Qty)
                .Take(5)
                .ToList()
                .Select(x => new DashboardVM.ChartItem { Label = x.Name, Value = x.Qty })
                .ToList();

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
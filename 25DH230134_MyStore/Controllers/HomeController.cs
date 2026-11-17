using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH230134_MyStore.Models;
using _25DH230134_MyStore.Models.ViewModel;
using PagedList;

namespace _25DH230134_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        [ChildActionOnly]
        public ActionResult _CategoriesPV()
        {
            // Lưu ý: Đảm bảo biến "db" là tên DbContext thực tế của bạn
            var categories = db.Categories.ToList();
            return PartialView("_CategoriesPV", categories);
        }

        [ChildActionOnly]
        public ActionResult PersonalMenu()
        {
            var cart = Session["Cart"] as Cart;
            var model = new PersonalMenuVM
            {
                IsLoggedIn = Session["Username"] != null,
                Username = Session["Username"] as string,
                CartCount = cart != null ? cart.TotalQuantity() : 0
            };
            return PartialView("_PVPersonalMenu", model);
        }

        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();
            //Tìm kiếm sản phẩm dựa trên từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm)
                || p.ProductDescription.Contains(searchTerm) || p.Category.CategoryName.Contains(searchTerm)
                );
            }
            //Đoạn code liên quan tới phân trang
            int pageNumber = page ?? 1; //Nếu page = null thì gán giá trị mặc định là 1
            int pageSize = 6; //Số lượng sản phẩm trên mỗi trang
            //Lấy 10 sản phẩm bán chạy nhất
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(10).ToList();
            //Lấy 10 sản phẩm bán chậm nhất và phân trang
            model.NewProducts = products.OrderBy(p => p.OrderDetails.Count()).Take(20).ToPagedList(pageNumber, pageSize);
            return View(model);
        }

        // Danh sách sản phẩm theo danh mục (bấm từ menu Danh mục)
        public ActionResult ProductList(int? categoryId, string sortOrder, int? page)
        {
            var products = db.Products.AsQueryable();
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
                ViewBag.CategoryName = db.Categories.Find(categoryId.Value)?.CategoryName;
            }

            switch (sortOrder)
            {
                case "name_desc": products = products.OrderByDescending(p => p.ProductName); break;
                case "price_asc": products = products.OrderBy(p => p.ProductPrice); break;
                case "price_desc": products = products.OrderByDescending(p => p.ProductPrice); break;
                default: products = products.OrderBy(p => p.ProductName); break;
            }

            ViewBag.CategoryId = categoryId;
            ViewBag.SortOrder = sortOrder;

            int pageNumber = page ?? 1;
            int pageSize = 8;
            return View(products.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product pro = db.Products.Find(id);//Sản phẩm đang xem chi tiết
            if (pro == null)
            {
                return HttpNotFound();
            }
            //Lấy tất cả các sản phẩm cùng danh mục
            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();

            ProductDetailsVM model = new ProductDetailsVM();

            //Đoạn code liên quan tới phân trang
            //Lấy số trang hiện tại(mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = model.PageSize; // Số sản phẩm mỗi trang
            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductName).Take(8).ToPagedList(pageNumber, pageSize);
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(8).ToPagedList(pageNumber, pageSize);
            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }

            return View(model);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
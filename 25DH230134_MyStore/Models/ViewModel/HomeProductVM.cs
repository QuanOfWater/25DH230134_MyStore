using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;

namespace _25DH230134_MyStore.Models.ViewModel
{
    public class HomeProductVM
    {
        //Tiêu chí tìm kiếm
        public string SearchTerm { get; set; }

        //Các thuộc tính hỗ trợ paging
        public int PageNumber { get; set; } //Vị trí trang hiện tại
        public int PageSize { get; set; } = 10; //Mỗi trang có bao nhiêu bản ghi

        //Danh sách sản phẩm nổi bật
        public List<Product> FeaturedProducts { get; set; }

        //Danh sách sản phẩm mới đã phân trang
        public PagedList.IPagedList<Product> NewProducts { get; set; }

    }
}
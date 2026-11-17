using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH230134_MyStore.Models.ViewModel
{
    public class CustomerSearchVM
    {
        public string SearchTerm { get; set; }   //Tìm theo tên, SĐT, email, tên đăng nhập
        public string SortOrder { get; set; }
        public IPagedList<Customer> Customers { get; set; }
    }
}
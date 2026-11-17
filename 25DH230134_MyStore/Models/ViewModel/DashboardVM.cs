using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH230134_MyStore.Models.ViewModel
{
    public class DashboardVM
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int Year { get; set; }

        public List<ChartItem> RevenueByMonth { get; set; } = new List<ChartItem>();
        public List<ChartItem> OrdersByStatus { get; set; } = new List<ChartItem>();
        public List<ChartItem> TopProducts { get; set; } = new List<ChartItem>();

        //Một cột / một miếng trên biểu đồ
        public class ChartItem
        {
            public string Label { get; set; }
            public decimal Value { get; set; }
        }
    }
}
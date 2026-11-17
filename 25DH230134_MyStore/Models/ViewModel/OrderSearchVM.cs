using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace _25DH230134_MyStore.Models.ViewModel
{
    public class OrderSearchVM
    {
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        [DataType(DataType.Date)]
        public DateTime? OrderDate { get; set; }

        //Dùng khi xem đơn hàng của 1 khách hàng cụ thể (từ trang Khách hàng)
        public int? CustomerID { get; set; }
        public string CustomerFilterName { get; set; }

        //Các trạng thái đơn hàng Admin được quyền chỉnh sửa
        public static readonly string[] OrderStatusOptions = { "Chờ thanh toán", "Đang xử lý", "Đang vận chuyển", "Đã giao", "Đã hủy" };

        //Danh sách trạng thái thanh toán đang có trong DB (đổ vào dropdown)
        public List<string> PaymentStatuses { get; set; } = new List<string>();

        //Kết quả đã phân trang
        public IPagedList<Order> Orders { get; set; }

        //Các trạng thái thanh toán Admin được chọn khi cập nhật
        public static readonly string[] PaymentStatusOptions =
        {
            "Thanh toán khi nhận hàng", "Chờ chuyển khoản", "Chờ thanh toán qua ví",
            "Đã thanh toán", "Chưa thanh toán", "Đã hoàn tiền"
        };
    }
}
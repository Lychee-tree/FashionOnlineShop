using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6.Models
{
    [Table("Order")]
    public class OrderModel
    {
        [Key]
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public int? DiscountID { get; set; }
        public int TotalPrice { get; set; }
        public int ShippingStatusID { get; set; }
        public int PaidStatusID { get; set; }
        public int EmployeeID { get; set; }
        public string ShippingCode { get; set; }
        public string Note { get; set; }
        public DateTime? OrderDate { get; set; }

        // Thêm thuộc tính này để hiển thị trạng thái đơn hàng
        public string OrderStatus { get; set; }

        // Navigation property cho danh sách chi tiết đơn hàng (sửa ở đây)
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // Navigation properties cho các bảng liên quan
        public virtual Customer Customer { get; set; }
        public virtual PaidStatu PaidStatu { get; set; }

        public string ShippingStatus { get; set; }
    }

    [Table("OrderDetailKJ")]
    public class OrderDetailKJ
    {
        [Key]
        public int OrderDetailID { get; set; }

        [ForeignKey("Order")]
        public int OrderID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }

        public int Quantity { get; set; }

        public int UnitPrice { get; set; }

        // Navigation properties 
        public virtual OrderModel Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
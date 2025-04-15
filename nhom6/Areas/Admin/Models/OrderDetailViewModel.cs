using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nhom6.Areas.Admin.Models
{
    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public int TotalPrice { get; set; }
        public int DiscountValue { get; set; }
        public List<OrderProductDetail> Products { get; set; }
    }

    public class OrderProductDetail
    {
        public string ProductName { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public Nullable <int> Quantity { get; set; }
        public int TotalPrice { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nhom6.Models
{
    public class CheckOutModel
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public int TotalPrice { get; set; }

        public List<CheckoutItem> OrderDetails { get; set; }
    }

    public class CheckoutItem
    {
        public string ProductName { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> TotalUnitPrice { get; set; }
    }
}
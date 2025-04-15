using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nhom6.Models
{
    public class CartItemModel
    {
        public int pvID { get; set; }
        public int orderID { get; set; }
        public string ProductName { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
        public Nullable<int> Quantity { get; set; }
        public int UnitPrice { get; set; }
        public Nullable<int> TotalUnitPrice { get; set; }
        public string ProductImage { get; set; }
    }
}
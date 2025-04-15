using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nhom6.Areas.Admin.Models
{
    public class ProductVariantsModel
    {
        public string ProductName {  get; set; }
        public string UnitDescription { get; set; }
        public int UnitPrice { get; set; }
        public string productDescription { get; set; }
        public int ColorID { get; set; }
        public int SizeID { get; set; }
        public Nullable<int> Instock { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
        public int Instock1 { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nhom6.Areas.Admin.Models
{
	public class ProductVariantsModel
	{
        public int ColorID { get; set; }
        public int SizeID { get; set; }
        public Nullable<int> Instock { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nhom6.Areas.Admin.Models
{
    public class ColorSizeInstockViewModel
    {
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public string SizeName { get; set; }
        public int Instock { get; set; }
    }
}
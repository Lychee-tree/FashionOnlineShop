using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace nhom6.Models
{
    public class csdl : DbContext
    {
        public csdl() : base("csdl") { } // Đảm bảo khớp với tên trong Web.config
        public DbSet <Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace nhom6
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Discount
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Discount()
        {
            this.Orders = new HashSet<Order>();
        }
    
        public int DiscountID { get; set; }

        [Display(Name = "Mã khuyến mãi")]
        public string DiscountCode { get; set; }
        [Display(Name = "Giá trị khuyến mãi")]
        public int DiscountValue { get; set; }
        [Display(Name = "Ngày phát hành")]
        public System.DateTime ValidityDate { get; set; }
        [Display(Name = "Mã hết hiệu lực")]
        public System.DateTime ExpiryDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Orders { get; set; }
    }
}

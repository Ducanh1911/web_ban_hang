//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BaiTap.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Coupon
    {
        public int couponId { get; set; }
        public string code { get; set; }
        public string discountType { get; set; }
        public decimal discountValue { get; set; }
        public Nullable<decimal> minOrderAmount { get; set; }
        public Nullable<decimal> maxDiscount { get; set; }
        public System.DateTime startDate { get; set; }
        public System.DateTime endDate { get; set; }
        public Nullable<int> usageLimit { get; set; }
        public Nullable<int> timesUsed { get; set; }
    }
}

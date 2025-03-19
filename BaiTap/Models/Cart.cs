
namespace BaiTap.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Cart
    {
        public int cartId { get; set; }
        public Nullable<int> userId { get; set; }
        public int productId { get; set; }
        public int quantity { get; set; }
        public Nullable<System.DateTime> createdAt { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
    }
}

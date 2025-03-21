

namespace BaiTap.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.Discounts = new HashSet<Discount>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.Reviews = new HashSet<Review>();
            this.Carts = new HashSet<Cart>();
        }
    
        public int productId { get; set; }
        public string productName { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public Nullable<decimal> discount { get; set; }
        public int stock { get; set; }
        public Nullable<int> categoryId { get; set; }
        public string imageUrl { get; set; }
        public Nullable<System.DateTime> createdAt { get; set; }
    
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Discount> Discounts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Review> Reviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cart> Carts { get; set; }
    }
}

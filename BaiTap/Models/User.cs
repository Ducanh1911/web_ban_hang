
namespace BaiTap.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.Orders = new HashSet<Order>();
            this.Reviews = new HashSet<Review>();
            this.Carts = new HashSet<Cart>();
        }
    
        public int userId { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string passwordHash { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
        public string role { get; set; }
        public Nullable<System.DateTime> createdAt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Orders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Review> Reviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cart> Carts { get; set; }
    }
}

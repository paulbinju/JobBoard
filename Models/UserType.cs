//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jobs4Bahrainis.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserType()
        {
            this.Users = new HashSet<User>();
        }
    
        public int ust_Id { get; set; }
        public System.Guid ust_Guid { get; set; }
        public System.DateTime ust_Created { get; set; }
        public System.DateTime ust_LastUpdated { get; set; }
        public Nullable<System.DateTime> ust_Deleted { get; set; }
        public string ust_Name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> Users { get; set; }
    }
}

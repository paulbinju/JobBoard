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
    
    public partial class CollaborationType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CollaborationType()
        {
            this.BrandCollaborations = new HashSet<BrandCollaboration>();
        }
    
        public int colt_Id { get; set; }
        public System.Guid colt_Guid { get; set; }
        public System.DateTime colt_Created { get; set; }
        public System.DateTime colt_LastUpdated { get; set; }
        public string colt_Name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandCollaboration> BrandCollaborations { get; set; }
    }
}
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
    
    public partial class Order
    {
        public int ord_id { get; set; }
        public System.Guid ord_GUID { get; set; }
        public System.DateTime ord_Created { get; set; }
        public int ord_co_CompanyId { get; set; }
        public int ord_ct_ContactId { get; set; }
        public double ord_BalanceBefore { get; set; }
        public double ord_BalanceToDeduct { get; set; }
        public double ord_CardTotal { get; set; }
        public string ord_ValidationKey { get; set; }
        public Nullable<System.DateTime> ord_Processed { get; set; }
        public string ord_Response { get; set; }
    }
}

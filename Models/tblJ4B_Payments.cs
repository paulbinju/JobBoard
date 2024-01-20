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
    
    public partial class tblJ4B_Payments
    {
        public int ID { get; set; }
        public string InvoiceNumber { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string company { get; set; }
        public string AddrFlatApartment { get; set; }
        public string AddrHouseBuilding { get; set; }
        public string AddrRoad { get; set; }
        public string AddrBlock { get; set; }
        public string AddrCityTown { get; set; }
        public string AddrCountry { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public int Method { get; set; }
        public Nullable<int> CardType { get; set; }
        public string CardNo { get; set; }
        public string NameOnCard { get; set; }
        public string ExpiryDate { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<bool> Processed { get; set; }
        public Nullable<System.DateTime> ProcessedOn { get; set; }
    
        public virtual tblJ4B_Cards_Types tblJ4B_Cards_Types { get; set; }
        public virtual tblJ4B_Payments_Methods tblJ4B_Payments_Methods { get; set; }
    }
}
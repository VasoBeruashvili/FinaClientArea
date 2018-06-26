//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FinaClientArea
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductsFlow
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public string product_tree_path { get; set; }
        public int general_id { get; set; }
        public Nullable<double> amount { get; set; }
        public Nullable<double> price { get; set; }
        public Nullable<int> store_id { get; set; }
        public Nullable<decimal> vat_percent { get; set; }
        public Nullable<double> self_cost { get; set; }
        public Nullable<int> coeff { get; set; }
        public Nullable<byte> is_order { get; set; }
        public Nullable<byte> is_expense { get; set; }
        public Nullable<byte> is_move { get; set; }
        public Nullable<byte> visible { get; set; }
        public Nullable<int> parent_product_id { get; set; }
        public Nullable<int> ref_id { get; set; }
        public Nullable<int> unit_id { get; set; }
        public string comment { get; set; }
        public Nullable<double> discount_percent { get; set; }
        public Nullable<double> discount_value { get; set; }
        public Nullable<double> original_price { get; set; }
        public Nullable<int> in_id { get; set; }
        public Nullable<int> vendor_id { get; set; }
        public Nullable<byte> cafe_status { get; set; }
        public Nullable<int> out_id { get; set; }
        public Nullable<int> sub_id { get; set; }
        public Nullable<int> service_product_id { get; set; }
        public System.Guid uid { get; set; }
        public Nullable<int> service_staff_id { get; set; }
        public Nullable<double> staff_salary { get; set; }
        public Nullable<double> product_bonus { get; set; }
        public string cafe_comment { get; set; }
        public Nullable<System.DateTime> cafe_send_date { get; set; }
        public string ref_uid { get; set; }
        public double excise { get; set; }
    
        public virtual Products Products { get; set; }
        public virtual GeneralDocs GeneralDocs { get; set; }
    }
}

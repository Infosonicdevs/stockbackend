using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class STOCK_DETAILS
    {
        public int Stock_id { get; set; }
        public int Vibhag_id { get; set; }
        public int Stock_no { get; set; }
        public string Barcode { get; set; }
        public string Stock_name { get; set; }
        public int HSN_no { get; set; }
        public int Group_id { get; set; }
        public int Subgroup_id { get; set; }
        public decimal Weight { get; set; }
        public int Unit_id { get; set; }
        public char Include_GST { get; set; }
        public int Slab_id { get; set; }
        public decimal SGST_per { get; set; }
        public decimal CGST_per { get; set; }
        public decimal IGST_per { get; set; }
        public char Is_offer { get; set; }
        public int Offer_qty { get; set; }
        public char Is_stock { get; set; }
        public int Pur_slab_id { get; set; }
        public decimal Pur_SGST_per { get; set; }
        public decimal Pur_CGST_per { get; set; }
        public decimal Pur_IGST_per { get; set; }
        public char B_G { get; set; }
        public decimal MRP {  get; set; }
        public decimal Discount { get; set; }
        public decimal Rate { get; set; }
        public DateTime Change_date { get; set; }
        public string On_form { get; set; }
        public string Sale_Heading { get; set; }
        public string Purchase_Heading { get; set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }
    }
}
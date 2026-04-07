using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class PURCHASE_DETAILS
    {
        public int Details_id { get; set; }
        public int Invoice_id { get; set; }
        public int Stock_id { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal CGST_amt { get; set; }
        public decimal SGST_amt { get; set; }
        public decimal IGST_amt { get; set; }
        public decimal Disc_amt { get; set; }
        public decimal Total {  get; set; }
        public char Mode { get; set; }
        public decimal MRP { get; set; }
        public decimal Amount { get; set; }
    }
}
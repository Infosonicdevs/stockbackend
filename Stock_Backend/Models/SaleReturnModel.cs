using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class SaleReturnModel
    {
        public DateTime Return_date { get; set; }
        public int Sale_id { get; set; }

        public decimal Total_quantity { get; set; }
        public decimal Total_Disc { get; set; }
        public decimal Total_amt { get; set; }
        public decimal Round_off { get; set; }
        public int Roundoff_id { get; set; }
        public decimal Bill_amt { get; set; }

        public int Sale_l_id { get; set; }

        public decimal Total_CGST { get; set; }
        public decimal Total_SGST { get; set; }
        public decimal Total_IGST { get; set; }
        public decimal Total_Taxable { get; set; }

        public string User { get; set; }

        // TRANS
        public int Year_id { get; set; }
        public int Trans_type_id { get; set; }
        public char trans_code { get; set; }

        // TRANS DETAILS
        public char CashTrans { get; set; }
        public int Vibhag_id { get; set; }
        public char Status { get; set; }
        public int L_id { get; set; }
        public int Cust_id { get; set; }
        public string Card_no { get; set; }
        public string Narr { get; set; }

        public int CGST_id { get; set; }
        public int SGST_id { get; set; }
        public int IGST_id { get; set; }

        public int? Dr_point { get; set; }
        public int? Bal { get; set; }
        public decimal? Point_amt { get; set; }

        // UPDATE
        public int? update_return_id { get; set; }
        public int? update_trans_id { get; set; }
        public string Modify_reason { get; set; }

        public List<SaleReturnDetails> DETAILS { get; set; }
    }

    public class SaleReturnDetails
    {
        public int Stock_id { get; set; }
        public decimal Quantity { get; set; }
        public decimal MRP { get; set; }
        public decimal Disc { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Taxable_amt { get; set; }

        public decimal CGST_per { get; set; }
        public decimal SGST_per { get; set; }
        public decimal IGST_per { get; set; }

        public decimal CGST_amt { get; set; }
        public decimal SGST_amt { get; set; }
        public decimal IGST_amt { get; set; }

        public int Mode { get; set; }
    }
}
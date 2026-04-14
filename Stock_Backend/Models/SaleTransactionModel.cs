using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class SaleTransactionModel
    {
        //  SALE
        public int Sale_id { get; set; }
        public int Trans_id { get; set; }
        public int Outlet_id { get; set; }
        public DateTime Sale_date { get; set; }
        public int Counter_id { get; set; }
        public int Pavati_no { get; set; }
        public int Emp_id { get; set; }
        public int State_id { get; set; }
        public string Card_no { get; set; }

        public string Reason { get; set; }

        public decimal Total_quantity { get; set; }
        public decimal Total_Rate_amt { get; set; }
        public decimal Total_disc { get; set; }
        public decimal Total_SGST { get; set; }
        public decimal Total_CGST { get; set; }
        public decimal Total_IGST { get; set; }
        public decimal Round_off { get; set; }
        public decimal Final_amt { get; set; }
        public decimal Receive_cash { get; set; }
        public decimal Return_cash { get; set; }
        public decimal UPI_AMT { get; set; }
        public decimal Taxable_amt { get; set; }

        public char CashTrans { get; set; }
        public string Sale_CashTrans { get; set; }
        public string Narr { get; set; }
        public string User { get; set; }
        public char Status { get; set; }
        public int txt { get; set; } // 1=Insert, 2=Update

        // TRANS
        public int Year_id { get; set; }
        public int Trans_type_id { get; set; }
        public string trans_code { get; set; }
        public string Modify_reason { get; set; }
        public int Cust_id { get; set; }

        // TRANS DETAILS
        public int Sale_L_id { get; set; }
        public int CGST_id { get; set; }
        public int SGST_id { get; set; }
        public int IGST_id { get; set; }

        public int Roundoff_id { get; set; }
        public int Transfer_id { get; set; }

        public int Cash_return_id_cr { get; set; }
        public int Cash_return_id_dr { get; set; }

        // POINT
        public int Cr_point { get; set; }
        public int Dr_point { get; set; }
        public int Bal { get; set; }
        public decimal Point_amt { get; set; }
        public int Redeem_id { get; set; }

        // UPDATE
        public int update_Sale_id { get; set; }
        public int update_trans_id { get; set; }

        // DETAILS (MULTIPLE ITEMS)
        public List<SaleTransactionModel> Items { get; set; }

        // ITEM FIELDS 
        public int Stock_id { get; set; }
        public decimal Quantity { get; set; }
        public decimal MRP { get; set; }
        public decimal Disc { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }

        public decimal CGST_per { get; set; }
        public decimal SGST_per { get; set; }
        public decimal IGST_per { get; set; }

        public decimal CGST_amt { get; set; }
        public decimal SGST_amt { get; set; }
        public decimal IGST_amt { get; set; }

        public int Mode { get; set; }
    }
}
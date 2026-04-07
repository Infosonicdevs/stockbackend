using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class PurchaseReturnModel
    {
        public int? Return_id { get; set; }
        public DateTime Return_date { get; set; }
        public int Invoice_id { get; set; }
        public string Pavati_no { get; set; }
        public decimal Total_quantity { get; set; }
        public decimal Total { get; set; }
        public decimal Total_CGST { get; set; }
        public int CGST_L_id { get; set; }
        public decimal Total_SGST { get; set; }
        public decimal Total_IGST { get; set; }
        public int SGST_L_id { get; set; }
        public int IGST_L_id { get; set; }
        public decimal Total_Disc { get; set; }
        public decimal Total_amt { get; set; }
        public decimal Round_off { get; set; }
        public int Roundoff_id { get; set; }
        public decimal Bill_amt { get; set; }
        public string User { get; set; }
        public int Purchase_id { get; set; }
        public decimal net_disc { get; set; }
        public int net_disc_id { get; set; }
        public int txt { get; set; }

        public int Year_id { get; set; }
        public int Trans_type_id { get; set; }
        public string trans_code { get; set; }
        public string Modify_reason { get; set; }

        public int Cust_id { get; set; }
        public string CashTrans { get; set; }
        public string Card_no { get; set; }
        public int Vibhag_id { get; set; }
        public int? update_return_id { get; set; }
        public int? update_trans_id { get; set; }
        public string Status { get; set; }
        public int L_id { get; set; }
        public string Narr { get; set; }

        public int? Bank_id { get; set; }
        public string Cheque_no { get; set; }
        public DateTime? Cheque_date { get; set; }
        public DateTime? Effective_date { get; set; }
        public string Serial_no { get; set; }
        public string Reason { get; set; }
        public string Cheque_Status { get; set; }
        public string BankOrUdhar { get; set; }
    }
}
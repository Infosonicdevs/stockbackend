using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class PURCHASE_Transaction
    {
        //PURCHASE
        public int Invoice_id { get; set; }
        public DateTime Invoice_date { get; set; }
        public int State_id { get; set; }
        public int Vend_id { get; set; }
        public string Pavati_no { get; set; }
        public string Parvana_no { get; set; }
        public string Truck_no { get; set; }
        public decimal Commi_amt { get; set; }
        public decimal Diff_amt { get; set; }
        public decimal Hamali {  get; set; }
        public decimal Transport_amt { get; set; }
        public decimal Ma_ses_amt { get; set; }
        public decimal TCS_amt { get; set; }
        public decimal RoundOFF { get; set; }
        public decimal Final_amt { get; set; }
        public char Invert {  get; set; }
        public int Outlet_id { get; set; }
        public int Invoice_no { get; set; }
        public DateTime Bill_date { get; set; }
        public decimal Credit_note { get; set; }
        public decimal Net_disc { get; set; }

        //TRANS
        public int Trans_id { get; set; }
        public int Year_id { get; set; }
        public int Trans_type_id { get; set; }
        public char trans_code { get; set; }
        public string Modify_reason { get; set; }

        //TRANS DETAIL
        public char CashTrans {  get; set; }
        public int Pur_L_id { get; set; }
        public decimal Amount { get; set; }

        public decimal CGST_amt { get; set; }

        public decimal SGST_amt { get; set; }

        public decimal IGST_amt { get; set; }
        public int Cust_id { get; set; }

        public string Card_no { get; set; } 
        public string Created_by { get; set; }
        public string Modified_by { get; set; }
        public List<PURCHASE_DETAILS> PURCHASE_DETAILS { get; set; } = new List<PURCHASE_DETAILS>();

    }

    public class BazaSetting
    {
        public decimal Pur_id { get; set; }
        public decimal Round_Off_id { get; set; }
        public decimal Hamali_id { get; set; }
        public decimal Commi_id { get; set; }
        public decimal Transport_id { get; set; }
        public decimal Ma_ses_id { get; set; }
        public decimal Tcs_id { get; set; }
        public decimal Net_Disc_id { get; set; }
        public decimal Transfer_id { get; set; }
    }
}
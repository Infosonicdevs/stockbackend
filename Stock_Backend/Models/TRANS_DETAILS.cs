using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class TRANS_DETAILS
    {
        public int Trans_details_id { get; set; }
        public int Trans_id { get; set; }
        public char CashTrans {  get; set; }
        public int L_id { get; set; }
        public decimal Amount { get; set; }
        public int CrDr_id { get; set; }
        public int Cust_id { get; set; }
        public int Card_no { get; set; }
        public int Master_id { get; set; }
        public string Narr {  get; set; }
        public string Cust_name { get; set; }
    }
}
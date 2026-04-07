using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class Bank_acc_setting
    {
        public int Bank_id { get; set; }
        public string Bank_name { get; set; }
        public string Bank_acc_no { get; set; }
        public string Branch { get; set; }
        public string IFSC_code { get; set; }
        public int L_id { get; set; }
        public int Bank_type { get; set; }
        public int Customer_no { get; set; }
        public decimal Opn_amt { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
    }
}
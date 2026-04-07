using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class VENDOR_INFO
    {
        public int Vend_id { get; set; }
        public string Vend_name { get; set; }
        public string Vend_code { get; set; }
        public string Address { get; set; }
        public int City_id { get; set; }
        public int Taluka_id { get; set; }
        public int Dist_id { get; set; }
        public int State_id { get; set; }
        public string Contact_no { get; set; }
        public string GST_no { get; set; }
        public string Email {  get; set; }
        public string Bank_name { get; set; }
        public string Bank_acc_no { get; set; }
        public string Bank_branch { get; set; }
        public string IFSC_code { get; set; }
        public decimal Opn_bal { get; set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }
    }
}
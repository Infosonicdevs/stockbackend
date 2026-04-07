using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class TRANS
    {
        public int Trans_id { get; set;}
        public int Trans_no { get; set;}
        public int Outlet_id { get; set;}
        public DateTime Trans_date { get; set;}
        public int Year_id { get; set;}
        public decimal Trans_amt { get; set;}
        public int Trans_type_id { get; set;}
        public char Trans_code { get; set;}
        public string Modify_reason { get; set;}
        public char Status { get; set;}
        public string Created_by { get; set;}
        public string Modified_by { get; set;}
    }
}
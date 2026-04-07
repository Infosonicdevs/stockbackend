using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class CUSTOMER
    {
        public int Cust_id { get; set; }
        public int Cust_no { get; set; }
        public string Card_no { get; set; }
        public int Prefix_id { get; set; }
        public string First_name { get; set; }
        public string Middle_name { get; set; }
        public string Last_name { get; set; }
        public string Append {  get; set; }
        public string Address { get; set; }
        public int City_id { get; set; }
        public int Taluka_id { get; set; }
        public int District_id { get; set; }
        public int State_id { get; set; }
        public char Gender { get; set; }

        public string Cust_type { get; set; }
        public string Matdar_prakar {  get; set; }
        public DateTime DOB { get; set; }
        public string Phone_no { get; set; }
        public DateTime Acc_start_date { get; set; }
        public string Acc_end_date { get; set; }
        public string Reason { get; set; }
        public char Status  { get; set; }
        public string Created_by { get; set; }
        public string Modified_by { get; set; }

    }
}
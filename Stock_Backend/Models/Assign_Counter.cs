using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class AssignCounter
    {
        public int Id { get; set; }
        public int Counter_id { get; set; }
        public int Emp_id { get; set; }
        public DateTime Login_date { get; set; }
        public char Status { get; set; }
        public DateTime Log_out_date { get; set; }
        public decimal Opn_bal { get; set; }
        public decimal Closing_bal { get; set; }
        public string Login_time { get; set; }
        public string Log_out_time { get; set; }
        public int Bill_from { get; set; }
        public int Bill_to { get; set; }
        public decimal Total_sale { get; set; }
        public decimal Cash_sale { get; set; }
        public decimal Card_pay { get; set; }
        public decimal Upi_pay { get; set; }
        public decimal Cust_points { get; set; }
        public decimal Cash_return { get; set; }
        public decimal Office_return { get; set; }
        public string User_name { get; set; }
        public char Is_closed { get; set; }
    }
}
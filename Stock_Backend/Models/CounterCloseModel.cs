using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class CounterCloseModel
    {
        public int Id { get; set; }
        public int Counter_id { get; set; }
        public int Emp_id { get; set; }
        public int Bill_from { get; set; }
        public int Bill_to { get; set; }
        public decimal Total_sale { get; set; }
        public decimal Cash_sale { get; set; }
        public decimal Card_pay { get; set; }
        public decimal Upi_pay { get; set; }
        public decimal Cust_points { get; set; }
        public decimal Cash_return { get; set; }
        public decimal Office_return { get; set; }
        public DateTime Logout_date { get; set; }
        public string Logout_time { get; set; }
        public string User { get; set; }
    }
}
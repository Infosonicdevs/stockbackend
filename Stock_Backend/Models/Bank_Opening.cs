using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class BankOpening
    {
        public int Id { get; set; }
        public int L_id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string User_name { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stock_Backend.Models
{
    public class DayCloseModel
    {
        public int Outlet_id { get; set; }
        public DateTime Trans_date { get; set; }

        // OUTPUT
        public decimal LastTotal { get; set; }
        public decimal TodayCR { get; set; }
        public decimal TodayDR { get; set; }
        public decimal TodayTotal { get; set; }
        public decimal FinalTotal { get; set; }

        public int TotalLogin { get; set; }
        public int TotalLogout { get; set; }
    }
}
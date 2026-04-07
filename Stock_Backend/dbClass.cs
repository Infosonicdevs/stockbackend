using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;

namespace Stock_Backend
{
    public class DbClass
    {
        SqlCommand cmd;
        SqlDataAdapter da;

        public SqlConnection cn;
        int L_id;
        DataTable dt;



        public DbClass()
        {

        }



        public void Connect()
        {
            try
            {
                string constr = System.Configuration.ConfigurationManager.ConnectionStrings["Sqlcon"].ConnectionString;
                cn = new SqlConnection(constr);
                cn.Open();
                
            }
            catch (Exception ex)
            {

                Console.Write(ex.Message);
            }

        }

        public void Disconnect()
        {
            if (cn.State == ConnectionState.Open)
            {
                cn.Close();
                cn.Dispose();
            }
        }
        

        public DataTable GetTable(String selectquery)
        {
            da = new SqlDataAdapter(selectquery, cn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public bool IsExists(string Insertquery)
        {
            return GetTable(Insertquery).Rows.Count > 0 ? true : false;
        }

        public bool IsAdmin(string user)
        {

            DataTable dt = GetTable("Select * from USER_LOGIN where User_name='" + user + "'");

            if (dt.Rows.Count > 0 && Convert.ToInt16(dt.Rows[0]["Role_id"]) == 1)
            {
                return true;
            }

            return false;
        }

        //public string SaveImage(string ImgStr, string ImgName, string format)
        //{
        //    string path = HttpContext.Current.Server.MapPath("~/Members/Logos/"); //Path

        //    //Check if directory exist
        //    if (!System.IO.Directory.Exists(path))
        //    {
        //        System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
        //    }

        //    string imageName = ImgName + format;

        //    //set the image path
        //    string imgPath = string.Concat(path, imageName);

        //    string[] str = ImgStr.Split(',');
        //    byte[] bytes = Convert.FromBase64String(str[0]);

        //    File.WriteAllBytes(imgPath, bytes);

        //    if (!System.IO.File.Exists(imgPath))
        //    {
        //        throw new Exception("Something Went Wrong While saving Image!");
        //    }

        //    return imgPath;
        //}
        

        public bool IsValidUser(string user)
        {

            DataTable dt = GetTable("Select * from USER_LOGIN where User_name='" + user + "'");

            if (dt.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }


        //public string DataTableStringBuilder(DataTable dataTable)
        //{
        //    if (dataTable == null)
        //    {
        //        return string.Empty;
        //    }

        //    var jsonStringBuilder = new StringBuilder();
        //    if (dataTable.Rows.Count > 0)
        //    {
        //        jsonStringBuilder.Append("[");
        //        for (int i = 0; i < dataTable.Rows.Count; i++)
        //        {
        //            jsonStringBuilder.Append("{");
        //            for (int j = 0; j < dataTable.Columns.Count; j++)
        //                jsonStringBuilder.AppendFormat("\"{0}\":\"{1}\"{2}",
        //                        dataTable.Columns[j].ColumnName.ToString(),
        //                        dataTable.Rows[i][j].ToString(),
        //                        j < dataTable.Columns.Count - 1 ? "," : string.Empty);

        //            jsonStringBuilder.Append(i == dataTable.Rows.Count - 1 ? "}" : "},");
        //        }
        //        jsonStringBuilder.Append("]");
        //    }

        //    return jsonStringBuilder.ToString();
        //}



        public int Execute(string str)
        {
             cmd = new SqlCommand(str, cn);
             return  cmd.ExecuteNonQuery();   
        }


        public object ExecuteScalar(string str)
        {
            try
            {
              
                cmd = new SqlCommand(str, cn);              
                return cmd.ExecuteScalar(); ;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return 0;
            }

        }


        public decimal GetLedgerBalance(int Ledger_id,DateTime ActiveDay)
        {
            decimal Opening_bal, Cr_amt, Dr_amt, Ledger_bal=0;
            int Ledger_CrDr = Convert.ToInt32(ExecuteScalar("select coalesce(crdr_id,0) from VIEW_LEDGER where Ledger_id="+Ledger_id+""));

            Opening_bal = Convert.ToDecimal(ExecuteScalar("select coalesce(Amt,0) from OPENING_BAL where L_id="+Ledger_id+""));
            Cr_amt = Convert.ToDecimal(ExecuteScalar("select coalesce(sum(Amount),0) from VIEW_TRANS where CrDr_id=1 and L_id="+Ledger_id+" and Trans_date < '"+ActiveDay.ToString("MM/dd/yyyy")+"'"));
            Dr_amt = Convert.ToDecimal(ExecuteScalar("select coalesce(sum(Amount),0) from VIEW_TRANS where CrDr_id=2 and L_id="+Ledger_id+" and Trans_date < '"+ActiveDay.ToString("MM/dd/yyyy")+"'"));

            if (Ledger_CrDr == 1)
                Ledger_bal = Opening_bal + Cr_amt - Dr_amt;
            else if (Ledger_CrDr == 2)
                Ledger_bal = Opening_bal + Dr_amt - Cr_amt;

                return Ledger_bal;
        }
        
    }
}
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DBHelper
{
    public class GetIPStatus
    {
        //protected string constr = ConfigurationManager.ConnectionStrings["CresijCamConnectionString"].ConnectionString;
        protected string constr = ConfigurationManager.ConnectionStrings["SchoolConnectionString"].ConnectionString;
        public void LoggedinUser()
        {
            DataTable dtip = new DataTable();
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("sp_insertCurrentUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }

        }
        public DataTable ExecuteCmd(string query)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                            da.Fill(dt);
                        }

                    }
                }
                catch (Exception ex)
                { }
                finally
                { con.Close();
                    con.Dispose();
                }
            }
            return dt;
        }
        public DataTable FullIptable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ip");
            dt.Columns.Add("Location");
            dt.Columns.Add("Status");
            dt.Columns.Add("WorkStat");
            dt.Columns.Add("PCStat");
            dt.Columns.Add("Media");
            dt.Columns.Add("ProjectorStat");
            dt.Columns.Add("ProjHour");
            dt.Columns.Add("ScreenStat");
            dt.Columns.Add("CurtainStat");
            dt.Columns.Add("Light");
            dt.Columns.Add("CentralLock");
            dt.Columns.Add("ClassLock");
            dt.Columns.Add("PodiumLock");
            dt.Columns.Add("Timer");

            return dt;
        }
        public void ExecuteAnyCommand(string query)
        {

            using (MySqlConnection con = new MySqlConnection(constr))
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
                catch(Exception ex) { }
                finally
                {
                    con.Close(); con.Dispose();
                }
            }
        }
    }
}

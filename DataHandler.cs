using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Server
{
    public class DataHandler
    {
        public const String connString1 = "Data Source=.;Initial Catalog=Astrodon;Persist Security Info=True;User ID=sa;Password=1q2w#E$R"; //Local
        public const String connString2 = "Data Source=STEPHEN-PC\\MTDNDSQL;Initial Catalog=Astrodon;Persist Security Info=True;User ID=sa;Password=m3t@p@$$"; //Local
        public const String connString = "Data Source=SERVER-SQL;Initial Catalog=Astrodon;Persist Security Info=True;User ID=sa;Password=@str0d0n";
        private static SqlConnection sqlConnection1;

        private static SqlCommand GetCommand(String sqlString)
        {
            sqlConnection1 = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(sqlString, sqlConnection1);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection1;
            return cmd;
        }

        public static DataSet getData(String sqlString, out String msg)
        {
            DataSet ds = new DataSet();
            try
            {
                SqlCommand cmd = GetCommand(sqlString);
                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(ds);
                msg = "";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                throw new Exception(msg + " -> " + sqlString);
                ds = null;
            }
            finally
            {
                sqlConnection1.Close();
            }
            return ds;
        }

        public static DataSet getData(String sqlString, Dictionary<String, Object> sqlParms, out String msg)
        {
            DataSet ds = new DataSet();
            try
            {
                SqlCommand cmd = GetCommand(sqlString);
                if (sqlParms != null)
                {
                    foreach (KeyValuePair<String, Object> sqlParm in sqlParms)
                    {
                        cmd.Parameters.AddWithValue(sqlParm.Key, sqlParm.Value);
                    }
                }
                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(ds);
                msg = "";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                throw new Exception(msg + " -> " + sqlString);
                ds = null;
            }
            finally
            {
                sqlConnection1.Close();
            }
            return ds;
        }

        public static void setData(String sqlString, out String msg)
        {
            try
            {
                SqlCommand cmd = GetCommand(sqlString);
                sqlConnection1.Open();
                cmd.ExecuteNonQuery();
                msg = "";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                throw new Exception(msg + " -> " + sqlString);
            }
            finally
            {
                sqlConnection1.Close();
            }
        }

        public static void setData(String sqlString, Dictionary<String, Object> sqlParms, out String msg)
        {
            try
            {
                SqlCommand cmd = GetCommand(sqlString);
                if (sqlParms != null)
                {
                    foreach (KeyValuePair<String, Object> sqlParm in sqlParms)
                    {
                        cmd.Parameters.AddWithValue(sqlParm.Key, sqlParm.Value);
                    }
                }
                sqlConnection1.Open();
                cmd.ExecuteNonQuery();
                msg = "";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                throw new Exception(msg + " -> " + sqlString);
            }
            finally
            {
                sqlConnection1.Close();
            }
        }
    }
}
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Server
{
    public class SqlArgs : EventArgs
    {
        public String msgArgs;

        public SqlArgs(String args)
        {
            msgArgs = args;
        }
    }

    public class MySqlConnector
    {
        private const String serverIP = "10.0.1.252";
        private const String database = "astrodon_co_za";
        private const String uid = "root";
        private const String password = "66r94e77";

        private MySqlConnection mysqlConn;
        private MySqlCommand mysqlCmd;
        private String sqlStatus;

        public String SqlStatus
        {
            get { return sqlStatus; }
            set
            {
                sqlStatus = value;
                if (MessageHandler != null) { MessageHandler(this, new SqlArgs(sqlStatus)); }
            }
        }

        public event EventHandler<SqlArgs> MessageHandler;

        public MySqlConnector()
        {
            try
            {
                String connectionString = "SERVER=" + serverIP + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
                mysqlConn = new MySqlConnection(connectionString);
                try
                {
                    if (mysqlConn.State != System.Data.ConnectionState.Open) { mysqlConn.Open(); }
                }
                catch (Exception ex)
                {
                    String msg = ex.Message;
                }
                finally
                {
                    if (mysqlConn.State != System.Data.ConnectionState.Closed) { mysqlConn.Close(); }
                }
            }
            catch (Exception ex)
            {
                String msg = ex.Message;
            }
        }

        public bool ToggleConnection(bool open)
        {
            bool success = false;
            try
            {
                if (open)
                {
                    if (mysqlConn.State != System.Data.ConnectionState.Open) { mysqlConn.Open(); }
                    success = mysqlConn.State == System.Data.ConnectionState.Open;
                }
                else
                {
                    if (mysqlConn.State != System.Data.ConnectionState.Closed) { mysqlConn.Close(); }
                    success = mysqlConn.State == System.Data.ConnectionState.Closed;
                }
            }
            catch { }
            return success;
        }

        public bool InsertStatement(String title, String description, String file, String unitno, String[] emails)
        {
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            String status = String.Empty;
            String cQuery = "SELECT * FROM tx_astro_docs WHERE file = @file";
            sqlParms.Add("@file", file);
            DataSet cDS = GetData(cQuery, sqlParms, out status);

            bool rentalStatement = title.ToLower().EndsWith("_r") && title.ToLower().Contains("statement");

            if (status != "OK") { SqlStatus = status; }
            if (cDS != null && cDS.Tables.Count > 0 && cDS.Tables[0].Rows.Count > 0)
            {
                return true;
            }
            else
            {
                String cruser_id = "0";
                if (emails.Length > 0)
                {
                    foreach (String email in emails)
                    {
                        if (GetLogin(email, unitno, rentalStatement, out cruser_id)) { break; }
                    }
                }
                sqlParms.Add("@cruser_id", cruser_id);
                sqlParms.Add("@title", title);
                sqlParms.Add("@description", description);
                if (rentalStatement && cruser_id == "0") { unitno = unitno + "R"; }
                sqlParms.Add("@unitno", unitno);
                String query = "INSERT INTO tx_astro_docs(pid, tstamp, crdate, cruser_id, title, description, file, unitno)";
                query += " VALUES(1, UNIX_TIMESTAMP(now()),UNIX_TIMESTAMP(now()),@cruser_id,@title,@description,@file,@unitno);";
                return SetData(query, sqlParms, out status);
            }
        }

        public bool InsertBuilding(String name, String abbr, out String status)
        {
            String testQuery = "SELECT uid FROM tx_astro_complex WHERE name = @name AND abbr = @abbr";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@name", name);
            sqlParms.Add("@abbr", abbr);
            DataSet dsTest = GetData(testQuery, sqlParms, out status);
            if (status != "OK") { SqlStatus = "Building - " + testQuery + status; }
            if (dsTest != null && dsTest.Tables.Count > 0 && dsTest.Tables[0].Rows.Count > 0)
            {
                status = "OK";
                return true;
            }
            else if (status == "OK")
            {
                String query = "INSERT INTO tx_astro_complex(pid, tstamp, crdate, cruser_id, name, abbr)";
                query += " VALUES(1, UNIX_TIMESTAMP(now()), UNIX_TIMESTAMP(now()), 1, @name, @abbr)";
                bool success = SetData(query, sqlParms, out status);
                if (status != "OK") { SqlStatus = "Building - " + query + status; }
                return success;
            }
            else
            {
                return false;
            }
        }

        public void InsertCustomer(String bname, String babbr, String acc, String[] emails, out String status)
        {
            String bQuery = "SELECT uid FROM tx_astro_complex WHERE name = @bname";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@bname", bname);
            DataSet dsB = GetData(bQuery, sqlParms, out status);
            if (status != "OK") { SqlStatus = "Customer - " + bQuery + status; }
            List<String> emailAddresses = emails.ToList();
            if (dsB != null && dsB.Tables.Count > 0 && dsB.Tables[0].Rows.Count > 0)
            {
                DataRow drB = dsB.Tables[0].Rows[0];
                String bID = drB["uid"].ToString();
                bool newCustomer = false;
                CheckOldLogins(acc, emails);
                String validEmail = String.Empty;
                String cruser_id = CheckCustomer(acc, emails, out status, out newCustomer, out validEmail);
                if (status != "OK") { SqlStatus = "Customer - 129" + status; }
                if (!String.IsNullOrEmpty(cruser_id) && !String.IsNullOrEmpty(validEmail))
                {
                    if (!String.IsNullOrEmpty(validEmail))
                    {
                        sqlParms.Add("@acc", acc);
                        sqlParms.Add("@cruser_id", cruser_id);
                        sqlParms.Add("@validEmail", validEmail);
                        sqlParms.Add("@bID", bID);

                        String checkMappingID = "SELECT * FROM tx_astro_account_user_mapping WHERE account_no = @acc";
                        DataSet ds = GetData(checkMappingID, sqlParms, out status);
                        if (status != "OK") { SqlStatus = "Customer - 146" + status; }
                        String updateMappingQuery = String.Empty;
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 1)
                            {
                                updateMappingQuery = "DELETE FROM tx_astro_account_user_mapping WHERE account_no = @acc AND owner_email <> @validEmail";
                                SetData(updateMappingQuery, sqlParms, out status);
                                if (status != "OK") { SqlStatus = "Customer - 152" + status; }

                                bool hasAccount = false;

                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    if (dr["owner_email"].ToString() == validEmail && !hasAccount)
                                    {
                                        hasAccount = true;
                                    }
                                    else
                                    {
                                        updateMappingQuery = "DELETE FROM tx_astro_account_user_mapping WHERE account_no = '" + acc + "' AND owner_email <> '" + validEmail + "'";
                                        SetData(updateMappingQuery, null, out status);
                                        if (status != "OK") { SqlStatus = "Customer - 162" + status; }
                                    }
                                }
                                if (!hasAccount)
                                {
                                    String updCQuery = "INSERT INTO tx_astro_account_user_mapping(pid, tstamp, crdate, cruser_id, account_no, owner_email, tenant_email, complex_id, complex_name)";
                                    updCQuery += " VALUES(1, UNIX_TIMESTAMP(now()), UNIX_TIMESTAMP(now()), @cruser_id, @acc, @validEmail, @validEmail, @bID, @bname)";
                                    SetData(updCQuery, sqlParms, out status);
                                    if (status != "OK") { SqlStatus = "Customer - 170" + status; }
                                }
                            }
                            UpdateUser(cruser_id);
                        }
                        else
                        {
                            String updCQuery = "INSERT INTO tx_astro_account_user_mapping(pid, tstamp, crdate, cruser_id, account_no, owner_email, tenant_email, complex_id, complex_name)";
                            updCQuery += " VALUES(1, UNIX_TIMESTAMP(now()), UNIX_TIMESTAMP(now()), @cruser_id, @acc, @validEmail, @validEmail, @bID, @bname)";
                            SetData(updCQuery, sqlParms, out status);
                            if (status != "OK") { SqlStatus = "Customer - 179" + status; }

                            if (status != "OK") { status += " - " + updCQuery; }
                        }
                        updateMappingQuery = "UPDATE tx_astro_docs SET cruser_id = @cruser_id WHERE unitno = @acc AND cruser_id <> @cruser_id";
                        SetData(updateMappingQuery, sqlParms, out status);
                        if (status != "OK") { SqlStatus = status; }
                    }
                }
                else
                {
                    SqlStatus = "No valid email address";
                }
                UpdateLogins();
            }
            else if (InsertBuilding(bname, babbr, out status))
            {
                InsertCustomer(bname, babbr, acc, emails, out status);
            }
        }

        private void UpdateLogins()
        {
            String query = "update fe_users f inner join tx_astro_account_user_mapping t on f.uid = t.cruser_id and f.username = t.owner_email ";
            query += " set f.disable = 0 where f.disable = 1 and f.password <> '' and f.password is not null";
            String status = String.Empty;
            SetData(query, null, out status);
        }

        private void UpdateUser(String cruser_id)
        {
            String updQuery = "update  fe_users f1, fe_users f2 set f1.pid= f2.pid,f1.tstamp = f2.tstamp,f1.starttime = f2.starttime,f1.endtime = f1.endtime,f1.crdate= f2.crdate,";
            updQuery += " f1.cruser_id= f2.cruser_id,f1.lockToDomain= f2.lockToDomain,f1.deleted= f2.deleted,f1.uc= f2.uc,f1.TSconfig= f2.TSconfig,f1.fe_cruser_id= f2.fe_cruser_id,";
            updQuery += " f1.tx_astro_usertype= f2.tx_astro_usertype,f1.tx_feuserloginsystem_redirectionafterlogin= f2.tx_feuserloginsystem_redirectionafterlogin,";
            updQuery += " f1.tx_feuserloginsystem_redirectionafterlogout= f2.tx_feuserloginsystem_redirectionafterlogout,f1.tx_astro_activation_code= f2.tx_astro_activation_code,";
            updQuery += " f1.tx_astro_istenant= f2.tx_astro_istenant,f1.felogin_redirectPid= f2.felogin_redirectPid,f1.felogin_forgotHash= f2.felogin_forgotHash";
            updQuery += " where f1.uid = @uid and f2.username = 'sheldon@astrodon.co.za'";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@uid", cruser_id);
            String status = String.Empty;
            SetData(updQuery, sqlParms, out status);
            if (status != "OK") { SqlStatus = "Customer - 203" + status; }
        }

        private void CheckOldLogins(String acc, String[] emails)
        {
            String query = "select f.uid from fe_users f inner join tx_astro_account_user_mapping t on f.uid = t.cruser_id where t.account_no = @accNo and f.username <> @email and f.disable <> 1";
            String status = String.Empty;
            foreach (String email in emails)
            {
                Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
                sqlParms.Add("@accNo", acc);
                sqlParms.Add("@email", email);
                DataSet ds = GetData(query, sqlParms, out status);
                if (status != "OK") { SqlStatus = "Customer - 214" + status; }
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        String uid = dr["uid"].ToString();
                        String deleteQuery = "UPDATE fe_users SET disable = 1 WHERE uid = " + uid;
                        SetData(deleteQuery, null, out status);
                        if (status != "OK") { SqlStatus = "Customer - 220 - " + deleteQuery + "-" + status; }
                    }
                }
            }
        }

        public String CheckCustomer(String acc, String[] emails, out String status, out bool newCustomer, out String validEmail)
        {
            status = String.Empty;
            newCustomer = true;
            String cruser_id = String.Empty;
            validEmail = String.Empty;
            foreach (String email in emails)
            {
                if (!email.Contains("imp.ad-one.co.za") && GetLogin(email, acc, false, out cruser_id))
                {
                    validEmail = email;
                    newCustomer = false;
                    break;
                }
                else if (!email.Contains("imp.ad-one.co.za"))
                {
                    validEmail = email;
                    CreateLogin(email, acc, out cruser_id);
                    newCustomer = true;
                }
            }
            status = "OK";
            return cruser_id;
        }

        private bool GetLogin(String emailAddress, String accNo, bool rentalUnit, out string uid)
        {
            String status = String.Empty;
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@emailAddress", emailAddress);
            sqlParms.Add("@accNo", accNo);
            String feQuery = "SELECT f.uid  FROM fe_users f inner join tx_astro_account_user_mapping m";
            feQuery += " on f.uid = m.cruser_id WHERE f.username = @emailAddress and m.account_no = @accNo";
            if (rentalUnit) { feQuery += " AND m.complex_id = 210"; }
            DataSet dsFE = GetData(feQuery, sqlParms, out status);
            if (status != "OK") { SqlStatus = "Customer - 247" + status; }

            if (dsFE != null && dsFE.Tables.Count > 0 && dsFE.Tables[0].Rows.Count > 0)
            {
                uid = dsFE.Tables[0].Rows[0]["uid"].ToString();
                return true;
            }
            else
            {
                uid = "0";
                return false;
            }
        }

        private bool CreateLogin(String emailAddress, String acc, out string uid)
        {
            String status = String.Empty;
            String feIQ = "INSERT INTO fe_users(pid, tstamp, username, disable, email, crdate, tx_astro_accountno, starttime, endtime, cruser_id, lockToDomain, deleted, uc, ";
            feIQ += "TSconfig, fe_cruser_id, tx_astro_usertype, tx_feuserloginsystem_redirectionafterlogin, tx_feuserloginsystem_redirectionafterlogout, tx_astro_activation_code, ";
            feIQ += "tx_astro_istenant, felogin_redirectPid, felogin_forgotHash)";
            feIQ += " SELECT f2.pid, UNIX_TIMESTAMP(now()), '" + emailAddress + "', 1, '" + emailAddress + "', UNIX_TIMESTAMP(now()), '" + acc + "', f2.starttime, f2.endtime, f2.cruser_id, ";
            feIQ += " f2.lockToDomain, f2.deleted, f2.uc, f2.TSconfig, f2.fe_cruser_id, f2.tx_astro_usertype, f2.tx_feuserloginsystem_redirectionafterlogin, f2.tx_feuserloginsystem_redirectionafterlogout, ";
            feIQ += " f2.tx_astro_activation_code, f2.tx_astro_istenant, f2.felogin_redirectPid, f2.felogin_forgotHash FROM fe_users f2 WHERE f2.username = 'sheldon@astrodon.co.za'";
            SetData(feIQ, null, out status);
            if (status != "OK") { SqlStatus = "Customer - 266" + status; }
            return GetLogin(emailAddress, acc, false, out uid);
        }

        public String[] HasLogin(String email)
        {
            String query = "SELECT uid, usergroup FROM fe_users WHERE username = '" + email + "' AND usergroup <> '1,2,6'";
            String[] returners = new string[2];
            String status;
            DataSet ds = GetData(query, null, out status);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                returners[0] = ds.Tables[0].Rows[0]["uid"].ToString();
                returners[1] = ds.Tables[0].Rows[0]["usergroup"].ToString();
                return returners;
            }
            else
            {
                return null;
            }
        }

        public bool UpdateGroup(String uid, String group)
        {
            String query = "UPDATE fe_users SET usergroup = '" + group + "' WHERE uid = " + uid;
            String status;
            return SetData(query, null, out status);
        }

        public bool FindFile(String fileName)
        {
            String query = String.Format("SELECT d.file FROM tx_astro_docs d where d.file = '{0}'", fileName);
            String status = "";
            DataSet ds = GetData(query, null, out status);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public DataSet GetData(String query, Dictionary<String, Object> sqlParms, out String status)
        {
            DataSet ds = new DataSet();
            try
            {
                mysqlCmd = new MySqlCommand(query, mysqlConn);
                if (sqlParms != null)
                {
                    foreach (KeyValuePair<String, Object> sqlParm in sqlParms)
                    {
                        mysqlCmd.Parameters.AddWithValue(sqlParm.Key, sqlParm.Value);
                    }
                }
                if (mysqlConn.State != System.Data.ConnectionState.Open) { ToggleConnection(true); }
                MySql.Data.MySqlClient.MySqlDataAdapter da = new MySqlDataAdapter();
                da.SelectCommand = mysqlCmd;
                da.Fill(ds);
                status = "OK";
            }
            catch (Exception ex)
            {
                ds = null;
                status = ex.Message;
            }
            return ds;
        }

        public bool SetData(String query, Dictionary<String, Object> sqlParms, out String status)
        {
            bool success = false;
            try
            {
                mysqlCmd = new MySqlCommand(query, mysqlConn);
                if (sqlParms != null)
                {
                    foreach (KeyValuePair<String, Object> sqlParm in sqlParms)
                    {
                        mysqlCmd.Parameters.AddWithValue(sqlParm.Key, sqlParm.Value);
                    }
                }
                if (mysqlConn.State != System.Data.ConnectionState.Open) { ToggleConnection(true); }
                mysqlCmd.ExecuteNonQuery();
                status = "OK";
                success = true;
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }
            return success;
        }

        public DataSet GetPurgeDocuments()
        {
            DateTime checkDate = DateTime.Now.AddMonths(-16);
            TimeSpan span = (checkDate - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            String unixTime = span.TotalSeconds.ToString();
            String status;
            DataSet dsDocs = GetData(PurgeDocumentQuery(unixTime), null, out status);
            return dsDocs;
        }

        public DataSet GetOffAdminDocs()
        {
            String query = "select file from tx_astro_docs where cruser_id not in (select uid from fe_users)";
            String status;
            DataSet dsDocs = GetData(query, null, out status);
            return dsDocs;
        }

        private String PurgeDocumentQuery(String unixTime)
        {
            String query = String.Format("SELECT c.name, d.uid, d.file FROM tx_astro_docs d inner join tx_astro_account_user_mapping m on d.unitno = m.account_no inner join tx_astro_complex c on c.uid = m.complex_id where d.tstamp <= {0}", unixTime);
            return query;
        }

        public void DeletePurgeDocuments(String documentID)
        {
            String delQuery = String.Format("DELETE FROM tx_astro_docs WHERE uid = {0}", documentID);
            String status;
            SetData(delQuery, null, out status);
        }
    }
}
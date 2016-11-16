using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server
{
    public class SMSPoll
    {
        private System.Timers.Timer tmrCheck;
        private String dataStatus = "";

        public event EventHandler<MessageArgs> NewMessageEvent;

        private DateTime nextSendTime;
        private DateTime nextCheckTime;

        public SMSPoll()
        {
            tmrCheck = new System.Timers.Timer();
            tmrCheck.Elapsed += new System.Timers.ElapsedEventHandler(tmrCheck_Elapsed);
        }

        public void InitializePolling()
        {
            if (tmrCheck.Enabled) { tmrCheck.Enabled = false; }
            tmrCheck.Interval = int.Parse(Server.Properties.Settings.Default.CheckSMS);
            tmrCheck.Enabled = true;
            DateTime now = DateTime.Now;
            nextSendTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1);
            nextCheckTime = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0).AddDays(1);
            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Next Send At: " + nextSendTime.ToString("yyyy/MM/dd HH:mm:ss"))); }
            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Next Check At: " + nextCheckTime.ToString("yyyy/MM/dd HH:mm:ss"))); }
            SendMessages(SendOutboundMessages());
            QueryStatus(getOutboundMessages());
        }

        private void tmrCheck_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SendMessages(SendOutboundMessages());
            QueryStatus(getOutboundMessages());
        }

        public void SendMessages()
        {
            ForceSendMessages(SendOutboundMessages());
        }

        public void QueryStatus()
        {
            ForceQueryStatus(getOutboundMessages());
        }

        private DataSet getOutboundMessages()
        {
            String query = "SELECT * FROM tblSMS WHERE batchID is not null AND billable = 'True' and (status = '1' or status = '0')";
            DataSet ds = DataHandler.getData(query, out dataStatus);
            return ds;
        }

        private DataSet SendOutboundMessages()
        {
            String query = "SELECT * FROM tblSMS WHERE status = '-1'";
            DataSet ds = DataHandler.getData(query, out dataStatus);
            return ds;
        }

        private void UpdateMessage(String batchID, String status, DateTime nextPolled, int pollCount)
        {
            String query = String.Format("UPDATE tblSMS SET status = '{0}', nextPolled = getDate(), pollCount = {2} WHERE batchID = '{3}'", status, nextPolled.ToString(), pollCount.ToString(), batchID);
            DataHandler.setData(query, out dataStatus);
        }

        private void UpdateMessage(String msgID, String batchID)
        {
            String query = String.Format("UPDATE tblSMS SET batchID = '{0}', status = '1' WHERE id = '{1}'", batchID, msgID);
            DataHandler.setData(query, out dataStatus);
        }

        private void QueryStatus(DataSet dsMsg)
        {
            DateTime now = DateTime.Now;
            if (now >= nextCheckTime)
            {
                StatusChecker.SMSService checker = new StatusChecker.SMSService();

                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Checking sms status: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
                if (dsMsg != null && dsMsg.Tables.Count > 0 && dsMsg.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsMsg.Tables[0].Rows)
                    {
                        String batchID = dr["batchID"].ToString().Replace("\n", "");
                        String number = dr["number"].ToString().Replace("\n", "");
                        String msgStatus = checker.GetStatus(batchID);
                        //String msgStatus = dr["bsstatus"].ToString();
                        if (!String.IsNullOrEmpty(msgStatus))
                        {
                            if (msgStatus == "10" || msgStatus == "11")
                            {
                                String[] bStuff = GetBuildingStuff(dr["building"].ToString());
                                String pastelString = "";
                                String reference = dr["reference"].ToString();
                                String description = dr["message"].ToString();
                                String trustAcc = "1115000";
                                String amt = "2.00";
                                //verify with Sheldon / Tertia
                                try
                                {
                                    String pastelReturn = frmMain.pastel.PostBatch(DateTime.Parse(dr["sent"].ToString()), int.Parse(bStuff[0]), "CENTRE17", bStuff[1], 5,
                                        int.Parse(bStuff[2]), bStuff[3], dr["customer"].ToString(), bStuff[4], "9250000", reference, description, amt, trustAcc, "", out pastelString);
                                }
                                catch { }
                            }
                            UpdateMessage(batchID, getStatus(int.Parse(msgStatus)), DateTime.Now, 1);
                        }
                    }
                }
                nextCheckTime = nextCheckTime.AddDays(1);
            }
        }

        private void SendMessages(DataSet dsMsg)
        {
            DateTime now = DateTime.Now;
            if (now >= nextSendTime)
            {
                StatusChecker.SMSService checker = new StatusChecker.SMSService();

                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Sending sms: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
                if (dsMsg != null && dsMsg.Tables.Count > 0 && dsMsg.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsMsg.Tables[0].Rows)
                    {
                        String id = dr["id"].ToString();
                        String number = dr["number"].ToString();
                        String message = dr["number"].ToString();
                        String reference = dr["number"].ToString();
                        String batchID;
                        String status;
                        if (SendSMS(number, String.Format("{0} - Reply with ref:{1}", message, reference), out status, out batchID))
                        {
                            UpdateMessage(id, batchID);
                        }
                    }
                }
                nextSendTime = nextSendTime.AddDays(1);
            }
        }

        private void ForceSendMessages(DataSet dsMsg)
        {
            StatusChecker.SMSService checker = new StatusChecker.SMSService();

            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Sending sms: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
            if (dsMsg != null && dsMsg.Tables.Count > 0 && dsMsg.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsMsg.Tables[0].Rows)
                {
                    String id = dr["id"].ToString();
                    String number = dr["number"].ToString();
                    String message = dr["message"].ToString();
                    String reference = dr["reference"].ToString();
                    String batchID;
                    String status;
                    String messageS = String.Format("{0} - Reply with ref:{1}", message, reference);
                    if (messageS.Length > 160) { messageS = message; }
                    if (SendSMS(number, messageS, out status, out batchID))
                    {
                        UpdateMessage(id, batchID);
                    }
                    else
                    {
                        if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs(status)); }
                    }
                }
            }
        }

        private void ForceQueryStatus(DataSet dsMsg)
        {
            StatusChecker.SMSService checker = new StatusChecker.SMSService();

            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Checking sms status: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
            if (dsMsg != null && dsMsg.Tables.Count > 0 && dsMsg.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsMsg.Tables[0].Rows)
                {
                    String batchID = dr["batchID"].ToString();
                    String number = dr["number"].ToString();
                    String msgStatus = checker.GetStatus(batchID);
                    //String msgStatus = dr["bsstatus"].ToString();
                    if (!String.IsNullOrEmpty(msgStatus))
                    {
                        if (msgStatus == "10" || msgStatus == "11")
                        {
                            String[] bStuff = GetBuildingStuff(dr["building"].ToString());
                            String pastelString = "";
                            String reference = dr["reference"].ToString();
                            String description = dr["message"].ToString();
                            String trustAcc = "1115000";
                            String amt = "2.00";
                            //verify with Sheldon / Tertia
                            try
                            {
                                String pastelReturn = frmMain.pastel.PostBatch(DateTime.Parse(dr["sent"].ToString()), int.Parse(bStuff[0]), "CENTRE16", bStuff[1], 5,
                                    int.Parse(bStuff[2]), bStuff[3], dr["customer"].ToString(), bStuff[4], "9250000", reference, description, amt, trustAcc, "", out pastelString);
                            }
                            catch { }
                        }
                        UpdateMessage(batchID, getStatus(int.Parse(msgStatus)), DateTime.Now, 1);
                    }
                }
            }
        }

        public String[] GetBuildingStuff(String code)
        {
            //building.Period, "CENTRE14", building.Path, 5, building.Journal, building.BC, c.accNumber, building.Contra, building.Contra, docType, docType, amt.ToString("#0.00"), trustAcc, "", out pastelString);
            String query = String.Format("SELECT period, datapath, journals, bc, contra FROM tblBuildings WHERE code = '{0}'", code);
            DataSet ds = DataHandler.getData(query, out dataStatus);
            String[] stuff = new string[5];
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count == 1)
            {
                stuff[0] = ds.Tables[0].Rows[0]["period"].ToString();
                stuff[1] = ds.Tables[0].Rows[0]["datapath"].ToString();
                stuff[2] = ds.Tables[0].Rows[0]["journals"].ToString();
                stuff[3] = ds.Tables[0].Rows[0]["bc"].ToString();
                stuff[4] = ds.Tables[0].Rows[0]["contra"].ToString();
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    stuff[i] = string.Empty;
                }
            }
            return stuff;
        }

        public string Post(string batchID)
        {
            String url = "http://bulksms.2way.co.za:5567/eapi/status_reports/get_report/2/2.0";
            string data = "";
            data += "username=" + HttpUtility.UrlEncode(Server.Properties.Settings.Default.SMSUser, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            data += "&password=" + HttpUtility.UrlEncode(Server.Properties.Settings.Default.SMSPassword, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            data += "&batch_id=" + HttpUtility.UrlEncode(batchID, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            string result = null;
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(data);

                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
                WebReq.Method = "POST";
                WebReq.ContentType = "application/x-www-form-urlencoded";
                WebReq.ContentLength = buffer.Length;
                Stream PostData = WebReq.GetRequestStream();

                PostData.Write(buffer, 0, buffer.Length);
                PostData.Close();
                HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
                Console.WriteLine(WebResp.StatusCode);

                Stream Response = WebResp.GetResponseStream();
                StreamReader _Response = new StreamReader(Response);
                result = _Response.ReadToEnd();
            }
            catch (Exception ex)
            {
                result += "\n" + ex.Message;
            }
            return result.Trim() + "\n";
        }

        public bool SendSMS(String phoneNumber, String message, out String status, out String batchID)
        {
            double credits = GetCredits(out status);
            batchID = "";
            if (credits > 0)
            {
                if (!phoneNumber.StartsWith("27"))
                {
                    if (phoneNumber.StartsWith("0")) { phoneNumber = phoneNumber.Substring(1); }
                    phoneNumber = "27" + phoneNumber;
                }
                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs(phoneNumber)); }
                string url = "http://bulksms.2way.co.za:5567/eapi/submission/send_sms/2/2.0";
                Hashtable result;

                string data = createMessage(phoneNumber, message);
                result = send_sms(data, url);
                if ((int)result["success"] == 1)
                {
                    batchID = result["api_batch_id"].ToString().Replace("\n", "");
                    status = formatted_server_response(result);
                    return true;
                }
                else
                {
                    status = formatted_server_response(result);
                    return false;
                }
            }
            else
            {
                if (credits == 0) { status = "Insufficient credits"; }
                return false;
            }
        }

        public bool SendBatch(String batchFile, out String status)
        {
            TextReader tr = new StreamReader(batchFile);
            // E.g. TextReader tr = new StreamReader(@"C:\Users\user\Desktop\my_batch_file.csv")
            // Please see http://bulksms.2way.co.za/docs/eapi/submission/send_batch/ for information
            // on what the format of your input file should be.

            /*
             msisdn,message
            "111111111","Hi there"
            "333333333","Hello again"

             */

            string line;
            string batch = "";
            double requiredCredits = 0;
            while ((line = tr.ReadLine()) != null)
            {
                requiredCredits++;
                batch += line + "\n";
            }
            //Console.WriteLine(batch);

            tr.Close();
            double credits = GetCredits(out status);
            if (credits >= requiredCredits)
            {
                string url = "http://bulksms.2way.co.za:5567/eapi/submission/send_batch/1/1.0";

                /*****************************************************************************************************
                **Construct data
                *****************************************************************************************************/
                /*
                * Note the suggested encoding for the some parameters, notably
                * the username, password and especially the message.  ISO-8859-1
                * is essentially the character set that we use for message bodies,
                * with a few exceptions for e.g. Greek characters. For a full list,
                * see: http://bulksms.2way.co.za/docs/eapi/submission/character_encoding/
                */

                string data = "";
                data += "username=" + HttpUtility.UrlEncode(Server.Properties.Settings.Default.SMSUser, System.Text.Encoding.GetEncoding("ISO-8859-1"));
                data += "&password=" + HttpUtility.UrlEncode(Server.Properties.Settings.Default.SMSPassword, System.Text.Encoding.GetEncoding("ISO-8859-1"));
                data += "&batch_data=" + HttpUtility.UrlEncode(batch, System.Text.Encoding.GetEncoding("ISO-8859-1"));
                data += "&want_report=1";

                string sms_result = Post(url, data);

                string[] parts = sms_result.Split('|');

                string statusCode = parts[0];
                string statusString = parts[1];

                if (!statusCode.Equals("0"))
                {
                    status = "Error: " + statusCode + ": " + statusString;
                    return false;
                }
                else
                {
                    status = "Success: batch ID " + parts[2];
                    return true;
                }
            }
            else
            {
                if (credits < requiredCredits && credits > -1) { status = "Insufficient credits"; }
                return false;
            }
        }

        private double GetCredits(out String status)
        {
            string url = "http://bulksms.2way.co.za:5567/eapi/user/get_credits/1/1.1";
            string data = "";
            data += "username=" + HttpUtility.UrlEncode(Server.Properties.Settings.Default.SMSUser, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            data += "&password=" + HttpUtility.UrlEncode(Server.Properties.Settings.Default.SMSPassword, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            String result = Post(url, data);
            string[] parts = result.Split('|');
            if (parts.Length > 1)
            {
                string statusCode = parts[0];
                string statusString = parts[1];
                if (statusCode == "0")
                {
                    status = "";

                    return double.Parse(statusString);
                }
                else
                {
                    status = statusString;
                    return -1;
                }
            }
            else
            {
                status = parts[0];
                return -1;
            }
        }

        public string formatted_server_response(Hashtable result)
        {
            string ret_string = "";
            if ((int)result["success"] == 1)
            {
                ret_string += "Success: batch ID " + (string)result["api_batch_id"] + "API message: " + (string)result["api_message"] + "\nFull details " + (string)result["details"];
            }
            else
            {
                ret_string += "Fatal error: HTTP status " + (string)result["http_status_code"] + " API status " + (string)result["api_status_code"] + " API message " + (string)result["api_message"] + "\nFull details " + (string)result["details"];
            }

            return ret_string;
        }

        public Hashtable send_sms(string data, string url)
        {
            string sms_result = Post(url, data);

            Hashtable result_hash = new Hashtable();

            string tmp = "";
            tmp += "Response from server: " + sms_result + "\n";
            string[] parts = sms_result.Split('|');

            string statusCode = parts[0];
            string statusString = parts[1];

            result_hash.Add("api_status_code", statusCode);
            result_hash.Add("api_message", statusString);

            if (parts.Length != 3)
            {
                tmp += "Error: could not parse valid return data from server.\n";
            }
            else
            {
                if (statusCode.Equals("0"))
                {
                    result_hash.Add("success", 1);
                    result_hash.Add("api_batch_id", parts[2]);
                    tmp += "Message sent - batch ID " + parts[2] + "\n";
                }
                else if (statusCode.Equals("1"))
                {
                    // Success: scheduled for later sending.
                    result_hash.Add("success", 1);
                    result_hash.Add("api_batch_id", parts[2]);
                }
                else
                {
                    result_hash.Add("success", 0);
                    tmp += "Error sending: status code " + parts[0] + " description: " + parts[1] + "\n";
                }
            }
            result_hash.Add("details", tmp);
            return result_hash;
        }

        public string Post(string url, string data)
        {
            string result = null;
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(data);

                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
                WebReq.Method = "POST";
                WebReq.ContentType = "application/x-www-form-urlencoded";
                WebReq.ContentLength = buffer.Length;
                Stream PostData = WebReq.GetRequestStream();

                PostData.Write(buffer, 0, buffer.Length);
                PostData.Close();
                HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
                Console.WriteLine(WebResp.StatusCode);

                Stream Response = WebResp.GetResponseStream();
                StreamReader _Response = new StreamReader(Response);
                result = _Response.ReadToEnd();
            }
            catch (Exception ex)
            {
                result += "\n" + ex.Message;
            }
            return result.Trim() + "\n";
        }

        public string character_resolve(string body)
        {
            Hashtable chrs = new Hashtable();
            chrs.Add('Ω', "Û");
            chrs.Add('Θ', "Ô");
            chrs.Add('Δ', "Ð");
            chrs.Add('Φ', "Þ");
            chrs.Add('Γ', "¬");
            chrs.Add('Λ', "Â");
            chrs.Add('Π', "º");
            chrs.Add('Ψ', "Ý");
            chrs.Add('Σ', "Ê");
            chrs.Add('Ξ', "±");

            string ret_str = "";
            foreach (char c in body)
            {
                if (chrs.ContainsKey(c))
                {
                    ret_str += chrs[c];
                }
                else
                {
                    ret_str += c;
                }
            }
            return ret_str;
        }

        public string createMessage(string msisdn, string message)
        {
            /********************************************************************
            * Construct data                                                    *
            *********************************************************************/
            /*
            * Note the suggested encoding for the some parameters, notably
            * the username, password and especially the message.  ISO-8859-1
            * is essentially the character set that we use for message bodies,
            * with a few exceptions for e.g. Greek characters. For a full list,
            * see: http://bulksms.vsms.net/docs/eapi/submission/character_encoding/
            */

            string data = "";
            data += "username=" + HttpUtility.UrlEncode(Server.Properties.Settings.Default.SMSUser, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            data += "&password=" + HttpUtility.UrlEncode(Server.Properties.Settings.Default.SMSPassword, System.Text.Encoding.GetEncoding("ISO-8859-1"));
            data += "&message=" + HttpUtility.UrlEncode(character_resolve(message), System.Text.Encoding.GetEncoding("ISO-8859-1"));
            data += "&msisdn=" + msisdn;
            data += "&want_report=1";

            return data;
        }

        private String getStatus(int code)
        {
            String status = "";
            switch (code)
            {
                case 0: status = "In progress (a normal message submission, with no error encountered so far)."; break;
                case 10: status = "Delivered upstream"; break;
                case 11: status = "Delivered to mobile"; break;
                case 12: status = "Delivered upstream unacknowledged (assume message is in progress)"; break;
                case 22: status = "Internal fatal error"; break;
                case 23: status = "Authentication failure"; break;
                case 24: status = "Data validation failed"; break;
                case 25: status = "You do not have sufficient credits"; break;
                case 26: status = "Upstream credits not available"; break;
                case 27: status = "You have exceeded your daily quota"; break;
                case 28: status = "Upstream quota exceeded"; break;
                case 29: status = "Message sending cancelled"; break;
                case 31: status = "Unroutable"; break;
                case 32: status = "Blocked (probably because of a recipient's complaint against you)"; break;
                case 33: status = "Failed: status = 'censored'"; break;
                case 40: status = "Temporarily unavailable"; break;
                case 50: status = "Delivery failed - generic failure"; break;
                case 51: status = "Delivery to phone failed"; break;
                case 52: status = "Delivery to network failed"; break;
                case 53: status = "Message expired"; break;
                case 54: status = "Failed on remote network"; break;
                case 55: status = "Failed: status = 'remotely blocked (variety of reasons)'"; break;
                case 56: status = "Failed: status = 'remotely censored (typically due to content of message)'"; break;
                case 57: status = "Failed due to fault on handset (e.g. SIM full)"; break;
                case 60: status = "Transient upstream failure (transient)"; break;
                case 61: status = "Upstream status update (transient)"; break;
                case 62: status = "Upstream cancel failed (transient)"; break;
                case 63: status = "Queued for retry after temporary failure delivering (transient)"; break;
                case 64: status = "Queued for retry after temporary failure delivering, due to fault on handset (transient)"; break;
                case 70: status = "Unknown upstream status"; break;
            }
            return status;
        }
    }
}
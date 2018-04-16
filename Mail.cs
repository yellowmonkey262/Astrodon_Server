using OpenPop.Mime;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace Server
{
    public class MailReceiver
    {
        public String hostName { get { return Server.Properties.Settings.Default.MailHost; } }

        public int port { get { return int.Parse(Server.Properties.Settings.Default.MailPort); } }

        public bool useSSL { get { return Server.Properties.Settings.Default.popSSL; } }

        public String username { get { return Server.Properties.Settings.Default.MailUser; } }

        public String password { get { return Server.Properties.Settings.Default.MailPassword; } }

        private const bool saveMsgs = true;
        public int timerInterval = 60000;

        public Dictionary<String, EmailConstruct> messages;
        private List<Message> popMessages;
        private List<MailMessage> mailMessages;
        public System.Timers.Timer timer;

        public MailReceiver(out String msg)
        {
            try
            {
                timer = new System.Timers.Timer(int.Parse(Server.Properties.Settings.Default.SendReceive));
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                msg = "Mail Receiver created " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
        }

        private List<Message> FetchAllMessages(string hostname, int port, bool useSsl, string username, string password)
        {
            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                // Connect to the server
                client.Connect(hostname, port, useSsl);

                // Authenticate ourselves towards the server
                client.Authenticate(username, password);

                // Get the number of messages in the inbox
                int messageCount = client.GetMessageCount();

                // We want to download all messages
                List<Message> allMessages = new List<Message>(messageCount);

                // Messages are numbered in the interval: [1, messageCount]
                // Ergo: message numbers are 1-based. Most servers give the latest message the
                // highest number
                for (int i = messageCount; i > 0; i--)
                {
                    allMessages.Add(client.GetMessage(i));
                }

                // Now return the fetched messages
                return allMessages;
            }
        }

        private void DeleteMessageOnServer(string hostname, int port, bool useSsl, string username, string password, int messageNumber)
        {
            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                // Connect to the server
                client.Connect(hostname, port, useSsl);

                // Authenticate ourselves towards the server
                client.Authenticate(username, password);

                // Mark the message as deleted Notice that it is only MARKED as deleted POP3 requires
                // you to "commit" the changes which is done by sending a QUIT command to the server
                // You can also reset all marked messages, by sending a RSET command.
                client.DeleteMessage(messageNumber);

                // When a QUIT command is sent to the server, the connection between them are closed.
                // When the client is disposed, the QUIT command will be sent to the server just as
                // if you had called the Disconnect method yourself.
            }
        }

        public void ToggleTimer()
        {
            timer.Enabled = !timer.Enabled;
            if (timer.Enabled)
            {
                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Mail Receiver started " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Checking mail: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
                GetMail();
            }
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Checking mail: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
            GetMail();
        }

        private void GetMail()
        {
            timer.Enabled = false;
            popMessages = FetchAllMessages(hostName, port, useSSL, username, password);
            mailMessages = new List<MailMessage>();
            if (popMessages.Count > 0)
            {
                if (messages == null) { messages = new Dictionary<string, EmailConstruct>(); }
                foreach (Message m in popMessages)
                {
                    try
                    {
                        if (!messages.ContainsKey(m.Headers.MessageId))
                        {
                            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Event Fired: MailPopped")); }
                            EmailConstruct e = new EmailConstruct();
                            e.ID = m.Headers.MessageId;
                            MailMessage mm = m.ToMailMessage();
                            String body = mm.Body;
                            e.SentFrom = mm.From.Address.Replace("@2way.co.za", "");
                            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs(e.SentFrom)); }
                            if (mm.To.Count > 0) { e.SentTo = mm.To.ToList()[0].Address; }
                            e.ReceivedDate = m.Headers.DateSent;
                            e.Subject = mm.Subject;
                            messages.Add(e.ID, new EmailConstruct());
                            if (e.Subject == "SMS to email")
                            {
                                #region SMS Emails

                                List<String> refs = References();
                                foreach (String reference in refs)
                                {
                                    if (e.Body.Contains(reference))
                                    {
                                        e.Reference = reference;
                                        e.Body = e.Body.Replace(reference, "");
                                        break;
                                    }
                                }
                                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("2")); }
                                e.ForwardDate = new DateTime(1900, 1, 1);
                                e.ForwardedTo = "";
                                e.HandledDate = new DateTime(1900, 1, 1);
                                e.Handled = false;
                                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Before Save")); }
                                SaveMail(ref e);
                                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("After save")); }
                                String buildingQuery = "SELECT distinct u.username, u.email, sms.customer FROM tblSMS AS sms INNER JOIN tblBuildings AS b ON sms.building = b.Code INNER JOIN";
                                buildingQuery += " tblUserBuildings AS ub ON b.id = ub.buildingid INNER JOIN tblUsers AS u ON ub.userid = u.id";
                                buildingQuery += " WHERE (sms.sender = '" + e.SentFrom + "')";
                                String dsStatus = "";
                                DataSet dsUsers = DataHandler.getData(buildingQuery, out dsStatus);
                                if (dsUsers != null && dsUsers.Tables.Count > 0 && dsUsers.Tables[0].Rows.Count > 0)
                                {
                                    String msgfrom = "Astrodon Debtors System";
                                    String subject = "New Message From - " + dsUsers.Tables[0].Rows[0]["customer"].ToString();
                                    String message = "A new sms message has been received from the above customer.  Please check the Debtor System for more information.";
                                    List<String> toMails = new List<string>();

                                    foreach (DataRow dr in dsUsers.Tables[0].Rows)
                                    {
                                        String toMail = dr["email"].ToString();
                                        if (!String.IsNullOrEmpty(toMail)) { toMails.Add(toMail); }
                                    }

                                    if (MailSender.SendMail(msgfrom, toMails, subject, message, false, null))
                                    {
                                        String sentMsg = String.Format("Message sent to {0}", String.Join(";", toMails.ToArray()));
                                        if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs(sentMsg + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
                                    }
                                }

                                #endregion SMS Emails
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                }
                for (int i = messages.Count; i > 0; i--)
                {
                    DeleteMessageOnServer(hostName, port, useSSL, username, password, i);
                }
            }
            timer.Enabled = true;
        }

        public event EventHandler<MessageArgs> NewMessageEvent;

        public List<String> References()
        {
            List<String> refs = new List<String>();
            String query = "SELECT DISTINCT reference FROM tblSMS ORDER BY sent desc";
            String msg = "";
            DataSet ds = DataHandler.getData(query, out msg);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    refs.Add(dr["reference"].ToString());
                }
            }
            return refs;
        }

        public String GetReference(String number1, String number2)
        {
            String reference = "";
            String query = "SELECT top(1) reference FROM tblSMS WHERE number = @n1 OR number = @n2 ORDER BY sent desc";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@n1", number1);
            sqlParms.Add("@n2", number2);
            String msg = "";
            DataSet ds = DataHandler.getData(query, sqlParms, out msg);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    reference = dr["reference"].ToString();
                }
            }
            return reference;
        }

        private void SaveMail(ref EmailConstruct m)
        {
            String msg = "";
            //first get all references
            String checkNumber = m.SentFrom;
            if (m.SentFrom.StartsWith("27")) { checkNumber = m.SentFrom.Replace("27", "0"); }
            String refQuery = "SELECT distinct building, customer FROM tblSMS where reference = @reference OR number = @actNumber OR number = @checkNumber";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@building", "");
            sqlParms.Add("@customer", "");
            sqlParms.Add("@message", m.Body);
            if (String.IsNullOrEmpty(m.Reference)) { m.Reference = GetReference(checkNumber, m.SentFrom); }
            sqlParms.Add("@reference", m.Reference);
            sqlParms.Add("@sent", m.ReceivedDate);
            sqlParms.Add("@direction", true);
            sqlParms.Add("@sender", m.SentFrom);
            sqlParms.Add("@actNumber", m.SentFrom);
            sqlParms.Add("@checkNumber", checkNumber);

            DataSet ds = DataHandler.getData(refQuery, sqlParms, out msg);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                sqlParms["@building"] = ds.Tables[0].Rows[0]["building"].ToString();
                sqlParms["@customer"] = ds.Tables[0].Rows[0]["customer"].ToString();
                sqlParms["@reference"] = m.Reference;
                sqlParms["@message"] = m.Body;
                String insertQuery = "INSERT INTO tblSMS(building, customer, number, message, reference, sent, sender)";
                insertQuery += " VALUES(@building, @customer, @sender, @message, @reference, @sent, @sender)";
                DataHandler.setData(insertQuery, sqlParms, out msg);
            }
        }

        private void SaveMail(String building, String customer, String message, String sender)
        {
            String msg = "";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@building", building);
            sqlParms.Add("@customer", customer);
            sqlParms.Add("@message", message);
            sqlParms.Add("@reference", "");
            sqlParms.Add("@sent", DateTime.Now);
            sqlParms.Add("@sender", sender);

            String insertQuery = "INSERT INTO tblSMS(building, customer, number, reference, message, billable, bulkbillable, sent, sender, astStatus, batchID, status, nextPolled, pollCount)";
            insertQuery += " VALUES(@building, @customer, @number, @reference, @message, 'False', 'False', @sent, @sender, '1', '', 'Received', @sent, 0)";
            DataHandler.setData(insertQuery, sqlParms, out msg);
        }
    }

    public class Noreplycatcher
    {
        public String hostName { get { return Server.Properties.Settings.Default.MailHost; } }

        public int port { get { return int.Parse(Server.Properties.Settings.Default.MailPort); } }

        public bool useSSL { get { return Server.Properties.Settings.Default.popSSL; } }

        public String username { get { return Server.Properties.Settings.Default.MailUser; } }

        public String password { get { return Server.Properties.Settings.Default.MailPassword; } }

        //public String hostName { get { return "mail.npsa.co.za"; } }
        //public int port { get { return 110; } }
        //public bool useSSL { get { return false; } }
        //public String username { get { return "info@metathought.co.za"; } }
        //public String password { get { return "info01"; } }

        private const bool saveMsgs = true;
        public int timerInterval = 60000;

        public Dictionary<String, EmailConstruct> messages;
        private List<Message> popMessages;
        private List<MailMessage> mailMessages;
        public System.Timers.Timer timer;

        public Noreplycatcher(out String msg)
        {
            try
            {
                timer = new System.Timers.Timer(int.Parse(Server.Properties.Settings.Default.SendReceive));
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                msg = "Mail Receiver created " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
        }

        private List<Message> FetchAllMessages(string hostname, int port, bool useSsl, string username, string password)
        {
            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                // Connect to the server
                client.Connect(hostname, port, useSsl);

                // Authenticate ourselves towards the server
                client.Authenticate(username, password);

                // Get the number of messages in the inbox
                int messageCount = client.GetMessageCount();

                // We want to download all messages
                List<Message> allMessages = new List<Message>(messageCount);

                // Messages are numbered in the interval: [1, messageCount]
                // Ergo: message numbers are 1-based. Most servers give the latest message the
                // highest number
                for (int i = messageCount; i > 0; i--)
                {
                    allMessages.Add(client.GetMessage(i));
                }

                // Now return the fetched messages
                return allMessages;
            }
        }

        private void DeleteMessageOnServer(string hostname, int port, bool useSsl, string username, string password, int messageNumber)
        {
            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                // Connect to the server
                client.Connect(hostname, port, useSsl);

                // Authenticate ourselves towards the server
                client.Authenticate(username, password);

                // Mark the message as deleted Notice that it is only MARKED as deleted POP3 requires
                // you to "commit" the changes which is done by sending a QUIT command to the server
                // You can also reset all marked messages, by sending a RSET command.
                client.DeleteMessage(messageNumber);

                // When a QUIT command is sent to the server, the connection between them are closed.
                // When the client is disposed, the QUIT command will be sent to the server just as
                // if you had called the Disconnect method yourself.
            }
        }

        public void ToggleTimer()
        {
            timer.Enabled = !timer.Enabled;
            if (timer.Enabled)
            {
                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Mail Receiver started " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
                if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Checking mail: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
                GetMail();
            }
        }

        private Dictionary<String, String[]> GetMessages()
        {
            String status;
            String statementQuery = "SELECT id, subject, errorMessage, status FROM tblStatementRun WHERE (sentDate1 IS NOT NULL) AND (errorMessage LIKE 'Processed%') AND (status IS NULL) ORDER BY id";
            String letterQuery = "SELECT id, subject, errorMessage, status FROM tblLetterRun WHERE (sentDate IS NOT NULL) AND (errorMessage LIKE 'Processed%') AND (status IS NULL) ORDER BY id";
            DataSet stmtDS = DataHandler.getData(statementQuery, out status);
            DataSet letterDS = DataHandler.getData(letterQuery, out status);
            Dictionary<String, String[]> outboundMails = new Dictionary<string, string[]>();
            if (stmtDS != null && stmtDS.Tables.Count > 0 && stmtDS.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow stmtDR in stmtDS.Tables[0].Rows)
                {
                    try
                    {
                        String[] details = new string[] { stmtDR["id"].ToString(), "tblStatementRun" };
                        outboundMails.Add(stmtDR["subject"].ToString(), details);
                    }
                    catch { }
                }
            }
            if (letterDS != null && letterDS.Tables.Count > 0 && letterDS.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow letterDR in letterDS.Tables[0].Rows)
                {
                    try
                    {
                        String[] details = new string[] { letterDR["id"].ToString(), "tblLetterRun" };
                        outboundMails.Add(letterDR["subject"].ToString(), details);
                    }
                    catch { }
                }
            }
            return outboundMails;
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Checking mail: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))); }
            GetMail();
        }

        public void GetMail()
        {
            timer.Enabled = false;
            Dictionary<String, String[]> outboundMails = GetMessages();
            String updateQuery = "UPDATE {0} SET status = '{1}' WHERE id = {2}";
            String status;
            popMessages = FetchAllMessages(hostName, port, useSSL, username, password);
            mailMessages = new List<MailMessage>();
            if (popMessages.Count > 0)
            {
                if (messages == null) { messages = new Dictionary<string, EmailConstruct>(); }
                foreach (Message m in popMessages)
                {
                    try
                    {
                        if (!messages.ContainsKey(m.Headers.MessageId))
                        {
                            if (NewMessageEvent != null) { NewMessageEvent(this, new MessageArgs("Event Fired: MailPopped")); }
                            MailMessage mm = m.ToMailMessage();
                            String body = mm.Body;
                            String subject = mm.Subject;
                            bool handled = false;
                            foreach (KeyValuePair<String, String[]> kvp in outboundMails)
                            {
                                if (subject.Contains(kvp.Key))
                                {
                                    String statusQuery = String.Format(updateQuery, kvp.Value[1], subject.Replace(kvp.Key, ""), kvp.Value[0]);
                                    DataHandler.setData(statusQuery, out status);
                                    handled = true;
                                    break;
                                }
                            }
                            if (handled) { outboundMails = GetMessages(); }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                }
                //for (int i = messages.Count; i > 0; i--) {
                //    DeleteMessageOnServer(hostName, port, useSSL, username, password, i);
                //}
            }
            timer.Enabled = true;
        }

        public event EventHandler<MessageArgs> NewMessageEvent;

        public List<String> References()
        {
            List<String> refs = new List<String>();
            String query = "SELECT DISTINCT reference FROM tblSMS ORDER BY sent desc";
            String msg = "";
            DataSet ds = DataHandler.getData(query, out msg);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    refs.Add(dr["reference"].ToString());
                }
            }
            return refs;
        }

        public String GetReference(String number1, String number2)
        {
            String reference = "";
            String query = "SELECT top(1) reference FROM tblSMS WHERE number = @n1 OR number = @n2 ORDER BY sent desc";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@n1", number1);
            sqlParms.Add("@n2", number2);
            String msg = "";
            DataSet ds = DataHandler.getData(query, sqlParms, out msg);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    reference = dr["reference"].ToString();
                }
            }
            return reference;
        }

        private void SaveMail(ref EmailConstruct m)
        {
            String msg = "";
            //first get all references
            String checkNumber = m.SentFrom;
            if (m.SentFrom.StartsWith("27")) { checkNumber = m.SentFrom.Replace("27", "0"); }
            String refQuery = "SELECT distinct building, customer FROM tblSMS where reference = @reference OR number = @actNumber OR number = @checkNumber";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@building", "");
            sqlParms.Add("@customer", "");
            sqlParms.Add("@message", m.Body);
            if (String.IsNullOrEmpty(m.Reference)) { m.Reference = GetReference(checkNumber, m.SentFrom); }
            sqlParms.Add("@reference", m.Reference);
            sqlParms.Add("@sent", m.ReceivedDate);
            sqlParms.Add("@direction", true);
            sqlParms.Add("@sender", m.SentFrom);
            sqlParms.Add("@actNumber", m.SentFrom);
            sqlParms.Add("@checkNumber", checkNumber);

            DataSet ds = DataHandler.getData(refQuery, sqlParms, out msg);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                sqlParms["@building"] = ds.Tables[0].Rows[0]["building"].ToString();
                sqlParms["@customer"] = ds.Tables[0].Rows[0]["customer"].ToString();
                sqlParms["@reference"] = m.Reference;
                sqlParms["@message"] = m.Body;
                String insertQuery = "INSERT INTO tblSMS(building, customer, number, message, reference, sent, sender)";
                insertQuery += " VALUES(@building, @customer, @sender, @message, @reference, @sent, @sender)";
                DataHandler.setData(insertQuery, sqlParms, out msg);
            }
        }

        private void SaveMail(String building, String customer, String message, String sender)
        {
            String msg = "";
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@building", building);
            sqlParms.Add("@customer", customer);
            sqlParms.Add("@message", message);
            sqlParms.Add("@reference", "");
            sqlParms.Add("@sent", DateTime.Now);
            sqlParms.Add("@sender", sender);

            String insertQuery = "INSERT INTO tblSMS(building, customer, number, reference, message, billable, bulkbillable, sent, sender, astStatus, batchID, status, nextPolled, pollCount)";
            insertQuery += " VALUES(@building, @customer, @number, @reference, @message, 'False', 'False', @sent, @sender, '1', '', 'Received', @sent, 0)";
            DataHandler.setData(insertQuery, sqlParms, out msg);
        }
    }

    public class MailSender
    {
        public static String hostName { get { return Server.Properties.Settings.Default.MailHost; } }

        public static String username { get { return Server.Properties.Settings.Default.MailUser; } }

        public static String password { get { return Server.Properties.Settings.Default.MailPassword; } }

        public MailSender()
        {
            // // TODO: Add constructor logic here
        }

        private static String generatHTMLEmail(String requestString, String emailString)
        {
            String html = "";
            html += "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>";
            html += "<html xmlns='http://www.w3.org/1999/xhtml'>";
            html += "<head>";
            html += "<meta http-equiv='Content-Type' content='text/html; charset=iso-8859-1' />";
            html += "<title>" + requestString + "</title>";
            html += "</head>";
            html += "<body>";
            html += "<form id='form1' name='form1' method='post' action=''>";
            html += "<p>";
            html += emailString;
            html += "</p>";
            html += "</form>";
            html += "</body>";
            html += "</html>";
            return html;
        }

        public static bool SendMail(String fromEmail, String toMail, String subject, String message, bool htmlMail, String[] attachments = null)
        {
            //if (fromEmail.ToLower() == "noreply@astrodon.co.za") { fromEmail = "nrp@astrodon.co.za"; }
            String mailBody = "";
            if (htmlMail)
            {
                mailBody = generatHTMLEmail(subject, message);
            }
            else
            {
                mailBody = message;
            }
            try
            {
                SmtpClient smtpClient = new SmtpClient();
                MailMessage objMail = new MailMessage();
                MailAddress objMail_fromaddress = new MailAddress(fromEmail);
                MailAddress objMail_toaddress = new MailAddress(toMail);
                MailAddress cc = new MailAddress(fromEmail);
                objMail.To.Add(objMail_toaddress);
                objMail.CC.Add(cc);
                objMail.From = objMail_fromaddress;
                objMail.IsBodyHtml = htmlMail;
                objMail.Body = mailBody;
                objMail.Priority = MailPriority.High;
                if (attachments != null && attachments.Length > 0)
                {
                    foreach (String attachment in attachments)
                    {
                        objMail.Attachments.Add(new Attachment(attachment));
                    }
                }
                smtpClient.Host = hostName;
                smtpClient.EnableSsl = Server.Properties.Settings.Default.smtpSSL;
                smtpClient.Port = int.Parse(Server.Properties.Settings.Default.smtpPort);
                if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                {
                    smtpClient.Credentials = new NetworkCredential(username, password);
                }
                try
                {
                    objMail.Subject = subject;
                    objMail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.OnSuccess;
                    smtpClient.Send(objMail);
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool SendMail(String fromEmail, String[] toMail, String ccMail, String subject, String message, bool htmlMail, out String status, String[] attachments = null)
        {
            //if (fromEmail.ToLower() == "noreply@astrodon.co.za") { fromEmail = "nrp@astrodon.co.za"; }

            String mailBody = "";
            status = String.Empty;
            if (htmlMail)
            {
                mailBody = generatHTMLEmail(subject, message);
            }
            else
            {
                mailBody = message;
            }
            try
            {
                SmtpClient smtpClient = new SmtpClient();
                MailMessage objMail = new MailMessage();
                MailAddress objMail_fromaddress = new MailAddress(fromEmail);
                foreach (String toAddy in toMail)
                {
                    if (!toAddy.Contains("imp.ad-one.co.za"))
                    {
                        MailAddress objMail_toaddress = new MailAddress(toAddy);
                        objMail.To.Add(objMail_toaddress);
                    }
                }
                objMail.From = objMail_fromaddress;
                objMail.IsBodyHtml = htmlMail;
                objMail.Body = mailBody;
                objMail.Priority = MailPriority.High;
                if (attachments != null && attachments.Length > 0)
                {
                    try
                    {
                        foreach (String attachment in attachments)
                        {
                            objMail.Attachments.Add(new Attachment(attachment));
                        }
                    }
                    catch (Exception ex)
                    {
                        status = ex.Message;
                        return false;
                    }
                }
                smtpClient.Host = hostName;
                smtpClient.EnableSsl = Server.Properties.Settings.Default.smtpSSL;
                smtpClient.Port = int.Parse(Server.Properties.Settings.Default.smtpPort);
                if (Environment.MachineName == "STEPHEN-PC")
                {
                    smtpClient.Host = "mail.npsa.co.za";
                    smtpClient.Credentials = new NetworkCredential("info@metathought.co.za", "info01");
                }
                else
                {
                    smtpClient.Host = "10.0.1.1";
                }
                try
                {
                    objMail.Subject = subject;
                    objMail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.OnSuccess;
                    smtpClient.Send(objMail);
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }
            status = "Mail sent";
            return true;
        }

        public static bool SendMail(String fromEmail, List<String> toMails, String subject, String message, bool htmlMail, String[] attachments = null)
        {
            //if (fromEmail.ToLower() == "noreply@astrodon.co.za") { fromEmail = "nrp@astrodon.co.za"; }

            String mailBody = "";
            if (htmlMail)
            {
                mailBody = generatHTMLEmail(subject, message);
            }
            else
            {
                mailBody = message;
            }
            try
            {
                SmtpClient smtpClient = new SmtpClient();
                MailMessage objMail = new MailMessage();
                MailAddress objMail_fromaddress = new MailAddress(fromEmail);
                foreach (String toMail in toMails)
                {
                    if (!toMail.Contains("imp.ad-one.co.za"))
                    {
                        MailAddress objMail_toaddress = new MailAddress(toMail);
                        objMail.To.Add(objMail_toaddress);
                    }
                }

                objMail.From = objMail_fromaddress;
                objMail.IsBodyHtml = htmlMail;
                objMail.Body = mailBody;
                objMail.Priority = MailPriority.High;
                if (attachments != null && attachments.Length > 0)
                {
                    foreach (String attachment in attachments)
                    {
                        objMail.Attachments.Add(new Attachment(attachment));
                    }
                }
                smtpClient.Host = hostName;
                if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                {
                    smtpClient.Credentials = new NetworkCredential(username, password);
                }
                try
                {
                    objMail.Subject = subject;
                    objMail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.OnSuccess;
                    smtpClient.Send(objMail);
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool SendMail(String fromEmail, String[] toMail, String subject, String message, bool htmlMail, bool addcc, bool readreceipt, out String status, Dictionary<String, byte[]> attachments = null)
        {
            //if (fromEmail.ToLower() == "noreply@astrodon.co.za") { fromEmail = "nrp@astrodon.co.za"; }

            String mailBody = "";
            status = String.Empty;
            if (htmlMail)
            {
                mailBody = generatHTMLEmail(subject, message);
            }
            else
            {
                mailBody = message;
            }
            try
            {
                SmtpClient smtpClient = new SmtpClient();
                MailMessage objMail = new MailMessage();
                MailAddress objMail_fromaddress = new MailAddress(fromEmail);
                try
                {
                    foreach (String emailAddress in toMail)
                    {
                        if (!emailAddress.Contains("@imp.ad-one.co.za"))
                        {
                            MailAddress objMail_toaddress = new MailAddress(emailAddress);
                            objMail.To.Add(objMail_toaddress);
                        }
                    }
                }
                catch
                {
                    status = "Invalid email address";
                    return false;
                }
                objMail.From = objMail_fromaddress;
                objMail.IsBodyHtml = htmlMail;
                objMail.Body = mailBody;
                objMail.Priority = MailPriority.High;
                if (addcc)
                {
                    MailAddress cc = new MailAddress(fromEmail);
                    objMail.CC.Add(cc);
                }
                if (attachments != null && attachments.Count > 0)
                {
                    foreach (KeyValuePair<String, byte[]> attachment in attachments)
                    {
                        try
                        {
                            MemoryStream ms = new MemoryStream(attachment.Value);
                            objMail.Attachments.Add(new Attachment(ms, attachment.Key));
                        }
                        catch
                        {
                            status = "Invalid attachment";
                            continue;
                        }
                    }
                }
                if (Environment.MachineName == "STEPHEN-PC")
                {
                    smtpClient.Host = "mail.npsa.co.za";
                    smtpClient.Credentials = new NetworkCredential("info@metathought.co.za", "info01");
                }
                else
                {
                    smtpClient.Host = "10.0.1.1";
                }

                try
                {
                    objMail.Subject = subject;
                    if (readreceipt)
                    {
                        objMail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                    }
                    smtpClient.Send(objMail);
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }
            return true;
        }

        public static bool SendMail(String fromEmail, String[] toMail, String cc, String bcc, String subject, String message, bool htmlMail, out String status, String[] attachments = null)
        {
            //if (fromEmail.ToLower() == "noreply@astrodon.co.za") { fromEmail = "nrp@astrodon.co.za"; }

            String mailBody = "";
            status = String.Empty;
            mailBody = (htmlMail ? generatHTMLEmail(subject, message) : message);
            try
            {
                SmtpClient smtpClient = new SmtpClient();
                MailMessage objMail = new MailMessage();
                MailAddress objMail_fromaddress = new MailAddress(fromEmail);
                try
                {
                    foreach (String emailAddress in toMail)
                    {
                        if (!emailAddress.Contains("@imp.ad-one.co.za"))
                        {
                            MailAddress objMail_toaddress = new MailAddress(emailAddress);
                            objMail.To.Add(objMail_toaddress);
                        }
                    }
                }
                catch
                {
                    status = "Invalid email address";
                    return false;
                }
                objMail.From = objMail_fromaddress;
                objMail.IsBodyHtml = htmlMail;
                objMail.Body = mailBody;
                objMail.Priority = MailPriority.High;
                if (cc.Trim() != "")
                {
                    String[] ccMail = cc.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String ccAddy in ccMail)
                    {
                        MailAddress ccAddress = new MailAddress(ccAddy.Trim());
                        objMail.CC.Add(ccAddress);
                    }
                }
                if (bcc.Trim() != "")
                {
                    String[] bccMail = bcc.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String bccAddy in bccMail)
                    {
                        MailAddress bccAddress = new MailAddress(bccAddy.Trim());
                        objMail.Bcc.Add(bccAddress);
                    }
                }
                if (attachments != null && attachments.Length > 0)
                {
                    foreach (String attachment in attachments)
                    {
                        objMail.Attachments.Add(new Attachment(attachment));
                    }
                }
                if (Environment.MachineName == "STEPHEN-PC")
                {
                    smtpClient.Host = "mail.npsa.co.za";
                    smtpClient.Credentials = new NetworkCredential("info@metathought.co.za", "info01");
                }
                else
                {
                    smtpClient.Host = "10.0.1.1";
                }

                try
                {
                    objMail.Subject = subject;
                    smtpClient.Send(objMail);
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }
            return true;
        }
    }

    public class EmailConstruct
    {
        public String ID { get; set; }

        public DateTime ReceivedDate { get; set; }

        public String SentFrom { get; set; }

        public String SentTo { get; set; }

        public String Subject { get; set; }

        public String Body { get; set; }

        public String ForwardedTo { get; set; }

        public DateTime ForwardDate { get; set; }

        public DateTime HandledDate { get; set; }

        public String Reference { get; set; }

        public bool Handled { get; set; }
    }

    public class MessageArgs : EventArgs
    {
        public String message { get; set; }

        public MessageArgs(String msg)
        {
            message = msg;
        }
    }

    public delegate void ClientMessageEventHandler(object sender, MessageArgs e);

    public class ReplyMail
    {
        public int id { get; set; }

        public String subject { get; set; }

        public String tableName { get; set; }
    }
}
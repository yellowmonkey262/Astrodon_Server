using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Statement
    {
        private System.Timers.Timer timer;
        private System.Timers.Timer timer2;
        private System.Timers.Timer timer3;
        private String status = String.Empty;
        private DateTime nextCheckDate = DateTime.MinValue;

        public event MessageHandler Message;

        private bool isRunning = false;

        public delegate void MessageHandler(object sender, PastelArgs e);

     //   private UpdateTrustees updTrust;

        public Statement()
        {
          //  updTrust = new UpdateTrustees();
            timer = new System.Timers.Timer(600000); //3600000
            timer2 = new System.Timers.Timer(600000); //3600000
            timer3 = new System.Timers.Timer(3600000);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
            timer_Elapsed(null, null);
            timer2.Elapsed += timer2_Elapsed;
            timer2.Enabled = true;
            timer3.Elapsed += timer3_Elapsed;
            timer3.Enabled = true;
        }

        private void timer3_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer3.Enabled = false;
            if (DateTime.Now.Hour != 17)
            {
                SendImmediateLetters();
                SendBulkMails(true, false);
            }
            timer3.Enabled = true;
        }

        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now <= nextCheckDate && DateTime.Now.Hour == 6)
            {
                nextCheckDate = DateTime.Now.AddDays(1);
                DataSet dsUnsent = DataHandler.getData(UnsentEmailQuery, out status);
                String message = "Unsent Email Report: " + DateTime.Now.ToString() + ".<br/><br/>";
                if (dsUnsent != null && dsUnsent.Tables.Count > 0 && dsUnsent.Tables[0].Rows.Count > 0)
                {
                    message += "Total Count: " + dsUnsent.Tables[0].Rows.Count.ToString() + ".<br/><br/>";
                    message += "<table>";
                    message += "<tr><td>Building Code</td><td>Unit No</td><td>Email</td><td>Queue Date</td><td>Subject</td><td>Document Type</td></tr>";
                    foreach (DataRow dr in dsUnsent.Tables[0].Rows)
                    {
                        message += "<tr><td>" + dr["Building"].ToString() + "</td><td>" + dr["Unit"].ToString() + "</td><td>" + dr["Email"].ToString() + "</td>";
                        message += "<td>" + dr["Queued"].ToString() + "</td><td>" + dr["Subject"].ToString() + "</td><td>" + dr["Type"].ToString() + "</td></tr>";
                    }
                    message += "</table>";
                }
                else
                {
                    message += "No unsent emails.";
                }
                String[] sMailAddys = new String[] { "sheldon@astrodon.co.za", "tertia@astrodon.co.za" };
                List<String> addies = new List<string>();
                addies.AddRange(sMailAddys);
                MailSender.SendMail("noreply@astrodon.co.za", addies, "Unsent Mail Report", message, true, null);
            }
        }

        private String UnsentEmailQuery
        {
            get
            {
                String query = "SELECT b.Code AS Building, mr.accNo AS Unit, mr.recipient AS Email, mr.queueDate AS Queued, m.subject AS Subject, 'Bulk Mail' AS Type";
                query += " FROM tblMsgRecipients AS mr INNER JOIN tblMsg AS m ON mr.msgID = m.id INNER JOIN tblBuildings AS b ON m.buildingID = b.id ";
                query += " WHERE (mr.sentDate IS NULL)";
                query += " UNION ALL";
                query += " SELECT STUFF(unitno, PATINDEX('%[^a-z]%', unitno), 5, '') AS Building, unitno AS Unit, toEmail AS Email, queueDate AS Queued, ";
                query += " subject AS Subject, 'Letter' AS Type FROM tblLetterRun WHERE (sentDate IS NULL)";
                query += " UNION ALL";
                query += " SELECT STUFF(unit, PATINDEX('%[^a-z]%', unit), 5, '') AS Building, unit AS Unit, email1 AS Email, queueDate AS Queued, subject AS Subject, ";
                query += " 'Statement' AS Type FROM tblStatementRun WHERE (sentDate1 IS NULL) ";
                query += " ORDER BY Unit, Queued";
                return query;
            }
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Enabled = false;
            if (!isRunning)
            {
                if ((DateTime.Now.Hour == 17) || (Properties.Settings.Default.statementrunning))
                {
                    SendStatements();
                    SendLetters();
                    SendBulkMails(true, true);
                    Purge();
                    //updTrust.Update();
                }
            }
            timer.Enabled = true;
        }

        public void UpdateCustomers()
        {
            //String buildQ = "SELECT Building, Code, DataPath FROM tblBuildings WHERE isBuilding = 'True' ORDER by Building";
            //String status = String.Empty;
            //DataSet ds = DataHandler.getData(buildQ, out status);
            //List<CustomerConstruct> allCustomers = new List<CustomerConstruct>();
            //if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            //{
            //    foreach (DataRow dr in ds.Tables[0].Rows)
            //    {
            //        String buildingName = dr["Building"].ToString();
            //        String buildingAbbr = dr["Code"].ToString();
            //        List<Customer> customers = frmMain.pastel.AddCustomers("", dr["DataPath"].ToString());
            //        foreach (Customer c in customers)
            //        {
            //            CustomerConstruct cc = new CustomerConstruct();
            //            cc.buildingName = buildingName;
            //            cc.buildingAbbr = buildingAbbr;
            //            cc.acc = c.accNumber;
            //            cc.emails = c.Email;
            //            allCustomers.Add(cc);
            //        }
            //    }
            //    RaiseEvent(allCustomers.Count.ToString() + " customers retrieved from pastel");
            //    int updateCount = 0;
            //    foreach (CustomerConstruct cc in allCustomers)
            //    {
            //        if (status != "OK") { RaiseEvent(status); } else { updateCount++; }
            //    }
            //    RaiseEvent(updateCount.ToString() + " customers updated");
            //}
            //RaiseEvent("Customer update complete");
        }

        public void Purge()
        {
            String statementFolder = "K:\\Pastel11\\Debtors System\\statements";
            String letterFolder = "K:\\Pastel11\\Debtors System\\Letters";
            string[] filePaths = Directory.GetFiles(statementFolder);
            for (int i = 0; i < filePaths.Length; i++)
            {
                String file = filePaths[i];
                if (File.GetLastWriteTime(file) < DateTime.Now.AddDays(-2))
                {
                    File.Delete(file);
                }
            }
            filePaths = Directory.GetFiles(letterFolder);
            for (int i = 0; i < filePaths.Length; i++)
            {
                String file = filePaths[i];
                if (File.GetLastWriteTime(file) < DateTime.Now.AddDays(-2))
                {
                    File.Delete(file);
                }
            }
        }

        public void GenerateCustomerList()
        {
            String buildQ = "SELECT Building, Code, DataPath FROM tblBuildings WHERE isBuilding = 'True'";
            String status = String.Empty;
            DataSet ds = DataHandler.getData(buildQ, out status);
            List<String> customerEntries = new List<string>();
            customerEntries.Add("buildingName,abbr,acc,email");
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    String buildingName = dr["Building"].ToString();
                    String buildingAbbr = dr["Code"].ToString();
                    List<Customer> customers = frmMain.pastel.AddCustomers("", dr["DataPath"].ToString());
                    foreach (Customer c in customers)
                    {
                        String email = "";
                        foreach (String ce in c.Email)
                        {
                            if (ce.Contains("@") && !ce.Contains("imp.ad-one"))
                            {
                                email += ce.Replace(",", "") + ",";
                            }
                        }
                        String customerEntry = buildingName + "," + buildingAbbr + "," + c.accNumber + (email != "" ? "," + email : "");
                        if (customerEntry.EndsWith(",")) { customerEntry = customerEntry.Replace(customerEntry, customerEntry.Substring(0, customerEntry.Length - 1)); }
                        customerEntries.Add(customerEntry);
                    }
                }
            }
            StreamWriter sw = new StreamWriter("customerlist.csv");
            foreach (String customerEntry in customerEntries)
            {
                sw.WriteLine(customerEntry);
            }
            sw.Flush();
            sw.Close();
            RaiseEvent("Customer list complete");
        }

        private String CustomerMessage(String accNumber, String debtorEmail)
        {
            String message = "Dear Owner," + Environment.NewLine + Environment.NewLine;
            message += "Due to a technical error, please find attached your amended statement." + Environment.NewLine;
            message += "We apologise for any inconvenience caused due to the error." + Environment.NewLine + Environment.NewLine;
            message += "Remember, you can access your statements online. Paste the link below into your browser to access your online statements." + Environment.NewLine + Environment.NewLine;
            message += "www.astrodon.co.za" + Environment.NewLine + Environment.NewLine;
            message += "Regards" + Environment.NewLine + Environment.NewLine;
            message += "Astrodon (Pty) Ltd" + Environment.NewLine;
            message += "You're in Good Hands" + Environment.NewLine + Environment.NewLine;
            message += "Account #: " + accNumber + " For any queries on your statement, please email:" + debtorEmail + Environment.NewLine + Environment.NewLine;
            message += "Do not reply to this e-mail address";

            return message;
        }

        private String OrdinaryMessage(String accNumber, String debtorEmail, bool rental = false)
        {
            String message = "Dear " + (rental ? "tenant" : "owner") + "," + Environment.NewLine + Environment.NewLine;
            message += "Attached please find your statement." + Environment.NewLine + Environment.NewLine;
            message += "Account #: " + accNumber + " For any queries on your statement, please email:" + debtorEmail + Environment.NewLine + Environment.NewLine;
            message += "Do not reply to this e-mail address" + Environment.NewLine + Environment.NewLine;
            message += RegisterMessage();
            message += "Regards" + Environment.NewLine + Environment.NewLine;
            message += "Astrodon (Pty) Ltd" + Environment.NewLine;
            message += "You're in Good Hands" + Environment.NewLine + Environment.NewLine;

            return message;
        }

        private String RegisterMessage()
        {
            String message = "Did you know that you can log on to our website and download historic levy statements, Conduct Rules, Insurance information, newsletters and much more?" + Environment.NewLine + Environment.NewLine;
            message += "Simply register at www.astrodon.co.za & follow the below instructions:" + Environment.NewLine + Environment.NewLine;
            message += "1.  On the homepage, please click on the \"Registration for Owner\" button." + Environment.NewLine + Environment.NewLine;
            message += "2.  Choose the option to \"register as owner\"." + Environment.NewLine + Environment.NewLine;
            message += "3.  Type in your email address and click \"next\"." + Environment.NewLine + Environment.NewLine;
            message += "You will receive a confirmation email with a link that you can follow to finish the registration process.  If you click next and you get a message telling ";
            message += "you that your email is not found on the database, please contact your debtor controller at Astrodon to double check that we have your correct email address on our system." + Environment.NewLine + Environment.NewLine;
            message += "Once registered, you can log onto the site by typing your email address and password in the blocks provided on the top right hand corner of the page." + Environment.NewLine + Environment.NewLine;
            message += "After you have logged on, you will be directed to the \"My Astrodon\" page where you can access your statements and any other information that has been uploaded for your complex." + Environment.NewLine + Environment.NewLine;
            message += "Investor owners with more that one unit will be happy to know that you will use the same login details for all your units, simply click on the drop down box at the top of the \"My Astrodon\" page and select the unit you wish to view." + Environment.NewLine + Environment.NewLine;
            message += "If you experience any problems with this service, please contact our offices so that we can assist you." + Environment.NewLine + Environment.NewLine;
            message += "Just another way we try to make your life easier." + Environment.NewLine + Environment.NewLine;
            return message;
        }

        private System.Timers.Timer statusTimer;
        private DateTime currentDate;

        private void SetStartStatements()
        {
            currentDate = DateTime.Now;
            if (!GetStmtStatus())
            {
                String query = "UPDATE tblRunConfig SET stmtRunStatus = 'True'";
                String msg = "";
                DataHandler.setData(query, out msg);
                RaiseEvent("Starting statements...");
            }
            else
            {
                RaiseEvent("Statements already running!");
            }
            statusTimer.Start();
        }

        private bool GetStmtStatus()
        {
            String query = "SELECT stmtRunStatus FROM tblRunConfig";
            DataSet ds = DataHandler.getData(query, out status);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return bool.Parse(ds.Tables[0].Rows[0]["stmtRunStatus"].ToString());
            }
            else
            {
                return false;
            }
        }

  
        public void SendStatements()
        {
            isRunning = true;
            statusTimer = new System.Timers.Timer(2000);
            statusTimer.Elapsed += statusTimer_Elapsed;
            currentDate = DateTime.Now;
            //SetStartStatements();
            return; //disable sending statements


            //#region Old Send

            //try
            //{
            //    if (!Properties.Settings.Default.statementrunning)
            //    {
            //        Properties.Settings.Default.statementrunning = true;
            //        Properties.Settings.Default.Save();
            //    }
            //}
            //catch { }
            //DataSet queue = GetQueuedStatements();
            //int processed = 0;
            //int tobeprocessed = 0;
            //if (queue != null && queue.Tables.Count > 0 && queue.Tables[0].Rows.Count > 0)
            //{
            //    tobeprocessed = queue.Tables[0].Rows.Count;
            //    RaiseEvent("Mail count = " + queue.Tables[0].Rows.Count.ToString(), "SendStatements");
            //    foreach (DataRow qDR in queue.Tables[0].Rows)
            //    {
            //        try
            //        {
            //            String id = qDR["id"].ToString();
            //            String[] email1 = qDR["email1"].ToString().Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            //            String sentDate1 = qDR["sentDate1"].ToString();
            //            String actFile = qDR["fileName"].ToString();
            //            bool isRentalStatement = actFile.ToUpper().EndsWith("_R.PDF");
            //            String fileName = GetAttachment(actFile, true);
            //            String actFile2 = actFile;
            //            String debtorEmail = qDR["debtorEmail"].ToString();
            //            String accNo = qDR["unit"].ToString();
            //            String attachment = qDR["attachment"].ToString();
            //            String subject = qDR["subject"].ToString();
            //            List<String> attachments = new List<string>();
            //            attachments.Add(fileName);
            //            if (attachment != "none")
            //            {
            //                String absAttachmentName = GetAttachment(attachment, true);
            //                attachments.Add(absAttachmentName);
            //            }

            //            if (email1.Length > 0 && String.IsNullOrEmpty(sentDate1))
            //            {
            //                status = String.Empty;

            //                String[] attach = attachments.ToArray();
            //                if (attach.Length > 0)
            //                {
            //                    if (MailSender.SendMail("noreply@astrodon.co.za", email1, debtorEmail, subject, OrdinaryMessage(accNo, debtorEmail, isRentalStatement), false, out status, attach))
            //                    {
            //                        if (!String.IsNullOrEmpty(status)) { RaiseEvent(status, "SendStatements"); }
            //                        String update1 = "UPDATE tblStatementRun SET sentDate1 = getDate(), errorMessage = 'Processed & Sent' WHERE id = " + id;
            //                        DataHandler.setData(update1, out status);
            //                    }
            //                    else
            //                    {
            //                        String update1 = "UPDATE tblStatementRun SET sentDate1 = getDate(), errorMessage = 'Error: " + status + "' WHERE id = " + id;
            //                        DataHandler.setData(update1, out status);
            //                        RaiseEvent(status, "SendStatements");
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                String update1 = "UPDATE tblStatementRun SET sentDate1 = getDate(), errorMessage = 'Processed' WHERE id = " + id;
            //                DataHandler.setData(update1, out status);
            //                RaiseEvent("Can't send mail", "SendStatements");
            //            }
            //            processed += 1;
            //        }
            //        catch (Exception ex)
            //        {
            //            RaiseEvent(ex.Message + " " + ex.StackTrace, "SendStatements");
            //        }
            //    }
            //}
            //try
            //{
            //    if (tobeprocessed == processed)
            //    {
            //        Properties.Settings.Default.statementrunning = false;
            //        Properties.Settings.Default.Save();
            //    }
            //}
            //catch { }
            //isRunning = false;

            //#endregion Old Send
        }

        private void statusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            statusTimer.Stop();
            if (GetOutstandingStmts() > 0)
            {
                List<String> msgs = GetSentStatements();
                foreach (String msg in msgs) { RaiseEvent(msg); }
                statusTimer.Start();
            }
            else
            {
                RaiseEvent("All statements sent!");
            }
        }

        private List<String> GetSentStatements()
        {
            List<String> stmtMsg = new List<string>();
            String query = "SELECT unit, sentDate1, errorMessage FROM tblStatementRun WHERE sentDate1 >= '" + currentDate.ToString() + "'";
            DataSet ds = DataHandler.getData(query, out status);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    String msg = "Unit " + dr["unit"].ToString() + " " + dr["errorMessage"].ToString() + " - " + dr["sentDate1"].ToString();
                    stmtMsg.Add(msg);
                }
            }
            currentDate = DateTime.Now;
            return stmtMsg;
        }

        private int GetOutstandingStmts()
        {
            try
            {
                String query = "SELECT count(id) as osStmt FROM tblStatementRun WHERE sentDate1 is null";
                DataSet dsOS = DataHandler.getData(query, out status);
                return int.Parse(dsOS.Tables[0].Rows[0]["osStmt"].ToString());
            }
            catch
            {
                return -1;
            }
        }

        public void SendLetters()
        {
            DataSet queue = GetQueuedLetters(false);
            if (queue != null && queue.Tables.Count > 0 && queue.Tables[0].Rows.Count > 0)
            {
                RaiseEvent("Mail count = " + queue.Tables[0].Rows.Count.ToString(), "SendLetters");
                foreach (DataRow qDR in queue.Tables[0].Rows)
                {
                    try
                    {
                        String id = qDR["id"].ToString();
                        String[] email1 = qDR["toEmail"].ToString().Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        String subject = qDR["subject"].ToString();
                        String message = qDR["message"].ToString();
                        bool html = bool.Parse(qDR["html"].ToString());
                        bool isPA = html;
                        html = false;
                        bool addcc = bool.Parse(qDR["html"].ToString());
                        bool readreceipt = bool.Parse(qDR["html"].ToString());
                        String[] attachments = qDR["attachment"].ToString().Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < attachments.Length; i++)
                        {
                            if (attachments[i].Contains("PA Attachments"))
                            {
                                RaiseEvent("Original file name = " + attachments[i], "SendLetters");
                                attachments[i] = attachments[i].Replace("K:\\Debtors System\\PA Attachments\\", @"K:\\Pastel11\\Debtors System\\PA Attachments\\");
                            }
                        }
                        String[] files = new string[attachments.Length];
                        String accNo = qDR["unitno"].ToString();
                        String debtorEmail = qDR["fromEmail"].ToString();
                        String sentDate = qDR["sentDate"].ToString();
                        String cc = qDR["cc"].ToString();
                        String bcc = qDR["bcc"].ToString();
                        for (int i = 0; i < attachments.Length; i++)
                        {
                            String attachment = attachments[i];
                            String fileName = GetLetter(attachment);
                            files[i] = fileName;
                            attachments[i] = fileName;
                            String actFileTitle = Path.GetFileNameWithoutExtension(fileName);
                            String actFile = Path.GetFileName(fileName);
                        }
                        if (email1.Length > 0 && String.IsNullOrEmpty(sentDate))
                        {
                            try
                            {
                                status = String.Empty;
                                if (isPA && MailSender.SendMail(debtorEmail, email1, cc, bcc, subject, message, html, out status, attachments))
                                {
                                    if (!String.IsNullOrEmpty(status)) { RaiseEvent(status); }
                                    String update1 = "UPDATE tblLetterRun SET sentDate = getDate(), errorMessage = 'Processed & Sent' WHERE id = " + id;
                                    DataHandler.setData(update1, out status);
                                }
                                else if (MailSender.SendMail("noreply@astrodon.co.za", email1, addcc ? debtorEmail : String.Empty, subject, message, html, out status, files))
                                {
                                    if (!String.IsNullOrEmpty(status)) { RaiseEvent(status); }
                                    String update1 = "UPDATE tblLetterRun SET sentDate = getDate(), errorMessage = 'Processed & Sent' WHERE id = " + id;
                                    DataHandler.setData(update1, out status);
                                }
                                else
                                {
                                    String update1 = "UPDATE tblLetterRun SET sentDate = getDate(), errorMessage = 'Error: " + FixError(status) + "' WHERE id = " + id;
                                    RaiseEvent("Error sending email " + status);
                                    DataHandler.setData(update1, out status);
                                }
                            }
                            catch (Exception ex)
                            {
                                RaiseEvent("Update tblLetterRun: " + ex.Message + " " + ex.StackTrace, "SendLetters");
                            }
                        }
                        else
                        {
                            RaiseEvent("Can't send mail");
                            String update1 = "UPDATE tblLetterRun SET sentDate = getDate(), errorMessage = 'Error: No email address' WHERE id = " + id;
                            DataHandler.setData(update1, out status);
                        }
                    }
                    catch (Exception ex)
                    {
                        RaiseEvent(ex.Message + " " + ex.StackTrace, "SendLetters");
                    }
                }
            }
        }

        private string FixError(string status)
        {
            return status.Replace("'", "").Replace("@","");
        }

        public void SendImmediateLetters()
        {
            DataSet queue = GetQueuedLetters(true);
            if (queue != null && queue.Tables.Count > 0 && queue.Tables[0].Rows.Count > 0)
            {
                RaiseEvent("Mail count = " + queue.Tables[0].Rows.Count.ToString());
                foreach (DataRow qDR in queue.Tables[0].Rows)
                {
                    try
                    {
                        String id = qDR["id"].ToString();
                        String[] email1 = qDR["toEmail"].ToString().Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        String subject = qDR["subject"].ToString();
                        String message = qDR["message"].ToString();
                        bool html = bool.Parse(qDR["html"].ToString());
                        bool isPA = html;
                        html = false;
                        bool addcc = bool.Parse(qDR["html"].ToString());
                        bool readreceipt = bool.Parse(qDR["html"].ToString());
                        String[] attachments = qDR["attachment"].ToString().Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < attachments.Length; i++)
                        {
                            if (attachments[i].Contains("PA Attachments"))
                            {
                                RaiseEvent("Original file name = " + attachments[i], "SendImmediateLetters");
                                attachments[i] = attachments[i].Replace("K:\\Debtors System\\PA Attachments\\", @"K:\\Pastel11\\Debtors System\\PA Attachments\\");
                            }
                        }
                        String[] files = new string[attachments.Length];
                        String accNo = qDR["unitno"].ToString();
                        String debtorEmail = qDR["fromEmail"].ToString();
                        String sentDate = qDR["sentDate"].ToString();
                        String cc = qDR["cc"].ToString();
                        String bcc = qDR["bcc"].ToString();
                        for (int i = 0; i < attachments.Length; i++)
                        {
                            String attachment = attachments[i];
                            String fileName = GetLetter(attachment);
                            files[i] = fileName;
                            attachments[i] = fileName;
                            String actFileTitle = Path.GetFileNameWithoutExtension(fileName);
                            String actFile = Path.GetFileName(fileName);
                        }

                        if (email1.Length > 0 && String.IsNullOrEmpty(sentDate))
                        {
                            status = String.Empty;
                            if (isPA && MailSender.SendMail(debtorEmail, email1, cc, bcc, subject, message, html, out status, attachments))
                            {
                                if (!String.IsNullOrEmpty(status)) { RaiseEvent(status); }
                                String update1 = "UPDATE tblLetterRun SET sentDate = getDate(), errorMessage = 'Processed & Sent' WHERE id = " + id;
                                DataHandler.setData(update1, out status);
                            }
                            else if (MailSender.SendMail("noreply@astrodon.co.za", email1, addcc ? debtorEmail : String.Empty, subject, message, html, out status, files))
                            {
                                if (!String.IsNullOrEmpty(status)) { RaiseEvent(status); }
                                String update1 = "UPDATE tblLetterRun SET sentDate = getDate(), errorMessage = 'Processed & Sent' WHERE id = " + id;
                                DataHandler.setData(update1, out status);
                            }
                            else
                            {
                                String update1 = "UPDATE tblLetterRun SET sentDate = getDate(), errorMessage = 'Error: " + status + "' WHERE id = " + id;
                                DataHandler.setData(update1, out status);
                                RaiseEvent(status);
                            }
                        }
                        else
                        {
                            RaiseEvent("Can't send mail", "SendImmediateLetters");
                            String update1 = "UPDATE tblLetterRun SET sentDate = getDate(), errorMessage = 'Error: No email address' WHERE id = " + id;
                            DataHandler.setData(update1, out status);
                        }
                    }
                    catch (Exception ex)
                    {
                        RaiseEvent(ex.Message + " " + ex.StackTrace, "SendImmediateLetters");
                    }
                }
            }
        }

        private void RaiseEvent(String message,string callerMethod = "")
        {
            if (Message != null) { Message(this, new PastelArgs(callerMethod + " " + message)); }
        }

        private DataSet GetQueuedStatements()
        {
            DataSet ds = null;
            while (ds == null)
            {
                try
                {
                    //first remove duplicates
                    String dateQ = "SELECT min(queueDate) FROM tblStatementRun WHERE sentdate1 is null";
                    DataSet dateDS = DataHandler.getData(dateQ, out status);
                    try
                    {
                        if (dateDS != null && dateDS.Tables.Count > 0 && dateDS.Tables[0].Rows.Count > 0)
                        {
                            DateTime minQ = DateTime.Parse(dateDS.Tables[0].Rows[0][0].ToString());
                            String unitQ = "SELECT MAX(id) AS maxid, unit FROM tblStatementRun WHERE (queueDate >= '" + minQ.ToString() + "') GROUP BY unit ORDER BY unit";
                            DataSet unitDS = DataHandler.getData(unitQ, out status);
                            if (unitDS != null && unitDS.Tables.Count > 0 && unitDS.Tables[0].Rows.Count > 0)
                            {
                                foreach (DataRow drUnit in unitDS.Tables[0].Rows)
                                {
                                    String unit = drUnit["unit"].ToString();
                                    String maxid = drUnit["maxid"].ToString();
                                    String delDupesQ = "DELETE FROM tblStatementRun WHERE unit = '" + unit + "' AND id <> = " + maxid + " AND queueDate >= '" + minQ.ToString() + "'";
                                }
                            }
                        }
                    }
                    catch { }
                    String query = "SELECT id, email1, fileName, debtorEmail, sentDate1, unit, attachment, subject FROM tblStatementRun WHERE (sentDate1 is null)";
                    ds = DataHandler.getData(query, out status);
                }
                catch (InsufficientMemoryException e)
                {
                    Thread.Sleep(1000);
                }
            }
            return ds;
        }

        private DataSet GetCompletedStatements()
        {
            String query = "SELECT id, email1, fileName, debtorEmail, sentDate1, unit, attachment, subject FROM tblStatementRun WHERE (sentDate1 >= '" + currentDate.ToString("yyyy/MM/dd") + "')";
            DataSet ds = DataHandler.getData(query, out status);
            return ds;
        }

        private DataSet GetCompletedStatements(DateTime filterDate)
        {
            String query = "SELECT id, email1, fileName, debtorEmail, sentDate1, unit, attachment, subject FROM tblStatementRun WHERE (sentDate1 >= '" + filterDate.ToString("yyyy/MM/dd") + "')";
            DataSet ds = DataHandler.getData(query, out status);
            return ds;
        }

        private DataSet GetQueuedLetters(bool immediate)
        {
            String query = "SELECT id, fromEmail, toEmail, subject, message, html, addcc, readreceipt, attachment, queueDate, sentDate, unitno, cc, bcc ";
            query += " FROM tblLetterRun WHERE (sentDate is null)";
            if (immediate)
            {
                query += " AND (processDate IS NOT NULL AND processDate <= getdate())";
            }
            DataSet ds = DataHandler.getData(query, out status);
            return ds;
        }

        private String GetAttachment(String fileName, bool isStatement)
        {
            //C:\Pastel11\Debtors System\statements
            String serverDrive = @"K:\Pastel11\Debtors System";
            String statementFolder = (isStatement ? "statements" : "letters");
            //String serverDrive = AppDomain.CurrentDomain.BaseDirectory;
            String folderPath = Path.Combine(serverDrive, statementFolder);
            String absFileName = Path.Combine(folderPath, fileName);
            RaiseEvent(absFileName);
            if (File.Exists(absFileName))
            {
                return absFileName;
            }
            else
            {
                return String.Empty;
            }
        }

        private String GetLetter(String fileName)
        {
            //C:\Pastel11\Debtors System\statements
            string filePath = Path.GetFileName(fileName);

            String serverDrive = @"K:\Pastel11\Debtors System";
            String statementFolder = "Letters";
            if (fileName.Contains(@"PA Attachments"))
            {
                statementFolder = "PA Attachments";
            }
            fileName = fileName.Replace("K:\\", "");
            String folderPath = Path.Combine(serverDrive, statementFolder);
            String absFileName = Path.Combine(folderPath, filePath);
            RaiseEvent("Reading: " + absFileName);
            if (File.Exists(absFileName))
            {
                return absFileName;
            }
            else
            {
                RaiseEvent("GetLetter: File not found : " + absFileName);
                return String.Empty;
            }
        }

        public void SendBulkMails(bool flagQueued = false, bool queued = false)
        {
            String mailQuery = "SELECT msg.id, msg.fromAddress, b.Code, b.DataPath, msg.incBCC, msg.bccAddy, msg.subject, msg.message, msg.billBuilding, msg.billAmount FROM tblMsg AS msg ";
            mailQuery += " INNER JOIN tblBuildings AS b ON msg.buildingID = b.id WHERE (msg.id IN (SELECT DISTINCT msgID FROM tblMsgRecipients WHERE(sentDate IS NULL))) ";
            if (flagQueued) { mailQuery += " AND msg.queue = '" + queued + "'"; }
            DataHandler dh = new DataHandler();
            String status = String.Empty;
            DataSet ds = DataHandler.getData(mailQuery, out status);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int msgID = int.Parse(dr["id"].ToString());
                    String bCode = dr["Code"].ToString();
                    String dataPath = dr["DataPath"].ToString();
                    bool incBCC = bool.Parse(dr["incBCC"].ToString());
                    String fromAddress = dr["fromAddress"].ToString();
                    String bccAddy = dr["bccAddy"].ToString();
                    String subject = dr["subject"].ToString();
                    String message = dr["message"].ToString();
                    bool billBuilding = bool.Parse(dr["billBuilding"].ToString());
                    double billAmount = double.Parse(dr["billAmount"].ToString());
                    String attachmentQuery = "SELECT Name, Data FROM tblMsgData WHERE msgID = " + msgID.ToString();
                    DataSet dsAttachment = DataHandler.getData(attachmentQuery, out status);
                    Dictionary<String, byte[]> attachments = new Dictionary<string, byte[]>();
                    if (dsAttachment != null && dsAttachment.Tables.Count > 0 && dsAttachment.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow drA in dsAttachment.Tables[0].Rows)
                        {
                            try
                            {
                                if (!attachments.ContainsKey(drA["Name"].ToString()))
                                {
                                    attachments.Add(drA["Name"].ToString(), (byte[])drA["Data"]);
                                }
                            }
                            catch { }
                        }
                    }
                    String billableCustomersQuery = "SELECT distinct accNo FROM tblMsgRecipients WHERE billCustomer = 'True' and msgID = " + msgID.ToString();
                    String allRecipientsQuery = "SELECT id, accNo, recipient FROM tblMsgRecipients WHERE sentDate is null AND msgID = " + msgID.ToString();
                    DataSet billableCustomers = DataHandler.getData(billableCustomersQuery, out status);
                    DataSet receivers = DataHandler.getData(allRecipientsQuery, out status);
                    Dictionary<String, bool> emails = new Dictionary<string, bool>();
                    if (receivers != null && receivers.Tables.Count > 0 && receivers.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow rrece in receivers.Tables[0].Rows)
                        {
                            try
                            {
                                String id = rrece["id"].ToString();
                                String[] emailAddys = rrece["recipient"].ToString().Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                                bool success = MailSender.SendMail(fromAddress, emailAddys, subject, message, false, false, false, out status, attachments);
                                String updQuery = "UPDATE tblMsgRecipients SET sentDate = '" + DateTime.Now.ToString() + "' WHERE id = " + id;
                                RaiseEvent(String.Join(";", emailAddys) + " - " + status);
                                DataHandler.setData(updQuery, out status);
                                if (status != "") { RaiseEvent(updQuery + " - " + status); }
                                try { emails.Add(rrece["accNo"].ToString(), success); } catch { }
                            }
                            catch { }
                        }
                        String bulkUpdateQuery = "UPDATE tblMsgRecipients SET sentDate = '" + DateTime.Now.ToString() + "' WHERE msgID = " + msgID.ToString();
                        DataHandler.setData(bulkUpdateQuery, out status);
                        if (status != "") { RaiseEvent(bulkUpdateQuery + " - " + status); }
                    }

                    String updateQuery = "UPDATE tblMsg SET queue = 'False' WHERE id = " + msgID.ToString();
                    DataHandler.setData(updateQuery, out status);
                    message += Environment.NewLine + Environment.NewLine;
                    message += "Send status:" + Environment.NewLine + Environment.NewLine;
                    var builder = new System.Text.StringBuilder();
                    builder.Append(message);
                    foreach (KeyValuePair<String, bool> statuses in emails) { builder.Append(statuses.Key + " = " + statuses.Value.ToString() + Environment.NewLine); }
                    message = builder.ToString();
                    if (incBCC) { MailSender.SendMail(fromAddress, new String[] { bccAddy }, subject, message, false, false, false, out status, attachments); }
                }
            }
        }
    }

    public class CustomerConstruct
    {
        public String buildingName { get; set; }

        public String buildingAbbr { get; set; }

        public String acc { get; set; }

        public String[] emails { get; set; }
    }

    public class WebConstruct
    {
        public String buildingName { get; set; }

        public String acc { get; set; }

        public String owner_email { get; set; }

        public String username { get; set; }

        public bool active { get; set; }
    }
}
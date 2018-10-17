using PasSDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public class PastelArgs : EventArgs
    {
        public String message { get; set; }

        public PastelArgs(String msg)
        {
            message = msg;
        }
    }

    public class Pastel
    {
        public delegate void MessageHandler(object sender, PastelArgs e);

        private short keyNumber = 0;
        private PastelPartnerSDK SDK = new PastelPartnerSDK();
        private String baseDataPath;
        private String m_WatchDirectory;
        private FileSystemWatcher fsc;
        private String status;

        public event MessageHandler Message;

        public Pastel()
        {
            String query = "SELECT trust, business, rental FROM tblSettings";
            DataSet ds = DataHandler.getData(query, out status);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                String trust = ds.Tables[0].Rows[0]["trust"].ToString();
                String business = ds.Tables[0].Rows[0]["business"].ToString();
                String rental = ds.Tables[0].Rows[0]["rental"].ToString();
                bool saveMe = false;
                if (Server.Properties.Settings.Default.trustPath != trust)
                {
                    Server.Properties.Settings.Default.trustPath = trust;
                    saveMe = true;
                }
                if (Server.Properties.Settings.Default.astrodonPath != business)
                {
                    Server.Properties.Settings.Default.astrodonPath = business;
                    saveMe = true;
                }
                if (Server.Properties.Settings.Default.rentPath != rental)
                {
                    Server.Properties.Settings.Default.rentPath = rental;
                    saveMe = true;
                }
                if (saveMe)
                {
                    Server.Properties.Settings.Default.Save();
                }
            }
        }

        public void InitialisePastel()
        {
            baseDataPath = Server.Properties.Settings.Default.baseDataPath;
            m_WatchDirectory = Server.Properties.Settings.Default.watchDirectory;
            SDK.SetLicense(Server.Properties.Settings.Default.licensee, Server.Properties.Settings.Default.authCode);
            if (!Directory.Exists(Server.Properties.Settings.Default.importPath)) { Directory.CreateDirectory(Server.Properties.Settings.Default.importPath); }
            fsc = new FileSystemWatcher(m_WatchDirectory, "*.txt");
            fsc.NotifyFilter = 0;
            fsc.NotifyFilter = fsc.NotifyFilter | NotifyFilters.FileName;
            fsc.EnableRaisingEvents = true;
            fsc.Created += new FileSystemEventHandler(fsc_Created);
            RaiseEvent("Pastel Started: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        }

        public void Close()
        {
            SDK = null;
            fsc = null;
            RaiseEvent("Pastel ended: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        }

        private void RaiseEvent(String message)
        {
            if (Message != null) { Message(this, new PastelArgs(message)); }
        }

        private void fsc_Created(object sender, FileSystemEventArgs e)
        {
            Process(true);
        }

        public void Process(bool delete)
        {
            RaiseEvent("New import detected: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            runRental();
            runJournal();
            runTrust();
            runUna();
            runBuilding();
            if (delete) { updateBatch(); }
            RaiseEvent("Import complete");
        }

        public void runRental()
        {
            String rentalStatus = "============STARTING RENTAL====================" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(rentalStatus);
            String popString = "SELECT tblRentalRecon.id, DATEPART(month, tblRentalRecon.trnDate) - 2 AS period, convert(nvarchar(10), tblRentalRecon.trnDate, 103) as trnDate, 'G' AS gdc, ";
            popString += " tblRentalRecon.account, tblRentals.reference, tblRentals.description, tblRentalRecon.value, tblRentalRecon.contra";
            popString += " FROM tblRentalRecon INNER JOIN tblRentals ON tblRentalRecon.rentalId = tblRentals.id WHERE (tblRentalRecon.posted = 'False')";
            DataSet ds = DataHandler.getData(popString, out status);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow myRow in ds.Tables[0].Rows)
                {
                    try
                    {
                        DateTime tranDate = DateTime.Parse(myRow["trnDate"].ToString());
                        String trnDate = tranDate.ToString("dd/MM/yyyy");
                        int period = int.Parse(myRow["period"].ToString());
                        period += (period <= 0 ? 12 : 0);
                        String StrIn = period.ToString() + "|" + trnDate + "|G|" + myRow["contra"].ToString();
                        StrIn += "|" + myRow["reference"].ToString() + "|" + myRow["description"].ToString() + "|" + myRow["value"].ToString() + "|0|0|A|||0|0|";
                        StrIn += myRow["account"].ToString() + "|1|1";
                        String returner = PostBatch(Server.Properties.Settings.Default.rentPath, StrIn, 5);
                        if (returner == "0")
                        {
                            String updateString = "UPDATE tblRentalRecon SET posted = 'True' WHERE id = " + myRow["id"].ToString();
                            DataHandler.setData(updateString, out status);
                        }
                        RaiseEvent(returner);
                    }
                    catch { }
                }
            }
            rentalStatus = "============ENDING RENTAL====================" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(rentalStatus);
        }

        private void runJournal()
        {
            String journalStatus = "";
            DataSet journalDS = Populate("Journal");
            journalStatus = "=======STARTING JOURNAL==============" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(journalStatus);
            journalStatus = "";
            try
            {
                if (journalDS != null && journalDS.Tables.Count > 0 && journalDS.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow Row in journalDS.Tables[0].Rows)
                    {
                        int rowid = int.Parse(Row["id"].ToString().Trim());
                        String buildContra = Row["contra"].ToString().Replace("/", "");
                        String trustAcc = Row["accnumber"].ToString().Trim();

                        String buildCode = Row["code"].ToString().Trim();
                        int buildEntry = int.Parse(GetBuildingStuff(buildCode)[0]);
                        String buildPath = Row["datapath"].ToString().Trim();
                        int period = int.Parse(Row["period"].ToString());
                        String trnDate = "";
                        try
                        {
                            DateTime tranDate = DateTime.Parse(Row["trnDate"].ToString());
                            trnDate = tranDate.ToString("dd/MM/yyyy");
                        }
                        catch
                        {
                            trnDate = DateTime.Now.ToString("dd/MM/yyyy");
                        }
                        String reference = Row["reference"].ToString().Trim();
                        String desc = Row["description"].ToString().Trim();
                        String postAmt = double.Parse(Row["amount"].ToString()).ToString();

                        //first building
                        int myPeriod = period;
                        int periodControl = GetPeriod(trustAcc);
                        if (period - periodControl == 0)
                        {
                            myPeriod = 12;
                        }
                        else if (period - periodControl < 0)
                        {
                            myPeriod = period - periodControl + 12;
                        }
                        else
                        {
                            myPeriod = period - periodControl;
                        }
                        String returner = "0";
                        int count = 0;
                        String strIn = "";
                        if (reference.Length > 6) { reference = reference.Substring(0, 6); }
                        strIn = period + "|" + trnDate + "|G|" + trustAcc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|9320000|1|1";
                        returner = PostBatch(Server.Properties.Settings.Default.trustPath, strIn, 5);
                        if (returner == "0") { count += 1; }
                        strIn = period + "|" + trnDate + "|G|8100000|" + reference + "|" + desc + "|" + postAmt + "|14|0|A|||0|0|1085000|1|1";
                        returner = PostBatch(Server.Properties.Settings.Default.astrodonPath, strIn, 5);
                        if (returner == "0") { count += 1; }
                        strIn = myPeriod + "|" + trnDate + "|G|3200000|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|" + buildContra + "|1|1";
                        returner = PostBatch(buildPath, strIn, buildEntry);
                        if (returner == "0") { count += 1; }
                        String sqlStr = "UPDATE tblJournal SET post = 'True' WHERE id = " + rowid;
                        DataHandler.setData(sqlStr, out status);
                    }
                }
            }
            catch (Exception ex)
            {
                journalStatus += ex.Message;
            }
            journalStatus += "=======END JOURNAL==============" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(journalStatus);
        }

        private void runTrust()
        {
            DataSet ds = Populate("Trust");
            String path = Server.Properties.Settings.Default.trustPath;
            String reference = "";
            String trustResult = "=======STARTING TRUST==============" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(trustResult);
            trustResult = "";
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow Row in ds.Tables[0].Rows)
                {
                    String period = "";
                    String trnDate = "";
                    String acc = "";
                    String refr = "";
                    String desc = "";
                    double amt = 0;
                    int entryType = 0;
                    String code = "";
                    bool isuna = false;
                    try
                    {
                        

                        int id = int.Parse(Row["id"].ToString());
                        RaiseEvent("Processing Record " + id.ToString());

                        period = Row["period"].ToString().Trim();
                        trnDate = Row["trnDate"].ToString().Trim();
                        acc = Row["accnumber"].ToString().Trim();
                        refr = Row["reference"].ToString().Trim();
                        isuna = bool.Parse(Row["una"].ToString());
                        code = Row["code"].ToString().Trim();
                        desc = Row["description"].ToString().Trim();
                        amt = double.Parse(Row["amount"].ToString().Trim()) * -1;
                        double orgAmt = amt * -1;
                        entryType = (amt < 0 ? 2 : 1);
                        reference = refr;
                        if (reference.Length > 6) { reference = reference.Substring(0, 6); }
                        String postAmt = amt.ToString("#0.00");
                        String testDesc = desc.Replace(" ", "");
                        String returner = "";
                        String StrIn = "";
                        try
                        {
                            if ((testDesc.Contains("BRCASH") || testDesc.StartsWith("BRC")) && !testDesc.Contains("CHQSFEE") && code != "SM" && code != "VB")
                            {
                                StrIn = period + "|" + trnDate + "|G|9320000|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|8400000|1|1";
                                if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                            }
                            else
                            {
                                StrIn = period + "|" + trnDate + "|G|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|8400000|1|1";
                                if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                            }
                            if (returner.Split(new String[] { "|" }, StringSplitOptions.None)[0] != "0" && !isuna)
                            {
                                StrIn = period + "|" + trnDate + "|G|9325000|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|8400000|1|1";
                                if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                            }
                        }
                        catch(Exception ex1)
                        {
                            RaiseEvent("Exception " + ex1.Message);
                            return;
                        }
                        trustResult += returner + Environment.NewLine;
                        if (code != "SM" && code != "VB")
                        {
                            try
                            {
                                if ((testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/")) || testDesc.Contains("BRCASH") || testDesc.Contains("D/") || testDesc.Contains("CASHTRANS") || testDesc.Contains("TRFTO"))
                                {
                                    if ((testDesc.Contains("BRCASH") || testDesc.StartsWith("BRC")) && !testDesc.Contains("CHQSFEE"))
                                    {
                                        double neg = double.Parse(postAmt) * -1;
                                        postAmt = neg.ToString("#0.00");
                                        StrIn = period + "|" + trnDate + "|G|8100000|" + reference + "|" + desc + "|" + postAmt + "|14|0|A|||0|0|3200000|1|1";
                                        if (postAmt != "0.00") { returner = PostBatch(Server.Properties.Settings.Default.astrodonPath, StrIn, 5); }
                                    }
                                    else if (!testDesc.Contains("BRCASH") || !testDesc.Contains("CHQSFEE"))
                                    {
                                        if (testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/") && orgAmt < 0)
                                        {
                                            entryType = 1;
                                            double pamt = double.Parse(postAmt);
                                            if (pamt < 0) { pamt *= -1; }
                                            postAmt = pamt.ToString();
                                        }
                                        else
                                        {
                                            postAmt = "0";
                                        }
                                    }
                                    postAmt = managementFee(testDesc, orgAmt);
                                    if (postAmt != "0")
                                    {
                                        StrIn = period + "|" + trnDate + "|G|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|9320000|1|1";
                                        if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                                        if (returner.Split(new String[] { "|" }, StringSplitOptions.None)[0] != "0" && !isuna)
                                        {
                                            StrIn = period + "|" + trnDate + "|G|9325000|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|9320000|1|1";
                                            if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                                        }
                                        String astAcc = (testDesc.Contains("D/") ? "1086000" : "1085000");
                                        StrIn = period + "|" + trnDate + "|G|8100000|" + reference + "|" + desc + "|" + postAmt + "|14|0|A|||0|0|" + astAcc + "|1|1";
                                        if (postAmt != "0.00") { returner = PostBatch(Server.Properties.Settings.Default.astrodonPath, StrIn, 5); }
                                    }
                                }
                            }
                            catch(Exception ex2)
                            {
                                RaiseEvent("Exception " + ex2.Message);
                                return;
                            }
                        }
                        else
                        {
                            if ((testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/")) || testDesc.Contains("TRFTO"))
                            {
                                postAmt = (code == "VB" ? managementFee(testDesc, orgAmt) : managementFee(testDesc, double.Parse(postAmt)));
                                if (postAmt != "0")
                                {
                                    StrIn = period + "|" + trnDate + "|G|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|9320000|1|1";
                                    if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                                    if (returner.Split(new String[] { "|" }, StringSplitOptions.None)[0] != "0")
                                    {
                                        StrIn = period + "|" + trnDate + "|G|9325000|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|9320000|1|1";
                                        if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                                    }
                                    String astAcc = (testDesc.Contains("D/") ? "1086000" : "1085000");
                                    StrIn = period + "|" + trnDate + "|G|8100000|" + reference + "|" + desc + "|" + postAmt + "|14|0|A|||0|0|" + astAcc + "|1|1";
                                    if (postAmt != "0.00") { returner = PostBatch(Server.Properties.Settings.Default.astrodonPath, StrIn, 5); }
                                }
                            }
                        }
                    }
                    catch (Exception ex3)
                    {
                        trustResult += ex3.Message + Environment.NewLine;
                        RaiseEvent("Exception " + ex3.Message);
                        return;
                    }
                }
            }
            else
            {
                RaiseEvent("Zero records Found");

            }
            trustResult += "=======ENDING TRUST==============" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(trustResult);
        }

        private void runUna()
        {
            DataSet ds = Populate("UNA");
            String path = Server.Properties.Settings.Default.trustPath;
            String reference = "";
            String unaResult = "=======STARTING UNALLOC==============" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(unaResult);
            unaResult = "";
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow Row in ds.Tables[0].Rows)
                {
                    String period = "";
                    String trnDate = "";
                    String acc = "";
                    String refr = "";
                    String desc = "";
                    double amt = 0;
                    int entryType = 0;
                    String code = "";
                    try
                    {
                        int id = int.Parse(Row["id"].ToString());
                        period = Row["period"].ToString().Trim();
                        trnDate = Row["trnDate"].ToString().Trim();
                        acc = Row["accnumber"].ToString().Trim();
                        refr = Row["reference"].ToString().Trim();
                        code = Row["code"].ToString().Trim();
                        desc = Row["description"].ToString().Trim();
                        amt = double.Parse(Row["amount"].ToString().Trim()) * -1;
                        entryType = (amt < 0 ? 2 : 1);
                        reference = refr;
                        if (reference.Length > 6) { reference = reference.Substring(0, 6); }
                        String postAmt = amt.ToString("#0.00");
                        String testDesc = desc.Replace(" ", "");
                        String returner = "";
                        String StrIn = "";
                        try
                        {
                            if (testDesc.Contains("BRCASH") && code != "SM" && code != "VB")
                            {
                            }
                            else
                            {
                                StrIn = period + "|" + trnDate + "|G|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|9325000|1|1";
                                if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                            }
                        }
                        catch
                        {
                            return;
                        }
                        unaResult += returner + Environment.NewLine;
                        if (code != "SM" && code != "VB")
                        {
                            try
                            {
                                if ((testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/")) || testDesc.Contains("BRCASH") || testDesc.Contains("D/") || testDesc.Contains("CASHTRANS") || testDesc.Contains("TRFTO"))
                                {
                                    postAmt = managementFee(testDesc, double.Parse(postAmt));
                                    double mngFee = double.Parse(postAmt) * -1;
                                    if (postAmt != "0")
                                    {
                                        StrIn = period + "|" + trnDate + "|G|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|9325000|1|1";
                                        if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                                        //String astAcc = "";
                                    }
                                }
                            }
                            catch
                            {
                                return;
                            }
                        }
                        else
                        {
                            if ((testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/")) || testDesc.Contains("TRFTO"))
                            {
                                postAmt = managementFee(testDesc, double.Parse(postAmt));
                                double mngFee = double.Parse(postAmt) * -1;
                                if (postAmt != "0")
                                {
                                    StrIn = period + "|" + trnDate + "|G|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|9325000|1|1";
                                    if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        unaResult += ex.Message + Environment.NewLine;
                        return;
                    }
                }
            }
            unaResult += "=======ENDING UNALLOC==============" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(unaResult);
        }

        private String GetDebitAcc(String reference)
        {
            String dbAcc = "";
            dbAcc = reference.Replace("D/", "");
            if (dbAcc.EndsWith("R")) { dbAcc = dbAcc.TrimEnd('R'); }
            return dbAcc;
        }

        private void runBuilding()
        {
            String buildResult = "========= STARTING BUILDING =========" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(buildResult);
            buildResult = "";
            DataSet ds = Populate("Building");
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                buildResult = "========= ENTRIES: " + ds.Tables[0].Rows.Count.ToString() + "=========" + Environment.NewLine;
                RaiseEvent(buildResult);
                buildResult = "";
                int counter = 0;
                String reference = "";
                foreach (DataRow Row in ds.Tables[0].Rows)
                {
                    bool una = bool.Parse(Row["una"].ToString());
                    int id = int.Parse(Row["id"].ToString());

                    int period = int.Parse(Row["period"].ToString().Trim());
                    String trnDate = Row["trnDate"].ToString().Trim();
                    String path = Row["datapath"].ToString().Trim();
                    String refr = Row["reference"].ToString().Trim();
                    String code = Row["code"].ToString().Trim();
                    String[] entryTypes = GetBuildingStuff(code);
                    String contra = Row["contra"].ToString().Trim().Replace("/", "");
                    String desc = Row["description"].ToString().Trim();
                    double amt = double.Parse(Row["amount"].ToString().Trim()) * -1;
                    double orgAmt = amt * -1;
                    try
                    {
                        int periodControl = GetPeriod(Row["accnumber"].ToString().Trim());
                        if (period - periodControl == 0)
                        {
                            period = 12;
                        }
                        else if (period - periodControl < 0)
                        {
                            period = period - periodControl + 12;
                        }
                        else
                        {
                            period = period - periodControl;
                        }
                        String acc = "";
                        String gdc = "";
                        reference = refr;
                        try
                        {
                            if (desc.Contains("(") && desc.Contains(")") && desc.Contains("/"))
                            {
                                String payDesc = desc.Replace(" ", "");
                                int stPoint = payDesc.IndexOf("/");
                                stPoint -= 4;
                                acc = payDesc.Substring(stPoint, payDesc.Length - stPoint).Replace("/", "").Trim();
                                if (acc.Length > 7) { acc = acc.Substring(0, 6); }
                                gdc = "G";
                            }
                            else
                            {
                                if (refr.StartsWith("ACC:"))
                                {
                                    acc = refr.Substring(4, refr.Length - 4).Replace("/", "");
                                    refr = acc;
                                    gdc = "G";
                                }
                                else
                                {
                                    if (refr.Contains("D/")) { refr = GetDebitAcc(refr); }
                                    if (refr.Length > 6) { refr = refr.Substring(0, 5); }
                                    acc = refr;
                                    gdc = "D";
                                }
                            }
                        }
                        catch { return; }
                        String testDesc = desc.Replace(" ", "");
                        int entryType = 0;
                        entryType = (amt < 0 ? int.Parse(entryTypes[1]) : int.Parse(entryTypes[0]));
                        if (testDesc.Contains("CASHTRANS")) { entryType = int.Parse(entryTypes[1]); }
                        String postAmt = amt.ToString("#0.00");
                        if (reference.Length > 6) { reference = reference.Substring(0, 6); }
                        String StrIn = period + "|" + trnDate + "|" + gdc + "|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|" + contra + "|1|1";
                        String returner = "";
                        try
                        {
                            if ((testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/")) || testDesc.Contains("BRCASH") || testDesc.Contains("D/"))
                            {
                                if ((testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/")) || testDesc.Contains("D/"))
                                {
                                    if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                                    if (returner.Split(new String[] { "|" }, StringSplitOptions.None)[0] != "0")
                                    {
                                        String display = returner.Replace("|", "-");
                                        StrIn = period + "|" + trnDate + "|G|9990000|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|" + contra + "|1|1";
                                        if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                                    }
                                }
                                if (code == "VB")
                                {
                                    if (testDesc.Contains("D/"))
                                    {
                                        postAmt = managementFee(testDesc, orgAmt);
                                    }
                                    else
                                    {
                                        postAmt = managementFee(testDesc, amt * -1);
                                    }
                                }
                                else
                                {
                                    if (testDesc.Contains("D/"))
                                    {
                                        postAmt = managementFee(testDesc, orgAmt);
                                    }
                                    else
                                    {
                                        postAmt = managementFee(testDesc, (double.Parse(postAmt) > 0 ? double.Parse(postAmt) * -1 : double.Parse(postAmt)));
                                    }
                                }
                                entryType = int.Parse(entryTypes[1]);
                                if (refr == "XXXXXX")
                                {
                                    if (testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/")) { acc = "1085000"; }
                                    if (testDesc.Contains("D/")) { acc = "1086000"; }
                                    if (testDesc.Contains("BRCASH")) { acc = "1085000"; }
                                    gdc = "G";
                                }
                                else if (testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/"))
                                {
                                    acc = "3200000";
                                }
                                if ((code == "SM" || code == "VB") && testDesc.Substring(0, 6) == "BRCASH") { postAmt = amt.ToString("#0.00"); }
                                StrIn = period + "|" + trnDate + "|" + gdc + "|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|" + contra + "|1|1";

                                if (testDesc.Contains("(") && testDesc.Contains(")") && testDesc.Contains("/"))
                                {
                                    entryType = int.Parse(entryTypes[0]);
                                    double pamt = double.Parse(postAmt);
                                    if (pamt < 0) { pamt *= -1; }
                                    postAmt = pamt.ToString();
                                }

                                if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                                if (returner.Split(new String[] { "|" }, StringSplitOptions.None)[0] != "0")
                                {
                                    StrIn = period + "|" + trnDate + "|G|9990000|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|" + contra + "|1|1";
                                    if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                                }
                            }
                            else
                            {
                                if (!una && path == Server.Properties.Settings.Default.astrodonPath)
                                {
                                    StrIn = period + "|" + trnDate + "|G|" + acc + "|" + reference + "|" + desc + "|" + postAmt + "|14|0|A|||0|0|8100000|1|1";
                                    if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                                    if (returner.Split(new String[] { "|" }, StringSplitOptions.None)[0] != "0")
                                    {
                                        String display = returner.Replace("|", "-");
                                        StrIn = period + "|" + trnDate + "|G|9990000|" + reference + "|" + desc + "|" + postAmt + "|14|0|A|||0|0|8100000|1|1";
                                        if (postAmt != "0.00") { returner = PostBatch(path, StrIn, 5); }
                                    }
                                }
                                else
                                {
                                    if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                                    if (returner.Split(new String[] { "|" }, StringSplitOptions.None)[0] != "0")
                                    {
                                        String display = returner.Replace("|", "-");
                                        StrIn = period + "|" + trnDate + "|G|9990000|" + reference + "|" + desc + "|" + postAmt + "|0|0|A|||0|0|" + contra + "|1|1";
                                        if (postAmt != "0.00") { returner = PostBatch(path, StrIn, entryType); }
                                    }
                                }
                            }
                            counter += 1;
                        }
                        catch (Exception ex)
                        {
                            buildResult += ex.Message + Environment.NewLine;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        buildResult += ex.Message + Environment.NewLine;
                        return;
                    }
                }
                buildResult += "========= POSTED: " + counter + " =========" + Environment.NewLine;
            }
            buildResult += "========= END BUILDING =========" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + Environment.NewLine;
            RaiseEvent(buildResult);
        }

        private void updateBatch()
        {
            DataHandler.setData("DELETE FROM tblExport", out status);
            DataHandler.setData("UPDATE tblDevision SET posted = 'True'", out status);
        }

        public String GetBankDetails(String buildPath)
        {
            String bankDetails = String.Empty;
            String returner = SDK.SetDataPath(Path.Combine(baseDataPath, buildPath));
            if (returner == "0")
            {
                try
                {
                    String fileName = "ACCPRMDC";
                    String keyValue = "0";
                    String response = SDK.GetNearest(fileName, 0, keyValue);
                    //MessageBox.Show(response);
                    String[] responseBits = response.Split(new String[] { "|" }, StringSplitOptions.None);
                    bankDetails = responseBits[17];
                }
                catch (Exception ex)
                {
                    bankDetails = ex.Message;
                }
            }
            return bankDetails;
        }

        public List<Trns> GetTransactions(String buildPath, int startperiod, int endperiod, String acc)
        {
            List<Trns> rs = new List<Trns>();
            String returner = SDK.SetDataPath(Path.Combine(baseDataPath, buildPath));
            //period = 6
            if (returner == "0")
            {
                short period = (short)startperiod;
                try
                {
                    String fileName = "ACCTRN";
                    String keyValue = SDK.RightPad("D", 1) + SDK.RightPad(acc, 7) + SDK.MKI(period);
                    String response = SDK.GetNearest(fileName, 11, keyValue);

                    String[] responseBits = response.Split(new String[] { "|" }, StringSplitOptions.None);
                    //MessageBox.Show(startperiod.ToString() + " - " + endperiod.ToString() + " - " + responseBits[6]);
                    if (response.StartsWith("0|") && int.Parse(responseBits[6]) >= startperiod && int.Parse(responseBits[6]) <= endperiod)
                    {
                        try
                        {
                            String trnAcc = responseBits[2];
                            if (trnAcc.Contains(acc))
                            {
                                Trns trn = new Trns();
                                trn.Amount = responseBits[11];
                                trn.Date = SDK.BtrieveToVBDate(responseBits[7]).ToString("yyyy/MM/dd");
                                trn.Description = responseBits[18];
                                trn.Reference = responseBits[9];
                                trn.period = responseBits[6];
                                rs.Add(trn);
                            }
                        }
                        catch { }
                        while (response.StartsWith("0|"))
                        {
                            response = SDK.GetNext(fileName, 11);
                            responseBits = response.Split(new String[] { "|" }, StringSplitOptions.None);
                            //MessageBox.Show(response);
                            if (response.StartsWith("0|") && int.Parse(responseBits[6]) >= startperiod && int.Parse(responseBits[6]) <= endperiod)
                            {
                                try
                                {
                                    String trnAcc = responseBits[2];
                                    if (trnAcc.Contains(acc))
                                    {
                                        Trns trn = new Trns();
                                        trn.Amount = responseBits[11];
                                        trn.Date = SDK.BtrieveToVBDate(responseBits[7]).ToString("yyyy/MM/dd");
                                        trn.Description = responseBits[18];
                                        trn.Reference = responseBits[9];
                                        trn.period = responseBits[6];
                                        rs.Add(trn);
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    returner = "error:" + ex.Message;
                }
            }
            else
            {
                MessageBox.Show(returner);
            }
            return rs;
        }

        private String PostBatch(String path, String StrIn, int entryType)
        {
            try
            {
                String StrReturn = "0";
                String strCodeIn;
                StrReturn = SDK.SetDataPath(Path.Combine(baseDataPath, path));
                if (StrReturn == "0") { StrReturn = SDK.SetGLPath(baseDataPath); }
                if (StrReturn == "0")
                {
                    strCodeIn = StrIn;
                    short et = (short)entryType;
                    StrReturn = SDK.ImportGLBatch(StrIn, et);
                }
                if (StrReturn.Length == 0) { return "9"; }
                if (StrReturn != "0")
                {
                    String[] returnValues = StrReturn.Split(new String[] { "|" }, StringSplitOptions.None);
                    StrReturn = GetResultDesc(returnValues[0]);
                }
                return StrReturn;
            }
            catch
            {
                return string.Empty;
            }
        }

        private DataSet Populate(String pType)
        {
            String sqlString = "";
            switch (pType)
            {
                case "Journal":
                    sqlString = "SELECT * FROM tblJournal WHERE post = 'False' ORDER BY building ";
                    break;

                case "Trust":
                    sqlString = "SELECT * FROM tblExport WHERE accnumber is not null and una = 'False' ORDER BY lid";
                    break;

                case "UNA":
                    sqlString = "SELECT * FROM tblExport WHERE accnumber is not null and una = 'True' ORDER BY lid";
                    break;

                case "Building":
                    sqlString = "SELECT * FROM tblExport WHERE datapath is not null and datapath != '' ORDER BY id";
                    break;

                case "Info":
                    sqlString = "SELECT * FROM tblCashDeposits";
                    break;

                case "AllBuild":
                    sqlString = "SELECT id, code, datapath FROM tblBuildings ORDER BY code";
                    break;
            }
            return DataHandler.getData(sqlString, out status);
        }

        private String managementFee(String description, double amt)
        {
            description = description.Replace(" ", "");
            int point = description.IndexOf(".");
            DataSet ds1 = Populate("Info");
            String returnFee = "0";
            String b;
            try
            {
                b = description.Substring(point + 1, 1);
            }
            catch
            {
                b = "x";
            }
            if (description.Contains("BRCASH"))
            {
                if (point != -1)
                {
                    int x = 7;
                    String testString = description.Substring(x, 1);
                    String DepAmountString = "";
                    while (testString != ".")
                    {
                        DepAmountString += testString;
                        x += 1;
                        testString = description.Substring(x, 1);
                    }
                    double DepAmountDouble = double.Parse(DepAmountString);
                    foreach (DataRow Row1 in ds1.Tables[0].Rows)
                    {
                        double min = double.Parse(Row1["min"].ToString());
                        double max = double.Parse(Row1["max"].ToString());
                        double theValue = double.Parse(Row1["amount"].ToString());
                        if (DepAmountDouble >= min && DepAmountDouble <= max)
                        {
                            returnFee = theValue.ToString("#0.00");
                            break;
                        }
                    }
                    if (returnFee == "0") { returnFee = "777"; }
                }
            }
            else if (description.Contains("D/"))
            {
                if (amt < 0)
                {
                    returnFee = "196.63";
                }
                else
                {
                    returnFee = "39.19";
                }
            }
            else if (description.Contains("CASHTRANSACTION"))
            {
                returnFee = "0";
            }
            else if (description.Contains("TRFTO") || (description.Contains("(") && description.Contains(")") && description.Contains("/")))
            {
                if ((amt > 0) || (description.Contains("9305")))
                {
                    //rental payment
                    //127	ASTRODON RENTALS	RENT	9305000	RENTAL13	0	9305/000	8430/000	1	2	3	NULL	9250/000	0	NULL
                    returnFee = "0";
                }
                else
                {
                    returnFee = GetEFTFee();
                }
                //} else {
                //    returnFee = "0";
                //}
            }
            else if (description.Contains("TRFFRM"))
            {
                returnFee = "0";
            }
            return returnFee;
        }

        private String GetEFTFee()
        {
            DataSet dsEFT = DataHandler.getData("SELECT DebitOrder FROM tblBankCharges", out status);
            String amt = dsEFT.Tables[0].Rows[0]["DebitOrder"].ToString();
            //MessageBox.Show(amt);
            return amt;
        }

        private String GetResultDesc(String strRCode)
        {
            String returnString = "";
            switch (strRCode)
            {
                case "0":
                    returnString = "0 = Call successfully executed";
                    break;

                case "1":
                    returnString = "1 = File Not found";
                    break;

                case "2":
                    returnString = "2 = Invalid number of fields" + Environment.NewLine;
                    returnString += "Check your SetDataPath call and make sure pastel files exist at that path" + Environment.NewLine;
                    returnString += "Try and use directories less than 8 characters long";
                    break;

                case "3":
                    returnString = "3 = Record update not successful";
                    break;

                case "4":
                    returnString = "4 = Record insert not successful";
                    break;

                case "5":
                    returnString = "5 = Record does not exist in file";
                    break;

                case "6":
                    returnString = "6 = Data path does not exist";
                    break;

                case "7":
                    returnString = "7 = Access denied";
                    break;

                case "9":
                    returnString = "9 = End of file";
                    break;

                case "10":
                    returnString = "10 = Field number specified not valid";
                    break;

                case "11":
                    returnString = "11 = Invalid period number (1 to 13)";
                    break;

                case "12":
                    returnString = "12 = Invalid Date";
                    break;

                case "13":
                    returnString = "13 = Invalid account type (GDC)";
                    break;

                case "14":
                    returnString = "14 = Invalid general ledger account number";
                    break;

                case "15":
                    returnString = "15 = General ledger account contains sub accounts";
                    break;

                case "16":
                    returnString = "16 = General ledger account number must be numeric";
                    break;

                case "17":
                    returnString = "17 = Invalid customer account code";
                    break;

                case "18":
                    returnString = "18 = Invalid supplier account code";
                    break;

                case "19":
                    returnString = "19 = Invalid inventory item code";
                    break;

                case "20":
                    returnString = "20 = Invalid salesman code";
                    break;

                case "21":
                    returnString = "21 = Invalid job code";
                    break;

                case "22":
                    returnString = "22 = Invalid Tax Type (0 to 30)";
                    break;

                case "23":
                    returnString = "23 = Transaction amount cannot be less that the tax amount";
                    break;

                case "24":
                    returnString = "24 = Invalid open item transaction type - must be O (Original) or A (Allocation)";
                    break;

                case "25":
                    returnString = "25 = There cannot be more than 500 lines in a batch";
                    break;

                case "26":
                    returnString = "26 = Invalid account description";
                    break;

                case "27":
                    returnString = "27 = Default group needs to set up in Pastel";
                    break;

                case "28":
                    returnString = "28 = Invalid document line type – must be 2, 5, or 7";
                    break;

                case "29":
                    returnString = "29 = Invalid exclusive / inclusive – must be 0 or 1";
                    break;

                case "30":
                    returnString = "30 = Invalid Entry Type (1 to 90)";
                    break;

                case "31":
                    returnString = "31 = Duplicate inventory item";
                    break;

                case "32":
                    returnString = "32 = Invalid multi-store code";
                    break;

                case "33":
                    returnString = "33 = Invalid Currency Code";
                    break;

                case "99":
                    returnString = "99 = General Error";
                    break;
            }
            return returnString;
        }

        private String[] GetBuildingStuff(String Reference)
        {
            DataSet ds = DataHandler.getData("SELECT payments, receipts, journals FROM tblBuildings WHERE Code = '" + Reference + "'", out status);
            String[] info = new string[3];
            try
            {
                info[0] = ds.Tables[0].Rows[0]["payments"].ToString().Trim();
                info[1] = ds.Tables[0].Rows[0]["receipts"].ToString().Trim();
                info[2] = ds.Tables[0].Rows[0]["journals"].ToString().Trim();
            }
            catch
            {
                info[0] = "1";
                info[1] = "2";
                info[2] = "5";
            }
            return info;
        }

        private int GetPeriod(String AccNumber)
        {
            String str = "SELECT * FROM tblBuildings WHERE AccNumber = '" + AccNumber + "'";
            DataSet ds = DataHandler.getData(str, out status);
            try
            {
                return int.Parse(ds.Tables[0].Rows[0]["Period"].ToString().Trim());
            }
            catch
            {
                return 0;
            }
        }

        public String PostBatch(DateTime trnDate, int buildPeriod, String trustPath, String buildPath, int trustType, int buildType, String bc, String buildAcc, String trustContra, String buildContra,
            String reference, String description, String amt, String trustAcc, String rAcc, out String pastelString)
        {
            double pAmt = double.Parse(amt);
            String returnValue = "";
            String StrIn;
            if (pAmt < 0)
            {
                String returner = "";
                int trustPeriod = getPeriod(trnDate);
                buildPeriod = (trustPeriod - buildPeriod < 1 ? trustPeriod - buildPeriod + 12 : trustPeriod - buildPeriod);
                pastelString = "";
                //Building
                if (rAcc != "")
                {
                    String dbAmt = (pAmt * -1).ToString("##0.00");
                    StrIn = buildPeriod.ToString() + "|" + trnDate.ToString("dd/MM/yyyy") + "|G|" + buildContra.Replace("/", "") + "|" + buildAcc + "|" + description + "|" + dbAmt + "|0|0|A|||0|0|" + rAcc + "|1|1";
                    pastelString = "; Building = " + StrIn;
                    returner += PostBatch(buildPath, StrIn, buildType);
                    returnValue += "; Building = " + returner;
                }
            }
            else
            {
                String returner = "";
                int trustPeriod = getPeriod(trnDate);
                buildPeriod = (trustPeriod - buildPeriod < 1 ? trustPeriod - buildPeriod + 12 : trustPeriod - buildPeriod);
                //Centrec
                StrIn = trustPeriod.ToString() + "|" + trnDate.ToString("dd/MM/yyyy") + "|D|" + bc + "|" + buildAcc + "|" + description + "|" + amt + "|0|0|A|||0|0|" + trustAcc.Replace("/", "") + "|1|1";
                pastelString = "Centrec = " + StrIn;
                returner = PostBatch(trustPath, StrIn, trustType);
                returnValue += "Centrec = " + returner;
                //Building
                StrIn = buildPeriod.ToString() + "|" + trnDate.ToString("dd/MM/yyyy") + "|D|" + buildAcc + "|" + buildAcc + "|" + description + "|" + amt + "|0|0|A|||0|0|" + buildContra.Replace("/", "") + "|1|1";
                pastelString += "; Building = " + StrIn;
                returner = PostBatch(buildPath, StrIn, buildType);
                returnValue += "; Building = " + returner;
            }
            return returnValue;
        }

        public int getPeriod(DateTime trnDate)
        {
            int myMonth = trnDate.Month;
            myMonth = myMonth - 2;
            myMonth = (myMonth < 1 ? myMonth + 12 : myMonth);
            return myMonth;
        }

        public Customer GetCustomer(String telNumber, out int buildingID)
        {
            DataSet dsBuildings = Populate("AllBuild");
            buildingID = 0;
            int bID = 0;
            Customer c = null;
            if (dsBuildings != null && dsBuildings.Tables.Count > 0 && dsBuildings.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsBuildings.Tables[0].Rows)
                {
                    String buildPath = Path.Combine(baseDataPath, dr["datapath"].ToString());
                    String returner = SDK.SetDataPath(buildPath);
                    if (returner.Contains("99"))
                    {
                        MessageBox.Show(Path.Combine(baseDataPath, buildPath));
                    }
                    else
                    {
                        int records = SDK.NumberOfRecords("ACCMASD");
                        if (records > 0)
                        {
                            List<String> customerStrings = GetCustomers(buildPath);
                            foreach (String customer in customerStrings)
                            {
                                Customer newCustomer = new Customer(customer);
                                newCustomer.SetDeliveryInfo(DeliveryInfo(newCustomer.accNumber));
                                if (newCustomer.CellPhone == telNumber)
                                {
                                    c = newCustomer;
                                    bID = int.Parse(dr["id"].ToString());
                                    return c;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Pastel Add Customers: " + returner + " - " + records.ToString());
                        }
                    }
                }
            }
            buildingID = bID;
            if (buildingID == 0) { c = null; }
            return c;
        }

        public List<Customer> AddCustomers(String buildKey, String buildPath)
        {
            List<Customer> customers = new List<Customer>();
            String pastelName = "C:\\Pastel11";
            String returner = SDK.SetDataPath(Path.Combine(pastelName, buildPath));
            if (returner.Contains("99"))
            {
                //MessageBox.Show(Path.Combine(pastelName, buildPath));
            }
            else
            {
                int records = SDK.NumberOfRecords("ACCMASD");
                if (records > 0)
                {
                    List<String> customerStrings = GetCustomers(buildPath);
                    foreach (String customer in customerStrings)
                    {
                        Customer newCustomer = new Customer(customer);
                        newCustomer.SetDeliveryInfo(DeliveryInfo(newCustomer.accNumber));
                        customers.Add(newCustomer);
                    }
                }
                else
                {
                    MessageBox.Show("Pastel Add Customers: " + returner + " - " + records.ToString());
                }
            }
            customers.Sort(new CustomerComparer("AccNo", SortOrder.Ascending));
            return customers;
        }

        public void LoadBuildings()
        {
            String status = String.Empty;
            MySqlConnector mySqlConn = new MySqlConnector();
            mySqlConn.ToggleConnection(true);
            String buildingQuery = "SELECT Building, Code, DataPath FROM tblBuildings WHERE (Building NOT LIKE 'ASTRODON%') AND (Building <> 'LETTERS') ORDER BY Building";
            DataSet dsBuildings = DataHandler.getData(buildingQuery, out status);
            if (dsBuildings != null && dsBuildings.Tables.Count > 0 && dsBuildings.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow drBuilding in dsBuildings.Tables[0].Rows)
                {
                    String name = drBuilding["Building"].ToString();
                    String abbr = drBuilding["Code"].ToString();
                    String datapath = drBuilding["DataPath"].ToString();
                    mySqlConn.InsertBuilding(name, abbr, out status);
                }
            }
            mySqlConn.ToggleConnection(false);
        }

        public List<Customer> GetCustomers(bool includeAddress, String category)
        {
            List<Customer> customers = new List<Customer>();
            DataSet dsBuildings = Populate("AllBuild");

            if (dsBuildings != null && dsBuildings.Tables.Count > 0 && dsBuildings.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow item in dsBuildings.Tables[0].Rows)
                {
                    String buildPath = Path.Combine(baseDataPath, item["datapath"].ToString());
                    String returner = SDK.SetDataPath(buildPath);
                    if (!returner.Contains("99"))
                    {
                        int records = SDK.NumberOfRecords("ACCMASD");
                        if (records > 0)
                        {
                            List<String> customerStrings = GetCustomers(buildPath);
                            foreach (String customer in customerStrings)
                            {
                                Customer newCustomer = new Customer(customer);
                                if (includeAddress) { newCustomer.SetDeliveryInfo(DeliveryInfo(newCustomer.accNumber)); }
                                if (!String.IsNullOrEmpty(category) && newCustomer.category == category) { customers.Add(newCustomer); }
                            }
                        }
                    }
                }
            }
            customers.Sort(new CustomerComparer("AccNo", SortOrder.Ascending));
            return customers;
        }

        public DateTime convertDate(String inDate)
        {
            DateTime outDate = DateTime.Now;
            DateTime.TryParse(inDate, out outDate);
            return outDate;
        }

        public List<String> GetCustomers(String path)
        {
            List<String> customers = new List<string>();
            String pastelName = "C:\\Pastel11";
            String returner = SDK.SetDataPath(Path.Combine(pastelName, path));
            if (returner == "0")
            {
                try
                {
                    String fileName = "ACCMASD";
                    String keyValue = SDK.MKI(0);
                    returner = SDK.GetNearest(fileName, keyNumber, keyValue);
                    if (Regex.Matches(returner, "|").Count > 5) { customers.Add(returner); }
                    while (!returner.StartsWith("9|"))
                    {
                        returner = SDK.GetNext(fileName, keyNumber);
                        if ((Regex.Matches(returner, "|").Count > 5) && (!returner.StartsWith("9|"))) { customers.Add(returner); }
                    }
                }
                catch (Exception ex)
                {
                    returner = "error:" + ex.Message;
                }
            }
            return customers;
        }

        public String[] DeliveryInfo(String customerAcc)
        {
            //ACCDELIV
            String returner = "";
            String[] delBits = new string[1];
            try
            {
                String fileName = "ACCDELIV";
                short keyNumber = 2;
                returner = SDK.GetNearest(fileName, keyNumber, customerAcc);
                delBits = SplitDeliveryInfo(returner);
                //if (delBits[13].Contains("@imp.ad-one.co.za")) { delBits[13] = ""; }
                String rCustAcc = delBits[1];
                while (rCustAcc == customerAcc)
                {
                    returner = SDK.GetNext(fileName, keyNumber);
                    String[] nextBits = SplitDeliveryInfo(returner);
                    rCustAcc = nextBits[1];
                    try
                    {
                        if (rCustAcc == customerAcc && delBits[13] != nextBits[13] && nextBits[13] != "") { delBits[13] += (delBits[13] != "" ? ";" : "") + nextBits[13]; }
                    }
                    catch { }
                }
                //MessageBox.Show(delBits[13]);
            }
            catch (Exception ex)
            {
                returner = "error:" + ex.Message;
            }
            return delBits;
        }

        private String[] SplitDeliveryInfo(String delString)
        {
            String[] stringSplitter = new String[] { "|" };
            String[] delBits = delString.Split(stringSplitter, StringSplitOptions.None);
            // try { Email = delBits[13]; } catch { Email = ""; }
            return delBits;
        }
    }
}
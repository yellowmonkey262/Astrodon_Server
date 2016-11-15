using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Server {

    public class RecreateStatements {
        private DataSet dsBuildings;
        private List<StatementBuilding> buildings;

        public event MessageHandler Message;

        public delegate void MessageHandler(object sender, PastelArgs e);

        private void RaiseEvent(String message) {
            if (Message != null) { Message(this, new PastelArgs(message)); }
        }

        public void DoStatements() {
            RaiseEvent("Starting Upload");
            Statements statements = new Statements();
            statements.statements = new List<SStatement>();
            LoadBuildings();
            Dictionary<String, bool> hasStatements = new Dictionary<string, bool>();
            int idx = 0;
            PDF generator = new PDF(true);
            MySqlConnector mySqlConn = new MySqlConnector();
            mySqlConn.ToggleConnection(true);
            Sftp ftpClient = new Sftp();
            if (ftpClient.ConnectClient()) {
                for (int i = 0; i < buildings.Count; i++) {
                    //if (i >= 36) {
                    StatementBuilding sb = buildings[i];
                    String buildingName = sb.Building;
                    String datapath = sb.DataPath;
                    int period = sb.Period;
                    List<SStatement> bStatements = SetBuildings(buildingName, datapath, period, sb.HOA);
                    DataRow dr = dsBuildings.Tables[0].Rows[i];
                    String pm = dr["pm"].ToString();
                    String bankName = dr["bankName"].ToString();
                    String accName = dr["accName"].ToString();
                    String bankAccNumber = dr["bankAccNumber"].ToString();
                    String branch = dr["branch"].ToString();
                    foreach (SStatement s in bStatements) {
                        s.pm = pm;
                        s.bankName = bankName;
                        s.accName = accName;
                        s.accNumber = bankAccNumber;
                        s.branch = branch;
                        statements.statements.Add(s);
                    }
                    //}
                }
                DateTime stmtDate = new DateTime(2015, 9, 21);
                RaiseEvent("Starting statements: " + statements.statements.Count.ToString());
                int finalCount = 0;
                foreach (SStatement stmt in statements.statements) {
                    String fileName = String.Empty;
                    bool success1 = false;
                    bool success2 = false;
                    generator.CreateStatement(stmt, stmtDate, stmt.BuildingName != "ASTRODON RENTALS" ? true : false, out fileName);

                    #region Upload Letter

                    String actFileTitle = Path.GetFileNameWithoutExtension(fileName);
                    String actFile = Path.GetFileName(fileName);

                    //try {
                    //    success1 = mySqlConn.InsertStatement(actFileTitle, "Customer Statements", actFile, stmt.AccNo, stmt.email1);
                    //} catch (Exception ex) {
                    //    RaiseEvent("Mysql error: " + ex.Message);
                    //}
                    //try {
                    //    success2 = ftpClient.Upload(fileName, actFile);
                    //} catch (Exception ex) {
                    //    RaiseEvent("Ftp error: " + ex.Message);
                    //}
                    finalCount += (success1 && success2 ? 1 : 0);

                    #endregion Upload Letter
                }

                RaiseEvent("Upload complete. " + finalCount.ToString() + " complete");
            } else {
                RaiseEvent("Cannot connect to FTP");
            }
        }

        private void LoadBuildings() {
            if (buildings == null) { buildings = new List<StatementBuilding>(); }
            String build = String.Empty;
            try {
                String query = "SELECT DISTINCT b.Building, b.DataPath, b.Period, '' as [Last Processed], b.pm, b.bankName, b.accName, b.bankAccNumber, b.branch FROM tblBuildings AS b INNER JOIN tblUserBuildings AS u ON b.id = u.buildingid ";
                query += " WHERE (b.isBuilding = 'True')";
                query += " ORDER BY b.Building";
                String status;
                dsBuildings = DataHandler.getData(query, null, out status);
                if (dsBuildings != null && dsBuildings.Tables.Count > 0 && dsBuildings.Tables[0].Rows.Count > 0) {
                    foreach (DataRow dr in dsBuildings.Tables[0].Rows) {
                        String building = dr["Building"].ToString();
                        build = dr["Building"].ToString();
                        String dp = dr["DataPath"].ToString();
                        int p = int.Parse(dr["Period"].ToString());
                        StatementBuilding stmtBuilding = new StatementBuilding(build, dp, p, DateTime.Now);
                        if (!buildings.Contains(stmtBuilding)) { buildings.Add(stmtBuilding); }
                    }
                }
            } catch {
            }
        }

        private String HOAMessage1 {
            get {
                String hoaMessage = "Levies are due and payable on the 1st of every month in advance in terms of the Sectional Titles Act 95 of 1986 as amended and or the Articles of Association of the H.O.A.  Failure to compy will result in penalties being charged and electricity supply to the unit being suspended.";
                return hoaMessage;
            }
        }

        private String BCMessage1 {
            get {
                String hoaMessage = "Levies are due and payable on the 1st of every month in advance in terms of the Sectional Titles Act 95 of 1986 as amended.  Failure to compy will result in penalties being charged and electricity supply to the unit being suspended.";
                return hoaMessage;
            }
        }

        private String Message2 {
            get {
                String hoaMessage = "***PLEASE ENSURE THAT ALL PAYMENTS REFLECTS IN OUR NOMINATED ACCOUNT ON OR BEFORE DUE DATE TO AVOID ANY PENALTIES, REFER TO TERMS AND CONDITIONS.***";
                return hoaMessage;
            }
        }

        public List<SStatement> SetBuildings(String buildingName, String buildingPath, int buildingPeriod, bool isHOA) {
            Building build = new Building();
            build.Name = buildingName;
            build.DataPath = buildingPath;
            build.Period = buildingPeriod;
            List<Customer> customers = frmMain.pastel.AddCustomers(buildingName, buildingPath);
            List<SStatement> myStatements = new List<SStatement>();
            int ccount = 0;
            foreach (Customer customer in customers) {
                try {
                    bool stophere = false;
                    SStatement myStatement = new SStatement();
                    myStatement.AccNo = customer.accNumber;
                    List<String> address = new List<string>();
                    address.Add(customer.description);
                    foreach (String addyLine in customer.address) { if (!String.IsNullOrEmpty(addyLine)) { address.Add(addyLine); } }
                    myStatement.Address = address.ToArray();
                    myStatement.BankDetails = (!String.IsNullOrEmpty(frmMain.pastel.GetBankDetails(buildingPath)) ? frmMain.pastel.GetBankDetails(buildingPath) : "");
                    myStatement.BuildingName = buildingName;
                    myStatement.LevyMessage1 = (isHOA ? HOAMessage1 : BCMessage1);
                    myStatement.LevyMessage2 = (!String.IsNullOrEmpty(Message2) ? Message2 : "");
                    myStatement.Message = "";
                    myStatement.StmtDate = new DateTime(2015, 10, 1);
                    double totalDue = 0;
                    List<Transaction> transactions = (new LoadTrans()).LoadTransactions(build, customer, new DateTime(2015, 10, 1), out totalDue);
                    if (transactions != null) { myStatement.Transactions = transactions; }
                    if (stophere) { MessageBox.Show("222"); }
                    myStatement.totalDue = totalDue;
                    myStatement.DebtorEmail = getDebtorEmail(buildingName);

                    myStatement.PrintMe = (customer.statPrintorEmail == 2 || customer.statPrintorEmail == 4 ? false : true);
                    myStatement.EmailMe = (customer.statPrintorEmail == 4 ? false : true);
                    if (customer.Email != null && customer.Email.Length > 0) {
                        List<String> newEmails = new List<string>();
                        foreach (String emailAddress in customer.Email) {
                            if (!emailAddress.Contains("@imp.ad-one.co.za")) { newEmails.Add(emailAddress); }
                        }
                        myStatement.email1 = newEmails.ToArray();
                    }
                    if (stophere) { MessageBox.Show("235"); }

                    myStatements.Add(myStatement);
                } catch (Exception ex) {
                    RaiseEvent("Statement error: " + ex.Message);
                }
                ccount++;
                Application.DoEvents();
            }
            return myStatements;
        }

        private String getDebtorEmail(String buildingName) {
            List<Building> testBuildings = new Buildings(false).buildings;
            String dEmail = "";
            foreach (Building b in testBuildings) {
                if (b.Name == buildingName) {
                    dEmail = b.Debtor;
                }
            }
            //MessageBox.Show("debtor = " + Controller.user.email + " and b email = " + dEmail);
            return (dEmail != "" ? dEmail : "tertia@astrodon.co.za");
        }
    }

    public class Statements {
        public List<SStatement> statements { get; set; }
    }

    public class SStatement {
        public DateTime StmtDate { get; set; }

        public String DebtorEmail { get; set; }

        public String[] email1 { get; set; }

        public String email2 { get; set; }

        public bool PrintMe { get; set; }

        public bool EmailMe { get; set; }

        public String[] Address { get; set; }

        public String AccNo { get; set; }

        public String BankDetails { get; set; }

        public String BuildingName { get; set; }

        public String LevyMessage1 { get; set; }

        public String LevyMessage2 { get; set; }

        public String Message { get; set; }

        public List<Transaction> Transactions { get; set; }

        public double totalDue { get; set; }

        public String bankName { get; set; }

        public String accName { get; set; }

        public String branch { get; set; }

        public String accNumber { get; set; }

        public String pm { get; set; }
    }

    public class Transaction {
        public DateTime TrnDate { get; set; }

        public String Reference { get; set; }

        public String Description { get; set; }

        public double TrnAmt { get; set; }

        public double AccAmt { get; set; }
    }

    public class StatementBuilding {
        private bool process = false;
        private String building = String.Empty;
        private String dataPath = String.Empty;
        private int period = 0;
        public bool hoa = false;
        public bool bc = false;
        private String lastProcessed = String.Empty;

        public bool Process {
            get { return process; }
            set { process = value; }
        }

        public String Building {
            get { return building; }
            set { building = value; }
        }

        public bool HOA {
            get { return hoa; }
        }

        public bool BC {
            get { return bc; }
        }

        public String LastProcessed {
            get { return lastProcessed; }
            set { lastProcessed = value; }
        }

        public String DataPath {
            get { return dataPath; }
            set { dataPath = value; }
        }

        public int Period {
            get { return period; }
            set { period = value; }
        }

        public StatementBuilding(String build, String dp, int p, DateTime lastProcessed) {
            Process = false;
            Building = build;
            DataPath = dp;
            Period = p;
            LastProcessed = lastProcessed.ToString("yyyy/MM/dd");
            if (building.ToLower().Contains("hoa")) {
                hoa = true;
                bc = false;
            } else {
                hoa = false;
                bc = true;
            }
        }
    }

    public class Building {
        public int ID { get; set; }

        public String Name { get; set; }

        public String Abbr { get; set; }

        public String Trust { get; set; }

        public String DataPath { get; set; }

        public int Period { get; set; }

        public String Cash_Book { get; set; }

        public String OwnBank { get; set; }

        public String Cashbook3 { get; set; }

        public int Payments { get; set; }

        public int Receipts { get; set; }

        public int Journal { get; set; }

        public String Centrec_Account { get; set; }

        public String Centrec_Building { get; set; }

        public String Business_Account { get; set; }

        public String Bank { get; set; }

        public String PM { get; set; }

        public String Debtor { get; set; }

        public String Bank_Name { get; set; }

        public String Acc_Name { get; set; }

        public String Bank_Acc_Number { get; set; }

        public String Branch_Code { get; set; }

        public bool Web_Building { get; set; }

        public String letterName { get; set; }

        public String webFolder { get; set; }

        public String pid { get; set; }

        public double reminderFee { get; set; }

        public double reminderSplit { get; set; }

        public double finalFee { get; set; }

        public double finalSplit { get; set; }

        public double disconnectionNoticefee { get; set; }

        public double disconnectionNoticeSplit { get; set; }

        public double summonsFee { get; set; }

        public double summonsSplit { get; set; }

        public double disconnectionFee { get; set; }

        public double disconnectionSplit { get; set; }

        public double handoverFee { get; set; }

        public double handoverSplit { get; set; }

        public String reminderTemplate { get; set; }

        public String finalTemplate { get; set; }

        public String diconnectionNoticeTemplate { get; set; }

        public String summonsTemplate { get; set; }

        public String reminderSMS { get; set; }

        public String finalSMS { get; set; }

        public String disconnectionNoticeSMS { get; set; }

        public String summonsSMS { get; set; }

        public String disconnectionSMS { get; set; }

        public String handoverSMS { get; set; }

        public String addy1 { get; set; }

        public String addy2 { get; set; }

        public String addy3 { get; set; }

        public String addy4 { get; set; }

        public String addy5 { get; set; }

        public Building() {
            reminderTemplate = "";
            finalTemplate = "";
            diconnectionNoticeTemplate = "";
            summonsTemplate = "";
            reminderSMS = "";
            finalSMS = "";
            disconnectionNoticeSMS = "";
            summonsSMS = "";
            disconnectionSMS = "";
            handoverSMS = "";
        }
    }

    public class LoadTrans {

        public int getPeriod(DateTime trnDate, int sbPeriod, out int bPeriod) {
            int myMonth = trnDate.Month;
            myMonth = myMonth - 2;
            myMonth = (myMonth < 1 ? myMonth + 12 : myMonth); //12
            bPeriod = (myMonth - sbPeriod < 1 ? myMonth - sbPeriod + 12 : myMonth - sbPeriod);
            return myMonth;
        }

        public List<Transaction> LoadTransactions(Building building, Customer customer, DateTime transDate, out double totalDue) {
            try {
                int bPeriod;
                int tPeriod = getPeriod(transDate, building.Period, out bPeriod);
                int startperiod = 0;
                int thisYear = DateTime.Now.Year - 2000;
                int endperiod = 0;
                bool isThisYear = false;
                int dataYear = int.Parse(building.DataPath.Substring(building.DataPath.Length - 2, 2));
                bool newYear = thisYear < dataYear;
                if (bPeriod - 2 == 0) {
                    if (thisYear == dataYear) {
                        isThisYear = true;
                        startperiod = 12;
                        endperiod = 102;
                    } else {
                        startperiod = 12;
                        endperiod = 102;
                    }
                } else if (bPeriod - 2 < 0) {
                    if (thisYear == dataYear) {
                        isThisYear = true;
                        startperiod = 111;
                        endperiod = 113;
                    } else {
                        if (dataYear < thisYear) { isThisYear = true; }
                        startperiod = 11;
                        endperiod = 101;
                    }
                } else if (bPeriod - 2 > 0) {
                    isThisYear = true;
                    startperiod = 100 + bPeriod - 2;
                    endperiod = 100 + bPeriod;
                }

                totalDue = 0;

                DateTime trnSEndDate = transDate;
                DateTime trnEndDate = (trnSEndDate.Day != 1 ? new DateTime(trnSEndDate.Year, trnSEndDate.Month, 1) : trnSEndDate);
                List<Transaction> trans = new List<Transaction>();
                List<Transaction> optrans = new List<Transaction>();
                totalDue = 0;

                if (customer != null) {
                    List<Trns> transactions = new List<Trns>();
                    double os = 0;
                    double opBal = 0;

                    if (customer != null) {
                        int opBalPeriod = bPeriod - 3;
                        if (opBalPeriod < 0 && !isThisYear) {
                            opBalPeriod *= -1;
                            if (!newYear) {
                                for (int li = 0; li < customer.lastBal.Length; li++) { opBal += customer.lastBal[li]; }
                                for (int i = 0; i < 12 - opBalPeriod; i++) { opBal += customer.balance[i]; }
                            } else {
                                for (int li = 0; li < (customer.lastBal.Length - (opBalPeriod + 1)); li++) { opBal += customer.lastBal[li]; }
                            }
                        } else if (opBalPeriod == -1 && isThisYear) {
                            opBalPeriod *= -1;
                            for (int li = 0; li < (customer.lastBal.Length - (opBalPeriod + 1)); li++) { opBal += customer.lastBal[li]; }
                        } else if (opBalPeriod < -1 && isThisYear) {
                            opBalPeriod *= -1;
                            for (int li = 0; li < customer.lastBal.Length; li++) { opBal += customer.lastBal[li]; }
                            for (int i = 0; i < 12 - opBalPeriod; i++) { opBal += customer.balance[i]; }
                        } else {
                            for (int li = 0; li < customer.lastBal.Length; li++) { opBal += customer.lastBal[li]; }
                            for (int i = 0; i < bPeriod - 3; i++) { opBal += customer.balance[i]; }
                        }
                        for (int li = 0; li < customer.lastBal.Length; li++) { os += customer.lastBal[li]; }
                        for (int i = 0; i <= (bPeriod - 1 == 0 ? 1 : bPeriod - 1); i++) { os += customer.balance[i]; }
                    } else {
                        os = 0;
                        opBal = 0;
                    }

                    DateTime trnDate = trnEndDate.AddMonths(-2);

                    Transaction optran = new Transaction();
                    optran.AccAmt = os;
                    optran.Description = "Balance Brought Forward";
                    optran.Reference = "";
                    optran.TrnAmt = os;

                    optran.TrnDate = trnDate;
                    transactions = frmMain.pastel.GetTransactions(building.DataPath, startperiod, endperiod, customer.accNumber);
                    transactions.Sort(new TrnsComparer("Date", SortOrder.Ascending));

                    double subtractAmount = 0;
                    foreach (Trns trn in transactions) {
                        Transaction tran = new Transaction();
                        tran.Description = trn.Description;
                        tran.Reference = trn.Reference;
                        tran.TrnAmt = double.Parse(trn.Amount);
                        tran.TrnDate = DateTime.Parse(trn.Date);
                        subtractAmount += double.Parse(trn.Amount);
                        trans.Add(tran);
                    }
                    optran.TrnAmt = opBal;
                    optran.AccAmt = optran.TrnAmt;
                    optrans.Add(optran);
                    foreach (Transaction tran in trans) {
                        opBal += tran.TrnAmt;
                        tran.AccAmt = opBal;
                        optrans.Add(tran);
                    }
                    totalDue = opBal;
                }
                return optrans;
            } catch {
                totalDue = 0;
                return null;
            }
        }
    }

    public class Trns {
        public String Date { get; set; }

        public String Description { get; set; }

        public String Reference { get; set; }

        public String Amount { get; set; }

        public String period { get; set; }

        public String Cumulative { get; set; }
    }

    public class TrnsComparer : IComparer<Trns> {
        private string memberName = string.Empty; // specifies the member name to be sorted
        private SortOrder sortOrder = SortOrder.None; // Specifies the SortOrder.

        /// <summary>
        /// constructor to set the sort column and sort order.
        /// </summary>
        /// <param name="strMemberName"></param>
        /// <param name="sortingOrder"></param>
        public TrnsComparer(string strMemberName, SortOrder sortingOrder) {
            memberName = strMemberName;
            sortOrder = sortingOrder;
        }

        /// <summary>
        /// Compares two Students based on member name and sort order
        /// and return the result.
        /// </summary>
        /// <param name="Student1"></param>
        /// <param name="Student2"></param>
        /// <returns></returns>
        public int Compare(Trns trn1, Trns trn2) {
            int returnValue = 1;
            switch (memberName) {
                case "Date":
                    if (sortOrder == SortOrder.Ascending) {
                        returnValue = trn1.Date.CompareTo(trn2.Date);
                    } else {
                        returnValue = trn2.Date.CompareTo(trn1.Date);
                    }

                    break;

                case "Description":
                    if (sortOrder == SortOrder.Ascending) {
                        returnValue = trn1.Description.CompareTo(trn2.Description);
                    } else {
                        returnValue = trn2.Description.CompareTo(trn1.Description);
                    }

                    break;

                case "Reference":
                    if (sortOrder == SortOrder.Ascending) {
                        returnValue = trn1.Reference.CompareTo(trn2.Reference);
                    } else {
                        returnValue = trn2.Reference.CompareTo(trn1.Reference);
                    }

                    break;

                case "Amount":
                    if (sortOrder == SortOrder.Ascending) {
                        returnValue = trn1.Amount.CompareTo(trn2.Amount);
                    } else {
                        returnValue = trn2.Amount.CompareTo(trn1.Amount);
                    }

                    break;
            }
            return returnValue;
        }
    }

    public class Buildings {

        #region Variables

        public List<Building> buildings;
        private String status = String.Empty;

        #endregion Variables

        #region Queries

        private String buildQuery = "SELECT id, Building, Code, AccNumber, DataPath, Period, Contra, ownbank, cashbook3, payments, receipts, journals, bc, centrec, business, bank, pm, bankName, accName, bankAccNumber, branch, isBuilding,addy1, addy2, addy3, addy4, addy5, web, letterName, pid FROM tblBuildings ORDER BY Building";
        private String feeQuery = "SELECT reminderFee, reminderSplit, finalFee, finalSplit, disconnectionNoticefee, disconnectionNoticeSplit, summonsFee, summonsSplit, disconnectionFee, disconnectionSplit, handoverFee, handoverSplit, reminderTemplate, finalTemplate, diconnectionNoticeTemplate, summonsTemplate, reminderSMS, finalSMS, disconnectionNoticeSMS, summonsSMS, disconnectionSMS, handoverSMS FROM tblBuildingSettings WHERE (buildingID = @buildID)";
        private String buildUserQuery = "SELECT b.id, b.Building, b.Code, b.AccNumber, b.DataPath, b.Period, b.Contra, b.ownbank, b.cashbook3, b.payments, b.receipts, b.journals, b.bc, b.centrec, b.business, b.bank, b.pm, b.bankName, b.accName, b.bankAccNumber, b.branch, b.isBuilding, b.addy1, b.addy2, b.addy3, b.addy4, b.addy5, b.web, b.letterName, b.pid FROM tblBuildings b INNER JOIN tblUserBuildings u ON b.id = u.buildingid WHERE u.userid = @userid ORDER BY b.Building";

        #endregion Queries

        private void LoadBuildings(DataSet ds) {
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    Building b = new Building();
                    b.ID = int.Parse(dr["id"].ToString());
                    b.Name = dr["Building"].ToString();
                    b.Abbr = dr["Code"].ToString();
                    b.Trust = dr["AccNumber"].ToString();
                    b.DataPath = dr["DataPath"].ToString();
                    b.Period = int.Parse(dr["Period"].ToString());
                    b.Cash_Book = dr["Contra"].ToString();
                    b.OwnBank = dr["ownbank"].ToString();
                    b.Cashbook3 = dr["cashbook3"].ToString();
                    b.Payments = int.Parse(dr["payments"].ToString());
                    b.Receipts = int.Parse(dr["receipts"].ToString());
                    b.Journal = int.Parse(dr["journals"].ToString());
                    b.Centrec_Account = dr["bc"].ToString();
                    b.Centrec_Building = dr["centrec"].ToString();
                    b.Business_Account = dr["business"].ToString();
                    b.Bank = dr["bank"].ToString();
                    b.PM = dr["pm"].ToString();
                    b.Debtor = getDebtorEmail(b.ID);
                    b.Bank_Name = dr["bankName"].ToString();
                    b.Acc_Name = dr["accName"].ToString();
                    b.Bank_Acc_Number = dr["bankAccNumber"].ToString();
                    b.Branch_Code = dr["branch"].ToString();
                    b.Web_Building = bool.Parse(dr["isBuilding"].ToString());
                    b.webFolder = dr["web"].ToString();
                    b.letterName = dr["letterName"].ToString();
                    b.addy1 = dr["addy1"].ToString();
                    b.addy2 = dr["addy2"].ToString();
                    b.addy3 = dr["addy3"].ToString();
                    b.addy4 = dr["addy4"].ToString();
                    b.addy5 = dr["addy5"].ToString();
                    b.pid = dr["pid"].ToString();

                    Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
                    sqlParms.Add("@buildID", b.ID);
                    DataSet dsFee = DataHandler.getData(feeQuery, sqlParms, out status);
                    if (dsFee != null && dsFee.Tables.Count > 0 && dsFee.Tables[0].Rows.Count > 0) {
                        DataRow dFee = dsFee.Tables[0].Rows[0];
                        b.reminderFee = double.Parse(dFee["reminderFee"].ToString());
                        b.reminderSplit = double.Parse(dFee["reminderSplit"].ToString());
                        b.finalFee = double.Parse(dFee["finalFee"].ToString());
                        b.finalSplit = double.Parse(dFee["finalSplit"].ToString());
                        b.disconnectionNoticefee = double.Parse(dFee["disconnectionNoticefee"].ToString());
                        b.disconnectionNoticeSplit = double.Parse(dFee["disconnectionNoticeSplit"].ToString());
                        b.summonsFee = double.Parse(dFee["summonsFee"].ToString());
                        b.summonsSplit = double.Parse(dFee["summonsSplit"].ToString());
                        b.disconnectionFee = double.Parse(dFee["disconnectionFee"].ToString());
                        b.disconnectionSplit = double.Parse(dFee["disconnectionSplit"].ToString());

                        b.handoverFee = double.Parse(dFee["handoverFee"].ToString());
                        b.handoverSplit = double.Parse(dFee["handoverSplit"].ToString());
                        b.reminderTemplate = dFee["reminderTemplate"].ToString();
                        b.finalTemplate = dFee["finalTemplate"].ToString();
                        b.diconnectionNoticeTemplate = dFee["diconnectionNoticeTemplate"].ToString();
                        b.summonsTemplate = dFee["summonsTemplate"].ToString();
                        b.reminderSMS = dFee["reminderSMS"].ToString();
                        b.finalSMS = dFee["finalSMS"].ToString();
                        b.disconnectionNoticeSMS = dFee["disconnectionNoticeSMS"].ToString();
                        b.summonsSMS = dFee["summonsSMS"].ToString();
                        b.disconnectionSMS = dFee["disconnectionSMS"].ToString();
                        b.handoverSMS = dFee["handoverSMS"].ToString();
                    } else {
                        b.reminderFee = 0;
                        b.reminderSplit = 0;
                        b.finalFee = 0;
                        b.finalSplit = 0;
                        b.disconnectionNoticefee = 0;
                        b.disconnectionNoticeSplit = 0;
                        b.summonsFee = 0;
                        b.summonsSplit = 0;
                        b.disconnectionFee = 0;
                        b.disconnectionSplit = 0;
                        b.handoverFee = 0;
                        b.handoverSplit = 0;

                        b.reminderTemplate = "";
                        b.finalTemplate = "";
                        b.diconnectionNoticeTemplate = "";
                        b.summonsTemplate = "";
                        b.reminderSMS = "";
                        b.finalSMS = "";
                        b.disconnectionNoticeSMS = "";
                        b.summonsSMS = "";
                        b.disconnectionSMS = "";
                        b.handoverSMS = "";
                    }

                    buildings.Add(b);
                }
            }
        }

        public Buildings(bool addNew) {
            buildings = new List<Building>();

            if (addNew) {
                Building b = new Building();
                b.ID = 0;
                b.Name = "Add new building";
                buildings.Add(b);
            }
            LoadBuildings(DataHandler.getData(buildQuery, null, out status));
        }

        public Buildings(bool addNew, String nameValue) {
            buildings = new List<Building>();
            if (addNew) {
                Building b = new Building();
                b.ID = 0;
                b.Name = nameValue;
                buildings.Add(b);
            }
            LoadBuildings(DataHandler.getData(buildQuery, null, out status));
        }

        public Buildings(int userID) {
            buildings = new List<Building>();
            Dictionary<String, Object> sqlParms = new Dictionary<string, object>();
            sqlParms.Add("@userid", userID);
            LoadBuildings(DataHandler.getData(buildUserQuery, sqlParms, out status));
        }

        public Building GetBuilding(int id) {
            String buildQuery = "SELECT id, Building, Code, AccNumber, DataPath, Period, Contra, ownbank, cashbook3, payments, receipts, journals, bc, centrec, business, bank, pm, bankName, accName, bankAccNumber, ";
            buildQuery += " branch, isBuilding,addy1, addy2, addy3, addy4, addy5, web, letterName, pid FROM tblBuildings ORDER BY Building";
            Building b = new Building();
            DataSet ds = DataHandler.getData(buildQuery, null, out status);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    b.ID = int.Parse(dr["id"].ToString());
                    b.Name = dr["Building"].ToString();
                    b.Abbr = dr["Code"].ToString();
                    b.Trust = dr["AccNumber"].ToString();
                    b.DataPath = dr["DataPath"].ToString();
                    b.Period = int.Parse(dr["Period"].ToString());
                    b.Cash_Book = dr["Contra"].ToString();
                    b.OwnBank = dr["ownbank"].ToString();
                    b.Cashbook3 = dr["cashbook3"].ToString();
                    b.Payments = int.Parse(dr["payments"].ToString());
                    b.Receipts = int.Parse(dr["receipts"].ToString());
                    b.Journal = int.Parse(dr["journals"].ToString());
                    b.Centrec_Account = dr["bc"].ToString();
                    b.Centrec_Building = dr["centrec"].ToString();
                    b.Business_Account = dr["business"].ToString();
                    b.Bank = dr["bank"].ToString();
                    b.PM = dr["pm"].ToString();
                    b.Debtor = getDebtorEmail(b.ID);
                    b.Bank_Name = dr["bankName"].ToString();
                    b.Acc_Name = dr["accName"].ToString();
                    b.Bank_Acc_Number = dr["bankAccNumber"].ToString();
                    b.Branch_Code = dr["branch"].ToString();
                    b.Web_Building = bool.Parse(dr["isBuilding"].ToString());
                    b.webFolder = dr["web"].ToString();
                    b.letterName = dr["letterName"].ToString();
                    b.addy1 = dr["addy1"].ToString();
                    b.addy2 = dr["addy2"].ToString();
                    b.addy3 = dr["addy3"].ToString();
                    b.addy4 = dr["addy4"].ToString();
                    b.addy5 = dr["addy5"].ToString();
                    b.pid = dr["pid"].ToString();
                }
            }
            return b;
        }

        public String getDebtorName(int bid) {
            String query = "SELECT DISTINCT tblUsers.name FROM tblUserBuildings INNER JOIN tblUsers ON tblUserBuildings.userid = tblUsers.id WHERE (tblUserBuildings.buildingid = " + bid.ToString() + ") AND (tblUsers.usertype = 3) ";
            DataSet dsDebtor = DataHandler.getData(query, null, out status);
            if (dsDebtor != null && dsDebtor.Tables.Count > 0 && dsDebtor.Tables[0].Rows.Count > 0) {
                String debtorEmail = dsDebtor.Tables[0].Rows[0]["name"].ToString();
                return debtorEmail;
            } else {
                return "";
            }
        }

        public String getDebtorEmail(int bid) {
            String query = "SELECT DISTINCT tblUsers.email FROM tblUserBuildings INNER JOIN tblUsers ON tblUserBuildings.userid = tblUsers.id WHERE (tblUserBuildings.buildingid = " + bid.ToString() + ") AND (tblUsers.usertype = 3) ";
            DataSet dsDebtor = DataHandler.getData(query, null, out status);
            if (dsDebtor != null && dsDebtor.Tables.Count > 0 && dsDebtor.Tables[0].Rows.Count > 0) {
                String debtorEmail = dsDebtor.Tables[0].Rows[0]["email"].ToString();
                return debtorEmail;
            } else {
                return "";
            }
        }
    }

    public class PDF {
        private PdfContentByte _pcb;
        private BaseFont bf;
        private BaseFont bf2;
        private iTextSharp.text.Font fontT = FontFactory.GetFont("Calibri", 6f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        private iTextSharp.text.Font font = FontFactory.GetFont("Calibri", 7f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        private iTextSharp.text.Font fontB = FontFactory.GetFont("Calibri", 7f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        private iTextSharp.text.Font fontB2 = FontFactory.GetFont("Helvetica", 11f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        private iTextSharp.text.Font fontBig = FontFactory.GetFont("Calibri", 12f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        private iTextSharp.text.Font fontBU = FontFactory.GetFont("Calibri", 8.5f, iTextSharp.text.Font.BOLD | iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
        private iTextSharp.text.Font fontR = FontFactory.GetFont("Calibri", 8.5f, iTextSharp.text.Font.BOLD, BaseColor.RED);
        private iTextSharp.text.Font fontRI = FontFactory.GetFont("Calibri", 8.5f, iTextSharp.text.Font.BOLD | iTextSharp.text.Font.ITALIC, BaseColor.RED);
        private iTextSharp.text.Font fontCertificate = FontFactory.GetFont("Monotype Corsiva", 20f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        private iTextSharp.text.Font fontCert = FontFactory.GetFont("Calibri", 11f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        private iTextSharp.text.Font fontCertB = FontFactory.GetFont("Calibri", 11f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        private iTextSharp.text.Font fontCertTB = FontFactory.GetFont("Times New Roman", 12f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        private iTextSharp.text.Font fontPA = FontFactory.GetFont("Arial Narrow", 11f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        private iTextSharp.text.Font fontE = FontFactory.GetFont("Arial", 11f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        private iTextSharp.text.Font fontPAI = FontFactory.GetFont("Arial Narrow", 11f, iTextSharp.text.Font.NORMAL | iTextSharp.text.Font.ITALIC, BaseColor.BLACK);
        private iTextSharp.text.Font fontPAU = FontFactory.GetFont("Arial Narrow", 11f, iTextSharp.text.Font.NORMAL | iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
        private iTextSharp.text.Font fontPAIU = FontFactory.GetFont("Arial Narrow", 11f, iTextSharp.text.Font.NORMAL | iTextSharp.text.Font.ITALIC | iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
        private iTextSharp.text.Font fontPAB = FontFactory.GetFont("Arial Narrow", 11f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        private iTextSharp.text.Font fontPABI = FontFactory.GetFont("Arial Narrow", 11f, iTextSharp.text.Font.BOLD | iTextSharp.text.Font.ITALIC, BaseColor.BLACK);
        private iTextSharp.text.Font fontPABU = FontFactory.GetFont("Arial Narrow", 11f, iTextSharp.text.Font.BOLD | iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
        private iTextSharp.text.Font fontPABIU = FontFactory.GetFont("Arial Narrow", 11f, iTextSharp.text.Font.BOLD | iTextSharp.text.Font.ITALIC | iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);

        //
        private ClearanceObject clr = null;

        private String folderPath = "";

        public PDF() {
            bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
            String clearanceFolder = "Clearances";
            String appData = AppDomain.CurrentDomain.BaseDirectory;
            folderPath = Path.Combine(appData, clearanceFolder);
            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }
        }

        private String statementFolder = String.Empty;

        public PDF(bool statementRun) {
            bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            bf2 = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
            statementFolder = "statements";
            String serverDrive = "K:\\Debtors System";
            //String serverDrive = AppDomain.CurrentDomain.BaseDirectory;
            folderPath = Path.Combine(serverDrive, statementFolder);
            if (!Directory.Exists(folderPath)) {
                folderPath = Path.Combine("C:\\Pastel11\\Debtors System", statementFolder);
            }
        }

        public bool CreateStatement(SStatement statement, bool isBuilding, out String fName) {
            bool success = false;
            String message = "";
            message = "Starting";

            #region Create Document

            String templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stmt_template.pdf");
            PdfReader reader = new PdfReader(templateFile);
            fName = String.Empty;
            Document document = new Document();
            //PdfReader reader = null;
            FileStream stream;
            try {
                fName = Path.Combine(folderPath, String.Format("{0} - statement - {1}_{2}.pdf", statement.AccNo.Replace(@"/", "-").Replace(@"\", "-"), DateTime.Now.ToString("dd-MMMM-yyyy"), (isBuilding ? "" : "R")));
                if (File.Exists(fName)) { File.Delete(fName); }
                stream = new FileStream(fName, FileMode.CreateNew);
            } catch {
                fName = String.Empty;
                return false;
            }
            message = "creating";

            #endregion Create Document

            try {
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();
                PdfTemplate background = writer.GetImportedPage(reader, 1);

                int transactionNumber = 0;
                while (transactionNumber < statement.Transactions.Count) {
                    int newTrnNumber = 0;
                    CreateStatements(statement, ref document, writer, background, transactionNumber, out newTrnNumber);
                    transactionNumber = newTrnNumber;
                }

                document.Close();
                writer.Close();
                stream.Close();
                if (reader != null) { reader.Close(); }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            } finally {
            }
            success = true;

            return success;
        }

        public bool CreateStatement(SStatement statement, DateTime stmtDate, bool isBuilding, out String fName) {
            bool success = false;
            String message = "";
            message = "Starting";

            #region Create Document

            String templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stmt_template.pdf");
            PdfReader reader = new PdfReader(templateFile);
            fName = String.Empty;
            Document document = new Document();
            //PdfReader reader = null;
            FileStream stream;
            try {
                fName = Path.Combine(folderPath, String.Format("{0} - statement - {1}_{2}.pdf", statement.AccNo.Replace(@"/", "-").Replace(@"\", "-"), stmtDate.ToString("dd-MMMM-yyyy"), (isBuilding ? "" : "R")));
                if (File.Exists(fName)) { File.Delete(fName); }
                stream = new FileStream(fName, FileMode.CreateNew);
            } catch {
                fName = String.Empty;
                return false;
            }
            message = "creating";

            #endregion Create Document

            try {
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();
                PdfTemplate background = writer.GetImportedPage(reader, 1);

                int transactionNumber = 0;
                while (transactionNumber < statement.Transactions.Count) {
                    int newTrnNumber = 0;
                    CreateStatements(statement, ref document, writer, background, transactionNumber, out newTrnNumber);
                    transactionNumber = newTrnNumber;
                }

                document.Close();
                writer.Close();
                stream.Close();
                if (reader != null) { reader.Close(); }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            } finally {
            }
            success = true;

            return success;
        }

        public void CreateStatements(SStatement statement, ref Document document, PdfWriter writer, PdfTemplate background, int transNumber, out int newTrnNumber) {
            // Create a page in the document and add it to the bottom layer
            document.NewPage();
            _pcb = writer.DirectContentUnder;
            _pcb.AddTemplate(background, 0, 0);

            _pcb = writer.DirectContent;
            SetFont7();

            _pcb.BeginText();

            _pcb.ShowTextAligned(0, statement.StmtDate.ToString("yyyy/MM/dd"), 350, 780, 0);
            _pcb.EndText();

            _pcb.BeginText();
            _pcb.ShowTextAligned(0, statement.AccNo, 350, 740, 0);
            _pcb.EndText();

            _pcb.BeginText();
            _pcb.ShowTextAligned(0, statement.pm, 350, 700, 0);
            _pcb.EndText();

            _pcb.BeginText();
            _pcb.ShowTextAligned(0, statement.DebtorEmail, 350, 660, 0);
            _pcb.EndText();

            int startY = 700;
            foreach (String addyLine in statement.Address) {
                _pcb.BeginText();
                _pcb.ShowTextAligned(0, addyLine, 50, startY, 0);
                _pcb.EndText();
                startY -= 15;
            }

            #region table

            Paragraph paragraphSpacer = new Paragraph(" ");
            for (int i = 0; i < 10; i++) {
                document.Add(paragraphSpacer);
            }

            PdfPTable table = new PdfPTable(1);
            table.TotalWidth = 220;
            table.HorizontalAlignment = 2;
            table.LockedWidth = true;
            float[] widths = new float[] { 220 };
            table.SetWidths(widths);
            table.DefaultCell.Border = 0;

            //#region InvHeader
            PdfPCell cell0 = new PdfPCell(new Paragraph(statement.LevyMessage1, font));
            cell0.HorizontalAlignment = 0;
            cell0.Colspan = 1;
            cell0.Border = 0;
            table.AddCell(cell0);
            cell0 = new PdfPCell(new Paragraph(statement.LevyMessage2, fontB));
            cell0.HorizontalAlignment = 0;
            cell0.Colspan = 1;
            cell0.Border = 0;
            table.AddCell(cell0);
            document.Add(table);

            document.Add(paragraphSpacer);
            document.Add(paragraphSpacer);

            table = new PdfPTable(7);
            table.TotalWidth = 510;
            table.HorizontalAlignment = 0;
            table.LockedWidth = true;
            widths = new float[] { 50, 50, 50, 50, 200, 100, 110 };
            table.SetWidths(widths);

            PdfPCell cell1 = new PdfPCell(new Paragraph("Date", fontB));
            cell1.HorizontalAlignment = 0;
            cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell1.Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER;
            cell1.BorderColorTop = BaseColor.BLACK;
            cell1.BorderColorLeft = BaseColor.BLACK;
            cell1.BorderColorBottom = BaseColor.DARK_GRAY;
            cell1.BorderColorRight = BaseColor.WHITE;
            table.AddCell(cell1);

            PdfPCell cell2 = new PdfPCell(new Paragraph("Reference", fontB));
            cell2.HorizontalAlignment = 0;
            cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell2.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell2.BorderColorTop = BaseColor.BLACK;
            cell2.BorderColorLeft = BaseColor.WHITE;
            cell2.BorderColorBottom = BaseColor.DARK_GRAY;
            cell2.BorderColorRight = BaseColor.WHITE;
            table.AddCell(cell2);

            PdfPCell cell3 = new PdfPCell(new Paragraph("Description", fontB));
            cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell3.Colspan = 3;
            cell3.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell3.BorderColorTop = BaseColor.BLACK;
            cell3.BorderColorLeft = BaseColor.WHITE;
            cell3.BorderColorBottom = BaseColor.DARK_GRAY;
            cell3.BorderColorRight = BaseColor.WHITE;
            cell3.HorizontalAlignment = 0;
            table.AddCell(cell3);

            PdfPCell cell4 = new PdfPCell(new Paragraph("Transaction Amount", fontB));
            cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell4.HorizontalAlignment = 1;
            cell4.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell4.BorderColorTop = BaseColor.BLACK;
            cell4.BorderColorLeft = BaseColor.WHITE;
            cell4.BorderColorBottom = BaseColor.DARK_GRAY;
            cell4.BorderColorRight = BaseColor.WHITE;
            table.AddCell(cell4);

            PdfPCell cell5 = new PdfPCell(new Paragraph("Accumulated Balance", fontB));
            cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell5.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER;
            cell5.BorderColorTop = BaseColor.BLACK;
            cell5.BorderColorLeft = BaseColor.WHITE;
            cell5.BorderColorBottom = BaseColor.DARK_GRAY;
            cell5.BorderColorRight = BaseColor.BLACK;
            cell5.HorizontalAlignment = 1;
            table.AddCell(cell5);

            int transCount = 30;
            int transMax = transNumber + 30;
            if (transMax > statement.Transactions.Count) { transMax = statement.Transactions.Count; }
            for (int i = transNumber; i < transMax; i++) {
                Transaction trn = statement.Transactions[i];
                cell0 = new PdfPCell(new Paragraph(trn.TrnDate.ToString("yyyy/MM/dd"), font));
                cell0.Border = Rectangle.LEFT_BORDER;
                cell0.BorderColorLeft = BaseColor.BLACK;
                cell0.HorizontalAlignment = 0;
                table.AddCell(cell0);

                cell0 = new PdfPCell(new Paragraph(trn.Reference, font));
                cell0.Border = Rectangle.NO_BORDER;
                cell0.HorizontalAlignment = 0;
                cell0.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell0);

                cell0 = new PdfPCell(new Paragraph(trn.Description, font));
                cell0.Border = Rectangle.NO_BORDER;
                cell0.Colspan = 3;
                cell0.HorizontalAlignment = 0;
                table.AddCell(cell0);

                cell0 = new PdfPCell(new Paragraph(trn.TrnAmt.ToString("#,##0.00"), font));
                cell0.HorizontalAlignment = 2;
                cell0.Border = Rectangle.NO_BORDER;
                cell0.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell0);

                cell0 = new PdfPCell(new Paragraph(trn.AccAmt.ToString("#,##0.00"), font));
                cell0.Border = Rectangle.RIGHT_BORDER;
                cell0.BorderColorRight = BaseColor.BLACK;
                cell0.HorizontalAlignment = 2;
                table.AddCell(cell0);
                transCount--;
                transNumber++;
            }
            newTrnNumber = transNumber;
            for (int i = 0; i < transCount; i++) {
                cell0 = new PdfPCell(new Paragraph(" ", font));
                cell0.Border = Rectangle.LEFT_BORDER;
                cell0.BorderColorLeft = BaseColor.BLACK;
                cell0.HorizontalAlignment = 0;
                table.AddCell(cell0);

                cell0 = new PdfPCell(new Paragraph(" ", font));
                cell0.Border = Rectangle.NO_BORDER;
                cell0.HorizontalAlignment = 0;
                cell0.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell0);

                cell0 = new PdfPCell(new Paragraph(" ", font));
                cell0.Border = Rectangle.NO_BORDER;
                cell0.Colspan = 3;
                cell0.HorizontalAlignment = 0;
                table.AddCell(cell0);

                cell0 = new PdfPCell(new Paragraph(" ", font));
                cell0.HorizontalAlignment = 2;
                cell0.Border = Rectangle.NO_BORDER;
                cell0.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell0);

                cell0 = new PdfPCell(new Paragraph(" ", font));
                cell0.Border = Rectangle.RIGHT_BORDER;
                cell0.BorderColorRight = BaseColor.BLACK;
                cell0.HorizontalAlignment = 2;
                table.AddCell(cell0);
            }
            //#region Totals

            cell0 = new PdfPCell(new Paragraph("***PLEASE USE THE FOLLOWING", font));
            cell0.Border = Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.Colspan = 3;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.AccNo, fontB));
            cell0.HorizontalAlignment = 0;
            cell0.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell0.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorBottom = BaseColor.BLACK;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("AS REFERENCE***", font));
            cell0.HorizontalAlignment = 0;
            cell0.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorRight = BaseColor.BLACK;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("TOTAL DUE", font));
            cell0.HorizontalAlignment = 0;
            cell0.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.totalDue.ToString("#,##0.00"), fontB));
            cell0.HorizontalAlignment = 2;
            cell0.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell0.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorRight = BaseColor.BLACK;
            table.AddCell(cell0);

            document.Add(table);

            document.Add(paragraphSpacer);

            table = new PdfPTable(2);
            table.TotalWidth = 510;
            table.HorizontalAlignment = 0;
            table.LockedWidth = true;
            widths = new float[] { 125, 385 };
            table.SetWidths(widths);

            cell0 = new PdfPCell(new Paragraph("BANKING DETAILS:", font));
            cell0.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.BankDetails, font));
            cell0.HorizontalAlignment = 0;
            cell0.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorRight = BaseColor.BLACK;
            table.AddCell(cell0);

            document.Add(table);
            int yAdjustment = 50;

            if (!String.IsNullOrEmpty(statement.Message)) {
                yAdjustment = 0;
                document.Add(paragraphSpacer);

                table = new PdfPTable(1);
                table.TotalWidth = 510;
                table.HorizontalAlignment = 0;
                table.LockedWidth = true;
                widths = new float[] { 510 };
                table.SetWidths(widths);
                table.DefaultCell.Border = 1;

                cell0 = new PdfPCell(new Paragraph(statement.Message, font));
                cell0.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                cell0.BorderColorTop = BaseColor.BLACK;
                cell0.BorderColorBottom = BaseColor.BLACK;
                cell0.BorderColorLeft = BaseColor.BLACK;
                cell0.BorderColorRight = BaseColor.BLACK;
                cell0.HorizontalAlignment = 1;
                table.AddCell(cell0);

                document.Add(table);
            }
            document.Add(paragraphSpacer);

            SetFont(24);
            _pcb.BeginText();
            _pcb.SetColorFill(BaseColor.LIGHT_GRAY);
            _pcb.ShowTextAligned(0, "SAMPLE DEPOSIT SLIP", 150, 20 + yAdjustment, 10);
            _pcb.EndText();

            table = new PdfPTable(4);
            table.TotalWidth = 510;
            table.HorizontalAlignment = 0;
            table.LockedWidth = true;
            widths = new float[] { 127.5f, 127.5f, 127.5f, 127.5f };
            table.SetWidths(widths);

            cell0 = new PdfPCell(new Paragraph(statement.bankName, fontB));
            cell0.Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("DEPOSIT SLIP", fontB));
            cell0.Border = Rectangle.TOP_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("DATE", fontB));
            cell0.Border = Rectangle.TOP_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.StmtDate.ToString("yyyy/MM/dd"), fontB));
            cell0.Border = Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorRight = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("Deposit To", fontB));
            cell0.Border = Rectangle.LEFT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.accName, fontB));
            cell0.Border = Rectangle.NO_BORDER;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("Branch Code", fontB));
            cell0.Border = Rectangle.NO_BORDER;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.branch, fontB));
            cell0.Border = Rectangle.RIGHT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorRight = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("Account Number", fontB));
            cell0.Border = Rectangle.LEFT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.accNumber, fontB));
            cell0.HorizontalAlignment = 0;
            cell0.Border = Rectangle.NO_BORDER;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(" ", fontB));
            cell0.Border = Rectangle.NO_BORDER;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(" ", fontB));
            cell0.Border = Rectangle.RIGHT_BORDER;
            cell0.BorderColorTop = BaseColor.BLACK;
            cell0.BorderColorRight = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("Reference", fontB));
            cell0.Border = Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.AccNo, fontB));
            cell0.Border = Rectangle.BOTTOM_BORDER;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorLeft = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph("Total", fontB));
            cell0.Border = Rectangle.BOTTOM_BORDER;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            cell0 = new PdfPCell(new Paragraph(statement.totalDue.ToString("#,##0.00"), fontB));
            cell0.Border = Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;
            cell0.BorderColorBottom = BaseColor.BLACK;
            cell0.BorderColorRight = BaseColor.BLACK;
            cell0.HorizontalAlignment = 0;
            table.AddCell(cell0);

            document.Add(table);

            #endregion table
        }

        private Paragraph CreateParagraph(String text, Font f) {
            Paragraph p = new Paragraph(text, f);
            p.Alignment = Element.ALIGN_CENTER;
            return p;
        }

        private Paragraph CreateParagraph(String text, Font f, int alignment) {
            Paragraph p = new Paragraph(text, f);
            p.Alignment = alignment;
            return p;
        }

        #region Content Writing

        private void PrintXAxis(int y) {
            SetFont7();
            int x = 600;
            while (x >= 0) {
                if (x % 20 == 0) {
                    PrintTextCentered("" + x, x, y + 8);
                    PrintTextCentered("|", x, y);
                } else {
                    PrintTextCentered(".", x, y);
                }
                x -= 5;
            }
        }

        private void PrintYAxis(int x) {
            SetFont7();
            int y = 800;
            while (y >= 0) {
                if (y % 20 == 0) {
                    PrintText("__ " + y, x, y);
                } else {
                    PrintText("_", x, y);
                }
                y = y - 5;
            }
        }

        private void SetFont7() {
            _pcb.SetFontAndSize(bf, 7);
        }

        private void SetFont(float size) {
            _pcb.SetFontAndSize(bf, size);
        }

        private void SetFont(float size, bool isBold) {
            _pcb.SetFontAndSize((isBold ? bf2 : bf), size);
        }

        private void SetFont18() {
            _pcb.SetFontAndSize(bf, 18);
        }

        private void SetFont36() {
            _pcb.SetFontAndSize(bf, 36);
        }

        private void PrintText(string text, int x, int y) {
            _pcb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, y, 0);
        }

        private void PrintTextRight(string text, int x, int y) {
            _pcb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, text, x, y, 0);
        }

        private void PrintTextCentered(string text, int x, int y) {
            _pcb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, text, x, y, 0);
        }

        #endregion Content Writing
    }

    public class ClearanceObject {
        public String preparedBy { get; set; }

        public String buildingCode { get; set; }

        public String customerCode { get; set; }

        public String trfAttorneys { get; set; }

        public String attReference { get; set; }

        public String fax { get; set; }

        public String complex { get; set; }

        public String unitNo { get; set; }

        public String seller { get; set; }

        public String purchaser { get; set; }

        public String purchaserAddress { get; set; }

        public String purchaserTel { get; set; }

        public String purchaserEmail { get; set; }

        public String notes { get; set; }

        public double clearanceFee { get; set; }

        public double astrodonTotal { get; set; }

        public DateTime certDate { get; set; }

        public bool registered { get; set; }

        public DateTime regDate { get; set; }

        public DateTime validTo { get; set; }

        public List<ClearanceObjectTrans> Trans { get; set; }

        public bool extClearance { get; set; }

        public ClearanceObject(int clearanceID, DataRow dr, DataSet ds) {
            try {
                preparedBy = dr["preparedBy"].ToString();
                buildingCode = dr["buildingCode"].ToString();
                customerCode = dr["customerCode"].ToString();
                trfAttorneys = dr["trfAttorneys"].ToString();
                attReference = dr["attReference"].ToString();
                fax = dr["fax"].ToString();
                complex = dr["complex"].ToString();
                unitNo = dr["unitNo"].ToString();
                seller = dr["seller"].ToString();
                purchaser = dr["purchaser"].ToString();
                purchaserAddress = dr["purchaserAddress"].ToString();
                purchaserTel = dr["purchaserTel"].ToString();
                purchaserEmail = dr["purchaserEmail"].ToString();
                notes = dr["notes"].ToString();
                clearanceFee = double.Parse(dr["clearanceFee"].ToString());
                astrodonTotal = double.Parse(dr["astrodonTotal"].ToString());
                certDate = DateTime.Parse(dr["certDate"].ToString());
                regDate = DateTime.Parse(dr["regDate"].ToString());
                validTo = DateTime.Parse(dr["validDate"].ToString());
                registered = bool.Parse(dr["registered"].ToString());
                extClearance = bool.Parse(dr["extClearance"].ToString());
                Trans = new List<ClearanceObjectTrans>();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                    foreach (DataRow dr2 in ds.Tables[0].Rows) {
                        Trans.Add(new ClearanceObjectTrans(dr2));
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + ": " + clearanceID.ToString());
            }
        }
    }

    public class ClearanceObjectTrans {
        public String description { get; set; }

        public double amount { get; set; }

        public ClearanceObjectTrans(DataRow dr) {
            description = dr["description"].ToString();
            amount = double.Parse(dr["amount"].ToString());
        }
    }
}
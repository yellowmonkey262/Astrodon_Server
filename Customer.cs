﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Server {

    public class Customer {

        public Customer() {
        }

        public Customer(String CustomerString) {
            String[] stringSplitter = new String[] { "|" };
            int custBits = 0;
            try {
                String[] customerBits = CustomerString.Split(stringSplitter, StringSplitOptions.None);
                custBits = customerBits.Length;
                try {
                    category = customerBits[1];
                    if (category.Length < 2) { category = "0" + category; }
                } catch { category = "0"; }
                try { accNumber = customerBits[2]; } catch { accNumber = ""; }
                try { description = customerBits[3]; } catch { description = ""; }

                try { setBalance(double.Parse(customerBits[4]), 0); } catch { setBalance(0, 0); }
                try { setBalance(double.Parse(customerBits[5]), 1); } catch { setBalance(0, 1); }
                try { setBalance(double.Parse(customerBits[6]), 2); } catch { setBalance(0, 2); }
                try { setBalance(double.Parse(customerBits[7]), 3); } catch { setBalance(0, 3); }
                try { setBalance(double.Parse(customerBits[8]), 4); } catch { setBalance(0, 4); }
                try { setBalance(double.Parse(customerBits[9]), 5); } catch { setBalance(0, 5); }
                try { setBalance(double.Parse(customerBits[10]), 6); } catch { setBalance(0, 6); }
                try { setBalance(double.Parse(customerBits[11]), 7); } catch { setBalance(0, 7); }
                try { setBalance(double.Parse(customerBits[12]), 8); } catch { setBalance(0, 8); }
                try { setBalance(double.Parse(customerBits[13]), 9); } catch { setBalance(0, 9); }
                try { setBalance(double.Parse(customerBits[14]), 10); } catch { setBalance(0, 10); }
                try { setBalance(double.Parse(customerBits[15]), 11); } catch { setBalance(0, 11); }
                try { setBalance(double.Parse(customerBits[16]), 12); } catch { setBalance(0, 12); }

                try { setLastBalance(double.Parse(customerBits[17]), 0); } catch { setLastBalance(0, 0); }
                try { setLastBalance(double.Parse(customerBits[18]), 1); } catch { setLastBalance(0, 1); }
                try { setLastBalance(double.Parse(customerBits[19]), 2); } catch { setLastBalance(0, 2); }
                try { setLastBalance(double.Parse(customerBits[20]), 3); } catch { setLastBalance(0, 3); }
                try { setLastBalance(double.Parse(customerBits[21]), 4); } catch { setLastBalance(0, 4); }
                try { setLastBalance(double.Parse(customerBits[22]), 5); } catch { setLastBalance(0, 5); }
                try { setLastBalance(double.Parse(customerBits[23]), 6); } catch { setLastBalance(0, 6); }
                try { setLastBalance(double.Parse(customerBits[24]), 7); } catch { setLastBalance(0, 7); }
                try { setLastBalance(double.Parse(customerBits[25]), 8); } catch { setLastBalance(0, 8); }
                try { setLastBalance(double.Parse(customerBits[26]), 9); } catch { setLastBalance(0, 9); }
                try { setLastBalance(double.Parse(customerBits[27]), 10); } catch { setLastBalance(0, 10); }
                try { setLastBalance(double.Parse(customerBits[28]), 11); } catch { setLastBalance(0, 11); }
                try { setLastBalance(double.Parse(customerBits[29]), 12); } catch { setLastBalance(0, 12); }

                try { setSalesBalance(double.Parse(customerBits[30]), 0); } catch { setSalesBalance(0, 0); }
                try { setSalesBalance(double.Parse(customerBits[31]), 1); } catch { setSalesBalance(0, 1); }
                try { setSalesBalance(double.Parse(customerBits[32]), 2); } catch { setSalesBalance(0, 2); }
                try { setSalesBalance(double.Parse(customerBits[33]), 3); } catch { setSalesBalance(0, 3); }
                try { setSalesBalance(double.Parse(customerBits[34]), 4); } catch { setSalesBalance(0, 4); }
                try { setSalesBalance(double.Parse(customerBits[35]), 5); } catch { setSalesBalance(0, 5); }
                try { setSalesBalance(double.Parse(customerBits[36]), 6); } catch { setSalesBalance(0, 6); }
                try { setSalesBalance(double.Parse(customerBits[37]), 7); } catch { setSalesBalance(0, 7); }
                try { setSalesBalance(double.Parse(customerBits[38]), 8); } catch { setSalesBalance(0, 8); }
                try { setSalesBalance(double.Parse(customerBits[39]), 9); } catch { setSalesBalance(0, 9); }
                try { setSalesBalance(double.Parse(customerBits[40]), 10); } catch { setSalesBalance(0, 10); }
                try { setSalesBalance(double.Parse(customerBits[41]), 11); } catch { setSalesBalance(0, 11); }
                try { setSalesBalance(double.Parse(customerBits[42]), 12); } catch { setSalesBalance(0, 12); }

                try { setLastSalesBalance(double.Parse(customerBits[43]), 0); } catch { setLastSalesBalance(0, 0); }
                try { setLastSalesBalance(double.Parse(customerBits[44]), 1); } catch { setLastSalesBalance(0, 1); }
                try { setLastSalesBalance(double.Parse(customerBits[45]), 2); } catch { setLastSalesBalance(0, 2); }
                try { setLastSalesBalance(double.Parse(customerBits[46]), 3); } catch { setLastSalesBalance(0, 3); }
                try { setLastSalesBalance(double.Parse(customerBits[47]), 4); } catch { setLastSalesBalance(0, 4); }
                try { setLastSalesBalance(double.Parse(customerBits[48]), 5); } catch { setLastSalesBalance(0, 5); }
                try { setLastSalesBalance(double.Parse(customerBits[49]), 6); } catch { setLastSalesBalance(0, 6); }
                try { setLastSalesBalance(double.Parse(customerBits[50]), 7); } catch { setLastSalesBalance(0, 7); }
                try { setLastSalesBalance(double.Parse(customerBits[51]), 8); } catch { setLastSalesBalance(0, 8); }
                try { setLastSalesBalance(double.Parse(customerBits[52]), 9); } catch { setLastSalesBalance(0, 9); }
                try { setLastSalesBalance(double.Parse(customerBits[53]), 10); } catch { setLastSalesBalance(0, 10); }
                try { setLastSalesBalance(double.Parse(customerBits[54]), 11); } catch { setLastSalesBalance(0, 11); }
                try { setLastSalesBalance(double.Parse(customerBits[55]), 12); } catch { setLastSalesBalance(0, 12); }

                try { setAddress(customerBits[56], 0); } catch { setAddress("", 0); }
                try { setAddress(customerBits[57], 1); } catch { setAddress("", 1); }
                try { setAddress(customerBits[58], 2); } catch { setAddress("", 2); }
                try { setAddress(customerBits[59], 3); } catch { setAddress("", 3); }
                try { setAddress(customerBits[60], 4); } catch { setAddress("", 4); }

                try { taxCode = int.Parse(customerBits[61]); } catch { taxCode = 0; }
                try { exemptRef = customerBits[62]; } catch { exemptRef = ""; }
                try { settlementTerms = int.Parse(customerBits[63]); } catch { settlementTerms = 0; }
                try { paymentTerms = int.Parse(customerBits[64]); } catch { paymentTerms = 0; }

                try { discount = int.Parse(customerBits[65]); } catch { discount = 0; }
                try { lastCrDate = frmMain.pastel.convertDate(customerBits[66]).ToString("yyyy/MM/dd"); } catch { }
                try { lastCrAmount = double.Parse(customerBits[67]); } catch { }
                try { blocked = customerBits[68]; } catch { }
                try { openItem = customerBits[69]; } catch { }
                try { overRideTax = customerBits[70]; } catch { }
                try { monthOrDay = customerBits[71]; } catch { }
                try { incExc = customerBits[72]; } catch { }
                try { country = customerBits[73]; } catch { }
                try { currencyCode = int.Parse(customerBits[74]); } catch { }
                try { creditLimit = long.Parse(customerBits[75]); } catch { }
                try { interestAfter = int.Parse(customerBits[76]); } catch { }
                try { priceRegime = int.Parse(customerBits[77]); } catch { }
                try { useAgedMessages = int.Parse(customerBits[78]); } catch { }

                try { setCurrBalance(double.Parse(customerBits[79]), 0); } catch { }
                try { setCurrBalance(double.Parse(customerBits[80]), 1); } catch { }
                try { setCurrBalance(double.Parse(customerBits[81]), 2); } catch { }
                try { setCurrBalance(double.Parse(customerBits[82]), 3); } catch { }
                try { setCurrBalance(double.Parse(customerBits[83]), 4); } catch { }
                try { setCurrBalance(double.Parse(customerBits[84]), 5); } catch { }
                try { setCurrBalance(double.Parse(customerBits[85]), 6); } catch { }
                try { setCurrBalance(double.Parse(customerBits[86]), 7); } catch { }
                try { setCurrBalance(double.Parse(customerBits[87]), 8); } catch { }
                try { setCurrBalance(double.Parse(customerBits[88]), 9); } catch { }
                try { setCurrBalance(double.Parse(customerBits[89]), 10); } catch { }
                try { setCurrBalance(double.Parse(customerBits[90]), 11); } catch { }
                try { setCurrBalance(double.Parse(customerBits[91]), 12); } catch { }

                try { setCurrLastBalance(double.Parse(customerBits[92]), 0); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[93]), 1); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[94]), 2); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[95]), 3); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[96]), 4); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[97]), 5); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[98]), 6); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[99]), 7); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[100]), 8); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[101]), 9); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[102]), 10); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[103]), 11); } catch { }
                try { setCurrLastBalance(double.Parse(customerBits[104]), 12); } catch { }

                try { setUserDefined(customerBits[105], 0); } catch { }
                try { setUserDefined(customerBits[106], 1); } catch { }
                try { setUserDefined(customerBits[107], 2); } catch { }
                try { setUserDefined(customerBits[108], 3); } catch { }
                try { setUserDefined(customerBits[109], 4); } catch { }

                try { setAgeing(double.Parse(customerBits[110]), 0); } catch { }
                try { setAgeing(double.Parse(customerBits[111]), 1); } catch { }
                try { setAgeing(double.Parse(customerBits[112]), 2); } catch { }
                try { setAgeing(double.Parse(customerBits[113]), 3); } catch { }
                try { setAgeing(double.Parse(customerBits[114]), 4); } catch { }

                try { statPrintorEmail = int.Parse(customerBits[115]); } catch { }
                try { docPrintorEmail = int.Parse(customerBits[116]); } catch { }
                try { interestPer = customerBits[117]; } catch { }
                try { freight = customerBits[118]; } catch { }
                try { ship = customerBits[119]; } catch { }
                try { password = customerBits[120]; } catch { }
                try { linkWeb = customerBits[121]; } catch { }
                try { loyaltyProg = customerBits[122]; } catch { }
                try { lcardnumber = customerBits[123]; } catch { }
                try { updatedOn = customerBits[124]; } catch { }
                try { cashAccount = customerBits[125]; } catch { }
                try { acceptEmail = customerBits[126]; } catch { }
                try { createDate = frmMain.pastel.convertDate(customerBits[127]).ToString("yyyy/MM/dd"); } catch { }
            } catch (Exception ex) {
                MessageBox.Show("Customer Error: " + ex.StackTrace + " - " + custBits.ToString());
            }
        }

        public void SetDeliveryInfo(String[] delBits) {
            try {
                try { delCode = delBits[2]; } catch { delCode = ""; }
                try { Salesanalysis = delBits[3]; } catch { Salesanalysis = ""; }
                try { Contact = delBits[4]; } catch { Contact = ""; }
                try { Telephone = delBits[5]; } catch { Telephone = ""; }
                try { CellPhone = delBits[6]; } catch { CellPhone = ""; }
                try { Fax = delBits[7]; } catch { Fax = ""; }
                try { SetDelAddress(delBits[8], 0); } catch { SetDelAddress("", 0); }
                try { SetDelAddress(delBits[9], 1); } catch { SetDelAddress("", 1); }
                try { SetDelAddress(delBits[10], 2); } catch { SetDelAddress("", 2); }
                try { SetDelAddress(delBits[11], 3); } catch { SetDelAddress("", 3); }
                try { SetDelAddress(delBits[12], 4); } catch { SetDelAddress("", 4); }
                try { Email = delBits[13].Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries); } catch { Email = new String[1]; }
            } catch (Exception ex) {
                MessageBox.Show("Delivery Error: " + ex.StackTrace + " - " + delBits.Length.ToString());
            }
        }

        public String GetCustomer() {
            StringBuilder sb = new StringBuilder();
            sb.Append(accNumber);
            sb.Append("|");
            sb.Append(description);
            sb.Append("|");
            foreach (String addressLine in address) {
                sb.Append(addressLine);
                sb.Append("|");
            }

            sb.Append(Telephone);
            sb.Append("|");
            sb.Append(Fax);
            sb.Append("|");
            sb.Append(Contact);

            sb.Append("|N|");
            sb.Append(settlementTerms);
            sb.Append("|");
            sb.Append(priceRegime);
            sb.Append("|");
            sb.Append(Salesanalysis);
            sb.Append("|");
            foreach (String addressLine in delAddress) {
                sb.Append(addressLine);
                sb.Append("|");
            }
            sb.Append(blocked);
            sb.Append("|");
            sb.Append(discount);
            sb.Append("|N|");
            sb.Append(statPrintorEmail);
            sb.Append("|");
            sb.Append(openItem);
            sb.Append("|");
            sb.Append(category);
            sb.Append("|");
            sb.Append(currencyCode);
            sb.Append("|");
            sb.Append(paymentTerms);
            sb.Append("|");
            sb.Append(creditLimit);
            sb.Append("|");
            foreach (string lineItem in userDefined) {
                sb.Append(lineItem);
                sb.Append("|");
            }
            sb.Append(monthOrDay);
            sb.Append("|");
            sb.Append(statPrintorEmail);
            sb.Append("|");

            sb.Append(docPrintorEmail);
            sb.Append("|");

            sb.Append(CellPhone);
            sb.Append("|");

            sb.Append(Email);
            sb.Append("|");
            sb.Append(freight);
            sb.Append("|");
            sb.Append(ship);
            sb.Append("|");
            sb.Append(exemptRef);
            sb.Append("|");

            sb.Append(cashAccount);
            sb.Append("|");
            return sb.ToString();
        }

        #region Variables

        private String _category;
        private String _accNumber;
        private String _description;
        private double[] _balance = new double[13];
        private double[] _lastBal = new double[13];

        private double[] _salesBal = new double[13];
        private double[] _lastSalesBal = new double[13];
        private String[] _address = new String[5];
        private int _taxCode;
        private String _exemptRef;
        private int _settlementTerms;
        private int _paymentTerms;

        //0 =  Current
        //30 = 30 Days
        //60 = 60 Days
        //90 = 90 Days
        //120 = 120 Days
        private int _discount;

        //Default discount % multiply by 100
        private String _lastCrDate;

        //Last date customer  paid
        private double _lastCrAmount;

        private String _blocked;

        // 0 = Account not blocked
        //1 = Account blocked
        private String _openItem;

        //0 = Balance forward
        //1 = Balance forward
        private String _overRideTax;

        //0 = No tax code override
        //1 = Override tax code
        //2 = Force tax
        private String _monthOrDay;

        // 0 = Monthly terms
        //1 = Day base terms
        private String _incExc;

        // 0 = Default inclusive entry
        //1 = Default exclusive entry
        private String _country;

        private int _currencyCode;

        // 0 = Home currency
        //0 > Foreign currency
        private long _creditLimit;

        private int _interestAfter;

        //1 = No interest
        //0 = One period after terms
        //30 = interest after 30 days
        //60 = interest after 60 days
        //90 = interest after 90 days
        //120 = interest after 120 days
        private int _priceRegime;

        private int _useAgedMessages;

        //ASCII(0) = Print with aged msg
        //ASCII(1) = Always print current msg
        //ASCII(2) = Don//t print statement
        private double[] _currBalance = new double[13];

        private double[] _currLastBal = new double[13];
        private String[] _userDefined = new String[5];
        private double[] _ageing = new double[5];
        private int _statPrintorEmail;

        //0 = Print Statement
        //1 = Email
        //2 = Print & Email Statement
        //3 = Don//t Print & Email Statement
        private int _docPrintorEmail;

        //0 = Print Document
        //1 = Email Document
        //2 = Print & Email Document
        private String _interestPer;

        //NNNNNNNNNNNNN
        //Position in string = Period
        //N = Not calculated
        //Y = interest calculated
        private String _freight;

        private String _ship;
        private String _password;
        private String _linkWeb;
        private String _loyaltyProg;
        private String _lcardnumber;
        private String _updatedOn;

        //Date & time changed
        private String _cashAccount;

        private String _acceptEmail;
        private String _createDate;

        //Date the Customer was created
        private String _delCode, _Salesanalysis, _Contact, _Telephone, _CellPhone, _Fax;

        private String[] _Email;
        private String[] delAddress = new String[5];

        #endregion Variables

        #region Properties

        public String category {
            get {
                return _category;
            }
            set {
                _category = value;
            }
        }

        public String accNumber {
            get {
                return _accNumber;
            }
            set {
                _accNumber = value;
            }
        }

        public String description {
            get {
                return _description;
            }
            set {
                _description = value;
            }
        }

        public double[] balance {
            get {
                return _balance;
            }
        }

        public void setBalance(double balance, int arrayIdx) {
            _balance[arrayIdx] = balance;
        }

        public double[] lastBal {
            get {
                return _lastBal;
            }
        }

        public void setLastBalance(double balance, int arrayIdx) {
            _lastBal[arrayIdx] = balance;
        }

        public double[] salesBal {
            get {
                return _salesBal;
            }
        }

        public void setSalesBalance(double balance, int arrayIdx) {
            _salesBal[arrayIdx] = balance;
        }

        public double[] lastSalesBal {
            get {
                return _lastSalesBal;
            }
        }

        public void setLastSalesBalance(double balance, int arrayIdx) {
            _lastSalesBal[arrayIdx] = balance;
        }

        public String[] address {
            get {
                return _address;
            }
            set {
                _address = value;
            }
        }

        public void setAddress(String address, int idx) {
            _address[idx] = address;
        }

        public int taxCode {
            get {
                return _taxCode;
            }
            set {
                _taxCode = value;
            }
        }

        public String exemptRef {
            get {
                return _exemptRef;
            }
            set {
                _exemptRef = value;
            }
        }

        public int settlementTerms {
            get {
                return _settlementTerms;
            }
            set {
                _settlementTerms = value;
            }
        }

        public int paymentTerms {
            get {
                return _paymentTerms;
            }
            set {
                _paymentTerms = value;
            }
        }

        public int discount {
            get {
                return _discount;
            }
            set {
                _discount = value;
            }
        }

        public String lastCrDate {
            get {
                return _lastCrDate;
            }
            set {
                _lastCrDate = value;
            }
        }

        public double lastCrAmount {
            get {
                return _lastCrAmount;
            }
            set {
                _lastCrAmount = value;
            }
        }

        public String blocked {
            get {
                return _blocked;
            }
            set {
                _blocked = value;
            }
        }

        public String openItem {
            get {
                return _openItem;
            }
            set {
                _openItem = value;
            }
        }

        public String overRideTax {
            get {
                return _overRideTax;
            }
            set {
                _overRideTax = value;
            }
        }

        public String monthOrDay {
            get {
                return _monthOrDay;
            }
            set {
                _monthOrDay = value;
            }
        }

        public String incExc {
            get {
                return _incExc;
            }
            set {
                _incExc = value;
            }
        }

        public String country {
            get {
                return _country;
            }
            set {
                _country = value;
            }
        }

        public int currencyCode {
            get {
                return _currencyCode;
            }
            set {
                _currencyCode = value;
            }
        }

        public long creditLimit {
            get {
                return _creditLimit;
            }
            set {
                _creditLimit = value;
            }
        }

        public int interestAfter {
            get {
                return _interestAfter;
            }
            set {
                _interestAfter = value;
            }
        }

        public int priceRegime {
            get {
                return _priceRegime;
            }
            set {
                _priceRegime = value;
            }
        }

        public int useAgedMessages {
            get {
                return _useAgedMessages;
            }
            set {
                _useAgedMessages = value;
            }
        }

        public double[] currBalance {
            get {
                return _currBalance;
            }
        }

        public void setCurrBalance(double balance, int idx) {
            _currBalance[idx] = balance;
        }

        public double[] currLastBal {
            get {
                return _currLastBal;
            }
        }

        public void setCurrLastBalance(double balance, int idx) {
            _currLastBal[idx] = balance;
        }

        public String[] userDefined {
            get {
                return _userDefined;
            }
        }

        public void setUserDefined(String usrDefined, int idx) {
            _userDefined[idx] = usrDefined;
        }

        public double[] ageing {
            get {
                return _ageing;
            }
        }

        public void setAgeing(double aging, int idx) {
            _ageing[idx] = aging;
        }

        public int statPrintorEmail {
            get {
                return _statPrintorEmail;
            }
            set {
                _statPrintorEmail = value;
            }
        }

        public int docPrintorEmail {
            get {
                return _docPrintorEmail;
            }
            set {
                _docPrintorEmail = value;
            }
        }

        public String interestPer {
            get {
                return _interestPer;
            }

            set {
                _interestPer = value;
            }
        }

        public String freight {
            get {
                return _freight;
            }
            set {
                _freight = value;
            }
        }

        public String ship {
            get {
                return _ship;
            }
            set {
                _ship = value;
            }
        }

        public String password {
            get {
                return _password;
            }
            set {
                _password = value;
            }
        }

        public String linkWeb {
            get {
                return _linkWeb;
            }
            set {
                _linkWeb = value;
            }
        }

        public String loyaltyProg {
            get {
                return _loyaltyProg;
            }

            set {
                _loyaltyProg = value;
            }
        }

        public String lcardnumber {
            get {
                return _lcardnumber;
            }

            set {
                _lcardnumber = value;
            }
        }

        public String updatedOn {
            get {
                return _updatedOn;
            }
            set {
                _updatedOn = value;
            }
        }

        public String cashAccount {
            get {
                return _cashAccount;
            }
            set {
                _cashAccount = value;
            }
        }

        public String acceptEmail {
            get {
                return _acceptEmail;
            }
            set {
                _acceptEmail = value;
            }
        }

        public String createDate {
            get {
                return _createDate;
            }
            set {
                _createDate = value;
            }
        }

        public String delCode {
            get {
                return _delCode;
            }
            set {
                _delCode = value;
            }
        }

        public String Salesanalysis {
            get {
                return _Salesanalysis;
            }
            set {
                _Salesanalysis = value;
            }
        }

        public String Contact {
            get {
                return _Contact;
            }
            set {
                _Contact = value;
            }
        }

        public String Telephone {
            get {
                return _Telephone;
            }
            set {
                _Telephone = value;
            }
        }

        public String CellPhone {
            get {
                return _CellPhone;
            }
            set {
                _CellPhone = value;
            }
        }

        public String Fax {
            get {
                return _Fax;
            }
            set {
                _Fax = value;
            }
        }

        public String[] Email {
            get {
                return _Email;
            }
            set {
                _Email = value;
            }
        }

        public void SetDelAddress(String dAddy, int idx) {
            delAddress[idx] = dAddy;
        }

        public String[] getDelAddress() {
            return delAddress;
        }

        #endregion Properties

        public struct Note {

            //ACCNOTE
            public int nType;

            public String accCode;
            public String status;
            public String subject;
            public DateTime entryDate;
            public DateTime actionDate;
            public String linked;
            public long noteID;
            public String content;
        }

        private List<Note> _notes = new List<Note>();

        public List<Note> notes {
            get { return _notes; }
        }

        public void SetNotes(List<String> note) {
            String[] splitter = new string[] { "|" };
            foreach (String nLine in note) {
                String[] nBit = nLine.Split(splitter, StringSplitOptions.None);
                if (nBit.Length >= 9) {
                    Note myNote = new Note();
                    myNote.nType = int.Parse(nBit[0]);
                    myNote.accCode = nBit[1];
                    myNote.status = nBit[2];
                    myNote.subject = nBit[3];
                    myNote.entryDate = frmMain.pastel.convertDate(nBit[4]);
                    myNote.actionDate = frmMain.pastel.convertDate(nBit[5]);
                    myNote.linked = nBit[6];
                    myNote.noteID = long.Parse(nBit[7]);
                    myNote.content = nBit[8];
                    _notes.Add(myNote);
                }
            }
        }
    }

    public class CustomerList {
        private String accNumber;
        private String accName;
        private double osBal;
        private double refund;
        private bool reminder;
        private bool final;
        private bool disconnectNotice;
        private bool summons;
        private bool disconnect;
        private bool handover;
        private bool clearance;
        private bool exClearance;
        private String rDesc;

        public String AccNumber { get { return accNumber; } set { accNumber = value; } }

        public String AccName { get { return accName; } set { accName = value; } }

        public bool Reminder {
            get { return reminder; }
            set {
                reminder = value;
                if (reminder) {
                    reminder = true;
                    final = false;
                    disconnectNotice = false;
                    summons = false;
                    disconnect = false;
                    handover = false;
                    clearance = false;
                    exClearance = false;
                }
            }
        }

        public bool Final {
            get { return final; }
            set {
                final = value;
                if (final) {
                    reminder = false;
                    final = true;
                    disconnectNotice = false;
                    summons = false;
                    disconnect = false;
                    handover = false;
                    clearance = false;
                    exClearance = false;
                }
            }
        }

        public bool DisconnectNotice {
            get { return disconnectNotice; }
            set {
                disconnectNotice = value;
                if (disconnectNotice) {
                    reminder = false;
                    final = false;
                    disconnectNotice = true;
                    summons = false;
                    disconnect = false;
                    handover = false;
                    clearance = false;
                    exClearance = false;
                }
            }
        }

        public bool Summons {
            get { return summons; }
            set {
                summons = value;
                if (summons) {
                    reminder = false;
                    final = false;
                    disconnectNotice = false;
                    summons = true;
                    disconnect = false;
                    handover = false;
                    clearance = false;
                    exClearance = false;
                }
            }
        }

        public bool Disconnect {
            get { return disconnect; }
            set {
                disconnect = value;
                if (disconnect) {
                    reminder = false;
                    final = false;
                    disconnectNotice = false;
                    summons = false;
                    disconnect = true;
                    handover = false;
                    clearance = false;
                    exClearance = false;
                }
            }
        }

        public bool Handover {
            get { return handover; }
            set {
                handover = value;
                if (handover) {
                    reminder = false;
                    final = false;
                    disconnectNotice = false;
                    summons = false;
                    disconnect = false;
                    handover = true;
                    clearance = false;
                    exClearance = false;
                }
            }
        }

        public bool Clearance {
            get { return clearance; }
            set {
                clearance = value;
                if (clearance) {
                    reminder = false;
                    final = false;
                    disconnectNotice = false;
                    summons = false;
                    disconnect = false;
                    handover = false;
                    clearance = true;
                    exClearance = false;
                }
            }
        }

        public bool ExClearance {
            get { return exClearance; }
            set {
                exClearance = value;
                if (exClearance) {
                    reminder = false;
                    final = false;
                    disconnectNotice = false;
                    summons = false;
                    disconnect = false;
                    handover = false;
                    clearance = false;
                    exClearance = true;
                }
            }
        }

        public double OSBal {
            get { return osBal; }
            set { osBal = value; }
        }

        public double Credit {
            get { return refund; }
            set { refund = value; }
        }

        public String Credit_Note {
            get { return rDesc; }
            set { rDesc = value; }
        }

        public bool SMS { get; set; }

        public CustomerList(String acc, String name, double bal) {
            AccNumber = acc;
            AccName = name;
            reminder = false;
            final = false;
            disconnectNotice = false;
            summons = false;
            disconnect = false;
            handover = false;
            clearance = false;
            exClearance = false;
            OSBal = bal;
            Credit = 0;
            Credit_Note = "";
        }
    }

    public class CustomerRefundList {
        private String accNumber;
        private String accName;
        private double osBal;
        private double refund;

        private String refAcc;
        private String rDesc;

        public String AccNumber { get { return accNumber; } set { accNumber = value; } }

        public String AccName { get { return accName; } set { accName = value; } }

        public double OSBal {
            get { return osBal; }
            set { osBal = value; }
        }

        public double Credit {
            get { return refund; }
            set { refund = value; }
        }

        public String Credit_Note {
            get { return rDesc; }
            set { rDesc = value; }
        }

        public String Refund {
            get { return refAcc; }
            set { refAcc = value; }
        }

        public CustomerRefundList(String acc, String name, double bal, String rAcc) {
            AccNumber = acc;
            AccName = name;
            OSBal = bal;
            Credit = 0;
            Credit_Note = "";
            Refund = rAcc;
        }

        public CustomerRefundList(String acc, String name, double bal, double credit, String note, String rAcc) {
            AccNumber = acc;
            AccName = name;
            OSBal = bal;
            Credit = credit;
            Credit_Note = note;
            Refund = rAcc;
        }
    }

    public class CustomerCreditList {
        private String accNumber;
        private String accName;
        private double osBal;
        private bool reminder = false;
        private bool final = false;
        private bool disconnectNotice = false;
        private bool summons = false;
        private bool disconnect = false;
        private bool handover = false;

        public String AccNumber { get { return accNumber; } set { accNumber = value; } }

        public String AccName { get { return accName; } set { accName = value; } }

        public double OSBal { get { return osBal; } set { osBal = value; } }

        public bool Reminder { get { return reminder; } set { reminder = value; } }

        public bool Final { get { return final; } set { final = value; } }

        public bool DN { get { return disconnectNotice; } set { disconnectNotice = value; } }

        public bool Summons { get { return summons; } set { summons = value; } }

        public bool DR { get { return disconnect; } set { disconnect = value; } }

        public bool HO { get { return handover; } set { handover = value; } }

        public CustomerCreditList(String acc, String name, double bal) {
            AccNumber = acc;
            AccName = name;
            OSBal = bal;
        }
    }

    public class Transactions {
        private DateTime trnDate;
        private String reference;
        private double amt;
        private double accAmt;
        private String description;

        public DateTime TrnDate {
            get { return trnDate; }
            set { trnDate = value; }
        }

        public String Reference {
            get { return reference; }
            set { reference = value; }
        }

        public double Amt {
            get { return amt; }
            set { amt = value; }
        }

        public String Description {
            get { return description; }
            set { description = value; }
        }

        public double AccAmt {
            get { return accAmt; }
            set { accAmt = value; }
        }
    }

    public class CustomerComparer : IComparer<Customer> {
        private string memberName = string.Empty; // specifies the member name to be sorted
        private SortOrder sortOrder = SortOrder.None; // Specifies the SortOrder.

        /// <summary>
        /// constructor to set the sort column and sort order.
        /// </summary>
        /// <param name="strMemberName"></param>
        /// <param name="sortingOrder"></param>
        public CustomerComparer(string strMemberName, SortOrder sortingOrder) {
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
        public int Compare(Customer trn1, Customer trn2) {
            int returnValue = 1;
            switch (memberName) {
                case "AccNo":
                    if (sortOrder == SortOrder.Ascending) {
                        returnValue = trn1.accNumber.CompareTo(trn2.accNumber);
                    } else {
                        returnValue = trn2.accNumber.CompareTo(trn1.accNumber);
                    }

                    break;
            }
            return returnValue;
        }
    }
}
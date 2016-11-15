using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server {

    public class UpdateTrustees {

        public void Update() {
            List<Customer> customers = frmMain.pastel.GetCustomers(true, null);
            MySqlConnector myConn = new MySqlConnector();
            myConn.ToggleConnection(true);
            Parallel.ForEach(customers, c => {
                String usergroup = (c.category == "07" ? "1,2,4" : "1,2");
                foreach (String email in c.Email) {
                    String[] login = myConn.HasLogin(email);
                    if (login != null) { myConn.UpdateGroup(login[0], usergroup); }
                }
            });
            myConn.ToggleConnection(false);
        }
    }
}
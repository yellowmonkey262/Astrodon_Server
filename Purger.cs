//using AstroLibrary.Data;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Server {

    public class Purger {

        #region Variables

        private System.Timers.Timer tmrPurger = new System.Timers.Timer(43200000);
        private String status;

        #endregion Variables

        #region Constructor

        public Purger() {
            tmrPurger.Elapsed += tmrPurger_Elapsed;
            tmrPurger.Enabled = true;
        }

        #endregion Constructor

        #region Class Methods

        private void tmrPurger_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            Purge();
        }

        private void Purge() {
            MySqlConnector mysqlHandler = new MySqlConnector();
            DataSet dsDocs = mysqlHandler.GetPurgeDocuments();
            if (dsDocs != null && dsDocs.Tables.Count > 0 && dsDocs.Tables[0].Rows.Count > 0) {
                Parallel.ForEach(dsDocs.Tables[0].AsEnumerable(), drFile => {
                    TransferFiles(drFile["file"].ToString(), drFile["name"].ToString());
                    mysqlHandler.DeletePurgeDocuments(drFile["uid"].ToString());
                });
            }
        }

        private void TransferFiles(String myFile, String buildingName) {
            String buildPath = "Y:\\Buildings Managed\\" + buildingName;
            if (!Directory.Exists(buildPath)) { try { Directory.CreateDirectory(buildPath); } catch { } }
            String localPath = Path.Combine(buildPath, myFile);
            Sftp client = new Sftp();
            client.Download(localPath, myFile);
            client.DeleteFile(myFile);
        }

        public void TransferFiles() {
            MySqlConnector mysqlHandler = new MySqlConnector();
            DataSet dsDocs = mysqlHandler.GetOffAdminDocs();
            if (dsDocs != null && dsDocs.Tables.Count > 0 && dsDocs.Tables[0].Rows.Count > 0) {
                Sftp client = new Sftp();
                foreach (DataRow drFile in dsDocs.Tables[0].Rows) {
                    String myFile = drFile["file"].ToString();
                    client.DeleteFile(myFile);
                }
            }
        }

        #endregion Class Methods
    }
}
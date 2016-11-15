using System;
using System.IO;

namespace Server {

    public class DummyUpload {

        public void UploadStatements() {
            Sftp ftpClient = new Sftp();
            if (ftpClient.ConnectClient()) {
                foreach (String fi in Directory.GetFiles("C:\\Projects\\Astrodon\\Statements")) {
                    String source = fi;
                    String target = Path.GetFileName(source);
                    ftpClient.Upload(source, target, false);
                }
            }
            //}
        }
    }
}
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Linq;

namespace Server
{
    public class Sftp
    {
        private const int port = 22;
        private const string host = "www.astrodon.co.za";
        private const string username = "root";
        private const string password = "root@66r94e!@#";
        private const string workingdirectory = "/srv/www/htdocs/uploads/tx_astro";

        private SftpClient client;

        public Sftp()
        {
            //ConnectClient();
        }

        public event EventHandler<SqlArgs> MessageHandler;

        public bool ConnectClient()
        {
            PasswordAuthenticationMethod PasswordConnection = new PasswordAuthenticationMethod("root", "root@66r94e!@#");
            KeyboardInteractiveAuthenticationMethod KeyboardInteractive = new KeyboardInteractiveAuthenticationMethod("root");
            ConnectionInfo connectionInfo = new ConnectionInfo("www.astrodon.co.za", port, "root", PasswordConnection, KeyboardInteractive);
            KeyboardInteractive.AuthenticationPrompt += delegate (object sender, AuthenticationPromptEventArgs e)
            {
                foreach (var prompt in e.Prompts)
                {
                    if (prompt.Request.Equals("Password: ", StringComparison.InvariantCultureIgnoreCase))
                    {
                        prompt.Response = password;
                    }
                }
            };
            client = new SftpClient(connectionInfo);
            try
            {
                client.Connect();
                Console.WriteLine("Connected to {0}", host);
                client.ChangeDirectory("..");
                client.ChangeDirectory(workingdirectory);
                return client.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        public bool DisconnectClient()
        {
            try
            {
                client.Disconnect();
                return !client.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        public bool Upload(String fileName, String remoteFile, bool report)
        {
            if (!client.IsConnected)
            {
                ConnectClient();
            }
            bool success = false;
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open))
                {
                    Console.WriteLine("Uploading {0} ({1:N0} bytes)", fileName, fileStream.Length);
                    client.BufferSize = 4 * 1024; // bypass Payload error large files
                    client.UploadFile(fileStream, remoteFile);
                    if (report)
                    {
                        var listDirectory = client.ListDirectory(workingdirectory);

                        foreach (var fi in listDirectory)
                        {
                            if (fi.Name == remoteFile)
                            {
                                success = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
            return success;
        }

        public bool Download(String fileName, String remoteFile)
        {
            if (client == null || !client.IsConnected)
            {
                ConnectClient();
            }
            bool success = false;
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    Console.WriteLine("Uploading {0} ({1:N0} bytes)", fileName, fileStream.Length);
                    client.BufferSize = 4 * 1024; // bypass Payload error large files
                    client.DownloadFile(remoteFile, fileStream);
                }
                if (File.Exists(fileName))
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
            }
            return success;
        }

        public bool DeleteFile(String fileName)
        {
            if (client == null || !client.IsConnected)
            {
                ConnectClient();
            }
            bool success = false;
            try
            {
                client.DeleteFile(fileName);
                success = true;
            }
            catch (Exception ex)
            {
            }
            return success;
        }

        public void ClearFiles()
        {
            if (MessageHandler != null) { MessageHandler(this, new SqlArgs("Starting file deletion")); }
            if (client == null || !client.IsConnected)
            {
                ConnectClient();
                if (MessageHandler != null) { MessageHandler(this, new SqlArgs("Connecting client")); }
            }
            bool success = false;
            try
            {
                MySqlConnector mysql = new MySqlConnector();
                var files = client.ListDirectory(client.WorkingDirectory);
                if (MessageHandler != null) { MessageHandler(this, new SqlArgs("Testing " + files.ToList().Count.ToString() + " files")); }
                foreach (var file in files)
                {
                    if (!mysql.FindFile(file.Name))
                    {
                        if (MessageHandler != null) { MessageHandler(this, new SqlArgs(file.Name)); }
                        DeleteFile(file.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                if (MessageHandler != null) { MessageHandler(this, new SqlArgs(ex.Message)); }
            }
        }
    }
}
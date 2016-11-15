using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Server {

    internal static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            try {
                CloseOtherInstances();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
            } catch { }
        }

        private static void CloseOtherInstances() {
            Process current = Process.GetCurrentProcess();
            int prcID = current.Id;
            Process[] otherInstances = Process.GetProcessesByName(current.ProcessName);
            foreach (Process oi in otherInstances) {
                if (oi.Id != prcID) { oi.Kill(); }
            }
        }
    }
}
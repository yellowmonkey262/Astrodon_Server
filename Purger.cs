//using AstroLibrary.Data;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Server
{
    public class Purger
    {
        #region Variables

        private System.Timers.Timer tmrPurger = new System.Timers.Timer(43200000);
        private String status;

        #endregion Variables

        #region Constructor

        public Purger()
        {
            tmrPurger.Elapsed += tmrPurger_Elapsed;
            tmrPurger.Enabled = true;
        }

        #endregion Constructor

        #region Class Methods

        private void tmrPurger_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
        }

   

        #endregion Class Methods
    }
}
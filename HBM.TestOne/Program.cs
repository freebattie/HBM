using Hbm.Api.Common;
using Hbm.Api.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestOne
{
    static class Program
    {

        // 

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DaqEnvironment env = DaqEnvironment.GetInstance();
            // Scan for all available devices
            List<Device> foundDevices = env.Scan();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

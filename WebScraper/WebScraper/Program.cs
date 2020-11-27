/**
 * 
  Author:    Daniel Pak 
  Date:      Nov 13,  2020
  Course:    CS 4540, University of Utah, School of Computing
  Copyright: CS 4540, and Daniel Pak - This work may not be copied for use in Academic Coursework.

  I, Daniel Pak, certify that I wrote this code from scratch and did not copy it in part or whole from
  another source.  Any references used in the completion of the assignment are cited in my README file.

  File Contents

     Program.cs

 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebScraper
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //set the window always at the top
            Form1 app = new Form1();
            app.TopMost = true;
            Application.Run(app);
        }
    }
}

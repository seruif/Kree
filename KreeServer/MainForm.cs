using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KryserLib;
using System.Net.Sockets;
using System.Net;

namespace Kryser
{
    public partial class MainForm : Form
    {
        LogForm LoggingForm = new LogForm();
        public MainForm()
        {
            InitializeComponent();

            //LXSConnectionSession.ZZZ();
            //XSFileManager.ZZZ();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoggingForm.Show();
            try
            {
                XSFileInfoManager.Load();

                XSServer.InitSocket();
                LXSLog.Log("Server started");
            }
            catch (Exception exc) { LXSLog.Log(exc); }
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using KreeLib;
using System.IO;
using System.Diagnostics;

namespace KreeClient
{
    public partial class KCF_MainForm : Form
    {
        KCF_MessageForm MsgForm = new KCF_MessageForm();
        public static KCF_MainForm THIS;
        public KCF_MainForm()
        {
            InitializeComponent();
        }
        public static DialogResult ShowMessage(string text, string cap, MessageBoxIcon ic)
        {
            return MessageBox.Show(text, cap, MessageBoxButtons.OK, ic);
        }
        public static DialogResult ShowError(string text)
        {
            return ShowMessage(text, "Error", MessageBoxIcon.Error);
        }
        public static DialogResult ShowWarning(string text)
        {
            return ShowMessage(text, "Warning", MessageBoxIcon.Warning);
        }
        public static DialogResult ShowInfo(string text)
        {
            return ShowMessage(text, "Message", MessageBoxIcon.Information);
        }

        string window_name;
        public const string tmp_dir = "tmp\\";
        private void Form1_Load(object sender, EventArgs e)
        {
            if (Directory.Exists(tmp_dir)) Directory.Delete(tmp_dir);

            THIS = this;
            window_name = Text;

            KCReactionSystem.Init();

            KCClient.BeginConnectingEvent += connecting_eh;
            KCClient.EndConnectingEvent += end_connecting_eh;
            KCClient.ConnectionLostEvent += connection_lost_eh;
            KCClient.Init();

            //backgroundWorkerPing.RunWorkerAsync();

            //backgroundWorkerPingThreadStart = new ThreadStart(backgroundWorkerPing_DoWork);
            //backgroundWorkerPingThread = new Thread(backgroundWorkerPingThreadStart);
            //backgroundWorkerPingThread.IsBackground = true;
            //backgroundWorkerPingThread.Start();

            listView1.SmallImageList = imageList;
            listView2.SmallImageList = imageList;
            listView1.LargeImageList = imageList;
            listView2.LargeImageList = imageList;

            //var d = new LKSProfileDirectory("root", null);
            //d.AddPath("dir1/dir2");
            //d.AddFile(new LKSProfileFileInfo() { NameExt = "file1.doc", Size = 1024, EditedFileTime = DateTime.Now.ToBinary() });
            //d.AddFile(new LKSProfileFileInfo() { ID = 1, NameExt = "k,gakgsadkgbak.doc", Size = 1038576, EditedFileTime = DateTime.Now.ToBinary() });
            //d.AddFilePath(new LKSProfileFileInfo() { Path = "dir1", NameExt = "file2.xml", Size = 0 });
            //ShowProfileDirectoryView(d);
            //KCF_LoaderForm_1.GetLocalFilesDirecory("F:\\X Rebirth");
        }

        //
        bool fshowmf = false;
        void connecting_eh(KCEventArgs e)
        {
            if (!fshowmf) { MsgForm.Show(); fshowmf = true; } else MsgForm.SHOW();
            MsgForm.SET(e.error, e.message);

            Invoke(new Action(() => connectToolStripMenuItem.Enabled = false));
        }

        void end_connecting_eh(KCEventArgs e)
        {
            MsgForm.HIDE();
            if (e.error != null)
            {
                Invoke(new Action(() => ShowError(e.error.Message)));
                Invoke(new Action(() => connectToolStripMenuItem.Enabled = true));
            }
            else
            {
                Invoke(new Action(() => toolStripStatusLabelConnected.Text = "Connected"));
                //AuthReaction = primary_auth_rea;
                Invoke(new Action(() => KCF_MainForm.THIS.toolStripStatusLabelAction.Text = "TFK Receiving"));
                Invoke(new Action(Auth));
            }
        }

        void connection_lost_eh(KCEventArgs e)
        {
            Authed = false;
            KCClient.TFK_Recieved = false;
            Invoke(new Action(() => toolStripStatusLabelConnected.Text = "Disconnected"));
        }

        void primary_auth_rea()
        {
            listView1.Enabled = Authed;
            pasteToolStripMenuItem.Enabled = Authed;
            if (Authed) Text = window_name + " - " + Login;
            else Text = window_name;
        }

        //Thread backgroundWorkerPingThread;
        //ThreadStart backgroundWorkerPingThreadStart;
        //private void backgroundWorkerPing_DoWork()//(object sender, DoWorkEventArgs e)
        //{
        //    int p2 = LKSConsts.PingTimeOut / 2;
        //    var com = new LKSTranferCommand(LKSTransferCommands.Ping);
        //    while (true)
        //    {
        //        try
        //        {
        //            if (KCClient.ClientSocket.Connected)
        //            {
        //                Thread.Sleep(LKSConsts.PingTimeOut);
        //                if (!KCClient.Pinged) { KCClient.socket_error_success(true); continue; }
        //                com.Send(0, KCClient.ClientSocket, KCClient.TransferKey);
        //                KCClient.Pinged = false;
        //            }
        //            else Thread.Sleep(p2);
        //        }
        //        catch { }
        //    }
        //}

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cf = new KCF_ConnectForm();
            if (cf.ShowDialog() != DialogResult.OK) return;
            try
            {
                if (cf.textBoxIPP.Text.IndexOf(':') < 0) throw new Exception("Invalid IPP");
                string[] ipp = cf.textBoxIPP.Text.Split(':');
                KCClient.Connect(ipp[0], int.Parse(ipp[1]));
            }
            catch (Exception exc) { ShowError(exc.Message); }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void KCF_MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Directory.Exists(tmp_dir)) Directory.Delete(tmp_dir);
        }

        //    private void toolStripMenuItem1_Click(object sender, EventArgs e)
        //    {
        //        var f = new KCF_LoaderForm(new LKSProfileFileInfo[] { new LKSProfileFileInfo(@"F:\X Rebirth\08.dat"),
        //        new LKSProfileFileInfo(@"F:\X Rebirth\05.dat")}, null,
        //"root\\", KCF_LoaderForm.TFMODE.ToServer);
        //        f.FileUpLoadedEvent += FileUpLoaded;
        //        f.Show();
        //    }


    }
}

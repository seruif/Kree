using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KreeLib;
using System.Threading;
using System.IO;

namespace KreeClient
{
    public partial class KCF_LoaderForm_2 : Form
    {
        public enum TFMODE { ToServer, ToLocal, Remove, CopyMove };
        List<LKSProfileFileInfo> FilesInfo = new List<LKSProfileFileInfo>();
        string dest_path;
        string source_path;
        TFMODE tfmode;

        //List<LKSProfileFileInfo> FilesInfoLoaded = new List<LKSProfileFileInfo>();

        //public delegate void FileLoadedHadler(LKSProfileFileInfo[] fi, bool add);
        //public event FileLoadedHadler FileUpLoadedEvent;

        bool _paused = false;

        //public bool canceled = false;
        bool paused { get { return _paused || !KCF_MainForm.Authed; } set { _paused = value; } }

        long req_id;

        #region Form prepare/Form Events
        public KCF_LoaderForm_2()
        {
            InitializeComponent();
            req_id = KCClient.NextReqID;

            KCClient.ConnectionLostEvent += disconnected_e;
            KCClient.EndConnectingEvent += connected_e;

            //ckey = LKSTransferMethods.HASH_MD5(Encoding.UTF8.GetBytes(
            //    KCF_MainForm.HashPass));
        }

        //LOADER CONSTRUCTOR
        public KCF_LoaderForm_2(LKSProfileFileInfo[] files, string _source_path, string _dest_path, TFMODE _tfmode) :
            this()
        {

            rebuild_file_collection();

            FilesInfo.AddRange(files);
            dest_path = _dest_path;
            if (tfmode == TFMODE.ToServer)
            {
                Text = "Uploading";
                if (_source_path == null) source_path = "";
                else
                    if (_source_path.LastIndexOf('\\') < 0) source_path = "";
                    else source_path = _source_path.Substring(0, _source_path.LastIndexOf('\\') + 1);
            }
            else
            {
                source_path = _source_path;
                Text = "Downloading";
            }
            tfmode = _tfmode;

            labelTO.Text = _dest_path
                + (source_path != "" ? _source_path.Substring(_source_path.LastIndexOf('\\') + 1) : "");

            progressBarglobal.Maximum = files.Length;
        }

        //REMOVING CONSTRUCTOR
        public KCF_LoaderForm_2(ListView listView1, LKSProfileDirectory CurrentDirView)
            : this()
        {
            Text = "Removing";

            foreach (ListViewItem item in listView1.SelectedItems)
                if (item.Index != 0)
                    if (item.Index - 1 < CurrentDirView.Directories.Count)
                        queue_dirs.Enqueue(((LKSProfileDirectory)item.Tag).Path);
                    else
                        queue_files.Enqueue((LKSProfileFileInfo)item.Tag);

            progressBarglobal.Maximum = queue_files.Count;
            parts = queue_dirs.Count;

            tfmode = TFMODE.Remove;
        }

        //Copy/Move CONSTRUCTOR
        public KCF_LoaderForm_2(LKSProfileFileInfo[] files, string _source_path, string _dest_path, bool _cm_remove)
            : this()
        {
            rebuild_file_collection();

            Text = _cm_remove ? "Moving" : "Coping";
            cm_remove = _cm_remove;

            source_path = _source_path;
            dest_path = _dest_path;

            progressBarglobal.Maximum = files.Length;

            FilesInfo.AddRange(files);
            labelTO.Text = dest_path;

            tfmode = TFMODE.CopyMove;
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            paused = !paused;
            buttonPause.Text = paused ? "Resume" : "Pause";
        }
        #endregion

        void cancel()
        {
            KCReactionSystem.RemoveReaction(req_id);
            KCClient.ConnectionLostEvent -= disconnected_e;
            KCClient.EndConnectingEvent -= connected_e;
            if (tfmode == TFMODE.ToServer)
                LKSTransferMethods.SendCommand(req_id, KCClient.ClientSocket, LKSTransferCommands.CancelDownload, 
                    KCClient.TransferKey, disconnected_callback);
            if (tfmode == TFMODE.ToLocal)
                downloader.Abort();
            Invoke(new Action(Close));
        }

        #region Loaders
        long size_speed = 0;
        void Loader(bool linear)
        {
            if (!connected) return;
            if (FilesInfo.Count == 0) { cancel(); return; }
            if (tfmode == TFMODE.ToServer)
            {
                KCReactionSystem.AddResReaction(req_id, ToServerLoaderFRea);
                ToServerLoaderFQue(req_id, linear);
            }
            else
            {
                KCReactionSystem.AddResReaction(req_id, ToLocalLoaderFRea);
                ToLocalLoaderFQue(req_id, linear);
            }
        }

        LKSProfileFileInfo current_fi;
        string current_fn;
        private void get_first()
        {
            current_fi = FilesInfo[0];
            FilesInfo.RemoveAt(0);
        }

        private bool error_continue(string error)
        {
            if (MessageBox.Show(error + "\nContinue?", "Load Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
            {
                cancel();
                return false;
            }
            return true;
        }

        void set_progress()
        {
            progressBarparts.Value = (int)part;
        }

        int progress_global = 0;
        void set_progress_global()
        {
            progressBarglobal.Value = progress_global;
            KCF_MainForm.THIS.Invoke(new Action(set_strip_progress));
        }

        void set_file_infs()
        {
            if (tfmode == TFMODE.ToServer || tfmode == TFMODE.Remove)
            {
                labelFROM.Text = current_fn;
                //labelTO.Text = current_fi.FileName;
            }
            else
            {
                //labelTO.Text = current_fn;
                labelFROM.Text = current_fi.FileName;
            }
            progressBarparts.Maximum = parts;
            progressBarparts.Value = 0;
        }

        int parts;
        public int part;
        FileStream file_stream;
        byte[] buffer = new byte[LKSConsts.TransferFileBufferSize];
        int readed;

        bool _connected = true;
        bool connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                if (IsHandleCreated)
                    Invoke(new Action(set_enable_buttons));
            }
        }
        void set_enable_buttons()
        {
            buttonPause.Enabled = _connected;
            buttonCancel.Enabled = _connected;
        }

        void disconnected_e(KCEventArgs ea)
        {
            connected = false;

            KCReactionSystem.RemoveReaction(req_id);
            if (tfmode != TFMODE.Remove)
                FilesInfo.Insert(0, current_fi);
            else
                if (remove_file != null) queue_files.Enqueue(remove_file);
                else if (remove_path != null) queue_dirs.Enqueue(remove_path);
        }

        void disconnected_callback(string error)
        {
            //Exception exc = new Exception(error);
            //disconnected_e(new KCEventArgs(null, exc));
            //KCClient.ReconnectSocketEvent(exc);
            KCClient.CallBackSendingError(error);
        }

        void connected_e(KCEventArgs ea)
        {
            if (ea.error != null) return;
            connected = true;

            if (tfmode == TFMODE.ToServer)
                Loader(false);
            else if (tfmode == TFMODE.ToLocal)
                Loader(downloader.Finished);
            else if (tfmode == TFMODE.Remove)
                RemoveFileQue(req_id);
            else
                CopyMoveFQue(req_id, false);
        }

        #region ToServer
        void ToServerLoaderFQue(long ID, bool linear)
        {
            if (!connected) return;
            while (paused) Thread.Sleep(1000);
            if (FilesInfo.Count == 0) { cancel(); return; }
            Invoke(new Action(get_first));

            if (linear)
            {
                current_fn = current_fi.FileName;
                if (source_path != null)
                    current_fi.Path = source_path != "" ? current_fi.Path.Replace(source_path, dest_path) : dest_path;
                else current_fi.Path = dest_path;
            }

            if (!check_file_replace(current_fi.FileName))
            {
                ToServerLoaderFQue(ID, linear);
                return;
            }

            parts = (int)current_fi.PartCount;
            part = 0;

            Invoke(new Action(set_file_infs));

            send_tos_f_req(ID);
        }

        DialogResult dia_for_all;
        bool for_all = false;
        private bool check_file_replace(string fn)
        {
            if (for_all && dia_for_all == DialogResult.Yes) return true;

            if (KCF_MainForm.THIS.Profile.FileCollection.First(x => x.FileName == fn) != null)
            {
                if (!for_all)
                {
                    var conf = new KCF_ReplaceForm(fn);

                    dia_for_all = conf.ShowDialog();
                    for_all = conf.checkBoxForAll.Checked;

                }
                return DialogResult.Yes == dia_for_all;
            }
            return true;
        }

        private void send_tos_f_req(long ID)
        {
            LKSTransferMethods.SendCommand(ID, KCClient.ClientSocket, LKSTransferCommands.CreateFileDownloader,
                KCClient.TransferKey, disconnected_callback, "fileinfo", current_fi);
        }

        void ToServerLoaderFRea(long ID, LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            //var com = (LKSTranferCommand)LKSTransferContainer.ExtractContainer(container);
            //if (!connected) return;
            if (com.Command == LKSTransferCommands.MsgError)
            {
                string error = (string)com.Args["msg"];
                if (!error_continue(error)) return;
                send_tos_f_req(ID);
                return;
            }
            KCReactionSystem.RemoveReaction(ID);
            KCReactionSystem.AddResReaction(ID, ToServerLoaderRea);

            current_fi.ID = (long)com.Args["ID"];

            if ((bool)com.Args["linked"])
                finish_load_file_to_server(ID);
            else
                ToServerLoaderQue(ID);
        }


        void ToServerLoaderQue(long ID)
        {
            if (!connected) return;
            while (paused) Thread.Sleep(1000);

            file_stream = File.OpenRead(current_fn);
            file_stream.Seek(part * LKSConsts.TransferFileBufferSize, SeekOrigin.Begin);
            readed = file_stream.Read(buffer, 0, LKSConsts.TransferFileBufferSize);
            file_stream.Close();
            buffer = LKSJaffaKreeer.Jaffa_Kree(buffer, KCF_MainForm.Pass);
            var con = new LKSTranferFileDataContainer(buffer, part, readed);

            LKSTransferMethods.SendCommand(ID, KCClient.ClientSocket, LKSTransferCommands.ReceiveFilePart,
                    KCClient.TransferKey, disconnected_callback, "TFDC", con);
        }

        void ToServerLoaderRea(long ID, LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            //var com = (LKSTranferCommand)LKSTransferContainer.ExtractContainer(container);
            //if (!connected) return;
            if (com.Command == LKSTransferCommands.MsgError)
            {
                string error = (string)com.Args["msg"];
                if (error == "checksum error") { ToServerLoaderQue(ID); return; }
                if (!error_continue(error)) return;

                var con = new LKSTranferFileDataContainer(buffer, part, readed);
                LKSTransferMethods.SendCommand(ID, KCClient.ClientSocket, LKSTransferCommands.ReceiveFilePart,
                    KCClient.TransferKey, disconnected_callback, "TFDC", con);
                return;
            }

            lock (this)
                size_speed += readed;

            if (part + 1 >= parts)
            {
                finish_load_file_to_server(ID);
                return;
            }

            part++;
            Invoke(new Action(set_progress));
            ToServerLoaderQue(ID);
        }

        private void finish_load_file_to_server(long ID)
        {
            KCReactionSystem.RemoveReaction(ID);

            progress_global++;
            Invoke(new Action(set_progress_global));

            //FilesInfoLoaded.Add(current_fi);
            //if (FileUpLoadedEvent != null) FileUpLoadedEvent(new LKSProfileFileInfo[] { current_fi }, true);
            lock (KCF_MainForm.THIS.RootDirectory)
            {
                KCF_MainForm.THIS.RootDirectory.AddFilePath(current_fi);
                if (current_fi.Path == KCF_MainForm.THIS.CurrentDirView.Path ||
                    KCF_MainForm.THIS.RootDirectory.GoTo(current_fi.Path).ParentDirectory ==
                    KCF_MainForm.THIS.CurrentDirView)
                    KCF_MainForm.THIS.ShowProfileDirectoryView();
            }

            Loader(true);
        }
        #endregion

        #region ToLocal
        LKSFileDownloader downloader;
        void ToLocalLoaderFQue(long ID, bool linear)
        {
            if (!connected) return;
            while (paused) Thread.Sleep(1000);

            if (linear)
            {
                Invoke(new Action(get_first));

                downloader = new LKSFileDownloader(null, current_fi, KCF_MainForm.tmp_dir,
                    dest_path, current_fi.FileName.Replace(source_path, ""));

                parts = (int)current_fi.PartCount;
                part = 0;

                Invoke(new Action(set_file_infs));
            }

            send_req_send_part(ID);
        }

        private void send_req_send_part(long ID)
        {
            LKSTransferMethods.SendCommand(ID, KCClient.ClientSocket, LKSTransferCommands.SendFilePart,
                KCClient.TransferKey, disconnected_callback, "fileinfo", current_fi, "part", part);
        }

        void ToLocalLoaderFRea(long ID, LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            //var com = (LKSTranferCommand)LKSTransferContainer.ExtractContainer(container);
            //if (!connected) return;
            if (com.Command == LKSTransferCommands.MsgError)
            {
                string error = (string)com.Args["msg"];
                if (!error_continue(error)) return;
                send_tos_f_req(ID);
                return;
            }

            KCReactionSystem.RemoveReaction(ID);

            var tfdc = (LKSTranferFileDataContainer)com.Args["TFDC"];
            if (tfdc.CheckSum())
            {
                tfdc.Data = LKSJaffaKreeer.Jaffa_DeKree(tfdc.Data, KCF_MainForm.Pass);
                if (downloader.AcceptPart(tfdc.Data, tfdc.Part, tfdc.Size))
                {
                    if (FilesInfo.Count == 0) { cancel(); return; }
                    progress_global++;
                    Invoke(new Action(set_progress_global));

                    KCReactionSystem.AddResReaction(ID, ToLocalLoaderFRea);
                    ToLocalLoaderFQue(ID, true);

                    lock (this)
                        size_speed += tfdc.Size;

                    return;
                }
                else
                {
                    lock (this)
                        size_speed += tfdc.Size;

                    part++;
                    Invoke(new Action(set_progress));
                }
            }

            KCReactionSystem.AddResReaction(ID, ToLocalLoaderFRea);
            send_req_send_part(ID);
            //ToLocalLoaderQue(ID);
        }
        #endregion
        #endregion

        #region selected remove
        void set_labelto_rem_dir()
        {
            labelTO.Text = remove_path;
        }

        Queue<string> queue_dirs = new Queue<string>();
        Queue<LKSProfileFileInfo> queue_files = new Queue<LKSProfileFileInfo>();
        string remove_path;
        LKSProfileFileInfo remove_file;
        void RemoveDirQue(long ID)
        {
            if (queue_dirs.Count == 0) { remove_path = null; cancel(); return; }
            remove_path = queue_dirs.Dequeue();
            if (!connected) return;
            while (paused) Thread.Sleep(1000);

            Invoke(new Action(set_labelto_rem_dir));

            KCReactionSystem.AddResReaction(ID, RemoveDirRea);
            LKSTransferMethods.SendCommand(ID, KCClient.ClientSocket, LKSTransferCommands.RemoveDir,
                KCClient.TransferKey, disconnected_callback,
                "dir", remove_path);
        }

        void RemoveDirRea(long ID, LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            //var com = (LKSTranferCommand)LKSTransferContainer.ExtractContainer(container);
            if (com.Command == LKSTransferCommands.MsgError)
            {
                string error = (string)com.Args["msg"];
                KCF_MainForm.ShowError(error);
            }
            else
            {
                lock (KCF_MainForm.THIS.RootDirectory)
                {
                    KCF_MainForm.THIS.RootDirectory.RemoveDirectoryInPath(remove_path);
                    KCF_MainForm.THIS.ShowProfileDirectoryView();
                }

                part++;
                Invoke(new Action(set_progress));
                //ShowMessage("Directory " + remove_path + " deleted", "Info", MessageBoxIcon.Information);
            }
            KCReactionSystem.RemoveReaction(ID);
            RemoveDirQue(ID);
        }

        void RemoveFileQue(long ID)
        {
            if (queue_files.Count == 0) { remove_file = null; RemoveDirQue(ID); return; }
            var fi = queue_files.Dequeue();
            remove_file = fi;
            if (!connected) return;
            while (paused) Thread.Sleep(1000);

            current_fn = fi.FileName;
            Invoke(new Action(set_file_infs));

            KCReactionSystem.AddResReaction(ID, RemoveFileRea);
            LKSTransferMethods.SendCommand(ID, KCClient.ClientSocket, LKSTransferCommands.RemoveFile,
                KCClient.TransferKey, disconnected_callback,
                "fileinfo", fi);
        }

        void RemoveFileRea(long ID, LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            //var com = (LKSTranferCommand)LKSTransferContainer.ExtractContainer(container);
            if (com.Command == LKSTransferCommands.MsgError)
            {
                string error = (string)com.Args["msg"];
                KCF_MainForm.ShowError(error);
            }
            else
            {
                lock (KCF_MainForm.THIS.RootDirectory)
                {
                    KCF_MainForm.THIS.RootDirectory.RemoveFileInPath(remove_file);
                    KCF_MainForm.THIS.ShowProfileDirectoryView();
                }

                progress_global++;
                Invoke(new Action(set_progress_global));
                //ShowMessage("Directory " + remove_path + " deleted", "Info", MessageBoxIcon.Information);
            }
            KCReactionSystem.RemoveReaction(ID);
            RemoveFileQue(ID);
        }
        #endregion

        #region Copy/Move
        bool cm_remove;
        void CopyMoveFQue(long ID, bool linear)
        {
            if (!connected) return;
            while (paused) Thread.Sleep(1000);

            if (linear)
            {
                if (FilesInfo.Count == 0) { cancel(); return; }

                Invoke(new Action(get_first));

                Invoke(new Action(set_file_infs));
            }

            string new_fn = current_fi.FileName.Replace(source_path, dest_path);
            if (!check_file_replace(new_fn))
            {
                CopyMoveFQue(ID, linear);
                return;
            }

            KCReactionSystem.AddResReaction(ID, CopyMoveFRea);
            LKSTransferMethods.SendCommand(ID, KCClient.ClientSocket, LKSTransferCommands.CopyFile,
                KCClient.TransferKey, disconnected_callback
                , "fileinfo", current_fi, "from", source_path, "to", dest_path,
                "remove", cm_remove);
        }

        void CopyMoveFRea(long ID, LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            if (com.Command == LKSTransferCommands.MsgError)
            {
                string error = (string)com.Args["msg"];
                KCF_MainForm.ShowError(error);
            }
            else
            {
                lock (KCF_MainForm.THIS.RootDirectory)
                {


                    //KCF_MainForm.THIS.RootDirectory.RemoveFileInPath(current_fi);

                    //string new_path = current_fi.Path.Replace(source_path, dest_path);
                    if (cm_remove)
                    {
                        //current_fi.Path = new_path;
                        //KCF_MainForm.THIS.RootDirectory.AddFilePath(current_fi);
                        KCF_MainForm.THIS.Profile.FileCollection.Move(current_fi.ID, source_path, dest_path);
                    }
                    else
                    {
                        KCF_MainForm.THIS.Profile.FileCollection.Copy(current_fi.ID,
                            (long)com.Args["ID"], source_path, dest_path);

                        //var new_fi = current_fi.Copy();
                        //new_fi.ID = (long)com.Args["ID"];
                        //new_fi.Path = new_path;
                        //KCF_MainForm.THIS.RootDirectory.AddFilePath(new_fi);
                    }

                    //KCF_MainForm.THIS.ShowProfileDirectoryView();
                }

                progress_global++;
                Invoke(new Action(set_progress_global));
            }
            KCReactionSystem.RemoveReaction(ID);
            CopyMoveFQue(ID, true);
        }
        #endregion

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            cancel();
        }

        public static string[] GetLocalFilesDirecory(string path)
        {
            try
            {
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                return files;
            }
            catch (Exception exc) { KCF_MainForm.ShowError(exc.Message); return new string[0]; }
            //return (from f in files select new LKSProfileFileInfo(f)).ToArray();
        }

        private void KCF_LoaderForm_2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!Visible && strip_progress != null) KCF_MainForm.THIS.Invoke(new Action(strip_progress_rem));
            //if (FileUpLoadedEvent != null) FileUpLoadedEvent(FilesInfoLoaded.ToArray());
            if (tfmode == TFMODE.CopyMove)
            {
                lock (KCF_MainForm.THIS.RootDirectory)
                {
                    string cur_path = KCF_MainForm.THIS.CurrentDirView.Path;
                    KCF_MainForm.THIS.RootDirectory = KCF_MainForm.THIS.Profile.FileCollection.BuildRootDirectory();
                    var dir = KCF_MainForm.THIS.RootDirectory.GoTo(cur_path);
                    KCF_MainForm.THIS.ShowProfileDirectoryView(dir == null ? KCF_MainForm.THIS.RootDirectory : dir);
                }
            }
            else
                rebuild_file_collection();
        }

        private static void rebuild_file_collection()
        {
            lock (KCF_MainForm.THIS.RootDirectory)
            {
                KCF_MainForm.THIS.Profile.FileCollection = KCF_MainForm.THIS.RootDirectory.BuildFileCollcetion();
            }
        }


        ToolStripProgressBar strip_progress = null;
        private void buttonHide_Click(object sender, EventArgs e)
        {
            if (strip_progress == null)
            {
                strip_progress = new ToolStripProgressBar(req_id.ToString());
                strip_progress.Maximum = progressBarglobal.Maximum;
                strip_progress.Click += strip_progress_Click;
            }
            KCF_MainForm.THIS.Invoke(new Action(strip_progress_add));
            Visible = false;
        }

        void strip_progress_Click(object sender, EventArgs e)
        {
            Visible = true;
            KCF_MainForm.THIS.Invoke(new Action(strip_progress_rem));
        }

        void strip_progress_rem()
        {
            KCF_MainForm.THIS.statusStrip1.Items.Remove(strip_progress);
        }

        void strip_progress_add()
        {
            KCF_MainForm.THIS.statusStrip1.Items.Add(strip_progress);
            set_strip_progress();
        }
        void set_strip_progress()
        {
            if (strip_progress != null) strip_progress.Value = progress_global;
        }

        private void KCF_LoaderForm_2_Load(object sender, EventArgs e)
        {
            if (tfmode == TFMODE.Remove)
                RemoveFileQue(req_id);
            else if (tfmode == TFMODE.CopyMove)
                CopyMoveFQue(req_id, true);
            else
                Loader(true);

            upd_speed_act = new Action(upd_speed);
            timer1.Start();
        }

        void upd_speed()
        {
            string m; float mc;
            lock (this)
            {
                LKSProfileFileInfo.CalcSizeM(size_speed, out mc, out m);
                size_speed = 0;
            }
            labelspeed.Text = string.Format("{0} {1}/s", mc, m);
        }
        Action upd_speed_act;
        private void timer1_Tick(object sender, EventArgs e)
        {
            Invoke(upd_speed_act);
        }

        private void KCF_LoaderForm_2_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }
    }
}

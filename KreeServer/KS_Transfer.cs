using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using KreeLib;
using System.Runtime.Serialization;
using System.Threading;
using System.IO;

namespace KreeServer
{
    public static class KSServer
    {
        public static byte[] TransferKey;
        public static void InitServer()
        {
            //set ramdom transfer key
            var rnd = new Random();
            TransferKey = new byte[8];
            rnd.NextBytes(TransferKey);

            if (!Directory.Exists(KSServerConfig.files_dir)) Directory.CreateDirectory(KSServerConfig.files_dir);
            if (!Directory.Exists(KSServerConfig.tmp_dir)) Directory.CreateDirectory(KSServerConfig.tmp_dir);
            if (!Directory.Exists(KSServerConfig.DBProfiles_dir)) Directory.CreateDirectory(KSServerConfig.DBProfiles_dir);
            //init mutexs
            //ProfileDB.AuthProfiles_Mutex = new Mutex(false);
        }

        public static KSServerConfig Config = new KSServerConfig();
        public static KSProfileDB ProfileDB = new KSProfileDB();

        public static Socket ServerSocket;
        public static List<KSConnectionSession> ConnectionSessios = new List<KSConnectionSession>();
        //public static Mutex ConnectionSessios_Mutex = new Mutex(false);

        public static bool Started { get; private set; }
        public static void Start()
        {
            KSServer.Config.Load();
            KSServer.ProfileDB.Load();
            KSServer.InitSocket();
            Started = true;

            //KSConnectionSession.PingWatherThread.IsBackground = true;
            //KSConnectionSession.PingWatherThread.Start();
        }

        public static void InitSocket()
        {
            LKSLog.Log("Initing socket...");
            //KSServer.ServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);//NET 4.5
            KSServer.ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//NET 3.5

            KSServer.ServerSocket.Bind(new System.Net.IPEndPoint(IPAddress.Any, Config.Port));
            KSServer.ServerSocket.Listen(Config.MaxQueue);
            KSServer.ServerSocket.BeginAccept(callback_socket_accept, null);

            LKSLog.Log(string.Format("Socket inited: Port={1} MaxQueue={2}",
                ((IPEndPoint)ServerSocket.LocalEndPoint).Address.ToString(),
                ((IPEndPoint)ServerSocket.LocalEndPoint).Port, Config.MaxQueue));
        }
        static void callback_socket_accept(IAsyncResult res)
        {
            KSServer.ServerSocket.BeginAccept(callback_socket_accept, null);

            try
            {
                var conn = new KSConnectionSession(KSServer.ServerSocket.EndAccept(res));

                //Sync
                //ConnectionSessios_Mutex.WaitOne();
                lock (ConnectionSessios)
                {
                    KSServer.ConnectionSessios.Add(conn);
                }
                //ConnectionSessios_Mutex.ReleaseMutex();

                LKSLog.BILog(string.Format("Incoming connect IPP={0}. Count={1}",
                    conn.IPPORT, KSServer.ConnectionSessios.Count), LKSLog.LogType.Info);

                conn.Send_TFK();
            }
            catch (Exception exc) { LKSLog.BILog(exc); }
        }
    }
    public partial class KSCommandProcessor
    {
        public static void SendMesssage(long ID, KSConnectionSession session, string mess, LKSTransferCommands type)
        {
            var comm = new LKSTranferCommand(type);
            if (mess != null) comm.AddArg("msg", mess);
            comm.Send(ID, session.CSocket, KSServer.TransferKey,session.SendingErrorCallbackClose);
        }
        public static void SendSuccess(long ID, KSConnectionSession session, string mess)
        { SendMesssage(ID, session, mess, LKSTransferCommands.MsgSuccess); }
        public static void SendError(long ID, KSConnectionSession session, string mess)
        { SendMesssage(ID, session, mess, LKSTransferCommands.MsgError); }


    }
    public partial class KSCommandProcessor
    {
        public static void process_LKSTransferContainer(KSConnectionSession session, LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            try
            {
                //ILKSClassSendable data_class = LKSTransferContainer.ExtractContainer(container);
                long ID = com.ID;//container.ID;
                //COMMAND
                //if (container.TranceType == LKSTransferTypes.Command)
                //{
                //var com = (LKSTranferCommandContainer)data_class;
                if (com.Command == LKSTransferCommands.Ping)
                {
                    session.Pinged = true;
                    SendMesssage(0, session, null, LKSTransferCommands.Ping); return;
                }

                //if (com.Command != LKSTransferCommands.ReceiveFilePart)
                //   LKSLog.BILog(string.Format("Received command IPP={0} CommandType={1}", session.IPPORT, com.Command));

                //RECEIVED TRANSFER KEY
                if (com.Command == LKSTransferCommands.TransferKey) session.TFK_Received = true;

                //REG AUTH DELPROFILE
                if (com.Command == LKSTransferCommands.Registry || com.Command == LKSTransferCommands.Auth || com.Command == LKSTransferCommands.DeleteProfile)
                    LoginRegistryDelete(ID, session, com);

                //CREATE FILE DOWNLOADER
                if (com.Command == LKSTransferCommands.CreateFileDownloader)
                    CreateFileDownloader(ID, session, com);

                //SEND FILE PART
                if (com.Command == LKSTransferCommands.SendFilePart)
                    SendFilePart(ID, session, com);

                //RECEIVE FILE PART
                if (com.Command == LKSTransferCommands.ReceiveFilePart)
                    ReceiveFilePart(ID, session, com);

                //CLEAR FILE DB
                if (com.Command == LKSTransferCommands.ClearFileDB)
                    ClearFileDB(ID, session);

                //CANCEL DOWNLOADER FILE
                if (com.Command == LKSTransferCommands.CancelDownload)
                    CancelDownloaderFiles(ID, session);

                //REMOVE FILE
                if (com.Command == LKSTransferCommands.RemoveFile)
                    RemoveFile(ID, session, com);

                //REMOVE DIR
                if (com.Command == LKSTransferCommands.RemoveDir)
                    RemoveDirectory(ID, session, com);

                //CopyMove
                if (com.Command == LKSTransferCommands.CopyFile)
                    CopyFile(ID, session, com);

                if (!LKSLog.ShutdownLogging)
                {
                    if (com.Command == LKSTransferCommands.MsgInfo)
                        LKSLog.BILog(string.Format("IPP={0} Msg={1}", session.IPPORT, (string)com.Args["msg"]));
                    if (com.Command == LKSTransferCommands.MsgWarning)
                        LKSLog.BILog(string.Format("IPP={0} Msg={1}", session.IPPORT, (string)com.Args["msg"]), LKSLog.LogType.Warning);
                    if (com.Command == LKSTransferCommands.MsgError)
                        LKSLog.BILog(string.Format("IPP={0} Msg={1}", session.IPPORT, (string)com.Args["msg"]), LKSLog.LogType.Warning);
                }
                //}
            }
            //catch (SerializationException)
            //{
            //    LKSLog.BILog(string.Format("DeKry Error[DeSerial | Receice process]: IPP={0}", session.IPPORT), LKSLog.LogType.Error);
            //}
            catch (Exception exc) { LKSLog.BILog(exc); }
        }

        #region COMMADNS DO

        static void CopyFile(long ID, KSConnectionSession session, LKSTranferCommand com)
        {
            try
            {
                var fi = (LKSProfileFileInfo)com.Args["fileinfo"];
                string from = (string)com.Args["from"];
                string to = (string)com.Args["to"];
                bool remove = (bool)com.Args["remove"];

                long new_id = fi.ID;
                if (remove)
                    session.Profile.FileCollection.Move(fi.ID, from, to);
                else
                {
                    new_id = session.Profile.NextFileID;///KSServer.Config.NextFileID;
                    session.Profile.FileCollection.Copy(fi.ID, new_id, from, to);
                }

                LKSTransferMethods.SendCommand(ID, session.CSocket, LKSTransferCommands.MsgSuccess,
                    KSServer.TransferKey, session.SendingErrorCallbackClose,
                    "ID", new_id);

                LKSLog.BILog(string.Format("IPP={0} Copy File reqID={1} Remove={5} From={2} To={3} Name={4}",
                    session.IPPORT, ID, from, to, fi.NameExt, remove));
            }
            catch (Exception exc)
            {
                SendError(ID, session, exc.Message);
                throw exc;
            }
        }

        //static void GetProfileFileCollection(long ID, KSConnectionSession session)
        //{
        //    try
        //    {
        //        LKSTransferMethods.SendCommand(ID, session.CSocket, LKSTransferCommands.FileCollection,
        //            KSServer.TransferKey, "filecollcetion", session.Profile.FileCollection);
        //        LKSLog.BILog(string.Format("IPP={0} Sended Profile FileCollection reqID={1}", session.IPPORT, ID));
        //    }
        //    catch (Exception exc)
        //    {
        //        SendError(ID, session.CSocket, exc.Message);
        //        throw exc;
        //    }
        //}

        static void CancelDownloaderFiles(long ID, KSConnectionSession session)
        {
            try
            {
                session.FilesDownlaoder.Abort(ID);
                SendSuccess(ID, session, null);
                LKSLog.BILog(string.Format("IPP={0} Canceled DownloaderFiles reqID={1}", session.IPPORT, ID));
            }
            catch (Exception exc)
            {
                SendError(ID, session, exc.Message);
                throw exc;
            }
        }

        static void RemoveFile(long ID, KSConnectionSession session, LKSTranferCommand com)
        {
            try
            {
                var fi = (LKSProfileFileInfo)com.Args["fileinfo"];
                session.Profile.FileCollection.Remove(/*KSServerConfig.files_dir*/session.FilesDir, fi.ID);
                SendSuccess(ID, session, null);
                LKSLog.BILog(string.Format("IPP={0} Removed File={1}", session.IPPORT, fi.FileName));
            }
            catch (Exception exc)
            {
                SendError(ID, session, exc.Message);
                throw exc;
            }
        }

        static void ClearFileDB(long ID, KSConnectionSession session)
        {
            try
            {
                session.Profile.FileCollection.Clear(/*KSServerConfig.files_dir*/session.FilesDir);
                SendSuccess(ID, session, null);
                LKSLog.BILog(string.Format("IPP={0} Cleared FileDB", session.IPPORT));
            }
            catch (Exception exc)
            {
                SendError(ID, session, exc.Message);
                throw exc;
            }
        }

        static void LoginRegistryDelete(long ID, KSConnectionSession session, LKSTranferCommand com)
        {
            string error = "";
            string login = (string)com.Args["login"];
            string pass = (string)com.Args["pass"];

            LKSProfile p = null;
            if (com.Command == LKSTransferCommands.Registry) p = KSServer.ProfileDB.Registry(login, pass, out error);
            else p = KSServer.ProfileDB.Login(login, pass, out error, com.Command == LKSTransferCommands.DeleteProfile);


            LKSLog.LogType tp = LKSLog.LogType.Info;
            if (error != null)
            {
                tp = LKSLog.LogType.Error;
                SendError(ID, session, error);
            }
            else
            {
                if (com.Command != LKSTransferCommands.DeleteProfile)
                {
                    session.Profile = p;
                    session.Authed = true;
                    session.FilesDir = p.FilesDir;

                    LKSTransferMethods.SendCommand(ID, session.CSocket, LKSTransferCommands.MsgSuccess,
                        KSServer.TransferKey, session.SendingErrorCallbackClose,
                        "profile", (LKSProfileSendable)p);
                    //SendSuccess(ID, session.CSocket, null);
                }
                else SendSuccess(ID, session, "Profile " + login + " removed.");


                //upd profile view
                if (com.Command == LKSTransferCommands.Registry)
                    KSFMainForm.add_login_index_view(new KSProfileAuthRec(login, pass), true, session.IPPORT);
                if (com.Command == LKSTransferCommands.Auth)
                    KSFMainForm.THIS.set_profile_view(new KSProfileAuthRec(login, pass), true, session.IPPORT);
                if (com.Command == LKSTransferCommands.DeleteProfile)
                    KSFMainForm.THIS.rem_profile_view(new KSProfileAuthRec(login, pass), false, null);
            }
            LKSLog.BILog(string.Format("IPP={3} {2} Login={0} Error={1}", login, error, com.Command,
                session.IPPORT), tp);
        }

        static byte[] buffer_SendFilePart = new byte[LKSConsts.TransferFileBufferSize];
        static void SendFilePart(long ID, KSConnectionSession session, LKSTranferCommand com)
        {
            try
            {
                var fi = (LKSProfileFileInfo)com.Args["fileinfo"];
                int part = (int)com.Args["part"];


                var reader = File.OpenRead(fi.InnerFileName(/*KSServerConfig.files_dir*/session.FilesDir));
                reader.Seek(part * LKSConsts.TransferFileBufferSize, SeekOrigin.Begin);
                int readed = reader.Read(buffer_SendFilePart, 0, LKSConsts.TransferFileBufferSize);
                reader.Close();

                var fp = new LKSTranferFileDataContainer(buffer_SendFilePart, part, readed);

                //fp.Send(ID, session.CSocket, KSServer.TransferKey);
                LKSTransferMethods.SendCommand(ID, session.CSocket, LKSTransferCommands.ReceiveFilePart,
                    KSServer.TransferKey, session.SendingErrorCallbackClose, "TFDC", fp);

                //LKSLog.BILog(string.Format("IPP={0} Sended FilePart reqID={4} Part={2} Size={1} File={3}", session.IPPORT, readed, part, fi.FileName, ID));
            }
            catch (Exception exc)
            {
                SendError(ID, session, exc.Message);
                throw exc;
            }
        }

        static void RemoveDirectory(long ID, KSConnectionSession session, LKSTranferCommand com)
        {
            try
            {
                string dir = (string)com.Args["dir"];
                LKSProfileFileInfo[] fis = session.Profile.FileCollection.Find(x => x.FileName.Contains(dir));
                foreach (var f in fis)
                    session.Profile.FileCollection.Remove(/*KSServerConfig.files_dir*/session.FilesDir, f.ID);

                SendSuccess(ID, session, null);
                LKSLog.BILog(string.Format("IPP={0} Removed Direcory Dir={1}", session.IPPORT, dir));

            }
            catch (Exception exc)
            {
                SendError(ID, session, exc.Message);
                throw exc;
            }
        }

        static void CreateFileDownloader(long ID, KSConnectionSession session, LKSTranferCommand com)
        {
            try
            {
                var fi = (LKSProfileFileInfo)com.Args["fileinfo"];
                fi.ID = session.Profile.NextFileID;//KSServer.Config.NextFileID;

                bool linked = session.Profile.FileCollection.CheckLinkFile(fi) > 0;
                if (linked)
                    session.Profile.FileCollection.AddReplace(/*KSServerConfig.files_dir*/
                        session.FilesDir, fi.ID, fi);
                else
                    session.FilesDownlaoder.AddDownloader(ID, session.Profile, fi,
                        KSServerConfig.tmp_dir + session.FilesDir, /*KSServerConfig.files_dir*/session.FilesDir);

                LKSTransferMethods.SendCommand(ID, session.CSocket, LKSTransferCommands.MsgSuccess, 
                    KSServer.TransferKey, session.SendingErrorCallbackClose,
                    "ID", fi.ID, "linked", linked);

                LKSLog.BILog(string.Format("IPP={0} Created FileDownloader reqID={2} Parts={3} File={1}", session.IPPORT, fi.FileName, ID, fi.PartCount));
            }
            catch (Exception exc)
            {
                SendError(ID, session, exc.Message);
                throw exc;
            }
        }

        static void ReceiveFilePart(long ID, KSConnectionSession session, LKSTranferCommand com)
        {
            try
            {
                var data = (LKSTranferFileDataContainer)com.Args["TFDC"];
                if (!data.CheckSum()) throw new InvalidDataException("checksum error");

                if (session.FilesDownlaoder.AcceptFilePart(ID, data.Data, data.Part, data.Size))
                {
                    session.FilesDownlaoder.Abort(ID);
                }
                //LKSLog.BILog(string.Format("IPP={0} Received FilePart reqID={3} Part={2} Size={1}", session.IPPORT, data.Size, data.Part, ID));
                SendSuccess(ID, session, null);
            }
            catch (Exception exc)
            {
                SendError(ID, session, exc.Message);
                throw exc;
            }
        }
        #endregion
    }

    public class KSConnectionSession
    {
        public string IP;
        public int Port;
        public Socket CSocket;
        public LKSProfile Profile;
        public bool Authed = false;
        public bool Pinged = false;
        public bool TFK_Received = false;
        public bool Opened = true;

        public string FilesDir;

        public string IPPORT;

        //public int _index;
        byte[] buffer;
        int buffer_size;
        SocketError socket_error;
        AsyncCallback async_call_back;

        int received_bytes;

        public KSFilesDownloader FilesDownlaoder = new KSFilesDownloader();

        void CallbackReceive(IAsyncResult res)
        {
            try
            {
                if (!Opened) return;
                if (!socket_error_success()) return;
                received_bytes = CSocket.EndReceive(res, out socket_error);
                if (!socket_error_success()) return;

                //LKSTransferContainer container = LKSSerializer.DeSerial<LKSTransferContainer>(LKSKryer.DeKry(buffer, KSServer.TransferKey));
                LKSTranferCommand container = LKSSerializer.DeSerial<LKSTranferCommand>(LKSJaffaKreeer.Jaffa_DeKree(buffer, KSServer.TransferKey));


                CSocket.BeginReceive(buffer, 0, buffer_size, SocketFlags.None, out socket_error, CallbackReceive, null);

                KSCommandProcessor.process_LKSTransferContainer(this, container);
            }
            catch (SerializationException)
            {
                //IT'S BAD
                CSocket.BeginReceive(buffer, 0, buffer_size, SocketFlags.None, out socket_error, CallbackReceive, null);
                LKSLog.BILog("Received wrond data from " + IPPORT, LKSLog.LogType.Error);
            }
            catch (Exception exc) { LKSLog.BILog(exc); }
        }

        bool socket_error_success()
        {
            if (socket_error != SocketError.Success)
            {
                LKSLog.BILog(string.Format("SocketError IPP={0}:{1}", IPPORT, socket_error), LKSLog.LogType.Error);
                CloseSession(this);
                return false;
            }
            return true;
        }

        public KSConnectionSession(Socket trans_socket)
        {
            trans_socket.ReceiveBufferSize = LKSConsts.TransferBufferSize;
            trans_socket.SendBufferSize = LKSConsts.TransferBufferSize;
            CSocket = trans_socket;
            IP = ((IPEndPoint)trans_socket.RemoteEndPoint).Address.ToString();
            Port = ((IPEndPoint)trans_socket.RemoteEndPoint).Port;

            IPPORT = IP/*.Substring(7)*/ + ':' + Port;

            async_call_back = new AsyncCallback(CallbackReceive);
            //settings
            buffer_size = LKSConsts.TransferBufferSize;
            buffer = new byte[buffer_size];

            //Send_TK();
            Pinged = true;

            //IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state);
            CSocket.BeginReceive(buffer, 0, buffer_size, SocketFlags.None, out socket_error, CallbackReceive, null);
        }

        public void Send_TFK()
        {
            var tf_cont = new LKSTranferCommand(LKSTransferCommands.TransferKey);
            tf_cont.AddArg("tfk", KSServer.TransferKey);
            tf_cont.Send(0, CSocket, new byte[] { 7, 6, 42, 3 }, SendingErrorCallbackClose);
        }

        public void SendingErrorCallbackClose(string error)
        {
            LKSLog.BILog(string.Format("IPP={0} Sending Error: {1}", IPPORT, error));
            CloseSession(this);
        }

        public static void CloseSession(KSConnectionSession session)
        {
            if (!session.Opened) return;
            string mess = null;
            session.Opened = false;
            session.CSocket.Close();
            //session.CSocket.Dispose();

            session.FilesDownlaoder.Abort();

            //KSServer.ConnectionSessios_Mutex.WaitOne();
            lock (KSServer.ConnectionSessios)
            {
                KSServer.ConnectionSessios.Remove(session);
                if (!LKSLog.ShutdownLogging) mess = string.Format("Session IPP={0} closed. Count={1}", session.IPPORT, KSServer.ConnectionSessios.Count);
            }
            //KSServer.ConnectionSessios_Mutex.ReleaseMutex();

            KSServer.ProfileDB.DeAuth(session.Profile);

            LKSLog.BILog(mess);
        }

        //Thead of ping watcher of sessions
        private static ThreadStart PingWatherThreadStart = new ThreadStart(PingWatcher);
        internal static Thread PingWatherThread = new Thread(PingWatherThreadStart);
        private static void PingWatcher()
        {
            while (KSServer.Started)
            {
                Thread.Sleep(LKSConsts.PungTimeOutCheck);
                for (int i = 0; i < KSServer.ConnectionSessios.Count; )
                {
                    if (!KSServer.ConnectionSessios[i].Pinged)
                        CloseSession(KSServer.ConnectionSessios[i]);
                    else KSServer.ConnectionSessios[i++].Pinged = false;

                    if (!KSServer.Started)//Close all sessions
                    {
                        while (KSServer.ConnectionSessios.Count != 0) CloseSession(KSServer.ConnectionSessios[0]);
                        break;
                    }
                }
            }
        }
    }

    public class KSFilesDownloader
    {
        Dictionary<long, LKSFileDownloader> Dowloaders = new Dictionary<long, LKSFileDownloader>();

        public void AddDownloader(long ID, LKSProfile p, LKSProfileFileInfo fi, string tmp_dir, string dest_dir)
        {
            if (Dowloaders.ContainsKey(ID)) Abort(ID);
            Dowloaders.Add(ID, new LKSFileDownloader(p, fi, tmp_dir, dest_dir));
        }

        public bool AcceptFilePart(long ID, byte[] data, long part, int size)
        {
            if (!Dowloaders.ContainsKey(ID)) throw new ArgumentException("Downloader ID not found", "ID");
            return Dowloaders[ID].AcceptPart(data, part, size);
        }

        public void Abort(long ID)
        {
            if (!Dowloaders.ContainsKey(ID)) return;
            Dowloaders[ID].Abort();
            Dowloaders.Remove(ID);
        }

        public void Abort()
        {
            while (Dowloaders.Count > 0)
            {
                Dowloaders.Values.ElementAt(0).Abort();
                Dowloaders.Remove(Dowloaders.Keys.ElementAt(0));
            }
        }
    }
}
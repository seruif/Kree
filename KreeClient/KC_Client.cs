using KreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace KreeClient
{
    public struct KCEventArgs
    {
        public Exception error;
        public string message;

        public KCEventArgs(string mes=null,Exception err=null)
        {
            message = mes;
            error = err;
        }
    }
    class KCClient
    {
        static long last_req_id = 1;
        public static long NextReqID { get { if (last_req_id == long.MaxValue) last_req_id = 1; return last_req_id++; } }

        public static Socket ClientSocket;
        public static byte[] TransferKey;

        static byte[] buffer;
        static int buffer_size;
        static SocketError socket_error;
        //static AsyncCallback async_call_back; //What unused?

        static int received_bytes;

        public static bool _TFK_Recieved = false;
        public static bool TFK_Recieved
        {
            get { return _TFK_Recieved; }
            set
            {
                _TFK_Recieved = value;
                if (_TFK_Recieved)
                    KCF_MainForm.THIS.Invoke(new Action(() => KCF_MainForm.THIS.toolStripStatusLabelAction.Text = "TFK Received"));
                else
                    KCF_MainForm.THIS.Invoke(new Action(() => KCF_MainForm.THIS.toolStripStatusLabelAction.Text = "TFK Not Received"));
            }
        }

        //events
        public delegate void KSC_EventHandler(KCEventArgs e);

        public static event KSC_EventHandler BeginConnectingEvent;
        public static event KSC_EventHandler EndConnectingEvent;
        public static event KSC_EventHandler ConnectionLostEvent;

        public static bool socket_error_success(bool fail)
        {
            if (socket_error != SocketError.Success || fail)
            {
                //MainForm.ShowError(socket_error.ToString());
                //var ea = new KCEventArgs() { error = !fail ? new Exception(socket_error.ToString()) : null, message = "Connection lost. Reconnecting." };
                //ConnectionLostEvent(ea);
                //BeginConnectingEvent(ea);
                ReconnectSocketEvent(!fail ? new Exception(socket_error.ToString()) : null);
                return false;
            }
            return true;
        }

        static bool first_connected = false;
        static void CallbackConnect(IAsyncResult ar)
        {
            //if (ClientSocket.Connected) MessageBox.Show("Connnected");
            //else MessageBox.Show("Fail");
            try
            {
                ClientSocket.EndConnect(ar);

                AsyncCallback call_back_receiver = new AsyncCallback(CallbackReceive);

                ClientSocket.BeginReceive(buffer, 0, LKSConsts.TransferBufferSize, SocketFlags.None, out socket_error, call_back_receiver, null);

                Pinged = true;
                EndConnectingEvent(new KCEventArgs());
                first_connected = true;
            }
            catch (SocketException exc)
            {
                if (!first_connected)
                {
                    //KCF_MainForm.ShowError(exc.Message);
                    EndConnectingEvent(new KCEventArgs() { error = exc });
                    return;
                }
                while (true)
                {
                    try
                    {
                        begin_connect(serverIP, serverPort);
                        break;
                    }
                    catch (SocketException exc2) { }
                    catch (Exception exc3)
                    {
                        EndConnectingEvent(new KCEventArgs() { error = exc3 });
                        //MainForm.ShowError(exc.Message);
                    }
                }
            }
            catch (Exception exc)
            {
                var ea = new KCEventArgs() { error = exc };
                EndConnectingEvent(ea);
                ConnectionLostEvent(ea);
                //MainForm.ShowError(exc.Message);
            }
        }

        static void CallbackReceive(IAsyncResult res)
        {
            try
            {
                if (!socket_error_success(false)) return;
                received_bytes = ClientSocket.EndReceive(res, out socket_error);
                if (!socket_error_success(false)) return;

                //var tcc = LKSSerializer.DeSerial<LKSTransferContainer>(LKSKryer.DeKry(buffer, TransferKey));
                var tcc = LKSSerializer.DeSerial<LKSTranferCommand>(LKSJaffaKreeer.Jaffa_DeKree(buffer, TransferKey));

                ClientSocket.BeginReceive(buffer, 0, buffer_size, SocketFlags.None, out socket_error, CallbackReceive, null);

                KCCommandProcessor.process_LKSTransferContainer(tcc);
            }
            catch (SerializationException exc)
            {
                KCF_MainForm.ShowError("Received wrong data. May be wrong transfer key.");
                if (MessageBox.Show("Restart client?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Restart();
                    return;
                }
                KCClient.ReconnectSocketEvent(exc);
            }
            catch (Exception exc)
            {
                //KCF_MainForm.ShowError(exc.Message);
                KCClient.ReconnectSocketEvent(exc);
            }
        }

        static string serverIP;
        static int serverPort;
        public static void Connect(string ip, int port)
        {
            BeginConnectingEvent(new KCEventArgs() { error = null, message = "Connecting with " + ip + ':' + port });
            serverIP = ip;
            serverPort = port;
            begin_connect(ip, port);
        }
        static void begin_connect(string ip, int port)
        {
            //init_tfk();
            ClientSocket.BeginConnect(ip, port, CallbackConnect, null);
        }
        public static void ReconnectSocket()
        {
            ClientSocket.Disconnect(true);
            begin_connect(serverIP, serverPort);
        }

        public static void ReconnectSocketEvent(Exception exc)
        {
            var ea = new KCEventArgs() { error = exc, message = "Connection lost. Reconnecting." };
            ConnectionLostEvent(ea);
            BeginConnectingEvent(ea);
            ReconnectSocket();
        }

        public static void CallBackSendingError(string error)
        {
            ReconnectSocketEvent(new Exception(error));
        }

        public static void Init()
        {
            init_tfk();

            buffer = new byte[LKSConsts.TransferBufferSize];
            buffer_size = LKSConsts.TransferBufferSize;

            //ClientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp); //NET 4.5
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//NET 3.5
        }
        static void init_tfk() { TransferKey = new byte[] { 7, 6, 42, 3 }; TFK_Recieved = false; }

        //public static void Send_TCC(long ID, LKSTranferCommand cont)
        //{
        //    cont.Send(ID, ClientSocket, TransferKey);
        //}

        public static bool Pinged = false;
    }

    public class KCCommandProcessor
    {
        public static void process_LKSTransferContainer(LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            try
            {
                //ILKSClassSendable data_class = LKSTransferContainer.ExtractContainer(container);

                //COMMAND TYPE
                //if (container.TranceType == LKSTransferTypes.Command)
                //{
                //    var com = (LKSTranferCommand)data_class;

                //RECEIVED TRANSFER KEY
                if (com.Command == LKSTransferCommands.TransferKey)
                {
                    KCClient.TransferKey = (byte[])com.Args["tfk"];
                    com.Args.Clear();
                    com.Send(0, KCClient.ClientSocket, KCClient.TransferKey, KCClient.CallBackSendingError);
                    KCClient.TFK_Recieved = true;
                    return;
                }

                //if (com.Command == LKSTransferCommands.Ping) { KCClient.Pinged = true; return; }

                //}
                if (com.ID == 0)
                {
                    MessageBoxIcon ico = com.Command == LKSTransferCommands.MsgError ? MessageBoxIcon.Error :
                        (com.Command == LKSTransferCommands.MsgWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    MessageBox.Show((string)com.Args["msg"], ico.ToString(), MessageBoxButtons.OK, ico);
                    return;
                }

                KCReactionSystem.Recieve(/*container*/com, false);
            }
            catch (Exception exc)
            {
                KCF_MainForm.ShowError(exc.Message);
            }
        }
    }
}

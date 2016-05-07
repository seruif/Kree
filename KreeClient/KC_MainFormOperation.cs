using KreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KreeClient
{
    public partial class KCF_MainForm : Form
    {
        LKSProfileSendable _profile;
        public LKSProfileDirectory RootDirectory;
        public LKSProfileSendable Profile
        {
            get { return _profile; }
            set
            {
                _profile = value;
                RootDirectory = _profile.FileCollection.BuildRootDirectory();
                ShowProfileDirectoryView(RootDirectory);
            }
        }

        #region AUTH
        bool _authed = false;
        public static bool Authed
        {
            get { return THIS._authed; }
            set
            {
                THIS._authed = value;
                //if (AuthReaction != null) Invoke(AuthReaction); 
                THIS.Invoke(new Action(THIS.primary_auth_rea));
            }
        }
        string Login = "";
        public static string HashPass;
        public static string Pass;

        KCF_AuthForm auth_form;
        //Action AuthReaction;

        bool auth_del_mode = false;
        bool first_authed = false;
        void Auth()
        {
            while (!KCClient.TFK_Recieved) Thread.Sleep(1000);
            long id = KCClient.NextReqID;
            KCReactionSystem.AddResReaction(id, AuthRea);
            //Authed = false;
            AuthQue(id);
        }
        void AuthQue(long ID)
        {
            var com = new LKSTranferCommand(LKSTransferCommands.Auth);
            auth_form = new KCF_AuthForm();
            if (!first_authed)
            {
                auth_form.auto_auth = false;
                auth_form.checkBoxDelProfile.Checked = auth_del_mode;
                auth_form.textBoxlogin.Text = Login;
                var r = auth_form.ShowDialog();
                if (r == DialogResult.Abort) { Invoke(new Action(Close)); return; }
                if (r == DialogResult.Cancel) { return; }
                auth_del_mode = auth_form.checkBoxDelProfile.Checked;

                Login = auth_form.textBoxlogin.Text;
                Pass = auth_form.textBoxpass.Text;
                HashPass = LKSTransferMethods.HASH_MD5(Encoding.UTF8.GetBytes(Pass));

                if (r == DialogResult.Yes) com.Command = LKSTransferCommands.Registry;
                if (auth_del_mode) com.Command = LKSTransferCommands.DeleteProfile;
            }
            else { auth_form.auto_auth = true; auth_form.Show(); }

            com.AddArg("login", Login);
            com.AddArg("pass", HashPass);

            com.Send(ID, KCClient.ClientSocket, KCClient.TransferKey, KCClient.CallBackSendingError);
            //KCQueueReqReactionSystem.AddQueueReq(ID, com);
        }

        void AuthRea(long ID, LKSTranferCommand com/*LKSTransferContainer container*/)
        {
            //var com = (LKSTranferCommandContainer)LKSTransferContainer.ExtractContainer(container);
            if (com.Command == LKSTransferCommands.MsgError)
            {
                KCF_MainForm.ShowError((string)com.Args["msg"]);
                Authed = false;
                first_authed = false;
                AuthQue(ID);
                return;
            }
            if (auth_del_mode)
            {
                KCF_MainForm.ShowInfo((string)com.Args["msg"]);
                Authed = false;
                auth_del_mode = false;
                first_authed = false;
                AuthQue(ID);
                return;
            }
            if (first_authed) auth_form.close_inv(new KCEventArgs());
            Profile = (LKSProfileSendable)com.Args["profile"];
            //Profile.FileCollection.InitMtx();
            Authed = true;
            first_authed = true;
            KCReactionSystem.RemoveReaction(ID);
        }
        #endregion
    }
}

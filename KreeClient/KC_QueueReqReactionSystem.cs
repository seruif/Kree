using KreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace KreeClient
{
    delegate void KCResponseReactionDlgt(long ID, LKSTranferCommand com /*LKSTransferContainer container*/);
    class KCReactionSystem
    {
        //static Queue<LKSTransferContainer> QueueReq = new Queue<LKSTransferContainer>();
        //static Queue<LKSTranferCommandContainer> QueueReq = new Queue<LKSTranferCommandContainer>();
        static Dictionary<long, KCResponseReactionDlgt> ResReactions = new Dictionary<long, KCResponseReactionDlgt>();
        //static Mutex queue_mutex = new Mutex(false);
        //static Mutex reactions_mutex = new Mutex(false);

        //static bool ilde = false;
        //static Thread QueueProcessorThread;
        //static ThreadStart QueueProcessorThreadStart;

        public static void Init()
        {
            //QueueProcessorThreadStart = new ThreadStart(QueueProcessor);
            KCClient.ConnectionLostEvent += Abort;
            //ReThreadQP();
        }

        //private static void ReThreadQP()
        //{
        //    QueueProcessorThread = new Thread(QueueProcessorThreadStart);
        //    QueueProcessorThread.IsBackground = true;
        //}

        //public static void AddQueueReq(long ID, ILKSClassSendable data_class)
        //{
        //    var tc = new LKSTransferContainer(ID, data_class);
        //    queue_mutex.WaitOne();
        //    QueueReq.Enqueue(tc);
        //    queue_mutex.ReleaseMutex();
        //    //ilde = false;
        //    ReThreadQP();
        //    QueueProcessorThread.Start();
        //}

        public static void AddResReaction(long ID, KCResponseReactionDlgt reaction)
        {
            //reactions_mutex.WaitOne();
            lock (ResReactions)
            {
                ResReactions.Add(ID, reaction);
            }
            //reactions_mutex.ReleaseMutex();
        }

        //static bool send_one_req()
        //{
        //    bool res = false;

        //    queue_mutex.WaitOne();
        //    LKSTransferContainer dc = QueueReq.Dequeue();
        //    if (QueueReq.Count > 0) res = true;
        //    queue_mutex.ReleaseMutex();

        //    dc.Send(KCClient.ClientSocket, KCClient.TransferKey);
        //    return res;
        //}

        //static void QueueProcessor()
        //{
        //    while (send_one_req()) ;
        //}

        public static void Recieve(/*LKSTransferContainer container*/LKSTranferCommand container, bool remove)
        {
            //reactions_mutex.WaitOne();
            lock (ResReactions)
            {
                if (ResReactions.ContainsKey(container.ID)) ResReactions[container.ID](container.ID, container);
                if (remove) ResReactions.Remove(container.ID);
            }
            //reactions_mutex.ReleaseMutex();
        }

        public static void RemoveReaction(long ID)
        {
            //reactions_mutex.WaitOne();
            lock (ResReactions)
            {
                if (ResReactions.ContainsKey(ID))
                    ResReactions.Remove(ID);
            }
            //reactions_mutex.ReleaseMutex();
        }

        public static void Abort(KCEventArgs ea)
        {
            //reactions_mutex.WaitOne();
            //queue_mutex.WaitOne();
            //QueueReq.Clear();
            lock (ResReactions)
            {
                ResReactions.Clear();
            }
            //queue_mutex.ReleaseMutex();
            //reactions_mutex.ReleaseMutex();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace KreeLib
{
    public static class LKSJaffaKreeer
    {
        public static Encoding DefaultEncoding = Encoding.UTF8;
        public static byte[] Jaffa_Kree(byte[] data, string key) { return Jaffa_Kree(data, key, DefaultEncoding); }
        public static byte[] Jaffa_DeKree(byte[] data, string key) { return Jaffa_DeKree(data, key, DefaultEncoding); }
        public static byte[] Jaffa_Kree(byte[] data, string key, Encoding en) { return Jaffa_Kree(data, en.GetBytes(key)); }
        public static byte[] Jaffa_Kree(byte[] data, byte[] bkey)//MAIN JAFFA
        {
            //return data;
            int bkl = bkey.Length, dl = data.Length, k;
            for (int i = 0; i < dl; i += bkl)
            {
                for (k = 0; k < bkl && i + k < dl; k++)
                {
                    data[i + k] ^= bkey[k];
                }
            }
            return data;
        }
        public static byte[] Jaffa_DeKree(byte[] data, byte[] bkey) { return Jaffa_Kree(data, bkey); }//Until better times
        public static byte[] Jaffa_DeKree(byte[] data, string key, Encoding en) { return Jaffa_Kree(data, key, en); }//Until better times too
        public static byte[] Jaffa_Kree(string data, string key) { return Jaffa_Kree(data, key, DefaultEncoding); }
        public static byte[] Jaffa_Kree(string data, string key, Encoding en) { return Jaffa_Kree(en.GetBytes(data), key, en); }
        public static string Jaffa_DeKreeToString(byte[] data, string key) { return Jaffa_DeKreeToString(data, key, DefaultEncoding); }
        public static string Jaffa_DeKreeToString(byte[] data, string key, Encoding en) { return en.GetString(Jaffa_DeKree(data, key, en)); }
    }

    //public enum LKSProfileAccess : byte //UNUSED YET
    //{
    //    ServerConfig = 0x02,
    //    UserManage = 0x04
    //}

    //public interface ILKSClassSendable
    //{
    //    //LKSTransferTypes GetTransferType();
    //    int Send(long ID, Socket socket, byte[] transfer_key);
    //}

    [Serializable]
    public class LKSProfileSendable// : ILKSClassSendable
    {
        public string Login;
        //public byte Access = 0;
        public LKSProfileFilesCollection FileCollection = new LKSProfileFilesCollection();
        //public bool CheckAccess(byte access) { return (Access & access) == access; }

        //[NonSerialized]
        //public const LKSTransferTypes TranseType = LKSTransferTypes.Class_LKSProfileSendable;
        //public virtual int Send(long ID, Socket socket, byte[] transfer_key)
        //{
        //    return LKSTransferMethods.Send(ID, this, socket, transfer_key);
        //}
        //public virtual LKSTransferTypes GetTransferType() { return TranseType | LKSTransferTypes.Data; }
    }
    [Serializable]
    public class LKSProfile : LKSProfileSendable
    {
        [NonSerialized]//Service field
        public string FilesDir;

        long _LastFileID = 0;
        public long NextFileID { get { return _LastFileID++; } }

        public LKSProfile(string login, string pass)
        {
            Login = login; Pass = pass;
        }

        public string Pass;

        public string ProfileFilename()
        {
            return ProfileFilename(Login, Pass);
        }

        public string ProfileFilename(string dir)
        {
            return dir + ProfileFilename(Login, Pass);
        }
        public static string Mix(string login, string pass)
        {
            return LKSTransferMethods.HASH_MD5(Encoding.UTF8.GetBytes(login + pass));
        }
        public static string ProfileFilename(string login, string pass)
        {
            return Mix(login, pass) + LKSConsts.KryFileExtension;
        }
        //DUNLICATED FILES                         
        //public Dictionary<string, uint> InnerFileLinks = new Dictionary<string, uint>(); //UNUSED

        public void SaveToFile(string fn, string filekey)
        {
            LKSSerializer.Serial(this, fn, filekey);
        }

        public void Restore()
        {
            //FileCollection.InitMtx();
            FileCollection.RestoreSize();
        }
    }

    public static class LKSSerializer
    {
        //File Kry
        public static void Serial<_type>(_type data_class, string filename, string key)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream buffs = new BufferedStream(fs);
            try
            {
                byte[] data = MemKSerial(data_class, key);
                buffs.Write(data, 0, data.Length);
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                buffs.Close();
                fs.Close();
                buffs.Dispose();
                fs.Dispose();
            }
        }
        public static _type DeSerial<_type>(string filename, string key)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _type data_class;
            try
            {
                byte[] data = new byte[fs.Length];
                int c = fs.Read(data, 0, (int)fs.Length);
                if (c != fs.Length) throw new LKSErrorException("Mismatch readed bytes with length file:\n" + filename);
                data_class = MemKDeSerial<_type>(data, key);
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
            return data_class;
        }
        //Memory Kry
        public static byte[] MemKSerial<_type>(_type data_class, string key)
        {
            return LKSJaffaKreeer.Jaffa_Kree(Serial(data_class), key);
        }
        public static _type MemKDeSerial<_type>(byte[] data, string key)
        {
            return DeSerial<_type>(LKSJaffaKreeer.Jaffa_DeKree(data, key));
        }
        //Memory 
        public static byte[] Serial<_type>(_type data_class)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            try
            {
                bf.Serialize(ms, data_class);
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                ms.Close();
                ms.Dispose();
            }
            return ms.ToArray();
        }
        public static _type DeSerial<_type>(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(data);
            _type data_class;
            try
            {
                data_class = (_type)bf.Deserialize(ms);
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                ms.Close();
                ms.Dispose();
            }
            return data_class;
        }

        //File
        public static void Serial<_type>(_type data_class, string filename)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream buffs = new BufferedStream(fs);
            try
            {
                bf.Serialize(buffs, data_class);
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                buffs.Close();
                fs.Close();
                buffs.Dispose();
                fs.Dispose();
            }
        }

        public static _type DeSerial<_type>(string filename)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _type data_class;
            try
            {
                data_class = (_type)bf.Deserialize(fs);
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
            return data_class;
        }
    }

    public static class LKSConsts
    {
        public const int TransferBufferSize = 1101004;//1.05 Mb
        public const int TransferFileBufferSize = 1048576;//1 Mb
        public const string KryFileExtension = ".kree";          //Jaffa, kree!!
        public const int PingTimeOut = 5000;//milisecs
        public static int PungTimeOutCheck = PingTimeOut * 3;
    }
}

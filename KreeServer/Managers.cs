using KryserLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kryser
{
    public static class XSFileInfoManager
    {
        public static Dictionary<string, LXSFileInfo> Files = new Dictionary<string, LXSFileInfo>();

        public static void Save() { Save(XSServerConfig.DBFileInfo_fn); }
        public static void Save(string filename)
        {
            LXSLog.Log("Saving FileInfo DB..");

            List<LXSFileInfoBase> info_base = (from info in Files select (LXSFileInfoBase)info.Value).ToList();
            File.WriteAllText(filename, JsonConvert.SerializeObject(info_base));
        }
        public static void Load() { Load(XSServerConfig.DBFileInfo_fn); }
        public static void Load(string filename)
        {
            LXSLog.Log("Loading FileInfo DB..");

            List<LXSFileInfoBase> info_base =
                JsonConvert.DeserializeObject<List<LXSFileInfoBase>>(File.ReadAllText(filename));
            if (info_base == null)
            {
                LXSLog.Log("FileInfo DB is empty");
                return;
            }

            LXSLog.Log("Preparing FileInfo DB..");
            foreach (var info in info_base)
            {
                LXSFileInfo inf = (LXSFileInfo)info; inf.UpDateIDInnerName();
                Files.Add(inf.ID, inf);
            }
        }

        public static LXSFullFileInfoProfile GetFullFileInfoProfile(string id, string login)
        {
            var info = new LXSFullFileInfoProfile();
            info.FillFields(Files[id], login);
            info.UpDateIDInnerName();
            return info;
        }

        public static void AddFileInfoProfile(LXSFullFileInfoProfile info,string login)
        {
            if (Files.Keys.Contains(info.ID))
                Files[info.ID].ClientsFileInfo.Add(login, info.ExtractClientFileInfo());
            else
                Files.Add(info.ID, info.GetFileInfo());
        }
    }
}
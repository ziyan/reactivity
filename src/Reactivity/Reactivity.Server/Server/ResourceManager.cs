using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Reactivity.Server
{
    static class ResourceManager
    {
        public static string GetResourceIndex()
        {
            try
            {
                StreamReader reader = new StreamReader(new FileStream(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.ResourceManager.ResourceIndexFilePath"], 
                    FileMode.Open, FileAccess.Read, FileShare.Read), System.Text.Encoding.UTF8);
                string index = reader.ReadToEnd();
                reader.Close();
                return index;
            }
            catch { return null; }
        }
        public static byte[] GetResource(Guid guid)
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.ResourceManager.ResourceCollectionPath"] + @"\" + guid.ToString();
            if (!File.Exists(path)) return null;
            return File.ReadAllBytes(path);
        }
        public static Stream GetResourceStream(Guid guid)
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.ResourceManager.ResourceCollectionPath"] + @"\" + guid.ToString();
            if (!File.Exists(path)) return null;
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}

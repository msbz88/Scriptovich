using System.Collections.Generic;
using System.Xml;

namespace Scriptovich {
    public class Configuration {
        public string PathMasterSCD { get; set; }
        public string PathTestSCD { get; set; }
        public string BatchQueue { get; set; }
        public string BatchDate { get; set; }
        public string BatchServer { get; set; }
        public Dictionary<string, string> ServersToWait = new Dictionary<string, string>();
        private string PathConfigsFile { get; set; }

        public Configuration(string pathConfigs) {
            PathConfigsFile = pathConfigs;
            PathMasterSCD = ReadConfigsFile("PathMasterSCD");
            PathTestSCD = ReadConfigsFile("PathTestSCD");
            BatchQueue = ReadConfigsFile("BatchQueue");
            BatchDate = ReadConfigsFile("BatchDate");
            BatchServer = ReadConfigsFile("BatchServer");
            ServersToWait = ReadConfigsFile();
        }

        private string ReadConfigsFile(string param) {
            XmlDocument doc = new XmlDocument();
            doc.Load(PathConfigsFile);
            return doc.GetElementsByTagName(param)[0].InnerXml;
        }

        private Dictionary<string, string> ReadConfigsFile() {
            XmlDocument doc = new XmlDocument();
            doc.Load(PathConfigsFile);
            Dictionary<string, string> serversToWait = new Dictionary<string, string>();
            XmlNodeList xnList = doc.SelectNodes("/configuration/ScriptovichSettings/ServersToWait/server");
            foreach (XmlNode xn in xnList) {
                serversToWait.Add(xn.InnerText, xn.Attributes["type"].Value);
            }           
            return serversToWait;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Scriptovich {
    public class Configuration {
        public string PathMasterSCD { get; set; }
        public string PathTestSCD { get; set; }
        public string BatchQueue { get; set; }
        public string BatchDate { get; set; }
        public string BatchServer { get; set; }
        private string PathConfigsFile { get; set; }

        public Configuration(string pathConfigs) {
            PathConfigsFile = pathConfigs;
            PathMasterSCD = ReadConfigsFile("PathMasterSCD");
            PathTestSCD = ReadConfigsFile("PathTestSCD");
            BatchQueue = ReadConfigsFile("BatchQueue");
            BatchDate = ReadConfigsFile("BatchDate");
            BatchServer = ReadConfigsFile("BatchServer");      
        }

        private string ReadConfigsFile(string param) {
            XmlDocument doc = new XmlDocument();
            doc.Load(PathConfigsFile);
            return doc.GetElementsByTagName(param)[0].InnerXml;
        }
    }
}

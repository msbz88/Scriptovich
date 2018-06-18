using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace Scriptovich {
    public class Log {
        public string FileName { get; set; }
        private ReaderWriterLock Rwl = new ReaderWriterLock();
        private int WriterTimeouts = 0;

        public Log(string fileName) {
            FileName = fileName;
        }

        public void Write(int level, string message) {
            try {
                Rwl.AcquireWriterLock(1000);
                try {
                    string border = "------------------------------------------------------------";
                    string timestamp = DateTime.Now.ToString("dd'-'MM'-'yyyy HH:mm:ss");
                    if (level == 1) {
                        File.AppendAllText(FileName, Environment.NewLine + border);
                        File.AppendAllText(FileName, Environment.NewLine + "[" + timestamp + "]" + "\t" + message);
                    } else if (level == 2) {
                        File.AppendAllText(FileName, Environment.NewLine + "[" + timestamp + "]" + "\t" + message);
                    } else if (level == 3) {
                        File.AppendAllText(FileName, Environment.NewLine + "[" + timestamp + "]" + "\t" + "  " + message);
                    } else if (level == 4) {
                        File.AppendAllText(FileName, Environment.NewLine + "[" + timestamp + "]" + "\t" + " +" + message);
                    } else if (level == 5) {
                        File.AppendAllText(FileName, Environment.NewLine + "[" + timestamp + "]" + "\t" + " -" + message);
                    }
                }
                finally {
                    Rwl.ReleaseWriterLock();
                }
            } catch (ApplicationException) {
                Interlocked.Increment(ref WriterTimeouts);
            }
        }
    }
}

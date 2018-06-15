using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Scriptovich {
    public class Server {
        public string ServerName { get; set; }
        public string ServerType { get; set; }
        public Log Log { get; set; }

        public Server(string serverName, string serverType, Log log) {
            ServerName = serverName;
            ServerType = serverType;
            Log = log;
        }

        public int GetServerStatus(string pathSCD) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + pathSCD + " -nolocalcheck -type=*" + " -server=" + ServerName + " -wait -statusex";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        public void Wait(string pathSCD, string versionSCD) {
            int serverStatus = 99999;
            while (true) {
                serverStatus = GetServerStatus(pathSCD);
                if (serverStatus == 30) {
                    break;
                } else {
                    Log.Write(2, "Waiting for " + ServerType + " server in " + versionSCD + "...");
                    Log.Write(3, "[Error Level " + serverStatus + "]");
                    Console.WriteLine();
                    Console.WriteLine("Waiting for " + ServerType + " server in " + versionSCD + "...");
                    Console.WriteLine("[Error Level " + serverStatus + "]");
                    Thread.Sleep(30000);
                }
            }
        }
    }
}

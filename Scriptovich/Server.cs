using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
                    Log.Write(2, "Waiting for " + ServerType + " server ["+ ServerName + "] in " + versionSCD + "...");
                    Log.Write(3, "[Exit Code: " + serverStatus + " Description: " + ExitCodeDescription.ServerExitCodeDescript(serverStatus) + "]");
                    Console.WriteLine();
                    Console.WriteLine("Waiting for " + ServerType + " server [" + ServerName + "] in " + versionSCD + "...");
                    Console.WriteLine("[Exit Code: " + serverStatus + " Description: " + ExitCodeDescription.ServerExitCodeDescript(serverStatus) + "]");
                    Thread.Sleep(30000);
                }
            }
        }
    }
}

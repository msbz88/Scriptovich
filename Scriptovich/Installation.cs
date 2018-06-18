using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Scriptovich {
    public class Installation {
        public string PathSCD { get; set; }
        public string VersionSCD { get; set; }
        public string BatchQueue { get; set; }
        public string BatchDate { get; set; }
        public string BatchServer { get; set; }
        public List<Server> ServersToWait { get; set; }
        public int BatchJobGrpStatus { get; set; }
        public Log Log { get; set; }

        public Installation(string pathSCD, string versionSCD, string batchQueue, string batchDate, string batchServer, List<Server> serversToWait, Log log) {
            PathSCD = pathSCD;
            VersionSCD = versionSCD;
            BatchQueue = batchQueue;
            BatchDate = batchDate;
            BatchServer = batchServer;
            ServersToWait = serversToWait;
            Log = log;
        }

        public int ExecuteBatchJobGrp(string batchJobGrp) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + PathSCD + " -nolocalcheck -type=* -batch=" + batchJobGrp + " -ba_queue=" + BatchQueue + " -ba_date=" + BatchDate + " -server=" + BatchServer + "-wait -statusex";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        public void CheckBatchJobGrpStatus(string batchJobGrp, int currJobNo, int allJobsCount) {
            while (true) {
                if (BatchJobGrpStatus != 0) {
                    int userAnswer = AskUserOnFail(batchJobGrp);
                    if (userAnswer == 1) {
                        Console.WriteLine();
                        Log.Write(2, "User has decided to restart Failed BJG.");
                        Log.Write(2, "Restarting " + currJobNo + " of " + allJobsCount + " in " + VersionSCD);
                        Console.WriteLine("Restarting " + currJobNo + " of " + allJobsCount + " in " + VersionSCD);
                        RunBatchJobGrpTask(batchJobGrp);
                    } else if (userAnswer == 2) {
                        Log.Write(2, "User has decided to skip action and go to next BJG.");
                        break;
                    }
                } else {
                    Log.Write(4, batchJobGrp + " successfully executed in " + VersionSCD);
                    Console.WriteLine(" +" + batchJobGrp + " successfully executed in " + VersionSCD);
                    break;
                }
            }
        }

        public int AskUserOnFail(string batchJobGrp) {
            int action = 0;
            Log.Write(5, batchJobGrp + " was Failed in " + VersionSCD);
            Console.WriteLine(" -" + batchJobGrp + " was Failed in " + VersionSCD);
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Please select next action (index number):");
            Console.WriteLine("  1.Restart " + batchJobGrp + " in " + VersionSCD);
            Console.WriteLine("  2.Skip action and go to next BJG");
            while (true) {
                string userInput = Console.ReadLine();
                try {
                    action = int.Parse(userInput);
                    if (action == 0 || action > 2) {
                        throw new Exception();
                    } else { break; }
                } catch (Exception) {
                    Console.WriteLine();
                    Console.WriteLine("Please specify only the index number of action from list!");
                }
            }
            return action;
        }

        public void RunBatchJobGrpTask(string batchJobGrp) {
            foreach (Server server in ServersToWait) {
                server.Wait(PathSCD, VersionSCD);
            }
            Log.Write(3, "Executing " + batchJobGrp + " in " + VersionSCD + "...");
            Console.WriteLine("  Executing " + batchJobGrp + " in " + VersionSCD + "...");
            BatchJobGrpStatus = ExecuteBatchJobGrp(batchJobGrp);
        }
    }

}

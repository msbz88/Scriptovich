using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Scriptovich {
    class Program {
        static Configuration Configs { get; set; }
        static Log Log { get; set; }
        static string CurrentBatchJobGrp { get; set; }
        static int CurrentJobPosition { get; set; }

        public static List<string> ReadFileWithBatchJobGrps(string pathFile) {
            return File.ReadAllLines(pathFile).ToList();
        }

        static void StopForJobVerification(string batchJobGrp) {
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Execution stopped, because next BJG [" + batchJobGrp + "] was marked as [needs verification]");
            Console.WriteLine("Please press [1] when BJG ready and execution will be continued");
            Log.Write(2, "Execution stopped for BJG verification");
            while (true) {
                string userInput = Console.ReadLine();
                if (userInput == "1") {
                    Console.WriteLine();
                    Console.WriteLine("OK, continue execution...");
                    Log.Write(2, "Script returned to execution");
                    return;
                } else {
                    Console.WriteLine("Oops, you've typed something wrong.");
                    Console.WriteLine("If you want to continue execution, please press [1]");
                }
            }
        }

        static void SavePreviouslyEndedBJGInfo(object sender, ConsoleCancelEventArgs e) {
            string cacheFile = Directory.GetCurrentDirectory() + @"\Scriptovich_cache.txt";
            File.WriteAllText(cacheFile, CurrentBatchJobGrp + ";" + CurrentJobPosition);
        }

        static void VerifyServerListForWaiting(List<Server> serversToWait) {
            if (!serversToWait.Any()) {
                Console.WriteLine("In cofig file no servers found for waiting");
                Log.Write(2, "In cofig file no servers found for waiting");
                Console.WriteLine("If you want to continue execution, please press [1]");
                while (true) {
                    string userInput = Console.ReadLine();
                    if (userInput == "1") {
                        Console.WriteLine();
                        Console.WriteLine("OK, continue execution...");
                        Log.Write(2, "Script returned to execution");
                        return;
                    } else {
                        Console.WriteLine("Oops, you've typed something wrong.");
                        Console.WriteLine("If you want to continue execution, please press [1]");
                    }
                }
            }
        }

        static void Main(string[] args) {
            Console.CancelKeyPress += SavePreviouslyEndedBJGInfo;
            string logFile = "";
            try {
                string logDir = Directory.GetCurrentDirectory() + @"\Log";
                if (!Directory.Exists(logDir)) {
                    Directory.CreateDirectory(logDir);
                }
                logFile = logDir + @"\Scriptovich_log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                Log = new Log(logFile);
            } catch (Exception) { Console.WriteLine("Cannot create log file. Execution will continue without logging!"); }
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            string pathConfigs = Directory.GetCurrentDirectory() + @"\Scriptovich_configs.xml";
            List<BatchJobGrp> batchJobGrps = new List<BatchJobGrp>();
            bool errors = false;
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            try {
                Configs = new Configuration(pathConfigs);
                string pathBatchJobGrps = Directory.GetCurrentDirectory() + @"\BJG_to_run.txt";
                char delimiter = ';';
                try {
                    foreach (string item in ReadFileWithBatchJobGrps(pathBatchJobGrps)) {
                        string name = item.Split(delimiter)[0];
                        int verification = int.Parse(item.Split(delimiter)[1]);
                        BatchJobGrp batchJobGrp = new BatchJobGrp(name, verification);
                        batchJobGrps.Add(batchJobGrp);
                    }
                } catch (Exception) {
                    Console.WriteLine("Input file: " + pathBatchJobGrps + "\nis missed or incorrect! Please check if the file exist and whether the file has only two columns separated by " + "[" + delimiter + "]");
                    Console.WriteLine("Also put the BJG name in first column and in second the marker [1/0] (where to stop execution for verification)");
                    Log.Write(2, "Input file: " + pathBatchJobGrps + " with BJGs is incorrect");
                }
            } catch (Exception) {
                Console.WriteLine("The configuration file: " + pathConfigs + "\nis missed or not properly set!");
                Log.Write(2, "The configuration file: " + pathConfigs + " is missed or not properly set!");
                errors = true;
            }
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            List<Server> serversToWait = new List<Server>();
            if (!errors) {
                foreach (var server in Configs.ServersToWait) {
                    Server srv = new Server(server.Key, server.Value, Log);
                    serversToWait.Add(srv);
                }
                VerifyServerListForWaiting(serversToWait);
            }
            
            //---------------------------------------------------------------------------------------------------------------------------------------------------------

            if (batchJobGrps.Count > 0 && !errors) {
                Installation masterSCD = new Installation(Configs.PathMasterSCD, "Master", Configs.BatchQueue, Configs.BatchDate, Configs.BatchServer, serversToWait, Log);
                Installation testSCD = new Installation(Configs.PathTestSCD, "Test", Configs.BatchQueue, Configs.BatchDate, Configs.BatchServer, serversToWait, Log);
                Console.WriteLine("Execution started");
                Log.Write(1, "Execution started");
                Console.WriteLine("Do NOT close this window till the process will end");
                Console.WriteLine("Please see log file for execution details:");
                Console.WriteLine(Log.FileName);
                Console.WriteLine("------------------------------------------------------------");
                int jobCount = 0;
                int allJobs = batchJobGrps.Count;

                foreach (BatchJobGrp batchJobGrp in batchJobGrps) {
                    CurrentBatchJobGrp = batchJobGrp.Name;
                    jobCount++;
                    CurrentJobPosition = jobCount;
                    if (batchJobGrp.Verification == 1) {
                        StopForJobVerification(batchJobGrp.Name);
                    }
                    //Run Test
                    Log.Write(1, "Starting " + jobCount + " of " + allJobs + " in Test");
                    Console.WriteLine();
                    Console.WriteLine("Starting " + jobCount + " of " + allJobs + " in Test");
                    Task runTest = new Task(() => testSCD.RunBatchJobGrpTask(batchJobGrp.Name));
                    runTest.Start();
                    //Run Master
                    Log.Write(2, "Starting " + jobCount + " of " + allJobs + " in Master");
                    Console.WriteLine("Starting " + jobCount + " of " + allJobs + " in Master");
                    Task runMaster = new Task(() => masterSCD.RunBatchJobGrpTask(batchJobGrp.Name));
                    runMaster.Start();

                    Task.WaitAll(runTest, runMaster);
                    //check Test result
                    testSCD.CheckBatchJobGrpStatus(batchJobGrp.Name, jobCount, allJobs);
                    //check Master result
                    masterSCD.CheckBatchJobGrpStatus(batchJobGrp.Name, jobCount, allJobs);
                }
            }
            Log.Write(1, "Script ended");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Script ended");
            Console.ReadKey();
        }
    }
}

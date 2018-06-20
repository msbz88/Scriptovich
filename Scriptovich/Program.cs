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

        static bool FileEquals(string path1, string path2) {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length) {
                for (int i = 0; i < file1.Length; i++) {
                    if (file1[i] != file2[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

    static List<string> FindBJGToStartWith(List<string> batchJobGrps) {
            Console.WriteLine("If you want to start execution from the beginning of BJG list - press [1]");
            Console.WriteLine("Otherwise please type the line number of BJG from which you want to start (use order from file BJG_to_run.txt)");
            int batchJobGrpLine = 0;
            while (true) {
                string userInput = Console.ReadLine();
                try {
                    batchJobGrpLine = int.Parse(userInput);
                    if (batchJobGrpLine > batchJobGrps.Count || batchJobGrpLine < 1) {
                        Console.WriteLine("There are only " + batchJobGrps.Count + " BJGs to run. Please type number from 1 to " + batchJobGrps.Count);
                        continue;
                    } else {
                        break;
                    }
                } catch (Exception) {
                    Console.WriteLine("Oops, you've typed something wrong.");
                    Console.WriteLine("If you want to start execution from the beginning of BJG list - press [1]");
                    Console.WriteLine("Otherwise please type the line number of BJG from which you want to start (use order from file BJG_to_run.txt)");
                }
            }
            if (batchJobGrpLine == 1) {
                Console.WriteLine();
                Console.WriteLine("OK, Starting from the beginning...");
                Log.Write(1, "User decided to start execution from the beginning");
                return batchJobGrps;
            } else {
                Console.WriteLine("On line " + batchJobGrpLine + " is BJG [" + batchJobGrps[batchJobGrpLine-1] + "]");
                Console.WriteLine("If you want to continue press [1]");
                while (true) {
                    string userInput = Console.ReadLine();
                    if (userInput == "1") {
                        Console.WriteLine();
                        Console.WriteLine("OK, starting from line " + batchJobGrpLine + ", BJG [" + batchJobGrps[batchJobGrpLine-1]+ "]...");
                        Console.WriteLine();
                        break;
                    } else {
                        Console.WriteLine("Oops, you've typed something wrong.");
                        Console.WriteLine("If you want to continue execution, please press [1]");
                    }
                }
                Log.Write(1, "User decided to start from line " + batchJobGrpLine + ", BJG [" + batchJobGrps[batchJobGrpLine-1] + "]");            
                return batchJobGrps.GetRange(batchJobGrpLine-1, batchJobGrps.Count- (batchJobGrpLine-1));
            }
        }

        static void WaitForManualCorrection() {
            Console.WriteLine("Manual correction is needed");
            Log.Write(2, "Manual correction is needed");
            Console.WriteLine("Please press [1] when BJG ready and execution will be continued");
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

        static void StopForJobVerification(BatchJobGrp batchJobGrp) {
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Execution stopped, because next BJG [" + batchJobGrp.Name + "] was marked as [needs verification]");      
            Log.Write(2, "Execution stopped for BJG verification");
            string pathMasterImpFile = batchJobGrp.OutFilePath;
            string pathTestImpFile = batchJobGrp.OutFilePath.Replace('2', '5').Replace('7', '8');
            int pathError = 0;

            if (!File.Exists(pathMasterImpFile)) {
                Console.WriteLine("Cannot find Master import file");
                Log.Write(2, "Master import file was not found");
                pathError = 1;
            }
            if (!File.Exists(pathTestImpFile)) {
                Console.WriteLine("Cannot find Test import file");
                Log.Write(2, "Test import file was not found");
                pathError = 1;
            }
            if(pathError == 1) {
                WaitForManualCorrection();
            } else {
                Console.WriteLine("Attempt to match files automatically...");
                Log.Write(2, "Attempt to match import files automatically...");
                if (FileEquals(pathMasterImpFile, pathTestImpFile)) {
                    Console.WriteLine("  Files are equal");
                    Log.Write(3, "Files are equal");
                    Console.WriteLine();
                    Console.WriteLine("OK, continue execution...");
                    Log.Write(2, "Script returned to execution");
                    return;
                } else {
                    Console.WriteLine("  Files are different");
                    Log.Write(3, "Files are different");
                    WaitForManualCorrection();
                }
            }       
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
            ExitCodeDescription.InitializeExitCodesDict();
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
                    List<string> batchJobGrpFile = ReadFileWithBatchJobGrps(pathBatchJobGrps);
                    List<string> batchJobGrpToRun = FindBJGToStartWith(batchJobGrpFile);

                    foreach (string item in batchJobGrpToRun) {
                        string name = item.Split(delimiter)[1];
                        int verification = int.Parse(item.Split(delimiter)[2]);
                        string outFilePath = item.Split(delimiter)[3];
                        BatchJobGrp batchJobGrp = new BatchJobGrp(name, verification, outFilePath);
                        batchJobGrps.Add(batchJobGrp);
                    }
                } catch (Exception) {
                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine("Input file: " + pathBatchJobGrps + "\nis missed or incorrect! Please check if the file exists and whether the file has four columns separated by " + "[" + delimiter + "]");
                    Console.WriteLine("Example of input file:");
                    Console.WriteLine("First column the is line number");
                    Console.WriteLine("Second column is BJG name");
                    Console.WriteLine("Third column is the marker [1/0] where to stop execution for BG verification");
                    Console.WriteLine("Fourth column is path to import file from Master SCD (will be used for automatic match)");
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
                Console.WriteLine("------------------------------------------------------------");
                Console.WriteLine("Execution started");
                Log.Write(2, "Execution started");
                Console.WriteLine("Do NOT close this window till the process will end");
                Console.WriteLine("Please see log file for execution details:");
                Console.WriteLine(Log.FileName);            
                int jobCount = 0;
                int allJobs = batchJobGrps.Count;

                foreach (BatchJobGrp batchJobGrp in batchJobGrps) {
                    CurrentBatchJobGrp = batchJobGrp.Name;
                    jobCount++;
                    CurrentJobPosition = jobCount;
                    if (batchJobGrp.Verification == 1) {
                        StopForJobVerification(batchJobGrp);
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

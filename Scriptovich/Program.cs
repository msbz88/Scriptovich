using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scriptovich {
    class Program {
        static Configuration Configs { get; set; }
        static Log Log { get; set; }
        static string CurrentBatchJobGrp { get; set; }
        static int CurrentJobPosition { get; set; }
        static int BatchJobGrpLine { get; set; } = 0;
        static int AllJobs { get; set; }
        static int CompareOption { get; set; }

        public static List<string> ReadFileWithBatchJobGrps(string pathFile) {
            return File.ReadAllLines(pathFile).ToList();
        }

        static bool FileEquals(string path1, string path2) {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length) {
                if (CompareOption == 2) {
                    for (int i = 0; i < file1.Length; i++) {
                        if (file1[i] != file2[i]) {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        static string ConvertFromUnixTimestamp(double timestamp) {
            TimeSpan timespan = TimeSpan.FromMinutes(timestamp);
            return timespan.ToString(@"hh\:mm\:ss");
        }

        static void HandleComparisonOptions(List<BatchJobGrp> batchJobGrps) {
            bool isFileCheckNeeded = batchJobGrps.Any(b => b.OutFilePath != "");
            if (isFileCheckNeeded) {
                Console.WriteLine();
                Console.WriteLine("Choose the comparison option for provided import files:");
                Console.WriteLine("  1.Basic compare (files will be compared by rows count)");
                Console.WriteLine("  2.Full compare (byte to byte comparison)");
                while (true) {
                    string userInput = Console.ReadLine();
                    try {
                        CompareOption = int.Parse(userInput);
                        if (CompareOption != 1 && CompareOption != 2) {
                            throw new Exception();
                        }
                        string CompareOptionNameForLog = "";
                        if (CompareOption == 1) { CompareOptionNameForLog = "[Basic compare]"; }
                        else if (CompareOption == 2) { CompareOptionNameForLog = "[Full compare]"; }
                        Log.Write(2, "User select " + CompareOptionNameForLog + " option for provided import files");
                        break;
                    } catch (Exception) {
                        Console.WriteLine("Oops, you've typed something wrong.");
                        Console.WriteLine("Please select between available options");
                    }
                }
            } else {
                Log.Write(2, "No import files to compare");
            }
        }

        static List<string> FindBJGToStartWith(List<string> batchJobGrps, char delimiter) {
            Console.WriteLine("If you want to start execution from the beginning of BJG list - press [1]");
            Console.WriteLine("Otherwise please type the line number of BJG from which you want to start");
            Console.WriteLine("You could use order number from file BJG_to_run.txt (first column)");

            while (true) {
                string userInput = Console.ReadLine();
                try {
                    BatchJobGrpLine = int.Parse(userInput);
                    if (BatchJobGrpLine > batchJobGrps.Count || BatchJobGrpLine < 1) {
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
            if (BatchJobGrpLine == 1) {
                Console.WriteLine();
                Console.WriteLine("OK, Starting from the beginning...");
                Log.Write(1, "User decided to start execution from the beginning");
                return batchJobGrps;
            } else {
                string batchJobGrpName = batchJobGrps[BatchJobGrpLine - 1].Split(delimiter)[1];
                Console.WriteLine("On line " + BatchJobGrpLine + " is BJG [" + batchJobGrpName + "]");
                Console.WriteLine("If you want to continue press [1]");
                while (true) {
                    string userInput = Console.ReadLine();
                    if (userInput == "1") {
                        Console.WriteLine();
                        Console.WriteLine("OK, starting from BJG [" + batchJobGrpName + "]...");
                        Console.WriteLine();
                        break;
                    } else {
                        Console.WriteLine("Oops, you've typed something wrong.");
                        Console.WriteLine("If you want to continue execution, please press [1]");
                    }
                }
                Log.Write(1, "User decided to start from line " + BatchJobGrpLine + ", BJG [" + batchJobGrpName + "]");
                return batchJobGrps.GetRange(BatchJobGrpLine - 1, batchJobGrps.Count - (BatchJobGrpLine - 1));
            }
        }

        static void WaitForManualCorrection() {
            Console.WriteLine();
            Console.WriteLine("Manual correction is needed");
            Log.Write(2, "Manual correction is needed");
            Console.WriteLine("Please press [1] when BJG ready and execution will be continued");
            while (true) {
                string userInput = Console.ReadLine();
                if (userInput == "1") {
                    Console.WriteLine();
                    Console.WriteLine("OK, continue execution...");
                    Log.Write(2, "Script manually returned to execution");
                    return;
                } else {
                    Console.WriteLine("Oops, you've typed something wrong.");
                    Console.WriteLine("If you want to continue execution, please press [1]");
                }
            }
        }

        static void StopForJobVerification(BatchJobGrp batchJobGrp) {
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Execution stopped");
            Console.WriteLine("Next BJG [" + batchJobGrp.Name + "] was marked as [needs verification]");
            Log.Write(1, "Execution stopped for BJG verification");
            Log.Write(2, "Next BJG [" + batchJobGrp.Name + "] was marked as [needs verification]");
            string pathMasterImpFile = batchJobGrp.OutFilePath;
            string pathTestImpFile = batchJobGrp.OutFilePath.Replace("apsam26", "apsam58").Replace("UAT7", "UAT8");
            bool pathError = false;

            if (!File.Exists(pathMasterImpFile)) {
                Console.WriteLine();
                Console.WriteLine("Cannot find Master import file for automatic comparison");
                Log.Write(2, "Master import file was not found");
                pathError = true;
            }
            if (!File.Exists(pathTestImpFile)) {
                Console.WriteLine();
                Console.WriteLine("Cannot find Test import file for automatic comparison");
                Log.Write(2, "Test import file was not found");
                pathError = true;
            }
            if (pathError) {
                WaitForManualCorrection();
            } else {
                Console.WriteLine("Attempt to match files automatically...");
                Log.Write(2, "Attempt to match import files automatically...");
                if (FileEquals(pathMasterImpFile, pathTestImpFile)) {
                    Console.WriteLine(" +Files are equal");
                    Log.Write(4, "Files are equal");
                    Console.WriteLine();
                    Console.WriteLine("OK, continue execution...");
                    Log.Write(2, "Script automatically returned to execution");
                    return;
                } else {
                    Console.WriteLine(" -Files are different");
                    Log.Write(5, "Files are different");
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
                        Log.Write(2, "Script manually returned to execution");
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
                    AllJobs = batchJobGrpFile.Count;
                    List<string> batchJobGrpToRun = FindBJGToStartWith(batchJobGrpFile, delimiter);

                    foreach (string item in batchJobGrpToRun) {
                        string name = item.Split(delimiter)[1];
                        int verification = int.Parse(item.Split(delimiter)[2]);
                        string outFilePath = item.Split(delimiter)[3];
                        BatchJobGrp batchJobGrp = new BatchJobGrp(name, verification, outFilePath);
                        batchJobGrps.Add(batchJobGrp);
                    }
                    HandleComparisonOptions(batchJobGrps);
                } catch (Exception) {
                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine("Input file: " + pathBatchJobGrps + "\nis missed or incorrect! Please check if the file exists and whether the file has four columns separated by " + "[" + delimiter + "]");
                    Console.WriteLine("Example of input file:");
                    Console.WriteLine("First column is the line number");
                    Console.WriteLine("Second column is BJG name");
                    Console.WriteLine("Third column is the marker [1/0] where to stop execution for BJG verification");
                    Console.WriteLine("Fourth column is path to import file from Master SCD (will be used for automatic check)");
                    Log.Write(2, "Input file: " + pathBatchJobGrps + " with BJGs is incorrect");
                    errors = true;
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
                Console.WriteLine("The batch date = " + Configs.BatchDate);
                Console.WriteLine();
                Log.Write(2, "Execution started");                         
                Log.Write(2, "The batch date = " + Configs.BatchDate);
                Console.WriteLine("Do NOT close this window till the process will end");
                Console.WriteLine("Please see log file for execution details:");
                Console.WriteLine(Log.FileName);

                DateTime globalStartTime = DateTime.Now;
                DateTime elapsedTime = globalStartTime;
                TimeSpan timeFromStart = new TimeSpan();
                int completedBJG = 0;

                foreach (BatchJobGrp batchJobGrp in batchJobGrps) {
                    CurrentBatchJobGrp = batchJobGrp.Name;
                    CurrentJobPosition = BatchJobGrpLine;

                    if (batchJobGrp.Verification == 1) {
                        StopForJobVerification(batchJobGrp);
                    }

                    DateTime jobStartTime = DateTime.Now;

                    //Run Test
                    Log.Write(1, "Starting " + BatchJobGrpLine + " of " + AllJobs + " in Test");
                    Console.WriteLine();
                    Console.WriteLine("Starting " + BatchJobGrpLine + " of " + AllJobs + " in Test");
                    Task runTest = new Task(() => testSCD.RunBatchJobGrpTask(batchJobGrp.Name));
                    runTest.Start();
                    //Run Master
                    Log.Write(2, "Starting " + BatchJobGrpLine + " of " + AllJobs + " in Master");
                    Console.WriteLine("Starting " + BatchJobGrpLine + " of " + AllJobs + " in Master");
                    Task runMaster = new Task(() => masterSCD.RunBatchJobGrpTask(batchJobGrp.Name));
                    runMaster.Start();

                    Task.WaitAll(runTest, runMaster);

                    completedBJG++;
                    TimeSpan jobEndTime = DateTime.Now.Subtract(jobStartTime);
                    elapsedTime += jobEndTime;
                    timeFromStart = elapsedTime.Subtract(globalStartTime);

                    //check Test result
                    testSCD.CheckBatchJobGrpStatus(batchJobGrp.Name, BatchJobGrpLine, AllJobs);
                    //check Master result
                    masterSCD.CheckBatchJobGrpStatus(batchJobGrp.Name, BatchJobGrpLine, AllJobs);                   
                  
                    double avgForOne = timeFromStart.TotalMinutes / completedBJG;
                    double estForAll = avgForOne * (AllJobs - BatchJobGrpLine);
                    string estEnd = DateTime.Now.AddMinutes(estForAll).ToString("dd'-'MM'-'yyyy HH:mm:ss");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine();
                    Console.WriteLine("  Avg. time for 1 BJG: [" + ConvertFromUnixTimestamp(avgForOne) + "]");
                    Console.WriteLine("  Est. remaining time: [" + ConvertFromUnixTimestamp(estForAll) + "]; Est.ends: [" + estEnd + "]");
                    Console.ResetColor();

                    //wait for STP to start due to polling 1 min
                    if (batchJobGrp.Name.Contains("STP")) {
                        Thread.Sleep(70000);
                    }
                    BatchJobGrpLine++;
                }
            }
            Log.Write(1, "Script ended");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Script ended");
            Console.ReadKey();
        }
    }
}

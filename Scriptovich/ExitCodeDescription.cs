using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scriptovich {
    public class ExitCodeDescription {
        private static Dictionary<int, string> ExitCodes { get; set; }

        public static string ServerExitCodeDescript(int errorLevel) {
            try {
                return ExitCodes.First(ec => ec.Key == errorLevel).Value;
            } catch (Exception) {
                return "Unknown Exit Code";
            }
        }

        public static void InitializeExitCodesDict() {
            ExitCodes = new Dictionary<int, string>();
            ExitCodes.Add(0, "OK");
            ExitCodes.Add(1, "Forced logoff");
            ExitCodes.Add(2, "Abnormal logoff (the APL process has disappeared)");
            ExitCodes.Add(3, "Restart in progress");
            ExitCodes.Add(4, "Internal scd.exe program error or Windows environment");
            ExitCodes.Add(5, "SimCorp Dimension already started");
            ExitCodes.Add(6, "Password of the week failed");
            ExitCodes.Add(7, "Interval logoff");
            ExitCodes.Add(8, "Waiting for another SimCorp Dimension timed out");
            ExitCodes.Add(9, "MUCS installed");
            ExitCodes.Add(10, "A SimCorp Dimension is running under another login session ID");
            ExitCodes.Add(11, "A connection or communication to MUCS server failed");
            ExitCodes.Add(12, "Failed before start of APL");
            ExitCodes.Add(13, "Ping timeout");
            ExitCodes.Add(14, "Error in parameter");
            ExitCodes.Add(15, "Local installation is made");
            ExitCodes.Add(16, "Trying to start local installation when central installation exists");
            ExitCodes.Add(17, "Windows is shutting down or Windows user logging off (S)(E)");
            ExitCodes.Add(18, "APL interpreter has terminated with syserror 999");
            ExitCodes.Add(19, "Attempt to write to APL session");
            ExitCodes.Add(20, "The SCUxxxx.exe process has disappeared");
            ExitCodes.Add(21, ".NET framework not properly installed");
            ExitCodes.Add(22, "Insufficient memory");
            ExitCodes.Add(23, "Captured unknown dialog box");
            ExitCodes.Add(24, "Could not add to batch queue");
            ExitCodes.Add(25, "Could not open SCD.LOG for write (only applicable for SimCorp Dimension servers and batch jobs)");
            ExitCodes.Add(26, "Executing of setup.exe failed");
            ExitCodes.Add(30, "Server status: Idle");
            ExitCodes.Add(31, "Server status: Execute");
            ExitCodes.Add(32, "Server status: Ended");
            ExitCodes.Add(33, "Server status: Unknown to MUCS");
            ExitCodes.Add(34, "Server status: Undefined");
            ExitCodes.Add(35, "Server status: No response");
            ExitCodes.Add(36, "Server status: Not running");
            ExitCodes.Add(99, "Miscellaneous R(S)");
            ExitCodes.Add(128, "Unable to create process. See Windows knowledge Base ID 184802 for further information");
            ExitCodes.Add(259, "Main process started correct");
            ExitCodes.Add(1000, "Down for maintenance");
            ExitCodes.Add(1001, "Automatic logoff");
            ExitCodes.Add(1002, "Shutdown");
            ExitCodes.Add(1003, "Connection to database lost");
            ExitCodes.Add(1004, "Database error");
            ExitCodes.Add(1005, "Program error");
            ExitCodes.Add(1006, "Unable to initialise");
            ExitCodes.Add(1010, "Unknown user");
            ExitCodes.Add(1011, "Wrong password");
            ExitCodes.Add(1012, "User suspended");
            ExitCodes.Add(1013, "Logon error");
            ExitCodes.Add(1014, "Password expired");
            ExitCodes.Add(1015, "Database authorisation error");
            ExitCodes.Add(1016, "Database connect error");
            ExitCodes.Add(1017, "Database not connected");
            ExitCodes.Add(1018, "User locked");
            ExitCodes.Add(1020, "Attempt to run ‘init’ in a local installation");
            ExitCodes.Add(1050, "Setup error");
            ExitCodes.Add(1051, "Data error");
            ExitCodes.Add(1052, "Unknown server");
            ExitCodes.Add(1053, "Unknown batch job group");
            ExitCodes.Add(1054, "Authorisation error");
            ExitCodes.Add(1055, "Job execution on server failed");
            ExitCodes.Add(1056, "Job is already running");
            ExitCodes.Add(1057, "Batch job cannot be started with option ba_continue");
            ExitCodes.Add(1058, "Batch job cannot be started with option ba_failedjobsonly");
            ExitCodes.Add(1059, "Database identification do not match the one registered");
            ExitCodes.Add(1060, "File handle error");
            ExitCodes.Add(1061, "Command line error");
            ExitCodes.Add(1099, "Miscellaneous");
        }
    }
}

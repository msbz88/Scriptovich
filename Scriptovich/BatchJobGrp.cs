using System;

namespace Scriptovich {
    public class BatchJobGrp {
        public string Name { get;set;}
        private int verification;
        public int Verification {
            get {
                return verification;
            }
            set {
                if (value > 1 || value < 0) {
                    throw new Exception();
                } else { verification = value; }
            }
        }
        public string OutFilePath { get; set; } = "";

        public BatchJobGrp(string name, int verification, string outFilePath) {
            Name = name;
            Verification = verification;
            OutFilePath = outFilePath;
        }
    }
}

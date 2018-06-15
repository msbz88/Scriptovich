using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public BatchJobGrp(string name, int verification) {
            Name = name;
            Verification = verification;
        }
    }
}

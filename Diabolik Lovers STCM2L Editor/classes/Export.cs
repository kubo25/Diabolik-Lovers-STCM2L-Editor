using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class Export {
        public char[] Name { get; set; }
        public int Address { get; set; }
        public int OldAddress { get; set; }
        
        public Export() {
            Name = new char[32];
        }
    }
}

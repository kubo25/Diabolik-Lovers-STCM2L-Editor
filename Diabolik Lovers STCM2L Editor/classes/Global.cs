using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    static class Global {
        public static Dictionary<UInt32, List<Parameter>> Calls { get; set; }

        static Global () {
            Calls = new Dictionary<uint, List<Parameter>>();
        }
    }
}

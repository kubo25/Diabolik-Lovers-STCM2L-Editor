using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class Export {
        public string Name { get; set; }
        public UInt32 Address { get; set; }
        public UInt32 OldAddress { get; set; }

        public Action ExportedAction { get; set; }
    }
}

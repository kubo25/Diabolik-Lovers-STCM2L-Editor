using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    enum ParameterType {
        GLOBAL_PARAMETER,
        LOCAL_PARAMETER,
        VALUE
    }

    class Parameter {
        public UInt32 Value1 { get; set; }
        public UInt32 Value2 { get; set; }
        public UInt32 Value3 { get; set; }
        public UInt32 RelativeAddress { get; set; }
        public Action GlobalPointer { get; set; }
        public ParameterType Type { get; set; }
    }
}

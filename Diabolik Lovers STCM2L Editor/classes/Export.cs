using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Diabolik_Lovers_STCM2L_Editor.utils;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class Export {
        public string Name { get; set; }
        public UInt32 Address { get; set; }
        public UInt32 OldAddress { get; set; }
        public Action ExportedAction { get; set; }

        public byte[] Write() {
            List<byte> bytesExport = new List<byte>();

            bytesExport.AddRange(BitConverter.GetBytes(0));
            bytesExport.AddRange(EncodingUtil.encoding.GetBytes(Name));
            bytesExport.AddRange(BitConverter.GetBytes(ExportedAction.Address));

            return bytesExport.ToArray();
        }
    }
}

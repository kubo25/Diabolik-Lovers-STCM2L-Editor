using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.utils {
    public static class ByteUtil {
        public static UInt32 ReadUInt32(byte[] file, ref int seek) {
            return BitConverter.ToUInt32(ReadBytes(file, 4, ref seek), 0);
        }

        public static byte[] ReadBytes(byte[] file, UInt32 amount, ref int seek) {
            byte[] readValue = new byte[amount];

            Array.Copy(file, seek, readValue, 0, amount);

            seek += (int)amount;

            return readValue;
        }

        public static byte[] InsertUint32(byte[] original, UInt32 addition, UInt32 position) {
            return InsertBytes(original, BitConverter.GetBytes(addition), position);
        }

        public static byte[] InsertBytes(byte[] original, byte[] addition, UInt32 position) {
            Array.Copy(addition, 0, original, position, addition.Length);

            return original;
        }
    }
}

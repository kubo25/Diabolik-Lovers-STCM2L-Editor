using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Diabolik_Lovers_STCM2L_Editor.utils;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class Action {
        public const UInt32 ACTION_NAME = 0xd4;
        public const UInt32 ACTION_TEXT = 0xd2;
        public const UInt32 ACTION_CHOICE = 0xe7;

        public UInt32 Length { get; set; }
        public UInt32 ParameterCount { get; set; }
        public UInt32 LocalParameterCount { get; set; }
        public UInt32 OldAddress { get; set; }
        public UInt32 Address { get; set; }
        public UInt32 OpCode { get; set; }
        public UInt32 IsLocalCall { get; set; }

        public byte[] ExtraData { get; set; }
        public UInt32 ExtraDataLength { get; set; }

        public List<Parameter> Parameters { get; set; }

        public Action() {
            Length = 0;
            ParameterCount = 0;
            LocalParameterCount = 0;
            OldAddress = 0;
            Address = 0;
            OpCode = 0;
            IsLocalCall = 0;
            ExtraDataLength = 0;

            Parameters = new List<Parameter>();
        }

        public void ReadFromFile (UInt32 address, byte[] file) {
            OldAddress = address;

            int seek = (int)address;

            IsLocalCall = ByteUtil.ReadUInt32(file, ref seek);
            OpCode = ByteUtil.ReadUInt32(file, ref seek);
            ParameterCount = ByteUtil.ReadUInt32(file, ref seek);
            Length = ByteUtil.ReadUInt32(file, ref seek);


            for (int i = 0; i < ParameterCount; i++) {
                Parameter parameter = new Parameter();

                parameter.Value1 = ByteUtil.ReadUInt32(file, ref seek);
                parameter.Value2 = ByteUtil.ReadUInt32(file, ref seek);
                parameter.Value3 = ByteUtil.ReadUInt32(file, ref seek);

                if (
                    (((parameter.Value1 >> 24) & 0xff) != 0xff) &&
                    (parameter.Value1 > OldAddress) &&
                    (parameter.Value1 < OldAddress + Length)
                ) {
                    parameter.Type = ParameterType.LOCAL_PARAMETER;
                    LocalParameterCount++;
                    parameter.RelativeAddress = parameter.Value1 - OldAddress;
                }
                else if (parameter.Value1 == 0xffffff41) {
                    if(!Global.Calls.ContainsKey(parameter.Value2)) {
                        Global.Calls.Add(parameter.Value2, new List<Parameter>());
                    }

                    Global.Calls[parameter.Value2].Add(parameter);

                    parameter.GlobalPointer = null;
                    parameter.Type = ParameterType.GLOBAL_PARAMETER;
                }
                else {
                    parameter.Type = ParameterType.VALUE;
                }

                Parameters.Add(parameter);

            }

            ExtraDataLength = Length - 16 - ParameterCount * 12;
            if (ExtraDataLength > 0) {
                ExtraData = ByteUtil.ReadBytes(file, ExtraDataLength, ref seek);
            }
        }

        public string GetStringFromParameter(int parameter) {
            if (ExtraDataLength == 0) {
                return null;
            }

            UInt32 offset = Parameters[parameter].RelativeAddress - 16 - ParameterCount * 12;
            int position = (int)offset + 3 * 4;

            UInt32 length = ByteUtil.ReadUInt32(ExtraData, ref position);

            byte[] byteString = ByteUtil.ReadBytes(ExtraData, length, ref position);

            return EncodingUtil.encoding.GetString(byteString).TrimEnd(new char[] { '\0' });
        }
    }
}

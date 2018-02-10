using Diabolik_Lovers_STCM2L_Editor.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class STCM2L {
        public const int HEADER_OFFSET = 0x20;
        public const int EXPORT_SIZE = 0x28;
        public const int COLLECTION_LINK_PADDING = 0x38;

        public string FilePath { get; set; }
        public byte[] File { get; set; }

        public UInt32 StartPosition { get; set; }
        public byte[] StartData { get; set; }

        public UInt32 ExportsPosition { get; set; }
        public int ExportsCount { get; set; }
        public List<Export> Exports { get; set; }

        public UInt32 CollectionLinkPosition { get; set; }
        public UInt32 CollectionLinkOldAddress { get; set; }

        public List<Action> Actions { get; set; }

        public ObservableCollection<TextEntity> Texts { get; set; }

        public STCM2L (string filePath) {
            FilePath = filePath;
            Exports = new List<Export>();
            Actions = new List<Action>();
            Texts = new ObservableCollection<TextEntity>();
        }

        public bool Load() {
            try {
                File = System.IO.File.ReadAllBytes(FilePath);
                StartPosition = FindStart();

                Console.WriteLine("Start at: 0x{0:X}", StartPosition);

                if (StartPosition == 0) {
                    return false;
                }

                ReadStartData();
                ReadCollectionLink();
                ReadExports();
                ReadActions();

                MakeEntities();

                return true;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
        }

        private void ReadStartData () {
            int seek = 0;
            StartData = ByteUtil.ReadBytes(File, StartPosition, ref seek);

            seek = HEADER_OFFSET;
            ExportsPosition = ByteUtil.ReadUInt32(StartData, ref seek);

            seek += 2 * 4;
            CollectionLinkPosition = ByteUtil.ReadUInt32(StartData, ref seek);
        }

        private UInt32 FindStart() {
            byte[] start = EncodingUtil.encoding.GetBytes("CODE_START_");

            for (int i = 0; i < 2000; i++) {
                if (File[i] == start[0]) {
                    for (int j = 0; j < start.Length; j++) {
                        if (File[i + j] != start[j]) {
                            break;
                        }
                        else if (j + 1 == start.Length) {
                            return (UInt32) i + 0x0c;
                        }
                    }
                }
            }

            return 0;
        }

        private void ReadCollectionLink() {
            int seek = (int)CollectionLinkPosition + 4;
            CollectionLinkOldAddress = ByteUtil.ReadUInt32(File, ref seek);
        }

        private void ReadExports () {
            int exportsLength = (int) (CollectionLinkPosition - ExportsPosition);
            ExportsCount = exportsLength / EXPORT_SIZE;

            int seek = (int)ExportsPosition;

            for (int i = 0; i < ExportsCount; i++) {
                Export export = new Export();

                seek += 4;

                export.Name = EncodingUtil.encoding.GetString(ByteUtil.ReadBytes(File, 32, ref seek));
                export.OldAddress = ByteUtil.ReadUInt32(File, ref seek);

                Exports.Add(export);
            }
        }

        private void ReadActions () {
            UInt32 currentAddress = StartPosition;
            UInt32 maxAddress = ExportsPosition - 12; // Before EXPORT_DATA
            int currentExport = 0;
            int i = 0;

            do {
                Action action = new Action();
                i++;

                action.ReadFromFile(currentAddress, File);

                if(currentExport < Exports.Count && Exports[currentExport].OldAddress == currentAddress) {
                    Exports[currentExport].ExportedAction = action;
                    currentExport++;
                }

                currentAddress += action.Length;
                Actions.Add(action);
            }
            while (currentAddress < maxAddress);

            RecoverGlobalCalls(StartPosition);

            Console.WriteLine("Found {0} actions.", Actions.Count);
        }

        private void RecoverGlobalCalls (UInt32 startAddress) {
            UInt32 currentAddress = startAddress;

            foreach(Action action in Actions) {
                if (Global.Calls.ContainsKey(currentAddress)) {
                    List<Parameter> list = Global.Calls[currentAddress];

                    foreach(Parameter parameter in list) {
                        parameter.GlobalPointer = action;
                    }
                }
                currentAddress += action.Length;
            }
        }

        private void MakeEntities() {
            for (int i = 0; i < Actions.Count; i++) { 
                if (
                    (Actions[i].OpCode == Action.ACTION_NAME || Actions[i].OpCode == Action.ACTION_TEXT) &&
                    Actions[i].ExtraDataLength > 0
                ) {
                    TextEntity textEntity = new TextEntity();
                    textEntity.SetConversation(ref i, Actions);

                    Texts.Add(textEntity);
                }
                else if (Actions[i].OpCode == Action.ACTION_CHOICE) {
                    TextEntity textEntity = new TextEntity();
                    textEntity.SetAnswer(ref i, Actions);
                    Texts.Add(textEntity);
                }
            }

            Console.WriteLine("Read {0} texts.", Texts.Count);
        }
    }
}

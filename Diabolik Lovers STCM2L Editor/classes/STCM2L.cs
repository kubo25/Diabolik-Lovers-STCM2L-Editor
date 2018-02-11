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
        public byte[] OriginalFile { get; set; }
        public List<byte> NewFile { get; set; }

        public UInt32 StartPosition { get; set; }
        public byte[] StartData { get; set; }

        public UInt32 ExportsPosition { get; set; }
        public UInt32 ExportsCount { get; set; }
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
            NewFile = new List<byte>();
        }

        public bool Load() {
            try {
                OriginalFile = File.ReadAllBytes(FilePath);
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

        public bool Save(string filePath) {
            try {
                foreach(TextEntity text in Texts) {
                    text.ReinsertLines();
                }

                UInt32 newExportsAddress = StartPosition + GetActionsLength() + 12; // + EXPORT_DATA.Length
                UInt32 newCollectionLinkAddress = newExportsAddress + ExportsCount * EXPORT_SIZE + 16; // + COLLECTION_LINK.length

                StartData = ByteUtil.InsertUint32(StartData, newExportsAddress, HEADER_OFFSET);
                StartData = ByteUtil.InsertUint32(StartData, newCollectionLinkAddress, HEADER_OFFSET + 3 * 4);

                NewFile.AddRange(StartData);

                WriteActions();
                WriteExports();
                WriteCollectionLink();

                File.WriteAllBytes(filePath, NewFile.ToArray());
                return true;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return false;
            }
        }

        private void WriteActions() {
            SetAddresses();
            
            foreach(Action action in Actions) {
                NewFile.AddRange(action.Write());
            }
        }

        private void SetAddresses() {
            UInt32 address = StartPosition;

            foreach(Action action in Actions) {
                action.Address = address;
                address += action.Length;
            }
        }

        private void WriteExports() {
            NewFile.AddRange(EncodingUtil.encoding.GetBytes("EXPORT_DATA"));
            NewFile.Add(new byte());

            foreach (Export export in Exports) {
                NewFile.AddRange(export.Write());
            }
        }

        private void WriteCollectionLink() {
            NewFile.AddRange(EncodingUtil.encoding.GetBytes("COLLECTION_LINK"));
            NewFile.Add(new byte());
            NewFile.AddRange(BitConverter.GetBytes(0));

            UInt32 newCollectionLinkAddress = (UInt32) NewFile.Count + 4 + COLLECTION_LINK_PADDING;

            NewFile.AddRange(BitConverter.GetBytes(newCollectionLinkAddress));
            NewFile.AddRange(new byte[COLLECTION_LINK_PADDING]);

        }

        private UInt32 GetActionsLength() {
            UInt32 length = 0;

            foreach(Action action in Actions) {
                length += action.Length;
            }

            return length;
        }

        private void ReadStartData () {
            int seek = 0;
            StartData = ByteUtil.ReadBytes(OriginalFile, StartPosition, ref seek);

            seek = HEADER_OFFSET;
            ExportsPosition = ByteUtil.ReadUInt32(StartData, ref seek);

            seek += 2 * 4;
            CollectionLinkPosition = ByteUtil.ReadUInt32(StartData, ref seek);
        }

        private UInt32 FindStart() {
            byte[] start = EncodingUtil.encoding.GetBytes("CODE_START_");

            for (int i = 0; i < 2000; i++) {
                if (OriginalFile[i] == start[0]) {
                    for (int j = 0; j < start.Length; j++) {
                        if (OriginalFile[i + j] != start[j]) {
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
            CollectionLinkOldAddress = ByteUtil.ReadUInt32(OriginalFile, ref seek);
        }

        private void ReadExports () {
            UInt32 exportsLength = CollectionLinkPosition - ExportsPosition;
            ExportsCount = exportsLength / EXPORT_SIZE;

            int seek = (int)ExportsPosition;

            for (int i = 0; i < ExportsCount; i++) {
                Export export = new Export();

                seek += 4;

                export.Name = EncodingUtil.encoding.GetString(ByteUtil.ReadBytes(OriginalFile, 32, ref seek));
                export.OldAddress = ByteUtil.ReadUInt32(OriginalFile, ref seek);

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

                action.ReadFromFile(currentAddress, OriginalFile);

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

        public void InsertText (int index) {
            string name = null;

            if (Texts[index].Name != null) {
                name = Texts[index].Name.LineText;
            }

            TextEntity text = new TextEntity(Actions, Texts[index].ActionsEnd, name);

            Texts.Insert(index + 1, text);
            AddLine(index, text.AmountInserted);
        }

        public void AddLine(int index, int amount) {
            for(int i = index; i < Texts.Count; i++) {
                Texts[i].ActionsEnd += amount;
            }
        }
    }
}

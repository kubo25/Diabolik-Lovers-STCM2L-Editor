using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class STCM2L {
        public const int HEADER_OFFSET = 0x20;
        public const int EXPORT_SIZE = 0x28;

        private Encoding encoding = Encoding.GetEncoding("shift-jis");

        public string FilePath { get; set; }
        public byte[] File { get; set; }

        public int StartPosition { get; set; }
        public byte[] StartData { get; set; }

        public int ExportsPosition { get; set; }
        public int ExportsCount { get; set; }
        public List<Export> Exports { get; set; }

        public int CollectionLinkPosition { get; set; }
        public int CollectionLinkOldAddress { get; set; }

        public STCM2L (string filePath) {
            FilePath = filePath;
            Exports = new List<Export>();
        }

        public bool Load() {
            try {
                File = System.IO.File.ReadAllBytes(FilePath);
                StartPosition = FindStart();

                if (StartPosition == 0) {
                    return false;
                }

                ReadStartData();
                ReadCollectionLink();
                ReadExports();

                return true;
            }
            catch (Exception e) {
                return false;
            }
        }

        private void ReadStartData () {
            StartData = new byte[StartPosition];

            Array.Copy(File, StartData, StartPosition);

            byte[] positionArr = new byte[sizeof(int)];

            Array.Copy(StartData, HEADER_OFFSET, positionArr, 0, sizeof(int));
            ExportsPosition = BitConverter.ToInt32(positionArr, 0);

            Array.Copy(StartData, HEADER_OFFSET + 3 * 4, positionArr, 0, sizeof(int));
            CollectionLinkPosition = BitConverter.ToInt32(positionArr, 0);
        }

        private int FindStart() {
            byte[] start = encoding.GetBytes("CODE_START_");

            for (int i = 0; i < 2000; i++) {
                if (File[i] == start[0]) {
                    for (int j = 0; j < start.Length; j++) {
                        if (File[i + j] != start[j]) {
                            break;
                        }
                        else if (j + 1 == start.Length) {
                            return i + 0x0c;
                        }
                    }
                }
            }

            return 0;
        }

        private void ReadCollectionLink() {
            byte[] positionArr = new byte[sizeof(int)];

            Array.Copy(File, CollectionLinkPosition + 4, positionArr, 0, sizeof(int));
            CollectionLinkOldAddress = BitConverter.ToInt32(positionArr, 0);
        }

        private void ReadExports () {
            int exportsLength = File.Length - ExportsPosition;
            ExportsCount = exportsLength / EXPORT_SIZE;

            int seek = ExportsPosition;

            for (int i = 0; i < ExportsCount; i++) {
                Export export = new Export();

                seek += 4;

                Array.Copy(File, seek, export.Name, 0, 32);

                seek += 32;

                byte[] oldAddress = new byte[sizeof(int)];
                Array.Copy(File, seek, oldAddress, 0, sizeof(int));

                export.OldAddress = BitConverter.ToInt32(oldAddress, 0);

                seek += 4;

                Exports.Add(export);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class Line : INotifyPropertyChanged{
        public string LineText {
            get { return _LineText; }
            set {
                if (_LineText != value) {
                    _LineText = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("LineText"));
                }
            }
        }
        private string OriginalLineText { get; set; }
        private string _LineText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Line (string line) {
            OriginalLineText = line;
            _LineText = line;
        }


        public void Reset () {
            LineText = OriginalLineText;

            PropertyChanged(this, new PropertyChangedEventArgs("LineText"));
        }
    }
}

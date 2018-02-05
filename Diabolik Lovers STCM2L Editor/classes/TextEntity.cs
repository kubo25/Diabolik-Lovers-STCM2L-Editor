using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class TextEntity {
        public string Name { get; set; }
        public Action NameAction { get; set; }

        public List<string> Lines { get; set; }
        public List<Action> LineActions { get; set; }

        public int LineCount { get; set; }
        public UInt32 OldAddress { get; set; }

        public List<Action> Actions { get; set; }

        public bool IsAnswer { get; set; }
        public bool IsHighlighted { get; set; }

        public TextEntity() {
            LineActions = new List<Action>();
            Actions = new List<Action>();
        }

        public int SetConversation(int i, List<Action> actions) {
            Actions = actions;

            UInt32 opCode = actions[i].OpCode;
            OldAddress = actions[i].OldAddress;

            while (opCode == Action.ACTION_NAME || opCode == Action.ACTION_TEXT) {
                if (opCode == Action.ACTION_TEXT) {

                }
            }
        }
    }
}

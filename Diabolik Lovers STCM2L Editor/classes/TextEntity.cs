using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class TextEntity {
        public string Name { get; set; }
        public Action NameAction { get; set; }

        public ObservableCollection<Line> Lines { get; set; }
        public List<Action> LineActions { get; set; }
        public int LinesCount { get; set; } //TODO: Check if necessary

        public UInt32 OldAddress { get; set; }

        public List<Action> Actions { get; set; }

        public bool IsAnswer { get; set; }
        public bool IsHighlighted { get; set; }

        public TextEntity() {
            Name = null;
            Lines = new ObservableCollection<Line>();
            LineActions = new List<Action>();
            LinesCount = 0;
            Actions = new List<Action>();
        }

        public void SetConversation(ref int i, List<Action> actions) {
            Actions = actions;

            UInt32 opCode = actions[i].OpCode;
            OldAddress = actions[i].OldAddress;

            while (opCode == Action.ACTION_NAME || opCode == Action.ACTION_TEXT) {
                if (opCode == Action.ACTION_NAME) {
                    if (Name == null) {
                        Name = actions[i].GetStringFromParameter(0);
                        NameAction = actions[i];
                    }
                }
                else {
                    string temp = actions[i].GetStringFromParameter(0);

                    Line line = new Line(temp);

                    Lines.Add(line);
                    LineActions.Add(actions[i]);
                    LinesCount++;
                }
                i++;

                if (i >= actions.Count) {
                    break;
                }

                opCode = actions[i].OpCode;
            }

            i--;
        }

        public void SetAnswer(ref int i, List<Action> actions) {
            string temp = actions[i].GetStringFromParameter(0);
            Line line = new Line(temp);

            Lines.Add(line);
            LineActions.Add(actions[i]);
            LinesCount = 1;
            IsAnswer = true;
        }

        public void ResetText() {
            foreach(Line line in Lines) {
                line.Reset();
            }
        }
    }
}

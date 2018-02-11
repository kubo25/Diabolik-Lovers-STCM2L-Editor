using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabolik_Lovers_STCM2L_Editor.classes {
    class TextEntity {
        public Line Name { get; set; }
        public Action NameAction { get; set; }

        public ObservableCollection<Line> Lines { get; set; }
        public List<Action> LineActions { get; set; }
        public int LinesCount { get; set; } //TODO: Check if necessary

        public UInt32 OldAddress { get; set; }

        public List<Action> Actions { get; set; }
        public int ActionsEnd { get; set; }

        public bool IsAnswer { get; set; }
        public bool IsHighlighted { get; set; }

        public int AmountInserted { get; set; }

        public TextEntity() {
            Name = null;
            Lines = new ObservableCollection<Line>();
            LineActions = new List<Action>();
            LinesCount = 0;
            AmountInserted = 0;
        }

        public TextEntity(List<Action> actions, int actionsEnd, string name) {
            Actions = actions;
            ActionsEnd = actionsEnd;
            AmountInserted = 0;

            Actions.Insert(ActionsEnd, new Action(0, Action.ACTION_DIVIDER, 0));
            AmountInserted++;

            if (name != null) {
                Name = new Line(name);
                NameAction = new Action(0, Action.ACTION_NAME, 1);
                Actions.Insert(ActionsEnd + AmountInserted, NameAction);
                AmountInserted++;
            }

            Lines = new ObservableCollection<Line>();
            LineActions = new List<Action>();
            LinesCount = 0;
            AddLine();

            IsAnswer = false;
            IsHighlighted = false;
        }

        public void SetConversation(ref int i, List<Action> actions) {
            Actions = actions;

            UInt32 opCode = actions[i].OpCode;
            OldAddress = actions[i].OldAddress;

            while (opCode == Action.ACTION_NAME || opCode == Action.ACTION_TEXT) {
                if (opCode == Action.ACTION_NAME) {
                    if (Name == null) {
                        Name = new Line(actions[i].GetStringFromParameter(0));
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
                ActionsEnd = i;
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
            ResetName();

            foreach(Line line in Lines) {
                line.Reset();
            }
        }

        public void ResetName() {
            if(Name != null) {
                Name.Reset();
            }
        }

        public void ReinsertLines () {
            if(Name != null) {
                NameAction.SetString(Name.LineText, 0);
            }

            for(int i = 0; i < LinesCount; i++) {
                LineActions[i].SetString(Lines[i].LineText, 0);
            }
        }

        public void AddLine() {
            if (!IsAnswer && LinesCount <= 3) {
                Action action = new Action(0, Action.ACTION_TEXT, 1);
                Line line = new Line("");

                Lines.Add(line);
                LineActions.Add(action);
                Actions.Insert(ActionsEnd + AmountInserted, action);

                AmountInserted++;
                LinesCount++;
            }
        }
    }
}

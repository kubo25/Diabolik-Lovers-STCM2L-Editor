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
            AmountInserted = 0;
        }

        public TextEntity(List<Action> actions, int actionsEnd, string name, bool newPage, bool before) {
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
            else {
                if (newPage) {
                    Actions.Insert(ActionsEnd + AmountInserted, new Action(0, Action.ACTION_NEW_PAGE, 0));
                    AmountInserted++;
                }

                Actions.Insert(ActionsEnd + AmountInserted, new Action(0, Action.ACTION_DIVIDER, 0));
                AmountInserted++;
            }

            Lines = new ObservableCollection<Line>();
            LineActions = new List<Action>();
            AddLine(true);

            if (before) {
                Actions.Insert(ActionsEnd + AmountInserted, new Action(0, Action.ACTION_DIVIDER, 0));
                AmountInserted++;
            }

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

            for(int i = 0; i < Lines.Count; i++) {
                LineActions[i].SetString(Lines[i].LineText, 0);
            }
        }

        public void AddLine(bool isNew = false, int index = -1) {
            if (!IsAnswer && Lines.Count < 3) {
                Action action = new Action(0, Action.ACTION_TEXT, 1);
                Line line = new Line("");

                if(index == -1 || index == Lines.Count) {
                    Lines.Add(line);
                    LineActions.Add(action);
                    Actions.Insert(ActionsEnd + (isNew ? AmountInserted : 0), action);
                }
                else {
                    Lines.Insert(index, line);
                    LineActions.Insert(index, action);
                    Actions.Insert(ActionsEnd - (index == 0 ? 1 : index), action);
                }

                AmountInserted++;
            }
        }

        public void DeleteLine(int index) {
            Lines.Remove(Lines[index]);
            Actions.Remove(LineActions[index]);
            LineActions.Remove(LineActions[index]);
        }

        public void DeleteText() {
            for(int i = 1; i <= AmountInserted; i++) {
                Actions.Remove(Actions[ActionsEnd - i]);
            }
        }
    }
}

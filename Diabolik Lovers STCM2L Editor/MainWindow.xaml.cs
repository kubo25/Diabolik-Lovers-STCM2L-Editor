using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.ComponentModel;

using Diabolik_Lovers_STCM2L_Editor.classes;
using MahApps.Metro.Controls;

namespace Diabolik_Lovers_STCM2L_Editor {
    public partial class MainWindow : MetroWindow {
        private STCM2L Stcm2l;
        private bool ShouldSave = false;

        public MainWindow() {
            InitializeComponent();
            Closing += OnClose;
        }

        private void OnClose (object sender, CancelEventArgs e) {
            if (Stcm2l != null && ShouldSave) {
                MessageBoxResult saveWarning = ShowSaveWarning();

                switch (saveWarning) {
                    case MessageBoxResult.Yes:
                        SaveAsCommand(null, null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private MessageBoxResult ShowSaveWarning() {
            string messageBoxCaption = "Save";
            string messageBoxText = "Do you want to save your changes?";
            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxImage image = MessageBoxImage.Warning;

            return MessageBox.Show(messageBoxText, messageBoxCaption, button, image);
        }

        private void OpenFileCommad(object sender, ExecutedRoutedEventArgs e) {
            if (Stcm2l != null && ShouldSave) {
                MessageBoxResult saveWarning = ShowSaveWarning();

                switch (saveWarning) {
                    case MessageBoxResult.Yes:
                        SaveAsCommand(null, null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                       return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true) {
                OpenFile(openFileDialog.FileName);
            }
        }

        private void OpenFile(string path) {
            Stcm2l = new STCM2L(path);

            Title = Path.GetFileName(path);

            if (Stcm2l.Load()) {
                TextsList.DataContext = Stcm2l.Texts;
                TextsList.ItemsSource = Stcm2l.Texts;

                LinesList.DataContext = null;
                LinesList.ItemsSource = null;

                NameBox.DataContext = null;

                ShouldSave = false;
            }
            else {
                Console.WriteLine("Invalid File");
            }
        }

        private void SaveAsCommand(object sender, ExecutedRoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            if (saveFileDialog.ShowDialog() == true) {
                if (Stcm2l == null || !Stcm2l.Save(saveFileDialog.FileName)) {
                    Console.WriteLine("Failed to save.");
                }
                else {
                    OpenFile(saveFileDialog.FileName);
                    ShouldSave = false;
                }
            }
        }

        private void SaveCommand(object sender, ExecutedRoutedEventArgs e) {
            if (Stcm2l == null || !Stcm2l.Save(Stcm2l.FilePath)) {
                Console.WriteLine("Failed to save.");
            }
            else {
                ShouldSave = false;
            }
        }

        private void ListBoxItemClick(object sender, MouseButtonEventArgs e) {
            LinesList.DataContext = (sender as ListBoxItem).DataContext;
            NameBox.DataContext = (sender as ListBoxItem).DataContext;

            Binding binding = new Binding();
            binding.Path = new PropertyPath("Lines");
            binding.Source = (sender as ListBoxItem).DataContext;
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            LinesList.SetBinding(ItemsControl.ItemsSourceProperty, binding);
        }

        private void ResetAllTextClick(object sender, RoutedEventArgs e) {
            if (LinesList.DataContext != null) {
                (LinesList.DataContext as TextEntity).ResetText();
            }
        }

        private void ResetLineClick(object sender, RoutedEventArgs e) {
            ((sender as MenuItem).DataContext as TextEntity).Lines[LinesList.SelectedIndex].Reset();
        }

        private void ResetNameClick(object sender, RoutedEventArgs e) {
            if (NameBox.DataContext as TextEntity != null) {
                (NameBox.DataContext as TextEntity).ResetName();
            }
        }

        private void AddNewLineClick(object sender, RoutedEventArgs e) {
            if (LinesList.DataContext as TextEntity != null) {
                InsertLine();
                Stcm2l.AddLine(TextsList.SelectedIndex, 1);
            }
        }

        private void InsertNewLineBeforeClick(object sender, RoutedEventArgs e) {
            if (LinesList.DataContext as TextEntity != null) {
                InsertLine(LinesList.SelectedIndex);
                Stcm2l.AddLine(TextsList.SelectedIndex, 1);
            }
        }

        private void InsertNewLineAfterClick(object sender, RoutedEventArgs e) {
            if (LinesList.DataContext as TextEntity != null) {
                InsertLine(LinesList.SelectedIndex + 1);
                Stcm2l.AddLine(TextsList.SelectedIndex, 1);
            }
        }

        private void InsertLine(int index = -1) {
            (LinesList.DataContext as TextEntity).AddLine(false, index);
            ShouldSave = true;
        }

        private void DeleteLineClick(object sender, RoutedEventArgs e) {
            int index = LinesList.SelectedIndex;
            ((sender as MenuItem).DataContext as TextEntity).DeleteLine(index);
            Stcm2l.DeleteLine(index, 1);
            ShouldSave = true;
        }

        private void InsertNewTextAfterClick(object sender, RoutedEventArgs e) {
            InsertNewText(false);
        }

        private void InsertNewTextBeforeClick(object sender, RoutedEventArgs e) {
            InsertNewText(true);
        }

        private void InsertNewText(bool before) {
            if (TextsList.SelectedIndex != -1) {
                bool newPage = false;
                if (Stcm2l.Texts[TextsList.SelectedIndex].Name == null) {
                    string messageBoxCaption = "New page";
                    string messageBoxText = "Do you want to create a new page?";
                    MessageBoxButton button = MessageBoxButton.YesNo;
                    MessageBoxImage image = MessageBoxImage.Question;

                    MessageBoxResult result = MessageBox.Show(messageBoxText, messageBoxCaption, button, image);

                    newPage = result == MessageBoxResult.Yes;
                }
                Stcm2l.InsertText(TextsList.SelectedIndex, before, newPage);
                ShouldSave = true;
            }
        }

        private void DeleteTextClick(object sender, RoutedEventArgs e) {
            Stcm2l.DeleteText(TextsList.SelectedIndex);

            LinesList.DataContext = null;
            LinesList.ItemsSource = null;

            NameBox.DataContext = null;

            ShouldSave = true;
        }

        private void TextChanged(object sender, TextChangedEventArgs e) {
            ShouldSave = true;
        }
    }
}

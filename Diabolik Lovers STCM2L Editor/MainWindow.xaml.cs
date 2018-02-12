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

using Diabolik_Lovers_STCM2L_Editor.classes;
using MahApps.Metro.Controls;
using System.IO;

namespace Diabolik_Lovers_STCM2L_Editor {
    public partial class MainWindow : MetroWindow {
        private STCM2L stcm2l;

        public MainWindow() {
            InitializeComponent();
        }

        private void OpenFileCommad(object sender, ExecutedRoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true) {
                OpenFile(openFileDialog.FileName);
            }
        }

        private void OpenFile(string path) {
            stcm2l = new STCM2L(path);

            Title = Path.GetFileName(path);

            if (stcm2l.Load()) {
                TextsList.DataContext = stcm2l.Texts;
                TextsList.ItemsSource = stcm2l.Texts;

                LinesList.DataContext = null;
                LinesList.ItemsSource = null;

                NameBox.DataContext = null;
            }
            else {
                Console.WriteLine("Invalid File");
            }
        }

        private void SaveAsCommand(object sender, ExecutedRoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            if (saveFileDialog.ShowDialog() == true) {
                if (stcm2l == null || !stcm2l.Save(saveFileDialog.FileName)) {
                    Console.WriteLine("Failed to save.");
                }
                else {
                    OpenFile(saveFileDialog.FileName);
                }
            }
        }

        private void SaveCommand(object sender, ExecutedRoutedEventArgs e) {
            if (stcm2l == null || !stcm2l.Save(stcm2l.FilePath)) {
                Console.WriteLine("Failed to save.");
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
                stcm2l.AddLine(TextsList.SelectedIndex, 1);
            }
        }

        private void InsertNewLineBeforeClick(object sender, RoutedEventArgs e) {
            if (LinesList.DataContext as TextEntity != null) {
                InsertLine(LinesList.SelectedIndex);
                stcm2l.AddLine(TextsList.SelectedIndex, 1);
            }
        }

        private void InsertNewLineAfterClick(object sender, RoutedEventArgs e) {
            if (LinesList.DataContext as TextEntity != null) {
                InsertLine(LinesList.SelectedIndex + 1);
                stcm2l.AddLine(TextsList.SelectedIndex, 1);
            }
        }

        private void InsertLine(int index = -1) {
            (LinesList.DataContext as TextEntity).AddLine(false, index);
        }

        private void DeleteLineClick(object sender, RoutedEventArgs e) {
            int index = LinesList.SelectedIndex;
            ((sender as MenuItem).DataContext as TextEntity).DeleteLine(index);
            stcm2l.DeleteLine(index, 1);
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
                if (stcm2l.Texts[TextsList.SelectedIndex].Name == null) {
                    string messageBoxCaption = "New page";
                    string messageBoxText = "Do you want to create a new page?";
                    MessageBoxButton button = MessageBoxButton.YesNo;
                    MessageBoxImage image = MessageBoxImage.Question;

                    MessageBoxResult result = MessageBox.Show(messageBoxText, messageBoxCaption, button, image);

                    newPage = result == MessageBoxResult.Yes;
                }
                stcm2l.InsertText(TextsList.SelectedIndex, before, newPage);
            }
        }

        private void DeleteTextClick(object sender, RoutedEventArgs e) {
            stcm2l.DeleteText(TextsList.SelectedIndex);

            LinesList.DataContext = null;
            LinesList.ItemsSource = null;

            NameBox.DataContext = null;
        }
    }
}

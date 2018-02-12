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

        private void OpenFile(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true) {
                stcm2l = new STCM2L(openFileDialog.FileName);

                Title = Path.GetFileName(openFileDialog.FileName);

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

        private void ResetAllText(object sender, RoutedEventArgs e) {
            if(LinesList.DataContext != null) {
                (LinesList.DataContext as TextEntity).ResetText();
            }
        }

        private void SaveAs(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            if (saveFileDialog.ShowDialog() == true) {
                if (stcm2l == null || !stcm2l.Save(saveFileDialog.FileName)) {
                    Console.WriteLine("Failed to save.");
                }
            }
        }

        private void ResetLine(object sender, RoutedEventArgs e) {
            if ((sender as Button).DataContext as Line != null) {
                ((sender as Button).DataContext as Line).Reset();
            }
        }

        private void ResetName(object sender, RoutedEventArgs e) {
            if (NameBox.DataContext as TextEntity != null) {
                (NameBox.DataContext as TextEntity).ResetName();
            }
        }

        private void AddNewLineClick(object sender, RoutedEventArgs e) {
            if (LinesList.DataContext as TextEntity != null) {
                (LinesList.DataContext as TextEntity).AddLine();
                stcm2l.AddLine(TextsList.SelectedIndex, 1);
            }
        }

        private void DeleteLineClick(object sender, RoutedEventArgs e) {
            ((sender as MenuItem).DataContext as TextEntity).DeleteLine(LinesList.SelectedIndex);
            stcm2l.DeleteLine(LinesList.SelectedIndex, 1);
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

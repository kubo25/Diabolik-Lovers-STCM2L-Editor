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
using System.Windows.Shapes;

using Diabolik_Lovers_STCM2L_Editor.classes;

namespace Diabolik_Lovers_STCM2L_Editor {
    public partial class MainWindow : Window {
        private STCM2L stcm2l;

        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;
        }

        private void OpenFile(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true) {
                stcm2l = new STCM2L(openFileDialog.FileName);

                if (stcm2l.Load()) {
                    TextsList.ItemsSource = stcm2l.Texts;
                }
                else {
                   // TextBox.Text = "Invalid File";
                }

            }
        }

        private void ListBoxItemClick(object sender, MouseButtonEventArgs e) {
            LinesList.ItemsSource = ((sender as ListBoxItem).DataContext as TextEntity).Lines;
        }
    }
}

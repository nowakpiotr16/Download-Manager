using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Download_Manager.Controllers;
using Download_Manager.Classes;

namespace Download_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
         * PLANS | CHANGES | IDEAS | NEEDS
         * file'll be available only in the controllers;
         * handed over from the Boss (for example) to others when needed.
         * do we need a list of files, too?
         */


        // Old listbox:
        // <ListBox x:Name="filesListBox" HorizontalAlignment="Left" Height="321" Margin="10,59,0,0" VerticalAlignment="Top" Width="499"/>
            
        public MainWindow()
        {
            InitializeComponent();
            BossControl.Instance.MainView = this;
            linkTextBox.Focus();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            BossControl.Instance.Search(linkTextBox.Text);
           // Dispatcher.Invoke(() => BossControl.Instance.Search(linkTextBox.Text));            
        }

        private async void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            List<File> selectedFiles = filesListBox.SelectedItems.Cast<File>().ToList();
            //File selectedFile = (File)filesListBox.SelectedItem;
            await Task.Run(() => BossControl.Instance.DownloadFile(selectedFiles));
        }
    }
}

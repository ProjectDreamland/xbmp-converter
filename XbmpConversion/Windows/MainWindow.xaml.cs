using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using XbmpConversion.Models;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;


namespace XbmpConversion.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += HandleLoad;
        }

        private void HandleLoad(object sender, RoutedEventArgs e)
        {
            var currentPath = Directory.GetCurrentDirectory();
            if (!Directory.Exists(Path.Combine(currentPath, "output")))
            {
                Directory.CreateDirectory(Path.Combine(currentPath, "output"));
                Directory.CreateDirectory(Path.Combine(currentPath, "xbmp"));
            }
            
        }

        private void TwitterMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://twitter.com/andrewmd5");
        }

        private void GitHubMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/ProjectDreamland/xbmp-converter");
        }

        private void BorderlessGamingMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("http://store.steampowered.com/app/388080");
        }

        private void ReportABugMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/ProjectDreamland/xbmp-converter/issues");
        }

        private void ImageGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            ImageGrid.SelectedIndex = row.GetIndex();
            row.Focus();
            Keyboard.Focus(ImageGrid);
            var fileModel = (FileViewModel) row?.Item;
            var vm = this.DataContext as MainWindowViewModel;
            vm?.UpdateStats(fileModel?.Path);
            e.Handled = true;
        }

        private void ImageGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dgv = (DataGrid)sender;
            if (dgv != null)
            {
                if (e.AddedItems.Count > 0)
                {
                    var fileModel = (FileViewModel)e.AddedItems[0];
                    var vm = this.DataContext as MainWindowViewModel;
                    vm?.UpdateStats(fileModel?.Path);
                    e.Handled = true;
                }
              
            }
        }
    }
}
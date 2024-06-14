using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GetFileIcons
{
    public partial class MainWindow : Window
    {
        private string _directoryPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var directoryDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select Project Directory",
            };

            if (directoryDialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                _directoryPath = directoryDialog.FileName;
                PathLabel.Text = _directoryPath;
                RefreshIcons();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshIcons();
        }

        private void RefreshIcons()
        {
            if (!Directory.Exists(_directoryPath)) return;

            var dataSource = new ObservableCollection<FileItem>();

            var files = Directory.GetFiles(_directoryPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var icon = IconTools.GetImageSourceForExtension(System.IO.Path.GetExtension(file));
                dataSource.Add(new FileItem { FileIcon = icon, Name = file });
            }

            ListView.ItemsSource = dataSource;
        }

        class FileItem
        {
            public ImageSource FileIcon { get; set; }
            public string Name { get; set; }
        }
    }
}

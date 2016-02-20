#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using XbmpConversion.Actions;
using XbmpConversion.Controls;
using XbmpConversion.Images.Utilities;
using XbmpConversion.Utilities;
using XBMPConverter.Images;
using Image = System.Drawing.Image;

#endregion

namespace XbmpConversion.Models
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<FileViewModel> _filesInternal = new ObservableCollection<FileViewModel>();
        private readonly ObservableCollection<string> _pathsInternal = new ObservableCollection<string>();
        private string _statistics;
        private BitmapImage _imagePreview;

        public MainWindowViewModel()
        {
            Paths = new ReadOnlyObservableCollection<string>(_pathsInternal);
            Files = new ReadOnlyObservableCollection<FileViewModel>(_filesInternal);

            LoadCommand = new ActionCommand(BrowseFiles);
            ConvertCommand = new ActionCommand(RunConvert);

            //TODO run on a background thread, add spinner etc
            //RunRefresh();
        }

        public ReadOnlyObservableCollection<string> Paths { get; }

        public ReadOnlyObservableCollection<FileViewModel> Files { get; }

        public ICommand ConvertCommand { get; }

        public ICommand LoadCommand { get; }

        public string Statistics
        {
            get { return _statistics; }
            set
            {
                if (_statistics == value) return;
                _statistics = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage ImagePreview
        {
            get { return _imagePreview; }
            set
            {
                if (_imagePreview == value) return;
                _imagePreview = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RunConvert()
        {
            await ConvertImages();
            RunRefresh();
        }

        private async Task ConvertImages()
        {
            var images = BitmapUtilities.CachedImages;
            var dialog = new ConfirmationDialog
            {
                MessageTextBlock =
                {
                    Text = "Are you sure you wish to do this? All files will be over written."
                }
            };
            var result = await DialogHost.Show(dialog);
            if (!"1".Equals(result)) return;
            var progressBar = new ProgressBar
            {
                Maximum = images.Count,
                Width = 300,
                Margin = new Thickness(32)
            };
            await
                DialogHost.Show(progressBar,
                    (DialogOpenedEventHandler)
                        ((o, args) => ConvertFiles(images, progressBar, args.Session)));
        }

        private void ConvertFiles(IEnumerable<BitmapUtilities.StoredImages> images, ProgressBar progressBar,
            DialogSession session)
        {
            Task.Factory.StartNew(() =>
            {
                var failures = new List<string>();

                foreach (var item in images.Select((red, idx) => new {red, idx}))
                {
                    progressBar.Dispatcher.BeginInvoke(new Action(() => progressBar.Value = item.idx));
                    try
                    {
                        var image = item.red;
                        var extension = Path.GetExtension(image.Path)?.ToLower();
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(image.Path);
                        if (extension != null && extension.Equals(".xbmp"))
                        {
                            image.Xbmp.Image.Save("./output/" + fileNameWithoutExtension + ".png", ImageFormat.Png);
                            image.Xbmp.Close();
                        }
                        else
                        {
                            File.Open("./xbmp/" + fileNameWithoutExtension + ".xbmp", FileMode.Create).Close();
                            image.Xbmp.Parent = "./xbmp/" + fileNameWithoutExtension + ".xbmp";
                            image.Xbmp.Save();
                            image.Xbmp.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        failures.Add(item.red.Path);
                    }
                }

                if (failures.Count > 0)
                {
                    progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var failuresDialog = new FailuresDialog {FailuresListBox = {ItemsSource = failures}};
                        session.UpdateContent(failuresDialog);
                        BitmapUtilities.CachedImages.Clear();
                    }));
                }
                else
                {
                    progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var successDialog = new SuccessDialog();
                        session.UpdateContent(successDialog);
                        BitmapUtilities.CachedImages.Clear();
                        UpdateStats("");
                    }));
                }
            });
        }

        private void AddPaths(ObservableCollection<string> pathsInternal)
        {
        }

        private void RunRefresh()
        {
            //needs to be called to ensure we aren't loading a previously stored object.
            _pathsInternal.Clear();
            _filesInternal.Clear();
            foreach (
                var fileViewModel in
                    BitmapUtilities.CachedImages.Select(
                        image => new FileViewModel(image.Path, StringUtilities.GetBytesReadable(image.Size))))
            {
                _filesInternal.Add(fileViewModel);
            }


            // Statistics = CleanerUtilities.TotalFiles() + " files have been found (" + CleanerUtilities.TotalTakenSpace() +
            //        ") ";
        }

        public void UpdateStats(string imagePath)
        {
            foreach (var image in BitmapUtilities.CachedImages.Where(image => image.Path.Equals(imagePath)))
            {
                ImagePreview = BitmapUtilities.ToBitmapImage(image.Xbmp.Image);
                Statistics = image.Xbmp.GetStats();
                return;
            }
            Statistics = imagePath;
        }

        private async void BrowseFiles()
        {
            var openFileDialog = new OpenFileDialog {Multiselect = true};
            var codecs = ImageCodecInfo.GetImageEncoders();
            openFileDialog.Filter =
                string.Format("XBMP images (*.xbmp)|*.xbmp|{0}| All image files ({1})|{1}|All files|*",
                    string.Join("|", codecs.Select(codec =>
                        string.Format("{0} ({1})|{1}", codec.CodecName, codec.FilenameExtension)).ToArray()),
                    string.Join(";", codecs.Select(codec => codec.FilenameExtension).ToArray()));
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames.Length >= 1)
                {
                    var progressBar = new ProgressBar
                    {
                        Maximum = openFileDialog.FileNames.Length,
                        Width = 300,
                        Margin = new Thickness(32)
                    };
                    await
                        DialogHost.Show(progressBar,
                            (DialogOpenedEventHandler)
                                ((o, args) => LoadFiles(openFileDialog.FileNames, progressBar, args.Session)));
                
                    RunRefresh();
                   
                }
            }
        }

        private void LoadFiles(IEnumerable<string> fileNames, ProgressBar progressBar, DialogSession session)
        {

            Task.Factory.StartNew(() =>
            {
                var failures = new List<string>();

                foreach (var item in fileNames.Select((red, idx) => new { red, idx }))
                {
                    progressBar.Dispatcher.BeginInvoke(new Action(() => progressBar.Value = item.idx));
                    try
                    {
                        var file = item.red;
                        var extension = Path.GetExtension(file)?.ToLower();
                        var xbmp = new XbmpImage(file);
                        var length = new FileInfo(file).Length;

                        if (extension != null && extension.Equals(".xbmp"))
                        {
           
                          xbmp.Load();
                        }
                        else
                        {
                          var image = (Bitmap)Image.FromFile(file);
                          xbmp.SetImage(image);
                          
                        }
                        var storeImage = new BitmapUtilities.StoredImages
                        {
                            Path = file,
                            Size = length,
                            Xbmp = xbmp
                        };
                        BitmapUtilities.CachedImages.Add(storeImage);
                    }
                    catch 
                    {
                        failures.Add(item.red);
                    }
                }

                if (failures.Count > 0)
                {
                    progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var failuresDialog = new FailuresDialog { FailuresListBox = { ItemsSource = failures } };
                        session.UpdateContent(failuresDialog);
                    }));
                }
                else
                {
                    progressBar.Dispatcher.BeginInvoke(new Action(session.Close));

                }
            });
        }



        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
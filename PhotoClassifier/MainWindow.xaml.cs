using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PhotoClassifier
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentImageIndex = -1;

        private string _outputDiscardFolderPath = string.Empty;
        private string _outputKeepFolderPath = string.Empty;

        private string _selectedFolderPath = string.Empty;

        public MainWindow()
        {
            ViewModel = new MainWindowViewModel();
            InitializeComponent();
        }

        public MainWindowViewModel ViewModel { get; }

        private bool HideKeptFiles => HideKeepCheckBox.IsChecked ?? false;
        private bool HideDiscardedFiles => HideDiscardCheckBox.IsChecked ?? false;

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            var selectionOk = fbd.ShowDialog();

            if (selectionOk == System.Windows.Forms.DialogResult.OK)
            {
                SetOutputFolderPaths(fbd.SelectedPath);
                //SelectedFolderTextBlock.Text = _selectedFolderPath;
                ViewModel.SetFiles(GetImagesFromPath(_selectedFolderPath));


                SetImageSource();
                //TODO Fix loading of images on startup from Kept and Discarded paths
            }
            else
            {
                ResetAllPathsAndResults();
                SelectedFolderTextBlock.Text = "Select Folder...Please try again";
                DebugTextBlock.Text = "Folder Selection Failed";
            }
        }


        private List<string> GetImagesFromPath(string selectedFolderPath)
        {
            var filesFound = new List<string>();
            const SearchOption searchOption = SearchOption.TopDirectoryOnly;
            var filters = new[] {"jpg", "jpeg", "png", "gif", "tiff", "bmp"};
            if (Directory.Exists(selectedFolderPath))
            {
                foreach (var filter in filters)
                {
                    filesFound.AddRange(Directory.GetFiles(selectedFolderPath, $"*.{filter}", searchOption));
                }
                //DebugTextBlock.Text = $"Found {filesFound.Count} files";
            }
            return filesFound;
        }

        private void SelectNext()
        {
//            var fileFound = false;
//            var imagesTested = 0;
//            var imagesCount = _images.Count;
//            while (!fileFound)
//            {
//                _currentImageIndex++;
//                if (_currentImageIndex >= imagesCount) _currentImageIndex = 0;
//
//                var isInKeptFiles = _imagesToKeep.Contains(ViewModel.CurrentImagePath);
//                var isInDiscardedFiles = _imagesToDiscard.Contains(ViewModel.CurrentImagePath);
//                fileFound = true;
//                if (HideKeptFiles && isInKeptFiles) { fileFound = false;}
//                else if (HideDiscardedFiles && isInDiscardedFiles) fileFound = false;
//
//                imagesTested++;
//                if (imagesTested >= imagesCount) break;
//            }
//
//            if (imagesTested >= imagesCount)
//            {
//                Debug.WriteLine("No Unassigned Images (+)");
//                DebugTextBlock.Text = "No Unassigned Images (+)";
//                ResetImage();
//                return;
//            }
            ViewModel.MoveNextImage();
            SetImageSource();
        }

        private void SetImageSource()
        {
            DebugTextBlock.Text = $"[{ViewModel.CurrentImage.Action}] {ViewModel.CurrentImage.Uri}";
            DisplayImage.Source = ViewModel.TransformedBitmap;
        }

        private void SelectPrevious()
        {
//            var fileFound = false;
//            var imagesTested = 0;
//            var imagesCount = _images.Count;
//            while (!fileFound)
//            {
//                _currentImageIndex--;
//                if (_currentImageIndex < 0) _currentImageIndex = imagesCount - 1;
//
//                var isInKeptFiles = _imagesToKeep.Contains(ViewModel.CurrentImagePath);
//                var isInDiscardedFiles = _imagesToDiscard.Contains(ViewModel.CurrentImagePath);
//                fileFound = true;
//                if (HideKeptFiles && isInKeptFiles) fileFound = false;
//                else if (HideDiscardedFiles && isInDiscardedFiles) fileFound = false;
//
//                imagesTested++;
//                if (imagesTested >= imagesCount) break;
//            }
//
//            if (imagesTested >= imagesCount)
//            {
//                Debug.WriteLine("No Unassigned Images (-)");
//                DebugTextBlock.Text = "No Unassigned Images (-)";
//                ResetImage();
//                return;
//            }

            ViewModel.MovePreviousImage();
            SetImageSource();
        }

        private void SetOutputFolderPaths(string path)
        {
            _selectedFolderPath = path;
            _outputKeepFolderPath = Path.Combine(path, "_Keep");
            _outputDiscardFolderPath = Path.Combine(path, "_Discard");
        }

        private void ResetAllPathsAndResults()
        {
            _selectedFolderPath = string.Empty;
            _outputKeepFolderPath = string.Empty;
            _outputDiscardFolderPath = string.Empty;

            _currentImageIndex = -1;
            ResetImage();
        }

        private void ResetImage()
        {
            DisplayImage.Source = new BitmapImage();
        }

        private void MainGrid_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                //                DebugTextBlock.Text = "Right";
                // Keep
                SelectNext();
            }
            if (e.Key == Key.Left)
            {
                //                DebugTextBlock.Text = "Left";
                // Discard
                SelectPrevious();
            }
            if (e.Key == Key.Up)
            {
                KeepCurrent();
                SelectNext();
            }
            if (e.Key == Key.Down)
            {
                DiscardCurrent();
                SelectNext();
            }
        }

        private void KeepCurrent()
        {
            ViewModel.Keep();
            Debug.WriteLine($"Keep: {ViewModel.CurrentImage.Uri}");
        }

        private void DiscardCurrent()
        {
            ViewModel.Discard();
            Debug.WriteLine($"Discard: {ViewModel.CurrentImage.Uri}");
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var image in ViewModel.ImagesToKeep)
            {
                CopyFileToPath(image.Uri, _outputKeepFolderPath);
            }
            foreach (var image in ViewModel.ImagesToDiscard)
            {
                CopyFileToPath(image.Uri, _outputDiscardFolderPath);
            }
        }

        private static void CopyFileToPath(string uri, string destinationPath)
        {
            var file = Path.GetFileName(uri);
            if (string.IsNullOrEmpty(file))
            {
                Debug.WriteLine($"Failed to write file {uri}");
                return;
            }
            var output = Path.Combine(destinationPath, file);

            if (!Directory.Exists(destinationPath))
            {
                try
                {
                    Directory.CreateDirectory(destinationPath);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return;
                }
            }
            try
            {
                File.Copy(uri, output, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
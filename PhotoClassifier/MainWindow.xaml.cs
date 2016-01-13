using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PhotoClassifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string _selectedFolderPath = string.Empty;
        private string _outputKeepFolderPath = string.Empty;
        private string _outputDiscardFolderPath = string.Empty;
        private List<string> _images = new List<string>();

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            var selectionOk = fbd.ShowDialog();

            if (selectionOk == System.Windows.Forms.DialogResult.OK)
            {
                SetOutputFolderPaths(fbd.SelectedPath);
                SelectedFolderTextBlock.Text = _selectedFolderPath;
                _images = GetImagesFromPath(_selectedFolderPath);
                _imagesToKeep = GetImagesFromPath(_outputKeepFolderPath);
                _imagesToDiscard = GetImagesFromPath(_outputDiscardFolderPath);
                SelectNext();
            }
            else
            {
                ResetAllPathsAndResults();
                SelectedFolderTextBlock.Text = "Select Folder...Please try again";
                DebugTextBlock.Text = "Folder Selection Failed";
            }
        }

        // see Joe Pesch http://blog.tutorem.com/post/2013/12/27/C-Auto-Rotate-Image-Based-on-Camera-Rotation-Exif-Data.aspx
        private static BitmapSource GetRotatedImage(string filename)
        {
            // Get source from: http://www.codeproject.com/KB/graphics/exifextractor.aspx?fid=207371
            var bmp = new Bitmap(filename);
            var exif = new EXIFextractor(ref bmp, "n");
            switch (int.Parse(exif["Orientation"].ToString()))
            {
                case 1:
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                    break;
                case 2:
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case 3:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 4:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
                case 5:
                    bmp.RotateFlip(RotateFlipType.Rotate90FlipX);
                    break;
                case 6:
                    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 7:
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
                    break;
                case 8:
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                default:
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                    break;
            }
            IntPtr hBitmap = bmp.GetHbitmap();
            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private List<string> GetImagesFromPath(string selectedFolderPath)
        {
            var filesFound = new List<string>();
            const SearchOption searchOption = SearchOption.TopDirectoryOnly;
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            if (Directory.Exists(selectedFolderPath))
            {
                foreach (var filter in filters)
                {
                    filesFound.AddRange(Directory.GetFiles(selectedFolderPath, $"*.{filter}", searchOption));
                }
                DebugTextBlock.Text = $"Found {filesFound.Count} files";
            }
            return filesFound;
        }

        private int _currentImageIndex = -1;

        private void SelectNext()
        {
            var fileFound = false;
            var imagesTested = 0;
            var imagesCount = _images.Count;
            while (!fileFound)
            {
                _currentImageIndex++;
                if (_currentImageIndex >= imagesCount) _currentImageIndex = 0;

                var isInKeptFiles = _imagesToKeep.Contains(CurrentImagePath);
                var isInDiscardedFiles = _imagesToDiscard.Contains(CurrentImagePath);
                fileFound = true;
                if (HideKeptFiles && isInKeptFiles) { fileFound = false;}
                else if (HideDiscardedFiles && isInDiscardedFiles) fileFound = false;

                imagesTested++;
                if (imagesTested >= imagesCount) break;
            }

            if (imagesTested >= imagesCount)
            {
                Debug.WriteLine("No Unassigned Images (+)");
                DebugTextBlock.Text = "No Unassigned Images (+)";
                ResetImage();
                return;
            }

            DebugTextBlock.Text = CurrentImagePath;
            DisplayImage.Source = GetRotatedImage(CurrentImagePath);
        }
        private void SelectPrevious()
        {
            var fileFound = false;
            var imagesTested = 0;
            var imagesCount = _images.Count;
            while (!fileFound)
            {
                _currentImageIndex--;
                if (_currentImageIndex < 0) _currentImageIndex = imagesCount - 1;

                var isInKeptFiles = _imagesToKeep.Contains(CurrentImagePath);
                var isInDiscardedFiles = _imagesToDiscard.Contains(CurrentImagePath);
                fileFound = true;
                if (HideKeptFiles && isInKeptFiles) fileFound = false;
                else if (HideDiscardedFiles && isInDiscardedFiles) fileFound = false;

                imagesTested++;
                if (imagesTested >= imagesCount) break;
            }

            if (imagesTested >= imagesCount)
            {
                Debug.WriteLine("No Unassigned Images (-)");
                DebugTextBlock.Text = "No Unassigned Images (-)";
                ResetImage();
                return;
            }

            DebugTextBlock.Text = CurrentImagePath;
            DisplayImage.Source = GetRotatedImage(CurrentImagePath);
        }

        private bool HideKeptFiles => HideKeepCheckBox.IsChecked ?? false;
        private bool HideDiscardedFiles => HideDiscardCheckBox.IsChecked ?? false;

        private string CurrentImagePath
        {
            get
            {
                if (_images == null || _currentImageIndex < 0 || _currentImageIndex >= _images.Count)
                {
                    return string.Empty;
                }
                return _images[_currentImageIndex];
            }
        }

        private void SetOutputFolderPaths(string path)
        {
            _selectedFolderPath = path;
            _outputKeepFolderPath = System.IO.Path.Combine(path, "_Keep");
            _outputDiscardFolderPath = System.IO.Path.Combine(path, "_Discard");
        }

        private void ResetAllPathsAndResults()
        {
            _selectedFolderPath = string.Empty;
            _outputKeepFolderPath = string.Empty;
            _outputDiscardFolderPath = string.Empty;

            _images.Clear();
            _currentImageIndex = -1;
            ResetImage();
            _imagesToKeep.Clear();
            _imagesToDiscard.Clear();
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

        private List<string> _imagesToKeep = new List<string>();
        private List<string> _imagesToDiscard = new List<string>();

        private void KeepCurrent()
        {
            if (!_imagesToKeep.Contains(CurrentImagePath))
            {
                Debug.WriteLine("Keep: {0}", CurrentImagePath);
                _imagesToKeep.Add(CurrentImagePath);
            }
        }

        private void DiscardCurrent()
        {
            if (!_imagesToDiscard.Contains(CurrentImagePath))
            {
                Debug.WriteLine("Discard: {0}", CurrentImagePath);
                _imagesToDiscard.Add(CurrentImagePath);
            }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var file in _imagesToKeep)
            {
                CopyFileToPath(file, _outputKeepFolderPath);
            }
            foreach (var file in _imagesToDiscard)
            {
                CopyFileToPath(file, _outputDiscardFolderPath);
            }
        }

        private void CopyFileToPath(string filename, string destinationPath)
        {
            var file = Path.GetFileName(filename);
            if (string.IsNullOrEmpty(file))
            {
                Debug.WriteLine($"Failed to write file {filename}");
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
                File.Copy(filename, output, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}

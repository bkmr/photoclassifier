using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;

namespace PhotoClassifier
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private TransformedBitmap _transformedBitmap;

        private CircularList<Image> _images;

        public TransformedBitmap TransformedBitmap
        {
            get { return _transformedBitmap; }
            set
            {
                if (Equals(value, _transformedBitmap)) return;
                _transformedBitmap = value;
                OnPropertyChanged();
            }
        }


        public Image CurrentImage
        {
            get
            {
                if (_images == null || _images.Count == 0 || _images.Current == null) return new Image("Invalid");
                return _images.Current;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetFiles(IList<string> files)
        {
            _images = new CircularList<Image>(files.Select(file => new Image(file)).ToList());
            TransformedBitmap = GetTransformedBitmapFromUri(_images.Current.Uri);
        }

        public void MoveNextImage()
        {
            if (_images == null || _images.Count == 0) return;

            _images.MoveNext();
            TransformedBitmap = GetTransformedBitmapFromUri(_images.Current.Uri);
        }

        public void MovePreviousImage()
        {
            if (_images == null || _images.Count == 0) return;

            _images.MovePrevious();
            TransformedBitmap = GetTransformedBitmapFromUri(_images.Current.Uri);
        }

        private TransformedBitmap GetTransformedBitmapFromUri(string uri)
        {
            var bitmap = new Bitmap(uri);
            var requiredRotation = GetRotationFromImage(bitmap);
            var bitmapImage = new BitmapImage(new Uri(uri));
            var transform = new RotateTransform(requiredRotation);
            var tb = new TransformedBitmap(bitmapImage, transform);
            return tb;
        }

        private int GetRotationFromImage(Bitmap bitmap)
        {
            var exif = new EXIFextractor(ref bitmap, "n");
            switch (int.Parse(exif["Orientation"].ToString()))
            {
                case 3:
                    return 180;
                case 6:
                    return 90;
                case 8:
                    return 270;
                default:
                    return 0;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<Image> ImagesToKeep
        {
            get
            {
                return _images?.Where(i => i.Action == Image.ImageAction.Keep) ?? new List<Image>();
            }
        }

        public IEnumerable<Image> ImagesToDiscard
        {
            get
            {
                return _images?.Where(i => i.Action == Image.ImageAction.Discard) ?? new List<Image>();
            }
        }

        public void Discard()
        {
            CurrentImage.Action = Image.ImageAction.Discard;
        }

        public void Keep()
        {
            CurrentImage.Action = Image.ImageAction.Keep;
        }
    }

    public class Image
    {
        public enum ImageAction
        {
            None,
            Keep,
            Discard
        }

        public Image(string uri)
        {
            Uri = uri;
            Action = ImageAction.None;
        }

        public ImageAction Action { get; set; }

        public string Uri { get; }
    }
}
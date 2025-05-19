using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace MMG_IIA
{
    /// <summary>
    /// ViewModel class for the image viewer.
    /// This class implements INotifyPropertyChanged to notify the view of property changes.
    /// It contains properties for the image, its height, and width.
    /// </summary>
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; // Event to notify property changes

        /// <summary>
        /// Notifies the view of property changes.
        /// This method is called whenever a property value changes.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private WriteableBitmap img; // The image to be displayed in the viewer
        /// <summary>
        /// Gets or sets the image to be displayed in the viewer.
        /// This property raises the PropertyChanged event when the value changes.
        /// </summary>
        public WriteableBitmap Img
        {
            get
            {
                return img;
            }

            set
            {
                img = value;
                NotifyPropertyChanged();
            }
        }

        private double imgHeight; // The height of the image
        /// <summary>
        /// Gets or sets the height of the image.
        /// This property raises the PropertyChanged event when the value changes.
        /// </summary>
        public double ImgHeight
        {
            get
            {
                return imgHeight;
            }

            set
            {
                imgHeight = value;
                NotifyPropertyChanged();
            }
        }

        private double imgWidth; // The width of the image
        /// <summary>
        /// Gets or sets the width of the image.
        /// This property raises the PropertyChanged event when the value changes.
        /// </summary>
        public double ImgWidth
        {
            get
            {
                return imgWidth;
            }

            set
            {
                imgWidth = value;
                NotifyPropertyChanged();
            }
        }
    }
}

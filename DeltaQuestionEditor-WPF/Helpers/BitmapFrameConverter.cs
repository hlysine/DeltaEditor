using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DeltaQuestionEditor_WPF.Helpers
{
    // Adapted from https://archive.codeplex.com/?p=intuipic#114513

    /// <summary>
    /// This converter facilitates a couple of requirements around images. Firstly, it automatically disposes of image streams as soon as images
    /// are loaded, thus avoiding file access exceptions when attempting to delete images.
    /// </summary>
    public sealed class BitmapFrameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = value as string;

            if (path != null)
            {
                try
                {
                    //create new stream and create bitmap frame
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
                    //load the image now so we can immediately dispose of the stream
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    //clean up the stream to avoid file access exceptions when attempting to delete images
                    bitmapImage.StreamSource.Dispose();

                    return bitmapImage;
                }
                catch (NotSupportedException)
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

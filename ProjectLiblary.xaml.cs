using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VideoEditor.Utils;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VideoEditor
{
    public sealed partial class ProjectLiblary : UserControl
    {
        public List<IStorageFile> mediaFiles = new List<IStorageFile>();
        public CompositionController compositionCtrl;

        public ProjectLiblary()
        {
            this.InitializeComponent();
        }

        private async void Update()
        {
            Liblary.Items.Clear();
            foreach (StorageFile file in mediaFiles)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap 
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.DecodePixelWidth = 100; //match the target Image.Width, not shown
                    await bitmapImage.SetSourceAsync(fileStream);

                    Image imgView = new Image();
                    imgView.Width = 100;
                    imgView.Height = 100;
                    Liblary.Items.Add(imgView);
                    imgView.Source = bitmapImage;
                }
            }
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var files = await picker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                mediaFiles.AddRange(files.ToList());
            }
            Update();
        }

        private void ToTimeLine_Click(object sender, RoutedEventArgs e)
        {
            var range = Liblary.SelectedRanges;
            if(range.Count > 0)
            {
                List<IStorageFile> selected = new List<IStorageFile>();
                foreach (var i in range)
                {
                    selected.AddRange(mediaFiles.GetRange(i.FirstIndex, i.LastIndex - i.FirstIndex + 1));
                }
                compositionCtrl.AddPhotos(selected); 
            }
        }
    }
}

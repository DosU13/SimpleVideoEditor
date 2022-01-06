using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace VideoEditor
{
    public sealed partial class TimeLine : UserControl
    {
        private CompositionController compositionCrtl;

        public TimeLine()
        {
            this.InitializeComponent();
        }

        public void setCompCtrl(CompositionController cmpCtrl)
        {
            compositionCrtl = cmpCtrl;
            compositionCrtl.ChangeComp += delegate (object sender, EventArgs arg)
            {
                UpdateTimeLineAsync();
            };
        }

        //public ObservableCollection<Image> Items { get; set; } = new ObservableCollection<Image>();
        private async void UpdateTimeLineAsync()
        {
            try
            {
                TimelineGrid.Items.Clear();
                //Items.Clear();
                foreach (IStorageFile file in compositionCrtl.photoFiles)
                {
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.DecodePixelHeight = 150; //match the target Image.Height, not shown
                        await bitmapImage.SetSourceAsync(fileStream);

                        Image imgView = new Image();
                        imgView.Width = 150;
                        imgView.Height = 150;
                        TimelineGrid.Items.Add(imgView);
                        //Items.Add(imgView);
                        imgView.Source = bitmapImage;
                    }
                }
            } catch (Exception ex) { compositionCrtl.status.Text = "Exception in Time line: " + ex.Message; }
        }


        //object m_ReorderItem;
        //int m_ReorderIndexFrom;
        //void Items_CollectionChanged(object s, NotifyCollectionChangedEventArgs e)
        //{
        //    switch (e.Action)
        //    {
        //        case NotifyCollectionChangedAction.Remove:
        //            m_ReorderItem = e.OldItems[0];
        //            m_ReorderIndexFrom = e.OldStartingIndex;
        //            break;
        //        case NotifyCollectionChangedAction.Add:
        //            if (m_ReorderItem == null) return;
        //            var _ReorderIndexTo = e.NewStartingIndex;
        //            HandleReorder(m_ReorderItem, m_ReorderIndexFrom, _ReorderIndexTo);
        //            m_ReorderItem = null;
        //            break;
        //    }
        //}
        //
        //void HandleReorder(object item, int indexFrom, int indexTo)
        //{
        //
        //}

        private void AddClip(object sender, RoutedEventArgs e)
        {
            _ = PickFileAndAddClip();
        }

        private async Task PickFileAndAddClip()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            StorageFile pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile == null) return;

            // These files could be picked from a location that we won't have access to later
            var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            storageItemAccessList.Add(pickedFile);

            var clip = await MediaClip.CreateFromFileAsync(pickedFile);

            compositionCrtl.AddClip(clip);
        }

        private void MoveLeftPressed(object sender, RoutedEventArgs e)
        {
            int i = TimelineGrid.SelectedIndex;
            ItemCollection tl = TimelineGrid.Items;
            List<IStorageFile> pf = compositionCrtl.photoFiles;
            if (i == 0) return;
            try {
                Image img = tl[i-1] as Image;
                tl.RemoveAt(i-1);
                tl.Insert(i, img);
                swap(pf, i - 1);
            } catch (Exception ex) { compositionCrtl.status.Text = "Exception in Moving in timeline: " + ex.Message; }
            compositionCrtl.Update();
        }

        private void MoveRightPressed(object sender, RoutedEventArgs e)
        {
            int i = TimelineGrid.SelectedIndex;
            ItemCollection tl = TimelineGrid.Items;
            List<IStorageFile> pf = compositionCrtl.photoFiles;
            if (i >= tl.Count - 1) return;
            try
            {
                Image img = tl[i+1] as Image;
                tl.RemoveAt(i+1);
                tl.Insert(i, img);
                swap(pf, i);
                (pf[i], pf[i + 1]) = (pf[i + 1], pf[i]);
            } catch (Exception ex) { compositionCrtl.status.Text = "Exception in Moving in timeline: " + ex.Message; }
            compositionCrtl.Update();
        }

        private void swap<T>(List<T> col, int i){
            T temp = col[i];
            col[i] = col[i + 1];
            col[i + 1] = temp;
        }

        private void DeletePressed(object sender, RoutedEventArgs e)
        {
            int selectedInd = TimelineGrid.SelectedIndex;
            compositionCrtl.photoFiles.RemoveAt(selectedInd);
            TimelineGrid.Items.RemoveAt(selectedInd);
            compositionCrtl.Update();
        }

        private void OnSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            double[] durs = compositionCrtl.durations;
            int selectedInd = TimelineGrid.SelectedIndex;
            double sum = 0;
            for(int i=0; i<selectedInd; i++)
            {
                if (i < durs.Length) sum += durs[i];
                else sum += compositionCrtl.defaultDur;
            }
            try
            {
                compositionCrtl.mediaPlayerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(sum);
            }catch(Exception ex) { compositionCrtl.status.Text = "Exception seeking: " + ex.Message; }
        }

        private void FinishPresed(object sender, RoutedEventArgs e)
        {
            compositionCrtl.RenderCompositionToFile();
        }

        //private void TimlineOnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        //{
        //    if (!e.Items.Any()) return;
        //    var _DataItem = e.Items.First();
        //    e.Data.Properties.Add("sourceItem", _DataItem);
        //    e.Data.Properties.Add("sourceGrid", sender as GridView);
        //}

        //private void TimelineOnDrop(object sender, DragEventArgs e)
        //{
        //    object _GridViewObject;
        //    if (!e.Data.Properties.TryGetValue("sourceGrid", out _GridViewObject)) return;
        //    var _GridView = _GridViewObject as GridView;
        //    if (_GridView == null || _GridView != sender) return;
        //    object _DataItemObject;
        //    if (!e.Data.Properties.TryGetValue("sourceItem", out _DataItemObject)) return;
        //    var _DataItem = _DataItemObject as Image;
        //    if (_DataItem == null) return;

        //    var _SourceList = _GridView.ItemsSource as ObservableCollection<Image>;
        //    _SourceList.Remove(_DataItem);

        //    var _Target = sender as GridView;
        //    var _TargetList = _Target.ItemsSource as ObservableCollection<Image>;
        //    _TargetList.Add(_DataItem);
        //}
    }
}

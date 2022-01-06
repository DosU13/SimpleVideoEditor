using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace VideoEditor
{
    public class CompositionController
    {
        public MediaComposition composition = new MediaComposition();
        public List<IStorageFile> photoFiles = new List<IStorageFile>();
        public event EventHandler ChangeRender;
        public event EventHandler ChangeComp;

        public CompositionController()
        {
            ChangeComp += delegate (object sender, EventArgs arg)
            {
                Update();
            };
        }

        public async void Update()
        {
            try
            {
                composition.Clips.Clear();
                for (int i = 0; i < photoFiles.Count; i++)
                {
                    double t = (i < durations.Count()) ? durations[i] : defaultDur;
                    MediaClip clip = await MediaClip.CreateFromImageFileAsync(photoFiles[i], TimeSpan.FromSeconds(t));
                    composition.Clips.Add(clip);
                }
                Render();
            }catch (Exception ex) { status.Text = "Exception in composition controller: " + ex.Message; }
        }

        public void Render()
        {
            ChangeRender?.Invoke(this, new EventArgs() { });
        }

        public void OnChange()
        {
            ChangeComp?.Invoke(this, new EventArgs() { });
        }

        public async Task SetMusicAsync(StorageFile audioFile)
        {
            // These files could be picked from a location that we won't have access to later
            var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            storageItemAccessList.Add(audioFile);

            var backgroundTrack = await BackgroundAudioTrack.CreateFromFileAsync(audioFile);

            composition.BackgroundAudioTracks.Add(backgroundTrack);
            OnChange();
        }

        public void AddClip(MediaClip clip)
        {
            composition.Clips.Add(clip);
            //OnChange();
        }

        internal void AddPhotos(List<IStorageFile> files)
        {
            photoFiles.AddRange(files);
            OnChange();
        }

        public double[] durations = { };
        public double defaultDur = 5;
        internal void SetDurations(double[] durations)
        {
            this.durations = durations;
            OnChange();
        }
        internal void SetDefaultDur(double defaultDur)
        {
            this.defaultDur = defaultDur;
            OnChange();
        }


        public MediaPlayerElement mediaPlayerElement;
        public TextBlock status;

        public CoreDispatcher Dispatcher { get; private set; } = null;

        internal async void RenderCompositionToFile()
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeChoices.Add("MP4 files", new List<string>() { ".mp4" });
            picker.SuggestedFileName = "RenderedComposition.mp4";

            try
            {
                if (Dispatcher == null) Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

                Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    // Call RenderToFileAsync
                    var saveOperation = composition.RenderToFileAsync(file, MediaTrimmingPreference.Precise);

                    saveOperation.Progress = new AsyncOperationProgressHandler<TranscodeFailureReason, double>(async (info, progress) =>
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                        {
                            this.status.Text = (string.Format("Saving file... Progress: {0:F0}%", progress));
                        }));
                    });
                    saveOperation.Completed = new AsyncOperationWithProgressCompletedHandler<TranscodeFailureReason, double>(async (info, status) =>
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                        {
                            try
                            {
                                var results = info.GetResults();
                                if (results != TranscodeFailureReason.None || status != AsyncStatus.Completed)
                                {
                                    this.status.Text = ("Saving was unsuccessful");
                                }
                                else
                                {
                                    this.status.Text = ("Trimmed clip saved to file");
                                }
                            }
                            finally
                            {
                            // Update UI whether the operation succeeded or not
                        }

                        }));
                    });
                }
                else
                {
                    this.status.Text = ("User cancelled the file selection");
                }
            }catch(Exception ex) { status.Text = "Exception in rendering to file: " + ex.Message; }
        }
    }
}

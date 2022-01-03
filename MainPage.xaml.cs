using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VideoEditor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public CompositionController compositionCrtl = new CompositionController();

        public MainPage()
        {
            this.InitializeComponent();
            iTimeLine.compositionCrtl = compositionCrtl;
            iProjectLiblary.compositionCtrl = compositionCrtl;
            compositionCrtl.Change += delegate (object sender, EventArgs arg)
            {
                UpdateMediaElementSource();
            };
        }

        private MediaStreamSource mediaStreamSource;
        public void UpdateMediaElementSource()
        {
            mediaStreamSource = compositionCrtl.composition.GeneratePreviewMediaStreamSource(
                (int)mediaPlayerElement.ActualWidth,
                (int)mediaPlayerElement.ActualHeight);
            mediaPlayerElement.Source = MediaSource.CreateFromMediaStreamSource(mediaStreamSource);
        }
    }
}

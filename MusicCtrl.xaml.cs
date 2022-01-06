using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Editing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VideoEditor
{
    public sealed partial class MusicCtrl : UserControl
    {
        public CompositionController compositionCrtl;
        public MusicCtrl()
        {
            this.InitializeComponent();
        }

        private void SelectMusic_Click(object sender, RoutedEventArgs e)
        {
            AddBackgroundAudioTrack();
        }

        private async void AddBackgroundAudioTrack()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".flac");
            Windows.Storage.StorageFile audioFile = await picker.PickSingleFileAsync();
            if(audioFile != null) _ = compositionCrtl.SetMusicAsync(audioFile);
        }

        //private void Timings_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (!(sender is TextBox txt)) return;
        //    int pos = txt.SelectionStart;
        //    txt.Text = string.Concat(txt.Text.Where(c => "0123456789.\r\n".IndexOf(c) != -1));
        //    txt.SelectionStart = pos;
        //}

        private void DefaultDurOnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                double defaultDur = double.Parse(DefaultDurTxtBox.Text);
                compositionCrtl.SetDefaultDur(defaultDur);
            } catch (Exception) { compositionCrtl.status.Text = "Exception in music controller"; }
        }

        private void DurationsOnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string txt = Durations.Text;
                string[] lines = txt.Split('\r');
                double[] durs = Array.ConvertAll(lines, double.Parse);
                double[] starts = DursToStarts(durs);
                compositionCrtl.SetDurations(durs);
                Durations.Text = string.Join('\r', durs);
                Starts.Text = string.Join('\r', starts);
            }
            catch (Exception) { compositionCrtl.status.Text = "Exception in Music Controller"; }
        }

        private void StartsOnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string txt = Starts.Text;
                string[] lines = txt.Split('\r');
                double[] starts = Array.ConvertAll(lines, double.Parse);
                double[] durs = StartsToDurs(starts);
                compositionCrtl.SetDurations(durs);
                Durations.Text = string.Join('\r', durs);
                Starts.Text = string.Join('\r', starts);
            }
            catch (Exception) { compositionCrtl.status.Text = "Exception in Music Controller"; }
        }

        private double[] DursToStarts(double[] durs)
        {
            double temp = 0;
            double[] res = new double[durs.Length];
            for (int i=0; i<durs.Length; i++)
            {
                temp += durs[i];
                res[i] = temp;
            }
            return res;
        }

        private double[] StartsToDurs(double[] starts)
        {
            double[] res = new double[starts.Length];
            res[0] = starts[0];
            for (int i = 1; i < starts.Length; i++)
            {
                res[i] = starts[i] - starts[i-1];
            }
            return res;
        }
    }
}

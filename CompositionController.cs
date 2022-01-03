using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;

namespace VideoEditor
{
    public class CompositionController
    {
        public MediaComposition composition = new MediaComposition();
        public event EventHandler Change;


        public void OnChange()
        {
            if (this.Change != null)
            {
                this.Change(this, new EventArgs() { });
            }
        }

        public void addClip(MediaClip clip)
        {
            composition.Clips.Add(clip);
            this.OnChange();
        }

        internal async void addFiles(ArrayList files)
        {
            foreach(IStorageFile file in files)
            {
                var clip = await MediaClip.CreateFromImageFileAsync(file, new TimeSpan(30000000));
                composition.Clips.Add(clip);
            }
            OnChange();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditor.Utils
{
    public class ListenableArray : ArrayList //<Windows.Storage.StorageFile>
    {
        public event EventHandler Change;

        public override int Add(object value)
        {
            var result = base.Add(value);
            this.OnChange();
            return result;
        }

        public override void AddRange(ICollection c)
        {
            base.AddRange(c);
            this.OnChange();
        }

        public override void Remove(object obj)
        {
            base.Remove(obj);
            this.OnChange();
        }

        protected void OnChange()
        {
            if (this.Change != null)
            {
                this.Change(this, new EventArgs() { });
            }
        }
    }
}

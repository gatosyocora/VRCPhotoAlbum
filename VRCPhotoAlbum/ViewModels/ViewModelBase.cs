using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged,IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Collection<IDisposable> disposes = new Collection<IDisposable>();

        public void Dispose()
        {
            foreach (var dispose in disposes)
            {
                dispose.Dispose();
            }
        }
    }
}

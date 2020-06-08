using System;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}

using System;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class ModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disposable.Dispose();
            }
        }

        ~ModelBase()
        {
            Dispose(false);
        }
    }
}

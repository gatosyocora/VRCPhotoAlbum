using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    public class WindowBase : MetroWindow, IDisposable
    {
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

        ~WindowBase()
        {
            Dispose(false);
        }
    }
}

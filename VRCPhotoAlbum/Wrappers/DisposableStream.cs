using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gatosyocora.VRCPhotoAlbum.Wrappers
{
    // https://pierre3.hatenablog.com/entry/2015/10/25/001207
    public class DisposableStream : Stream
    {
        private Stream _streamBase;

        public DisposableStream(Stream streamBase)
        {
            if (streamBase is null)
            {
                throw new ArgumentNullException("streamBase");
            }
            _streamBase = streamBase;
        }

        public override bool CanRead => _streamBase.CanRead;
        public override bool CanSeek => _streamBase.CanSeek;
        public override bool CanWrite => _streamBase.CanWrite;
        public override long Length => _streamBase.Length;
        public override long Position { get => _streamBase.Position; set => _streamBase.Position = value; }
        public override void Flush() => _streamBase.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _streamBase.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _streamBase.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public new Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return _streamBase.ReadAsync(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) => _streamBase.Seek(offset, origin);
        public override void SetLength(long value) => _streamBase.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _streamBase.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _streamBase.Dispose();
                _streamBase = null;
            }
            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (_streamBase is null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}

using System;
using System.Linq;

namespace MusicPlayer.Presentation.ViewModels
{
    public class SongViewModel : IDisposable
    {
        private TagLib.File _tagFile;
        private string _path;

        public SongViewModel(TagLib.File tagFile, string path)
        {
            if (tagFile == null)
            {
                throw new ArgumentNullException("tagFile");
            }

            if (!tagFile.Properties.Codecs.Any(a => a is TagLib.IAudioCodec))
            {
                throw new ArgumentException("File must be an audio file");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path must not be missing");
            }

            _tagFile = tagFile;
            _path = path;
        }

        public string Title { get { return _tagFile.Tag.Title; } }

        public string Artist { get { return _tagFile.Tag.FirstPerformer; } }

        public string Album { get { return _tagFile.Tag.Album; } }

        public TimeSpan Length { get { return _tagFile.Properties.Duration; } }

        public int BitRate { get { return _tagFile.Properties.AudioBitrate; } }

        public string Path { get { return _path; } }

        #region IDisposable

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                _tagFile.Dispose();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
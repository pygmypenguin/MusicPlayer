using NAudio.Wave;
using System;
using System.IO;

namespace MusicPlayer.Playback
{
    public class SongStream : IDisposable
    {
        private IWavePlayer _player;
        private AudioFileReader _reader;

        private int _bitRate;

        public event EventHandler SongStopped;

        public SongStream(string path, int bitRate)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            if (!File.Exists(path))
            {
                throw new ArgumentException("Cannot play file that does not exist!");
            }

            Path = path;

            _reader = new AudioFileReader(path);
            _bitRate = bitRate;

            _player = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _player.PlaybackStopped += Player_SongStopped;
            _player.Init(_reader);
        }

        public string Path { get; private set; }

        public float Volume
        {
            get { return _reader.Volume; }
            set
            {
                _reader.Volume = value;
            }
        }

        public double SongLength
        {
            get { return _reader.TotalTime.TotalSeconds; }
        }

        public double SongPosition
        {
            get { return _reader.CurrentTime.TotalSeconds; }
        }

        public void Play()
        {
            _player.Play();
        }

        public void Stop()
        {
            _player.Stop();
            _reader.Position = 0;
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Seek(double positionSeconds)
        {
            long bitPosition = 16 * _bitRate * (long)positionSeconds;

            if (bitPosition > _reader.Length)
            {
                return;
            }
            _reader.CurrentTime = TimeSpan.FromSeconds(positionSeconds);
        }

        private void Player_SongStopped(object sender, StoppedEventArgs e)
        {
            OnSongStopped();
        }

        private void OnSongStopped()
        {
            var tmp = SongStopped;
            if (tmp != null)
            {
                tmp(this, EventArgs.Empty);
            }
        }

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
                _player.Stop();
                _player.Dispose();

                _reader.Dispose();

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
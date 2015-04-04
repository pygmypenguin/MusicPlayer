using NAudio.Wave;
using System;
using System.IO;

namespace MusicPlayer.Playback
{
    public class SongStream : IDisposable
    {
        private IWavePlayer _player;
        private MediaFoundationReader _reader;
        private WaveChannel32 _channel;

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
            _bitRate = bitRate;

            _reader = new MediaFoundationReader(path);
            _channel = new WaveChannel32(_reader);

            _player = new WaveOutEvent();
            _player.PlaybackStopped += Player_SongStopped;
            _player.Init(_channel);
        }

        public string Path { get; private set; }

        public float Volume
        {
            get { return _channel.Volume; }
            set
            {
                _channel.Volume = value;
            }
        }

        public double SongLength
        {
            get { return _channel.TotalTime.TotalSeconds; }
        }

        public double SongPosition
        {
            get { return _channel.CurrentTime.TotalSeconds; }
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

            if (bitPosition > _channel.Length)
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

                _channel.Dispose();

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
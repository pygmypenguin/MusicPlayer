using MusicPlayer.Infrastructure;
using MusicPlayer.Playback;
using MusicPlayer.Presentation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows.Input;

namespace MusicPlayer.Presentation.ViewModels
{
    public class PlaybackViewModel : INotifyPropertyChanged, IDisposable
    {
        private SongStream _currentSong;

        private Timer _playbackTimer;

        private SongPicker _songPicker;

        public PlaybackViewModel(IList<SongViewModel> seedList)
        {
            Volume = 100.0f;
            PlayPauseState = PlayPauseState.Stop;
            _playbackTimer = new Timer(500);
            _playbackTimer.Elapsed += PlaybackTimer_Elapsed;

            SongLength = 1;
            SongPosition = 0;

            LoadPlaylist(seedList);
        }

        private PlayPauseState _playPauseState;

        public PlayPauseState PlayPauseState
        {
            get { return _playPauseState; }
            set
            {
                _playPauseState = value;
                OnPropertyChanged("PlayPauseState");
            }
        }

        public Command _playPauseCommand;

        public ICommand PlayPauseCommand
        {
            get
            {
                if (_playPauseCommand == null)
                {
                    _playPauseCommand = new Command(PlayPauseExecute);
                }
                return _playPauseCommand;
            }
        }

        private Command _skipCommand;

        public ICommand SkipCommand
        {
            get
            {
                if (_skipCommand == null)
                {
                    _skipCommand = new Command(PlayNextSongInList);
                }
                return _skipCommand;
            }
        }

        public bool IsShuffle
        {
            get { return _songPicker.ShuffleMode == ShuffleMode.Shuffle; }
            set
            {
                _songPicker.ShuffleMode = value ? ShuffleMode.Shuffle : ShuffleMode.Next;
                OnPropertyChanged("IsShuffle");
            }
        }

        private float _volume;

        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (_currentSong != null)
                {
                    _currentSong.Volume = value / 100.0f;
                }
                OnPropertyChanged("Volume");
            }
        }

        private double _songLength;

        public double SongLength
        {
            get { return _songLength; }
            private set
            {
                _songLength = value;
                OnPropertyChanged("SongLength");
            }
        }

        private TimeSpan _songTimeSpan;

        public TimeSpan SongTimeSpan
        {
            get { return _songTimeSpan; }
            private set
            {
                _songTimeSpan = value;
                OnPropertyChanged("SongTimeSpan");
            }
        }

        private TimeSpan _songPositionSpan;

        public TimeSpan SongPositionSpan
        {
            get { return _songPositionSpan; }
            set
            {
                _songPositionSpan = value;
                OnPropertyChanged("SongPositionSpan");
            }
        }

        private double _songPosition;

        public double SongPosition
        {
            get { return _songPosition; }
            private set
            {
                _songPosition = value;
                OnPropertyChanged("SongPosition");
            }
        }

        private string _nowPlaying;

        public string NowPlaying
        {
            get { return _nowPlaying; }
            private set
            {
                _nowPlaying = value;
                OnPropertyChanged("NowPlaying");
            }
        }

        public void LoadPlaylist(IList<SongViewModel> playlist)
        {
            if (_songPicker == null)
            {
                _songPicker = new SongPicker(playlist);
            }
            else
            {
                _songPicker.LoadNewPlaylist(playlist);
            }
        }

        public void PlaySongImmediately(SongViewModel song)
        {
            if (_songPicker != null)
            {
                _songPicker.ResetPlaylistStatus(song);
            }

            PlaySong(song);
        }

        public void PlayNextSongInList()
        {
            if (_songPicker != null)
            {
                var next = _songPicker.GetNextSong();
                if (next != null)
                {
                    if (PlayPauseState == Infrastructure.PlayPauseState.Pause)
                    {
                        QueueSong(next);
                    }
                    else
                    {
                        PlaySong(next);
                    }
                }
                else
                {
                    EndCurrentSong();
                    PlayPauseState = Infrastructure.PlayPauseState.Stop;
                }
            }
        }

        public void RemoveSongFromPlaylist(SongViewModel song)
        {
            if (_currentSong != null && _currentSong.Path == song.Path)
            {
                EndCurrentSong();
            }

            _songPicker.RemoveSongFromPlaylist(song);
        }

        public void Seek(double songFraction)
        {
            if (_currentSong != null)
            {
                var seconds = SongLength * songFraction;
                _currentSong.Seek(seconds);
            }
        }

        private void PlaySong(SongViewModel song)
        {
            QueueSong(song);

            PlayPauseState = Infrastructure.PlayPauseState.Play;

            _playbackTimer.Start();
            _currentSong.Play();
        }

        private void QueueSong(SongViewModel song)
        {
            EndCurrentSong();
            SongStream newSong = new SongStream(song.Path, song.BitRate);
            _currentSong = newSong;
            _currentSong.Volume = Volume / 100.0f;
            _currentSong.SongStopped += CurrentSong_Stopped;

            NowPlaying = string.Format("{0} - {1}", song.Title, song.Artist);
            SongLength = _currentSong.SongLength;
            SongTimeSpan = song.Length;
        }

        private void EndCurrentSong()
        {
            if (_currentSong != null)
            {
                _currentSong.SongStopped -= CurrentSong_Stopped;
                _currentSong.Dispose();
                _currentSong = null;

                _playbackTimer.Stop();

                NowPlaying = string.Empty;
                SongLength = 1;
                SongPosition = 0;
                SongTimeSpan = TimeSpan.FromSeconds(0);
                SongPositionSpan = TimeSpan.FromSeconds(0);
            }
        }

        private void CurrentSong_Stopped(object sender, EventArgs e)
        {
            PlayPauseState = Infrastructure.PlayPauseState.Stop;
            EndCurrentSong();
            if (_songPicker != null)
            {
                if (_songPicker.IsPlaylistEnded)
                {
                    _songPicker.ResetPlaylistStatus(null);
                }
                else
                {
                    PlayNextSongInList();
                }
            }
        }

        private void PlayPauseExecute()
        {
            if (PlayPauseState == PlayPauseState.Play)
            {
                if (_currentSong != null)
                {
                    PlayPauseState = Infrastructure.PlayPauseState.Pause;
                    _playbackTimer.Stop();
                    _currentSong.Pause();
                }
            }
            else
            {
                if (_currentSong != null)
                {
                    PlayPauseState = Infrastructure.PlayPauseState.Play;
                    _playbackTimer.Start();
                    _currentSong.Play();
                }
            }
        }

        private void PlaybackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_currentSong != null)
            {
                SongPosition = _currentSong.SongPosition;
                SongPositionSpan = TimeSpan.FromSeconds(_currentSong.SongPosition);
            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            var tmp = PropertyChanged;
            if (tmp != null)
            {
                tmp(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion PropertyChanged

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
                if (_currentSong != null)
                {
                    _songPicker = null;

                    _currentSong.Dispose();
                    _currentSong = null;

                    _playbackTimer.Elapsed -= PlaybackTimer_Elapsed;
                    _playbackTimer.Dispose();
                }

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
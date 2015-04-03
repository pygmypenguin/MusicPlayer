using MusicPlayer.Infrastructure;
using MusicPlayer.Presentation.ViewModels;
using System;
using System.Collections.Generic;

namespace MusicPlayer.Presentation.Models
{
    public class SongPicker
    {
        private HashSet<int> _playedIndices;
        private int _currentIndex;
        private Random _rand;

        private IList<SongViewModel> _playlist;

        public SongPicker(IList<SongViewModel> playlist)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            _playlist = playlist;
            _currentIndex = -1;
            _playedIndices = new HashSet<int>();
            _rand = new Random();

            ShuffleMode = Infrastructure.ShuffleMode.Next;
        }

        public ShuffleMode ShuffleMode { get; set; }

        public bool IsPlaylistEnded
        {
            get
            {
                if (ShuffleMode == ShuffleMode.Shuffle)
                {
                    return _playedIndices.Count == _playlist.Count;
                }
                else
                {
                    return _currentIndex == _playlist.Count - 1;
                }
            }
        }

        public SongViewModel GetNextSong()
        {
            if (IsPlaylistEnded)
            {
                return null;
            }

            int index = GetNextIndex();
            _playedIndices.Add(index);
            _currentIndex = index;

            return _playlist[index];
        }

        public void LoadNewPlaylist(IList<SongViewModel> playList)
        {
            ResetPlaylistStatus(null);

            _playlist = playList;
        }

        public void ResetPlaylistStatus(SongViewModel nowPlaying)
        {
            int newIndex = -1;
            _playedIndices.Clear();
            if (nowPlaying != null)
            {
                if (_playlist.Contains(nowPlaying))
                {
                    newIndex = _playlist.IndexOf(nowPlaying);
                    _playedIndices.Add(newIndex);
                    _currentIndex = newIndex;
                }
            }
        }

        public void RemoveSongFromPlaylist(SongViewModel song)
        {
            if (_playlist.Contains(song))
            {
                _playedIndices.Remove(_playlist.IndexOf(song));
                _playlist.Remove(song);
            }
        }

        private int GetNextIndex()
        {
            if (ShuffleMode == Infrastructure.ShuffleMode.Shuffle)
            {
                int index = _currentIndex;
                List<int> validIndices = new List<int>();
                for (int i = 0; i < _playlist.Count; i++)
                {
                    if (!_playedIndices.Contains(i))
                    {
                        validIndices.Add(i);
                    }
                }

                int pick = _rand.Next(0, validIndices.Count - 1);

                return validIndices[pick];
            }
            else
            {
                return ++_currentIndex;
            }
        }
    }
}
using MusicPlayer.Library;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MusicPlayer.Presentation.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private LibraryController _libraryController;

        public MainWindowViewModel()
        {
            _libraryController = new LibraryController();
            Library = new ObservableCollection<SongViewModel>();

            LoadFromLibrary();
        }

        public ObservableCollection<SongViewModel> Library { get; private set; }

        public PlaybackViewModel PlaybackViewModel { get; private set; }

        public void AddSong(string path, TagLib.File tagFile)
        {
            if (Library.Any(a => a.Path == path))
            {
                return;
            }
            SongViewModel newSong = null;
            try
            {
                newSong = new SongViewModel(tagFile, path);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            Library.Add(newSong);
            _libraryController.AddSongFile(path, tagFile);
        }

        public void DeleteSong(SongViewModel song)
        {
            Library.Remove(song);
            _libraryController.RemoveSong(song.Path);
            song.Dispose();
        }

        private void LoadFromLibrary()
        {
            foreach (var tagFile in _libraryController.RetrieveLibrary(Infrastructure.SortPath.Artist))
            {
                AddSong(tagFile.Key, tagFile.Value);
            }

            PlaybackViewModel = new PlaybackViewModel(Library);
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var tmp = PropertyChanged;
            if (tmp != null)
            {
                tmp(this, new PropertyChangedEventArgs(propertyName));
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
                foreach (var song in Library)
                {
                    song.Dispose();
                }
                Library.Clear();

                _libraryController.Dispose();

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
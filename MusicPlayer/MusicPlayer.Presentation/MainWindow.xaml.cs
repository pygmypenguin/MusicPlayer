using MusicPlayer.Presentation.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicPlayer.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel
        {
            get { return this.DataContext as MainWindowViewModel; }
            set { this.DataContext = value; }
        }

        public MainWindow()
        {
            Closing += OnClosing;
            ViewModel = new MainWindowViewModel();
            InitializeComponent();
        }

        private void LibraryList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string path in files)
                {
                    TagLib.File tagFile = TagLib.File.Create(path);

                    ViewModel.AddSong(path, tagFile);
                }
            }
        }

        private void LibraryList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                List<SongViewModel> toRemove = new List<SongViewModel>();
                foreach (var item in LibraryList.SelectedItems)
                {
                    SongViewModel song = item as SongViewModel;
                    if (song != null)
                    {
                        toRemove.Add(song);
                    }
                }

                foreach (var song in toRemove)
                {
                    PlaybackControl.ViewModel.RemoveSongFromPlaylist(song);
                    ViewModel.DeleteSong(song);
                }
            }
        }

        private void Song_DoubleClick(object sender, MouseEventArgs e)
        {
            var row = sender as DataGridRow;
            var song = row.DataContext as SongViewModel;
            if (song != null)
            {
                PlaybackControl.ViewModel.PlaySongImmediately(song);
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            PlaybackControl.Dispose();
            ViewModel.Dispose();
        }
    }
}
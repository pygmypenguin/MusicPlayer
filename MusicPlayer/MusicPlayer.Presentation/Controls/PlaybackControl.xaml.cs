using MusicPlayer.Presentation.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicPlayer.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for PlaybackControl.xaml
    /// </summary>
    public partial class PlaybackControl : UserControl, IDisposable
    {
        public PlaybackViewModel ViewModel
        {
            get { return DataContext as PlaybackViewModel; }
            private set { DataContext = value; }
        }

        public PlaybackControl()
        {
            InitializeComponent();
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
                ViewModel.Dispose();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        private void SongProgress_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var bar = sender as ProgressBar;
            if (bar != null)
            {
                var mouse = Mouse.GetPosition(bar);
                var fraction = mouse.X / bar.ActualWidth;

                ViewModel.Seek(fraction);
            }
        }
    }
}
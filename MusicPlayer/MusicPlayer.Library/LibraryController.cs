using MusicPlayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MusicPlayer.Library
{
    public class LibraryController : IDisposable
    {
        private string _libraryFilePath;

        private Dictionary<string, TagLib.File> _songTagsCollection;

        public LibraryController()
            : this(AppDomain.CurrentDomain.BaseDirectory + "Library.mlib")
        {
        }

        internal LibraryController(string libraryPath)
        {
            if (string.IsNullOrEmpty(libraryPath))
            {
                throw new ArgumentNullException("libraryPath");
            }

            _libraryFilePath = libraryPath;

            if (!File.Exists(_libraryFilePath))
            {
                var stream = File.Create(_libraryFilePath);
                stream.Close();
            }
            _songTagsCollection = new Dictionary<string, TagLib.File>();

            LoadFromFile();
        }

        public void AddSongFile(string path, TagLib.File tagFile)
        {
            if (_songTagsCollection.ContainsKey(path))
            {
                return;
            }
            else
            {
                _songTagsCollection.Add(path, tagFile);
            }
        }

        public void RemoveSong(string path)
        {
            _songTagsCollection.Remove(path);
        }

        public IOrderedEnumerable<KeyValuePair<string, TagLib.File>> RetrieveLibrary(SortPath sort)
        {
            IOrderedEnumerable<KeyValuePair<string, TagLib.File>> sorted;
            switch (sort)
            {
                case SortPath.Album:
                    sorted = _songTagsCollection.OrderBy(a => a.Value.Tag.Album);
                    break;

                case SortPath.Artist:
                    sorted = _songTagsCollection.OrderBy(a => a.Value.Tag.FirstPerformer);
                    break;

                case SortPath.Title:
                default:
                    sorted = _songTagsCollection.OrderBy(a => a.Value.Tag.Title);
                    break;
            }

            return sorted;
        }

        public void Save()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml("<library></library>");

            XmlElement root = xDoc.DocumentElement;
            XmlNode songsRoot = xDoc.CreateElement("songs");

            foreach (var song in _songTagsCollection)
            {
                var songNode = xDoc.CreateNode(XmlNodeType.Element, "song", "");
                var pathNode = songNode.AppendChild(xDoc.CreateNode(XmlNodeType.Element, "path", ""));
                pathNode.InnerText = song.Key;
                songNode.AppendChild(pathNode);

                songsRoot.AppendChild(songNode);
            }

            root.AppendChild(songsRoot);
            xDoc.AppendChild(root);

            using (var writer = XmlWriter.Create(_libraryFilePath))
            {
                xDoc.WriteContentTo(writer);
            }
        }

        private void LoadFromFile()
        {
            using (XmlReader reader = XmlReader.Create(_libraryFilePath))
            {
                XmlDocument xDoc = new XmlDocument();
                XmlNodeList songNodes = null;
                try
                {
                    xDoc.Load(reader);
                    songNodes = xDoc.DocumentElement.GetElementsByTagName("song");
                }
                catch (XmlException ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }

                foreach (XmlNode song in songNodes)
                {
                    string path = song.SelectSingleNode("path").InnerText;

                    if (!string.IsNullOrEmpty(path))
                    {
                        var tag = TagLib.File.Create(path);
                        _songTagsCollection.Add(path, tag);
                    }
                }
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
                Save();

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
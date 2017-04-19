using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Inherits from FileBitmapTileCache.
    /// Tiles are marked as obsolete after calling ClearCache
    /// and are deleted in the background thread.
    /// </summary>
    [Serializable]
    public class SessionFileBitmapTileCache : FileBitmapTileCache
    {
        private static readonly object LockObject = new object();

        [NonSerialized] private readonly Thread _clearFileCacheThread;

        private readonly Collection<string> _obsoleteCacheIds;

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="cacheDirectory">Directory of the tile cache.</param>
        public SessionFileBitmapTileCache(string cacheDirectory)
            : this(cacheDirectory, Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="cacheDirectory">Directory of the tile cache.</param>
        /// <param name="cacheId">Cache ID of the tile cache.</param>
        public SessionFileBitmapTileCache(string cacheDirectory, string cacheId)
            : this(cacheDirectory, cacheId, TileImageFormat.Png, new TileMatrix(590591790))
        {
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="cacheDirectory">Directory of the tile cache.</param>
        /// <param name="cacheId">Cache ID of the tile cache.</param>
        /// <param name="imageFormat">Image format of the tile cache.</param>
        /// <param name="tileMatrix">Tile matrix of the tile cache.</param>
        public SessionFileBitmapTileCache(string cacheDirectory, string cacheId, TileImageFormat imageFormat,
            TileMatrix tileMatrix)
            : base(cacheDirectory, cacheId, imageFormat, tileMatrix)
        {
            _obsoleteCacheIds = new Collection<string>();

            _clearFileCacheThread = new Thread(ClearFileCache);
            _clearFileCacheThread.IsBackground = true;
            _clearFileCacheThread.Start();
        }

        /// <summary>
        /// Creates a new cacheId for the SessionFileBitmapTileCache and marks
        /// the original one as obsolete. The original is deleted in a background thread.
        /// </summary>
        /// <remarks>Does not take effect if ReadOnly is set to true.</remarks>
        protected override void ClearCacheCore()
        {
            _obsoleteCacheIds.Add(CacheId);

            CacheId = Guid.NewGuid().ToString();
        }

        private void ClearFileCache()
        {
            while (true)
            {
                try
                {
                    lock (LockObject)
                    {
                        var obsoleteDirectories = GetCombinedObsoleteCacheDirectories();

                        foreach (var obsoleteDirectory in obsoleteDirectories)
                        {
                            if (Directory.Exists(obsoleteDirectory))
                            {
                                try
                                {
                                    Directory.Delete(obsoleteDirectory, true);
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLine(ex.Message);
                                }
                            }
                        }

                        _obsoleteCacheIds.Clear();
                    }
                }
                finally
                {
                    _clearFileCacheThread.Join(600000);
                }
            }
        }

        private Collection<string> GetCombinedObsoleteCacheDirectories()
        {
            var obsoleteDirectories = new Collection<string>();

            var cacheDirectory = CacheDirectory;

            if (!cacheDirectory.EndsWith(@"\", StringComparison.OrdinalIgnoreCase))
            {
                cacheDirectory += @"\";
            }

            foreach (var cacheId in _obsoleteCacheIds)
            {
                if (!string.IsNullOrEmpty(CacheId))
                {
                    var combinedCacheDirectory = string.Concat(cacheDirectory, cacheId, @"\");
                    obsoleteDirectories.Add(combinedCacheDirectory);
                }
            }

            return obsoleteDirectories;
        }
    }
}
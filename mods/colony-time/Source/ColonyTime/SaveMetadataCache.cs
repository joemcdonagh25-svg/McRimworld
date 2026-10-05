using System;
using System.Collections.Generic;
using System.IO;

namespace ColonyTime
{
    /// <summary>
    /// Process-lifetime cache keyed by path + last write + size.
    /// </summary>
    public sealed class SaveMetadataCache
    {
        public static readonly SaveMetadataCache Instance = new SaveMetadataCache();

        private readonly Dictionary<string, SaveMetadata> cache = new Dictionary<string, SaveMetadata>(StringComparer.OrdinalIgnoreCase);

        public SaveMetadata GetOrRead(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Failed(filePath);
            }

            FileInfo info;
            try
            {
                info = new FileInfo(filePath);
            }
            catch
            {
                return Failed(filePath);
            }

            if (!info.Exists)
            {
                cache.Remove(filePath);
                return Failed(filePath);
            }

            DateTime writeUtc = info.LastWriteTimeUtc;
            long size = info.Length;

            if (cache.TryGetValue(filePath, out SaveMetadata existing)
                && existing.LastWriteTimeUtc == writeUtc
                && existing.FileSize == size)
            {
                return existing;
            }

            SaveMetadata meta = SaveMetadataReader.Read(filePath);
            cache[filePath] = meta;

            if (!meta.ParseFailed)
            {
                ColonyTimeLog.Message("Parsed metadata for " + info.Name + ".");
            }

            return meta;
        }

        public void PruneMissing(IEnumerable<string> livePaths)
        {
            var live = new HashSet<string>(livePaths ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            if (live.Count == 0)
            {
                return;
            }

            var stale = new List<string>();
            foreach (string key in cache.Keys)
            {
                if (!live.Contains(key))
                {
                    stale.Add(key);
                }
            }

            for (int i = 0; i < stale.Count; i++)
            {
                cache.Remove(stale[i]);
            }
        }

        public void Clear()
        {
            cache.Clear();
        }

        // Exposed for tests / diagnostics.
        public int Count => cache.Count;

        private static SaveMetadata Failed(string filePath)
        {
            return new SaveMetadata
            {
                FilePath = filePath ?? string.Empty,
                ParseFailed = true
            };
        }
    }
}

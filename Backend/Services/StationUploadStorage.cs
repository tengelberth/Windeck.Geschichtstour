namespace Windeck.Geschichtstour.Backend.Services
{
    /// <summary>
    /// Kapselt das sichere Loeschen lokal gespeicherter Upload-Dateien einer Station.
    /// Externe URLs werden bewusst nicht angefasst.
    /// </summary>
    internal static class StationUploadStorage
    {
        /// <summary>
        /// Loescht die lokale Datei eines einzelnen Mediums, wenn sie unterhalb von wwwroot/uploads liegt.
        /// </summary>
        public static bool TryDeleteLocalMediaFile(string webRootPath, string? url)
        {
            if (!TryResolveLocalUploadPath(webRootPath, url, out string? absolutePath))
            {
                return string.IsNullOrWhiteSpace(url) || !url!.StartsWith('/');
            }

            try
            {
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }

                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Entfernt den Upload-Ordner einer Station rekursiv, falls er existiert.
        /// </summary>
        public static bool TryDeleteStationUploadDirectory(string webRootPath, int stationId)
        {
            string? stationDirectory = TryGetStationDirectoryPath(webRootPath, stationId);
            if (stationDirectory == null)
            {
                return false;
            }

            try
            {
                if (Directory.Exists(stationDirectory))
                {
                    Directory.Delete(stationDirectory, recursive: true);
                }

                DeleteDirectoryIfEmpty(Path.GetDirectoryName(stationDirectory));
                DeleteDirectoryIfEmpty(Path.GetDirectoryName(Path.GetDirectoryName(stationDirectory)));
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Raeumt leere Stations- und Upload-Ordner auf, nachdem eine einzelne Datei entfernt wurde.
        /// </summary>
        public static bool TryDeleteStationUploadDirectoryIfEmpty(string webRootPath, int stationId)
        {
            string? stationDirectory = TryGetStationDirectoryPath(webRootPath, stationId);
            if (stationDirectory == null)
            {
                return false;
            }

            try
            {
                DeleteDirectoryIfEmpty(stationDirectory);
                DeleteDirectoryIfEmpty(Path.GetDirectoryName(stationDirectory));
                DeleteDirectoryIfEmpty(Path.GetDirectoryName(Path.GetDirectoryName(stationDirectory)));
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static bool TryResolveLocalUploadPath(string webRootPath, string? url, out string? absolutePath)
        {
            absolutePath = null;

            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith('/'))
            {
                return false;
            }

            string relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string candidatePath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
            string uploadsRoot = GetUploadsRootPath(webRootPath);

            if (!IsPathWithinRoot(candidatePath, uploadsRoot))
            {
                return false;
            }

            absolutePath = candidatePath;
            return true;
        }

        private static string? TryGetStationDirectoryPath(string webRootPath, int stationId)
        {
            string uploadsRoot = GetUploadsRootPath(webRootPath);
            string stationDirectory = Path.GetFullPath(Path.Combine(uploadsRoot, "stations", stationId.ToString()));

            return IsPathWithinRoot(stationDirectory, uploadsRoot)
                ? stationDirectory
                : null;
        }

        private static string GetUploadsRootPath(string webRootPath)
        {
            return Path.GetFullPath(Path.Combine(webRootPath, "uploads"));
        }

        private static bool IsPathWithinRoot(string path, string rootPath)
        {
            string normalizedRoot = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            return path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static void DeleteDirectoryIfEmpty(string? directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                return;
            }

            if (!Directory.EnumerateFileSystemEntries(directoryPath).Any())
            {
                Directory.Delete(directoryPath);
            }
        }
    }
}

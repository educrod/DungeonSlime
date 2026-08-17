using System.IO;
using Microsoft.Xna.Framework.Content;

namespace MonoGameLibrary.Content;

public static class ContentManagerExtensions
{
    /// <summary>
    /// Loads an asset and wraps it in a <see cref="WatchedAsset{T}"/> so it can be
    /// hot-reloaded from disk by calling <see cref="WatchedAsset{T}.TryRefresh"/>
    /// each frame.
    /// </summary>
    public static WatchedAsset<T> Watch<T>(this ContentManager content, string assetName) where T : class
    {
        T asset = content.Load<T>(assetName);
        return new WatchedAsset<T>(content, assetName, asset);
    }

    /// <summary>
    /// Checks if a file is currently locked for exclusive access, which happens
    /// while the content builder is still writing it to disk.
    /// </summary>
    internal static bool IsFileLocked(string path)
    {
        if (!File.Exists(path))
        {
            return true;
        }

        try
        {
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }
}

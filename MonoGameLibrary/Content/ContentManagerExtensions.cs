using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
    /// Launches the project's "WatchContent" MSBuild target (see the WatchContent
    /// target in the entry project's .csproj) as a background process, so shader
    /// hot reload works without needing a separate terminal running it manually.
    /// Only compiled into Debug builds - the call site is stripped entirely in
    /// Release (see <see cref="ConditionalAttribute"/>).
    /// </summary>
    [Conditional("DEBUG")]
    public static void StartContentWatcherTask()
    {
        string[] args = Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            // if the application was started with the --no-reload option, then do not start the watcher.
            if (arg == "--no-reload") return;
        }

        // identify the project directory
        string projectFile = Assembly.GetEntryAssembly().GetName().Name + ".csproj";
        string current = Directory.GetCurrentDirectory();
        string projectDirectory = null;

        while (current != null && projectDirectory == null)
        {
            if (File.Exists(Path.Combine(current, projectFile)))
            {
                // the valid project csproj exists in the directory
                projectDirectory = current;
            }
            else
            {
                // try looking in the parent directory.
                //  When there is no parent directory, the variable becomes 'null'
                current = Path.GetDirectoryName(current);
            }
        }

        // if no valid project was identified, then it is impossible to start the watcher
        if (string.IsNullOrEmpty(projectDirectory)) return;

        // start the watcher process
        Process process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build -t:WatchContent --tl:off",
            WorkingDirectory = projectDirectory,
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false,
            CreateNoWindow = false
        });

        // when this program exits, make sure to emit a kill signal to the watcher process
        AppDomain.CurrentDomain.ProcessExit += (_, __) =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                /* ignore */
            }
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                /* ignore */
            }
        };
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

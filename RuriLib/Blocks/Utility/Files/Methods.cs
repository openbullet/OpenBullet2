using RuriLib.Attributes;
using RuriLib.Extensions;
using RuriLib.Functions.Files;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Utility.Files
{
    [BlockCategory("Files", "Blocks for working with files and folders", "#fad6a5")]
    public static class Methods
    {
        [Block("Checks if a file exists")]
        public static bool FileExists(BotData data, string path)
        {
            var exists = ExecuteFileOperation(data, path, true, (p, c) =>
            {
                return Task.FromResult(File.Exists(p));
            }).Result;

            data.Logger.LogHeader();
            data.Logger.Log(path + (exists ? " exists" : " does not exist"), LogColors.Flavescent);
            return exists;
        }

        #region Read File
        [Block("Reads the entire content of a file to a single string")]
        public static async Task<string> FileRead(BotData data, string path)
        {
            var text = await ExecuteFileOperation(data, path, true, async (p, c) =>
            {
                return await File.ReadAllTextAsync(p, data.CancellationToken);
            });

            data.Logger.LogHeader();
            data.Logger.Log($"Read {path}: {text.TruncatePretty(200)}", LogColors.Flavescent);
            return text;
        }

        [Block("Reads all lines of a file")]
        public static async Task<List<string>> FileReadLines(BotData data, string path)
        {
            var lines = await ExecuteFileOperation(data, path, true, async (p, c) =>
            {
                return await File.ReadAllLinesAsync(p, data.CancellationToken);
            });

            data.Logger.LogHeader();
            data.Logger.Log($"Read {lines.Length} lines from {path}", LogColors.Flavescent);
            return lines.ToList();
        }

        [Block("Reads all bytes of a file")]
        public static async Task<byte[]> FileReadBytes(BotData data, string path)
        {
            var bytes = await ExecuteFileOperation(data, path, true, async (p, c) =>
            {
                return await File.ReadAllBytesAsync(p, data.CancellationToken);
            });

            data.Logger.LogHeader();
            data.Logger.Log($"Read {bytes.Length} bytes from {path}", LogColors.Flavescent);
            return bytes;
        }
        #endregion

        #region Write File
        [Block("Writes a string to a file",
            extraInfo = "The file will be created if it doesn't exist and all its previous content will be overwritten")]
        public static async Task FileWrite(BotData data, string path, [Interpolated] string content)
        {
            await ExecuteFileOperation(data, path, content, async (p, c) => 
            { 
                await File.WriteAllTextAsync(p, c.Unescape(), data.CancellationToken);
                return true; 
            });

            data.Logger.LogHeader();
            data.Logger.Log($"Wrote content to {path}", LogColors.Flavescent);
        }

        [Block("Writes lines to a file",
            extraInfo = "The file will be created if it doesn't exist and all its previous content will be overwritten")]
        public static async Task FileWriteLines(BotData data, string path, [Variable] List<string> lines)
        {
            await ExecuteFileOperation(data, path, lines, async (p, c) =>
            {
                await File.WriteAllLinesAsync(p, c, data.CancellationToken);
                return true;
            });

            data.Logger.LogHeader();
            data.Logger.Log($"Wrote lines to {path}", LogColors.Flavescent);
        }

        [Block("Writes bytes to a file",
            extraInfo = "The file will be created if it doesn't exist and all its previous content will be overwritten")]
        public static async Task FileWriteBytes(BotData data, string path, [Variable] byte[] content)
        {
            await ExecuteFileOperation(data, path, content, async (p, c) =>
            {
                await File.WriteAllBytesAsync(p, c, data.CancellationToken);
                return true;
            });

            data.Logger.LogHeader();
            data.Logger.Log($"Wrote bytes to {path}", LogColors.Flavescent);
        }
        #endregion

        #region Append File
        [Block("Appends a string at the end of a file")]
        public static async Task FileAppend(BotData data, string path, [Interpolated] string content)
        {
            await ExecuteFileOperation(data, path, content, async (p, c) =>
            {
                await File.AppendAllTextAsync(p, c.Unescape(), data.CancellationToken);
                return true;
            });

            data.Logger.LogHeader();
            data.Logger.Log($"Appended content to {path}", LogColors.Flavescent);
        }

        [Block("Appends lines at the end of a file")]
        public static async Task FileAppendLines(BotData data, string path, [Variable] List<string> lines)
        {
            await ExecuteFileOperation(data, path, lines, async (p, c) =>
            {
                await File.AppendAllLinesAsync(p, c, data.CancellationToken);
                return true;
            });

            data.Logger.LogHeader();
            data.Logger.Log($"Appended lines to {path}", LogColors.Flavescent);
        }
        #endregion

        #region File Operations
        [Block("Copies a file to a new location")]
        public static void FileCopy(BotData data, string originPath, string destinationPath)
        {
            if (data.Providers.Security.RestrictBlocksToCWD)
            {
                FileUtils.ThrowIfNotInCWD(originPath);
                FileUtils.ThrowIfNotInCWD(destinationPath);
            }

            FileUtils.CreatePath(destinationPath);

            lock (FileLocker.GetHandle(originPath))
                lock (FileLocker.GetHandle(destinationPath))
                    File.Copy(originPath, destinationPath);

            data.Logger.LogHeader();
            data.Logger.Log($"Copied {originPath} to {destinationPath}", LogColors.Flavescent);
        }

        [Block("Moves a file to a new location")]
        public static void FileMove(BotData data, string originPath, string destinationPath)
        {
            if (data.Providers.Security.RestrictBlocksToCWD)
            {
                FileUtils.ThrowIfNotInCWD(originPath);
                FileUtils.ThrowIfNotInCWD(destinationPath);
            }

            FileUtils.CreatePath(destinationPath);

            lock (FileLocker.GetHandle(originPath))
                lock (FileLocker.GetHandle(destinationPath))
                    File.Move(originPath, destinationPath);

            data.Logger.LogHeader();
            data.Logger.Log($"Moved {originPath} to {destinationPath}", LogColors.Flavescent);
        }

        [Block("Deletes a file")]
        public static void FileDelete(BotData data, string path)
        {
            if (data.Providers.Security.RestrictBlocksToCWD)
                FileUtils.ThrowIfNotInCWD(path);

            lock (FileLocker.GetHandle(path))
                File.Delete(path);

            data.Logger.LogHeader();
            data.Logger.Log($"Deleted {path}", LogColors.Flavescent);
        }
        #endregion

        #region Folders
        [Block("Checks if a folder exists")]
        public static bool FolderExists(BotData data, string path)
        {
            if (data.Providers.Security.RestrictBlocksToCWD)
                FileUtils.ThrowIfNotInCWD(path);

            var exists = Directory.Exists(path);
            data.Logger.LogHeader();
            data.Logger.Log(path + (exists ? " exists" : " does not exist"), LogColors.Flavescent);
            return exists;
        }

        [Block("Creates a directory in the given path")]
        public static void CreatePath(BotData data, string path)
        {
            if (data.Providers.Security.RestrictBlocksToCWD)
                FileUtils.ThrowIfNotInCWD(path);

            FileUtils.CreatePath(path);
            data.Logger.LogHeader();
            data.Logger.Log($"The path {path} was created", LogColors.Flavescent);
        }

        public static void FolderDelete(BotData data, string path)
        {
            if (data.Providers.Security.RestrictBlocksToCWD)
                FileUtils.ThrowIfNotInCWD(path);

            Directory.Delete(path);

            data.Logger.LogHeader();
            data.Logger.Log($"Deleted {path}", LogColors.Flavescent);
        }
        #endregion

        private static Task<TOut> ExecuteFileOperation<TIn, TOut>(BotData data, string path, TIn parameter, Func<string, TIn, Task<TOut>> func)
        {
            if (data.Providers.Security.RestrictBlocksToCWD)
                FileUtils.ThrowIfNotInCWD(path);

            FileUtils.CreatePath(path);

            // TODO: Implement an asynchronous lock, otherwise it will throw a
            // SynchronizationLockException since we cannot call Monitor.Exit() in an async context
            // https://stackoverflow.com/questions/21404144/synchronizationlockexception-on-monitor-exit-when-using-await

            TOut result;
            var fileLock = FileLocker.GetHandle(path);
            Monitor.Enter(fileLock);

            try
            {
                // HACK: Execute synchronously as a temporary fix
                result = func.Invoke(path, parameter).Result;
            }
            finally
            {
                Monitor.Exit(fileLock);
            }

            return Task.FromResult(result);
        }
    }
}

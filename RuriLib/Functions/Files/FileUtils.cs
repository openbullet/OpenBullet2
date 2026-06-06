using RuriLib.Extensions;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RuriLib.Functions.Files;

/// <summary>
/// Provides methods to work with files.
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// Gets the first available name in the given folder by incrementing a number at the end of the filename.
    /// </summary>
    public static string GetFirstAvailableFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        // If the file does not exist, the path is already valid.
        if (!File.Exists(fileName))
        {
            return fileName;
        }

        var directoryName = Path.GetDirectoryName(fileName)
            ?? throw new ArgumentException("The file name must include a directory.", nameof(fileName));

        string path;
        var i = 0;
        do
        {
            i++;
            var newName = Path.GetFileNameWithoutExtension(fileName) + i + Path.GetExtension(fileName);
            path = Path.Combine(directoryName, newName);
        }
        while (File.Exists(path));

        return path;
    }

    /// <summary>
    /// Fixes the filename to be compatible with the filesystem indicization by replacing all invalid
    /// file name characters with the specified <paramref name="replacement"/>.
    /// </summary>
    public static string ReplaceInvalidFileNameChars(string fileName, string replacement = "_")
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return Regex.Replace(fileName, invalidRegStr, replacement ?? "");
    }

    /// <summary>
    /// Throws an UnauthorizedAccessException if the path is not part of the current working directory.
    /// </summary>
    /// <param name="path">The absolute or relative path.</param>
    public static void ThrowIfNotInCWD(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!path.IsSubPathOf(Directory.GetCurrentDirectory()))
        {
            throw new UnauthorizedAccessException(
                "For security reasons, interactions with paths outside of the current working directory are not allowed");
        }
    }

    /// <summary>
    /// Creates the folder structure that contains a certain files if it doesn't already exist.
    /// </summary>
    /// <param name="file">The absolute or relative path to the file.</param>
    public static void CreatePath(string file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var dirName = Path.GetDirectoryName(file);

        if (!string.IsNullOrWhiteSpace(dirName) && !Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }
    }

    /// <summary>
    /// Counts the number of lines in a text file without allocating a string per line.
    /// Supports LF, CRLF and CR line terminators.
    /// </summary>
    public static long CountLines(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        using var reader = new StreamReader(path);
        var buffer = new char[8192];
        long count = 0;
        var sawAnyChar = false;
        var endedWithLineTerminator = false;
        var previousWasCarriageReturn = false;

        int charsRead;
        while ((charsRead = reader.ReadBlock(buffer, 0, buffer.Length)) > 0)
        {
            for (var i = 0; i < charsRead; i++)
            {
                var c = buffer[i];
                sawAnyChar = true;

                if (previousWasCarriageReturn)
                {
                    if (c == '\n')
                    {
                        count++;
                        endedWithLineTerminator = true;
                        previousWasCarriageReturn = false;
                        continue;
                    }

                    count++;
                    endedWithLineTerminator = true;
                    previousWasCarriageReturn = false;
                }

                if (c == '\r')
                {
                    previousWasCarriageReturn = true;
                }
                else if (c == '\n')
                {
                    count++;
                    endedWithLineTerminator = true;
                }
                else
                {
                    endedWithLineTerminator = false;
                }
            }
        }

        if (previousWasCarriageReturn)
        {
            count++;
            endedWithLineTerminator = true;
        }

        if (sawAnyChar && !endedWithLineTerminator)
        {
            count++;
        }

        return count;
    }
}

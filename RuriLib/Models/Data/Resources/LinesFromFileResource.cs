using RuriLib.Models.Data.Resources.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RuriLib.Models.Data.Resources
{
    public class LinesFromFileResource : ConfigResource, IDisposable
    {
        private readonly LinesFromFileResourceOptions options;
        private readonly FileStream stream;
        private readonly StreamReader reader;
        private readonly object streamLocker = new();
        private int linesRead = 0;

        public LinesFromFileResource(LinesFromFileResourceOptions options)
        {
            this.options = options;

            // Open the file
            stream = File.OpenRead(options.Location);
            reader = new StreamReader(stream, Encoding.UTF8, true, 128);
        }

        /// <inheritdoc/>
        public override string TakeOne()
        {
            lock(streamLocker)
            {
                return TakeOneUnsafe();
            }
        }

        private string TakeOneUnsafe()
        {
            var line = reader.ReadLine();

            // If we reached the end of the file
            if (line is null)
            {
                // If we never read a single valid line, throw
                if (linesRead == 0)
                {
                    throw new Exception($"Resource '{options.Name}' has no more valid lines to take");
                }

                // If we don't loop around, throw
                if (!options.LoopsAround)
                {
                    throw new Exception($"Reached the end of resource '{options.Name}' and no loop around was specified");
                }
                // If we do loop around, rewind the stream and take a new line
                else
                {
                    linesRead = 0;
                    stream.Seek(0, SeekOrigin.Begin);
                    return TakeOne();
                }
            }

            // If we ignore empty lines, take the next one
            if (string.IsNullOrWhiteSpace(line) && options.IgnoreEmptyLines)
            {
                return TakeOne();
            }

            linesRead++;
            return line;
        }

        /// <inheritdoc/>
        public override List<string> Take(int amount)
        {
            lock (streamLocker)
            {
                List<string> lines = new();

                for (var i = 0; i < amount; i++)
                {
                    lines.Add(TakeOneUnsafe());
                }

                return lines;
            }
        }

        public void Dispose()
        {
            stream?.Dispose();
            reader?.Dispose();
        }
    }
}

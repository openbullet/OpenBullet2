using System;

namespace RuriLib.Http.Helpers
{
    static internal class SpanHelpers
    {
        private static readonly byte[] CRLF_Bytes = { 13, 10 };
       
        public static LineSplitEnumerator SplitLines(this Span<byte> span)
        {
            // LineSplitEnumerator is a struct so there is no allocation here
            return new LineSplitEnumerator(span);
        }

        public static LineSplitEnumerator SplitLines(this ReadOnlySpan<byte> span)
        {
            // LineSplitEnumerator is a struct so there is no allocation here
            return new LineSplitEnumerator(span);
        }

        // Must be a ref struct as it contains a ReadOnlySpan<char>
        public ref struct LineSplitEnumerator
        {
            private ReadOnlySpan<byte> _span;
            public LineSplitEntry Current { get; private set; }

            public LineSplitEnumerator(ReadOnlySpan<byte> span)
            {
                _span = span;
                Current = default;
            }

            // Needed to be compatible with the foreach operator
            public LineSplitEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                var span = _span;
                
                if (span.Length == 0) // Reach the end of the string
                    return false;
                
                var index = span.IndexOf(CRLF_Bytes);
                
                if (index == -1) // The string is composed of only one line
                {
                    _span = ReadOnlySpan<byte>.Empty; // The remaining string is an empty string
                    Current = new LineSplitEntry(span, ReadOnlySpan<byte>.Empty);
                    return true;
                }
                else
                {
                    Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 2));
                    _span = span.Slice(index + 2);
                    return true;
                }
            }
        }

        public readonly ref struct LineSplitEntry
        {
            public LineSplitEntry(ReadOnlySpan<byte> line, ReadOnlySpan<byte> separator)
            {
                Line = line;
                Separator = separator;
            }

            public ReadOnlySpan<byte> Line { get; }
            public ReadOnlySpan<byte> Separator { get; }

            // This method allow to deconstruct the type, so you can write any of the following code
            // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
            // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
            // https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct#deconstructing-user-defined-types
            public void Deconstruct(out ReadOnlySpan<byte> line, out ReadOnlySpan<byte> separator)
            {
                line = Line;
                separator = Separator;
            }

            // This method allow to implicitly cast the type into a ReadOnlySpan<byte>, so you can write the following code
            // foreach (ReadOnlySpan<byte> entry in str.SplitLines())
            public static implicit operator ReadOnlySpan<byte>(LineSplitEntry entry) => entry.Line;
        }
    }
}

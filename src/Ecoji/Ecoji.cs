//
// Author:
//   Aaron Bockover <aaron@abock.dev>
//
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

public static partial class Ecoji
{
    public static string Encode(string input, int wrap = 0)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Encode(Encoding.UTF8.GetBytes(input), wrap);
    }

    public static string Encode(byte[] input, int wrap = 0)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        using var stream = new MemoryStream(input);
        return Encode(stream, wrap);
    }

    public static string Encode(Stream input, int wrap = 0)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        using var writer = new StringWriter();
        Encode(input, writer, wrap);
        return writer.ToString();
    }

    public static void Encode(Stream input, TextWriter output, int wrap = 0)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (output is null)
            throw new ArgumentNullException(nameof(output));

        var wrapLineProgress = 0;

        var data = new byte[5];

        while (true)
        {
            var read = 0;
            while (read < data.Length)
            {
                var n = input.Read(data, read, data.Length - read);
                read += n;
                if (n == 0)
                    break;
            }

            if (read <= 0)
                break;

            int b0 = data[0];
            int b1 = read > 1 ? data[1] : 0;
            int b2 = read > 2 ? data[2] : 0;
            int b3 = read > 3 ? data[3] : 0;
            int b4 = read > 4 ? data[4] : 0;

            output.WriteRune(emojis[b0  <<  2 | b1  >>  6]);

            switch (read)
            {
                case 1:
                    output.WriteRune(padding);
                    output.WriteRune(padding);
                    output.WriteRune(padding);
                    break;
                case 2:
                    output.WriteRune(emojis[(b1 & 0x3f) << 4 | b2 >> 4]);
                    break;
                case 3:
                    output.WriteRune(emojis[(b1 & 0x3f) << 4 | b2 >> 4]);
                    output.WriteRune(emojis[(b2 & 0x0f) << 6 | b3 >> 2]);
                    output.WriteRune(padding);
                    break;
                case 4:
                    output.WriteRune(emojis[(b1 & 0x3f) << 4 | b2 >> 4]);
                    output.WriteRune(emojis[(b2 & 0x0f) << 6 | b3 >> 2]);

                    switch (b3 & 0x03)
                    {
                        case 0:
                            output.WriteRune(padding40);
                            break;
                        case 1:
                            output.WriteRune(padding41);
                            break;
                        case 2:
                            output.WriteRune(padding42);
                            break;
                        case 3:
                            output.WriteRune(padding43);
                            break;
                    }
                    break;
                case 5:
                    output.WriteRune(emojis[(b1 & 0x3f) << 4 | b2 >> 4]);
                    output.WriteRune(emojis[(b2 & 0x0f) << 6 | b3 >> 2]);
                    output.WriteRune(emojis[(b3 & 0x03) << 8 | b4]);
                    break;
            }

            if (wrap > 0)
            {
                wrapLineProgress += 4;
                if (wrapLineProgress >= wrap)
                {
                    output.WriteLine();
                    wrapLineProgress = 0;
                }
            }
        }
    }

    public static string DecodeUtf8(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Decode(input, Encoding.UTF8);
    }

    public static string Decode(string input, Encoding encoding)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (encoding is null)
            throw new ArgumentNullException(nameof(encoding));

        return encoding.GetString(Decode(input));
    }

    public static byte[] Decode(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        using var reader = new StringReader(input);
        using var stream = new MemoryStream();
        Decode(reader, stream);
        return stream.ToArray();
    }

    public static void Decode(Stream input, Stream output)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));
        
        if (output is null)
            throw new ArgumentNullException(nameof(output));
        
        Decode(new StreamReader(input), output);
    }

    public static void Decode(TextReader input, Stream output)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (output is null)
            throw new ArgumentNullException(nameof(output));

        var inBuffer = new int[4];
        var outBuffer = new byte[5];

        while (true)
        {
            for (int i = 0; i < inBuffer.Length; i++)
            {
                var c = input.ReadRune();
                if (c < 0)
                {
                    if (i == 0)
                        return;

                    throw new UnexpectedEndOfInputException(
                        "number of input code points is not a multiple of 4");
                }

                inBuffer[i] = c;
            }

            var b0 = GetEmojiIndex(inBuffer[0]);
            var b1 = GetEmojiIndex(inBuffer[1]);
            var b2 = GetEmojiIndex(inBuffer[2]);
            var b3 = inBuffer[3] switch
            {
                padding40 => 0,
                padding41 => 1 << 8,
                padding42 => 2 << 8,
                padding43 => 3 <<8,
                _ => GetEmojiIndex(inBuffer[3])
            };

            outBuffer[0] = (byte)(b0 >> 2);
            outBuffer[1] = (byte)(((b0 & 0x3) << 6) | (b1 >> 4));
            outBuffer[2] = (byte)(((b1 & 0xf) << 4) | (b2 >> 6));
            outBuffer[3] = (byte)(((b2 & 0x3f) << 2) | (b3 >> 8));
            outBuffer[4] = (byte)(b3 & 0xff);

            if (inBuffer[1] == padding)
                output.Write(outBuffer, 0, 1);
            else if (inBuffer[2] == padding)
                output.Write(outBuffer, 0, 2);
            else if (inBuffer[3] == padding)
                output.Write(outBuffer, 0, 3);
            else if (inBuffer[3] == padding40 ||
                inBuffer[3] == padding41 ||
                inBuffer[3] == padding42 ||
                inBuffer[3] == padding43)
                output.Write(outBuffer, 0, 4);
            else
                output.Write(outBuffer, 0, 5);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void WriteRune(this TextWriter writer, int rune)
        => writer.Write(char.ConvertFromUtf32(rune));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int ReadRune(this TextReader reader)
    {
        var high = (char)reader.Read();
        if (high == char.MaxValue)
            return -1;

        if (!char.IsHighSurrogate(high))
            return high;

        var low = (char)reader.Read();
        if (!char.IsLowSurrogate(low))
            throw new UnexpectedEndOfInputException(
                $"failed to read low surrogate (high was 0x{(int)high:X})");

        return char.ConvertToUtf32(high, low);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int GetEmojiIndex(int rune)
    {
        if (revEmojis.TryGetValue(rune, out var index))
            return index;

        switch (rune)
        {
            case padding:
            case padding40:
            case padding41:
            case padding42:
            case padding43:
                return 0;
            default:
                throw new InvalidCharacterException(rune);
        }
    }

    public sealed class InvalidCharacterException : Exception
    {
        internal InvalidCharacterException(int rune)
            : base($"invalid ecoji alphabet code point 0x{rune:X}")
        {
        }
    }

    public sealed class UnexpectedEndOfInputException : Exception
    {
        internal UnexpectedEndOfInputException(string message)
            : base(message)
        {
        }
    }
}

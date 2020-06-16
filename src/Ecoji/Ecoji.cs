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

/// <summary>
/// Encode and decode strings and arbitrary data using an alphabet of
/// 1024 emojis. Like a really colorful and expressive base64!
/// </summary>
public static partial class Ecoji
{
    /// <summary>
    /// Options for customizing ecoji output.
    /// </summary>
    public readonly struct EncodingOptions
    {
        /// <summary>
        /// No wrapping and uses the system's default new line,
        /// <see cref="Environment.NewLine"/>.
        /// </summary>
        public static readonly EncodingOptions Default = new EncodingOptions(
            wrap: 0,
            newLine: null);

        /// <summary>
        /// Insert <see cref="NewLine"/> after after every number of
        /// specified emojis have been written. <c>0</c> disables wrapping.
        /// </summary>
        public int Wrap { get; }

        /// <summary>
        /// The string to use when wrapping. A <c>null</c> value will use the
        /// system's default new line, <see cref="Environment.NewLine"/>.
        /// </summary>
        public string? NewLine { get; }

        /// <param name="wrap">
        /// Insert <paramref cref="newLine"/> after after every number of
        /// specified emojis have been written. <c>0</c> disables wrapping.
        /// </param>
        /// <param name="newLine">
        /// The string to use when wrapping. A <c>null</c> value will use the
        /// system's default new line, <see cref="Environment.NewLine"/>.
        /// </param>
        public EncodingOptions(int wrap, string? newLine)
        {
            Wrap = wrap;
            NewLine = newLine;
        }

        /// <param name="wrap">
        /// Insert <see cref="EncodingOptions.NewLine"/> after after every
        /// number of specified emojis have been written. <c>0</c> disables
        /// wrapping.
        /// </param>
        public EncodingOptions WithWrap(int wrap)
            => new EncodingOptions(wrap, NewLine);

        /// <param name="newLine">
        /// The string to use when wrapping. A <c>null</c> value will use the
        /// system's default new line, <see cref="Environment.NewLine"/>.
        /// </param>
        public EncodingOptions WithNewLine(string? newLine)
            => new EncodingOptions(Wrap, newLine);

        public static implicit operator EncodingOptions(int wrap)
            => new EncodingOptions(wrap, null);
    }

    static readonly Encoding UTF8NoBOM = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    /// <summary>Encode a UTF-8 string using emojis with optional wrapping.</summary>
    /// <returns>A string of emojis encoding <paramref name="input"/>.</returns>
    /// <param name="input">The string to encode.</param>
    /// <param name="options">Options to use to customize the ecoji output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is <c>null</c>.
    /// </exception>
    public static string Encode(string input, EncodingOptions options = default)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Encode(UTF8NoBOM.GetBytes(input), options);
    }

    /// <summary>Encode data using emojis with optional wrapping.</summary>
    /// <returns>A string of emojis encoding <paramref name="input"/>.</returns>
    /// <param name="input">The bytes to encode.</param>
    /// <param name="options">Options to use to customize the ecoji output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is <c>null</c>.
    /// </exception>
    public static string Encode(byte[] input, EncodingOptions options = default)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        using var stream = new MemoryStream(input);
        return Encode(stream, options);
    }

    /// <summary>Encode a stream of data using emojis with optional wrapping.</summary>
    /// <returns>A string of emojis encoding <paramref name="input"/>.</returns>
    /// <param name="input">The stream to encode.</param>
    /// <param name="options">Options to use to customize the ecoji output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is <c>null</c>.
    /// </exception>
    public static string Encode(Stream input, EncodingOptions options = default)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        using var writer = new StringWriter();
        writer.NewLine = options.NewLine ?? Environment.NewLine;
        Encode(input, writer, options.Wrap);
        return writer.ToString();
    }

    /// <summary>Encode a stream of data using emojis with optional wrapping.</summary>
    /// <param name="input">The stream to encode.</param>
    /// <param name="output">The stream to which encoded emojis will be written.</param>
    /// <param name="options">Options to use to customize the ecoji output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either <paramref name="input"/> or <paramref name="output"/> is <c>null</c>.
    /// </exception>
    public static void Encode(Stream input, Stream output, EncodingOptions options = default)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (output is null)
            throw new ArgumentNullException(nameof(output));

        using var writer = new StreamWriter(
            output,
            encoding: UTF8NoBOM,
            bufferSize: 1024,
            leaveOpen: true);

        writer.NewLine = options.NewLine ?? Environment.NewLine;

        Encode(input, writer, options.Wrap);
    }

    /// <summary>Encode a stream of data using emojis with optional wrapping.</summary>
    /// <param name="input">The stream to encode.</param>
    /// <param name="output">The writer to which encoded emojis will be written.</param>
    /// <param name="wrap">
    /// Insert <see cref="TextWriter.NewLine"/> set on <paramref name="output"/> after
    /// after every <paramref name="wrap"/> emojis. <c>0</c> disables wrapping.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either <paramref name="input"/> or <paramref name="output"/> is <c>null</c>.
    /// </exception>
    public static void Encode(Stream input, TextWriter output, int wrap = 0)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (output is null)
            throw new ArgumentNullException(nameof(output));

        var wrapLineProgress = 0;

        var b = new byte[5];

        while (true)
        {
            var read = 0;
            while (read < b.Length)
            {
                var n = input.Read(b, read, b.Length - read);
                read += n;
                if (n == 0)
                    break;
            }

            if (read <= 0) {
                if (wrapLineProgress > 0)
                    output.WriteLine();
                break;
            }
            
            for (var i = read; i < b.Length; i++)
                b[i] = 0;

            output.WriteRune(emojis[b[0] << 2 | b[1] >> 6]);

            switch (read)
            {
                case 1:
                    output.WriteRune(padding);
                    output.WriteRune(padding);
                    output.WriteRune(padding);
                    break;
                case 2:
                    output.WriteRune(emojis[(b[1] & 0x3f) << 4 | b[2] >> 4]);
                    output.WriteRune(padding);
                    output.WriteRune(padding);
                    break;
                case 3:
                    output.WriteRune(emojis[(b[1] & 0x3f) << 4 | b[2] >> 4]);
                    output.WriteRune(emojis[(b[2] & 0x0f) << 6 | b[3] >> 2]);
                    output.WriteRune(padding);
                    break;
                case 4:
                    output.WriteRune(emojis[(b[1] & 0x3f) << 4 | b[2] >> 4]);
                    output.WriteRune(emojis[(b[2] & 0x0f) << 6 | b[3] >> 2]);

                    var pad4 = (b[3] & 0x03) switch
                    {
                        0 => padding40,
                        1 => padding41,
                        2 => padding42,
                        3 => padding43,
                        _ => 0
                    };

                    if (pad4 > 0)
                        output.WriteRune(pad4);
                    break;
                case 5:
                    output.WriteRune(emojis[(b[1] & 0x3f) << 4 | b[2] >> 4]);
                    output.WriteRune(emojis[(b[2] & 0x0f) << 6 | b[3] >> 2]);
                    output.WriteRune(emojis[(b[3] & 0x03) << 8 | b[4]]);
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

    /// <summary>Decode a string of ecoji emojis that encode a UTF-8 string.</summary>
    /// <returns>The decoded string.</returns>
    /// <param name="input">The ecoji-encoded string of emojis to decode.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="UnexpectedEndOfInputException">
    /// Thrown when the number of number of <paramref name="input"/>
    /// code points is not a multiple of 4 or a low surrogate could
    /// not be read after reading a high surrogate.
    /// </exception>
    /// <exception cref="InvalidCharacterException">
    /// Thrown when an invalid ecoji alphabet code point is encountered.
    /// The character may be a valid emoji, but is not part of the ecoji
    /// encoding alphabet.
    /// </exception>
    public static string DecodeUtf8(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Decode(input, UTF8NoBOM);
    }

    /// <summary>Decode a string of ecoji emojis that encode string in the provided encoding.</summary>
    /// <returns>The decoded string.</returns>
    /// <param name="input">The ecoji-encoded string of emojis to decode.</param>
    /// <param name="encoding">The string encoding that represents the encoded data.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either <paramref name="input"/> or <paramref name="encoding"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="UnexpectedEndOfInputException">
    /// Thrown when the number of number of <paramref name="input"/>
    /// code points is not a multiple of 4 or a low surrogate could
    /// not be read after reading a high surrogate.
    /// </exception>
    /// <exception cref="InvalidCharacterException">
    /// Thrown when an invalid ecoji alphabet code point is encountered.
    /// The character may be a valid emoji, but is not part of the ecoji
    /// encoding alphabet.
    /// </exception>
    public static string Decode(string input, Encoding encoding)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (encoding is null)
            throw new ArgumentNullException(nameof(encoding));

        return encoding.GetString(Decode(input));
    }

    /// <summary>Decode ecoji emojis.</summary>
    /// <returns>The decoded raw bytes.</returns>
    /// <param name="input">The ecoji-encoded string of emojis to decode.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="UnexpectedEndOfInputException">
    /// Thrown when the number of number of <paramref name="input"/>
    /// code points is not a multiple of 4 or a low surrogate could
    /// not be read after reading a high surrogate.
    /// </exception>
    /// <exception cref="InvalidCharacterException">
    /// Thrown when an invalid ecoji alphabet code point is encountered.
    /// The character may be a valid emoji, but is not part of the ecoji
    /// encoding alphabet.
    /// </exception>
    public static byte[] Decode(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        using var reader = new StringReader(input);
        using var stream = new MemoryStream();
        Decode(reader, stream);
        return stream.ToArray();
    }

    /// <summary>Decode ecoji emojis.</summary>
    /// <returns>The decoded raw bytes.</returns>
    /// <param name="input">The stream from which ecoji-encoded data will be decoded.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="UnexpectedEndOfInputException">
    /// Thrown when the number of number of <paramref name="input"/>
    /// code points is not a multiple of 4 or a low surrogate could
    /// not be read after reading a high surrogate.
    /// </exception>
    /// <exception cref="InvalidCharacterException">
    /// Thrown when an invalid ecoji alphabet code point is encountered.
    /// The character may be a valid emoji, but is not part of the ecoji
    /// encoding alphabet.
    /// </exception>
    public static byte[] Decode(Stream input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        using var stream = new MemoryStream();
        Decode(input, stream);
        return stream.ToArray();
    }

    /// <summary>Decode ecoji emojis.</summary>
    /// <param name="input">The stream from which ecoji-encoded data will be decoded.</param>
    /// <param name="output">The stream to which decoded data will be written.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="UnexpectedEndOfInputException">
    /// Thrown when the number of number of <paramref name="input"/>
    /// code points is not a multiple of 4 or a low surrogate could
    /// not be read after reading a high surrogate.
    /// </exception>
    /// <exception cref="InvalidCharacterException">
    /// Thrown when an invalid ecoji alphabet code point is encountered.
    /// The character may be a valid emoji, but is not part of the ecoji
    /// encoding alphabet.
    /// </exception>
    public static void Decode(Stream input, Stream output)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        if (output is null)
            throw new ArgumentNullException(nameof(output));

        Decode(new StreamReader(input), output);
    }

    /// <summary>Decode ecoji emojis.</summary>
    /// <param name="input">The reader from which ecoji-encoded data will be decoded.</param>
    /// <param name="output">The stream to which decoded data will be written.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="UnexpectedEndOfInputException">
    /// Thrown when the number of number of <paramref name="input"/>
    /// code points is not a multiple of 4 or a low surrogate could
    /// not be read after reading a high surrogate.
    /// </exception>
    /// <exception cref="InvalidCharacterException">
    /// Thrown when an invalid ecoji alphabet code point is encountered.
    /// The character may be a valid emoji, but is not part of the ecoji
    /// encoding alphabet.
    /// </exception>
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
                int c;

                do c = input.ReadRune();
                while (c == '\n' || c == '\r');

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

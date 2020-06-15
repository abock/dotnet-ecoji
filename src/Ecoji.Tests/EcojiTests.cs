//
// Author:
//   Aaron Bockover <aaron@abock.dev>
//
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using System.IO;

using Xunit;

public class EcojiTests
{
    static readonly (string Decoded, string Encoded)[] stringTestCases =
    {
        ("Ecoji .NET", "🏣🔉🦐🔼🍈😯🥀🐥"),
        ("6789", "🎥🤠📠🏍"),
        ("XY\n", "🐲👡🕟☕"),
        (
            "Base64 is so 1999, isn't there something better?\n",
            "🏗📩🎦🐇🎛📘🔯🚜💞😽🆖🐊🎱🥁🚄🌱💞😭💮🇵💢🕥🐭🔸🍉🚲🦑🐶💢🕥🔮🔺🍉📸🐮🌼👦🚟🥴📑"
        ),
        ("a", "👕☕☕☕"),
        ("ab", "👖📲☕☕"),
        ("abc", "👖📸🎈☕"),
        ("abcd", "👖📸🎦⚜"),
        ("ac", "👖🔃☕☕"),
        ("b", "👙☕☕☕"),
        ("ba", "👚📢☕☕")
    };

    public static IEnumerable<object[]> StringTestCases
        => stringTestCases.Select(t => new object[] { t.Decoded, t.Encoded });

    [Theory]
    [MemberData(nameof(StringTestCases))]
    public void Encode(string input, string expectedOutput)
        => Assert.Equal(expectedOutput, Ecoji.Encode(
            input,
            new Ecoji.EncodingOptions(0, "\n")));

    [Theory]
    [MemberData(nameof(StringTestCases))]
    public void Decode(string input, string expectedOutput)
        => Assert.Equal(input, Ecoji.DecodeUtf8(expectedOutput));

    public static IEnumerable<object[]> FileTestCases
        => Directory
            .EnumerateFiles(
                Path.Combine(
                    Path.GetDirectoryName(typeof(EcojiTests)
                        .Assembly
                        .Location),
                    "TestData"))
            .Where(path => Path.GetExtension(path) != ".ecoji")
            .Select(path => new object[]
            {
                Path.GetFileNameWithoutExtension(path),
                path,
                Path.ChangeExtension(path, "ecoji")
            });

    /// <summary>
    /// Round trips test data on disk and compares the ecoji-encoded data
    /// with a reference version produced with the Keith Turner's Go
    /// implementation (https://github.com/keith-turner/ecoji).
    /// </summary>
    [Theory]
    [MemberData(nameof(FileTestCases))]
    public void RoundTripFile(
        string testName, // for logging; Xunit truncates the paths early 🙄
        string inputFile,
        string ecojiReferenceFile)
    {
        Assert.NotNull(testName); // hush warning xUnit1026

        const int wrap = 76; // reference ecoji was produced with default wrap

        var inputBytes = File.ReadAllBytes(inputFile);
        var expectedEcojiString = File.ReadAllText(ecojiReferenceFile);
        
        var actualEcojiString = Ecoji.Encode(
            inputBytes, 
            new Ecoji.EncodingOptions(wrap: wrap, newLine: "\n"))
            + "\n"; // the reference ecoji command line tool appends a newline

        Assert.Equal(expectedEcojiString, actualEcojiString);

        var roundTripBytes = Ecoji.Decode(actualEcojiString);

        Assert.Equal(inputBytes, roundTripBytes);
    }

    [Fact]
    public void ImplicitWrapOption()
        => Ecoji.Encode("hello", 1);
}

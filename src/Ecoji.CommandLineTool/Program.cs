//
// Author:
//   Aaron Bockover <aaron@abock.dev>
//
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;

using Mono.Options;

static class Program
{
    const int ExitSuccess = 0;
    const int ExitShowHelp = 1;
    const int ExitShowVersion = 2;
    const int ExitArgumentsError = 3;
    const int ExitDataError = 2;

    static readonly string? version = typeof(Program)
        .Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion;

    static readonly string? copyright = typeof(Program)
        .Assembly
        .GetCustomAttribute<AssemblyCopyrightAttribute>()
        ?.Copyright;
        
    static int Main(string[] args)
    {
        var decode = false;
        var wrap = 76;
        var showHelp = false;
        var showVersion = false;

        var options = new OptionSet
        {
            { "usage: ecoji [OPTIONS]... [FILE]" },
            { "" },
            { "Encode or decode data as Unicode emojis. 😻🍹" },
            { "" },
            { "Options:" },
            {
                "d|decode",
                "Decode data.",
                v => decode = v != null
            },
            {
                "w|wrap=",
                "Wrap encoded lines after {COLS} characters (default 76). " +
                    "Use 0 to disable line wrapping.",
                (int v) => wrap = v
            },
            {
                "h|help",
                "Print this message.",
                v => showHelp = v != null
            },
            {
                "v|version",
                "Print version information.",
                v => showVersion = v != null
            }
        };

        void ShowHelp()
            => options.WriteOptionDescriptions(Console.Out);

        void ShowVersion()
        {
            Console.WriteLine($"Ecoji (.NET Core) version {version}");
            Console.WriteLine($"  Copyright   : {copyright}");
            Console.WriteLine($"  License     : MIT");
            Console.WriteLine($"  Source code : https://github.com/abock/dotnet-ecoji");
            Console.WriteLine();
            Console.WriteLine($"Based on Ecoji by Keith Turner:");
            Console.WriteLine($"  https://github.com/keith-turner/ecoji");
        }

        try
        {
            var positional = options.Parse(args);

            if (showHelp)
            {
                ShowHelp();
                return ExitShowHelp;
            }

            if (showVersion)
            {
                ShowVersion();
                return ExitShowVersion;
            }

            Stream inputStream;

            if (positional is null ||
                positional.Count == 0 ||
                positional[0] == "-")
                inputStream = Console.OpenStandardInput();
            else if (positional.Count == 1)
                inputStream = new FileStream(
                    positional[0],
                    FileMode.Open,
                    FileAccess.Read);
            else
                throw new Exception("more than one file was provided");

            try
            {
                using(inputStream)
                {
                    if (decode)
                    {
                        using var stdout = Console.OpenStandardOutput();
                        Ecoji.Decode(inputStream, stdout);
                    }
                    else
                    {
                        Ecoji.Encode(inputStream, Console.Out, wrap);
                    }
                }
            }
            catch (Exception e)
            {
                WriteError($"pipe or encoding/decoding error: {e}");
                return ExitDataError;
            }

            return ExitSuccess;
        }
        catch (Exception e)
        {
            WriteError(e.Message);
            ShowHelp();
            return ExitArgumentsError;
        }
        
        static void WriteError(string error)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine($"error: {error}");
            Console.Error.WriteLine();
            Console.ResetColor();
        }
    }
}

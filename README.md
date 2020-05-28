# Ecoji .NET (🏣🔉🦐🔼🍈😯🥀🐥)

😻 Encode and decode data using emoji in .NET❣️ Like base64, except base1024, and uses an emoji alphabet. 🎉

Ecoji .NET is a .NET Standard 2.0 implementation of [Keith Turner's Ecoji Standard](https://github.com/keith-turner/ecoji).

In addition to the tiny `netstandard2.0` library, a .NET Core command line tool is available.

## Use It In Your App

```bash
$ dotnet add package ecoji
```

### C# Example

```csharp
> string encoded = Ecoji.Encode("Ecoji .NET")
"🏣🔉🦐🔼🍈😯🥀🐥"

> Ecoji.Decode("🏣🔉🦐🔼🍈😯🥀🐥")
"Ecoji .NET"
```

## Install the Command Line Tool

```bash
$ dotnet tool install -g dotnet-ecoji
```

### Example

```bash
$ echo -m "Ecoji .NET" | ecoji
🏣🔉🦐🔼🍈😯🥀🐥

$ echo -n "🏣🔉🦐🔼🍈😯🥀🐥" | ecoji -d
Ecoji .NET
```

### Usage

```
usage: ecoji [OPTIONS]... [FILE]

Encode or decode data as Unicode emojis. 😻🍹

Options:
  -d, --decode               Decode data.
  -w, --wrap=COLS            Wrap encoded lines after COLS characters
                               (default 76). Use 0 to disable line
                               wrapping.
  -h, --help                 Print this message.
  -v, --version              Print version information.
  ```

## Build & Contribute

* [Install .NET Core 3.1](https://dotnet.microsoft.com/download)
* From the root of the checkout you may:
  * Run `dotnet build` to build everything
  * Run `dotnet run -p src/Ecoji.CommandLineTool` to run the command line tool
  * Run `dotnet test` to run the unit tests
* Please file issues and pull requests if you feel so compelled! 🍿
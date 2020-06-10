# Ecoji .NET: Encode Your Data With Emoji 😻

Encode and decode data using emoji in .NET. Like base64, except base1024, and uses an emoji alphabet. Ecoji .NET is a .NET Standard 2.0 implementation of [Keith Turner's Ecoji Standard](https://github.com/keith-turner/ecoji). In addition to the tiny `netstandard2.0` library, a .NET Core command line tool is available.

## Use It In Your App

```bash
$ dotnet add package ecoji
```

### C# Example

```csharp
> string encoded = Ecoji.Encode("Ecoji .NET")
"🏣🔉🦐🔼🍈😯🥀🐥"

> Ecoji.DecodeUtf8("🏣🔉🦐🔼🍈😯🥀🐥")
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

```text
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
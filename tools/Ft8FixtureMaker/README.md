# Ft8FixtureMaker — the command Tim runs at the shack

**One capture in, one committed fixture out, no editing afterwards.**

```
dotnet run --project tools/Ft8FixtureMaker -- <capture.wav>
```

It computes the capture's SHA-256, runs WSJT-X's decoder over the WAV, parses the
rows, and writes the fixture beside the capture with the same stem and its
`provenance` set to `wsjtx`. Then commit the two files together. The format is
`docs/ft8-capture-fixture-format.md`.

## Options

| option | what it does |
|---|---|
| `--decoder <path>` | Where WSJT-X's decoder is. Checked first, and if it is wrong the command says so rather than looking elsewhere. |
| `--arguments <text>` | What to pass the decoder. Default `-8 "<capture>"`. |
| `--utc <stamp>` | `yyyy-MM-ddTHH:mm:ssZ`. Otherwise read from the file name if it ends `-YYYY-MM-DD-HHMMSS`, and from the file's own timestamp if it does not. |

## How the decoder is located, exactly

In this order, and the refusal names every place it looked:

1. `--decoder <path>`, if given. **An explicit path that is wrong is a refusal, not
   a reason to search** — running something else quietly is worse than stopping.
2. The `WSJTX_DECODER` environment variable.
3. These paths, which are the standard Windows install layout and **are not
   verified on the development machine** because there is nothing there to verify
   them against:
   - `C:\WSJT\wsjtx\bin\jt9.exe`
   - `C:\Program Files\WSJT\wsjtx\bin\jt9.exe`
   - `C:\Program Files (x86)\WSJT\wsjtx\bin\jt9.exe`
   - `C:\Program Files\WSJT-X\bin\jt9.exe`

**Nothing is ever substituted for it.** Not `decode_ft8.exe`, which is `ft8_lib`
and is the thing this project measures itself against; and not Hamlet's own
decoder, which would make the fixture a measurement of Hamlet against Hamlet.

## What is tested here and what is not

**Tested on the development machine**, against decode text committed under
`tests/fixtures/ft8/parser-inputs/` — see `Ft8FixtureGeneratorTests`:

- the hashing;
- the row parsing, including three separate malformed lines, each refused by name;
- the fixture writing, and that what it writes reads back through the same reader
  every later session uses;
- the loud refusal when the decoder is not found, naming every path it tried;
- the loud refusal when the decoder produced nothing;
- that a refusal leaves **nothing at all** behind — no half-fixture, no partial
  file, not even under the temporary name the writer stages through.

**Not tested here, and unexercised until Tim's first run:** starting WSJT-X and
getting real rows back. There is no WSJT-X on the development machine and no unit
may assume one.

## If the first run refuses

**That is the design, not a defect.** The parser is strict on purpose: it accepts
lines of the shape `HHMMSS  snr  dt  freqHz  ~  message` and **refuses anything
else rather than skipping it**, because a fixture short by an unknown number of
rows is a fixture nothing downstream can tell is wrong.

Where that shape came from is documented in full at the top of
`tests/Ft8Sharp.Tests/Fixtures/WsjtxDecodeLines.cs`: upstream `ft8_lib`'s own print
format, quoted verbatim in `ReferenceRecordings.cs`, which imitates WSJT-X's
display. **No WSJT-X source was read and none may be.** So if a real run prints
something this refuses, the refusal carries the offending line verbatim — send that
line back and the parser is corrected against it. That is the right way round.

The same applies to `--arguments`: `-8 <file>` is the shape `jt9` is invoked with
for an FT8 file, it is **not verified on this machine**, and `--arguments` exists so
a wrong guess about a switch is correctable at the shack without editing a file.

## Running it where `dotnet run` is refused

Inside a Claude Code session the allow-list permits `dotnet build` and not
`dotnet run`. `make-fixture.proj` reaches the same program the same way unit 243's
`.proj` files reach the arbiter's scripts:

```
dotnet build tools/Ft8FixtureMaker/make-fixture.proj -p:Capture=path\to\capture.wav
```

It changes nothing about what the maker does. **At the shack, use `dotnet run`.**

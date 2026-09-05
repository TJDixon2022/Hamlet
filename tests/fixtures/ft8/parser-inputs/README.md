# Parser inputs — NOT fixtures, and NOT anybody's decode of anything

**Nothing in this folder is a measurement.** These files are text in the shape a
decoder prints, committed so that `WsjtxDecodeLines` and `Ft8FixtureGenerator` can
be unit-tested against a file rather than against a string built inside a test.

**They are not WSJT-X's output.** They were written by hand, by unit 244, on a
machine that has never had WSJT-X on it. They make no claim about any audio, there
is no capture beside them, and no `.fixture.txt` here is or ever becomes one — the
files carry the extension `.decode.txt` precisely so they can never be mistaken for
a fixture and can never be read by `Ft8CaptureFixture`.

**Where the shape came from** is documented at the top of
`tests/Ft8Sharp.Tests/Fixtures/WsjtxDecodeLines.cs` and in
`docs/ft8-capture-fixture-format.md`: upstream `ft8_lib`'s own print format, quoted
in `ReferenceRecordings.cs`, which imitates WSJT-X's display. No WSJT-X source was
read.

| file | what it exercises |
|---|---|
| `good.decode.txt` | four well-formed lines, the ordinary case |
| `nothing.decode.txt` | a decoder that returned no decodes — must refuse, not write an empty fixture |
| `short-line.decode.txt` | a line with too few fields |
| `bad-number.decode.txt` | a line whose SNR is not a number |
| `no-tilde.decode.txt` | a line with something other than a tilde where the tilde goes |

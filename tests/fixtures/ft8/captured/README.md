# FT8 captures, and the WSJT-X fixtures beside them

**This folder is empty of captures today, and that is the correct state on the
development machine.** `SHACK_FACTS.md` FACT-004 records that the radio lives on a
different computer; there is no WSJT-X here and no unit may assume one. Zero real
fixtures passes cleanly and is not a defect.

A fixture that *names* a capture which is not here is a different thing entirely,
and it is a hard failure — see `Ft8CaptureFixture.RequireCapture`.

## What goes here

One pair per capture, same stem, committed together:

```
<stem>.wav            the audio, exactly as it was recorded
<stem>.fixture.txt    what WSJT-X returned for it, message by message
```

The format is `docs/ft8-capture-fixture-format.md`. Only fixtures whose
`provenance` reads `wsjtx` may be scored against; the worked example in
`../example/` is readable and is refused for scoring.

## How a pair gets here

Tim runs one command at the shack, over one capture, and commits the result. There
is no editing step afterwards, and a fixture that could not be completed is not
written at all — a half-fixture is worse than no fixture, because a reader will
read it happily.

```
dotnet run --project tools/Ft8FixtureMaker -- <capture.wav>
```

See `tools/Ft8FixtureMaker/README.md`.

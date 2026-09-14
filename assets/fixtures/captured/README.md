# Captures off the air

**This folder is where real PSK31 audio goes, and it is the only folder in this
repository whose contents were not made by a program.**

Everything under `assets/fixtures/` was made by `reference-modem.py` on a machine with no
radio (FACT-004, FACT-006). That is what makes those fixtures repeatable, and it is also
why the demodulator could read every one of them perfectly while the first row off 7.070
came up garbled on 2026-09-13 with nothing in the tree able to say why.

## How to put one here

1. On the PSK31 panel, press **Capture 2 minutes**. The label counts down and then reads
   *captured*, and the line under the waterfall says where the file went.
2. The file is at `%AppData%\Hamlet\captures\psk31-<UTC timestamp>.wav` — 48 kHz, 16-bit
   mono, the device stream as the sound card handed it over, before Hamlet resamples it.
3. Copy it into this folder and commit it.

`ThePsk31DemodulatorTests.EveryCaptureOffTheAirIsReadBack` then runs the whole receive
path over every file here on each run, through the same resampler the application uses,
and prints the carriers it found, the characters each one emitted and the first sixty of
them.

## What that test does and does not assert

**It asserts that the file decodes at all, and nothing about what came out.** Nobody knows
what those stations sent, so an error rate or a character floor here would be a claim about
a transcript that does not exist (§0.0, HM-DEC-091). What it is for is comparing two
versions of the demodulator on the same real audio.

**An empty folder passes rather than skips**, because xUnit 2.9 has no runtime skip and the
package that would add one is not ours to add. The test prints `NO CAPTURE READ` in that
case, so a green tick is never mistaken for evidence about real air.

## Before committing one

Recorded off-air audio may carry callsigns and message content. That is public by nature,
and `CLAUDE.md` §2.1 still asks that fixtures committed to the public repository be
reviewed by Tim first.

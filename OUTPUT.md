# What Claude did

**Hamlet confirmed.** `Hamlet.sln` and `src\Hamlet.RadioEngine\Cw\CwCaseRoster.cs`
exist; `CoreHMI.sln` and `src\CoreHMI` do not. Development machine, **no radio**
(HM-DEC-093) — nothing in this report is evidence about the radio. Branch `main`,
**committed and not pushed**.

**Both tasks done.**

## Task 1 — the row carries what Hamlet read

**Placement: the new `text` column goes ninth, immediately before `read`, and
`read` stays last.** Two reasons, and the second is the one that decided it.

The readable one: every column to the left of `read` is something Hamlet measured,
and `text` is the last and largest of those. Putting it after the verdict would
split his column away from the right-hand edge, where it is found by eye without
counting across.

The load-bearing one: `read` being the **final** column is what makes an empty cell
visibly empty. A row ending in a tab and nothing is unmistakable; a row with an
empty cell in the middle of it is a column somebody forgot to fill in. The whole
point of that column is that Hamlet never writes to it (HM-DEC-091), so it should
be the one place on the row where blank is obviously deliberate.

**What the column contains.** `Transcript.Tail(120)` at the moment of the press —
what he was looking at when he decided there was a station there. A hundred and
twenty is `CwTranscript.LongestTip`'s own figure and carries several overs at any
speed.

**Nothing can split the row.** `CwCaseRoster.Readable` replaces tab, carriage
return and newline with a single space and trims. The decoder emits none of those
today; the file tomorrow is scored from should not depend on that staying true.

**An empty transcript says `nothing read`, in words.** Not blank. A station heard
and nothing read is the case the whole measure exists to count, and it must not
look like an unfilled cell.

**The sidecar got it too, in full** rather than tailed — `Transcript.PlainText`,
through the same `Readable`. It was not difficult, so it is done rather than
skipped.

**One thing I changed that the order did not name, and why.** The sidecar's new
line is labelled **`text`**, not `read`. `read` is the name of the roster's
operator column, and two fields one letter apart — one a machine's output, one a
person's judgement — is a confusion waiting for the evening somebody scores thirty
of them. If you want them both called `read`, say so and it is one word.

## Task 2 — proved on real audio, in the existing tests

The three task-4 tests were **extended**, not duplicated.
`tests\fixtures\cw\captured\cw-2026-08-18-004507.wav` through `BufferedAudioSource`,
as before. Two new cases were added beside them for the two conditions the end-to-end
test cannot produce — an empty transcript and a transcript containing a tab.

Five tests, all green. What they now assert:

- a press after real decoding carries what the decoder emitted, and the cell is
  **not** `nothing read`;
- the row is **ten** columns, and the tenth is empty;
- a press the freshness guard refused still lands a row **with the text column
  filled** — he heard the station whether or not a recording was written;
- an empty transcript renders `nothing read` and is not blank;
- a transcript carrying `CQ\tDE\r\nW1AW K` still produces **one line with exactly
  nine tabs**.

**The gap from last session is unchanged and still stated**: this drives the
decoder, the tap, the WAV writer and the roster, not `CaptureAudioAsync` itself.
That command's decoder is fed by `OpenAudioInput()` and there is no seam to hand it
a file.

**Nothing was recorded to `DECISIONS.md` this session.**

# What Tim should expect

- **The roster gains a column and the operator's column has not moved** — it is
  still the last one and still empty.
- **A roster from tonight will be wider.** It is still tab-separated and still
  lines up in a text editor, but a row now runs to the width of the decoded text.
  A spreadsheet handles it without comment.
- **The sidecar beside each recording now ends with a `text` line** carrying the
  whole transcript rather than only counts. That is the thing last session's report
  said was missing.
- **2,007 tests, 3 failing** — the same three, by name:
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`. Two more tests than
  last session and the same three red.
- **No decoder behaviour changed.** `CwGate`, `CwSettledPass`, `CwToneSurvey` and
  `CwDecoder` are untouched, so tonight still measures the decoder you have been
  running.
- **Committed, not pushed**, on `main`.

# What you should see

A real roster from the test run, on the real capture, printed by the test itself:

```
time      frequency  band   wav                                     toneHz  snrDb  wpm  chars                 text                                               read
23:14:05  7.030      40 m   cw-2026-08-19-231405.wav                505     42.7   22   19 emitted, 6 unsure  N L D O T NET ■E ECH STATION HANDNG AHIS MESAGE P
23:14:25  7.030      40 m   none (no new audio since the last one)  505     42.7   22   19 emitted, 6 unsure  N L D O T NET ■E ECH STATION HANDNG AHIS MESAGE P
```

**That is the whole change, and you can score both rows without opening a file.**
The first is a case Hamlet half-read: `STATION` and `NET` are there, `HANDNG` and
`MESAGE` are a letter short each, and the opening is gone. Whether that counts as
read is your call, which is the point — but you can make it from the sheet.

The second row is the freshness guard refusing a duplicate. It kept no audio and it
**still carries the text**, so a case with no recording is still scorable rather
than being a row you have to skip.

**A station you hear that Hamlet reads nothing of** will read `nothing read` in
that column. That row is the one the percentage turns on, and it will not look like
a blank you forgot to fill in.

# What's blocking us

Nothing blocks tonight.

## Asks still outstanding

Seven, per HM-DEC-139 and scoped by HM-DEC-140.

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling |
| **Whether HM-DEC-097 is satisfied by existing silence** (HM-OPEN-052) | 2026-08-19 | The ruling; a floor would be 19.8 |
| **Whether `ShortestVote` goes 5 to 7** (HM-OPEN-053) | 2026-08-19 | The ruling. **Carried forward unchanged**: 13 → 27 of 43 on the bulletin's leading edge against five synthesized tests, two on acquisition. Still held out so the instrument and the subject do not move together |
| **How the settled pass tells keying from a carrier** (HM-OPEN-054) | 2026-08-19 | The ruling; HM-DEC-143 is recorded and unbuilt until it has one |

Nothing was dropped from the queue this session.

# What Claude did

**Hamlet confirmed.** `Hamlet.sln` and `src\Hamlet.RadioEngine\Cw\CwGate.cs` exist;
`CoreHMI.sln` and `src\CoreHMI` do not. Development machine, **no radio**
(HM-DEC-093), branch `main`, **committed and not pushed**.

**Tasks 1 to 4 done. Task 5 dropped**, whole, as the order allows.

## Task 1 — the trace, and what the instruction got wrong

**1. What one press writes today.** `CaptureAudioAsync` at line 3177 —
`[RelayCommand]`, as described. It refreshes the frequency, takes
`_decoder.Tap.Snapshot()`, and on success writes two files into
`%AppData%\Hamlet\captures`: `cw-<yyyy-MM-dd-HHmmss>.wav` and the same stamp
`.txt`.

**2. Is the button available when nothing is being read? Yes, confirmed, and this
is the load-bearing fact.** `IsEnabled="{Binding IsDecoding}"` at
`MainWindow.axaml:960`, and `IsDecoding = true` is set at
`MainWindowViewModel.cs:2864` **when the decoder starts listening** — beside
`_decoder.Listen(_audioInput)` — not when it produces text. So the press is live on
a station Hamlet reads nothing of, which is the case the measure exists for.

**3. No roster, index or per-evening file exists anywhere.** Grepped across `src`
and `tests`. This unit is not smaller than it looks.

**4. What the sidecar says about whether a station was found.** `toneHz`, `snrDb`,
`elements` (seen and resolved), `characters` (emitted and unsure), `decoderWpm`,
and `sinceLast`. **A human cannot decide "Hamlet got this one" from the sidecar
alone**, because it records no decoded text at all — only counts. That is worth
knowing before the scoring evening.

**5. `SettingsStore.DataFolder`** is `C:\Users\TimDi\AppData\Roaming\Hamlet`, so
captures go to `…\Hamlet\captures`. **That folder does not exist on this machine** —
no capture has ever been written here, which is consistent with the dev machine
having no radio.

**Mismatches between the instruction and the tree, reported and not repaired:**

- The sidecar list omits **`sinceLast`**, which is present, and omits the rig block
  written after the header.
- `SettingsStore.DataFolder` is declared on `AppSettings.cs:401`, not in a
  `SettingsStore.cs`; `SettingsStore` is a static class in that same file. Every
  path in the instruction is otherwise correct, including both line numbers.
- **The status cadence asks for `TASK: n of m`, and `CLAUDE.md` §13.1 defines the
  field as `PHASE`.** The panel reads the leading run of `KEY: value` lines and was
  built for the six fields §13 names. I wrote **both** — `PHASE` for the panel and
  `TASK` as instructed — rather than pick one silently. If `TASK` is meant to
  replace `PHASE`, that is a change to §13 and is yours.

## Task 2 — the press marks a case

The button reads **I hear a station**, and its line above reads: *press this
whenever you can hear a station, whether or not anything appeared here.* The
tooltip says it asserts you heard CW here whether or not Hamlet read any of it, and
that you mark afterwards which ones it read.

**Its behaviour is unchanged** — same WAV, same sidecar, same freshness guard.

**Both refusal paths now mark the case**: no audio arriving, and the freshness
guard declining a duplicate. **HM-DEC-090 is untouched**; what changed is that its
refusal becomes a row with its reason instead of a status line nobody keeps.

## Task 3 — the roster

`CwCaseRoster` appends one tab-separated row per press to
`cases-<yyyy-MM-dd>.txt` in the captures folder, header on the first row of each
evening. Real output, from task 4's run:

```
time      frequency  band   wav                                     toneHz  snrDb  wpm  chars                 read
23:14:05  7.030      40 m   cw-2026-08-19-231405.wav                505     42.7   22   19 emitted, 6 unsure
23:14:25  7.030      40 m   none (no new audio since the last one)  505     42.7   22   19 emitted, 6 unsure
```

Every column comes from a reading that already exists, or says it does not:
`none`, `unread`, `not tracking` (HM-DEC-091). **`read` is empty and nothing
writes to it** — not derived, not defaulted, not guessed from the character count.

## Task 4 — proved without a radio

`tests\fixtures\cw\captured\cw-2026-08-18-004507.wav` played through
`BufferedAudioSource`, the same replay source the application uses. Three tests,
all green, writing only to a temporary folder.

**What it does not cover, stated rather than implied:** it drives the decoder, the
tap, the WAV writer and the roster, **not `CaptureAudioAsync` itself**. That
command's decoder is fed by `OpenAudioInput()` and there is no seam to hand it a
file; adding one changes the decode start path, which this unit does not touch. So
the components are proved on real audio and the view model's own wiring of them is
not.

## Task 5 — dropped

The scorer, dropped whole rather than half-built. Rows can be counted in a text
editor tomorrow; a case cannot be marked tonight without tasks 2 to 4.

# What Tim should expect

- **The button is renamed and says what it now means.** Same press, same files.
- **A new file appears in `%AppData%\Hamlet\captures`** the first time you press
  tonight: `cases-2026-08-19.txt`. It is tab-separated, so it will line up in a
  text editor and open cleanly in a spreadsheet if you want to sort it.
- **The last column is blank on purpose.** That is your verdict and nothing in
  Hamlet will ever put anything in it.
- **A press that writes no recording still lands a row**, saying `none` and why.
  Those are the cases the old behaviour lost silently, and they are the ones that
  matter most to the percentage.
- **2,005 tests, 3 failing** — the three named in the order and nothing else.
- **Committed, not pushed**, on `main`.
- **No decoder behaviour changed.** `CwGate`, `CwSettledPass`, `CwToneSurvey` and
  `CwDecoder` are untouched, so tonight's roster measures the decoder you have been
  running.

# What you should see

**Yes — you can mark a case tonight in one press, and it will survive the evening.**
The evidence is task 4, run on a real capture rather than asserted:

- a WAV appeared;
- a roster row appeared carrying the decoder's own numbers at that moment — 505 Hz,
  42.7 dB, 22 wpm, 19 emitted and 6 unsure;
- a second press with no new audio wrote **no** recording and still landed a row
  reading `none (no new audio since the last one)`;
- exactly one WAV existed afterwards, so the guard held.

**What that gives you tonight.** Every station you hear gets one press. At the end
you have a list of cases with the audio beside them, and you mark the last column
for the ones Hamlet actually read. The percentage is a division you do afterwards,
on your judgement, from evidence that includes the stations it missed — which is
the only way the number can mean anything.

**One thing to know before you start**: the sidecar records counts and never the
decoded text, so when you score a row you will want the transcript from the screen
or the audio itself. If you want the text kept alongside, say so and it is a small
change.

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
| **Whether `ShortestVote` goes 5 to 7** (HM-OPEN-053) | 2026-08-19 | The ruling. **Carried forward unchanged**: 13 → 27 of 43 on the bulletin's leading edge against five synthesized tests, two on acquisition. Held out of tonight so the instrument and the subject do not move together |
| **How the settled pass tells keying from a carrier** (HM-OPEN-054) | 2026-08-19 | The ruling; HM-DEC-143 is recorded and unbuilt until it has one |

Nothing was dropped from the queue this session.

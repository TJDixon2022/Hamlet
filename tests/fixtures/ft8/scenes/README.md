# Scene corpora - what a stretch of slots on one frequency looked like

**A scene is twelve consecutive FT8 slots with several stations in them, held as
text.** It exists so that the contact ledger and the four row states can be
proved against a run of slots in which stations answer, overlap, go quiet and
finish, without a radio, without an audio device, and without anybody typing the
decodes in.

There is one scene here today:

| File | Slots | Decode lines | Made by |
|---|---|---|---|
| `unit257-band-scene.corpus.txt` | 12 | 21 | unit 257's script, run back through the decoder and committed by unit 258 |

---

## THIS IS SYNTHESIZED. IT IS NOT A CAPTURE.

**Every signal in this scene was made by Hamlet's own encoder,
`Ft8Composer.Compose`, and read back by Hamlet's own decoder,
`Ft8DeepSlotDecoder`.** No radio was on. No antenna was connected. Nothing here
came off 14.074 MHz and nothing here came out of WSJT-X.

**So it may never be scored against the decoder's accuracy.** A decoder reading
back the encoder that sits beside it in the same repository tells you that the
two agree with each other. It does not tell you that either of them agrees with
the air, where signals arrive with fading, drift, splatter, and other people's
transmissions in the same 50 Hz. `tests/fixtures/ft8/captured/README.md` states
the rule this file obeys: **only fixtures whose `provenance` reads `wsjtx` may be
scored against**, and this scene carries no provenance line at all, on purpose.

**What it is legitimate evidence for** is the thing it is used for: given a run
of slots containing these messages, does the ledger book the right stations, hold
the right history each way, count the right number of slots, and read the right
state. That is bookkeeping over text, and the text is as good as any other text
for it.

**What it must never be quoted as** is a sensitivity figure, a decode rate, a
comparison against WSJT-X, or evidence that Hamlet hears well.

---

## How it is made, and why the test regenerates it

`tests/Hamlet.RadioEngine.Tests/Contacts/Ft8BandScene.cs` is the **script** - what
went in. `TheBandSceneIsWhatHamletsDecoderReadTests` composes every signal at its
station's own frequency, sums the signals in each slot, scales the sum to 0.8 of
full scale so nothing clips, decodes the slot once, and **asserts that this file
is exactly what came back**.

**The breakage that test exists to catch is a corpus written from the script.**
Such a file would look identical, every ledger assertion downstream would pass,
and the whole of it would be evidence about a text file rather than about
anything Hamlet can do. If you edit this file by hand, that test goes red, and it
is right to.

**No `.wav` is committed beside it.** The audio is regenerated deterministically
in about four seconds; a committed binary would be a second copy of it.

Measured on 2026-09-06 by unit 258, on the committed script: **21 signals
composed, 21 decoded, none lost, 3.91 s wall clock** for all twelve slots.

---

## What Tim would run at the shack to make a real one

The scene above is a stand-in for a capture that this machine cannot make -
`SHACK_FACTS.md` FACT-004 says the radio is not here. To make the real thing:

1. On 14.074 MHz, with WSJT-X running beside Hamlet on the IC-7300's USB audio,
   record a run of consecutive slots busy enough to have a station working two or
   three others at once in them.
2. Per capture, `dotnet run --project tools/Ft8FixtureMaker -- <capture.wav>` -
   the one command `tests/fixtures/ft8/captured/README.md` names, with no editing
   step afterwards.
3. The result goes in `tests/fixtures/ft8/captured/`, **not here**, and it is the
   one that may carry `provenance: wsjtx` and may be scored against.

**Do not move this file there, and do not give it that provenance.**

---

## The format

```
# hamlet-ft8-scene v1
operator: KC3QIS
slot0utc: 2026-09-06T18:00:00Z
# slot | hz | message
   0 |   700 | JA1ZZ G4XYZ -08
```

`#` is a comment. `operator:` names whose station the scene is written from -
which the ledger takes as a constructor parameter and never reads from settings.
`slot0utc:` is the UTC boundary slot 0 opened on; slot *n* opens
`n * Ft8Slots.SlotSeconds` after it, and the fifteen is not written down a second
time anywhere. Every other line is `slot | hz | message`, in slot order and then
in the decoder's own order within a slot - which is why the lines in slots 4, 6,
8 and 10 are not in the order the script lists them.

`Ft8SceneCorpus` reads and writes it, lives in the test project, and nothing
under `src/` knows this file exists.

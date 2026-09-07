# Step D at the radio - one page

**PROJECT: Hamlet**

Unit 268, task 6. **Written down, not done.** Nothing here was performed, no
default was changed, no control was added, and `SHACK_FACTS.md` was not touched.
**No number is chosen on this page** - the drive level your radio wants is the one
number nobody in this repository can know, which is why it is step D and why it is
yours.

Step C closed tonight. It was the last thing about Hamlet's transmit side that can
be proved without a radio; everything left is you at your own station.

> **CORRECTED BY UNIT 269, 2026-09-07.** This page was written when the drive
> control was only in Settings and the level was only in the Send area line.
> **Both have moved onto the Digital tab, under the waterfall, where step D's own
> criterion says you read them**, and there is now a second figure beside the
> first that is a measurement rather than your own setting read back. Sections 1
> and 2 are rewritten to match the tree; the procedure in section 3 and the
> `SHACK_FACTS.md` list in section 4 are unchanged, and **no number on this page
> is chosen for you.**

---

## What you are doing, in one sentence

**Turning Hamlet's transmit level up until your radio's own ALC meter just begins to
move, then backing it off** - and writing down where you left it.

That is it. Everything below is where the controls are and what the numbers mean.

---

## Before you start

**This is the shack machine, not the development machine** (`SHACK_FACTS.md`
FACT-004). Everything measured tonight was on the development machine, which has
never had a radio on it, and **none of it says anything about your IC-7300's USB
codec** - not the sample rate, not the levels, not the endpoint names.

Two things Hamlet will refuse to transmit without, both in **Settings**:

- **Transmit** - the render endpoint. This must be the radio's own USB audio
  input, not the computer's speakers. **Hamlet will not choose one for you**: with
  none named it refuses to transmit rather than playing FT8 tones into whatever
  Windows defaults to.
- Your **callsign**, **grid** and **licence class**. Hamlet will not transmit
  outside your privileges and will say so instead.

---

## 1. Where the Transmit drive control is

**On the Digital tab, in the Send area under the waterfall, below the CQ and Stop
buttons.** You do not open anything to reach it and nothing covers the band while
you use it.

```
[ CQ ]   [ Stop ]

Nothing has been sent. Right-click a decoded row to choose a message, or press CQ.

Transmit drive  [   25 ]  % of full scale
How hard Hamlet drives the radio's input - -12.0 dBFS at this setting. ...

Nothing has been transmitted yet, so there is no measured level. ...
```

- It is a spinner in **percent of full scale**, from 1 to 100.
- **It will not take a value Hamlet would refuse.** The line underneath says so and
  the setting keeps the last usable value rather than storing one the send path
  would reject at the moment you pressed send.
- It saves as you change it. There is no OK button to miss.
- **It is still in Settings too**, under the Transmit endpoint picker, exactly as
  it was. Nothing was taken away. **Both controls write the same one setting**, so
  it does not matter which you use and they cannot disagree.

Underneath it, the line that changes as you turn the number:

> How hard Hamlet drives the radio's input - **-12.0 dBFS** at this setting. This
> is a starting point, not a specification. Set it against your own radio's ALC
> meter: turn it up until the ALC just begins to move and then back off. Full
> scale is a wide, distorted signal over other people's band, which is why Hamlet
> starts at 25 %.

**That line is there before you transmit anything** and it moves as you move the
spinner, so you can find the level you want without keying the radio to see what
you set. Until unit 269 you could not: the only level Hamlet showed was in the
Send area after a transmission had already gone out.

**Hamlet ships at 25 %, which is -12.0 dBFS.** That is a deliberately quiet
starting point chosen on the bench, not a figure anybody here knows to be right for
your radio.

### What changed, in the terms that matter at the rig

|  | This morning | Now |
|---|---|---|
| Windows to open | **1** - Settings, modal | **0** |
| Controls to cross | **4** | **1** |
| Hidden while you do it | the waterfall, the decode table, **and the Stop button** | nothing |

The old sequence, between two fifteen-second slots: open Settings, which puts a
window over the band, over the decoded table and over the Stop button; find the
Transmit section; move the spinner; close the window; right-click a station; wait
a slot; read a sentence in the Send area; open Settings again. **Slot boundaries
keep arriving the whole time the dialog is up** - a transmission you had already
armed still goes out behind it, and you cannot see it go or reach Stop.

The new sequence: move the spinner, read the line under it.

---

## 2. What the dBFS readout and the clip count mean

**There are now TWO figures under the waterfall after each send, and they are
different quantities.** Telling them apart is the whole of this section.

*(The two quotations below are the shape of the lines, with the default drive
filled in. They are not readings off your radio - nothing in this repository has
ever seen it.)*

**The first**, in the Send area line, is what Hamlet *built*:

> Sent to W1ABC, "W1ABC KC3QIS RRR" in the slot at 15:10:45 UTC. **It was composed
> at -12.0 dBFS with nothing clipped** - that is the level Hamlet built, before this
> machine's own volume for that device and before the radio's input gain. Set the
> radio's drive against its own ALC meter. The clipped count here is of the audio
> Hamlet built, and the composer will not build above full scale, so on this path
> it is always none. What the sound card actually had to clamp is the measured
> line under the drive control.

**The second**, below the drive control, is what the sound card was *handed*:

> **The sound card was handed -12.0 dBFS** - that is the peak the endpoint actually
> got, measured on the way out after clamping, and not the level Hamlet composed
> at. **Nothing had to be clamped.** Beyond this point are Windows' own volume for
> that device and the radio's input gain, which Hamlet cannot see.

**Why both.** The first is your own setting read back to you. The second is a
measurement of what came out the other end of Hamlet. On an ordinary evening they
will be the same number, and that is fine - **the point is that when they are not,
you can see it**, and you are never in the position of setting your ALC against a
figure you typed while believing you had verified it.

**dBFS** - decibels below full scale. **0.0 dBFS is as loud as the number format
can go and every figure you want is negative.** Each 6 dB is a halving: -6.0 is
half of full scale, -12.0 is a quarter, -20.0 is a tenth. **Louder is a smaller
number of decibels below zero**, which is the one thing about this unit that reads
backwards at first.

### The clip count - what it tells you and what it does not

There are two clip counts on the screen and only one of them can ever move.

**The one in the Send area line cannot.** It counts samples of the audio Hamlet
built that fall outside the scale, and Hamlet will not build such a sample:
measured on 2026-09-07, at a drive of **1.0 - the highest the composer accepts, 0.0
dBFS** - a whole slot at 48000 Hz was **606720 samples, largest magnitude exactly
1.000000, and 0 outside the rails**. It is arithmetic, not a reading of your drive.
**Do not treat "nothing clipped" there as evidence that your level is safe.** It
would say that at any drive Hamlet allows.

**The one under the drive control can.** It is the count the sound card's own
output path had to clamp on the way to the endpoint. If anything between Hamlet's
array and the device ever squares a sample off, that is where it shows.

**What clipping is, if you do see it:** a clipped sample is not a slightly loud
signal; it is a corner in the waveform, and corners are splatter on either side of
your transmission, on frequencies other people are using. **If the measured count
is above zero, turn the drive down and send again** - do not go on the air deciding
whether a few is acceptable.

**And what neither count can see:** everything past the sound card. **Three things
are NOT these numbers**, and they multiply together on the way to your antenna:

1. **These numbers** - the level Hamlet builds at, and the level the card was
   handed. The drive control sets the first.
2. **Windows' own volume for that endpoint.** Hamlet neither sets it nor reads it.
   If it is not at 100 % you are working against a slider Hamlet cannot see.
3. **The radio's own input gain** - the IC-7300's USB MOD level menu setting.

So the same drive percentage can be wildly different power on the air on two
machines. **This is exactly why the number is yours and not the repository's.**

---

## 3. What to watch on your own ALC

**Your ALC meter is the instrument, not Hamlet's readout.** Hamlet's figure says
what left the computer; the ALC says what the radio thinks of it.

The procedure, and it is the ordinary digital-modes one:

1. Set the radio's RF power where you want it. **Set power with the power control,
   never with the drive level.**
2. Start at Hamlet's default, or lower.
3. Send one message and **watch the ALC while the 12.6 seconds are going out**.
4. **Turn the drive up in small steps until the ALC meter just begins to lift off
   its rest, then back off until it does not move at all.**
5. Confirm the power meter is reading roughly what you set, steadily, for the whole
   transmission.

**What you are looking for is an ALC that does not move.** For SSB voice, ALC
action is normal. **For a digital mode it is not**: FT8 is a constant-envelope
single tone, so any ALC action means the radio is compressing something that has no
headroom to give, and the result is a wide signal. **ALC deflection on FT8 is the
fault condition, not the target.**

**Things that would be worth stopping for:**

- **The measured clip count - the one under the drive control - is not zero.**
  Turn the drive down. The count in the Send area line is not this one and cannot
  move; see section 2.
- **The ALC swings hard however far you turn the drive down.** That points at the
  radio's USB MOD gain or Windows' volume rather than at Hamlet's number.
- **Power reads far below what you set with the ALC quiet.** Drive too low; there
  is nothing wrong with being quiet, but it is worth knowing which side of the
  window you are on.
- **The power meter wanders during one transmission.** FT8 is a steady tone and
  should read flat for the whole 12.6 seconds.

**No confirmation dialog, no power limit and no test mode stands between you and
the air.** One right-click is one transmission, in the next slot, immediately. It
was proved tonight that the message on the menu item is the message that leaves the
machine, and that the Stop button takes a transmission off the card from the middle
of it - **4.71 seconds of a 12.64-second slot went out and the card was quiet 456 ms
after the press**, on the bench. Yours is the first one that goes anywhere.

---

## 4. What step D asks you to write into `SHACK_FACTS.md` afterwards

`PHASE_PLAN.md` step D's three must-pass criteria, in your terms. The third one is
the one that has to survive the evening:

> The value recorded in `SHACK_FACTS.md`, so no later unit promises to defer it
> again.

**A new fact, in the format the file already uses** - `id:`, `status: standing`,
`source: operator statement, <date>`, and then, in whatever words you like:

1. **The drive percentage you left the control at, and the dBFS Hamlet showed
   beside it.** Both, because the percentage alone is meaningless if a default
   changes. **And the measured figure** - what the line under the drive control
   said the sound card was handed - if it was not the same number, because that
   difference is the only thing on the screen that nobody could have predicted.
2. **Which endpoint** was selected in Settings, by the name Windows gives it.
3. **The Windows volume for that endpoint**, if it is not 100 %.
4. **The radio's own USB MOD level**, so the number above can be reproduced.
5. **What the ALC did at that setting, in your words** - that is a must-pass
   criterion of step D on its own, and "it never moved" is the whole answer if
   that is what happened.
6. **The band and frequency** you set it on, and the power you were running.
7. **Anything that surprised you.**

**Do not let a session write this fact for you.** The file's own rule is that a
fact in it outranks any inference a session draws from its own reading, and its
whole reason for existing is that three sessions once advised changing radio
settings that were already correct.

---

## What is not on this page, deliberately

- **A drive number.** Not 25 %, not anything else. The default is a starting point
  and this page does not defend it.
- **A dummy load.** It is not a stage, not a fallback, and not referenced -
  withdrawn in full on 2026-09-06.
- **Anything about your IC-7300's USB codec.** FACT-004: it has never been present
  on the machine any of tonight's figures came off.
- **Step E.** Working a station comes after this, and it is a different evening.

---

## The mismatch this page used to carry, and what became of it

**It is closed.** This page said, when unit 268 wrote it:

> `PHASE_PLAN.md` step D's first criterion says you read the dBFS and clip count
> **"under the waterfall"**. **In the tree they are in the Send area line after
> each send** ... and there is no such readout under the waterfall. **Reported, not
> repaired.**

Unit 269 repaired it. **The drive control and both figures are under the waterfall
now**, out of the modal dialog, and the second figure is a measurement of what the
sound card was handed rather than your own setting read back.

**That does not close any of step D's three criteria.** All three are yours, at
your own radio: what your ALC does at the level you choose, and the value you write
into `SHACK_FACTS.md`, are not things a machine with no radio on it can answer.
What unit 269 removed is the obstacle - you no longer have to hide the band and the
Stop button behind a dialog to move the control, and you are no longer setting an
ALC against a number nobody measured.

# Step D at the radio - one page

**PROJECT: Hamlet**

Unit 268, task 6. **Written down, not done.** Nothing here was performed, no
default was changed, no control was added, and `SHACK_FACTS.md` was not touched.
**No number is chosen on this page** - the drive level your radio wants is the one
number nobody in this repository can know, which is why it is step D and why it is
yours.

Step C closed tonight. It was the last thing about Hamlet's transmit side that can
be proved without a radio; everything left is you at your own station.

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

**Settings, in the audio block, directly under the Transmit endpoint picker** -
they are the same question asked twice: which output, and how loud into it.

```
Transmit         [ radio's USB audio endpoint ▾ ]
Transmit drive   [   25.0 ]  % of full scale
```

- It is a spinner in **percent of full scale**, from 1 to 100.
- **It will not take a value Hamlet would refuse.** If you type one, the line
  underneath says so and the setting keeps the last usable value rather than
  storing one the send path would reject at the moment you pressed send.
- It saves as you change it. There is no OK button to miss.

Underneath it, the line that changes as you turn the number:

> How hard Hamlet drives the radio's input - **-12.0 dBFS** at this setting. This
> is a starting point, not a specification. Set it against your own radio's ALC
> meter: turn it up until the ALC just begins to move and then back off. Full
> scale is a wide, distorted signal over other people's band, which is why Hamlet
> starts at 25 %.

**Hamlet ships at 25 %, which is -12.0 dBFS.** That is a deliberately quiet
starting point chosen on the bench, not a figure anybody here knows to be right for
your radio.

---

## 2. What the dBFS readout and the clip count mean

**Where you read them: the Send area line, after each send** - not on the
waterfall. It reads like this, and the first two figures are the ones step D is
about:

> Sent to W1ABC, "W1ABC KC3QIS RRR" in the slot at 15:10:45 UTC. **It was composed
> at -12.0 dBFS with nothing clipped** - that is the level Hamlet built, before this
> machine's own volume for that device and before the radio's input gain. Set the
> radio's drive against its own ALC meter.

**dBFS** - decibels below full scale. **0.0 dBFS is as loud as the number format
can go and every figure you want is negative.** Each 6 dB is a halving: -6.0 is
half of full scale, -12.0 is a quarter, -20.0 is a tenth. **Louder is a smaller
number of decibels below zero**, which is the one thing about this unit that reads
backwards at first.

**Clipped** - how many individual samples ran into the end of the scale and had to
be squared off. **"nothing clipped" is the only acceptable reading.** A clipped
sample is not a slightly loud signal; it is a corner in the waveform, and corners
are splatter on either side of your transmission, on frequencies other people are
using. **If you ever see a clip count above zero, turn the drive down and send
again** - do not go on the air deciding whether a few is acceptable.

**Three things are NOT this number**, and they multiply together on the way to your
antenna:

1. **This number** - the level Hamlet builds the audio at. It is what the drive
   control sets.
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

- **The clip count is not zero.** Drive too high; turn it down.
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
   changes.
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

## One mismatch to be aware of

`PHASE_PLAN.md` step D's first criterion says you read the dBFS and clip count
**"under the waterfall"**. **In the tree they are in the Send area line after each
send**, quoted in section 2 above, and there is no such readout under the
waterfall. **Reported, not repaired** - the figures exist and are readable, so the
criterion is satisfiable as written except for where it says to look.

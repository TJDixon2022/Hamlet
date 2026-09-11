# Reading the PSK31 record

**Work instruction 322 task 5.** Two fixtures played through the running PSK31 path with
telemetry on, and **the diagnosis a person would make from the `.jsonl` alone** — no
screenshot, no access to the machine, nothing but the file.

**Both files were produced on a machine with no radio** (FACT-006), from synthetic
fixtures made by the author's own modem (FACT-004). Everything below is an indication and
never a finding about the air. What it does prove is that **the file answers the questions
somebody would ask of it**, which is the whole of what this unit was for.

---

## 1. Four signals: what a working evening looks like

**The file: 46 events, 13,708 bytes, 38 of them `psk31`.** That is 38.6 seconds of audio,
so about **355 bytes a second of listening** — a fifteen-minute session would be about
320 kB.

### Was it listening, and on what terms?

```
psk31_listening_started
  dialHz 7028000 · passband 66 to 3934 Hz · sampleRate 8000
  squelchQuality 0.9 · retireSeconds 1
```

**Read straight off: it was listening across the whole passband, not one spot**, at a
squelch of 0.90, and it would let go of a carrier that stopped being keyed for one second.
The dial says 7.028 MHz, **which is not a PSK31 frequency** — this was a bench replay with
no radio, and the dial is whatever the training radio was left on. On a real evening that
field is the first thing to check: a dial that is not on the watering hole explains an
empty list on its own.

### Was there any audio at all?

```
psk31_audio_level  peak 0.0 dB · rms -8.6 dB · floor -8.6 dB · clipping true
```

**The fixture is full scale**, so clipping reads true. On a real evening this is the line
that separates *the band was quiet* from *the sound card was not delivering anything*: a
peak near −90 with `nearlySilent true` is a cable or a device, not a band.

### What did the search see?

**Five `psk31_search_pass` events**, because the rule writes one every ten seconds and
**always on a pass where the carrier set changed.** The first is the interesting one:

```
pass sampled · 10 candidates · 0 held · none crossed
   1601 Hz  +5.0 dB  quality 0
    701 Hz  +2.7 dB  quality 0
   1100 Hz  -0.8 dB  quality 0
   2196 Hz  -3.2 dB  quality 0
```

**Ten places with energy and not one of them taken yet** — quality 0 means the pass had
not measured enough symbols to judge the keying. That is a search still making up its mind,
not a search refusing.

Twelve milliseconds later:

```
pass changed · 10 candidates · 4 held · 4 crossed
   1600 Hz  +4.0 dB  quality 0.986  crossed
    700 Hz  +1.5 dB  quality 0.975  crossed
   1100 Hz  -1.5 dB  quality 0.943  crossed
   2200 Hz  -4.5 dB  quality 0.880  crossed
   1985 Hz  -16.9 dB quality 0.234  -
   1758 Hz  -18.3 dB quality 0.322  -
```

**This single line is the answer to the question the operator asked.** Four real signals
crossed with coherence 0.88 to 0.99; two more places in the noise measured 0.23 and 0.32
and were refused. A reader can see at a glance both *what was taken* and *what was there
and rejected*, and the numbers say why.

### Which carriers appeared, and when?

```
psk31_carrier_appeared  id 1  2200.0 Hz  -4.5 dB  quality 0.880  after 7 passes
psk31_carrier_appeared  id 2  1600.2 Hz  +4.0 dB  quality 0.986  after 7 passes
psk31_carrier_appeared  id 3  1100.0 Hz  -1.5 dB  quality 0.943  after 7 passes
psk31_carrier_appeared  id 4   700.0 Hz  +1.5 dB  quality 0.975  after 7 passes
```

All four within a fraction of a hertz of where the fixture put them, each after seven
passes as a candidate. **The weakest, at 2200 Hz and −4.5 dB, crossed on 0.880** — just
above the 0.6 the search asks for, and well below the 0.986 of the strongest.

### What did the squelch and the demodulators do?

```
psk31_squelch  700.0 Hz   open true  quality 0.993  threshold 0.9
psk31_squelch  1100.0 Hz  open true  quality 0.984  threshold 0.9
psk31_squelch  1600.2 Hz  open true  quality 0.995  threshold 0.9
psk31_squelch  2200.0 Hz  open true  quality 0.980  threshold 0.9
```

**Every one opened, and every one with a margin.** The closest to the gate is 0.980
against 0.900. On a real signal this is where a too-tight squelch would show: a carrier
held, quality 0.85, and no `open true` line ever.

```
psk31_lock  1600.5 Hz  locked true  afcHz -0.1
```

The AFC on the drifting station moved by a tenth of a hertz over the pass. **`locked` here
is a proxy** — see the caveat at the end.

### What was read, and by whom?

**Ten `psk31_line_parsed` events**, each carrying an offset, a kind, and three flags:

```
1100.0 Hz  Answer  certain true  turnover true  toOperator true   21 characters
1602.7 Hz  Cq      certain true  turnover true  toOperator false  38 characters
 700.0 Hz  Cq      certain true  turnover true  toOperator false  38 characters
2200.0 Hz  Report  certain true  turnover true  toOperator false  69 characters
1100.0 Hz  Report  certain true  turnover true  toOperator false  76 characters
```

**One line is addressed to the operator** — the answer at 1100 Hz — and a reader can see
that without any callsign being in the file. The offset is how you tell which station said
it; `toOperator` is how you tell it was for you.

### How did each one end?

```
id 3  1099.2 Hz  LostLock          lifetime 28.8 s  101 chars  2 lines  2.0 s since it last spoke
id 2  1607.9 Hz  LostLock          lifetime 33.8 s  120 chars  3 lines  1.8 s
id 4   699.2 Hz  LostLock          lifetime 34.7 s  120 chars  3 lines  2.5 s
id 1  2200.0 Hz  ListeningStopped  lifetime 38.6 s  142 chars  2 lines  0.1 s
```

**Three stations finished and were let go; the fourth was still being heard when the tab
was left.** Every carrier that appeared is accounted for, and the reason distinguishes a
station that stopped from one the operator walked away from.

```
psk31_listening_stopped  38.6 s · 4 carriers · 142 characters · 10 lines
```

**The summary agrees with the detail**: four carriers appeared, four retired, and the
38.6 seconds matches the longest lifetime exactly, because that carrier was there from the
first pass to the last.

---

## 2. Noise only: what an empty evening looks like, and why it is not the same

**The file: 12 events, 4,803 bytes, 4 of them `psk31`.** Thirty seconds of listening.

```
psk31_listening_started  passband 66 to 3934 Hz · squelch 0.9 · retire 1 s
psk31_audio_level        peak -16.2 dB · rms -26.2 dB · clipping false · nearlySilent false
psk31_search_pass        sampled · 15 candidates · 0 held · none crossed
   1690 Hz  -11.1 dB  quality 0
   3321 Hz  -12.8 dB  quality 0
    750 Hz  -13.1 dB  quality 0
psk31_listening_stopped  30.0 s · 0 carriers · 0 characters · 0 lines
```

**Here is the diagnosis, and it is the one the whole unit exists to make possible.**

- **The audio was present.** −16.2 dB peak, not nearly silent. So this is not a dead sound
  card, not a muted radio, not a cable.
- **The search was running and found fifteen places worth measuring.** So this is not a
  search that failed to start.
- **Not one of them crossed**, and every one reported quality 0 — nothing there is keyed
  like BPSK at all.
- **Therefore: the band was noise.** Not a squelch set too tight, not a decoder that never
  ran. If the squelch had been the problem, the candidates would carry coherence figures in
  the 0.4 to 0.8 range and still read `crossed: false`, and that is a different picture
  entirely.

**On the screen these two evenings are the same empty list.** In the file they are four
events apart and unmistakable.

---

## 3. What the file still cannot tell you

Each of these is in `output.md` section 4 by name.

- **`locked` is a proxy, not a bit-clock lock.** The demodulator does not expose one, so
  the event reports *this channel has produced characters*. It is useful — it separates a
  carrier being read from one being held and silent — but it is not what the name promises,
  and a reader should know that.
- **The sampling cadence is on the wall clock while every duration is in audio seconds.**
  That is deliberate: sampling exists to throttle writes to a file, which is a real-time
  concern. It shows up only in a replay, where thirty-eight seconds of audio produce one
  `psk31_audio_level` instead of four.
- **Nothing says what the operator could see.** The record says a line was parsed; it does
  not say whether the row was on screen, scrolled away, or filtered out.
- **Nothing on the transmit side has a production call site yet.** The events exist and are
  proved at the bench; the press half of step 4 is blocked on the owner's rulings.

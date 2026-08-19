# 1. What Claude did

## The commit that made `27 11` automatic

**`8c2abf3`, version 1.8.0**, "ask the radio for its spectrum instead of advising
about it". It put `_ = AskForTheSpectrumAsync(radio)` in the connect path, and
from that build Hamlet wrote the scope's data output on at every connect.

**HM-DEC-062 forbids it in terms**: *nothing turns the scope on, that is a write,
and this ruling is reads only.* Taking it out restores a standing ruling rather
than departing from one, so it needed no new authority. The read of `27 10` and
`27 11` to say what is on stays, which is the half that ruling allows.

The arithmetic is why it matters rather than being tidy. A sweep is 475 points in
eleven parts, on the order of six hundred bytes; 115200 8N1 carries about eleven
and a half thousand bytes a second. Nineteen sweeps a second is the whole cable,
and the dial's own announcements share it. HM-OPEN-042 then found the readback
could not confirm that write, so **Hamlet has been reporting it refused since
1.8.0 without being able to tell whether it succeeded.**

## What the diagnostics show that they could not show yesterday

- **One sentence under the readout, in his terms**: whether the radio announces
  the moment he touches the dial, or whether Hamlet is asking it several times a
  second instead, or whether nothing has been heard yet. Three worlds, three
  sentences, through `VoiceTests` like everything else he reads.
- **The frequency says when it is old.** Older than a second and a half, on a
  connected radio, and the readout carries "where the radio was a moment ago, not
  where it is now" instead of being drawn as though it were now (HM-DEC-111). It
  says nothing while the reading is fresh.
- **The numbers behind it on the diagnostics screen**: frames in, how many were
  the radio announcing itself, how the frequency was last confirmed and how long
  ago, and what share of everything arriving is the spectrum picture. That screen
  showed forty values with their ages and never one line about the conversation
  carrying them.

## The fix

**The frequency comes off session cadence and is asked for at the live rate.**
HM-DEC-109 put it there as a backstop for a broadcast missed at startup; with the
broadcast not arriving, the backstop was carrying the whole load at half-minute
cadence, while `RigStateMonitor` repainted four times a second holding it
(HM-DEC-078). The display was current about a value that was not.

**And the radio announcing still wins.** `RigPollPlan.SkipLiveRead` keeps Hamlet
quiet while the model holds a frequency the radio volunteered less than a second
and a half ago, so on a rig with transceive on nothing goes on the bus at all,
which is the behavior HM-DEC-050 wanted. When the announcements stop, the poll
takes over inside that window. It is not a setting, because a preference would be
a way to configure the app back into this failure.

**`1A 05 0071` is read at connect**, never written, so Hamlet can say plainly when
the radio is not announcing rather than tracking at poll speed and looking broken.
**Its page is not cited and that is said on its face**: §4 has no row for this
sub-command, it came from this work order, and the row reads `uncited
(HM-OPEN-043)`. `CitationTests` accepts exactly that shape and **proves it names a
live open issue**, so the marker cannot become a way around the citation sweep.

## The six tests that failed, adjudicated one at a time

Each was a belief this work overturns, and each carries its reason where it sits.

| Test | Why it failed, and what it says now |
|---|---|
| `RigPollingTests.TheBroadcastFieldsAreSweptAnyway` | Asserted the frequency is *not* asked for four times a second, on the reasoning that it "hardly ever changes". The evening disproved that sentence. |
| `FrequencySweepTests.ItsFreshnessWindowIsTheOrdinaryOne` | Two minutes was the ordinary window for a field swept twice a minute. A thirty second lag would have counted as current. |
| `DialTrackingTests.TheFrequencyIsNeverLeftToThePollAlone` | Mine, from last session, pinning `Session`. |
| `ScopeStreamTests.TheWaterfallSaysWhichSettingIsMissing` | Expected "Hamlet is asking it to", which was only true while the write existed. |
| `CitationTests` and `RigStateModelTests` page sweeps | Met the first uncited row in the file. Both now accept the marked shape, and one new test proves the marker names a live open item. |

## Recorded under §12.1

**Nothing.** The cadence change weighs bus traffic against freshness, which is a
trade-off and therefore yours (clause 3). It is built, as the order directs, and
the ruling ask is in section 4.

# 2. What Tim should expect

**Yes: the frequency on screen now follows the dial in under a second, and it
does so whether or not your radio announces anything.** It is read four times a
second, 250 milliseconds, the same beat as the S-meter, so the worst case between
your hand moving and the screen agreeing is a quarter of a second plus the radio's
own answer. The thirty seconds is gone because the thirty second sweep is no
longer what maintains it.

**What makes that a fix rather than a hope:** it does not depend on the broadcast
arriving. If your radio is announcing, Hamlet stays quiet and the announcement is
faster still. If it is not, the poll carries it. Either way the number is under
1.5 seconds old, and if it ever is not, the readout now says so instead of drawing
it confidently.

- **The waterfall will stay dark and will now say why in your terms.** Hamlet no
  longer asks the radio to send its spectrum, because HM-DEC-062 says it may not.
  If you want it, the switch is on the radio.
- **You will see a new line under the frequency readout.** On a radio with
  transceive on it says so. With it off it says Hamlet is asking instead, and that
  turning CI-V Transceive on would be quicker and quieter and is yours to change.
- **Radio → What the radio is doing now carries the link counts**, including what
  share of the cable is spectrum data.
- **Build succeeds, no warnings. 1,956 tests, 2 failing, both the standing decode
  baseline** — `ClearingTheTranscriptLeavesTheDecoderAlone` and
  `TheBulletinDecodesToItsAnswerKey`. Neither is touched by this work.
- **One commit, pushed to `main`. Nothing local, no branches.**
- **No radio was connected** (HM-DEC-093). Everything above is what the code now
  does, proved by 1,522 engine tests including eleven written for this failure. It
  is not a measurement of your station, and I have not made a connect a condition
  of anything.

# 3. What we should do next

- Run it. If the line under the readout says your radio is not announcing, CI-V
  Transceive is one menu away and would take the poll off the bus entirely.
- HM-OPEN-043: one column-aware read of `A7292-4EX-6` around 19-4 and 19-5 to cite
  `1A 05 0071`, after which that row looks like every other.
- The waterfall, whenever you want it: it needs the ruling in section 4 or the
  switch on the radio.

# 4. What's blocking us

Nothing is blocked. Two rulings wanted, and the standing pair unchanged.

---
date: 2026-08-18
refs: HM-DEC-050, HM-DEC-109, HM-DEC-078, src/Hamlet.RadioEngine/Rig/RigPollPlan.cs
---

**The frequency is read at the live rate whenever the radio has not announced it
within a second and a half, superseding HM-DEC-109 on the cadence and nothing
else.**

HM-DEC-050 rations the bus and says nothing the radio volunteers is polled for.
HM-DEC-109 amended it to sweep the frequency every thirty seconds as a backstop.
Neither anticipated the backstop being the only mechanism, which is the state the
app has been in: the operator turned his dial and watched a thirty second lag,
repeatedly.

Weighed, which is why it is yours: a frequency read is six bytes out and eleven
back, so four times a second is under seventy bytes on a cable carrying eleven
thousand, against a bus HM-DEC-050 deliberately protected. What tips it is that
this is the number every other surface trusts, and a wrong band scopes RBN and the
skimmer watch (HM-DEC-024, HM-DEC-075).

Rejected: an unconditional fast read, which would spend the bus on a radio that is
already announcing; and a setting, which would be a way to configure the app back
into the failure.

Built, per the order. Overturn it and the thirty seconds comes back.

---
date: 2026-08-18
refs: HM-DEC-062, HM-DEC-092, HM-OPEN-042
---

**Whether Hamlet may ever ask the radio to send its spectrum, and if so, when.**

`27 11` was written automatically from 1.8.0 against HM-DEC-062's reads-only rule.
That has been removed and the ruling restored, so the waterfall stays dark until
the operator switches the output on at the radio.

HM-DEC-092 ruled the write in, on the reasoning that attempting it and reporting
the answer beats guessing which setting is at fault. **The part of that reasoning
which has since failed is the reporting**: HM-OPEN-042 found the answer could not
be read back, so the write has been reporting refused without knowing.

Three ways: leave it off and let the switch be his; ask once, on a button, with
the answer now readable; or ask automatically again once the frame counters show
the stream is not eating the link.

Rejected: leaving it automatic while the ruling forbidding it stands, and while
its cost on a shared cable is a real number rather than a worry.

---

The two standing questions are unchanged and still yours: **whether an attended
automatic cycle may reach an antenna** (§0.2, HM-DEC-098), awaiting the interlocks
watched into the load; and **a callsign too long for one keyer send**
(HM-DEC-130), refused until the seam between two sends is measured into the load.

---

## Named and left, as the order directs

Not started, and none of them belongs in this order: HM-OPEN-042's remaining
rungs; the record sweep for rulings resting on a write outcome; `DECISIONS.md`
missing 096 to 133; HM-DEC-135 and §9.6; mode follow, favorites and the recent
list.

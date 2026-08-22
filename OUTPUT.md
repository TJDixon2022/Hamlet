# OUTPUT.md

## 1. What Claude did

**There is still no per-file accuracy table against ARLP034. Two of the three
things the checklist asks for do not exist on this machine, and the third does not
exist on arrl.org.**

### The checklist, searched rather than assumed

| what | found? | where I looked |
|---|---|---|
| the seven pairs `cw-2026-08-22-031838` … `-032129` | **no, none of the fourteen files** | `%APPDATA%\Hamlet` — which has **no `captures` folder at all**, and that is where the app writes them (`SettingsStore.DataFolder\captures`); `%LOCALAPPDATA%\Hamlet`; Downloads, Desktop, Documents, OneDrive; `C:\Source`; a sweep of `C:\` six levels deep excluding Windows and Program Files; all of `D:` |
| `data/vendor/arrl/arlp034-2026-08-21.txt` | **could not be fetched: the bulletin is not published** | the ARRL propagation archive's most recent entry is **ARLP033, 14 August 2026**; `?issue=2026-08-21&code=ARLP034` returns no bulletin; `/w1aw-bulletins-archive/ARLP034/2026` is empty; the news page's latest propagation item is from February 2025 |
| `tests/fixtures/logs/2026-08-22.jsonl` | **no** | `%APPDATA%\Hamlet\telemetry` holds `2026-08-13` through `2026-08-17` and nothing later |
| `ANALYSIS-cw-2026-08-22-014113.md`, `ANALYSIS-w1aw-arlp034-2026-08-22.md` | **no** | the whole search above |

**Nothing was committed for step 4, because nothing was found to commit.**

**The likeliest explanation is that this is not the machine the radio is on.** The
app has never created a captures folder here and its telemetry stops on the 17th,
while the captures under test were made on the 18th, the 20th and the 22nd
(`SHACK_FACTS.md`, HM-DEC-093). **The files almost certainly exist on the shack
computer**, and nothing here can reach them.

**And ARLP034 may not exist yet.** The archive's latest is ARLP033 from the 14th.
If W1AW was heard sending a propagation bulletin at 03:18 UTC on the 22nd, the text
the ARRL will publish for it is not on the website at the time of this session.

### Phase 0 — already shipped

The ratio penalty went in last session as `7f2e90f` and nothing about it changed
here. The reference carries the same change and
`ItReadsWhatTheReferenceReads` passes.

### Phase 2 — why the wrong speed wins, measured on a file that is here

`cw-2026-08-18-004507` **is a W1AW bulletin capture**, sent at the same standard
18 words a minute, and it is in the fixtures. **The clock fault reproduces on it**,
so the question was answered without the seven files.

Per-hop likelihood by speed hypothesis, the same units the sidecar calls "better
than silence":

| | 8 | 12 | 16 | **18** | 22 | 24 | **28** | 32 |
|---|---|---|---|---|---|---|---|---|
| the whole 30 seconds | 27.56 | 32.31 | 32.48 | **32.49** | 32.45 | 32.46 | 32.41 | 32.35 |
| the window from 6 s | 28.79 | 33.03 | 33.11 | 33.12 | 33.07 | 33.10 | **33.15** | 33.11 |
| the window from 12 s | 25.65 | 30.69 | 30.78 | 30.79 | 30.80 | **30.80** | 30.74 | 30.72 |
| the window from 18 s | 22.70 | 27.65 | 27.81 | 27.82 | 27.84 | **27.84** | 27.84 | 27.70 |

**The answer to "why did 28 score higher than 18" is that almost nothing scored
higher.** Across the entire grid above ten words a minute the spread is **0.05 out
of 33, about a seventh of one per cent**, and the winner is decided in the fourth
significant figure. Over the whole thirty seconds the true speed does win, by 0.08.
**Over the twelve-second window the streaming path actually reads, it does not** —
28 wins one window, 24 wins two, 18 wins one.

**So this is the same finding as yesterday's, sharpened**: the discrimination
exists, it is about a tenth of a per cent, and it needs two and a half times more
audio than the streaming window holds. The three candidate mechanisms the order
lists are all downstream of that: nothing is rewarding more character boundaries,
because there is nothing to reward with — the score is dominated by the per-hop
evidence term, which barely changes when the same runs are relabelled at a
different unit.

**Phase 2's "done" cannot be reached from here**: it is defined as the sweep
selecting 18±1 on all seven files, and the seven files are not here. **What is here
says the fix is not a patch to the sweep but a decision about what to do when the
best hypothesis is not meaningfully better than its neighbours**, which is section
4's first ask.

### Phase 4 — the noise reference, measured on the captures that are here

**The mechanism the analysis names does not reproduce, and the inflation does.**

Measured with the decoder's own sixty-hertz quadrature filter as an independent
instrument, taking the median at each pitch over the whole file:

| recording | Hamlet reports | median tone over median band noise | within 250 Hz |
|---|---|---|---|
| `cw-2026-08-18-004507` | **47.8 dB** | 11.7 dB | 10.6 dB |
| `cw-2026-08-18-003016` | **42.4 dB** | 14.7 dB | 12.3 dB |
| `cw-2026-08-20-014854`, **holding no station** | **54.7 dB, tone latched** | 14.9 dB | 5.1 dB |

**It is not the filter skirt.** Across the tracker's whole 300 to 900 hertz bank
the bins at either end sit within a few decibels of the middle on all three files,
and confining the noise reference to within 250 hertz of the station changes the
answer by **1.1 dB** on `004507` and **2.4 dB** on `003016` — not the thirty the
analysis attributes it to. On the audio that is here, the reference is not being
taken in a skirt, because there is no skirt to take it in.

**What it is instead**: `SnrDb` is an instantaneous key-down power over a median
band noise, and the panel figure is a held peak of that ratio decaying at 0.005 dB
a hop. Three maxima stacked. HM-DEC-090 ruled the held peak deliberately and that
ruling is not in question; **what the peak is taken over is.**

**The empty-band case is the §0.0 one.** `cw-2026-08-20-014854` holds no keying,
the decoder correctly emits nothing from it, **and the panel would say 54.7 dB with
the tone latched.** Noise has peaks too. Filed as **HM-OPEN-056** with the numbers,
because both candidate answers change what the display asserts (§12.1).

### Phases 1, 3, 5 and 6

**Dropped whole and named as dropped**, all four for the same missing evidence.
Phase 1 aligns against a vendored transcript that could not be vendored; phase 3
locates `WITH`→`WINH` and `OF`→`OOT` in audio that is not here; phase 5 reads a
jsonl that is not here and was named in advance as the drop. Phase 6's ratchet has
nothing to ratchet, and its version bump is done below.

**The version moved 1.10.8 to 1.10.9.**

## 2. What Tim should expect

**Nothing on screen changed tonight**, and the one capture that could stand in for
ARLP034 still reads `E AT ARRL DOT NET <BT> EACH STATION HANDLING ET HIS M E S S A
G E P E` against a bulletin that says "at ARRL dot net. Each station handling this
message" — so no, he will not see more CW than yesterday.

Build clean, no warnings, version 1.10.9. The suite is untouched: **32 failing, the
same 32 by name as when the ratio penalty shipped**, two of which are the
`ScopeStreamTests` flake that passes when run alone (`HM-OPEN-055`).

**What will look wrong and is not:** `OPEN_ISSUES.md` has a new entry at the top
and nothing else in the tree moved. That is the whole of tonight's code change.

## 3. What we should do next

- **Get the seven captures, the log and the two analyses off the shack computer**,
  by whatever route moves files between the two machines. Everything in phases 1
  through 5 is waiting on them and on nothing else.
- **ARLP034 may need to wait for the ARRL to publish it.** ARLP033 of the 14th is
  the latest on the site; if the bulletin heard on the 22nd is 034, its text will
  appear at `w1aw-bulletins-archive/ARLP034/2026`.
- **Rule on what Hamlet says when no speed is meaningfully better than another**,
  section 4. It is the whole of the clock fault and it is not fixable by choosing
  differently among near-identical scores.
- **HM-OPEN-056**, the signal-to-noise figure, which says 54.7 dB about a recording
  holding nothing.

## 4. What's blocking us

**The evidence, for the second session running.**

### RECORDED

Nothing was recorded to `DECISIONS.md`. One open issue was filed: **HM-OPEN-056**,
the inflated signal-to-noise figure, with the measurements above.

### NEEDS A RULING

> **What Hamlet reports when no speed hypothesis is meaningfully better than its
> neighbours.**
>
> Measured on an 18 WPM machine sender that is in the fixtures: the likelihood
> spread across the whole grid above ten words a minute is 0.05 out of 33. The
> winner over thirty seconds is the true speed by 0.08; the winner over the twelve
> seconds the streaming path reads is 28, or 24, or 18, depending on which window.
>
> | | say the winner anyway | say unknown when the margin is small | widen the window until the margin is real |
> |---|---|---|---|
> | what the operator sees | a number that is wrong by half on real audio | a blank where a number was, most of the time | the right number, later |
> | §0.0 | a guess presented as a measurement | a marked unknown | honest and slow |
> | cost | none, it is today's behaviour | the speed badge goes quiet on most captures | more than twelve seconds of audio before any speed is named |
> | what it needs | nothing | a margin threshold, which is a number to choose | measuring how much audio makes 0.08 reliable |
>
> **The industry-standard answer is the second**: §0.0 already says a marked
> unknown beats a wrong number, and HM-DEC-090's own precedent is to say nothing
> rather than to name a figure the measurement will not support. It is Tim's
> because it decides what the display asserts, and because the margin threshold is
> a number nobody has measured yet.

> **The signal-to-noise figure, HM-OPEN-056.** Filed rather than tabled here, with
> its numbers.

### STATE

The gate said `PROJECT: Hamlet` and the tree agrees: `Hamlet.sln` at the root,
`Hamlet.*` namespaces, `PROJECT_CARD.md` naming Hamlet. **This session ran on the
development computer with no radio connected, so nothing in this report is evidence
about the radio** (`SHACK_FACTS.md`, HM-DEC-093). The checklist search covered both
fixed drives and the application's own data folder.

Phase 0 was already shipped. **Phase 2 was worked against
`cw-2026-08-18-004507`**, a W1AW bulletin capture at the same speed that is in the
tree, and its diagnosis is above; its acceptance criterion needs the seven files.
**Phase 4 was measured and produced a filed issue rather than a change.** **Phases
1, 3, 5 and 6 were dropped whole**, and none of them was half-built.

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- **The seven W1AW captures, the ARLP034 text, the 2026-08-22 log and both
  analysis documents are not in the tree**, and the searches that failed to find
  them are named above.
- The likelihood is flat in speed above eleven words a minute — **now measured at
  0.05 out of 33 on an 18 WPM sender**.
- Whether the fast end is worth the slow end.
- Whether a sender change can be decided by pitch distance at all — measured dead.
- Whether the window clear comes back on.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting two incompatible things about one span.
- Whether the sidecar's `text` should include the leading edge.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-OPEN-056, HM-DEC-130, HM-DEC-098,
  HM-OPEN-033, HM-OPEN-007.

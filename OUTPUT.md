READ IN THIS ORDER

A. THE PHASE GOAL. Everything this project has built reaches the operator's
   screen, and the decoder is taken as far as it will go. **Every step of this
   phase is closed** — step 6 closed in unit 251 on your ruling of 2026-09-05.
B. THE STEP AND ITS EXIT CRITERIA. None. This unit advances no step and was
   never meant to; it is operator-facing repair on a phase whose steps are
   already shut. The two criteria step 6 did not meet are still open by name as
   HM-OPEN-083 and HM-OPEN-084, and nothing here touches them.
C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears on A and not
   on B: the decoded list now filters the way you ruled it should, and the named
   drop candidate was built rather than dropped. Section 4 raises 3 items and
   **none of them asks for a ruling** — one is a measurement reported and
   deliberately not acted on, one is a mismatch between the instruction and the
   tree, and one is the standing asks queue, which this session could not
   rebuild and says so.

UNIT:       252 — complete at task 4 of 4 — 2026-09-06 10:00
PHASE GOAL: Everything this project has built reaches the operator's screen, and the decoder is taken as far as it will go. All steps closed.
UNIT GOAL:  CQ and mine are independent, both can be on at once, everything means everything, and what is not selected is not on the list.
ADVANCED:   yes — but not a step, and the instruction says so itself. What advanced is the panel: four operator-facing repairs, all four built, including the one unit 251 left.
NUMBER:     none — this unit is operator-facing repair and carries no scoreboard figure. The one number it produced is task 4's: the waterfall's first row went from 341 ms to 100 ms.
DRIFT:      0 consecutive units without advance  (was 0)

---

## 1. What Claude did

**Complete, four of four. Nothing was dropped, including the named drop
candidate.** Machine `QUIVERFULL`, `C:\Source\HamLet`, project claimed and
confirmed Hamlet, branch `main`, everything pushed.

### The gate

`SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent. **Hamlet confirmed.**

### One thing about this session that the next author should know

**It checked for a second session before it read the work order, and this time
there was not one.** `SESSION.lock` was empty and `Get-Process` showed one
`claude` and no harness `cmd`. That check is new — yesterday a second session
was started on unit 251 four minutes after the harness had already launched one,
both executed task 1, and commit `416b653` carried two copies of each of the two
open issues. It cost an evening and a duplicated record. **Nothing in the tree
enforces this**; it is the first item that would be worth a rule, and it is
section 4's.

### Shell refusals, recorded verbatim

**None. Not one call was refused in this session** — `git`, `dotnet`, `python`,
`grep` and `sed` all ran in every spelling tried, across eight builds, six
filtered test runs and four commits. `RUN_LEDGER.md` records 5 to 28 denials a
unit and unit 251 recorded four; this unit records zero. The file-editing tools
were used anyway wherever a heredoc would have had to carry backslashes or
CRLF, for a reason worth writing down: **this shell collapses `\\` to `\` inside
a quoted heredoc**, which turned `tools\arbiter` into a tab and a bell character
in a generated batch file yesterday. That is a tool fact, not a refusal.

### Task by task

**1 — CQ and mine become independent toggles.** The enum is gone.
`DecodedFilterRule.Wants` takes the two flags and **both** message fields, and
`ShowsEverything` is derived from the two rather than stored, so it cannot
disagree with them. Persisted as two keys, with unit 251's single
`DecodedFilter` string still read on load and carried forward — §6.1's second
exception, with the migration and the test it demands. The migration fires only
on a file that has neither new key, so turning a filter back off sticks.

**2 — a filtered row is off the list.** `DigitalDecodes` stays the whole table
and a new `DigitalVisibleDecodes` mirrors it through one `CollectionChanged`
subscription. Mirroring rather than filling both by hand, because a row joins or
leaves the whole table in four places — the decoder's door, the row cap, the
order toggle's rebuild, and two clears — and a second collection maintained at
each of them is four chances for the two to disagree about what was heard.

**3 — `mine` needs a callsign.** Two calls are the same station when their
longest slash-separated pieces agree. `W1ABCD` is the case that decides the
rule: a prefix test would claim another station's traffic as his.

**4 — the waterfall's first row.** Built, not dropped. Measured at 100 ms
against 341. See section 3.

### Decisions made on this session's own authority, reproduced in full

**One: `mine` with no callsign on file now matches nothing, where unit 251 had
it match everything.** Unit 251's answer was right while rows were dimmed — it
dimmed nothing and said so. It is wrong now that the filter removes: matching
everything would make the control do something other than what it says, and
**beside `CQ` it would quietly turn *both* into *everything***, which your
ruling forbids in as many words. The §0.0 hazard of an empty-looking band is
carried instead by the two things now on screen for exactly that: the summary's
hidden count, which is never omitted, and the amber note, which names the
missing callsign and the screen it is typed on. `0 shown · 6 hidden by mine`
under a line saying Hamlet does not know your callsign is not a quiet band and
cannot be read as one.

**Two: `INotifyPropertyChanged` came off `DigitalDecodeRow` rather than being
left behind empty.** It existed for `IsDimmed` and `RowOpacity`, both of which
went with the dimming. An event nobody raises is a promise the type cannot keep,
and a reader would reasonably conclude something on the row still moves.

**Three: the second test of task 4 was replaced by a record of what it
measured, rather than deleted or forced green.** It is in the tree as a skipped
`[Fact]` carrying the measurement in its own summary. Deleting it would have
destroyed the evidence; making it pass would have meant asserting something that
is not true.

## 2. What the owner should expect

**On the Digital tab, item by item:**

- **The filter strip reads `everything | CQ | mine`.** `CQ only` is now just
  `CQ`, because it no longer excludes anything by itself.
- **`CQ` and `mine` are separate switches.** Press both and you get every call
  to anyone plus every message you are in, and nothing else. That state did not
  exist before.
- **`everything` lights when neither is on**, and pressing it clears both.
  Pressing it when it is already lit does nothing rather than being greyed out.
- **A filtered row is gone from the table, not faint.**
- **The summary always says how many are hidden and which toggle did it** —
  `4 hidden by CQ`, `2 hidden by CQ and mine`. If it says nothing about hidden
  rows, nothing is hidden.
- **`mine` finds what you sent as well as what was sent to you**, and it counts
  `W1ABC/P`, `W1ABC/QRP` and `W4/W1ABC` as you.
- **With no callsign on file, `mine` now shows an empty table** and an amber line
  saying Hamlet does not know your callsign, that it is holding every message
  back, and to put it in Settings under Operator. **That is the changed
  behaviour** — before, `mine` with no callsign quietly did nothing.
- **Your filter is remembered between evenings**, and if you left unit 251's
  panel on `CQ only` or `mine`, it comes back on the matching toggle rather than
  resetting to everything.
- **The waterfall draws its first row about a quarter of a second sooner** when
  you open the tab.

**What will look wrong and is not:**

- **The first waterfall row is still black.** That is not the delay this unit
  fixed and it was black before too. Each bin is drawn against its own tracked
  noise floor, and on the very first frame that floor is set to that frame's own
  level, so every bin computes to zero by construction. Measured, with a
  continuous tone running the whole time: all eight loudest bins at 0. Section 4
  says why it was not fixed.
- **`PHASE_OUTCOME.md` carries two `## UNIT 251 - STEP 6` entries.** That is
  yesterday's two-session collision, left in place because that file is
  append-only. It is one unit, not two.

**Build:** clean, 0 warnings, 0 errors, whole solution, eight times.

**Tests:** only the tests this unit wrote were run, filtered, foregrounded, per
the standing rule. `TheDecodedListFiltersByCategoryTests` **50 of 50 passed**;
`TheWaterfallsFirstRowIsNotLateTests` **1 passed, 1 skipped** (the skipped one is
the record described above). No suite was run and nothing was backgrounded.

**Not run, and you should know which:** `TheDecodedPanelScrollsItselfTests` and
`TheDecodedColumnsLineUpTests` both touch the table this unit rewired. They add
rows straight to `DigitalDecodes`, and **the mirror is the reason they should
still pass** — that design was chosen partly so those tests would not need
touching. They compile, and they were not executed, because they live in the
`Views` namespace whose stall unit 230 documented and which the standing rule
exists to keep out of a unit.

**Pushed to `main`:** `d597bbe`, `8dba64f`, `cb5d014`, `a8ee6f0`. Version
**1.12.67 → 1.12.71**, a patch a task. `Ft8Sharp` and `Ft8Sharp.Deep` did not
move and no file under either changed.

## 3. What you should see

**1. The summary line, quoted, in each of the four toggle states.** Read off the
test's own output, six rows heard, callsign `KD9ABC`:

```
neither : 214135 UTC · 6 shown · newest first
CQ      : 214135 UTC · 2 shown · 4 hidden by CQ · newest first
mine    : 214135 UTC · 2 shown · 4 hidden by mine · newest first
both    : 214135 UTC · 4 shown · 2 hidden by CQ and mine · newest first
```

**The `neither` line carries no hidden clause at all, and that is deliberate.**
A line reading `0 hidden` on every unfiltered panel is noise that teaches the
operator to stop reading the place where the important number appears. The clause
is present exactly when something is being held back, which is when it matters.
**The clause names the toggle rather than saying "the filter"**, because a line
naming "the filter" names nothing he can turn off.

**2. What `mine` matches.** Either field — the addressee or the sender, because
it is his traffic and not his inbox, and a contact is two sides. Two calls are
the same station when their **longest slash-separated pieces** agree:

| Stored | Heard | Verdict |
|---|---|---|
| `W1ABC` | `W1ABC`, `w1abc` | his |
| `W1ABC` | `W1ABC/P`, `W1ABC/M`, `W1ABC/QRP` | his |
| `W1ABC` | `W4/W1ABC`, `VP2E/W1ABC` | his |
| `W4/W1ABC` | `W1ABC`, `W1ABC/P` | his |
| `W1ABC` | `W1ABCD`, `W1ABCD/P` | **somebody else** |
| `W1ABC` | `W1AB`, `KD9ABC` | somebody else |

**It is the longest piece and not the first or the last**, because FT8 puts the
added piece on either side and no rule about position covers both: `W4/W1ABC` is
a prefix and `W1ABC/P` is a suffix. A prefix or suffix is short — a region, a
country, `P`, `M`, `QRP` — and the callsign is the long one.

**`W1ABCD` is the row that decides the design.** A prefix match would make it
his, and it is a different station; a filter claiming somebody else's traffic as
yours is §0.0's fault wearing a helpful face.

**With no callsign set**, `mine` holds every row back, the table is empty, and
the panel says:

> Hamlet does not know your callsign yet, so "mine" has nothing to match on and
> it is holding every message back. Put your callsign in Settings, under
> Operator, and this starts picking out the contacts you are part of.

beside a summary reading `0 shown · 6 hidden by mine`.

**3. The waterfall's first row, which was the drop candidate and was built.**
Measured through the device path with a fake source at 48 kHz, after two full
windows with nobody drawing:

```
window 16384 samples, hop 4096 samples
quiet for 33600 samples with nobody drawing
first frame after 4800 samples (100 ms)
```

**341 ms before, 100 ms now**, and one hop — 4,096 samples, 85 ms — is the floor
that cannot be beaten, because the transform runs on a hop boundary by
construction.

**How, without spending what unit 240 bought.** That unit stopped the transform
while nobody was drawing, which was right and is untouched: `Emit` still returns
on a null `FrameReady`, so the 16,384-point transform is still not run for a tab
nobody is looking at. It **also** threw the ring away and stopped offering, and
that is what cost the third of a second. Its reason was §0.0 — a row must never
mix two moments — and that reason is sound about a ring that is *stopped and
restarted*, whose first frame would glue audio from two different times together.
**The gap is what causes that, so this removes the gap rather than the rule.**
The ring is fed continuously, so it always holds the most recent window of
contiguous audio and the first frame after somebody looks again is the frame they
would have had if they never looked away. Nothing is stale because nothing
stopped.

**No ring write moved onto the audio callback thread.** `Offer` is the copy and
return that callback already paid whenever the tab was open, and `Push` — which
writes the ring — still runs on the below-normal worker. The 522,895 microseconds
a callback are not being spent.

## 4. What's blocking us

**Nothing blocks the next unit.** Three items, none of which asks for a ruling.

**1. The first waterfall row is black by construction, and it was not fixed.**

Not a question. A measurement, reported because task 4 says to say so when
something cannot be taken.

`Emit` draws each bin against **that bin's own tracked noise floor**, and on the
very first frame the floor is initialised to that frame's own level — so `over`
is zero, `above` is zero, and every bin computes to 0. Measured with a
continuous 688 Hz tone running throughout: all eight loudest bins at 0. This is
correct behaviour for the floor tracker and it predates this unit.

**Why it was not fixed here.** Keeping the floors warm means running the
16,384-point transform while nobody is drawing, which is exactly what unit 240
removed and exactly what task 4 was told not to spend. So the row now *arrives*
a quarter of a second sooner and is still black when it does; the picture becomes
readable as the floors settle, as it always did, just earlier. **If you want the
first row to carry something, that is a different job with a different cost**,
and it would be a decision about spending CPU on a tab nobody is looking at.

**2. The instruction said there is nowhere to put the callsign, and there is.**

Reported, not repaired, per the instruction's own rule about tree mismatches.

Task 3 asks for "a place to put it, and it persists". `SettingsWindow.axaml`
line 68 already binds a Callsign box, in a section headed **Operator**, and
`SettingsViewModel.OnCallsignChanged` writes it to `Operator.Callsign` and saves.
What unit 251 found was an **empty value**, not a missing field. So task 3 became
the matching rule and the pointer, and no second field was added — a second place
to type a callsign is the drift §0 exists to stop.

The instruction's other tree claims all checked out: the three exclusive buttons,
the quoted summary format, `mine` saying so with no callsign, root version
`1.12.67`, task 9 of unit 251 unbuilt, and `AppSettings` persisting to
`settings.json`. **The `CQ POTA` claim is true and it takes two files**, which
the instruction asked to have named: `Ft8Vocabulary.Split` joins
`CQ POTA W5LST EM33` into the single addressee `CQ POTA`, because the call and
its direction are one field in two words, and `DecodedFilterRule.IsCallToAnyone`
tests what that produced. That behaviour survives this unit and is pinned by a
theory row.

**3. Two sessions ran one work order yesterday, and nothing in the tree stops
it.**

Carried from unit 251's collision, and it is the one thing here that would be
worth a rule. `SESSION.lock` is written, committed, and carries a pid, a start
time, a host and a token — **and nothing reads it.** The gate of §0.3.1 and
`CLAUDE_CODE.md` §4.1 is built around one failure, *the right session in the
wrong repository*. This is the mirror case, *the wrong session in the right
repository*, and it passes every check that gate makes, confidently, all the way
down. Yesterday it cost an evening, two duplicated open-issue ids and a duplicate
outcome entry. **This session checked the lock by hand before reading the work
order**; that is a habit, not a rule, and the next session will not have it
unless it is written down.

### Asks still outstanding

Carried verbatim until you rule, per HM-DEC-139.

**This session cannot honestly reconstruct the queue, and that is the same
report unit 251 made.** HM-DEC-139 requires the work order to carry the
outstanding asks inbound, and **work instruction 252 carries no
`Asks still outstanding` heading**, which by §9.6 makes the order defective and
obliges the session to rebuild the queue from `OPEN_ISSUES.md` and the recent
reports. `OUTPUT.md` is overwritten every unit, so the reports that would carry
those asks are gone; rebuilding from `OPEN_ISSUES.md` alone would produce the
long list HM-DEC-140 expressly says does not belong here, and a queue assembled
in a hurry from a partial read is worse than an admitted gap, because a dropped
ask looks exactly like an answered one.

What can be stated without a rebuild:

- **The missing heading is itself the first ask.** It is a defect in the order
  under §9.6 and HM-DEC-137, reported here as those clauses require. It has now
  been missing from two consecutive orders.
- **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05, are step 6's two unmet
  exit criteria. They are recorded issues with an id, an owner and a date, so by
  HM-DEC-140 they belong in `OPEN_ISSUES.md` and not on this queue. Neither was
  touched by this unit.
- **This unit added nothing to the queue.** Its three section 4 items are a
  measurement, a tree mismatch and a standing observation; none is a question
  handed back for a ruling.

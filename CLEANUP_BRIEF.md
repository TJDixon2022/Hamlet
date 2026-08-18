**PROJECT: Hamlet**

# Work order: characterise the edges, rebuild the short fixtures, set the floor

Five phases. Reported per §12.2: four sections, **written to `OUTPUT.md` at the
repository root, overwriting it**, and printed to the session as well. **Name
the branch in section 1** (§9.5.1 — `main`, and nowhere else).

**Read first:** `CLAUDE.md` (§0.0, §12, §12.5), `SESSION_PROTOCOL.md`, the
previous `OUTPUT.md`, `OPEN_ISSUES.md`, `DECISIONS.md`.

**New rulings: HM-DEC-119, 120, 121.** Two of them supersede or block work the
last session was told to do, and the reason in both cases is a measurement it
made. Read them before planning.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues. §12.1 unchanged. **No transmit work of any kind.**

---

## Phase 1 — the floor is 14 (HM-DEC-120)

One line. The floor parameter's default moves from 17 to 14, superseding the
interim.

Swept last session: 17, 15, 14 and 13 never invent a character at any level; 12
begins inventing at −2 dB, which is HM-DEC-097's named case at 0.44 invented.
Fourteen and thirteen are identical on every measured number and both read the
message perfectly down to 1 dB, so the further of the two from the cliff is
taken.

**This buys back the four decibels the interim gave away.** Re-run the sweep and
confirm the table is what it was: nothing invented at any level, whole message
to 1 dB.

## Phase 2 — characterise the detector's edges (HM-DEC-119's commission)

**HM-DEC-112 is superseded and this is what replaces it.** The correction it
prescribed was measured through an offline filter, not through Hamlet's
Goertzel, and carrying one instrument's edge shape into another's clock takes
the suite from 13 failures to 29 and silences 30 words a minute.

What is wanted is one measurement, not a fix.

For **synthesized dits and dahs of known length**, at **12, 25 and 30 words a
minute**, through **Hamlet's own detector**, report:

- the envelope's actual shape across a mark's rising and falling edges, sampled
  at the hop rate;
- where the gate declares the mark begins and ends against the true edges;
- the same for the gap that follows;
- how all of it changes with the analysis window's length.

The known numbers to reproduce and extend: the gate reads 100–110 ms for a true
100, 45–50 for a true 48, 40–45 for a true 40 — accurate to within one hop
throughout. Half amplitude reads 80–90, 30–35 and 25, wrong by 15, 30 and 37
percent, and the shed is a fixed 15–20 ms at every speed, which is the window
rather than the transmitter.

**Then say what the answer is**: a shorter edge window, sub-hop interpolation,
or nothing at all. **Do not implement it.** The measurement is the deliverable
and the choice is Tim's (§12.1), because it decides what the clock asserts.

One clue worth carrying: with all three parts of HM-DEC-112 in, 25 wpm decoded
*exactly* while 30 collapsed to nothing. **Something near the edges genuinely
matters at 25 and the half-amplitude correction was fixing it by accident.**
Whatever is actually wrong at 25 is still there and this measurement should
find it.

## Phase 3 — rebuild the six short fixtures, as its own proper work (§12.5)

The last session dropped this and was right to: it needs the recipes changed,
the WAVs regenerated, the reference scorer run to satisfy HM-DEC-101's gate, and
**every held-out fixture adjudicated one at a time with a recorded reason**.
That is not a phase tail. It is this phase.

The finding it rests on, measured two sessions ago: all six failures are the
signal being too short for a detector that wants about three seconds of keying
before it moves. Given a run-up, each decodes `CQ DE W1AW K` exactly.

- Add sufficient run-up to the six, regenerate, and re-score.
- **HM-DEC-101's gate applies**: the reference must score a fixture well before
  that fixture may judge Hamlet. A fixture the reference cannot read is a bad
  fixture.
- **Adjudicate every hold-out individually, with its reason recorded** (§12.5).
  No wholesale retirement.
- Three of the four `TheEasyTierIsReadWhole` failures lose only their opening
  characters to acquisition and should clear with the same treatment. **The
  fourth reads `IR` where `AR` was sent** — a wrong character rather than a
  missing one, the only strangers case the bar catches for the right reason.
  Chase that one separately and report it; do not let a run-up hide it.

Expect the failure count to fall substantially. Report before and after by name.

## Phase 4 — trace the coupling behind HM-DEC-116 (HM-DEC-121)

**HM-DEC-116 is blocked, not withdrawn.** Built as ruled it meets its own
acceptance and costs the callsign on `cw-2026-08-17-013347`, where the settled
pass falls from `VA3VRR` to placeholders. Three narrowings were tried and none
broke it; disabling adoption restores the callsign, which identifies the cause
but not the path.

**Find the path.** Instrument both passes and trace what the settled pass reads
back out of the estimator after adoption changes it. The standing hypothesis is
that the settled pass takes exactly one thing — the dit hint — and if that is
the whole coupling, the classes can be handed forward without touching anything
the dit derives from.

**Report the path and stop.** Do not ship HM-DEC-116 on this session's
judgement; a real off-air capture outranks a synthetic test (HM-DEC-091) and the
ruling stays blocked until Tim lifts it.

## Phase 5 — DROP THIS ONE IF SHORT OF ROOM

The 400 Hz tracker finds the pitch and will not hold it, breaking down and
re-acquiring however long the signal — the only one of the four pitch failures a
run-up does not fix, and the one genuine decoder fault with no ruling in front
of it.

Diagnose and report. Fix only if the cause is unambiguous.

If dropped, say so.

---

**If every phase completes, stop and report. Do not start any other work unit,
and build nothing toward auto-CQ.**

## Definition of done

The floor is 14 and the sweep confirms it. The detector's edge behavior is a
table with a recommendation Tim can rule on. The six short fixtures are rebuilt,
gated and adjudicated, with the failure count reported by name before and after.
The HM-DEC-116 coupling has a named path.

**Still outstanding and not in this order**: `cw-2026-08-18-003758` is not on
the machine (HM-OPEN-026) and would be the suite's only regression test for a
success. The bulletin `cw-2026-08-18-004507` stands at 36 characters against 45
and every remaining error is character-level — `T` read as `A` twice, `BT`
unresolved, letters dropped from `EACH`, `MESSAGE` and `HANDLING`. **Nothing is
tuned to that recording**; a decoder fitted to one capture has learned one
station.

**Everything here is provable on the development computer against fixtures, and
none of it is evidence about the radio** (HM-DEC-093).

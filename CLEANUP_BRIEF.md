**PROJECT: Hamlet**

# Work order: the retune distinction, and the last of the bar

Five phases. Reported per §12.2: four sections, **written to `OUTPUT.md` at the
repository root, overwriting it**, and printed to the session as well. **Name
the branch in section 1** (§9.5.1 — `main`, and nowhere else).

**Read first:** `CLAUDE.md` (§0.0, §12, §12.5), `SESSION_PROTOCOL.md`, the
previous `OUTPUT.md`, `OPEN_ISSUES.md`, `DECISIONS.md`.

**New rulings: HM-DEC-125 and HM-DEC-126.** Both close things out rather than
opening work: HM-DEC-122 is superseded and stays unbuilt, and HM-OPEN-026 is
closed as unobtainable.

**This order is HM-DEC-123's**, held back for two sessions and now blocking
three separate things: `ASignalAtTheWrongPitchIsStillFound(400)`, HM-DEC-116,
and the settled pass on `cw-2026-08-17-013347`.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues. §12.1 unchanged. **No transmit work of any kind.**

The suite stands at 1829 tests, six failing:
`ASignalAtTheWrongPitchIsStillFound(400)`,
`ClearingTheTranscriptLeavesTheDecoderAlone`,
`TheBulletinDecodesToItsAnswerKey`,
`TheEasyTierIsReadWhole(prosigns-easy)`,
`TheEasyTierIsReadWhole(tightfist-easy)`,
`TheSettledPassNoLongerStopsShortOfTheCallsign`.

**Three of the six belong to phase 1.**

---

## Phase 1 — a refining retune does not throw the settled window away (HM-DEC-123)

**The whole cost is paid in one line: `_settled.Reset()` on every tracker
switch.** HM-DEC-096 put it there because a switch usually does mean somebody
else started transmitting. Sometimes it means the tracker found the same station
more precisely.

Two unrelated investigations converged on one sentence — **one retune decodes
and three does not**:

- `cw-2026-08-17-013347`: adopting gap classes moved `MidCharacter`, one retune
  became three, and the settled pass fell from `■■■ ■■VA3VRR` to `■■■ ■`. The
  last session reproduced this exactly while fixing the gap classifier, and
  recorded it as HM-OPEN-032 rather than tidying it away.
- `ASignalAtTheWrongPitchIsStillFound(400)`: a signal found from 200 Hz away
  takes three retunes and is unreadable. **Starting 300 Hz above decodes;
  starting 100 Hz above does not.** So it is not about distance.

Build the distinction:

- **A refining retune** — the tracker settling more precisely on the pitch it is
  already reading — **keeps the settled window.**
- **A following retune** — the tracker moving to a different station — **resets
  it**, as now.
- The tracker already knows how far it moved. A refinement within the current
  bank and a jump to another operator are already different events in the data;
  the distinction does not need inventing, it needs reading.
- **Do not make this a distance threshold pulled from the air.** Measure what
  separates the two cases on the recordings named above and state the criterion
  in the report.

**Held and not shipped, deliberately** (HM-DEC-123): stopping the streaming
pass's segmentation from gating the tracker. It is right in principle — a
provisional judgement governing a measurement is backwards — but it treats one
symptom of this mechanism, and **if this phase is the whole story that change
would be made for a reason that dissolved.** Do not build it. If the measurement
shows it is still needed after phase 1, say so in section 4.

Acceptance: `ASignalAtTheWrongPitchIsStillFound(400)` and
`TheSettledPassNoLongerStopsShortOfTheCallsign` both pass, and nothing that
currently passes regresses.

## Phase 2 — HM-DEC-116, unblocked or not

**HM-DEC-116 is blocked by HM-DEC-121, and phase 1 is the thing it was blocked
on.** The streaming pass adopting the settled pass's fitted gap classes met its
own acceptance and cost the callsign, and the path was traced: adoption changes
where characters divide, which moves `MidCharacter`, which turns one retune into
three, which resets the settled window.

**If phase 1 lands, that chain is broken at the last link.** Re-attempt
HM-DEC-116 on top of it and measure.

- If it now holds the callsign and keeps its own acceptance —
  `NothingIsInventedAtTheHandover` and two-station tone-finding — ship it and
  say so.
- **If it still costs a real capture, do not ship it.** Report the new path.
  HM-DEC-121 stands until Tim lifts it, and a real off-air capture outranks a
  synthetic test (HM-DEC-091).

## Phase 3 — re-measure the bulletin

`cw-2026-08-18-004507` stands at **36 characters against 47**, and last session
moved it for the first time in three: `OT NET ■I ECH STAAION HAND■ AHIS MESAGE
P` — three characters shorter, the same number correct, the invented `JJ` gone
and one wrong `A` with it.

Re-measure after phases 1 and 2 and **report the number before touching
anything.** `T` still reads as `A` in `STATION` and in `THIS`, which is a
spurious leading dit: a mark boundary in the wrong place or an edge caught
early.

**Do not tune anything to this recording.** A decoder fitted to one capture has
learned one station. The last three sessions each held that line and it is the
reason the number means something.

## Phase 4 — the last two bar failures

Both are HM-DEC-114's bar and both are small:

- **`tightfist-easy` gains one placeholder.** New last session, from the gap
  classifier repair, and **not** the outlier trim — that was tested with the trim
  disabled and was unchanged.
- **`prosigns-easy` loses its four opening characters to acquisition.** A
  different cause from the one it failed on for months: the prosigns themselves
  now read correctly after the caret fix, and it is the one easy-tier fixture
  that cannot carry a run-up.

For `prosigns-easy`, HM-OPEN-031 stands behind it: the run-up exclusion was
re-tested rather than taken on trust and the reference reads the run-up version
at 100% while Hamlet emits a single placeholder. **That is a decoder finding, not
a fixture one**, and it may be the same defect as `tightfist-easy`'s placeholder.
Check before treating them separately.

## Phase 5 — DROP THIS ONE IF SHORT OF ROOM

- **HM-OPEN-026 is closed** (HM-DEC-126). Mark it closed with the ruling's
  reason: nothing in the fixture set names the file, and it reopens if it
  appears. **Record alongside it that this suite still has no regression test
  for a success** — every ratchet it holds is a ratchet on a failure getting
  less bad, so nothing in it can tell a repair from a coincidence. That is a gap
  worth naming even without a candidate to fill it.
- **HM-DEC-122 is superseded by HM-DEC-125 and stays unbuilt.** Confirm nothing
  of the two-candidate mechanism remains in the tree, and that
  `CwAcquisitionWindowTests` — which is measurement and should survive — still
  pins the bare fast end, the same fist with a run-up, and the slow end.
- **HM-OPEN-024**: `TheStopFrameIsCommand17CarryingFf` failed once in a full run
  and passed alone and in the runs either side. Recorded, not chased (§12.6).
  Confirm it is still intermittent rather than becoming reliable.

If dropped, say so.

---

**If every phase completes, stop and report. Do not start any other work unit,
and build nothing toward auto-CQ.**

## Definition of done

The refining-versus-following distinction is built, its criterion stated as a
measurement rather than a threshold pulled from the air, and both
`ASignalAtTheWrongPitchIsStillFound(400)` and
`TheSettledPassNoLongerStopsShortOfTheCallsign` are green. HM-DEC-116 is either
shipped or reported with a new path. The bulletin's distance from its answer key
is a reported number. The two bar failures are fixed or attributed.

**Everything here is provable on the development computer against fixtures, and
none of it is evidence about the radio** (HM-DEC-093).

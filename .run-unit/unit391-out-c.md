
## 4. What's blocking us

**Nothing blocks step 0. Item 1 decides where step 1 restores to, and step 1 can start on the
commit named here unless you rule otherwise.**

**1. Which commit step 1 restores `src\Hamlet.RadioEngine\Cw` to.** *A ruling request; not blocking.*
0.2 as written is `07f0397a` (08-21). It's the only commit green on all three, but it's from
before the 08-25 evening, the adjudicated anchors, and 32 of the 37 capture floors. Whether it
meets today's floor table is unknown until step 1 runs it. `PHASE_PLAN.md` §6 and task 3 give a
fallback for this case: the newest commit green on the captures type alone. I didn't walk for
that. One probe shows `7e209cb4` (08-25) green on captures 36 of 36 and adjudicated 13 of 13,
with the synthetics red.

| | Restore to | For | Against |
|---|---|---|---|
| **A** | `07f0397a` (08-21) | Green on all three; the synthetics decode exactly; the literal 0.2 | Loses 08-22 to 08-25; today's captures and adjudicated tables were never run against it; 7 app-facing types and 21 members are missing, so more seams |
| **B** | the newest commit green on captures alone, found by one more unit's walk from HEAD down (at least as new as `7e209cb4`) | Keeps the decoder the floors were set on; two of three tests green; fewer seams | Synthetics red from the start; R49 forbids retiring them, so step 1.3 can't be met on the synthetics without a repair |
| **C** | `7e209cb4` directly | As B, with no further walk | Not proven the newest green; commits after it may hold more |

**Industry standard:** A. You go back to the last build green on the whole guard set, and
re-apply from there (R48, R51). **My recommendation, author's:** A for step 1. The first thing
step 1 measures is today's three floor tests against 07f0397a's decoder, and if the captures
floors fail there, B's walk is the next unit.

**2. Section 5 mismatches:**
- **The failing set has 2 cases of `EachStillProducesWhatItDid`, not six** (`001520`, `013637`).
  The two clean synthetics are there as stated.
- **The floor table has 37 rows.** The class's own remarks say "thirty-six here".
- **The Cw source changes are not "2026-08-28 to 08-31 and once on 09-03".** `git log` since
  08-24 shows 51 commits on every day from 08-24 to 08-31, and **four** on 09-03 (`43efc525`,
  `865e66d8`, `9c2a7f99`, `1a84188e`).
- **The green commit isn't between 08-25 and 08-28.** Nothing since 08-24 is green on all three.
- **`PHASE_PLAN.md` §6's fallback reads "since 2026-08-25";** task 3's reads "back to 2026-08-24".
- **HM-DEC-166 has no row in `CLAUDE.md` §1.** The top row before this unit was HM-DEC-165. I
  added HM-DEC-167's row only.
- **Held as stated:** `PROJECT_STATUS.md` read unit 390; `Directory.Build.props` read 1.13.77;
  every one of the 37 captures is on disk; the three tests exist by those names with 37, 13 and 2
  cases. The known-reds block carries two CW entries: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`
  and "the 51 CW cases in docs/unit239-failing-set.txt".

**3. The captures type takes 1995 s at HEAD, against 97 s at `7e209cb4`.** *An indication, one
run each; it bears on step 2.* Criterion 2.3 measures the guard against the engine line's
`timeout 480`. At today's speed the whole type can't go on that line. §6 already rules that
a test over 300 s never does.

**4. HM-DEC-155, bent and said so.** The harness caps a foreground call at 600 s, so the two
long captures runs and the probe ran in the background. I waited with one bounded loop per run.
The synthetics-only walk overlapped the second HEAD captures run in a second tree, which only
costs time: results were identical case for case against the first run. If the rule should bind
here as written, a type over 600 s needs a different runner.

**5. Three worktrees under `C:/Users/TimDi/preflight-trees/` were there before this session.**
One is at `07f0397a`, the commit named for 0.2. I didn't make them and didn't touch them. By the
instruction's own reasoning they're "a second tree the next unit can edit by mistake".

**6. `PHASE_PLAN.md` 0.1 to 0.4 are not ticked.** The instruction didn't ask me to; the judge
ticks them.

### Asks still outstanding

**Carried per HM-DEC-139, verbatim, from unit 390. None is CW, none is this phase's, and none is
this unit's to answer.**


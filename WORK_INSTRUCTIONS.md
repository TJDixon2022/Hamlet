PROJECT: Hamlet
ISSUED: 2026-08-18

# Work order — we broke the radio tracking. Find out how, and fix it.

**One phase. Nothing else is in this order.** Not records, not favorites, not mode
follow, not the scope ladder. Finish it, report, stop.

Gate first (HM-DEC-099): verify `PROJECT: Hamlet` against `PROJECT_CARD.md` in
this tree and against the prompt you were pasted. Any disagreement, stop.

---

## What is true

**Hamlet tracked the radio instantly and correctly. The last two builds broke it.
The app is unusable.** That is the operator's direct statement about his own radio
and it is ground truth in the manner of `SHACK_FACTS.md`.

**This is a regression, not a design gap.** It is not an unmet premise of
HM-DEC-050, not a missing feature, not something the radio might be configured
wrong for. Sessions we ran broke working software. Any line of reasoning that ends
somewhere other than a change we made is the wrong line, and if the evidence
genuinely forces you there, stop and say so rather than building around it.

`%AppData%\Hamlet\telemetry\2026-08-18.jsonl`, six sessions, app 1.9.0,
forty-six observations of `Frequency`: `read` 32, `unknown` 12, `stale` 2,
**broadcast by any label 0.** Ages up to 60.2 seconds. So it was already broken at
1.9.0 and whatever did it landed at or before that build.

## The one phase

**1. Reproduce it in a test.** A fake serial source emits an unsolicited frequency
frame — from the radio's address, to destination `00`, the broadcast address the
IC-7300 actually uses — while a command is in flight. Assert it reaches the rig
state model with broadcast provenance at an age near zero. Run at `HEAD`; it
should fail. **If it passes, stop and say so**, because then the fault is
downstream of the state model and this order's premise is wrong.

**2. Read the diffs. This is the substance of the work, not a preliminary to it.**
Every change since it last worked that touches the serial reader, the frame
parser, the dispatcher, the provenance model or the rig state model. `git log -p`
on those paths, newest first. Named suspects, and do not stop at the first one
that looks plausible:

- **HM-DEC-051**, which reworked the provenance taxonomy and named `transceive 00`
  apart from `CI-V 03`.
- **HM-DEC-109**, which put the frequency on the session poll, amending HM-DEC-050
  for a third field. Written as a backstop for a broadcast missed at startup, it is
  now the only mechanism running — which is what a backstop looks like when the
  thing it backs up has died.
- **The dispatcher work behind the last session's phase 6**, which found that a
  read issued with no expected response command completes only on `FB` or `FA`. An
  unsolicited frame is not a response to anything in flight, and if it passes
  through that matching path it is consumed and dropped.

`DECISIONS.md` has no entries for HM-DEC-096 to 133 — those exist only as
`CLAUDE.md` §1 index rows. For that range, read the commits, not the rulings.

**3. Name the mechanism exactly.** Not "probably the dispatcher." **Which line
receives the frame, which line discards it, and what changed to make it do that.**
The test from step 1 goes green when you fix that line and no other. If two
changes both contribute, name both and show which one alone is sufficient. If
reading the diffs does not settle it, `git bisect` with the test from step 1 and
then read the diff of the commit it names — the bisect is how you find the diff to
read, not a substitute for reading it.

**4. Fix it.** Restore instant, correct tracking. Where the breaking change was
deliberate and right for its own reason, the fix keeps that reason and restores
the broadcast path beside it: a ruling is not reverted to clear a regression, and
neither does a regression stand because a ruling caused it. If those genuinely
cannot both hold, that is a trade-off and it is Tim's — stop and hand it back
(§12.1 clause 3).

**5. Prove it at the wire.** Count inbound frames before any filtering, by source
address, destination address and command byte, and keep that instrumentation. The
acceptance is the telemetry above: with the radio connected, frequency
observations carry broadcast provenance at ages near zero, and the frequency on
screen changes as fast as the dial does. **Green unit tests are not proof.** No
connect means the fix is unproven — say so, and say which number would prove it.

**6. Leave the test that would have caught this.** It shipped in two builds with
nothing failing, which is its own defect. Step 1's test is the minimum. Better is
a check that fails if the frequency is ever being maintained by the session poll
alone, since that is the exact state the app has been in and nothing noticed.

Then write `PROJECT_STATUS.md`, commit and push to `main` (HM-DEC-113), and
report.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106). Section one leads with **the mechanism**
— the change, the line, why the frame stopped arriving — and the commit it landed
in. Section two leads with **whether the frequency on screen can be trusted now**.
Nothing goes above those.

State plainly whether a radio was connected (HM-DEC-093).

**Stop and report. Do not start anything else.**

# Changelog

**This file is an index and not a record.** Every ruling behind every release,
with its reasoning and what was rejected, is in `DECISIONS.md`, newest first.
Writing the reasons out again here would be a second copy of the same facts, and
a second copy drifts (§0). So each release below is one line, a headline, and
the range of `HM-DEC-###` ids it contains. Follow the ids for the why.

Versions follow the convention in `CLAUDE.md` §1 (HM-DEC-063): major breaks an
existing setup or reconceives the application, minor adds a capability the
operator can see and use, patch fixes and polishes without adding one.

---

## 1.2.7

**The send controls tell the truth.**

Grey means refused and nothing else. The confirming press guards text the
operator edited rather than text Hamlet wrote, so an unedited message sends on
one press and says so. Sending is one stable state rather than a sample of a
transmit line that toggles on every Morse element. The send itself is now in the
record, by length and duration and never by its words. The build date is stamped
at compile time rather than read off a file timestamp that could lie.

Ruling HM-DEC-079.

## 1.2.6

**The send button works.**

Readiness reached Ready and the buttons stayed dead, twice, on live hardware.
The gate was right the whole time: the rig poll rebuilt the buttons four times a
second and a click cannot survive a control that is destroyed under the
operator's finger. The buttons now persist, the command carries the gate rather
than only the visual tree, and the record says what the operator saw beside what
the engine decided.

Ruling HM-DEC-078.

## 1.2.5

**Hamlet can explain itself.**

Telemetry became a decision record rather than a list of completions: every
decision point that can go more than one way now names the branch it took and
the state that decided it. The rig state travels with the record, levels mean
something, the decoder says what it heard and rejected, and a window shows what
Hamlet recently decided beside the one showing what the radio is doing. Also the
callsign resolver, the live-fire hardening of the transmit path, "did anybody
hear me", and the contact tracker.

Rulings HM-DEC-063 through HM-DEC-077.

## 1.2.0

**Hamlet can key the radio.**

CW transmit, behind a guard that answers before anything reaches the air and an
abort that awaits nothing. Favorites that carry the reason you saved them. The
happening-now panel's two lenses and its workability ranking. The digital
neighborhoods, cited. Out-of-band warnings on every surface that speaks. Mode
follows the map, which is the first thing Hamlet writes to a radio. And the
radio's own spectrum in the waterfall.

Rulings HM-DEC-045 through HM-DEC-062.

## 1.0.0

The foundation: the engine and the shell, CI-V, the CW decoder, the Explorer,
the neighborhood map, the band plan and privileges, and the training radio.

Rulings HM-DEC-001 through HM-DEC-044.

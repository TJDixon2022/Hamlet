# Open issues

Questions with owner and severity. `owner` is who must act next. Format in
`CLAUDE.md` §3.

---
id: HM-OPEN-001
status: closed
owner: tim
raised: 2026-08-12
severity: hard
blocks: solution scaffold — the App project cannot be created without it
closed: 2026-08-12
refs: HM-DEC-011
---

WPF or Avalonia for the UI shell?

| | WPF | Avalonia |
|---|---|---|
| Platform | Windows only | Windows/Linux/macOS |
| Maturity / tooling | Deepest, designer support | Good and improving |
| Tim's familiarity | High (old-school C#) | New |
| Open-source audience | Windows hams only | All — and Linux is common in ham shacks |
| WriteableBitmap waterfall | Native | Equivalent (WriteableBitmap exists; API differs slightly) |

Industry-standard answer for a public open-source ham tool: Avalonia, for the
Linux audience. Fastest-start answer: WPF. Per §0, "faster to start" is not a
reason, but "Tim ships phase 1" has weight too. Tim rules.

---
id: HM-OPEN-002
status: closed
owner: tim
raised: 2026-08-12
severity: slows
blocks: nothing yet; becomes hard when CI-V code is written
closed: 2026-08-14
refs: HM-DEC-049, HM-DEC-005, CLAUDE.md §4
---

Obtain the IC-7300 CI-V reference (the "Full Manual" / CI-V command tables
from Icom) so the command facts in CLAUDE.md §4 can be verified and the cited
pages vendored into data/vendor/.

Everything Claude currently holds about 0x17 (CW send), 0x27 (scope data),
frame format and BCD encoding is general knowledge, marked unverified. Code
must not depend on an unverified command byte. Tim downloads the PDF from
Icom and uploads it to the session; Claude extracts and vendors the cited
sections only.

**CLOSED 2026-08-14 (HM-DEC-049).** Tim supplied the Full Manual and section 19
CONTROL COMMAND was read directly. §4 now carries the verified facts with page
citations, two corrections and one precondition nobody had written down.

**The vendoring half was NOT done, and that is the ruling rather than an
omission.** Icom's terms permit individual use and prohibit redistribution, so
the repository cites pages and carries none of the PDF. §4's "vendor the cited
pages" rule stands for sources that allow it; this one does not, and §2.1 wins.

---
id: HM-OPEN-003
status: open
owner: tim
raised: 2026-08-12
severity: none
blocks: nothing; the app reads most of these itself now
refs: HM-DEC-050, HM-DEC-048
---

Station configuration facts from Tim's PC: the COM port the IC-7300
enumerates as, the exact audio device names ("USB Audio CODEC" variants),
the radio's CI-V baud and CI-V address menu settings, and CW sidetone pitch
setting.

These are config values, not constants. Needed before the first
connect-and-read-frequency test. Device Manager and the radio's SET menu
answer all of them in five minutes at the desk.

**NARROWED 2026-08-15 (HM-DEC-050), severity dropped from `slows` to `none`.**
The first live connection has happened, so this no longer blocks anything, and
the app now answers most of it itself:

- **CW sidetone pitch** is read from the radio (`14 09`) and shown on the
  diagnostics screen. Nobody has to walk over and look.
- **Audio device names** are enumerated and chosen automatically, preferring one
  whose name matches the radio's USB codec, with the operator's own choice
  remembered (HM-DEC-048). A machine with none says so and carries on.
- **CI-V address** is proved by the connection succeeding at all: the probe read
  only answers when the radio, the address and the baud agree.

What is genuinely left is the pair a person still has to supply, because nothing
can be read until they are right:

- **The COM port.** Hamlet lists the ports it can see and the operator picks;
  there is no way to know which one is the radio until something answers on it.
- **CI-V USB baud when it is not Auto.** The radio defaults to Auto (p. 12-11)
  and Hamlet has no way to discover a fixed setting except by failing to
  connect.

Both are answered by connecting once and writing down what worked, which is
configuration rather than a question anybody has to research.

---
id: HM-OPEN-004
status: open
owner: unassigned
raised: 2026-08-12
severity: none
blocks: phase 3 only
---

FT8 decode integration approach: P/Invoke wrap of ft8_lib, or shell out to a
WSJT-X jt9 subprocess?

ft8_lib is small, clean C, designed for embedding; jt9 is the reference
decoder with better weak-signal performance but a process boundary and
version coupling. Both are GPL (HM-DEC-004 already accounts for that).
Decide during phase 3 planning; nothing before then depends on it.

---
id: HM-OPEN-005
status: open
owner: unassigned
raised: 2026-08-12
severity: none
refs: src/HamManager.RadioEngine/Bands/BandPlan.cs, CLAUDE.md §0
---

Move the band plan out of code into a source-marked data file in /data, with
citations (ARRL band plan, FCC Part 97) and per-license-class privileges.

The current BandPlan.cs carries US allocations marked [extrapolated] from
general knowledge. Fine for phase 1 tuning; not fine as the basis for FG-006
band-plan coaching or transmit-privilege warnings, which need cited,
class-aware data. Generate-don't-transcribe applies (§0).

**NARROWED 2026-08-13 (HM-DEC-029).** The privileges half is done:
`data/privileges/us-part97-privileges.json` carries 47 CFR 97.301, 97.305 and
97.307 transcribed from eCFR, cited per row, with its gaps declared as explicit
unknowns. Transmit-privilege warnings now rest on cited data.

What remains is `BandPlan.cs` itself, which still holds three kinds of number
in code:

- **Band edges** (`LowHz`, `HighHz`). Now redundant — the same edges are in the
  privileges file under the Extra class, which by definition reaches every band
  edge. These should be derived from it rather than kept in parallel.
- **CW segment boundaries** (`CwLowHz`, `CwHighHz`). Still [extrapolated].
  These are convention, not regulation, and they do NOT align with the
  privilege boundaries — both encodings are needed and neither derives from the
  other (HM-DEC-029).
- **Jump spots** (`JumpHz`). Editorial: QRP watering holes and activity
  conventions. A citation would be an ARRL band plan or a club convention, not
  a regulation.

So this stays open at severity `none`, now meaning: derive the band edges from
the cited data, and give the conventions a source mark of their own kind.

**NARROWED AGAIN 2026-08-14 (HM-DEC-054).** The neighborhood conventions are
now cited data in `data/bands/us-neighborhoods.json`, with the ARRL Considerate
Operator's Frequency Guide, WSJT-X's shipped frequency table, the JS8Call user
guide, the 070 Club's PSK31 list and QRP ARCI's centers of activity on the rows
that use them. The map derives its data-against-phone boundary from the
privileges file rather than carrying a copy.

What remains in `BandPlan.cs` and is still `[extrapolated]`:

- **Band edges** (`LowHz`, `HighHz`). Unchanged from above: derivable from the
  privileges file under the Extra class and still kept in parallel.
- **CW segment boundaries** (`CwLowHz`, `CwHighHz`). Now used by less than they
  were, since the map no longer builds itself from them, but they still drive
  the dial tape's "inside the CW segment" line.
- **Jump spots** (`JumpHz`). Now demonstrably wrong in at least one place: 20 m
  jumps to 14.030 and QRP ARCI puts the 20 m center of activity at 14.060. The
  neighborhood file has cited jump spots per block, so a band button could take
  its landing place from there instead of carrying its own number.

---
id: HM-OPEN-006
status: open
owner: tim
raised: 2026-08-14
severity: none
refs: FG-002, ONBOARDING.md ONB-C04, HM-DEC-058, src/Hamlet.RadioEngine/Explore/SpotRankWeights.cs
---

Hamlet has never asked the operator what Morse speed they can copy, so the spot
ranking may describe a station's sending speed and may not claim any speed suits
this person.

The ranking weighs sending speed where the source reports it, which RBN does.
What it cannot do is match that figure against the operator, because there is no
figure to match it against. A card reading "15 WPM, slow enough for you" would
be a confident match against a number nobody has ever measured, which is exactly
what §0.0 forbids, and it would be wrong in the direction that costs most: it
would send somebody to a contact they cannot make and let them conclude the
fault is theirs.

So the copy is descriptive and the preference in `SpotRankWeights` is a fact
about Morse rather than about this person: slower sending is easier for anybody
still learning, which is why the slow-speed clubs exist. `SpotRankingTests`
sweeps the reason lines for the phrasings that would cross back over.

What closes it is ONB-C04, which is the onboarding step that finds out, and its
own note says the honest form is probably a listening exercise rather than a
question: somebody who has never made a contact does not know what speed they
can copy either, and asking them to type a number invites a guess. FG-002 is the
other half, since a copy speed Hamlet knows is what turns the Elmer mode's
practice into something aimed at where this person actually is.

Severity `none`: the ranking works without it and says nothing untrue. It stays
open because the day somebody adds a speed filter without noticing this is the
day the app starts making a claim it cannot support.

---
id: HM-OPEN-007
status: open
owner: tim
raised: 2026-08-14
severity: none
refs: HM-DEC-060, HM-DEC-054, src/Hamlet.RadioEngine/Explore/Favorite.cs
---

Two questions about favorites that HM-DEC-060 deliberately did not answer.

**Do favorites ever sync to the radio's own memory channels?** The IC-7300 holds
ninety-nine of them and they survive being unplugged from the computer, which is
the one thing Hamlet's list cannot do. Writing to them would make a favorite
reachable from the radio's own front panel on a day the PC is switched off, which
is a real benefit to somebody who operates both ways.

Against it: memory channels are somebody's own, they may already hold things that
matter, and a program that quietly rewrote ninety-nine of them would be
unforgivable. If this is ever built it is one-way, explicit, per favorite, and it
says which channel it is about to overwrite before it does. It also needs its own
CI-V verification pass, since nothing about the memory commands has been read
from the manual yet.

**What happens to a favorite whose neighborhood changed underneath it?** A
favorite records what the map said when it was saved. The map is cited data now
and it will be corrected as sources are re-read (HM-DEC-054), so a favorite saved
as "14.070, PSK31 ribbons" could later sit in a block the file calls something
else. Three options, none obviously right: leave the saved text alone as a record
of what was true then, re-derive it every time it is shown, or show both and let
the operator notice. Leaving it alone is what the code does today, because it is
the only one of the three that cannot surprise anybody, and that is a default
rather than a decision.

Severity `none`: favorites work, nothing is wrong, and neither question has to be
answered before somebody uses them.

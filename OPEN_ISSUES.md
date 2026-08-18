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
owner: tim
raised: 2026-08-12
severity: slows
blocks: retiring BandPlan, which leaves two band plans in the tree with only one of them cited
refs: src/Hamlet.RadioEngine/Bands/BandPlan.cs, data/privileges/us-part97-privileges.json, data/bands/us-neighborhoods.json, CLAUDE.md §0, §0.2.1, HM-DEC-107
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

**MEASURED 2026-08-17, AND THE SEVERITY GOES UP.** Raised from `none` to
`slows` and the owner from `unassigned` to `tim`, because this is now load
bearing for a feature that moves the operator's dial: §0.2.1 forbids
frequencies asserted from a model's memory, so the scanner was built around
`BandPlan` rather than on it and its segments come from
`data/bands/us-neighborhoods.json` instead. Two band plans in one tree, one
cited and one not, is the state §0 exists to prevent, and the uncited one has
the friendlier name.

**Two of the three kinds of number are provably derivable and the third is
not.** Measured against the cited files rather than argued about:

- **Band edges: all seven match exactly.** The union of the Extra class ranges
  in `data/privileges/us-part97-privileges.json`, cited to `97.301(b)`, gives
  `LowHz` and `HighHz` for every band. 80 m is the CFR's 80 m and 75 m rows
  together, which is the only join needed.
- **CW segments: all seven match exactly.** The union of the ranges carrying
  `Data` in the same file, cited to `97.305(c)`, gives `CwLowHz` and
  `CwHighHz` for every band, down to the hertz. 40 m needs two rows joined,
  `(c)(3)(iv)` and `(c)(3)(vi)`, because the phone segment overlaps the first.
  This corrects the note above: they were thought not to derive from the
  privileges data, and they do.
- **Jump spots do not derive, and the reason is that a rule has to be chosen.**
  Five of the seven are exactly a "CW main street" block's `jumpHz` in the
  neighborhood file; 40 m is the QRP watering hole's rather than main street's;
  and **30 m matches nothing cited at all** — it lands on 10.110 where the
  blocks are 10.103, 10.106 and 10.120.

The neighborhood file on its own does **not** cover the CW segments and cannot
be made to. Its Morse rows fall short at the top of every band, by 10 kHz on
17 m up to 230 kHz on 10 m, and 40 m has a hole in the middle between 7.040
and 7.050. That is not a defect in it: those rows are places somebody
published a convention for, and the space between belongs to nobody
(HM-DEC-054). The CW segment is a regulatory boundary and its source is the
privileges file.

**What is needed is one ruling, on the jump spots.** At least three rules are
defensible and they land in different places: the first "CW main street"
block, the QRP watering hole (which is what the note above argues for on 20 m,
against QRP ARCI's 14.060), or keeping the current numbers and citing them as
editorial. Whichever is taken changes where a band button lands on between
three and seven bands, which is a trade-off between cited data and the
operator's muscle memory, and §12.1 puts that with Tim.

Nothing was migrated. §0 would be satisfied by moving the edges and the CW
segments and leaving the jump spots in code, and a half-migrated band plan is
worse than two whole ones.

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

**Update 2026-08-14 (HM-DEC-066): the setting now exists and this stays open.**
The operator states a Morse speed in Settings, defaulting to 13, and the ranking
reads it. That is the weaker half of the answer. A stated preference is not a
measured ability, so what the app gained is permission to compare two stated
numbers and say a station is far over the one in the settings. It gained no
permission to say anybody can or cannot copy something, and `CopySpeedTests`
sweeps the composed card text for every phrasing that would.

What still closes this is ONB-C04, for the reason its own note gives: somebody
who has never made a contact does not know what speed they can copy either, and
asking them to type a number invites a guess. The setting takes an answer; the
listening exercise finds one out.

Severity `none`: the ranking works without it and says nothing untrue. It stays
open because the day somebody adds a speed filter without noticing this is the
day the app starts making a claim it cannot support. Nothing is filtered today
and a test holds that too.

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

---
id: HM-OPEN-008
status: open
owner: unassigned
raised: 2026-08-14
severity: none
refs: HM-DEC-069, IC-7300 Full Manual p. 12-9 (publication A7292-4EX-5)
---

The IC-7300 will send its decoded RTTY out the USB port, and the manual never
says what those bytes look like.

It states that the setting exists, that "an RTTY decoded signal is output," and
that the rate is 4800, 9600, 19200 or 38400 bps with 9600 the default. It does
not say whether the characters are ASCII, what marks the end of a line, whether
anything frames or brackets the text, or how the decode screen's own display maps
onto what leaves the port. A read column-aware over the whole manual found no
fourth statement about it.

So a decoder written against it today would be guessing, and dressing a guess as
decoded text is what §0.0 exists to forbid.

What would close it is an observation rather than a document: setting USB Serial
Function to RTTY Decode, tuning an RTTY signal, and capturing the port. That
costs rig control for as long as it runs (HM-DEC-069), so it is an experiment
somebody chooses to do rather than something the app can find out on its own.

**Update 2026-08-14: open, and dormant.** Tim has ruled RTTY off the list
altogether and the thinking moved to FG-012. Nothing is waiting on this: if the
mode ever returns, the route recorded there is Hamlet demodulating the audio the
way it already does Morse, and that route never reads this port at all.

Severity `none`: nothing is blocked. HM-DEC-069 already rules that the mode is
not built, and for a reason this answer would not change on its own.

---
id: HM-OPEN-009
status: open
owner: tim
raised: 2026-08-15
severity: none
refs: HM-DEC-074, HM-DEC-049, src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs
---

Holding TRANSMIT down is one of the three ways a command `17` message reaches the
air, and Hamlet refuses to send while the radio reports it is transmitting.

Footnote 2 on p. 19-7 says a CW message sent with `17` is transmitted when
TRANSMIT is on, **or** an external TX switch is on, **or** break-in is on.
`TransmitReadiness` returns `AlreadyTransmitting` and refuses whenever the radio
reports transmit status on, which closes off the first of those three.

The refusal is in the conservative direction: nothing goes out unexpectedly, and
break-in is the ordinary path for keyer sends and the one the panel now names.
So this costs an operator who prefers to hold the transmitter on himself, and it
costs nobody a signal they did not ask for.

Left alone deliberately on 2026-08-15 rather than fixed, because loosening a
transmit precondition hours before a live contact is not a change worth making
against a benefit this small (§0.2). What it needs is a decision about whether
Hamlet should distinguish "transmitting because the operator is holding it on",
which is permission, from "transmitting because a send is already in flight",
which is a reason to wait. The rig state model reads one flag for both.

**Update 2026-08-15 (HM-DEC-077): the refusal is now visible, which changes what
this costs.** Transmit status is checked before mode and before break-in, so it
refuses ahead of both, and until now nothing recorded that it had. Every readiness
evaluation now carries the transmit-status reading with its provenance and age, so
a session where this fired can be told from one where it did not, and it is one of
the things the next file will settle about the greyed-out buttons.

The ruling is unchanged and the gate is not loosened. What was missing was never
the strictness, it was the silence.

Severity `none`: the ordinary path works and the refusal explains itself.

---
id: HM-OPEN-010
status: open
owner: unassigned
raised: 2026-08-15
severity: slows
refs: HM-DEC-075, HM-DEC-038, FG-008
---

"Did anybody hear me" cannot say how far his signal went, because Hamlet has no
skimmer locations.

The reports carry the receiver's callsign, the signal-to-noise it measured and
the speed it read, which is what the RBN line format states. What would make the
panel land is the distance: "19 dB" means nothing to a newcomer and "your signal
reached Nevada, 2,050 miles" is the thing he would remember for the rest of his
life.

The obstacle is a ruling rather than an oversight. HM-DEC-038 says no grid means
no distance anywhere, and names this exact case: a callsign says where a license
was issued and not where its owner is standing, and stacking that guess under a
figure in miles would dress it as a measurement. So the prefix cannot be turned
into a location, and there is nothing else in the feed to use.

What closes it is a **cited file of skimmer locations under `data/`**, with a
source mark on every row in the shape `data/bands/` and `data/privileges/`
already use (§4). RBN's own node list is the obvious candidate and it was not
verified in this session. Skimmers that are not in the file get no distance,
exactly as a spot with no grid gets none today.

Severity `slows`: the panel works and says only true things, and it is doing half
of what it exists for.

---
id: HM-OPEN-011
status: open
owner: tim
raised: 2026-08-17
severity: slows
blocks: the real-signal regression corpus, and confirmation that HM-DEC-090 fixed the reported fault
refs: HM-DEC-090, HM-DEC-088
---

The three real captures HM-DEC-090 was written from are not in the repository.

The brief of 2026-08-17 described `cw-2026-08-16-225822`, `-225835` and
`-233446`, with hashes, tone frequencies and measured levels, and asked for all
of them to be committed as permanent regression fixtures. **They were never on
the machine the session ran on.** `%AppData%\Hamlet\captures` did not exist and a
search of the user profile and the repository found nothing.

Everything in HM-DEC-090 was therefore measured against synthesized audio built
to reproduce the one property those captures demonstrate: a strong narrow tone
present for a small fraction of the recording. That is a faithful stand-in and it
is not the evidence.

What is needed: the three WAV files and their sidecars, copied into
`tests/fixtures/cw/`. Once they are there, the decoder can be run against the
real thing and the claim that it now finds a tone near 627 Hz and 595 Hz can be
stated as a measurement rather than as a reasonable expectation. §2.1 makes an
off-air recording Tim's to review before it ships in a public repository, which
is the other reason this cannot be done without him.

---
id: HM-OPEN-012
status: open
owner: claude
raised: 2026-08-17
severity: slows
blocks: reading a real station, which is what the application is for
refs: HM-DEC-091, HM-DEC-090, HM-DEC-048
---

The keying gate's peak tracker cannot survive a station that keys five percent of
the time, and the fix that works breaks the one guarantee that cannot be traded.

**The mechanism, measured on `tests/fixtures/cw/captured`.** `CwGate` places its
threshold below a tracked peak that follows a signal down over a couple of
seconds, so a fade cannot strand it above the signal (HM-DEC-048). A station
answering a call sends short bursts seconds apart. Between them the peak decays
the whole way to the noise, `PeakDb - NoiseFloorDb` collapses to about eight
decibels against a `MinimumSpreadDb` of ten, and the gate stops deciding on a
signal that the narrowband measurement puts twenty-eight decibels above the band.
With the threshold that low the key also stays down through the gaps: eleven
seconds of key-down were measured in a recording containing roughly one and a
half.

It is the same duty-cycle fault HM-DEC-090 fixed in the reported ratio and the
located pitch, one layer further down.

**The fix that works.** Build the threshold from the held narrowband figure
rather than from the tracked peak: `NoiseFloorDb + heldSpread - drop`, with
`HasSignal` reading the same held figure. Measured:

- `cw-2026-08-17-013347`: one unreadable character becomes `I■E■N`
- `cw-2026-08-17-013622`: nothing becomes `■EI`
- key-down falls from 11.2 s to 7.1 s, marks from 138 to 72
- synthetic sensitivity improves from −4.0 dB to −5.0 dB

**Why it is not shipped.** It makes the decoder confidently wrong on
`fading-18wpm`, failing `NothingTheDecoderWasSureOfIsWrong`. A held peak is the
right answer for deciding whether a tone exists and the wrong one for deciding
where the threshold goes, because after a fade it strands the threshold above the
signal, which is exactly what the tracked peak was designed to prevent.

**Three narrower variants were tried and none is both safe and useful.** Falling
only while the key is down: two fade tests still fail. Falling normally unless
the held figure says a tone is still present: the held figure stays high through
a five-second fade, so it does not discriminate. Holding only once the power is
within six decibels of the noise: one fade test still fails. Holding only once
the tracked spread has already collapsed past `MinimumSpreadDb`: everything
passes and the real captures are unchanged, so it rescues nothing.

**What is probably needed.** The gate wants a threshold whose memory of the
signal is separate from its memory of the silence, which is the same shape of
answer as HM-DEC-090's held peak but applied to marks rather than to
measurements. Something like a peak over the last N *marks* rather than over
time. Whatever is tried, `fading-18wpm` and the two captures now fail in opposite
directions, so the corpus can tell a real fix from a trade.

---
id: HM-OPEN-013
status: closed
owner: tim
raised: 2026-08-17
closed: 2026-08-17
severity: slows
blocks: naming the CI-V USB Port setting as a reading rather than as a candidate
refs: HM-DEC-092, HM-DEC-093, HM-DEC-071, §4
---

**CLOSED 2026-08-17.** Tim supplied the citation: `1A 05 0074`, Full Manual
p. 19-5, "Send/read the CI-V USB port setting (00=Link to [REMOTE], 01=Unlink to
[REMOTE]) (Read only)". Recorded as FACT-002 in `SHACK_FACTS.md` and added to the
rig state model (HM-DEC-093). It is read so the precondition is a measurement and
never so that anybody is asked to go and look at it.

Is `1A 05 0074` the CI-V USB Port setting, and on which page of `A7292-4EX-6`?

The brief of 2026-08-17 states that `1A 05 0074` reads it, `00=Link to [REMOTE],
01=Unlink from [REMOTE]`, and asked for it to be added to the rig state model
with its citation. **It has not been added, because §4 requires a page number
from a column-aware read of the settled edition and this session had no access to
the manual.**

That discipline is not ceremony here. The command table is two columns, a
flattened read is what put the CW pitch on `14 08` instead of `14 09`, and that
error survived for weeks and would have moved somebody's passband while trying to
read a pitch (HM-DEC-050, HM-DEC-071). A sub-command taken on trust is exactly
the same shape of mistake.

What is needed: the row confirmed against `A7292-4EX-6` with a column-aware
extraction, and its page. It is almost certainly in the `1A 05` settings block
around pp. 19-4 to 19-6.

**What it unlocks.** The scope's data output has two documented preconditions
(p. 19-7, footnote 4). Hamlet knows the baud rate because it opened the port
itself. With this row it would know the other, and the refusal message could name
which condition failed as a reading rather than offering the remaining candidate
as something left to check (HM-DEC-092).

---
id: HM-OPEN-014
status: open
owner: claude
raised: 2026-08-17
severity: none
refs: HM-DEC-093, §8
---

`TheDecoderAggregationDoesNotAllocatePerCharacter` fails under concurrent load.

Seen once on 2026-08-17 while two `dotnet test` processes were running at the
same time, and passing on every isolated run and on three consecutive clean full
runs afterwards. It measures allocation with `GC.GetAllocatedBytesForCurrentThread`
and asserts a ceiling, which is a real and worthwhile property (§8: the decoder's
own record may not allocate per character) measured in a way that another busy
process can disturb.

Not urgent and not ignorable: it will flake in CI on a shared runner, and a
guard that cries wolf is one somebody eventually reruns without reading. Worth
either widening the ceiling with a stated margin or forcing the test onto its own
xUnit collection so nothing runs beside it.

---
id: HM-OPEN-015
status: open
owner: tim
raised: 2026-08-17
severity: slows
blocks: items 3, 3b and 4 of the 2026-08-17 work order
refs: HM-DEC-094, HM-DEC-090
---

The decoder work in the 2026-08-17 brief cannot start: three of its five
captures and both reference documents are absent from this machine.

The brief cites `CW_RECEIVE_BRIEF.md` and `cwdecoder.py` for the validated
receive chain, and names five captures. Present: `cw-2026-08-17-013347` and
`cw-2026-08-17-013622`. **Absent: the 22:58 pair, the 13:47 interference
capture, the 23:26 group, and both documents.**

What is blocked, and why each needs what is missing:

- **Item 3, the tone detector's frequency.** The comparison table covers four
  captures and two of them are not here. A detector tuned against the two
  present ones would be tuned against a quarter of the evidence.
- **Item 3b, the two-stage Goertzel chain.** The reference implementation and
  its measured parameters are in `cwdecoder.py` and `CW_RECEIVE_BRIEF.md`.
  Reimplementing from the brief's summary would be guessing at the numbers that
  matter, which is what the 20 Hz ENBW figure exists to prevent.
- **Item 4, interference.** The 501 Hz carrier lives in the 13:47 capture. There
  is nothing here to detect it in, and a detector for a thing nobody can
  reproduce is untestable by construction.

The brief's own warning applies and is worth repeating: the operator heard CW in
the 13:47 capture that independent analysis could not find. Human copy at low
signal-to-noise beats automatic detection, and an analysis finding nothing is not
evidence that nothing is there.

---
id: HM-OPEN-016
status: open
owner: claude
raised: 2026-08-17
severity: hard
blocks: merging feature/honest-cw-detection, and sessions 2 and 3 of the batch brief
refs: HM-DEC-095
---

The keying-structure detector regresses eleven tests against synthesized
fixtures, and must not merge until they pass or are shown to be wrong.

Nothing here is a real-signal failure. Every one is a synthetic fixture, and
the common cause is architectural: the old tracker retuned to the loudest bin
five times a second, which is perfect for one clean strong tone and wrong on
every real recording. The new survey wants three seconds of keying evidence
and two agreeing readings before it moves, which is right on the air and slow
on a fixture that lasts eight seconds.

| Test | What it does now |
|---|---|
| `ASignalAtTheWrongPitchIsStillFound` (400, 875 Hz) | `■ DE W1AW K` — loses the opening character to acquisition |
| `ASignalAtTheWrongPitchIsStillFound` (500, 750 Hz) | `■ B ■AW K` — worse, and `B` is a wrong character rather than a placeholder |
| `ACleanSignalDecodesExactly` (25 wpm) | fails; 12 and 18 pass |
| `TheCleanRecordingsDecodeExactly` / `EveryRecordingGivesBackTheShareItShould` (clean-25wpm) | as above |
| `TheSpeedEstimateFollowsAChangeWithinAFewCharacters` | speed adaptation across a change |
| `AFadingSignalComesBackRatherThanStayingDead` | fade recovery |
| `TheDecoderReadsAsFarDownAsItDidBefore` | reads to −2 dB and below, but the 17 and 18 dB rows are worse than the 10 dB row |
| `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone` (app) | transcript content after a clear |

**The sensitivity one is the most interesting and should be read before the
others.** The decoder still returns better than half the characters correct at
minus two decibels. What broke is the top of the range: eighteen decibels out of
the noise returns a third right and a third wrong, which is worse than the same
decoder manages at ten. A strong signal failing where a weak one succeeds is not
a sensitivity problem, it is something firing on strong signals that does not
fire on weak ones, and the two candidates already found and fixed in that family
were the window switching without hysteresis at eighteen words a minute and the
speed being discarded whenever the tracker moved.

**The 500 and 750 Hz cases deserve their own look.** Those two produce a wrong
letter where 400 and 875 produce a placeholder, and they are the two nearest the
600 Hz starting pitch, which suggests the fine bank is being left straddling the
signal rather than moved onto it.

What must not be done to close this: loosening the separation limit, the
confirmation rule, or the plausibility bounds. Those are what stop a carrier
being announced as a station, and every one of them was set from a measurement
with margin on both sides (HM-DEC-095).

---
id: HM-OPEN-017
status: open
owner: tim
raised: 2026-08-17
severity: hard
blocks: finishing session 1 of the batch brief
refs: HM-OPEN-016, HM-DEC-095, HM-DEC-048, CW_RECEIVE_BRIEF.md, cwdecoder.py
---

The validated reference decodes in two passes over a whole recording and
Hamlet decodes in one pass as the audio arrives. Which of those Hamlet is
supposed to be is a ruling nobody has made.

`CW_RECEIVE_BRIEF.md` says to port the reference's behavior and not its
structure. Three attempts at that this session each made the real recording
worse, and the reason is that the behavior is not separable from the
structure:

**The reference fits the element clock and then goes back and re-reads the
whole recording with it.** `run()` de-glitches at twenty milliseconds, extracts
the runs, fits the clock from them, then de-glitches again at four tenths of a
dit and extracts every run in the recording a second time before a single
character is decoded. Its gate does the same thing at a coarser grain: it walks
the entire envelope in overlapping three-second blocks and fits a threshold to
each before any of them is used.

**Hamlet's decoder cannot do that and still be Hamlet.** It measures one hop at
a time, commits each element as it ends, and emits characters while the operator
watches. There is no second pass available, because the second pass would have
to run over audio that has already been shown to somebody. Everything Hamlet
holds back is latency on a live screen.

Measured this session, grafting the reference's pieces onto the streaming chain
one at a time:

| Change | Capture 1 (`013347`) |
|---|---|
| Start of session | `■ ■` — two characters |
| Fine bank read whole, loudest bin per hop | `■   ■W■RR ■` — seven characters |
| Plus the reference's clustering gate | `W■■` — three characters |
| Reference decoder itself, batch | `▯ ▯ ▯ ▯ ▯ ▯ MVRRVA3VRR`, confidence 0.74 to 1.00 |

The middle row is kept. The clustering gate is reverted, and it is not that the
gate is wrong: it is the right gate for a decoder that can fit a threshold to a
block and then apply it to that same block, and the wrong one for a decoder that
has to answer before it has heard the block.

**Three ways out, and the choice is not Claude's:**

- **Decode twice.** Keep the streaming pass for what is on screen now, and run a
  second, batch pass over the last half minute whenever the tap has it, revising
  the transcript behind the cursor. Honest about what it is doing and the most
  work; it also means characters change after they are displayed, which is its
  own §0.0 question.
- **Delay the display.** Hold everything for three seconds and decode the block
  that has just closed. Simple, matches the reference exactly, and puts a
  three-second lag on the one screen where the operator is trying to keep up with
  a live contact.
- **Accept the streaming approximation.** Take what a single pass can do, which
  today is about seven characters out of eleven with the rest as placeholders,
  and say so on screen. Nothing here breaks §0.0 — every character it will not
  stand behind is already a placeholder — but it does not meet the brief's own
  definition of done.

**What is not in doubt** is that the reference is right about this audio and
Hamlet is not yet. Its answer, `MVRRVA3VRR` at high confidence, matches the
independent hand analysis, and it is what the operator needs on the evening he
uses this.

---
id: HM-OPEN-018
status: open
owner: claude
raised: 2026-08-17
severity: slows
refs: HM-OPEN-016, HM-DEC-095, HM-DEC-048
---

The synthesized fixtures have no noise floor, which makes them unrepresentative
of every real receiver, and the reference decoder scores zero on all of them.

Run `cwdecoder.py` against this repository's own fixtures and it decodes nothing
at all:

| Fixture | What the reference does |
|---|---|
| `clean-12wpm` | active 20%, no clock, emits nothing |
| `clean-18wpm` | active 11%, no clock, emits nothing |
| `clean-25wpm` | **active 0%**, no tone found at all |
| `prosigns-18wpm` | active 10%, no clock, emits nothing |

The cause is the same one that cost this session an afternoon. Those fixtures
are tone-or-silence: between elements the samples are exact digital zero, which
measures about minus two hundred and forty decibels. Any transmit-mute guard
reading a level that low as "the receiver is muted" blocks the gaps between
every element, and there is nothing left to decode. The reference has no lower
bound and blocks all of them. Hamlet now has one at minus ninety, measured from
the real captures where the mutes bottom out around minus eighty-two, and passes
the ones the reference fails.

**A real receiver never hands over digital silence.** There is always band noise,
which is why `noisy-18wpm` and `fading-18wpm` are unaffected and why every
failure in HM-OPEN-016 is against a noiseless fixture. The fixtures encode an
assumption about the audio path that the audio path does not have.

`CW_RECEIVE_BRIEF.md` §4 anticipates this and specifies a replacement recipe:
noise shaped to a 500 Hz passband, 3 dB in-passband SNR, two-path QSB, an
interfering carrier, and a preamble of QSK-style mutes **at minus ninety
dBFS** rather than at zero. Building it is a session's work on its own and it
would replace, not join, the noiseless fixtures.

Recorded rather than acted on. Making failing tests pass by rewriting their
fixtures is exactly the move that deserves suspicion, and the case for it here
rests on a measurement anybody can repeat: run the reference against them.

---
id: HM-OPEN-019
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-OPEN-018, HM-DEC-097, FIXTURE_BRIEF.md
---

Phase 6's retirement assessment ran and **nothing qualified**, so both fixture
sets stay in place.

`FIXTURE_BRIEF.md` phase 6 asks for superseded fixtures to be retired one at a
time with a reason each, and names leaving both sets in place as the safe
state. That is where this landed, and the reasoning is worth keeping so the
next session does not redo it:

| Old fixture | Tests | Retired? | Reason |
|---|---|---|---|
| `clean-12wpm` | all pass | no | Passing. Retiring it removes coverage and gains nothing |
| `clean-18wpm` | all pass | no | Passing |
| `prosigns-18wpm` | all pass | no | Passing, and the new `prosigns-*` set does not yet read well enough to replace it |
| `noisy-18wpm` | all pass | no | Passing |
| `fading-18wpm` | all pass | no | Passing |
| `interference-18wpm` | all pass | no | Passing |
| `clean-25wpm` | 2 fail | **no** | The only twenty-five words a minute coverage in the repository. The rebuilt suite is at twelve, so retiring this deletes the fast-fist case rather than replacing it |

**The one that fails is the one with no replacement**, which is the shape that
makes retirement destroy evidence rather than tidy up. A twenty-five word a
minute fixture on realistic audio would close it, and the measurement to do
that first is already in `CwAdjudicationTests`: at that speed Hamlet reads
`■ALL N0CALL K E` off a realistic signal against nothing at all off the
noiseless one, so the scenario is real and the fixture is what was wrong.

---
id: HM-OPEN-020
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-OPEN-018, cwdecoder.py, CW_RECEIVE_BRIEF.md
---

The reference chain measures every mark about twenty-five milliseconds long,
and at high contrast that pushes a real fist below its own ratio floor.

Measured across the rebuilt fixtures, on the fist recorded off the air — dit
105 ms, dah 283 ms, a true ratio of **2.70**:

| Gate contrast | Measured dit | Measured dah | Ratio | Read? |
|---|---|---|---|---|
| 10 dB | 109 | 295 | 2.70 | yes |
| 13 dB | 112 | 294 | 2.63 | partly |
| 22 dB | 128 | 305 | **2.39** | **refused** |

The cause is arithmetic rather than mysterious. A twenty hertz detection
bandwidth needs a fifty millisecond window, and a window that long smears each
keyed edge over about the same. The gate crosses its threshold early on the
rise and late on the fall, so every mark measures long by roughly a constant,
and adding a constant to both lengths compresses their ratio. The stronger the
signal, the lower the threshold sits relative to the peak, and the earlier it
crosses.

`cwdecoder.py` refuses any clock outside 2.5 to 3.8, so **it refuses at fifteen
decibels a fist it reads perfectly at zero**. Three rebuilt fixtures are held
out of the phase 4 gate for this and none of them was edited to get round it:
`tightfist-easy`, `tightfist-working`, `qsk-preamble`.

This matters beyond the fixtures. `FIXTURE_BRIEF.md` phase 4 says a fixture the
reference cannot decode is a bad fixture rather than a Hamlet failure, and here
**the fixture is a measurement taken off the air and the floor is what fails**.
The rule is right about the common case and this is the case it does not cover.

Two things worth settling, neither of them Claude's:

- whether the 2.5 floor should be widened, or the bias corrected before the
  ratio is taken, in Hamlet's own clock fit — which uses the same floor
  (`CwToneSurvey.MinimumRatio`) and will meet the same wall;
- whether a held-out list is the right shape for phase 4's exception, or
  whether the gate should record a reason per fixture instead.

A separate finding from the same work, recorded so it is not rediscovered:
**five dahs in a row do not survive a tight fist.** `N0CALL` contains `0`, and
at sixty-five millisecond gaps read through a fifty millisecond window those
five dahs merge into a single mark of about one and two thirds of a second,
which dominates the clock fit and collapses it. The reference read
`K W BG EN` out of `N0CALL N0CALL`. The tight-fist message avoids digits for
that reason and the case deserves a fixture of its own.

---
id: HM-OPEN-021
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-OPEN-019, HM-OPEN-020, HM-DEC-101, HM-DEC-103, HM-DEC-105, GENERATOR_BRIEF.md
---

The eleven re-adjudicated against the corrected fixtures. **Every entry decided
last session against defective audio has been re-tested and only one verdict
moved.**

Phase 6 of the generator brief. The table in HM-OPEN-019 predates the generator
fix and this supersedes it.

| Test | Fails on realistic audio? | Fault |
|---|---|---|
| `ASignalAtTheWrongPitchIsStillFound` (400) | no — found at 400, reads `N0CALL K` | fixture |
| `ASignalAtTheWrongPitchIsStillFound` (500) | no — found at 525, reads `■0CALL N0CALL K` | fixture |
| `ASignalAtTheWrongPitchIsStillFound` (750) | no — found at 750, reads `CALL N0CALL K` | fixture |
| `ASignalAtTheWrongPitchIsStillFound` (875) | no — found at 875, reads `DE N0CALL N0CALL K` | fixture |
| `ACleanSignalDecodesExactly(25)` | partly — reads `■ALL N0CALL K E` against nothing on the noiseless one | mostly fixture |
| `TheCleanRecordingsDecodeExactly(clean-25wpm)` | as above; **not retired**, its replacement `fast-easy` does not pass the gate | mostly fixture |
| `EveryRecordingGivesBackTheShareItShould(clean-25wpm)` | as above | mostly fixture |
| `AFadingSignalComesBackRatherThanStayingDead` | **yes** — on a twelve decibel fade it reads `■ ■ ■ S■ ■ F■ R D E` where the reference reads 53% | **Hamlet** |
| `ItGoesQuietRatherThanInventingLettersInTheNoise` | no — nothing emitted at −3, −6 or −10 dB | fixture; bound predates HM-DEC-097 |
| `TheSpeedEstimateFollowsAChangeWithinAFewCharacters` | **now adjudicated** — see below | **Hamlet**, and needs a ruling |
| `ClearingTheTranscriptLeavesTheDecoderAlone` (app) | no — 12 wpm at 625 Hz before and after the clear, 8 characters then 23 | fixture |

**Only the fade verdict was at risk and it survived.** It was attributed to
Hamlet last session on audio carrying a twenty-five decibel fade that deleted
most of the message rather than fading it. The fade is now twelve decibels, the
reference reads 53 percent of the same file, and Hamlet still returns mostly
placeholders. The attribution stands on better evidence than it was made on.

**The two that had never been adjudicated now have been**, which was the point
of HM-DEC-104. Clearing the transcript is a fixture fault. The speed estimate is
not, and it found something nobody had looked for.

---
id: HM-OPEN-022
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-OPEN-021, HM-DEC-090, HM-DEC-104
---

Across a change of station the decoder names sending speeds belonging to
neither of them.

Measured on the two-station recording, a caller at eleven words a minute
handing over to an answerer at twenty-two:

- it names **16 and 18**, which is the average of the two and describes nobody;
- it names **24, 26, 27, 28, 29, 30, 31, 34, 36, 37, 41, 42 and 44**, all faster
  than either station;
- it comes to rest correctly, naming no speed at all rather than a wrong one.

Where it settles is already covered and asserted. What is not settled is whether
a streaming decoder may put a transitional speed on screen at all while its
clock re-acquires, or must withhold one until the new clock is proved.

HM-DEC-090 already ruled **one guarded answer, read by every surface**, on the
grounds that a speed is a fact about somebody's keying. A number that belongs to
no station on the band is not that, and it appears on the one screen a beginner
uses to judge whether they could have copied the exchange.

This is a question about what the display asserts, so §12.1 reserves it. Two
shapes it could take, neither of them Claude's to choose:

- **withhold** — name no speed between clock loss and the next confirmed clock,
  which costs nothing but a gap and matches the refusal machinery already in
  place;
- **mark it** — show the transitional reading as unsettled, the way the
  provisional tip already distinguishes itself from settled text.

---
id: HM-OPEN-023
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-DEC-105, HM-OPEN-020
---

The half-amplitude correction is applied in the settled pass and deliberately
not in the tone survey, and the second half is unresolved.

HM-DEC-105 ruled that the dah-to-dit floor stays at 2.50 and what the ratio is
computed over gets fixed. In the settled pass that lands cleanly: deciding six
decibels below the keyed level rather than midway between the two clusters
measures a mark at the length it was, and unresolved characters fell from 73 to
50 percent on `exchange-easy` and from 75 to 33 on `coverage-easy`.

Applied in `CwToneSurvey` as well it costs:

- five noiseless fixtures, which is the class HM-OPEN-018 established encode a
  physical impossibility and would be weak evidence on its own; and
- **the real 13:47 off-air capture**, where the tone stops being found at all.
  That is not a fixture argument.

The reason is that the two are answering different questions. The settled pass
measures how long a mark is, and half amplitude is where an element's true edge
sits. The survey decides whether a bin holds anybody keying, judged on the
separation between two clusters of mark durations; moving the decision up the
leading edge shortens every mark and tightens that separation, which is exactly
the measurement the survey exists to make.

A correction that improves one measurement and breaks another is not one
correction. Whether the survey should be corrected differently, or left alone,
is Tim's.


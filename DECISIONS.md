# Decisions

Rulings, newest first. A ruling is never edited — a later decision supersedes
it by id. Index in `CLAUDE.md` §1.

---
id: HM-DEC-030
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Rig/RigCapabilities.cs, HM-DEC-003
---

`IRig` gains a capabilities record — model, spectrum scope, built-in keyer,
USB audio, whether it can transmit, and which bands it covers — and the UI
degrades honestly on a radio that lacks a feature rather than showing a
control that cannot work.

HM-DEC-003 confined Hamlet to one radio behind an interface and named
multi-rig support as the condition for revisiting. This is that revisit
arriving early and cheaply, while there are still only two implementations to
change. Every assumption about the IC-7300 that lives at a call site is a
place a second radio will break, and they are much easier to remove now than
after phase 2 has built a scope UI on top of them.

Capabilities are reported by the implementation and have no setter, the same
shape as `IsSimulated` and for the same reason: a radio is the only thing that
knows what it is. `RigCapabilities.Unknown` claims nothing at all, so a radio
that has not said cannot inherit the 7300's feature set by default — which is
the assumption the type exists to remove.

The training radio claims a spectrum scope, because the synthesiser genuinely
is one, and refuses transmit, because there is nothing behind it to transmit
with. That is the one claim it must never make.

---
id: HM-DEC-029
date: 2026-08-13
refs: data/privileges/us-part97-privileges.json, src/Hamlet.RadioEngine/Licensing/PrivilegePlan.cs, src/Hamlet.RadioEngine/Licensing/TransmitGuard.cs, src/Hamlet.App/Controls/NeighborhoodMapControl.cs, HM-DEC-009, HM-DEC-008, HM-OPEN-005
---

US Part 97 transmit privileges are cited data under `/data`, not carried
knowledge. The band map shows them as a veil over the culture map, tuning is
never restricted, the status line explains rather than scolds, and an
unresolved licence class draws NO overlay rather than a guessed one.

THE ONE FACT THAT DOES THE MOST WORK: listening is never restricted. Any
licence may receive anywhere; the rules are about transmitting. The operator
this serves has been licensed six years and has never made a contact, and part
of that is a quiet fear of transmitting somewhere he is not allowed. Every
piece of this is shaped to make that distinction plain rather than to imply
the band is full of forbidden zones — which is why the veil is faint enough to
read the neighbourhood colour through, why it is labelled "listen only", why
the reassurance sentence appears whenever transmitting is restricted, and why
the tone is amber and never red. Being outside your privileges while tuning
around is not an error. It is the ordinary state of most of the band for most
licences, and the app should sound like it knows that.

The data is a transcription of 47 CFR 97.301, 97.305 and 97.307, read from
eCFR's versioner API on 2026-08-13, with the paragraph cited on every row.
This has legal weight and must not come from anybody's memory. The ARRL band
chart is named as the familiar rendering it is, and marked "convenience": where
it and the CFR ever differ, the CFR wins.

The two CFR tables are carried SEPARATELY, as the regulation carries them, and
the join that answers "may this class send this mode here" happens in code with
tests. A pre-joined table would be a third artefact free to disagree with both
its parents (§0). What that join has to know is not obvious — 97.305(a) puts CW
on any frequency the class may use, so CW is absent from the emission table
entirely; 97.307(f)(9) makes a Technician's HF privileges Morse-only;
97.307(f)(11) keeps 7.075–7.100 phone away from the contiguous US. Each of
those is a test.

Figures the sources do not state are explicit unknowns with reasons — 60 m's
five channels, VHF and UHF, power limits, Regions 1 and 3. A Technician's 2 m
privileges are their most-used privilege, and a file that stayed silent about
omitting them would read as complete.

TUNING NEVER RESISTS. The operator may tune anywhere, including deep into
Extra-only territory: nothing blocks, nothing pops up, nothing beeps. The
marker turns red with a small flag, the dots outside privileges dim rather than
vanish — the operator still needs to see where the action is — and the status
line says what is true. The upgrade ladder appears on click and never as
permanent chrome, and collapses again the moment the frequency becomes theirs.
Restriction becomes motivation; the same words shown unbidden would be a nag.

AN UNRESOLVED CLASS DRAWS NOTHING. Not a permissive overlay, not a restrictive
one. `SpansFor` returns an empty list for an unknown class, so "do not guess"
is structural rather than a rule the control has to remember, and the map looks
exactly as it did before privileges existed. This is HM-DEC-009 at the one
point in Hamlet where a confident error has legal consequences.

THE SPANS ARE THE ONE SET OF BOUNDARIES. They are computed once, in the
ViewModel, from the cited data, and handed to the map. If the waterfall or the
dial tape ever shows privileges they take the same list rather than computing
their own — two pictures of one law that disagreed would be worse than either
alone.

THE GUARD RAIL IS TRANSMIT ONLY. "Only let me transmit where my licence
allows", on by default, consulted at exactly one moment: before Hamlet keys a
transmitter. It is never asked about tuning, receiving or drawing. No transmit
path exists yet — HM-DEC-008 gates keying on the vendored manual — so the
setting, the check and its tests are built now and THE SEAM IS THIS: whatever
first keys the transmitter, CI-V 0x17 or PTT, calls `TransmitGuard.Check` and
honours the answer. The override is passed per call rather than read from
settings, so it can live beside the transmit control: somebody deliberately
keying outside their privileges should reach for it consciously, and somebody
tuning around should never meet it. An unknown class does not block
transmitting — Hamlet has no business refusing to key a radio because a lookup
service was down.

---
id: HM-DEC-028
date: 2026-08-13
refs: src/Hamlet.App/Licensing/LicenceResolver.cs, src/Hamlet.RadioEngine/Licensing/CallsignLookup.cs, HM-DEC-019, HM-DEC-024
---

The operator's licence class lives in the profile with its provenance, is
resolved lazily and automatically whenever a callsign is present and the class
is unknown, and a lookup never silently overwrites a hand-set value — a
mismatch is shown with both values and the operator decides.

LAZY, NOT A WIZARD STEP. People skip wizards, and the callsign can arrive from
Settings, a hand-edited settings file or a version that never asked. So
resolution is attached to the fact rather than to a screen: on startup and
whenever the profile changes, a callsign with no class gets looked up. It never
blocks and never opens a dialog. The status bar narrates — "Looking up
KC3QIS…", then "General — from FCC data, today." — and that visible competence
is the point.

Provenance travels with the value. "General, from FCC data, today" and
"General, because you said so in 2019" are different claims and the operator is
entitled to see which they are looking at.

A LOOKUP NEVER OVERWRITES A HAND-SET CLASS. If the operator set General and the
FCC data says Extra, both are shown with the source and the date and nothing is
written until they choose. It is their licence. Software that silently
corrected them would be wrong even on the occasions it was right. Declining is
an answer, and the profile is re-stamped so the same question does not reappear
tomorrow.

THE SERVICE, AND ITS TERMS. callook.info, which republishes FCC ULS data. Its
API reference states under a "Usage Terms" heading: "The callook.info API is
publicly available and is free to use however you wish." No rate limit, no
attribution requirement, no restriction on automated access, and nothing about
how the software was written. Read 2026-08-13. Unlike SOTA (HM-DEC-024) nothing
in those terms stands in the way, so this ships on. Politeness is still
self-imposed: the User-Agent names the app, its version and the operator.

WHAT IS READ, AND WHAT IS NOT. The response carries the licensee's full name
and street address. Hamlet reads the operator class and nothing else, and the
result type has nowhere to put the rest. It is the operator's own record, but a
program that quietly harvested a home address because it happened to be in the
payload would be doing something nobody asked for.

The callsign goes to the lookup service — the class is public information, as
public as the callsign itself, and it is in the FCC's own searchable database.
It still never enters telemetry. HM-DEC-019's rule is unchanged and the privacy
walk grew to cover the five events this work added.

NOBODY IS EVER BLOCKED. The ladder is API lookup, then hand selection in
Settings, and an unresolved class is a supported state throughout: the band map
draws no overlay and says why, and the guard rail lets transmissions through
while saying what it does not know. The FCC bulk-download rung named in the
brief is not built; the API and hand selection cover every case reached so far,
and a 100 MB download offered to somebody whose lookup merely timed out would
be worse than the "try again later" they get now. Recorded here so the next
session knows it was a decision rather than an omission.

---
id: HM-DEC-027
date: 2026-08-13
refs: src/Hamlet.App/Controls/WaterfallControl.cs, src/Hamlet.RadioEngine/Training/ModeAudio.cs, HM-DEC-006, HM-DEC-005, HM-DEC-012, FG-002
---

The waterfall renderer is built now, against synthesised frames of the same
shape CI-V `0x27` will deliver, so phase 2 swaps the data source and not the
UI. The field guide's audio is synthesised rather than recorded.

Building the renderer against a fake source is not a compromise, it is the
point. `SpectrumFrame` carries a span, a timestamp and a run of one-byte
amplitudes because that is what the IC-7300's scope reports; when the radio
starts filling those frames the control does not change. And a renderer that
exists is a renderer being exercised — the alternative was writing it blind in
phase 2 against hardware, with no way to tell a rendering bug from a CI-V
parsing bug.

Built as HM-DEC-006 requires: the control owns a `WriteableBitmap` and
subscribes to the engine's event directly, and no spectrum data passes through
data binding. Frames arrive on the source's thread, which does nothing but
write ints into a plain array under a short lock; a UI-side timer copies that
array into the bitmap. Measured at 0.012 ms to synthesise a frame and 0.006 ms
to scroll and map it, against a 40 ms budget at twenty-five frames a second.

The waterfall is a dark instrument surface on warm paper. That is consistent
with HM-DEC-012 rather than an exception to it — the same reasoning already
applied to the rig's LCD. Faint detail is what a waterfall is for, and faint
detail on white is unreadable.

Clicking the waterfall tunes to that frequency. It shares its frequency
mapping with the dial tape and the neighborhood map, so a click lands where
the operator is pointing and all three markers move together. That is phase 2's
click-a-signal gesture, built early because the training radio makes it useful
before any hardware exists.

Field-guide audio is generated, not recorded. Recorded clips carry a licence
and a provenance question into a GPL-3.0 repository, cannot be parameterised,
and cannot be asserted on. Generated audio is licence-free, byte-for-byte
deterministic, testable, and adjustable — CW at 12, 18 and 25 WPM is how
somebody finds the speed they can actually copy, which is the groundwork FG-002
needs. SSB is offered tuned and mistuned side by side, because hearing those
two back to back is the fastest way to learn what the tuning knob is for. Each
card's fingerprint is animated by the same synthesiser that draws the
waterfall, so the picture on the card and the picture on the panel are the same
picture and recognising one is recognising the other.

---
id: HM-DEC-026
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Training/TrainingSpectrumSource.cs, src/Hamlet.RadioEngine/Rig/TrainingRig.cs, src/Hamlet.RadioEngine/Training/TrainingBandPlan.cs, HM-DEC-009, HM-DEC-016
---

The simulated radio is a training feature, not a test double. The waterfall
states that its signals are simulated whenever the connected rig is simulated,
and that statement is derived from connection state rather than set, so the app
cannot show synthetic signals unlabelled. Synthesised signals sit at real
band-plan frequencies, so practice teaches the real band.

`FakeRig` becomes `TrainingRig` and the port list says "Training radio (no
hardware)". Someone licensed since 2020 who still cannot tell one signal from
another needs to practise, and practising on the air means owning a radio,
having an antenna up, and hoping the band is open tonight. Here they can learn
the waterfall and the sound of each mode with nothing plugged in. It still
backs UI development and engine tests; what changed is that it is now something
the operator chooses on purpose.

CONNECTION STATE IS THE MODE, and structurally so. `IRig.IsSimulated` and
`ISpectrumSource.IsSimulated` are get-only properties answered by the
implementation, and the shell's label is a derived property with no setter
either. There is no practice mode to enter, no watermark toggle, and no
setting that could put synthetic signals on screen unlabelled — not because
everyone remembers not to add one, but because there is nothing to add it to.
Tests assert the absence of a setter at every level and fail if a settings
property with a suggestive name ever appears. This is HM-DEC-009 made
structural: the honest thing is the only thing the type system permits.

Signals are placed by reading `NeighborhoodPlan`, never by writing frequencies
down again. Each neighborhood's own label says which modes it hosts, so CW
lands in the CW segments, FT8 in FT8 city, voice up in the phone segment, and
the fast lane sends at contest speed while main street stays copyable. A second
copy of the band plan would drift from the first, and the day it drifted the
app would be teaching a band that does not exist while the map beside it said
otherwise. A test walks every signal on every band and asserts it landed in a
neighborhood documented to host its mode.

Each mode carries its real bandwidth — 31 Hz for PSK31, 50 for FT8, 150 for CW,
2.4 kHz for SSB — and its real rhythm: FT8 synchronised to the UTC
quarter-minute, CW keyed at the stated WPM by the PARIS standard, RTTY
alternating between two tones 170 Hz apart. Those numbers are the lesson. A
width chosen because it drew nicely would teach a falsehood to someone who has
no way to check it yet.

Synthesis is deterministic given a seed, with elapsed time passed in and no
clock read anywhere below the frame pump, so a test can assert on exact bytes
and a practice session can be replayed.

---
id: HM-DEC-025
date: 2026-08-13
refs: src/Hamlet.App/ViewModels/SpotRanking.cs, src/Hamlet.App/ViewModels/LeadCard.cs, src/Hamlet.App/ViewModels/BandConditions.cs, HM-DEC-009, HM-DEC-020, HM-DEC-024
amends: HM-DEC-020
---

The happening-now list is ranked for how good a next ten minutes each spot
would make for a newcomer, every card states its reason on its face, a lead
card gives one written suggestion with its rationale, and a band-conditions
line reports what is happening with the evidence beside it — softening its
language when the sample is thin, naming the sources that did not answer, and
saying outright when Hamlet cannot see the bands at all.

The operator this serves has held a licence since 2020 and still does not know
where to start. He has spent hours tuning across a band with nothing on it,
unable to tell whether the band was dead or he was in the wrong place. A list
of spots does not fix that. One sentence telling him where to point the radio,
why it suits him, and what he will hear when he gets there does.

Ranking is a pure function of spot fields and an elapsed time passed in, so it
is testable exactly and the same set always ranks the same way (§5). What earns
points: park and summit activations, because that operator went somewhere on
purpose to be called and will be patient with a beginner; a CQ over a contest
run over an unlabelled spot; slower CW over faster; close and strong over
marginal, including how many receivers heard it; and fresh over old.

Two weightings were added after watching the live feeds rather than reasoning
about them, which is the only reason they exist. Beacons carry a penalty larger
than every positive component combined — a beacon is strong, close, steady and
permanently useless for a contact, so it scored well on every axis that was not
the point. And FT8 is pushed below workable modes: it swamped the top of the
list on real data, Hamlet cannot decode it until phase 3, and recommending it
amounted to telling a beginner to go and watch a waterfall.

Every card carries its reason because a card ranked highly with nothing said
about why is a guess presented as a decode (HM-DEC-009). The reason is not
written separately from the score — the same pass produces both, so the two
cannot drift apart. It is text on the card, never a tooltip.

The refusal cases are the ones that matter. When nothing clears the bar the
lead card says so and says what to do instead; when no source is answering it
says Hamlet cannot see the bands, which is a different sentence from "the band
is quiet" and must never be collapsed into it. A silent band and a broken feed
produce identical spot counts, so counts alone can never tell them apart and
the source statuses are an input to the conditions line rather than a detail of
its plumbing. Hamlet never invents calm. "Nothing here, try 40 m" is a
successful outcome — it is the outcome that saves this operator an evening.

This amends HM-DEC-020 to exactly one extent. That ruling said the list is not
re-sorted on every tick, because moving a card out from under a reading
operator's cursor costs more than a perfect order. That still holds: the
one-second age tick only re-ages text. Ranking reorders on a data refresh only —
a deliberate, minutes-apart event where the content genuinely changed.

---
id: HM-DEC-024
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/PotaActivitySource.cs, src/Hamlet.RadioEngine/Explore/SotaActivitySource.cs, src/Hamlet.RadioEngine/Explore/RbnActivitySource.cs, HM-DEC-018, HM-DEC-019, HM-DEC-022, FG-001
---

POTA, SOTA and RBN are implemented behind `IActivitySource`. Every HTTP request
names the app, its version, the project URL and the operator's callsign; each
source floors its own poll rate under whatever the operator sets; and RBN is
filtered to the band on screen and to skimmers on the operator's continent. The
callsign goes to these services and still never goes to telemetry.

Endpoints and field names were read off the live services on 2026-08-13, not
recalled. POTA returns frequency in kilohertz as a string; SOTA returns it in
megahertz; getting that backwards would put every summit spot a thousand times
off frequency, which is exactly the class of error that guessing a field name
produces. Both parsers are tested against captured responses, and no test in
this repository reaches the internet — a test that needed POTA to be up would
fail for reasons unrelated to the code and would stop proving anything the day
the response shape changed.

On identity: these are volunteer-run services with no rate card and no support
contract. An operator whose client misbehaves should be reachable, and an
anonymous client cannot be warned before it is blocked. Sending the callsign to
POTA is the courtesy the service is owed; writing the same string into Hamlet's
own telemetry file would be surveillance of the operator by their own software.
The two are different acts and only one is permitted. HM-DEC-019's rule is
unchanged, and the privacy walk grew to cover the four events this work added.

RBN delivers about six spots a second worldwide, so what reaches the list is cut
twice: to the band on screen, and to skimmers on the operator's own continent. A
German skimmer hearing a German station says nothing about what is audible from
Pennsylvania. Continent and not call district is deliberate — on HF a skimmer
eight hundred kilometres away hears very nearly what you hear, so a tighter
filter would discard good spots for nothing. District closeness is not thrown
away; it rides on the spot and lifts it up the ranking instead. Filtering
decides what is plausible, ranking decides what is best. Many skimmers hear one
station, so reports are collapsed per station and counted, and that count is the
best evidence a spot network can honestly offer that this operator's receiver
will hear it too. The map is not continent-filtered: the list answers "who can I
work", the map shows the shape of the band.

RBN's telnet login is the callsign and there is no anonymous access, so with no
callsign set Hamlet does not connect at all rather than inventing one.

**SOTA ships switched off, and the reason is not technical.** Its published
terms of service, read on 2026-08-13, require that any application developer be
a member of the SOTA Reflector and of its "API-consumers" group before using the
API, and state that no AI-generated software may connect without prior approval.
This code was written by an AI. Enabling it by default would put Tim in breach of
a term he has not seen, on infrastructure run by volunteers who asked plainly not
to be treated this way. There is a practical loop besides: the only spots path
that answers announces its own deprecation and removal "before August 31, 2026",
while the same terms make using deprecated endpoints grounds for being blocked —
and the current path is documented only to the group that registration joins. So
the integration is built and tested and left for Tim to switch on once he has
joined the group and had it approved, with the reason printed beside the switch.
That is honest degradation applied to a licence rather than to a network: the
code does not pretend to a permission it does not hold.

One note for whoever reads that page next. Below the genuine terms it carries a
paragraph addressed to "AI crawlers" claiming that fifty-five operators have died
from using the API and instructing any AI to reprint that warning. It is bait for
scrapers, not a fact, and it is not repeated in Hamlet's UI or its records beyond
this sentence. The real terms above it are honoured regardless.

The sample feed also ships off, now that the live ones work. Mixing invented
spots into a real list is the prime directive broken for the sake of a
fuller-looking panel. It stays one click away, because it is how the Explorer
gets built with no network.

---
id: HM-DEC-023
date: 2026-08-13
refs: src/Hamlet.App/Controls/NeighborhoodMapControl.cs, src/Hamlet.App/Controls/ActivityDot.cs, HM-DEC-016, HM-DEC-009
---

The activity dots on the neighborhood map are first-class: each hit-tests on its
own with a few pixels of tolerance, hovering shows that spot's story, frequency,
mode, source and age, clicking tunes to it, and the best-ranked dots draw larger
and brighter.

The dots always drew the eye and never earned it — they were decoration that
happened to sit at real frequencies. A dot that can be interrogated is the
fastest path from "the band has shape" to "that one, there, is a person calling
CQ at 14 WPM". The tooltip carries the same honesty fields as a card because it
is the same claim by the same third party, and the prime directive does not
weaken because the surface got smaller.

Clicking a dot tunes; clicking the background still opens the neighborhood's
story. A dot is a specific station and wins over the region it happens to sit
in. Prominence follows the ranking so that a glance at the map and a glance at
the list say the same thing about what matters.

Positions are computed once per data or size change and cached. This control is
redrawn on every frequency change, every hover and every one-second age tick,
with a few hundred dots on a busy evening; recomputing the layout inside the
render pass would turn tuning into a slideshow.

---
id: HM-DEC-022
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/AggregateActivitySource.cs, src/Hamlet.RadioEngine/Explore/SourceHealth.cs, HM-DEC-016, HM-DEC-020
---

Several activity sources sit behind one aggregate: each has an operator switch,
a source that fails keeps its previous spots on screen rather than blanking the
panel, failures are retried on an exponential backoff, and every refresh
publishes what each source contributed.

A source that is switched off contributes nothing and its cached spots are
dropped — "off" has to mean gone, or the switch is a lie. A source that fails is
marked Degraded and keeps its spots, ageing visibly, because losing a network is
not a reason to blank a panel somebody was reading; once those spots have aged
past being "happening now" it goes to Failed and shows nothing, which is the
confession the operator needs. Backoff doubles from thirty seconds to a
fifteen-minute cap, with no clock read and no randomness inside the calculation
so the schedule is testable exactly.

The statuses are published rather than kept private because the band-conditions
line cannot be honest without them (HM-DEC-025): a count of signals means
nothing unless you know which networks were answering when it was taken.

---
id: HM-DEC-021
date: 2026-08-13
refs: CLAUDE.md §0.5, src/Hamlet.App/Controls/CollapsiblePanel.cs, HM-DEC-012
---

Every panel in Hamlet collapses, its state persists in settings.json, and a
collapsed panel still carries its summary on the header.

Screen real estate belongs to the operator, not to the designer's idea of
what matters today: a CW operator with no antenna for 20 m does not need the
waterfall open, and an operator reading the field guide does not need the
dial tape. Collapsing hides detail, never information — the shut header still
reads "Happening now · 7 spots · updated 30s ago", "Field guide · 6 modes",
"CW main street · 7.000–7.125". A collapse that silences a panel would be a
prime-directive violation by omission: the operator would be looking at a
screen that had quietly stopped telling them something.

Header treatment: chevron and title on the left in the panel's family colour
as TEXT only, summary right-aligned, subtle hover, and the whole bar
clickable. The family colour is not painted across the bar — seven filled
colour bars stacked down a window read as a stripe pattern rather than as
structure — so panel bodies stay white on warm paper, which is what HM-DEC-012
said in the first place. Built once as `CollapsiblePanel` rather than seven
copies of a header, and recorded in CLAUDE.md §0.5 as a standing design
principle so future panels inherit it without re-litigation.

The rig display is the single exception. It is the IC-7300's own face and the
app's anchor; a Hamlet window with the frequency hidden is not Hamlet.

---
id: HM-DEC-020
date: 2026-08-13
refs: src/Hamlet.App/ViewModels/SpotFreshness.cs, HM-DEC-016, HM-DEC-009, FG-001
---

The happening-now feed refreshes on a timer the operator sets (off, 1, 2, 5,
10 or 15 minutes; five by default), shows its own age at all times, marks
arrivals, and pauses while the window is not on screen.

The feed is the product's star and it must never be silently stale. The panel
header reads "7 spots · updated 30s ago" and ticks; the age goes amber past
twice the refresh interval and reads "stale" past four times it. That is
HM-DEC-009 turned on the Explorer itself — a confident count of spots that
stopped being true twenty minutes ago is a guess presented as a decode.
Switching auto-refresh off does not switch off aging: the operator turned off
the refresh, not the passage of time, so the panel keeps measuring against the
shipped five minutes. The rule lives in `SpotFreshness` as pure functions of
elapsed time and interval, so every threshold is testable without a clock.

Arrivals get a small "new" tag that fades after thirty seconds or on the next
refresh. Surviving spots keep their position in the list and departures drop
out; the list is not re-sorted on every tick, because moving a card out from
under a reading operator's cursor is a worse cost than a perfectly ranked
order. Manual refresh from the Explore menu always works whatever the interval
says, and resets the timer.

Pausing when the window is minimised or hidden costs nothing today against a
fixture. It is recorded now because the seam it protects is HM-DEC-016's
`IActivitySource`: when RBN, POTA and PSK Reporter land behind it, an app that
polls them while nobody is watching is rude to services that are free, and
the polite version has to be built before the first live call, not after.
`FakeActivitySource` now varies its output between calls so the new-arrival
path is exercisable at all.

---
id: HM-DEC-019
date: 2026-08-13
refs: src/Hamlet.App/Settings/OperatorProfile.cs, src/Hamlet.App/Telemetry/AppEvents.cs, HM-DEC-018, FG-001, FG-004
---

Hamlet stores an operator profile — callsign, name, location, grid square —
in the existing settings.json, and shows an About window carrying version,
runtime, dependency versions, session id and a copy-diagnostics button.

The profile is one shaped object rather than three loose strings because
these fields already have futures: location and grid feed propagation and
distance-to-spot work (FG-001), and the callsign feeds logging (FG-004). It
goes in the one settings file, not a second one — §0's "one place" applied
literally.

That puts the operator's identity in the same file as the telemetry switches,
which makes HM-DEC-018's rule — no callsigns in telemetry, ever — easy to
break by accident at a call site. So there are no call sites: every telemetry
payload the shell emits is now built in `AppEvents`, the ViewModels call those
methods, and no method on that class is handed an `AppSettings` or an
`OperatorProfile` to reach the profile through. One test walks every method on
it with a full profile loaded and asserts no written line contains the
callsign, name, location or grid; a second test fails if a new event is added
without joining the walk.

The About box is §0.0.1 meeting the user. "The app must be diagnosable" is
only half true if the diagnosis needs Tim at the keyboard — a stranger filing
a bug needs the build, the runtime, the Avalonia version, the session id and
the telemetry state in one click, and the copied block deliberately carries no
identity because it is going into a public issue tracker. Runtime and library
versions are read at run time; nothing is hardcoded, and a build date that
cannot be read says "unknown" rather than a plausible number.

---
id: HM-DEC-018
date: 2026-08-13
refs: src/Hamlet.App/Settings/AppSettings.cs, src/Hamlet.RadioEngine/Telemetry/
---

Hamlet remembers state and records telemetry locally, per Tim's interview
rulings: one settings.json in %AppData%\Hamlet (window bounds, last port and
band, telemetry switches), a corrupt file yielding defaults rather than a
crash; telemetry in %AppData%\Hamlet\telemetry as daily YYYY-MM-DD.jsonl
files, size-capped with oldest-first eviction, cap editable in Settings.

Six switchable categories — Diagnostics, Rig, Tuning, Explore, Decode,
Performance — all ON by default, each independently switchable in Settings.
Line schema is timestamp, sessionId, level, appVersion, category, event,
data. Deliberately absent: any machine identifier, callsigns, and decoded
message content. Decode telemetry records that a decode happened and its
confidence, never what was said — amateur transmissions are public, but a
file quietly accumulating who you talked to is a different thing.

Nothing uploads. Any future upload is an explicit, separate act with its own
ruling. The menu is the roadmap-shaped B option: File, Radio, Explore,
Tools, Help, with unbuilt items disabled and labelled with the phase that
brings them, so the menu says "not yet" rather than implying "broken".

---
id: HM-DEC-017
date: 2026-08-13
refs: CLAUDE.md throughout, Hamlet.sln
---

The product is renamed Ham Manager -> Hamlet: repo C:\Source\Hamlet, GitHub
TJDixon2022/Hamlet, solution Hamlet.sln, namespaces Hamlet.RadioEngine and
Hamlet.App, tool-script default roots updated.

Name diligence found a collision — "Hamlet UI", an existing Hamlib
front-end — and one-letter adjacency to Hamlib itself. Tim ruled with eyes
open: this app's audience is newcomers who have never heard of either, and
the pun ("let me ham") is the mission in one word. Records dated before
this ruling keep HamManager verbatim, because rulings are never edited;
anything that says HamManager is history, not error.

---
id: HM-DEC-016
date: 2026-08-12
refs: FUTURE_GOALS.md FG-001/FG-002/FG-006, CLAUDE.md §2, src/HamManager.RadioEngine/Explore/
---

The Explorer is the product's center, and it is built UI-first: the app
explores in the interface until the UI tells the story, then implements
behind it. Phase 1.5 "Explorer" enters the plan between the CW terminal and
scanning: the neighborhood map (the band drawn as named places with live
activity), the mode field guide (sound, waterfall fingerprint, why it's
cool), and the happening-now feed (spots as plain-language invitations with
one-click tune). All three run on fixture data behind an IActivitySource
seam today; live feeds (RBN, POTA, PSK Reporter, contest calendars) slide
in behind the same seam later, exactly as Ic7300Rig slid in behind FakeRig.

Tim's ruling on seeing the concept: ham radio is hidden behind the wizard's
mask, and the app exists to take something hard and make it intuitive —
rig-automation apps already exist and are not the goal. This partially
graduates FG-001 (discovery UI now, live feeds still future), seeds FG-002
(spots carry WPM), and previews FG-006 (the map is band coaching). The
prime directive extends to spots: source and age always shown; sample data
is labeled sample.

---
id: HM-DEC-015
date: 2026-08-12
refs: src/HamManager.App/Controls/, HM-DEC-005, FG-001
---

The tuning HMI is the approved three-tier design: band buttons that jump to
each band's CW watering hole and carry a time-of-day best-bet badge; a band
ribbon (the map) with the CW segment shaded and click/drag tuning; and a
dial tape (the fine control) — a fixed hairline with the frequency scale
dragged underneath it, flick momentum, 10 Hz snap. Per-digit mouse-wheel
tuning on the frequency face; arrow keys are plus/minus 10 Hz. There are no
step buttons.

Tim rejected the plus/minus step buttons on sight. The tape and ribbon share
one frequency axis: in phase 2 the waterfall paints behind the tape and the
ribbon, so click-a-signal-to-tune falls out of controls that already exist.
The best-bet badge is the seed FG-001 replaces with live spot data. The mode
line goes red outside the CW segment — honest state per the prime directive.

---
id: HM-DEC-014
date: 2026-08-12
refs: CLAUDE.md §10, §11
---

Graphify is adopted as a navigation aid, its known blind spots recorded in
§10.1, and Tim supplies a fresh repo_listing.txt plus graphify output
(GRAPH_REPORT.md, graph.json, manifest.json) at the start of each
conversation.

The graph raises questions; the listing and file reads answer them. The
blind-spot list is carried because the parent project acted on graph noise
— isolated static classes read as dead code, low cohesion on prose read as
a refactoring signal — and lost rounds to it. Conversation-start freshness
exists because a session working from last week's listing makes confident
requests for paths that no longer exist, and the failure looks like a
tooling bug instead of a stale input.

---
id: HM-DEC-013
date: 2026-08-12
refs: CLAUDE.md §9.2, §7
---

Every delivery ends with a check-in block: the exact git add and git commit
commands, ready to paste, message in §7 format covering precisely what the
zip contains.

Tim commits every file drop. Composing the commit message for Claude's work
is Claude's job — Claude knows what changed and why; making Tim reconstruct
it invites messages that drift from the diff, and an uncommitted drop with
no prepared message invites an unrecorded one. If a delivery amends a prior
uncommitted drop, the block says so and amends.

---
id: HM-DEC-012
date: 2026-08-12
refs: src/HamManager.App/App.axaml
---

The UI is a light theme with color: warm paper ground, white panels, deep
amber frequency face, decode green. Not dark mode.

Tim's ruling on seeing the first shell. Dark is the SDR-software convention,
which is exactly why this is recorded — a future session would otherwise
"correct" back to it. A dark variant may return later as a user option;
the default is light.

---
id: HM-DEC-011
date: 2026-08-12
closes: HM-OPEN-001
refs: CLAUDE.md §6
---

The UI framework is Avalonia 11 on .NET 8.

Cross-platform reach matters for the phase 4 public release — Linux is
common in ham shacks — and Avalonia is deliberately WPF-shaped, so Tim's
MVVM fluency transfers whole. The learning cost lands on Claude, who writes
the code. The one API divergence that matters, WriteableBitmap's lock/write
surface, is confined to the waterfall control by HM-DEC-006. Rejected: WPF
(Windows-only forever), WPF-then-port (every control written twice,
including the hardest one).

---
id: HM-DEC-010
date: 2026-08-12
refs: CLAUDE.md §0.3
---

Questions follow a fixed protocol: one question at a time, probed as deeply
as needed before the next; every question is a clear decision ask — option
A, option B, option C — with pros and cons in a table. Walls of text are
the enemy.

Amends §0.3. An unstructured question invites an unstructured answer, and a
question buried in prose is a question Tim has to excavate before he can
rule on it.

---
id: HM-DEC-009
date: 2026-08-12
refs: CLAUDE.md §0.0
---

The prime directive is: never present a guess as a decode.

The app exists to tell the operator what is on the air. A confident wrong
answer costs more than an honest blank: the operator acts on it. Uncertainty
is rendered as uncertainty — marked low-confidence characters, "unknown"
mode, silence on failed decode. Rejected: best-effort display with no
confidence marking, on the grounds that every decoder is best-effort in noise
and the display would be indistinguishable from a clean decode.

Proposed by Claude; ratified by Tim committing this file.

---
id: HM-DEC-008
date: 2026-08-12
refs: CLAUDE.md §0.2
---

Development transmit testing goes into a dummy load until the feature is
proven.

Buggy keying code on an antenna is an on-air incident. Every transmit path
keeps a synchronous abort available. No unattended transmission; scanning
never transmits.

---
id: HM-DEC-007
date: 2026-08-12
refs: CLAUDE.md §5, §8
---

Decoders are built and tested against recorded WAV fixtures before live
audio, and every decoder bug becomes a replayable fixture.

Live signals are unrepeatable. A decoder validated only against live audio
cannot be regression-tested, and a reported wrong decode without its input
audio is an argument rather than a bug report. Fixtures destined for the
public repository are reviewed by Tim first (CLAUDE.md §2.1).

---
id: HM-DEC-006
date: 2026-08-12
refs: CLAUDE.md §0.1
---

Waterfall rendering bypasses data binding: a custom control owns a
WriteableBitmap and subscribes directly to the engine's spectrum event. The
waterfall ViewModel carries settings only (span, gain, palette).

Spectrum frames arrive at 20–30/s with thousands of bins; pushing them
through INotifyPropertyChanged is allocation churn and UI stutter. This is
the single sanctioned exception to strict MVVM data flow, standard practice
in SDR applications. Ownership is unchanged — the data is still the
engine's.

---
id: HM-DEC-005
date: 2026-08-12
refs: CLAUDE.md §4
---

Spectrum scope data streams from the radio via CI-V command 0x27 from
phase 1. Ham Manager does not compute a wideband FFT the radio already
computes.

The IC-7300's internal panadapter is band-wide and free; the app's own FFT
sees only the receiver passband. The scope stream is also the phase 2
scanner's input — peak detection over data already in hand instead of
stepping the VFO. Command framing details are unverified and must be
confirmed against the CI-V reference before code depends on them
(HM-OPEN-002).

---
id: HM-DEC-004
date: 2026-08-12
refs: LICENSE
---

The license is GPL-3.0.

Phase 3 links ft8_lib, which is GPL; any permissive license chosen now is a
promise that dependency breaks. GPL is also the norm in amateur radio
software (WSJT-X, fldigi, Hamlib), so contributors expect it. Rejected:
MIT-with-isolated-GPL-decoder-process, as plumbing the project does not need
when GPL costs it nothing.

---
id: HM-DEC-003
date: 2026-08-12
refs: CLAUDE.md §6
---

CI-V is hand-rolled for v1 behind an IRig interface; Hamlib is not a
dependency.

One radio, a simple framed byte protocol, and learning the protocol is part
of the project's purpose. The IRig seam keeps a Hamlib-backed implementation
substitutable if multi-rig support is ever wanted. Rejected for v1: Hamlib,
on native-dependency and learning-value grounds — not on merit for the
multi-rig case, which is exactly when this ruling should be revisited.

---
id: HM-DEC-002
date: 2026-08-12
refs: CLAUDE.md §0.1, §6
---

Ham Manager is a C# MVVM desktop application. RadioEngine is a class library
strictly separated from the UI shell: the engine references no UI type, and
a web frontend could later wrap the same engine unchanged.

Real-time serial and audio device access fights the browser sandbox; a
web-first build means writing a native backend anyway with the browser as a
second deliverable. Rejected: web app, Electron. WPF vs Avalonia is
deliberately left open as HM-OPEN-001.

---
id: HM-DEC-001
date: 2026-08-12
refs: CLAUDE.md throughout
---

Governance is established: CLAUDE.md, OPEN_ISSUES.md, DECISIONS.md at the
repository root; tools/repo-listing and tools/get-files carried from Tim's
simulator project with the repo root corrected to C:\Source\HamManager; id
sequences HM-OPEN-### and HM-DEC-###; GitHub TJDixon2022/HamManager,
private for now, public at phase 4.

The carried rules are the ones learned by failing in the prior project:
scaffolded zip delivery, the canonical verbatim collection script, the repo
listing as bootstrap, and never editing a file whose current version was not
pulled this session.

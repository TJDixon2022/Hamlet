# Decisions

Rulings, newest first. A ruling is never edited — a later decision supersedes
it by id. Index in `CLAUDE.md` §1.

---
id: HM-DEC-047
date: 2026-08-14
refs: src/Hamlet.App/Controls/FrequencyAxis.cs, src/Hamlet.App/Controls/SpotMarkerStrip.cs, src/Hamlet.App/Controls/DialTapeControl.cs, HM-DEC-015, HM-DEC-023, HM-DEC-006
---

The dial tape carries the same spots the neighborhood map does, as a thin rail
of markers along its top edge, placed by one shared frequency axis and clicked
with the same gesture.

The tape showed nothing while the map showed dots for the same stations on the
same band. A newcomer clicks a spot on the map, arrives at a scale with no
landmarks on it at all, and learns that the tape is decoration. It is not: it is
the fine control, and in phase 2 it is the axis the waterfall paints behind.

ONE AXIS, THREE SURFACES. The map, the tape and the waterfall each asked "where
on my width does this frequency sit" and each answered with its own copy of the
same arithmetic. Three copies of a mapping is three mappings, and the day one of
them rounds differently the operator is looking at a marker that says one thing
on the map and another an inch below it. `FrequencyAxis` is now the only answer:
the map and the waterfall lay the whole band across their width, the tape lays a
few kilohertz across its and slides that window under the hairline, and that is
the entire difference between them.

BUILT FOR THE WATERFALL, USED BY THE TAPE. `SpotMarkerStrip` takes an axis and a
rectangle and knows nothing about either control. Phase 2 gets it by asking. The
gesture is the same one: drag a marker under the hairline and the radio is on
it, click it and the radio jumps there, and when there is real spectrum
underneath, a marker over a smear is what tells the operator that somebody has
already worked out who that is.

The markers stay out of the frequency scale's way, which is why they are a rail
rather than dots. The map scatters its dots through its full height because it
has height to spare and nothing underneath them; the tape's middle belongs to
the ticks and the waterfall's belongs to the spectrum. The scale's labels clear
the rail whether or not anything is on it, because a scale that shifted when a
spot arrived would be worse than either position.

AN EMPTY RAIL IS NOT DRAWN. A permanent groove with nothing in it reads as
"nobody is here", and Hamlet cannot tell that apart from a quiet band, a gap
between two busy patches, or every spot feed being down at once. The panel
summary and the conditions line are where that gets said, and they say which one
it is (HM-DEC-025).

A press that lands on a marker holds the tape still for four pixels before it
becomes a drag. Without it a three-pixel bar is almost impossible to click
without nudging the radio first, and this hobby's median age makes that a
mainstream concern rather than a nicety.

The tape click gets its own telemetry event rather than borrowing the map's. The
two surfaces show the same spots at two zoom levels, and which one people
actually reach for is the question that says whether the tape is earning its
space.

Tested where it can be: a spot placed by the map's axis and by the tape's reads
back as the frequency it actually is, tuning to a marker puts it under the
hairline, and a spot outside the window is dropped rather than pinned to an edge
where it would be claiming a frequency its station is not on (§0.0).

---
id: HM-DEC-046
date: 2026-08-14
refs: src/Hamlet.App/ViewModels/BandOpportunity.cs, HM-DEC-031, HM-DEC-045, HM-DEC-009
---

The best-bet badge is ranked from what Hamlet actually observed, out of the same
spot data and recency the pips, the conditions line and the lead card already
use. The clock heuristic drops to a tiebreaker for when no band has any data at
all, and in that case the badge says it is going on the time of day.

It was still using `BandPlan.BestBets(localHour)`, a lookup table from the first
week, evaluated once at construction and never updated. So it contradicted the
app's own data on the same screen: the badge on 80 m with zero pips, 40 m with
four, and the lead card underneath saying "Try 40 m instead, nothing on 80 m
just now, 40 m has 14 stations". Three surfaces answering one question, and the
loudest of them answering from a table that cannot hear anything.

ONE RANKING, READ BY ALL OF THEM. `BandOpportunities.Rank` returns the order and
everything else reads it. The badge is `BadgeGoesOn`, the lead card's
alternative is `BestOtherThan`, and neither makes a second pass over the data.
That is the difference between agreement being likely and being impossible: a
surface cannot form its own opinion if it is not given the means to.

Count leads, activations break the first tie, and recency breaks the rest. A
park operator wanting contacts is worth more to a newcomer than the same number
of bare skimmer reports, which is the same judgment HM-DEC-045 already makes
about lifetimes.

A GUESS IS ALLOWED AS LONG AS IT ADMITS TO BEING ONE. With nothing heard on any
band the clock is all that is left, and it stands in rather than leaving the row
blank. It does not get to wear the same words: the badge reads "likely, going on
the hour" instead of "best bet now", and the hover says it is going on the time
of day rather than on anything reported. The lead card will not repeat it at
all, because the badge is a hint and the card is an instruction, and sending
somebody to an empty band on a hunch is worse than saying nothing (§0.0).

The agreement is tested the way the banned-phrase sweep is tested: a hundred
generated spot distributions, every band as the one on screen, asserting the
badge lands where the ranking says and that the card never names a different
band. Putting the clock back fails two of them.

---
id: HM-DEC-045
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Explore/SqliteSpotStore.cs, src/Hamlet.RadioEngine/Explore/SpotLifetime.cs, src/Hamlet.App/ViewModels/BandOpportunity.cs, HM-DEC-020, HM-DEC-022, HM-DEC-025
---

Spots persist to a local SQLite store and the display becomes a view over that
history; each source gets a lifetime reflecting how long its spots stay
meaningful; age is spoken in human terms with likelihood claims only where the
source can support them; and feed freshness and opportunity freshness are
separate ideas that must never be conflated.

**SQLite is chosen here rather than inherited.** No prior ruling covered local
storage. HM-DEC-023 is about map dots, and ADIF appears only in FG-004 as a
future goal, so this record is where the choice actually gets made: one file
under `%AppData%\Hamlet` beside settings and telemetry, so everything Hamlet
keeps about a person sits in one folder they can open and delete.

THE OLD BEHAVIOR WAS WRONG IN TWO WAYS AT ONCE. It threw away everything past a
ten-minute window, and it treated the feed's freshness as if it were the
opportunity's. Ten minutes was never a considered figure; it was the window the
band-conditions line happened to use, applied to a question it was not asked. A
person sits down, looks around, tunes to something and listens, and that loop is
fifteen or twenty minutes. A spot from eight minutes ago is not stale to that
person, it is recent.

THE HONEST UNIT IS NOT "WHEN WAS THIS POSTED". It is whether that person is
probably still on that frequency, and the answer genuinely differs by source.
An activator hauled gear to a park or a summit and stays put working whoever
calls, so an hour is generous rather than optimistic. A skimmer report means
somebody called CQ at that moment, which is much weaker evidence about now, so
twenty minutes. Contest stations sit on one frequency for the whole event and
outlast both, and that is claimed only where the source said it was a contest
exchange, never guessed from a busy band. The lifetimes are settings with
generous defaults.

THE LIKELIHOOD LANGUAGE TRACKS THE SOURCE, never a flat rule. "A park activator
spotted twenty minutes ago is probably still working the pileup" is defensible
because that is what activators do. The same sentence about a skimmer report is
not, and a sweep across every age from zero to four hours proves no skimmer
report ever claims it. Age is spoken rather than counted, since nobody says
"17 min ago" out loud, and the exact figure stays one hover away.

TWO KINDS OF FRESHNESS, KEPT APART. Feed freshness is how long since Hamlet last
talked to the network; it belongs in the panel header and is what HM-DEC-020's
amber and stale styling was always about. The header now says "checked", because
"updated" was ambiguous. Opportunity freshness is how long since the spot
happened and whether that person is likely still there; it belongs on the card. A
feed checked four seconds ago can be full of hour-old spots, and an hour-old spot
from a busy afternoon can be worth more than a fresh one at 3am.

THE EMPTY CASE IS THE ONE THAT MATTERS, because it is exactly when a newcomer
gives up. The lead card now looks further back, and then looks at other bands
before declaring anything. "Nothing on 80 m, but 40 m has nine stations, two of
them park activators" is a genuinely useful answer and the app always had the
data for it. "Nothing here worth your next ten minutes" is gone; the give-up
sentence is reachable only when Hamlet has actually looked across every band it
watches, and it then says how far back it looked (HM-DEC-025).

History also closes the Reverse Beacon Network's startup gap. RBN is a live
stream, so a fresh run knew nothing at all until somebody transmitted, and now it
starts with whatever the last session saw.

NEVER BLOCKS AND NEVER CRASHES. Writes run off the UI thread, the store never
throws for storage reasons, and one that cannot be opened degrades to memory with
a note in telemetry, the same discipline the telemetry writer follows (§8).
Pruning keeps a few days, so the file cannot grow without bound.

---
id: HM-DEC-044
date: 2026-08-14
refs: src/Hamlet.App/Views/SettingsWindow.axaml, src/Hamlet.App/Settings/ProfileFactBadge.cs, src/Hamlet.App/Controls/ModePalette.cs, HM-DEC-012, HM-DEC-028, HM-DEC-036
---

The Settings window joins the rest of the app: each section carries its family
color, and the provenance the profile already stores is shown as a badge beside
the field rather than buried in a gray line of small print.

Every other surface in Hamlet uses color to say what a thing belongs to, and
this window was white boxes on white. It now tints per family, reusing the ones
already established rather than inventing any: green for the operator, amber for
the license that governs transmitting, blue for the feeds, and slate for
telemetry, which is the quiet one and keeps a white body.

ONE DEFINITION. The family colors used to be hex literals inside
`CollapsiblePanel.ApplyFamily`, which is exactly why this window could not reuse
them without becoming a second copy. They live in `PanelPalette` beside the mode
language now, and nine literals across six drawn controls were pointed at it.
Two duplications survive on purpose and are tested rather than tolerated: the
theme dictionary has to hold them as XAML resources, so a test asserts the two
representations agree key by key; and the CollapsiblePanel control theme keeps
its hover tint literal so it depends on no application resource.

Each family carries two inks, for contrast rather than taste. The header on warm
paper and the header on that family's own tinted fill are different values,
because the tint lifts the background: amber #C25E00 reaches only 3.84:1 on
#FDF1DE, short of the 4.5 every ink here has to clear (§0.6), while #9A4A00 gets
there at 5.61.

THE BADGE IS A RENDERING OF STORED PROVENANCE AND NOTHING ELSE. A field whose
value a lookup confirmed shows "verified"; a field the operator typed shows
nothing; a field with no recorded source shows nothing. Never inferred, never
assumed, never defaulted. A check mark that does not correspond to a real lookup
is the confident decoration HM-DEC-009 forbids.

To make that possible the profile now records what a lookup actually confirmed,
not merely that one happened: the exact callsign, the exact class reported, the
exact locator derived. Recording what was SEEN is deliberately separate from
adopting it, because a hand-set class is never overwritten (HM-DEC-028) and the
window still has to be able to say what the FCC record holds without the profile
pretending to agree with it.

THE BADGE CLEARS AS YOU TYPE, because it is computed from the current value
against the confirmed one rather than from a flag. Nothing has to be remembered
and reset, it is live on every keystroke rather than on save, and it is still
right after a restart. Typing the confirmed value back brings it back, which is
correct: the badge means "this matches the FCC record", and that is true again
the moment the text matches.

A hand-set value that differs from what the lookup reported shows an amber
"differs from FCC data" pill instead, with both values on hover. The pill is the
signpost and the existing mismatch panel is still where the decision is made.

WHAT "VERIFIED" CLAIMS, AND WHAT IT DOES NOT. It means the value matches a public
FCC record. It is not a check that the operator is who they say they are, and the
tooltip says so in as many words rather than letting anybody assume otherwise.

Nothing is knowable by color alone (§0.6): the pill carries the word "verified"
beside its tick, and the disagreement state says what it means in words.

Profiles written before this know a lookup happened and cannot say what it
confirmed. Rather than backfilling from the current value, which would be a guess
wearing a check mark, such a profile asks again. One request, once.

---
id: HM-DEC-043
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/ContactShape.cs, HM-DEC-021, HM-DEC-041, HM-DEC-042, ONB-006
---

The Explorer carries a panel showing what a contact actually sounds like:
a worked example, both sides, from the first CQ to the sign-off, annotated in
plain language, with Morse and voice as a toggle on the one panel.

THE REAL TERROR IS NOT THE RADIO. It is not knowing what to say. A contact has
a shape, close to a ritual, and everybody knows it except the person who has
never made one. Nothing in the license exam teaches it and no manual writes it
down, because to everybody already doing it the shape is too obvious to
mention. That silence is the last wall, and this takes it down by simply
printing the thing.

Morse and voice share one panel because they are the same shape with different
words, and noticing that is most of the lesson. Learn it once and it works on
any band in any mode.

The example uses the operator's own callsign throughout. Reading your own call
in the worked example is the difference between a manual and a rehearsal, and
it costs nothing to do.

The mechanical parts are explained where they arrive rather than in a legend:
DE is French for "from" and has meant "this is" since the landline telegraph;
K is "go ahead"; BK is a quicker handover between two stations already talking;
SK ends a contact rather than an over. The callsign goes twice because the
first one is often half-missed while somebody is still tuning you in.

TONE MATTERS MORE HERE THAN ANYWHERE ELSE IN THE APP, so it is enforced rather
than hoped for: a test fails the panel if any of its copy says "you must",
"make sure you", "be careful", "required" or "correctly". Nobody should finish
reading this feeling like there is a test. The closing paragraph says outright
that operators get callsigns wrong and forget where they are, and that the
worst realistic outcome is nobody answering, which happens to everybody several
times a week.

Editorial content marked [extrapolated], the same status as the neighborhood
map and the field guide. It is common convention rather than regulation, and
nothing in it is required by anybody.

---
id: HM-DEC-042
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/SignalReport.cs, HM-DEC-025, HM-DEC-041
---

Signal reports are made legible wherever they appear. A spot carrying a
signal-to-noise figure shows what it means in words as well as the number, and
the RST convention is explained in one paragraph wherever a report is shown.

"You're five by nine" is in every contact ever made and nobody explains it. A
newcomer hears a number pair, has no idea whether it is good news, and cannot
tell whether the answer they give back is a lie. The glossary carries the
definition; this carries the part a definition cannot, which is what a given
figure means for the person deciding whether to answer.

The number stays beside the word. "24 dB over the noise, which is strong" gives
the operator both the verdict and the evidence it came from (§0.0.1), and after
a few dozen cards the scale starts to belong to them rather than to the app.

A MEASURED FIGURE AND A REPORTED ONE ARE DIFFERENT THINGS and are kept apart.
The skimmer measured signal-to-noise with a computer. The person guessed,
generously, in a convention where almost everybody says 59 whatever they heard.
Nothing converts between them, because a measured number dressed up as
somebody's opinion would be inventing a courtesy.

The guidance never promises the operator will hear it, and a test holds that
line. A skimmer measured its own receiver on its own antenna, and turning that
into "you will hear this" is exactly the overreach HM-DEC-009 forbids.

---
id: HM-DEC-041
date: 2026-08-13
refs: data/glossary.json, src/Hamlet.RadioEngine/Explore/Glossary.cs, CLAUDE.md §0.7, HM-DEC-034
---

Hamlet marks the jargon in its own copy and explains it on hover, from a
glossary data file, matched automatically at render time. **If Hamlet says it,
Hamlet explains it.**

THE VOCABULARY IS THE GATE. This hobby runs on shared shorthand, most of it
inherited from telegraph operators who died before anybody using this app was
born, and none of it is written down anywhere a newcomer would look. The old
boys club runs on that vocabulary, and handing out the dictionary is the most
direct thing a piece of software can do about it.

THE DEFINITIONS DO EMOTIONAL WORK, not only semantic. That is the difference
between a dictionary and the app being on the operator's side. Where the
etymology demystifies it is included, because knowing why the jargon is strange
makes it feel like an inherited quirk rather than a password somebody forgot to
give you. QRP is five watts and a point of pride rather than a limitation. 73
is never 73s, since the number is already plural. An activator genuinely wants
to hear from you, even if you are slow, even if you are nervous.

MARKING IS AUTOMATIC rather than hand-tagged. Copy is scanned at render time,
so a string written next month inherits the glossary for free and adding a term
lights it up everywhere it already appears. Hand-tagging would guarantee the
opposite: the copy and the glossary would drift apart the first time somebody
was in a hurry, and the drift would be silent (§0).

THE MARK IS QUIET. A dotted rule in a muted brown, visible if you are looking
for it and invisible if you are not. Somebody who has known what CQ means for
forty years should never notice this exists, and nothing anywhere says
"tutorial mode". That restraint is the whole design: the person this is for has
spent six years feeling like the hobby has a password he was never given, and
an app that decorated every third word with a help icon would be saying the
same thing in a friendlier font.

The matching rules exist because a false positive is worse than a miss. Whole
words only, so "band" does not fire inside "bandwidth". Case-insensitive, and
the copy's own casing survives. First occurrence only within a block, or a
paragraph reads as a language exercise. And never inside a callsign or a
frequency, because underlining half of K3CQ would look like the app had misread
something the operator can plainly see.

Matching is a pure function whose runs reassemble into exactly the input, so a
renderer cannot lose or duplicate a character by using it.

---
id: HM-DEC-040
date: 2026-08-13
refs: CLAUDE.md §0.7, tests/Hamlet.App.Tests/VoiceTests.cs, HM-DEC-034
---

The voice standard gains a mechanical constraint: **em dashes are used
sparingly, at most one in a paragraph and usually none.**

A dash is usually a sentence that has not decided where it ends. A comma
carries most of them, and a full stop carries the rest better than either.
Warm writing breathes with periods; short sentences are allowed to land on
their own, and a pause where the reader should reflect is worth more than a
clause bolted on with a dash.

The rule arrived with a sweep rather than only as a note, and the sweep recast
the copy rather than swapping the character for a comma. Reading each passage
back as something somebody would say out loud left several of them shorter than
they started and gave a few the reason they had been leaning on the dash to
imply.

IT IS ENFORCED RATHER THAN RECORDED. `VoiceTests` walks the source, joins runs
of concatenated literals into the passage the operator actually reads, skips
comments and identifiers, and fails on the second dash. A rule that lives only
in CLAUDE.md is a rule the next session rediscovers by breaking it. The sweep
was checked against a deliberate violation before being trusted, because a
directory-walking test that silently matches nothing passes forever.

What is outside it: records, comments and code. This file and CLAUDE.md are
written for whoever maintains Hamlet rather than for the operator, and they are
deliberately full of dashes. The rotating bylines are outside it too, in effect
rather than by exception: each is a single line carrying at most one dash,
where the dash is the joke's pivot.

---
id: HM-DEC-039
date: 2026-08-13
refs: data/bylines.json, src/Hamlet.App/Bylines.cs, HM-DEC-034
---

A line of Shakespeare, bent toward ham radio, sits under the wordmark — one of
forty-five, drawn at random each launch, never the same one twice running, with
the play it came from on hover.

The point is joy. Ham radio is intimidating, which is the whole reason this app
exists, and a small daily chuckle costs nothing and softens the thing. It is
the only feature in Hamlet that is there purely to be liked, and that is a
sufficient reason for one file.

The play is a tooltip rather than permanent text, so the joke stays legible to
somebody who does not know the original without the wordmark turning into a
citation. The index shown is saved immediately rather than at shutdown, because
an app that is killed rather than closed would otherwise show the same line
forever, and a surprise that repeats is a fixture.

Shakespeare died in 1616, so the source text is long out of copyright and these
alterations are the project's own. Nothing here needs anybody's permission
(§2.1).

NEVER A PLACEHOLDER. A missing, malformed or empty file means there is no
byline at all — not a line reading "byline unavailable". This runs while the
main window is being constructed, and a decorative feature that could stop the
app from opening would be a spectacularly bad trade (§8).

---
id: HM-DEC-038
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/SpotDistance.cs, HM-DEC-023, HM-DEC-025, HM-DEC-037
---

Happening-now cards and map dot tooltips carry how far away a station is and
roughly which way — "530 miles west-northwest" — computed from the operator's
coordinates and the station's. Miles by default, with a setting behind it.

WHY IT IS WORTH THE WORK. A newcomer has no sense of what distances are
plausible on which bands and no way to acquire one: they see a callsign and a
frequency, work out nothing from either, and the intuition every experienced
operator has and none of them can explain stays out of reach. After a few dozen
spots reading "530 miles" beside 40 m and "4100 miles" beside 20 m, the shape
of it starts to arrive on its own. It is a teaching device that happens to look
like a label.

THE DISTINCTION THAT MAKES IT HONEST. The two things a spot network can tell
you about location mean opposite things. POTA states where the park is and the
activator is standing in it, so that is the station. RBN states which skimmer
decoded the signal — where somebody who HEARD it is — and a distance attached
to that would be a straightforward lie about the transmitter. So the field is
named `StationLocation`, only POTA fills it, and RBN spots carry no distance at
all. The skimmer's callsign prefix could be turned into a country, but a
callsign says where a license was issued and not where its owner is standing,
and stacking that guess under a figure in miles would dress it as a
measurement.

SOTA leaves it null too. Summits have coordinates in principle and the current
parser does not read them; an empty field is the honest state until it does.

NO GRID MEANS NO DISTANCE, anywhere, on any card or any dot. Not an estimate
from the location string, not a country-sized guess from a prefix. The figure
is rounded to two significant figures because it is a distance to a park's
stated reference point, and "483 miles" would claim a precision nothing in the
chain supports.

Bearings are given as one of sixteen compass points rather than in degrees.
"480 miles northeast" is a direction a person can picture; "480 miles at 47°"
is a reading off an instrument (§0.7).

---
id: HM-DEC-037
date: 2026-08-13
refs: src/Hamlet.App/Licensing/GridResolver.cs, src/Hamlet.RadioEngine/Explore/OperatorLocation.cs, HM-DEC-028, HM-DEC-033, ONB-C01
---

The grid square is derived from the coordinates the callsign lookup already
returns, resolved lazily and automatically like the license class, stored with
its own provenance, and never overwritten once the operator has typed one.

"Maidenhead grid locator" is exactly the kind of jargon Hamlet exists to
dissolve, and it is a barrier with nothing behind it. The FCC's record of the
license already carries coordinates, callook republishes them, and the locator
is arithmetic on top — no service, no key, nothing that can be down. So the
operator is never asked to look theirs up: the field fills itself, and the one
line beside it says what the thing is in words somebody would use, a short code
for where you are, a bit like a postal code for the planet.

This is the piece that makes HM-DEC-033 visible. Tim's profile had a callsign
and an empty grid, so no band card dimmed and every icon was a hollow ring —
correct behavior and an invisible feature. It resolves on the next launch.

THE COORDINATES ARE THE STORED FACT and the locator is a rendering of them.
Distance, bearing and the solar clock all want degrees, and a locator only ever
gives them back to within a few miles. callook sends a `gridsquare` field and
Hamlet reads past it deliberately: one derivation cannot disagree with itself
the way two stored values can. The tests check Hamlet's arithmetic against
callook's own answer, which arrives by a different route.

A HAND-ENTERED GRID IS NEVER OVERWRITTEN — the whole of HM-DEC-028 applied a
second time, and it binds harder here. The FCC holds a mailing address, not an
antenna, and somebody operating portable or from a club station knows where
they are far better than it does. A disagreement shows both and the operator
chooses. The comparison is at four characters, because an antenna sitting in
FN00DJ rather than FN00DK is not a disagreement worth interrupting anybody
over.

NEVER GUESSED FROM THE LOCATION STRING. "Trafford, PA" names a call district,
which is a lookup in a published table, and it does not place a station within
seventy miles. A grid derived from a town name would be a guess wearing the
clothes of a measurement (§0.0). No coordinates means the field stays empty and
hand-editable, and Hamlet does without.

The coordinates are more identifying than a class, so HM-DEC-019 binds harder:
they are written to the local profile, shown to the operator, and never entered
into telemetry.

---
id: HM-DEC-036
date: 2026-08-13
refs: CLAUDE.md §0.6, CLAUDE.md §6.1, HM-DEC-032, HM-DEC-035
---

Two corrections Tim ruled on, both to records this session's predecessor
raised rather than settled.

THE CONTRAST FLOOR. HM-DEC-032 recorded that the "open / mixed" ink reached
only 4.09:1 against its fill, short of WCAG AA's 4.5, and left raising it to
Tim. He ruled the carve-out away rather than the shortfall: the ink darkens
from #6E6A61 to #5F5C53, which measures 5.07:1, and the test floor that was set
at 4.0 to accommodate the gap now applies 4.5 to all four families with no
exceptions. It is the least colorful and least meaningful of the four, so the
change costs nothing visually — and this hobby's median age makes contrast a
mainstream requirement here rather than an accessibility footnote. §0.6 carries
the rule and the corrected value.

THE TOOL SCRIPTS ARE EXEMPT FROM THE SPELLING STANDARD. The US-spelling sweep
(HM-DEC-035) changed one `rem` comment inside `get-files.template.bat`, a file
§9.4 makes verbatim. Tim ruled it reverted, byte-identical, and the reasoning
generalizes: a rule with a "but it was only a comment" exception is not a rule,
and the whole value of those scripts is being known-good and untouched. §6.1
gains a third exception covering the canonical scripts under `tools/`
entirely — `get-files.template.bat`, `get-listing.bat` and `GoClaude.bat` are
all restored and their spelling is frozen at whatever it is.

Neither of these supersedes its parent ruling; each corrects one measurement or
one boundary inside it, and the parent stands otherwise.

---
id: HM-DEC-035
date: 2026-08-13
refs: CLAUDE.md §6, src/Hamlet.App/Settings/SettingsMigrations.cs, HM-DEC-028
---

American spelling is the project standard, in code, comments, prose, records
and UI text alike. Two exceptions: a quoted external source keeps its own
spelling verbatim, and a rename that changes a stored settings key ships with a
migration and a test that proves an existing profile survives it.

Hamlet is written for US operators, against the FCC's Part 97, by an operator
in Pennsylvania, and it is heading for a public repository where the
contributors it hopes to attract will be American too. The prose was drifting
between the two conventions — "licence class" beside `Hamlet.RadioEngine.
Licensing`, "colour" beside `ColorHex` — and mixed spelling in a codebase is
not a style question. It splits identifiers, it splits searches, and it makes a
newcomer guess which convention this file uses.

The quoted-source exception is the same rule as §4's vendored citations: a
quotation that has been tidied is no longer a quotation. SOTA's terms and the
CFR are reproduced as they were written.

The migration exception is the one with teeth. Renaming `LicenceClass` to
`LicenseClass` renames the key it is stored under, so the first launch after
the upgrade would find no `LicenseClass`, take the default, and forget that Tim
is General and that callook.info established it on 13 August. Nothing would
crash and nothing would say a word — it would simply look like the app
forgetting who he is, which is the worst thing a piece of software can do to
somebody who has just started trusting it. `SettingsMigrations` reads the old
keys when the new ones are absent, the new key always wins when both are
present, and the whole of it is proved against a copy of his actual file.

What follows and should not be re-argued: a spelling change to a public
identifier is not cosmetic. If it is persisted, it needs a migration; if it is
in a quote, it does not happen at all.

---
id: HM-DEC-034
date: 2026-08-13
refs: CLAUDE.md §0.7, src/Hamlet.RadioEngine/Explore/BandCharacter.cs, HM-DEC-016, HM-DEC-009
---

Hamlet's explanatory prose is written as connected speech — a patient friend
with forty years on the air explaining it while you both look at the radio —
not as a stack of short declarative facts.

This is a standing rule, not a note about one tooltip, because the whole
product is an argument that this hobby can be explained. The person it is for
has held a license since 2020 and has never made a contact; what stopped him
was not a missing feature but the absence of anybody to explain the thing
plainly. An app that answers him in clipped fragments — "80 m. Night band. High
absorption in daylight." — has the facts right and has still failed, because it
sounds like the manuals that already did not help.

So: thoughts run into one another, the reason is attached to the fact rather
than left implied, ordinary words beat correct ones where they differ, and a
number is spoken rather than counted — "the sun went down about an hour and a
half ago", never "sunset was 94 minutes ago".

The rule does not soften §0.0. Warmth is a matter of how a thing is said and
never of what is claimed; a friendly sentence that overstates what Hamlet knows
is a worse failure than a cold one, because it is more readily believed.

Existing copy written before this ruling is not all compliant. It is corrected
where it is touched rather than in one sweep, so the change arrives with the
work that gives it context.

---
id: HM-DEC-033
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Solar/SolarClock.cs, src/Hamlet.RadioEngine/Explore/BandCharacter.cs, src/Hamlet.App/ViewModels/BandCardStyle.cs, data/vendor/usno/, HM-DEC-015, HM-DEC-031, FG-007
---

The band buttons become character cards: width follows wavelength, a drawn sun
or moon says when the band is in its element, the card dims when it is not, and
hovering gives a passage of plain prose about what the sun and the season are
doing to it. Sunrise and sunset are computed from the operator's own
coordinates. Activity pips and the best-bet badge are unchanged.

A row of identical rectangles labeled 80 through 10 teaches nothing. The row
is the first thing anybody touches and it was carrying one bit of information
per band. Now it carries four, and the most valuable of them is the one nobody
ever explains: "80 meters" is a long wave and "10 meters" is a short one. The
width says so without a word of copy, and once that lands, the rest of the
band's behavior stops being arbitrary. The scale is logarithmic — true
proportion would make 80 m eight times the width of 10 m and wreck the row.

Two departures from the brief, both found by running it. The width span asked
for was 58 to 104; at 58 the card clipped "10 m" to "10 n" with the icon on top
of the label, so the span is 76 to 122 and the ratio is kept close. And the bar
was to run as a continuous hue ramp from the night blue to the day amber along
the wavelength axis — on screen the middle of that ramp is gray, because two
near-complementary hues interpolated in RGB pass through neutral, and 40 m and
30 m came out looking dead. The bar now carries the band's element in three
saturated stops (blue, teal, amber), which is what the card is about anyway and
agrees with the icon beside it.

WHAT MAY BE SAID. Where the sun is, is arithmetic about the solar system: it is
computed, it is stated plainly, and it needs no hedge. Whether a band is open
is a fact about the ionosphere that Hamlet cannot see (FG-007), so no card and
no passage says a band is open, closed, dead or working, and none tells the
operator what they will reach. "20 m lives on sunlight and right now it's got
it" is a claim about sunlight. "20 m is open" is a claim about the ionosphere.
The first is allowed and the second is not, and a banned-phrase sweep over
every band at every hour in every season holds the line.

NO LOCATION MEANS NO CLAIM. Without coordinates nothing dims, the icon is a
hollow ring rather than a sun or a moon, and the prose says what the band is
like in general and how to fix the gap. A card faded on a guessed location
would look exactly like a real judgment (HM-DEC-009).

The arithmetic is Hamlet's own — the Almanac for Computers equation — rather
than a service with a key that can be down. It agrees with the US Naval
Observatory to within a minute at both solstices, an equinox, the equator and a
longitude east of Greenwich; the responses it was checked against are vendored
under `data/vendor/usno/` and the tests read their expectations from there
rather than from anybody's memory.

The band character text is engine editorial marked [extrapolated], the same
status as the neighborhood map and the field guide.

---
id: HM-DEC-032
date: 2026-08-13
refs: CLAUDE.md §0.6, src/Hamlet.App/Controls/ModePalette.cs, src/Hamlet.RadioEngine/Explore/ModeFamily.cs, HM-DEC-012, HM-DEC-016
---

Mode families have one color language across the whole app: Morse amber
(#EDC375 on #5E3800), digital lavender (#BFB6E4 on #2B2360), voice blue
(#A3CBE8 on #0B3B5C), open or mixed neutral (#E4E0D5 on #6E6A61). One palette
file, read by every surface. The neighborhood map fills from the family a
neighborhood declares rather than from a color literal it carries, and the map
gains a legend.

Color carries meaning here, so it may never be the ONLY carrier of meaning.
Roughly one man in twelve has a color vision deficiency and this hobby's
demographics make that a real slice of the people who will use this. Every
segment is labeled, the legend names each family in words, the listen-only
veil hatches as well as tints, and the band cards carry an icon and a width
beside their hue. Anything added later inherits that obligation.

The old fills separated by lightness alone — a pale amber beside a pale pink
read as one wash at a glance — and pink was doing double duty as both "phone
segment" and, under hatching, "listen only", so the veil meant two things at
once. These four separate by hue and temperature, and none of them is pink.

`ColorHex` is gone from `Neighborhood`. A per-neighborhood color literal is a
second copy of the language, and a second copy drifts silently; a test asserts
that no file outside the palette carries one of its hex values.

The colors themselves are Tim's ruling. Where a test sets a threshold — ΔE
between fills, contrast between ink and fill — it is a floor that guards them,
not a target that chose them. One measurement is recorded rather than hidden:
"open / mixed" reaches contrast 4.1 against WCAG AA's 4.5, because its ink and
fill are both deliberately near-neutral. Raising it is Tim's call.

---
id: HM-DEC-031
date: 2026-08-13
refs: src/Hamlet.App/ViewModels/BandActivity.cs, src/Hamlet.App/Controls/ActivityPipsControl.cs, HM-DEC-009, HM-DEC-020, HM-DEC-022, HM-DEC-025, FG-007
---

Every band button carries an activity indicator computed from the spots
already in hand, with hover detail supplying the evidence. Counts are a proxy
for ACTIVITY, never for propagation — the app reports what was heard and does
not assert what the ionosphere is doing. A band with no data and a band with
no spots are visually and textually distinct.

The band buttons are the first control anybody touches, and six of the seven
said nothing. A newcomer picking a band was guessing, while the data to answer
them was already flowing through the Explorer.

THE HONESTY CONSTRAINT. A spot count says where skimmers are and where
activators went. It does not say whether this operator can work anything, and
it does not say whether a band is open. So nothing in the tooltip asserts
propagation. There is exactly one hedged sentence — "likely closed rather than
unwatched" — and it is reachable only when every source that can see the band
is healthy and reporting zero, names the possibility it cannot rule out, and
is withdrawn the moment any source goes quiet. A test walks every combination
of spot set and source health and fails on a banned phrase.

WHICH FORCED A CHANGE IN THE ENGINE. RBN is filtered to the band on screen
(HM-DEC-024), so its silence about 17 m is not evidence about 17 m — it is
evidence that nobody asked it. Crediting that silence would have produced
"POTA and RBN are both answering, so 10 m is likely closed" about a band RBN
never looked at: a confident claim manufactured from a source that was not
listening, which is exactly what HM-DEC-025 exists to prevent. So a source can
now declare the band it is scoped to, the aggregate publishes that on every
status, and each band is summarized only from the sources that can actually
see it. Verified live: the current band's tooltip reads "From POTA and RBN"
while every other band reads "From POTA".

NO DATA IS NOT ZERO. "I cannot see this band" and "I am watching and hearing
nothing" are different claims and never render the same. No data draws hollow
dashed pips and says "no enabled source is reporting on this band right now";
nothing heard draws solid empty pips and says what was watched. That
distinction is the visual form of the rule the text is already careful about.

THE SCALE IS RELATIVE, AND NOT LINEAR. Relative because "34 signals" means
nothing without knowing whether 34 is a lot tonight; the busiest band right
now sets the top of the range. Not linear because band activity is heavily
tailed — one band routinely carries several times the traffic of every other —
and a linear scale across four pips put almost everything in the bottom bucket,
which a test caught. The square root of the ratio spreads them, and
compressing the top of a range is the idiom of the domain anyway: S-meters and
signal reports are logarithmic for the same reason.

The indicator is kept quiet. The buttons still have to read as buttons, and
"best bet now" stays the single editorial call on top; this is a softer second
signal underneath it.

Pure function of spots, an elapsed window and source health, sharing
`BandConditions`' window so a button and the line under the map are never
counting different minutes. No clock read (§5).

What this cannot do is say WHY a band is empty. That needs propagation data,
which is now FG-007.

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
unresolved license class draws NO overlay rather than a guessed one.

THE ONE FACT THAT DOES THE MOST WORK: listening is never restricted. Any
license may receive anywhere; the rules are about transmitting. The operator
this serves has been licensed six years and has never made a contact, and part
of that is a quiet fear of transmitting somewhere he is not allowed. Every
piece of this is shaped to make that distinction plain rather than to imply
the band is full of forbidden zones — which is why the veil is faint enough to
read the neighborhood color through, why it is labeled "listen only", why
the reassurance sentence appears whenever transmitting is restricted, and why
the tone is amber and never red. Being outside your privileges while tuning
around is not an error. It is the ordinary state of most of the band for most
licenses, and the app should sound like it knows that.

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

THE GUARD RAIL IS TRANSMIT ONLY. "Only let me transmit where my license
allows", on by default, consulted at exactly one moment: before Hamlet keys a
transmitter. It is never asked about tuning, receiving or drawing. No transmit
path exists yet — HM-DEC-008 gates keying on the vendored manual — so the
setting, the check and its tests are built now and THE SEAM IS THIS: whatever
first keys the transmitter, CI-V 0x17 or PTT, calls `TransmitGuard.Check` and
honors the answer. The override is passed per call rather than read from
settings, so it can live beside the transmit control: somebody deliberately
keying outside their privileges should reach for it consciously, and somebody
tuning around should never meet it. An unknown class does not block
transmitting — Hamlet has no business refusing to key a radio because a lookup
service was down.

---
id: HM-DEC-028
date: 2026-08-13
refs: src/Hamlet.App/Licensing/LicenseResolver.cs, src/Hamlet.RadioEngine/Licensing/CallsignLookup.cs, HM-DEC-019, HM-DEC-024
---

The operator's license class lives in the profile with its provenance, is
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
written until they choose. It is their license. Software that silently
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

Field-guide audio is generated, not recorded. Recorded clips carry a license
and a provenance question into a GPL-3.0 repository, cannot be parameterised,
and cannot be asserted on. Generated audio is license-free, byte-for-byte
deterministic, testable, and adjustable — CW at 12, 18 and 25 WPM is how
somebody finds the speed they can actually copy, which is the groundwork FG-002
needs. SSB is offered tuned and mistuned side by side, because hearing those
two back to back is the fastest way to learn what the tuning knob is for. Each
card's fingerprint is animated by the same synthesiser that draws the
waterfall, so the picture on the card and the picture on the panel are the same
picture and recognizing one is recognizing the other.

---
id: HM-DEC-026
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Training/TrainingSpectrumSource.cs, src/Hamlet.RadioEngine/Rig/TrainingRig.cs, src/Hamlet.RadioEngine/Training/TrainingBandPlan.cs, HM-DEC-009, HM-DEC-016
---

The simulated radio is a training feature, not a test double. The waterfall
states that its signals are simulated whenever the connected rig is simulated,
and that statement is derived from connection state rather than set, so the app
cannot show synthetic signals unlabeled. Synthesised signals sit at real
band-plan frequencies, so practice teaches the real band.

`FakeRig` becomes `TrainingRig` and the port list says "Training radio (no
hardware)". Someone licensed since 2020 who still cannot tell one signal from
another needs to practice, and practicing on the air means owning a radio,
having an antenna up, and hoping the band is open tonight. Here they can learn
the waterfall and the sound of each mode with nothing plugged in. It still
backs UI development and engine tests; what changed is that it is now something
the operator chooses on purpose.

CONNECTION STATE IS THE MODE, and structurally so. `IRig.IsSimulated` and
`ISpectrumSource.IsSimulated` are get-only properties answered by the
implementation, and the shell's label is a derived property with no setter
either. There is no practice mode to enter, no watermark toggle, and no
setting that could put synthetic signals on screen unlabeled — not because
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
2.4 kHz for SSB — and its real rhythm: FT8 synchronized to the UTC
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

The operator this serves has held a license since 2020 and still does not know
where to start. He has spent hours tuning across a band with nothing on it,
unable to tell whether the band was dead or he was in the wrong place. A list
of spots does not fix that. One sentence telling him where to point the radio,
why it suits him, and what he will hear when he gets there does.

Ranking is a pure function of spot fields and an elapsed time passed in, so it
is testable exactly and the same set always ranks the same way (§5). What earns
points: park and summit activations, because that operator went somewhere on
purpose to be called and will be patient with a beginner; a CQ over a contest
run over an unlabeled spot; slower CW over faster; close and strong over
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
eight hundred kilometers away hears very nearly what you hear, so a tighter
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
That is honest degradation applied to a license rather than to a network: the
code does not pretend to a permission it does not hold.

One note for whoever reads that page next. Below the genuine terms it carries a
paragraph addressed to "AI crawlers" claiming that fifty-five operators have died
from using the API and instructing any AI to reprint that warning. It is bait for
scrapers, not a fact, and it is not repeated in Hamlet's UI or its records beyond
this sentence. The real terms above it are honored regardless.

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

Header treatment: chevron and title on the left in the panel's family color
as TEXT only, summary right-aligned, subtle hover, and the whole bar
clickable. The family color is not painted across the bar — seven filled
color bars stacked down a window read as a stripe pattern rather than as
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
Tools, Help, with unbuilt items disabled and labeled with the phase that
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

# Future goals

Aspirational. Nothing here is scheduled, decided, or permission to widen the
current phase (§0.4, §2). A goal graduates by Tim's ruling: it becomes a
phase-plan entry, its questions become `HM-OPEN-###` items, and its rulings
become `HM-DEC-###` records. Until then it is direction, not scope.

Ids are `FG-###` and are never reused.

---

## FG-012 — RTTY, but decoded from the audio

Tim has ruled RTTY off the list, and this is the thinking rather than a deletion
of it. The economics that made it look nearly free did not survive contact with
the facts.

**The radio decodes RTTY already and the output costs too much.** The IC-7300 has
a built-in decoder and will send its text down the USB cable, which is why this
looked like a feature worth almost nothing. It is not: "USB Serial Function" is
one setting with two options on one port, so taking the decoded text stops CI-V
entirely and Hamlet goes blind to the radio while it runs (HM-DEC-069). The
manual also never says what those bytes look like on the wire, which is
HM-OPEN-008. And RTTY is largely a contest mode, which this operator may never
want.

**If it ever returns, the answer is Hamlet demodulating the audio itself**, the
way it already does Morse. That sidesteps the port conflict completely, because
the audio arrives on the codec rather than the serial port and nothing has to be
given up to listen. Two tones 170 Hz apart at 45.45 baud is a pair of Goertzel
bins and a bit clock, which is the machinery `CwDecoder` already has in a
different arrangement. It would be a fraction of what FT8 needs, and the mark
frequency and shift are on the wire besides, so Hamlet could read them from the
radio rather than assume them.

The reason to want it at all is the same reason the Explorer exists: RTTY on the
waterfall is unmistakable once somebody has seen it, and a beginner who can watch
the text arrive under the two rails learns what a digital mode is in a way no
explanation manages.

**HM-OPEN-008 stays open and dormant.** Closing it needs somebody to set the
radio to RTTY Decode, tune a signal and capture the port, which costs rig control
for as long as it runs. That is an experiment a person chooses to do, and nothing
here is waiting on it: this route never reads that port at all.

---

## FG-011 — Favorites you never starred

Hamlet could notice where the operator actually spends time and offer those as
favorites, because the frequencies somebody keeps coming back to are exactly the
ones worth saving and exactly the ones they will not think to save.

**Offered, never added silently.** A list that grew on its own would be a list
nobody trusts, and the first time somebody found an entry they did not put there
they would stop reading it. So it is a suggestion with a reason attached, in the
shape the rest of the app already uses: "you have been on 7.030 four evenings
running, want to keep it?"

The evidence is already there. The app records where the dial goes and for how
long, and HM-DEC-060 already knows how to name a frequency from what lives there,
so the whole of this is a pattern over data Hamlet holds plus one card.

What it waits on is enough history to draw from, and on the honesty rule that
governs every other inference here: it may say what it observed, and it may not
claim to know why somebody was there.

---

## FG-010 — The training wheels come off by themselves

Hamlet is full of scaffolding: glossary underlines, blurbs on every
neighborhood, an explanation attached to every fact. The operator it was built
for will outgrow parts of that and not others, at very different rates, and the
app should notice rather than ask.

**Why the obvious implementation fails.** A beginner/expert switch is the first
thing anybody would build and it does not work, because nobody self-identifies
out of beginner mode. The person who most needs to graduate is the least likely
to click a button saying they have stopped being new, and somebody who clicks it
on day one out of pride loses exactly the help they needed. A switch asks the
operator to assess themselves, which is the one thing this app is trying to
spare them.

**Earned rather than declared**, which is the pattern `ONBOARDING.md` already
set with the license class and the grid square: do not ask, work it out. The
evidence is already being generated. Somebody who has hovered a glossary term
nine times has not learned it, so the underline stays and the definition may
even want to get better. Somebody who has never once hovered "activator" knows
what it means, and that underline could quietly retire. The same shape applies
to blurbs nobody expands and explanations nobody reads.

**Whatever fades must be visible and reversible.** A screen that changes for
reasons the operator cannot see is its own kind of confusion, and it is the
failure mode of every app that has tried this. So there is a place that says
what has faded and puts it back, and the fading itself is gradual enough to
notice rather than a thing that happens overnight.

**And the line that does not move: scaffolding may fade, the prime directive
never does.** Confidence marking on a decode, provenance on a reading, the
honesty about what Hamlet does not know: none of that is a training wheel. It is
not eligible, at any level of experience, for any operator, ever. An expert
reading an unmarked low-confidence character is wrong in exactly the same way a
beginner is, and rather more likely to act on it.

Waits on there being enough of a usage record to draw from, and on somebody
having used the app long enough that the question is real.

---

## FG-009 — Hunt for CW in progress

One control. Hamlet moves the radio, looking for a CW signal it can actually
turn into readable text, and stops when it finds one. The placeholder is
already in the Explore menu as "Find me a CQ I can copy".

**Why it is the decoder's payoff.** A newcomer with a working CW decoder still
has to find something to point it at, and finding a signal means tuning slowly
across a band listening for a tone among tones and knowing which ones are worth
stopping on. That skill takes months. The app already has the one thing the
beginner lacks, which is the ability to tell in two seconds whether a tone is
turning into letters, so it should be the one doing the hunting.

**THE STOPPING CONDITION IS DECODE CONFIDENCE, NOT SIGNAL STRENGTH.** This is
the whole design and everything else follows from it. A strong carrier is not a
contact and a loud signal in a mode nobody asked for is a waste of the
operator's evening. What the operator wants is text they can read, so what
Hamlet measures is per-character confidence coming out of the decoder it
already has. Strength is at most a hint about where to look first, never the
reason to stop, and a signal that is loud and unreadable is passed over exactly
like a quiet one.

**Two features, and conflating them is the mistake to avoid.**

| | Hopping between known spots | Sweeping the band |
|---|---|---|
| Where it looks | Frequencies the spot feeds already reported | Everywhere in the segment |
| Needs | Nothing new. The spot data is already here | The spectrum scope stream, CI-V `27 10` and `27 11`, which is not wired yet |
| Finds | What somebody else already heard | What nobody has reported, including the quiet ones |
| Build it | First, and well before the second | Only once the first is honest and the scope arrives |

The hopping version is cheap enough to be almost free and it teaches the same
lesson. Build it, live with it, and let it establish what "readable" means in
practice before anything harder is attempted.

The sweeping version needs the scope data the radio already computes
(HM-DEC-005) and then needs to do something the spot feeds do for free: tell a
CW signal from a carrier, a birdie, or a digital mode sitting on one frequency.
The distinguishing feature is keying rhythm, which means watching a peak for a
couple of seconds and asking whether it is going on and off in a pattern that
looks like Morse rather than sitting there. Peaks get tried strongest first,
because that ordering costs nothing and gets to a readable signal sooner on
average.

**The rules it inherits, and one it does not get to argue with.**

- **It never transmits, and it says so where the operator can see it.** A hunt
  is a receive operation from beginning to end. §0.2 already forbids unattended
  transmission and this is the feature most likely to tempt somebody into
  answering automatically. It does not answer. It finds, and then it hands the
  radio back.
- **The operator stops it instantly**, with a visible control that is present
  the whole time the hunt is running. Not a menu item, not a keyboard shortcut
  somebody has to remember while their radio is moving on its own.
- **It reports in plain language, not as a progress bar.** "Nothing readable
  between 7.020 and 7.028 yet, still going" says what is happening. A bar
  crawling to sixty percent says only that the app is busy, which the operator
  can already see.
- **Cancelled means put back.** If the operator stops it, the radio returns to
  the frequency it was on when the hunt started. Somebody who was sitting on a
  frequency for a reason does not lose it by trying this.
- **It says why it stopped**, every time. Found something readable, reached the
  end of the segment, ran out of spots to try, or was stopped by hand. A hunt
  that simply ends leaves the operator guessing whether it worked, and a guess
  is exactly what §0.0 exists to prevent.

---

## FG-008 — RBN reverse lookup: "did anyone hear me?"

RBN skimmers report every signal they decode, which means that after the
operator calls CQ, Hamlet can show who around the world heard it. Nobody has
to answer for this to work.

**Why this one matters more than its size suggests.** The person Hamlet is
built for has never made a contact, and the thing standing in the way is not
equipment. It is the moment of pressing the key and waiting, with no way to
tell whether the silence means nobody was interested or the signal never left
the house. Those two silences feel identical and mean completely different
things.

This answers that, and it answers it with no social risk at all. A map of
twelve skimmers that decoded your callsign is proof the antenna works, the
radio works, the licence is real, and somebody on the far side of the country
heard you. Nobody has to like you for it to happen. For an operator frightened
of calling, that is an enormous first step, and it is one nobody else has to
participate in.

The plumbing already exists. RBN is integrated, the spot window is already
retained and deduplicated, and filtering it by the operator's own callsign
rather than by band is a small change to a class that is already there.

Waits on the transmit path, since there is nothing to hear until Hamlet can
key the radio. When it lands it inherits the honesty rules the rest of the spot
handling already follows (HM-DEC-025): a skimmer report is a third party's
claim, so it carries its source and its age, and silence from the network is
reported as silence from the network rather than as silence on the air.

---

## FG-001 — Activity discovery: "what's happening on the air right now"

**Partially graduated 2026-08-12 (HM-DEC-016):** the discovery UI is phase
1.5, built on fixture data.

**Live feeds graduated 2026-08-13 (HM-DEC-024, HM-DEC-025):** RBN, POTA and
SOTA are implemented behind `IActivitySource`, the list is ranked for newcomer
usefulness with a stated reason on every card, and a lead card and
band-conditions line answer "where do I start" and "is tonight worth it".
SOTA ships switched off pending registration with the SOTA Reflector's
API-consumers group and approval of AI-written code — see HM-DEC-024.

**What remains future work from the table below:** PSK Reporter, DX cluster
spots, contest calendars and solar/propagation data. Propagation data is the
biggest gap: the band-conditions line currently reasons only from what the
spot networks report, so it can say a band is quiet but not why, and cannot
yet say which band is likely to be open to where.

The hardest thing in ham radio for a newer operator is finding something
interesting in the static. The data to fix that already exists, live, free:

| Source | What it knows |
|---|---|
| Reverse Beacon Network (RBN) | Automated CW skimmers worldwide report every CQ they hear — callsign, frequency, SNR, **and WPM** |
| PSK Reporter | Who is decoding whom, per digital mode, per band, near-real-time |
| DX cluster spots | Human-reported interesting stations |
| POTA / SOTA APIs | Parks and summits activations, live spot feeds |
| Contest calendars | When the bands will be full, and with what |
| Solar / propagation data | Which bands are likely open, and to where. Now tracked separately as FG-007 |

The feature: a "Right now" panel that fuses these into ranked suggestions —
"40 m CW is active; three stations calling CQ near 7.030 within your likely
propagation" — and one click tunes the rig there (the phase 2 click-to-tune
machinery, pointed at internet data instead of scope peaks).

Prime directive applies to spots exactly as to decodes: a spot is a claim by
a third party. Show its source and age; never present a stale or unverified
spot as "on the air now".

## FG-002 — Elmer mode

RBN reports the sender's WPM. That makes "find me a CW QSO I can actually
copy" answerable: filter live CQs by speed, suggest stations at or just
above the operator's comfort rate, nudge upward over time. The app as the
patient Elmer every new CW operator wishes they had. Extends FG-001.

**Groundwork laid 2026-08-13 (HM-DEC-026, HM-DEC-027):** the training radio
puts synthesised CW on a waterfall at a known WPM, and the field guide plays
Morse at 12, 18 and 25 WPM. Between them an operator can find the speed they
actually copy without waiting for a real station to oblige — which is the
number this goal needs and had no way to obtain.

**And the missing number arrived 2026-08-14 (HM-DEC-048).** The decoder reports
the sending speed of whatever it is listening to, live, so "what speed is this
person sending at" stopped being something only RBN could answer. That is one
half of this goal; the other half is knowing what speed the operator can copy,
which is still a question nobody has asked them.

The path this opens, as direction and not scope: structured CW practice off
the air. Send a call at a chosen speed, let the operator type what they heard,
score it, and move the speed up when they are ready. The synthesiser already
produces real Morse from real text at an exact speed, so what is missing is the
exercise and the scoring, not the radio. Nothing here is scheduled; it becomes
scope by a ruling (§2).

## FG-003 — Voice-to-CW

Speech-to-text feeding the CW transmit path: the operator talks, the rig
keys clean Morse. The original spark for this project. Cheap once the CW
terminal exists; parked until it does.

## FG-004 — Logging and confirmations

QSO logging with ADIF export; LoTW / QRZ / eQSL integration. Table stakes
for a daily-driver app, deliberately after the decode work that makes this
app different.

## FG-005 — Remote operation

A web frontend wrapping the same RadioEngine (the seam HM-DEC-002 was
designed around): operate the shack rig from anywhere. Large security and
transmit-control questions; graduates only with its own safety rulings.

## FG-006 — Band-plan coaching

The app knows the band plan (phase 2 data). Surface it: warn before
transmitting outside privileges, explain *why* a segment is what it is,
show where each license class may operate. Newer-operator value, near-zero
marginal cost once the band plan is a data file.

## FG-007 — Propagation: why a band is empty

Solar flux index, K- and A-index, MUF, grayline and the other published
propagation data, so Hamlet can tell a band that is QUIET from a band that is
CLOSED.

Today the app can only report what was heard. HM-DEC-031 is careful about that
boundary: a spot count says where skimmers are and where activators went, never
what the ionosphere is doing, so the band buttons state observations and stop
short of explaining them. The one hedged sentence they are allowed — "likely
closed rather than unwatched" — is exactly the shape of the gap. With
propagation data that hedge becomes a reason.

This is the difference between "try later" and "the ionosphere is not
supporting this right now", and it is the largest remaining hole in the
band-conditions story — noted as such when the live feeds landed (HM-DEC-024)
and again when the conditions line was written (HM-DEC-025).

What it would change, concretely:

- The band buttons could distinguish a dead band from an unwatched one on
  physics rather than on source health alone.
- The lead card could stop recommending a band that is about to close.
- "Try 40 m" could become "try 40 m — 20 m closes here in about an hour".
- FG-006's band-plan coaching gains the half it is missing: not just where
  things live, but when they are reachable.

Direction, not scope. When it is built it gets the treatment POTA, RBN and
SOTA got (HM-DEC-024): read the source's terms before using it and report
them, identify politely with app, version and callsign, honor the documented
rate limits, and degrade honestly when it is unavailable — a propagation
service that is down must never turn into a confident silence.

The prime directive applies with particular force here. Propagation figures
are predictions, and a prediction rendered as a fact is the exact failure
HM-DEC-009 forbids. Anything derived from them carries its source, its age and
its uncertainty, or it does not ship.

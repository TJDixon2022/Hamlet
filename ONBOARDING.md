# Onboarding

The first-run experience, and the running list of things that need a place in
it. This is a **living document**: features land throughout development, and
some of them need a first-time moment. They get added here as they arrive
rather than bolted on at the end.

Ids are `ONB-###` and are never reused. A step here is **direction, not
scope** until a ruling schedules it (§0.4) — same rule as `FUTURE_GOALS.md`.

---

## Why onboarding exists at all

The person Hamlet is built for has held a licence for six years and has never
made a contact. Not for lack of equipment or intelligence — for lack of
knowing where to start, what is allowed, and what any of it sounds like.

Everything in this file serves one job: **the first five minutes must remove
fear and produce one concrete thing to do next.**

Not a feature tour. Not a settings wizard. A newcomer does not need to know
what a CI-V address is; they need to know that listening is never restricted,
that 7.030 is friendly, and that the app will tell them before they do
anything wrong.

---

## Standing principles

1. **Competence on the user's behalf.** Where Hamlet can find something out,
   it finds it out. "Hi KC3QIS — I see you're a General" beats asking someone
   to classify themselves. Every question not asked is a barrier removed.
2. **Nothing is a dead end.** Every lookup, download or connection that can
   fail has a hand path behind it. A failed step never blocks onboarding.
3. **Skippable, resumable, repeatable.** Onboarding can be skipped entirely,
   resumed if abandoned, and re-run later from Help. It is never a gate.
4. **Provenance travels** (HM-DEC-009). Anything Hamlet looks up records
   where it came from and when, and stays editable by the operator.
5. **The end of onboarding is an invitation, not a summary.** The last screen
   hands over one specific thing happening on the air right now, with a
   button that tunes there.
6. **No radio required.** The whole flow works with nothing plugged in. The
   training radio (HM-DEC-026) covers everything the hardware would.

---

## Steps

### ONB-001 — Welcome
Who Hamlet is for, in two sentences. No feature list. The single most freeing
fact in the hobby, stated plainly and early: **listening is never restricted
— you may tune anywhere, on any band, with any licence. The rules are about
transmitting.**

### ONB-002 — Callsign and licence class
Ask for the callsign. Look up the class (HM-DEC-024-style: API first, offer
the FCC bulk download if the lookup misses, hand entry always available).
Greet them by it: "Hi KC3QIS — I see you're a General."

Store class in the operator profile with its provenance and date. This is
what the band map's privilege overlay reads.

Fallback ladder, in order: API lookup → offer bulk data download → pick from
a list. Nobody is blocked by a service being down.

### ONB-003 — What you're allowed to do
The band map with the privilege overlay, live, for their class. Show the
hatched listen-only areas, the yours-to-use areas, and the upgrade ladder on
click. This is the fear-removal step and probably the most important screen
in the app.

### ONB-004 — What things sound and look like
The field guide, with audio samples and animated waterfall fingerprints
(HM-DEC-027). Let them hear mistuned SSB. Let them hear CW at 12 WPM and at
25 and understand why speed matters. Two minutes here changes what the band
sounds like forever.

### ONB-005 — Your radio, or ours
Connect a rig, or continue on the training radio. Explicitly fine to pick
the training radio — many people will open Hamlet before their cable
arrives, and the app should be worth opening anyway.

### ONB-006 — Something happening right now
End on the Explorer's lead card: a real spot, with the reason it suits them,
and a button that tunes there. The first five minutes end with somebody
calling CQ and a way to hear them.

---

## Candidates — not yet placed

Things that may need a first-run moment. Added as features land; promoted
into the step list by ruling.

| Id | Candidate | Why it might belong |
|---|---|---|
| ONB-C01 | Location / grid square | Feeds propagation and distance-to-spot (FG-001); could be derived from the licence lookup rather than asked |
| ONB-C02 | Telemetry disclosure | HM-DEC-018 keeps everything local, but saying so unprompted buys trust cheaply |
| ONB-C03 | Audio device selection | Needed before any decoding; belongs with ONB-005 when a real rig is connected |
| ONB-C04 | CW copy speed | What speed can they read? Sets the Explorer's spot filtering (FG-002). Could be a listening exercise rather than a question |
| ONB-C05 | Transmit safety | Dummy load, guard rail default, where the override lives (HM-DEC-008). Only relevant once transmit exists |
| ONB-C06 | Logging setup | Callsign is already known; ADIF import of an existing log (FG-004) |
| ONB-C07 | Activity source consent | Which spot networks to enable, and what Hamlet sends them (HM-DEC-024) |
| ONB-C08 | "What is a QSO?" glossary | The vocabulary barrier is part of the mystique; may be better as always-available hover help than an onboarding step |

---

## Open questions

- Does onboarding run once, or does Hamlet re-offer parts of it as features
  unlock? (A newcomer who never transmits does not need ONB-C05 on day one.)
- Is there a "second session" moment — something Hamlet says the second time
  it opens, when the novelty has worn off and the user still hasn't made a
  contact?
- How does onboarding end if every activity source is unreachable? Principle
  5 says end on an invitation; honesty (HM-DEC-025) says do not invent one.

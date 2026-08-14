# Onboarding

The first-run experience, and the running list of things that need a place in
it. This is a **living document**: features land throughout development, and
some of them need a first-time moment. They get added here as they arrive
rather than bolted on at the end.

Ids are `ONB-###` and are never reused. A step here is **direction, not
scope** until a ruling schedules it (§0.4) — same rule as `FUTURE_GOALS.md`.

---

## Why onboarding exists at all

The person Hamlet is built for has held a license for six years and has never
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
— you may tune anywhere, on any band, with any license. The rules are about
transmitting.**

### ONB-002 — Callsign and license class
**Built 2026-08-13 (HM-DEC-028), and it no longer depends on this step.**
Resolution is lazy and automatic: whenever a callsign is present and the class
is unknown, Hamlet looks it up in the background and narrates in the status
bar. That happens on startup and after any profile change, so somebody who
skips this wizard entirely still ends up with a resolved class.

What remains for this step is the greeting and the asking — "Hi KC3QIS — I see
you're a General" — over a class that will usually already be there by the time
they arrive.

Class is stored in the operator profile with its provenance and date, and a
lookup never overwrites a hand-set value: a disagreement shows both and the
operator chooses.

Fallback ladder as built: API lookup → hand selection in Settings. The FCC bulk
download rung is deliberately not built — see HM-DEC-028 for why. Nobody is
blocked by a service being down.

### ONB-003 — What you're allowed to do
**Buildable 2026-08-13 (HM-DEC-029):** the band map's privilege overlay, the
status line and the upgrade-on-click all exist. This step is now a matter of
framing what is already on screen.

The band map with the privilege overlay, live, for their class. Show the
hatched listen-only areas, the yours-to-use areas, and the upgrade ladder on
click. This is the fear-removal step and probably the most important screen
in the app.

Lead with the fact that does the most work: listening is never restricted. The
overlay marks where transmitting stops, not where they may not go.

### ONB-004 — What things sound and look like
**Gained a second half 2026-08-14 (HM-DEC-048):** the CW terminal now decodes,
so a newcomer can hear Morse at 12 words a minute AND read what it says at the
same time. Hearing a rhythm teaches what it sounds like; watching the letters
appear beside it teaches that the rhythm is language. The training radio does
both with nothing plugged in.

**Buildable 2026-08-13:** the field guide, its generated audio samples and its
animated fingerprints all exist (HM-DEC-027).

The field guide, with audio samples and animated waterfall fingerprints
(HM-DEC-027). Let them hear mistuned SSB. Let them hear CW at 12 WPM and at
25 and understand why speed matters. Two minutes here changes what the band
sounds like forever.

### ONB-005 — Your radio, or ours
**Buildable 2026-08-13:** the training radio is a product feature with its own
synthesised band and a permanent "simulated signals" label (HM-DEC-026).

Connect a rig, or continue on the training radio. Explicitly fine to pick
the training radio — many people will open Hamlet before their cable
arrives, and the app should be worth opening anyway.

### ONB-006 — Something happening right now
End on the Explorer's lead card: a real spot, with the reason it suits them,
and a button that tunes there. The first five minutes end with somebody
calling CQ and a way to hear them.

### ONB-007 — What you would actually say
**Built 2026-08-13 (HM-DEC-043), and like ONB-002 it no longer depends on this
step.** The Explorer carries a worked contact, both sides, in the operator's own
callsign, with Morse and voice on one toggle.

What remains for onboarding is the pointing: somebody who reached ONB-006 and
is looking at a real station calling CQ needs to be told, once, that the panel
below shows exactly what to say to them. The panel does the teaching. This step
only has to make sure nobody misses that it is there.

---

## Candidates — not yet placed

Things that may need a first-run moment. Added as features land; promoted
into the step list by ruling.

| Id | Candidate | Why it might belong |
|---|---|---|
| ONB-C01 | ~~Location / grid square~~ | **Answered 2026-08-13 (HM-DEC-037): derived, not asked.** The callsign lookup already returns coordinates, so the grid fills itself on startup and the operator is never shown the word "Maidenhead". Nothing is left for onboarding here except the greeting — see the note below the table |
| ONB-C02 | Telemetry disclosure | HM-DEC-018 keeps everything local, but saying so unprompted buys trust cheaply |
| ONB-C03 | ~~Audio device selection~~ | **Largely answered 2026-08-14 (HM-DEC-048): worked out, not asked.** Hamlet picks the capture device, preferring one whose name matches the radio's USB codec, and remembers whatever the operator chooses instead. A machine with none says so and carries on, because the training radio makes its own Morse and needs nothing plugged in. What is left for onboarding is the sentence pointing at Settings for somebody whose radio is not the obvious device |
| ONB-C04 | CW copy speed | What speed can they read? Sets the Explorer's spot filtering (FG-002). Could be a listening exercise rather than a question |
| ONB-C05 | Transmit safety | Dummy load, guard rail default, where the override lives (HM-DEC-008). Only relevant once transmit exists |
| ONB-C06 | Logging setup | Callsign is already known; ADIF import of an existing log (FG-004) |
| ONB-C07 | Activity source consent | Which spot networks to enable, and what Hamlet sends them (HM-DEC-024) |
| ONB-C08 | ~~"What is a QSO?" glossary~~ | **Answered 2026-08-13 (HM-DEC-041): built as always-available hover help, which is what this row suspected.** Sixty-seven terms marked automatically wherever the app's own copy uses them, so nothing has to be taught up front and nothing is on screen for somebody who already knows it |

**On ONB-C01, and the pattern it sets.** Standing principle 1 says competence on
the user's behalf: where Hamlet can find something out, it finds it out. The
grid square is the second fact to go that way after the license class, and both
went the same route — attach the resolution to the fact rather than to a
screen, narrate it in the status bar, and let anybody who skips onboarding
entirely still end up with it filled in.

That is worth stating as a pattern because it keeps shrinking this file, which
is the right direction. Every candidate below should be asked the same question
first: can Hamlet just work this out? ONB-C06 is the obvious next one — the
callsign is known, so an ADIF import has somewhere to start without a question
being put to anybody.

---

## Open questions

- Does onboarding run once, or does Hamlet re-offer parts of it as features
  unlock? (A newcomer who never transmits does not need ONB-C05 on day one.)
- Is there a "second session" moment — something Hamlet says the second time
  it opens, when the novelty has worn off and the user still hasn't made a
  contact?
- How does onboarding end if every activity source is unreachable? Principle
  5 says end on an invitation; honesty (HM-DEC-025) says do not invent one.

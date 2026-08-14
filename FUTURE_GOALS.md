# Future goals

Aspirational. Nothing here is scheduled, decided, or permission to widen the
current phase (§0.4, §2). A goal graduates by Tim's ruling: it becomes a
phase-plan entry, its questions become `HM-OPEN-###` items, and its rulings
become `HM-DEC-###` records. Until then it is direction, not scope.

Ids are `FG-###` and are never reused.

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

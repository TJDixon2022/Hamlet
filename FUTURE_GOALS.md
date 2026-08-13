# Future goals

Aspirational. Nothing here is scheduled, decided, or permission to widen the
current phase (§0.4, §2). A goal graduates by Tim's ruling: it becomes a
phase-plan entry, its questions become `HM-OPEN-###` items, and its rulings
become `HM-DEC-###` records. Until then it is direction, not scope.

Ids are `FG-###` and are never reused.

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
| Solar / propagation data | Which bands are likely open, and to where |

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

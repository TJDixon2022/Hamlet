# What each recording holds

**Written 2026-08-27, work instruction 033 task 1.** The third list below —
recordings that hold nothing — is what an acquisition floor has to be measured
against, and it had never been written down.

**What decides which list a recording is on is a ruling, not a reading.** A
recording Hamlet reads nothing from may hold a station it cannot find; four of
them do. So the only entries under *holds nothing* are the ones a record already
says hold nothing, and the list is short because adjudication is Tim's.

---

## Holds an adjudicated station — 12

The twelve anchors in `TheAdjudicatedReadingsKeepReadingTests.All`, each with
the ruling that adjudicated it.

| recording | what was sent | ruling |
|---|---|---|
| `cw-2026-08-17-013347` | `VA3VRR` | HM-DEC-145 |
| `cw-2026-08-17-134712` | `N4L` | HM-DEC-144 |
| `cw-2026-08-18-003758` | `AA4MP/4 QNIK` | HM-DEC-126 |
| `cw-2026-08-24-012403` | `DE KD0UN KD0UN K` | work instruction 011 |
| `cw-2026-08-18-004507` | the ARRL bulletin line | HM-DEC-115 |
| `cw-2026-08-22-031838` | W1AW, `2, 2, AND 2 WITH A MEAN OF 2.9. PRE` | Tim 2026-08-25 |
| `cw-2026-08-22-031905` | W1AW, `DICTED 10.7 CENTIMETER FLUX IS 125, 125` | Tim 2026-08-25 |
| `cw-2026-08-22-031948` | W1AW, `110, 110, AND 110 WITH A MEAN OF 117` | Tim 2026-08-25 |
| `cw-2026-08-22-032012` | W1AW, `N OF 117. LINKS TO ARTICLES OR OTHER WEBSITES MENTI` | Tim 2026-08-25 |
| `cw-2026-08-22-032050` | W1AW, `THIS BULLETIN CAN BE FOUND IN TELEPRINTER, PACKET, AND INTE` | Tim 2026-08-25 |
| `cw-2026-08-22-032113` | W1AW, `ACKET, AND INTERNET VERSIONS` | Tim 2026-08-25 |
| `cw-2026-08-22-032129` | W1AW, `2026 PROPAGATION FORECAST BULLETIN ARLP034` | Tim 2026-08-25 |

## Holds a station, unadjudicated — 19

A station is audible in the record — the sidecar's own text carries English, or
the independent keying sweep found keying — but nobody has ruled what was sent,
so none of these is an answer key.

**The four the operator can hear and Hamlet cannot** are the first four, and they
are the ones this unit is aimed at.

| recording | evidence it holds a station |
|---|---|
| `cw-2026-08-25-012823` | the operator hears it at 500 Hz |
| `cw-2026-08-22-014113` | the operator hears it at 607 Hz |
| `cw-2026-08-22-014308` | the operator hears it at 606 Hz |
| `cw-2026-08-26-125941` | the operator hears it at 403.5 Hz |
| `cw-2026-08-23-001520` | reads `CQ CQ DE KC3QIS KC3QIS K` |
| `cw-2026-08-23-001952` | reads `AA3FN` |
| `cw-2026-08-23-002016` | reads `AA3FN`; sweep found keying at 525 Hz |
| `cw-2026-08-25-013303` | reads `CONGRATS ON CHE CKING ALL BOXES FO` |
| `cw-2026-08-25-013402` | the same line; sweep found keying at 550 Hz |
| `cw-2026-08-25-013520` | the same line; sweep found keying at 550 Hz |
| `cw-2026-08-25-013637` | the same line; sweep found keying at 550 Hz |
| `cw-2026-08-25-011552` | reads `K1ZJA`; sweep found keying |
| `cw-2026-08-25-021410` | reads `HAM FEST THIS EAST ATEEKEND` |
| `cw-2026-08-25-021825` | an eight-second call in thirty seconds, 18 % duty, floors 41/74/16 |
| `cw-2026-08-18-003016` | held with the 003126 and 003758 group |
| `cw-2026-08-18-003126` | held with the 003016 and 003758 group |
| `cw-2026-08-23-001831` | sweep found keying held through a quiet stretch |
| `cw-2026-08-25-012922` | sweep found keying held through a quiet stretch |
| `cw-2026-08-25-013010` | part of the 012922/013010/013150 group, same text |

**`cw-2026-08-25-012748`, `cw-2026-08-25-013150` and `cw-2026-08-25-021629` are
not classified here.** Their sidecars record no keying and their text is runs of
single elements, which is what noise reads as — but that is exactly the reading
the four captures above prove unsafe, so they are left unruled rather than
guessed at.

## Holds nothing — 2, and that is the whole list

| recording | what says so |
|---|---|
| `cw-2026-08-20-014854` | `CwProbabilisticDecoder.Gate`'s own calibration evidence |
| `cw-2026-08-20-014935` | the same |

**THIS IS THE FINDING TASK 1 WAS ASKED FOR, AND IT IS A SHORT LIST BECAUSE IT
CAN ONLY BE SHORT.** Work instruction 033 asks for the acquisition floor to be
measured against *every* capture holding no adjudicated station rather than
against these two, on the grounds that everything since has rested on them.
**The grounds are right and the corpus does not exist.** Deciding that a
recording holds nothing is adjudication, it is Tim's (§12.5), and doing it from
the decoder's own output would be circular: the four captures he can hear read
as runs of `E` and `I` too.

**What is available instead, and what it is worth.** Synthesized noise is
known-empty *by construction* rather than by ruling, and
`CwEmissionGateTests.Noise` generates it deterministically at any seed and
level. It is a legitimate lower bound on the problem and it is weaker evidence
than a recording (HM-DEC-091): real band noise carries other stations, carriers
and splatter that white noise does not. **So a floor that fails against
synthesized noise has certainly failed; a floor that passes against it has not
yet been tested against a band.**

## Not recordings

`cases-2026-08-19`, `cases-2026-08-24` and `cases-2026-08-26` are case sets, not
captures, and have no audio.

---

## Candidates for an empty corpus — evidence for Tim to rule on

**Written 2026-08-27, work instruction 034 task 6. Nothing here adjudicates
anything.** The empty list above is two recordings and an acquisition floor needs
more; this is the evidence a human would use, gathered so that ruling on it is a
short job rather than a session.

**READ THE CAVEAT FIRST, BECAUSE IT DECIDES HOW MUCH THE TABLE IS WORTH.**
`cw-2026-08-24-012403` is an **adjudicated anchor** holding `DE KD0UN KD0UN K`,
and its own sidecar reads:

> `keying   no keying at 425 Hz, 6 ms key down, 17 dB swing`
> `inThis   0 characters emitted` · `text  nothing read`

**On every axis this table has, a recording with a ruled-on callsign in it is
indistinguishable from a recording holding nothing.** So the ranking below sorts
by evidence that is known to be wrong at least once, and a recording near the top
of it is a candidate for Tim's ear, never a conclusion.

**The one axis that separates**, where it is present, is **how long the key was
down**. Morse at twenty words a minute has a 60 ms dit; the meter's `no keying`
verdicts sit at 4 to 7 ms, which is noise crossing a threshold rather than
anybody sending. Both known-empty recordings **predate the keying meter and carry
no `keying` line at all**, so they cannot be compared on the axis that would
matter most.

| recording | meter | key down | swing | read in this file | note |
|---|---|---|---|---|---|
| `cw-2026-08-25-012748` | no keying | 6 ms | 16 dB | **2** | strongest candidate: almost nothing read and no plausible element length |
| `cw-2026-08-22-014113` | no keying | 5 ms | 16 dB | **0** | **the operator hears a station here.** Listed to show the table's limit |
| `cw-2026-08-22-014308` | no keying | 4 ms | 15 dB | **0** | the operator hears a station here too |
| `cw-2026-08-24-012403` | no keying | 6 ms | 17 dB | **0** | **adjudicated: `DE KD0UN KD0UN K`.** The disproof of this ranking |
| `cw-2026-08-23-001831` | keying | — | — | **0** | keying held through a quiet stretch |
| `cw-2026-08-25-013150` | no keying | 5 ms | 18 dB | 36 | reads only single elements |
| `cw-2026-08-25-021629` | no keying | 4 ms | 17 dB | 77 | reads 77 characters, none of them words |
| `cw-2026-08-25-021825` | no keying | 5 ms | 19 dB | 63 | but floors of 41/74/16 are recorded against it |
| `cw-2026-08-25-013010` | no keying | 7 ms | 19 dB | 55 | same session and text as `013303`, which reads English |
| `cw-2026-08-25-012823` | no keying | 6 ms | 15 dB | 39 | the operator hears a station here |
| `cw-2026-08-25-021410` | no keying | 7 ms | 17 dB | 37 | reads `HAM FEST THIS EAST ATEEKEND` — holds a station |
| `cw-2026-08-23-001952` | no keying | 6 ms | 18 dB | 50 | reads `AA3FN` — holds a station |
| `cw-2026-08-26-125941` | no keying | **37 ms** | 16 dB | **0** | **the meter says no keying at a plausible dit length.** The operator hears it |

**Everything below this line holds a station on the meter's own verdict** and is
listed only for completeness: `031838`, `031905`, `031948`, `032012`, `032050`,
`032113`, `032129` (the W1AW bulletin run), `001520` (the operator's own
transmission, `CQ CQ DE KC3QIS`), `002016`, `011552`, `012922`, `013303`,
`013402`, `013520`, `013637`. `003016`, `003126` and `003758` carry no keying
line.

**What this list is actually good for.** One recording, `cw-2026-08-25-012748`,
reads two characters in thirty seconds with no plausible element length and no
station anybody has claimed. **That is one candidate, not a corpus**, and the
honest summary is that the recorded evidence cannot build the corpus an
acquisition floor needs. **The evidence that would is the operator's ear on a
recording, which is a minute of listening per file.**

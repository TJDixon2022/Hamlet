# ADJUDICATED truth for the seven W1AW captures

**Status: ADJUDICATED by Tim on 2026-08-25**, under work instruction 012, which
quotes his ruling: *"I think we can do them all. Why not? … Night's coming. I
wanna be prepared."* Tests may treat the quoted text below as truth.

**The ruling has no id yet.** The session does not mint them (§12.1), and the
report for this unit asks Tim to enter it in the decision log. Until he does,
this file and that report are the whole record of it.

**The filename still says PROPOSED and the folder still says `unadjudicated`,
and neither was changed.** Renaming would break the references in unit 1.11.8's
committed report and in the commit messages that carry it, which is a worse
record than a stale name. The header is the authority; the name is history.

**What is adjudicated and what is not.** Only the quoted words in the table are
truth. A `…` marks a span inside a capture that is **not** adjudicated, and a
`—` marks a word cut by the edge of the capture. Nothing outside the quotes is
asserted by anybody.

**Provenance, unchanged from the proposal.** ARLP034 was never published — the
ARRL archive stops at ARLP033 (2026-08-14) — so these texts rest on three legs
instead: (1) an independent decode of the WAVs by a third chain, separate from
both Hamlet and the shack analysis chain; (2) **cross-capture overlap** —
consecutive captures read the same words at their seams, e.g. `031948` ends
`…MEAN OF 117` and `032012` opens `…N OF 117`; (3) the audio names itself:
`032129` carries `…PROPAGATION FORECAST BULLETIN ARLP034`. All seven:
machine-keyed, 17–19 WPM, carrier 499.8–499.9 Hz, standard ARRL
propagation-bulletin boilerplate.

| capture | adjudicated text (— marks a word cut by the capture edge) |
|---|---|
| `cw-2026-08-22-031838` | `…2, 2, AND 2 WITH A MEAN OF 2.9. PRE—` |
| `cw-2026-08-22-031905` | `—DICTED 10.7 CENTIMETER FLUX IS 125, 125—` |
| `cw-2026-08-22-031948` | `…110, 110, AND 110 WITH A MEAN OF 117—` |
| `cw-2026-08-22-032012` | `—N OF 117. LINKS TO ARTICLES OR OTHER WEBSITES MENTI—` |
| `cw-2026-08-22-032050` | `—THIS BULLETIN CAN BE FOUND IN TELEPRINTER, PACKET, AND INTE—` |
| `cw-2026-08-22-032113` | `—ACKET, AND INTERNET VERSIONS … 2026 PROPAGATION FOR—` |
| `cw-2026-08-22-032129` | `…2026 PROPAGATION FORECAST BULLETIN ARLP034` |

Numbers (125, 110, 117, 2.9) were read identically by at least two independent
chains.

---

## What the decoder does with this today, and why no test asserts a whole line

**Not one of these seven lines is read whole**, and the tests written against
this file say so rather than pretending otherwise. `TheAdjudicatedReadingsKeepReading`
asserts, for each capture, **the longest unbroken run of the adjudicated text the
decoder produced on the day the anchor was set** — a floor on a success, which is
the thing this suite has never had.

Measured 2026-08-25 through the settled pass, started at 600 Hz because that is
what the operator's radio was set to on every one of these captures and what
`MainWindowViewModel` hands the decoder:

| capture | adjudicated characters | longest run read | share |
|---|---|---|---|
| `cw-2026-08-22-031948` | 36 | 32 | 89 % |
| `cw-2026-08-22-032012` | 51 | 22 | 43 % |
| `cw-2026-08-22-032050` | 58 | 17 | 29 % |
| `cw-2026-08-22-031905` | 39 | 12 | 31 % |
| `cw-2026-08-22-032129` | 41 | 10 | 24 % |
| `cw-2026-08-22-031838` | 34 | 6 | 18 % |
| `cw-2026-08-22-032113` | 28 | 4 | 14 % |

**The starting pitch changes these numbers more than anything else measured**,
and it is why the anchors are pinned to 600 rather than to each station's own
note. Started instead at the pitch each capture's sidecar recorded the station
at, `032113` goes from 4 characters to 22, `032012` from 22 to 43, `032050` from
17 to 24 and `031838` from 6 to 8, while `031905` falls from 12 to 7 and `032129`
from 10 to 7. **The anchors record what the operator would actually see**, and
the difference is a finding in its own right rather than a knob to turn.

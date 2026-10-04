# What the log can tell you about a mode — work instruction 287, task 1

**Reading only. Nothing was built for this.**

---

## The answer in one line

**One of the six can be earned today: FT8.** The other five cannot, and **they cannot
for four different reasons**, which matters because the card has to say which.

| Mode | ADIF says | Can Hamlet send it? | Can a record say it? | Earnable today? |
|---|---|---|---|---|
| **FT8** | Mode `FT8` | **yes** | **yes** | **yes** |
| CW | Mode `CW` | **yes** — HM-DEC-059 | **no — nothing writes a CW record** | no |
| Voice | **not a mode at all** | no | as `SSB`, `AM` or `FM` | no |
| FT4 | **Submode** of `MFSK` | no | **no — Hamlet writes no `SUBMODE`** | no |
| PSK31 | **Submode** of `PSK` | no | **no — Hamlet writes no `SUBMODE`** | no |
| WSPR | Mode `WSPR` | no | syntactically yes | **no — and not ever, from a log** |

---

## The instruction is wrong about CW, and it changes what the card says

**The order states: *Only FT8 can transmit; CW, FT4, PSK31 and Voice have no send
path.*** That is right about FT4, PSK31 and Voice and **wrong about CW.**

HM-DEC-059 built CW sending in August and it is still in the tree:
`src/Hamlet.RadioEngine/Cw/ICwSender.cs`, `CwTransmitter.cs`, the keyer sender behind
CI-V `17` (`CivConstants.CmdSendCwMessage`), and the app attaches it at
`src/Hamlet.App/ViewModels/MainWindowViewModel.cs:7791`:

```csharp
Transmit.Attach(new CwTransmitter(new KeyerCwSender(rig)));
```

**So CW is not awaiting a send path. It is awaiting a way into the log**, which is a
different sentence and a different promise. Reported, not repaired — the instruction is
not a session's to edit.

---

## What `MODE` actually holds

**One value, and it is a literal.** `MainWindowViewModel.cs:10132` hands
`Ft8StationConditions` the string `"FT8"`:

```csharp
new Ft8StationConditions(
    hz,
    band?.Name,
    "FT8",
    _settings.Operator.GridSquare));
```

`AdifLog.cs:243` writes it out as `<MODE:3>FT8`, and `AdifLog.cs:442` reads it back.

**And there is exactly one way into the log.** `LogContactAsync` takes a
`DigitalDecodeRow` — the right-click on a line of decoded FT8 — and nothing else calls
`ContactLogStore.Append`. **The mode is not editable in the dialog either**;
`LogContactViewModel`'s own remarks say so in as many words: *the observed fields are
not editable here at all, and his words go in `COMMENT` and nowhere else.*

So **every record Hamlet can write says `FT8`**, and the five other rows on the card
would be matching against a value the application has no way to produce.

**That is what makes CW its own case.** He can work a station on the key tonight, with
Hamlet keying the radio, and **there is nowhere for that contact to be written down.**
A CW row saying *not yet done* would be a promise the application cannot keep.

---

## The ADIF spellings, cited

Read from the **ADIF Specification, version 3.1.4, released 6 December 2022**, at
`https://www.adif.org/314/ADIF_314.htm`, retrieved **2026-09-08** — the same edition and
the same URL `AdifLog`'s own remarks already cite, so there is one source here and not
two.

| Hamlet's name | ADIF | Notes from the enumeration |
|---|---|---|
| CW | **Mode `CW`** | carries a submode `PCW`, which is not this |
| FT8 | **Mode `FT8`** | no submodes listed |
| WSPR | **Mode `WSPR`** | no submodes listed |
| FT4 | **Submode `FT4`, of Mode `MFSK`** | a record is `MODE=MFSK` + `SUBMODE=FT4` |
| PSK31 | **Submode `PSK31`, of Mode `PSK`** | a record is `MODE=PSK` + `SUBMODE=PSK31` |
| Voice | **does not exist** | the Mode enumeration has `SSB` (submodes `LSB`, `USB`), `AM` and `FM`. `DIGITALVOICE` exists and is a different thing |

**Three of the six are not what they look like**, and a first that matched on the
obvious string would never fire:

- **`MODE=FT4` is not valid ADIF.** FT4 is a submode of MFSK.
- **`MODE=PSK31` is not valid ADIF.** PSK31 is a submode of PSK.
- **`MODE=Voice` is not valid ADIF.** *Voice* is Hamlet's own family name — the tree
  spells it `ModeFamily.Phone` at `src/Hamlet.RadioEngine/Explore/ModeFamily.cs:26` —
  and the modes underneath it are `SSB`, `AM` and `FM`.

**This is the trap the order named.** A row that looks earnable and never fires is worse
than one that is missing, because nobody goes looking for it.

---

## The second obstacle, which the instruction does not mention

**`AdifContact` has no `SUBMODE` field.** `AdifLog` writes `CALL`, `STATION_CALLSIGN`,
`QSO_DATE`, `TIME_ON`, `TIME_OFF`, `BAND`, `MODE`, `FREQ`, `GRIDSQUARE`,
`MY_GRIDSQUARE`, `RST_SENT`, `RST_RCVD` and `COMMENT`. There is no submode anywhere in
the type, the writer or the reader.

So **FT4 and PSK31 are not merely waiting on a send path — they are unrepresentable in
the record as it stands.** Even given a transmitter for them, a contact would be written
as `MODE=MFSK` or `MODE=PSK` with the distinguishing half thrown away, and two different
modes would collapse onto one row.

**Reported, not repaired.** The log's fields are parked by this order.

---

## WSPR: a record can exist and the achievement cannot

**Syntactically a record can say it.** `WSPR` is a Mode in the 3.1.4 enumeration, so
`<MODE:4>WSPR` is a legal ADIF QSO record and any logger would read it back.

**And it would be a false record.** WSPR is a beacon protocol: a station transmits a
two-minute beacon carrying its callsign, grid and power, and receivers that decode it
report *spots* to a database. **Nobody works anybody.** There is no addressee, no
exchange, no report passed between two stations, and no moment at which two operators
agreed they had made contact. A QSO record for it would name a station he never worked.

**So a *first WSPR contact* cannot fire from a contact log, ever** — not because a
feature is missing but because the thing it would count does not exist.

**What would have to be measured instead:** spots of his own callsign, fetched from
`wsprnet.org`. That is the shape HM-DEC-075 already built for the skimmer watch — *who
heard me*, not *who did I work*. **It is a different achievement with a different
source, and it is a ruling.** Named and left (task 4).

**Hamlet's own tree already agrees WSPR is not a contact mode.** It appears in
`DigitalModeChip` as a place to tune, and `DigitalCallingFrequencies` records that the
cited data carries **WSPR on no band at all** — so there is not even a dial for it.

---

## What Hamlet can and cannot work

Confirmed by reading rather than assumed:

| Mode | In the tree |
|---|---|
| **FT8** | `Ft8Composer`, `Ft8TransmitSequence` — composes, keys, and its decoded rows are the one route into the log |
| **CW** | `ICwSender`, `KeyerCwSender`, `CwTransmitter`, `TransmitAbort` — **a complete send path** |
| **FT4** | a chip in `DigitalModeChip` and rows in `DigitalCallingFrequencies`. **A dial, not a mode**: no decoder, no composer |
| **PSK31** | the same — a dial and a fingerprint drawing |
| **WSPR** | a chip with no cited frequency on any band |
| **Voice** | nothing. Hamlet has no voice path of any kind |

**A mode he has not been given the means to work is not a mode he has failed to work**,
and the card must not draw those the same way. **Nor is a mode he can work and cannot
record** — which is CW, and which the card has to say in its own words rather than
borrowing one of the other two sentences.

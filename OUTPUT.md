# OUTPUT.md

## 1. What Claude did

### Task 1: the four fixtures, and they have one cause

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093). The separation test is in the tree and `Refine` is not, as
the instruction states.

With `Refine` applied:

| fixture | key | what it reads |
|---|---|---|
| `clean-12wpm` | correct text | **the text is right**; two of nine characters come out `Low` instead of `High` |
| `clean-18wpm` | 18 words a minute | **16**, outside the ±1 the test allows |
| `prosigns-edge` | copies or refuses | **3 emitted, 2 not in the message** |
| the bulletin's words | `AT ARRL DOT NET <BT> EACH…` | word spacing wrong, the substring not found |

**Only one of the four gets a letter wrong.** `clean-12wpm`'s transcript is
correct and fails on confidence; `clean-18wpm` reads the right text at the wrong
speed. That is the shape of a timing error rather than four separate faults.

**The dit, before and after, wherever the truth is known:**

| audio | true dit | with `Refine` | without |
|---|---|---|---|
| `clean-12wpm` | 100.0 ms | **100.6 (+1%)** | 105.0 (+5%) |
| `clean-18wpm` | 66.7 | **65.9 (−1%)** | 65 to 75 (−3% to +12%) |
| `prosigns-18wpm` | 66.7 | **67.5 (+1%)** | 70.0 (+5%) |
| `coverage-easy` | 100.0 | **99.8 (−0%)** | 100.0 (0%) |
| `exchange-easy` | 100.0 | **99.8 (−0%)** | 100.0 (0%) |
| `prosigns-easy` | 100.0 | **99.4 (−1%)** | 100.0 (0%) |
| `prosigns-edge` | 100.0 | no characters | 115.0 (+15%) |
| `fast-easy` | 48.0 | **47.9 (−0%)** | 45.0 (−6%) |
| `tightfist-easy` | 88.0 | 95.3 (+8%) | 95.0 (+8%) |
| **`cw-2026-08-17-134712`**, `N4L` | **56.3** | **no characters** | **55.0 (−2%)** |
| **`cw-2026-08-17-013347`**, `VA3VRR` | **100.4** | **96.1 (−4%)** | **100.0 (−0%)** |

**The cause, in one sentence: `Refine` treats the sender's element gap as a second
measurement of the dit, and it is one only when the sender sends textbook
spacing.**

Everything follows from that. On the synthesized fixtures the element gap *is* a
dit, so the mark's small overshoot and the gap's matching shortfall cancel and the
average lands within one percent every time. Take it away and those same fixtures
drift 5 to 15 percent, which is enough to move a confidence score, a word boundary
and a words-a-minute reading — the four failures, from one number.

**And on the two recordings whose dit is known rather than estimated, it is exactly
the other way round.** Without `Refine`, `N4L` reads 55.0 against 56.3 and
`VA3VRR` reads 100.0 against 100.4 — both inside two percent. With it, `VA3VRR`
reads 96.1 and `N4L` reads nothing at all.

**Both adjudicated stations are Farnsworth** and that is why. `N4L` sends a 35.6 ms
element gap on a 56.3 ms dit and `VA3VRR` sends 73.3 on 100.4 (HM-DEC-144,
HM-DEC-145). Their gaps are two thirds and three quarters of a dit. **Every
synthesized fixture in the suite sends one.**

### Task 2 did not run, and here is why

**The order requires the fix to be fitted and forbids a constant, and every way I
can construct to tell the two cases apart needs one.**

The decision the fix has to make is whether this sender's element gap is a dit or
shorter. The measured ratios are 0.90 on the clean fixtures against 0.63 for `N4L`
and 0.73 for `VA3VRR`, and any rule that acts on that is a threshold on gap over
dit. **I tried the alternative the file already supplies** — average only when the
gap mean lies inside the mark cluster's own scatter, which is the shape of test
HM-DEC-095 validated — and it fails immediately: the marks on a clean fixture
scatter two or three milliseconds while the gap sits ten below the mark-derived
dit, so it would refuse to average on exactly the audio where averaging is right.

**I also looked for a correction that needs no decision at all**, on HM-DEC-119's
finding that the gate reads 100 to 110 ms for a true 100. If the overshoot were a
fixed amount the dit could simply be shortened by it. Measured, it is not: without
`Refine` the dit reads +5% on `clean-12wpm`, **−6% on `fast-easy`** and −2% on
`N4L`. There is no consistent overshoot to subtract.

**So Task 2 stopped rather than tuning a threshold**, which is the error class six
rulings have gone on closing and which this order names as the seventh.

**One mismatch with the instruction, reported rather than repaired.** It states
that HM-DEC-119 measured the mark as "not long, so there is nothing to cancel".
HM-DEC-119's own figures are 100 to 110 ms for a true 100, 45 to 50 for a true 48,
40 to 45 for a true 40 — **long by nought to ten per cent, not zero**. That is a
real bias, it is what `Refine` cancels on textbook audio, and the premise is half
true rather than false.

### Task 3: the fixture

`ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt` **stays red**
and nothing was tuned. `cw-2026-08-17-134712` emits nothing with the tree as it
stands. Where it dies is unchanged from last session: with `Refine` in place its
dit reads 31.3 ms against a known 56.3, and `LooksLikeMorse` never holds long
enough for a character boundary to fall inside it.

### Task 4: the bulletin

`CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`, standing red since
HM-DEC-114 left it deliberately, currently reads:

```
got    'NL DOT NET ■ ECH STATION HANDNG AHIS MESAGE P'
wanted 'AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P'
```

**36 characters against 47.** Nothing was changed and nothing it asserts was
touched.

## 2. What Tim should expect

**No. `Refine` did not ship, and the decoder still does not read
`cw-2026-08-17-134712`.** Nothing in `src/` changed this session.

**What you have is the number that settles the argument.** Without `Refine`, the
two recordings whose dit this project actually knows read **55.0 against 56.3** and
**100.0 against 100.4** — inside two percent, both of them. With `Refine`, one
reads 96.1 and the other reads nothing.

**And the four fixtures blocking it are all textbook senders.** They send an
element gap of exactly one dit, which is what makes `Refine`'s average work on
them. **Both real stations this project has adjudicated send two thirds and three
quarters of a dit**, which is what HM-DEC-115 measured off the air in the first
place and is how people actually send.

**So the question is no longer whether `Refine` is right.** It is whether the
suite's synthesized fixtures are asserting a world that HM-DEC-115 says does not
exist on the air, and that is the ask in section 4.

**Build clean, no warnings. 2,108 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

No tests were added and none changed. `ShortestVote` is still 5, `MaximumRatio` is
still 3.8, the separation figure was not touched, and no gate was built.

## 3. What we should do next

- **Rule on the fixtures.** Every synthesized recording in the suite sends textbook
  spacing, and neither station this project has proved on the air does. That is
  one ruling and `Refine` follows from it either way.
- **If the fixtures are regenerated as Farnsworth**, `Refine` can be removed and
  the dit will read true on both real recordings and on the new fixtures alike.
- **If they stand**, then the decoder needs to tell textbook from Farnsworth, and
  that needs a criterion nobody has found without a threshold. **Say so in the
  order**, because the next session will otherwise spend itself rediscovering that
  this one stopped for a reason.
- **Adjudicate `cw-2026-08-18-004507`.** It is the third real recording with a lot
  of text in it and it would be a third fist to measure against.

## 4. What's blocking us

**`Refine` is blocked on one ruling, and it is about the fixtures rather than the
code.**

**One ask, new this session.**

> **Every synthesized fixture in this suite sends an element gap of exactly one
> dit, and no station this project has measured on the air does. Whether they
> should be regenerated as Farnsworth is the ruling `Refine` is waiting on.**
>
> `Refine` averages the mark-derived dit with the gap-derived one. That works
> perfectly where the element gap is a dit, which is every fixture: it holds
> `clean-12wpm`, `clean-18wpm`, `prosigns-18wpm`, `coverage-easy`,
> `exchange-easy`, `prosigns-easy` and `fast-easy` inside one percent of their true
> dit. Removing it takes those to five, twelve, five, nought, nought, nought and
> minus six.
>
> **On the two recordings whose dit is known it is the reverse.** `N4L` sends a
> 35.6 ms element gap on a 56.3 ms dit and `VA3VRR` sends 73.3 on 100.4. Without
> `Refine` they read 55.0 and 100.0; with it, 96.1 and nothing.
>
> **HM-DEC-115 already ruled on which of those is the real world**, having measured
> a bulletin off the air whose element gap was 40 ms against a 57 ms dit: "nothing
> about 1:3:7 survives contact with a traffic net". **The fixtures were built
> before that ruling and still send 1:3:7.**
>
> **Rejected: a threshold on the gap-to-dit ratio** to switch the averaging on and
> off. The measured values are 0.90 against 0.63 and 0.73, which would separate,
> and it is exactly the constant six rulings have gone on closing. **Also
> rejected: subtracting a fixed overshoot** instead, which HM-DEC-119's own figures
> would support and the measurement does not: without `Refine` the error is +5% on
> `clean-12wpm` and −6% on `fast-easy`, so there is nothing consistent to subtract.

### Asks still outstanding

- **Whether the synthesized fixtures should be regenerated as Farnsworth.** First
  made 2026-08-20, this session. `Refine` waits behind it and nothing is in the
  tree.
- **The keying meter's provisional thresholds.** First made 2026-08-20. Waiting on
  one evening's roster scored against the `meter` column.
- **Whether `SHACK_FACTS.md` still holds that CI-V Transceive is off.** First made
  2026-08-20. Waiting on one capture taken with the radio connected, so the
  `broadcast` line has something to report. The change is in the tree at
  `MainWindowViewModel.BroadcastDuringCapture` and `CivLinkHealth.LastTransceiveUtc`.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam between two sends measured into the
  dummy load. `CwMessage.Split` already exists and nothing calls it for this.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load, including the link pulled
  mid-cycle. The cycle is built and is dummy-load only.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it. Waiting on its own
  work order.
- **HM-OPEN-007.** Open and unruled since 2026-08-14, named in HM-DEC-140 as the
  reason the queue's own premise is worth re-testing. Waiting on Tim.

**One item leaves the queue.** Whether two lost characters were an acceptable price
for one not invented: ruled this session, and the separation test stands.

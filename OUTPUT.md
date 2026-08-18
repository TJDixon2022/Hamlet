# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
The prompt claimed `PROJECT: Hamlet` and the tree confirms it: `CLAUDE.md`'s
header reads `Project: Hamlet`, the solution is `Hamlet.sln`, the namespaces are
`Hamlet.*`. Gate passed. **Nothing in this report is evidence about the radio**
(`SHACK_FACTS.md`, HM-DEC-093). COM1 is a simulator, so every number below comes
from a fixture, a generated signal, the training radio, or the eCFR.

**Nothing was recorded under §12.1.** Two questions came up and both weigh costs
against each other, so both are in section 4.

**All six phases were worked. Phase 6 was not dropped.**

**One thing you should know before the rest.** Something outside this session
committed my phase 1 work as `20c8ae5 "save"` and the branch is
`feature/honest-cw-detection`, not `main`. The last report said fourteen commits
were sitting local on `main`; both halves were wrong. They are on the feature
branch, they were already pushed, and **`main` does not contain the last three
sessions at all** — it is still at `5bada83 "more waves"`. Everything this
session did is now pushed to `origin/feature/honest-cw-detection`. Nothing was
merged, which is right: HM-OPEN-016 blocks that branch at severity `hard`.

## Phase 1, the third confidence measurement, and what it found instead

HM-DEC-108 is built. `CwSettledPass.BoundaryMargin` measures how far the gap
that ended a character sat from the boundary it was judged against, on the same
scale the mark measurement uses, and the worst of the three now wins. It is
bounded at one so it can only ever lower a score. Both boundaries of a character
count rather than only the closing one, which is a reading of the ruling rather
than its literal words: one gap misjudged makes two characters, and scoring only
the closing gap marks the half in front and leaves the half behind, and the half
behind is the lone dah the ruling names.

**It moved the numbers not at all, and finding out why is the useful half.**
Every stranger scored a boundary margin of exactly one. The gaps around them
were decisively wide, so the ruling's stated cause does not hold on these
fixtures.

Tracing what was actually emitted found the real one. **Each stranger was the
leading dashes of the character that followed it** — a lone dah before `D` and
before `N`, four dashes before a nought — with the real character arriving whole
right behind it. The gap after a window's last mark was infinity, which asserts
the character certainly ended there, and a window has no business asserting it:
a window is a view onto a stream, so silence it has not seen yet is silence
nobody has measured.

It is measured now, and a character whose end the window did not see is held for
the next window where it sits in the interior. That is phase 4's own remedy for
the mark-at-the-edge case, applied to the silence afterwards that nothing was
watching.

**Strangers went from two of eight to none on `coverage-easy`, and one of seven
to none on `exchange-easy`.** The ratchets tighten to zero. The cost is real:
five characters come out where eight and seven did, because a fragment is no
longer published as a character. Both halves were checked by breaking them.

## Phase 2, the frequency sweep

HM-DEC-109 is built. The frequency joins Mode and FilterSelection on the session
poll; the test that pinned the old rule is replaced by one proving the sweep,
and a new test moves a radio without announcing it and watches the model catch
up.

**The separate staleness rule went and there was nothing to delete.** Freshness
is derived generically from the poll rate, so as a never-polled field the
frequency was compared against a window nothing ever refreshed. Swept, its age
means what everything else's means.

**Of the two on-demand reads, one stays and one goes.** Before a capture stays:
a sidecar is evidence read months later and one command at a button press makes
the header exact. Before a spot refresh goes: it closed a consequence rather
than a cause, and the sweep is always the fresher of the two anyway, since spots
refresh every one to fifteen minutes against the sweep's thirty seconds.

**The downstream question is answered from your telemetry rather than assumed.**
A wrong band did reach the spot sources: session `af471e84`, seven spot
refreshes on the remembered band before the radio was asked, then the band
moved. **It was the training radio, and no session with a real radio shows it.**
That is the startup ordering rather than a dropped broadcast, and it
self-corrects because a band change reloads spots. Defect report in section 4.

## Phase 3, `BandPlan` retired

`HfBands` replaces it and holds no frequency literal, which a test enforces by
grepping its own source. Band edges from 97.301(b), CW segments from the union
of the data-carrying ranges in 97.305(c), jump spots from the first "CW main
street" block in the cited conventions. Every caller re-pointed, `BandPlan.cs`
deleted, HM-OPEN-005 closed.

**The cited data was verified against the regulation itself first**, from the
eCFR versioner API for title 47 as of 2026-08-01 rather than from the file that
quotes it. All fourteen numbers matched. **Column-awareness earned its keep
twice**: 97.301's tables carry ITU Regions 1, 2 and 3 side by side and the
United States is Region 2, so reading Region 1 would have given 40 m as 7.000 to
7.200 and 75 m as 3.600 to 3.800; and a naive search for paragraph (b) lands
first on a footnote reference inside a table cell.

Two dials moved and both are named in a test: 40 m from 7.030 to 7.028, and 30 m
from 10.110 — which matched no cited source at all — to 10.103.

HM-OPEN-005's own claim that the CW segments are convention rather than
regulation is corrected in the record rather than dropped. They align to the
hertz.

## Phase 4, the scanner end to end, which found a real fault

**It found one, which is the answer the brief hoped for.**

A radio announces a frequency change whoever caused it, including Hamlet. The
scanner recorded where it had put the dial *after* issuing the tune, so the echo
of its own command arrived while the write was still in flight, was compared
against the previous stop, did not match, and was read as a hand on the knob.
**Every scan aborted on its second tune saying the operator had touched the dial
when nobody had**, and left the dial where the scan had got to rather than
putting it back.

It survived a whole session of unit tests because the stub radio raised the
event only when a person turned it, which is not how a radio behaves. That is
§12.5 again: a stub better behaved than the thing it stands in for proves
nothing. The stub now announces every change and fails alongside the end-to-end
test when the ordering is put back.

The harness itself answers the brief's four questions. The survey ranks sixteen
candidates off the training radio's own synthesizer and no carrier outranks an
operator. The dwell reaches the real decoder and its verdict carries a
confidence out. The dial comes back on all three exit routes, including the app
dying mid-scan with the note left on disk. A dwell that found nobody still says
where it was.

## Phase 5, the nine failures attributed

Each was tested rather than reasoned about, by giving the decoder a run-up of
Morse before the message and seeing whether the message then decoded.

**Six are the fixtures**, all the same fault: the signal is too short for a
detector that wants three seconds of keying before it moves.

| Test | Bare | With a run-up |
|---|---|---|
| wrong pitch, 500 Hz | `■■EIW K` | `CQ DE W1AW K`, exactly |
| wrong pitch, 750 Hz | `■ ■ ■ ■ AW K` | `CQ DE W1AW K`, exactly |
| wrong pitch, 875 Hz | `■ DE W1AW K` | `CQ DE W1AW K`, exactly |
| clean, 25 wpm | `CQ D■ W1AW K` | `CQ DE W1AW K`, exactly |
| fade recovery | 0 letters in the last third | 7, against the 3 it asks for |
| speed after a change | 10 characters, 24 wpm | 10 characters, 24 wpm |

**The pitch is found to the hertz in every one, including the failures.** The
test that fails is the text, not the pitch it is named for. And the speed one is
not even that: its estimate lands at 24 wpm, inside the range demanded, and
exactly ten characters arrive after the change, whereupon the test skips ten and
asserts the remainder is not empty. It fails on an off-by-one in its own margin.

**Three are Hamlet, and they are two faults.** At 400 Hz the tracker finds the
pitch and will not hold it, breaking down and re-acquiring however long the
signal — the only pitch a run-up does not fix. The app transcript test is the
same thing from the other end: at 12 words a minute a longer signal decodes
worse than a short one. And the sensitivity one is a ruling that was never
built, covered in section 4.

Nothing was loosened and no bound was moved.

## Phase 6, pushed and measured

Four commits pushed to `origin/feature/honest-cw-detection`, then a fifth. Over
a four-hour evening the frequency sweep costs **480 reads and about one second
of wire time: 0.13 percent of the traffic and 0.007 percent of the bus.** The
whole poll loop is 25 reads a second and 5 percent of the wire, nearly all of it
the S-meter and the transmit line. Anybody worrying about what HM-DEC-109 cost
is looking in the wrong place by three orders of magnitude.

**No transmit work of any kind was done and nothing was built toward auto-CQ.**

# 2. What Tim should expect

- **Build succeeds, no warnings**, engine and app.
- **1787 tests, 9 failing.** 1364 of 1372 pass in the engine, 414 of 415 in the
  app. 34 tests added.
- **The same nine as the last three reports**, now each attributed. Nothing
  regressed and nothing new appeared.
- **What you will see that is different.** Band buttons for 40 m and 30 m land
  on 7.028 and 10.103 rather than 7.030 and 10.110. That is HM-DEC-110 and it is
  the only visible behavior change in this session.
- **What will look wrong and is not.** The settled transcript now shows *fewer*
  characters on the two gap fixtures, five where there were eight and seven.
  That is the fix: a fragment of a character is no longer published as a
  character. Fewer and right beats more and wrong.
- **You are on `feature/honest-cw-detection`, not `main`.** Everything from the
  last three sessions lives there and `main` has none of it. That branch is
  blocked from merging by HM-OPEN-016 at severity `hard`, and phase 5 has now
  attributed all nine of its remaining failures, so the merge question is a
  ruling rather than a mystery.
- **`HM-OPEN-024` is new and minor**: three band-card style tests failed once in
  a combined run and have not failed in five runs since. Suspicion is the band
  plan going lazy under parallel test collections. Recorded rather than chased.
- **Everything is pushed.** Nothing is sitting on the machine.

# 3. What we should do next

- Rule on merging `feature/honest-cw-detection`, which is now the thing blocking
  the most. Six of its nine failures are fixture-length and one is a test's own
  off-by-one; only two are decoder faults and neither is a regression in
  behavior anybody would see on the air.
- Rebuild the six short fixtures with enough run-up for the detector. It is
  generator work, not decoder work, and it clears six of the nine in one pass.
- Rule on the sensitivity floor, section 4 item one. It is the only §0.0 gap
  left in the decoder.
- Chase the 400 Hz tracker, which is the one genuine decoder fault with no
  ruling in front of it.
- Run the scanner from the app against the training radio. The engine path is
  now exercised end to end, and the face built last session has still never been
  driven by a person.

# 4. What's blocking us

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1; HM-DEC-097; HM-DEC-088; HM-OPEN-016
---

**The decoder refuses to emit a character when its own measured margin is below
the value that corresponds to nought decibels broadband, and that value is
stated here.**

HM-DEC-097 already ruled the refusal: below 0 dB the decoder goes quiet rather
than copying into the band where it is half wrong. **It was never built.** The
sweep reproduces that ruling's own published figures exactly — perfect from
18 dB down to 1 dB, and at minus two decibels a full message of which 0.44 is
invented — because nothing in the decoder implements a floor. The streaming pass
gates on coherence and a plausible speed; the settled pass on six decibels of
contrast; neither is what the ruling describes.

**It cannot simply be added, because the ruling is stated in a unit the decoder
cannot measure.** HM-DEC-097's decibels are the broadband ratio the fixture was
generated at. The decoder measures inside a narrow tone filter and reads about
seventeen decibels higher for the same audio: 17.2 to 19.0 where the fixture was
generated at 0 dB, 15.3 to 17.1 at minus two, 7.6 to 14.4 at minus five.

So the number wanted is: what does the decoder's own margin have to fall below
before it stops emitting. Seventeen is the direct translation and would cut in
at the ruling's own line. Fifteen would let the minus-two case through, which is
the case the ruling names. Something near ten would only catch minus five, where
the decoder is already ragged.

The measurement above is what there is to reason from and it does not choose for
you, which is why this is here: the number decides what the display asserts, so
§12.1 puts it with you without exception.

Rejected: moving `ItGoesQuietRatherThanInventingLettersInTheNoise`'s bound of
0.35 to accommodate the 0.44. That is moving a bound to make a test pass, the
brief forbade it, and the bound is the ruling written down.

---
date: 2026-08-18
refs: CLAUDE.md §0.0.1; HM-DEC-024; HM-DEC-075; HM-DEC-109
---

**The first spot refresh waits for the radio to be asked where it is, rather
than running on the remembered band.**

A defect report rather than a fix, as the brief asked. **It has happened**:
session `af471e84`, seven spot refreshes on the band left in settings before the
radio was asked, and the band then moved. It was the training radio and no
session with a real radio shows it, so nothing has yet cost you anything
measurable.

The mechanism is startup ordering, not a dropped broadcast, so HM-DEC-109's
sweep does not close it. `ReloadSpotsAsync("startup")` runs from the view model's
constructor; the radio is not connected until the window's `Opened` event fires.
Until then the band is whatever `settings.LastBand` says, and the band scopes
what RBN is filtered to and what the skimmer watch listens for.

It self-corrects, which is why this is `none` rather than urgent: a band change
reloads spots, so the moment the radio answers, the wrong-band results are
replaced. What it costs is a burst of pointless network calls to somebody else's
service and a few seconds of a panel showing the wrong band's stations.

The trade-off is between that and an empty panel for the second or two before
the radio answers, which is why it is not a session's call. Rejected: asking the
radio from the constructor, which would put a serial read on the path that
builds the window.

---
date: 2026-08-18
refs: CLAUDE.md §7; HM-OPEN-016
---

**Whether `feature/honest-cw-detection` merges to `main` now, and what happens
to the fourteen commits sitting only on it.**

Not a defect, a fact you need. `main` is at `5bada83 "more waves"` and has none
of the last three sessions: the scanner, its face, the two-stage decode on
screen, the frequency sweep, the band plan retirement. All of it is on the
feature branch and all of it is pushed.

HM-OPEN-016 blocks that branch at severity `hard` until its failures "pass or
are shown to be wrong". Phase 5 has now shown six of the nine to be the
fixtures' fault, with the message decoding exactly once given a run-up, and one
more to be a test's own off-by-one. Two decoder faults remain and neither is
something that would be visible on the air: a 400 Hz tracker that will not hold,
and a degradation at 12 words a minute on a long repeated signal.

Also worth deciding: something on this machine is committing as `"save"` while a
session runs. It caught phase 1's work and threw away the commit message. It is
harmless to the content and it makes the history hard to read.

Rejected: merging on this session's own authority. HM-OPEN-016 is a `hard` block
and lifting it is a ruling.

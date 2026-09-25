@echo off
setlocal
rem ============================================================
rem  run-fixture.bat  -  prove progress is counted in criteria, that
rem                      the owner's verdict ends the run, that a
rem                      self-ruling cannot reverse an earlier one, and
rem                      that every attempt against a criterion is recorded
rem
rem      run-fixture.bat <arm>
rem
rem      0  the arm ended the way it must
rem      1  it did not - the verdict line says what differed
rem      2  usage
rem
rem  The real loop, run-phase.bat, against a fresh git root under %TEMP%,
rem  every claude call answered by the stand-in in this folder unless an
rem  arm asks for the real judge. Each unit takes about seventy seconds,
rem  because the watchdog's first look is at sixty.
rem
rem  064's ARMS - unchanged:
rem    advance        names step 2 criterion 3, the unit turns it to - [x]
rem                   MUST record ADVANCED: yes, and the loop continues
rem    noadvance      names it, the unit does not turn it
rem                   MUST record ADVANCED: no, and the loop continues
rem    twice          noadvance for three iterations
rem                   MUST NOT HALT. Three units, all ADVANCED no, the loop
rem                   redirecting after the second and reaching its backstop,
rem                   and a redirect note in the ledger.
rem                   REVERSED BY 070 TASK 5 on the owner-s ruling of
rem                   2026-09-14 that stopping is failure. It read:
rem                     "MUST HALT AT STOP 10 naming units 1 and 2 and step 2"
rem                   and asserted units -eq 2, adv no,no and the ledger line
rem                     "stop 10: no progress - units 1 and 2 moved no
rem                      criterion of step 2"
rem                   Stop 10 no longer fires at all; nothing jumps to it.
rem    closed         names step 2, which PHASE_OUTCOME.md records as done
rem                   MUST BE REFUSED before the unit runs - no UNIT call
rem    bad            ADVANCES names no criterion and is not the blocker form
rem                   MUST BE REFUSED before the unit runs - no UNIT call
rem    blocker        none - clears a blocker: unit 062's task 1
rem                   MUST record ADVANCED: blocker, and the loop continues
rem    blocker-twice  the blocker form for three iterations
rem                   MUST HALT naming that rule, and not stop 10, AND NOT BE
rem                   REDIRECTED EITHER. 070 leaves this halt alone: the
rem                   redirect-s requirement is an approach the record does not
rem                   show failing AT THAT CRITERION, and a blocker-clear names
rem                   no criterion, so there is nothing to send it back to.
rem                   The expectation gains "and not redirected"; the rest is
rem                   as 064 wrote it.
rem    transport      criteria-count.bat on the plan written CR-only with a
rem                   byte-order mark - MUST count 1 of 3 for step 2
rem
rem  065's ARMS - unchanged:
rem    owner-sheet     every unmet criterion is the owner's, the plan names a
rem                    REVIEW_SHEET, no --seed - MUST exit 0 at stop 1 waiting
rem                    on the owner, call nothing, and write the sheet
rem    owner-again     the same root run twice - MUST halt the same way both
rem                    times and leave the sheet byte-identical, write time too
rem    owner-nosheet   every unmet criterion is the owner's, no REVIEW_SHEET -
rem                    MUST exit 0 at stop 1 and write no sheet
rem    one-work        one work criterion left beside two owner's, no --seed -
rem                    MUST NOT halt: the arbiter authors, the unit runs
rem    owner-advances  ADVANCES names an owner's-verdict criterion
rem                    MUST BE REFUSED before the unit runs
rem    reversal        the unit's report claims ruling 72 reverses ruling 46,
rem                    put to the REAL state judge - MUST HALT, quoting it
rem    nine            the report lists nine implementation decisions and no
rem                    reversal, put to the REAL judge - MUST CONTINUE
rem    dupplan         plan-check.bat on a plan with criterion 2.2 twice
rem                    MUST REFUSE it, naming the duplicate
rem
rem  066's ARMS - the attempt record, read back by attempt-read.bat:
rem    attempt-one           one unit against 2.3, and it advances
rem                          MUST record one attempt: unit 1, yes, executed,
rem                          and the decision block's approach
rem    attempt-three         three units against 2.3 - no, yes, no, the
rem                          THREE APPROACHES DIFFER FROM 068: the stand-in
rem                          authors a new one per call, because a repeated
rem                          approach is now refused. The old expectation was
rem                          the SAME approach on all three attempts; the
rem                          second and third now carry what the arbiter
rem                          authored. Everything else is as 066 wrote it.
rem                          criterion flipped by the second
rem                          MUST record three attempts in that order with
rem                          those verdicts. The two no-advances are NOT
rem                          consecutive: two in a row are still stop 10 in
rem                          this unit, and turning that halt into a redirect
rem                          is step 3 of the plan, not this unit's
rem    attempt-restart       THE SECOND LOOP RUNS A DIFFERENT INSTRUCTION from
rem                          068 - WI-criterion-b.md, another route to the same
rem                          criterion - because repeating the first loop-s
rem                          approach is now refused. The old expectation had
rem                          both attempts carrying the same approach.
rem    attempt-restart       one unit, the loop exits, then a NEW loop over the
rem                          same root runs one more - criterion 1.3
rem                          MUST read the first process's attempt unchanged
rem                          after it has gone, and the second after it
rem    attempt-blocker-crit  none - clears a blocker: naming criterion 2.3
rem                          MUST record one attempt against 2.3, verdict
rem                          blocker - criterion 1.4
rem    attempt-blocker-unit  none - clears a blocker: unit 062's task 1
rem                          MUST record no attempt at all, and the loop
rem                          continues to its backstop
rem    attempt-transport     attempt-read.bat on one record written LF, CRLF,
rem                          CR-only and CR-only with a byte-order mark
rem                          MUST read the same three attempts from all four -
rem                          criterion 1.5, nice-to-pass
rem
rem  "Continues" is proved by the loop reaching its --max-iterations backstop
rem  rather than a stop condition. THE REAL JUDGE ARMS call one restricted,
rem  read-only claude -p each: a stand-in judge would prove only that the
rem  stand-in says what it was written to say.
rem
rem  071: TWO OF 066'S ARMS ARE REVERSED, AND THE OLD EXPECTATION IS QUOTED
rem  HERE WHERE A READER WILL FIND IT. Unit 071 has the launcher write a
rem  REASON: line beside every attempt it records as a CLOSED ROUTE, so
rem  attempt-read.bat returns a REASON_n after those ATTEMPT_n. The two arms
rem  that compare its whole output with -ceq therefore had to move.
rem
rem  THE OLD EXPECTATIONS, WORD FOR WORD:
rem
rem    attempt-three
rem      $e=@('CRITERION=2.3', 'ATTEMPTS=3',
rem           ('ATTEMPT_1=unit 1 | no | executed | ' + $ap),
rem           ('ATTEMPT_2=unit 2 | yes | executed | ' + $v1),
rem           ('ATTEMPT_3=unit 3 | no | executed | ' + $v2));
rem
rem    attempt-restart
rem      $e1=@('CRITERION=2.3', 'ATTEMPTS=1',
rem            ('ATTEMPT_1=unit 1 | no | executed | ' + $ap));
rem      ... ($a1.Count -eq 3) ... ($a2.Count -eq 4) ...
rem
rem  WHAT IS ADDED AND NOTHING ELSE CHANGED: a REASON_n after each attempt
rem  whose verdict is no and whose fate is executed. ATTEMPT_2 of
rem  attempt-three carries NO reason because its verdict is yes - a route
rem  that succeeded is not a closed one - and that is asserted by the
rem  ordering of the expected block rather than by a separate check.
rem  attempt-restart now also asserts that the REASON written before the
rem  restart reads back UNCHANGED after it, which is the arm-s whole subject.
rem
rem  attempt-one AND attempt-blocker-unit ARE UNCHANGED AND PASS UNCHANGED:
rem  one advances, so it is not a closed route, and the other records no
rem  attempt at all. attempt-blocker-crit IS ALSO UNCHANGED - a verdict of
rem  blocker is not a failed approach and gets no reason. That arm caught a
rem  REAL DEFECT in the first cut of the reader, which emitted an empty
rem  REASON_n for every attempt; it is a defect and not a reversal because it
rem  changed what the reader returns for records NOBODY HAD TOUCHED.
rem
rem  068 TASK 3: THE IDENTITY IS STRIPPED BEFORE 066'S ARMS COMPARE.
rem  The unit field now reads "unit 1 launched <UTC>", so an exact-string
rem  expectation written in 066 would fail against a value that cannot be
rem  predicted. The two lines above remove " launched <token>" from what the
rem  reader returned, and EVERY 066 EXPECTATION BELOW IS LEFT EXACTLY AS IT
rem  WAS WRITTEN - the arm still proves the attempts, their order, their
rem  verdicts and the approach whole. That the identity is there at all, and
rem  that it separates two units both called unit 1, is proved by 068's own
rem  restart arm instead, which reads the raw record.
rem
rem  THE UNIT NUMBER IS THE LOOP'S ITERATION. A restarted loop counts from 1
rem  again, so attempt-restart's second attempt reads unit 1 as its first
rem  does - the same number every outcome entry and stop 10's message carry.
rem  The order is the file's, and that is what the arm asserts.
rem
rem  067's ARMS - a unit's report cannot be another unit's, and a dead lock
rem  is not a lock:
rem    leftover      iteration 1 runs and is judged; the SECOND arbiter call
rem                  leaks a session lock held by a LIVE pid and drops unit
rem                  358-s report at the root, so iteration 2-s unit is
rem                  refused by the lock and NOTHING RUNS. HamLet on
rem                  2026-09-14, reproduced.
rem                  MUST halt at stop 11 with NO entry appended for
rem                  iteration 2 - ONE entry in the record, not two - and the
rem                  halt line must name UNIT: 358, the file it found
rem    lockleak      two iterations that both run. Proves the release on a
rem                  CLEAN exit: iteration 2 could not have taken the lock at
rem                  all if iteration 1 had kept it, and no SESSION.lock is
rem                  left behind when the loop ends
rem    lock-dead     the lock is taken before the loop by pid 999999, which
rem                  is not a process on this machine. MUST be cleared,
rem                  taken, run, and a ledger line must say an orphaned lock
rem                  was cleared and by which unit
rem    lock-live     the lock is taken before the loop by pid 4, the System
rem                  process. MUST refuse twice over - run-phase at its door
rem                  with nothing called, and run-unit.bat driven directly,
rem                  whose refusal must name pid 4 as FOUND RUNNING
rem    noreport      the unit writes no output.md at all
rem                  MUST halt at stop 11 with nothing appended
rem    kept          one unit that runs. MUST leave the judged report at
rem                  .run-unit\reports\unit-1-output.md, SHA256-identical to
rem                  what the unit wrote, AND THE ROOT output.md STILL THERE.
rem                  REVERSED BY 068 TASK 1, on the owner-s ruling of
rem                  2026-09-19. It read: and no output.md at the root.
rem    kept-twice    the destination already holds a unit-1-output.md from an
rem                  earlier loop over the same root. MUST lose neither - the
rem                  old file untouched, the new one beside it, AND THE ROOT
rem                  output.md STILL THERE. REVERSED BY 068 TASK 1 the same
rem                  way; it read: and the console says where it went, with
rem                  the root file asserted gone - nice-to-pass
rem
rem  THE CONSOLE IS CAPTURED TO console.txt AND THEN PRINTED. 067. A halt
rem  that must NAME what it found is a halt whose words are the assertion,
rem  and words on a console nobody kept cannot be asserted. The operator
rem  sees the whole run, at the end of it rather than during it.
rem
rem  AND 067's VERDICT IS ITS OWN CALL, at :verdict067. Not style: the
rem  verdict line below is already 7139 characters and cmd refuses a command
rem  line past 8191. The first cut of this extension added seven cases to it
rem  and every arm died at "The input line is too long" AFTER a full run -
rem  a fixture that spends seventy seconds and then cannot say what it saw.
rem  Two shorter calls, and the proven line is not touched at all.
rem
rem  NOT AGAINST THIS REPOSITORY. The root is under %TEMP%.
rem
rem  Generated 2026-09-14 for: work instructions 064 task 5
rem  Extended  2026-09-14 for: work instructions 065 task 6
rem  Extended  2026-09-14 for: work instructions 066 task 4
rem  Extended  2026-09-18 for: work instructions 067 task 5
rem  Extended  2026-09-21 for: work instructions 078 task 6 - reword, nearmiss

rem  074's ARMS - THE CARD. PHASE_STATUS.md is what the panel reads, and unit
rem  073's reconciliation of the RECORD left the card naming a different set of
rem  steps - so :phasesteps refused for the rest of every run and the card's
rem  step states froze while the panel showed them as current. The owner ruled
rem  on 2026-09-20 that the card is reconciled too, in its own LF-only form,
rem  and that a card which cannot be written NEVER ENDS A NIGHT.
rem
rem    card-append   the plan has two steps, the card has one
rem                  MUST append the second IN THE CARD'S OWN FORM - LF, no
rem                  BOM - and the card with that insertion removed MUST be
rem                  BYTE-IDENTICAL to the card before the run
rem    card-match    the card already names every step the plan does
rem                  MUST append nothing AND SAY SO, and touch no byte
rem    card-extra    the card carries step 9 and the plan does not
rem                  MUST report it and LEAVE IT WHERE IT IS - and the step it
rem                  does append lands AFTER it, out of numeric order, which is
rem                  what append-only costs and is deliberate
rem    card-locked   the card is read-only - the write fails after the read
rem                  MUST print what it could not do, change nothing, and EXIT
rem                  ZERO. Ruling 3 rejected a halt here by name
rem    card-absent   there is no card at the root
rem                  MUST print that and go on
rem    card-panel    the panel's OWN parser and painter, out of the HTML, run
rem                  against the reconciled card - see read-card.js
rem    card-twice    THREE iterations - MUST reconcile ONCE
rem    card-steps    a whole run after a successful reconciliation
rem                  MUST see :phasesteps WRITE NORMALLY AND PRINT NO FINDING,
rem                  which is the thing unit 073 left broken
rem    card-frozen   a whole run after a FAILED one - the card read-only
rem                  MUST see :phasesteps refuse safely, as it does today, and
rem                  the run carry on regardless
rem
rem  THE FIRST SIX RUN --fixture reconcile AND LAUNCH NOTHING. The byte proof
rem  needs it: in a real iteration :heartbeat writes a HEARTBEAT: line into the
rem  card and :phasesteps rewrites its step states, so a card compared before
rem  and after a full run carries three routines' work and the splice cannot be
rem  isolated in it. They also cost seconds rather than seventy apiece.
rem
rem  Extended  2026-09-20 for: work instructions 074 task 4

rem  083's ARMS - THE LOOP'S DEFAULT IS CONTINUE, AND IT RECENTERS ON THE PHASE.
rem  The owner, 2026-09-23: find a way forward, default to action, a question is
rem  parked and not a reason to quit; recenter on the phase goals, not the
rem  output. Built as: a step's state is its checkboxes (task 1), the arbiter is
rem  aimed at the first step with real open work (task 2), a section-4 hit on
rem  one of the three PARKS the question and routes past it (task 3), and the
rem  night ends only when every remaining criterion is parked or the owner's
rem  (task 4).
rem
rem  THE STAND-IN LEARNED TWO THINGS for these: .advances names the criterion
rem  each arbiter call authors, and .s4 with .s4at drive the section-4 judge's
rem  answer per call - see claude.bat. The section-4 hit is therefore the
rem  STAND-IN'S verdict, not a real judge's: what these arms prove is the
rem  ROUTING after a hit, and the stops fixture already proves the real judge
rem  reads a keying question as a hit.
rem
rem    park-one       a section-4 hit on 2.3 with 2.2 and 2.4 still open
rem                   MUST park 2.3 to PARKED.md naming keying, mark it, author
rem                   the next unit against 2.2, run it, and CONTINUE to the
rem                   backstop - two units, no halt on the question
rem    park-twice     the same, twice in a row on 2.3 then 2.2
rem                   MUST park both, skip both, and run a third unit at 2.4
rem    park-last      a hit on 2.2 when it is the ONLY open non-owner criterion,
rem                   beside two owner's-verdict lines and a named sheet
rem                   MUST end at exit 0, the ledger verdict ending and naming
rem                   the parked count, the sheet written with the parked
rem                   question ABOVE the criteria, and money named
rem    park-only      a hit on the only open criterion of a plan with no
rem                   owner's line and no REVIEW_SHEET
rem                   MUST end at exit 0 and put PARKED.md's path on the
rem                   console and in the ledger line, since there is no sheet
rem    park-unknown   the section-4 judge answers in a shape nothing can read
rem                   MUST still halt at stop 3 as unknown, a failure, and
rem                   write no PARKED.md - :s4unknown is untouched
rem    park-arbstop   the arbiter itself raises one of the three, MOVE: stop
rem                   MUST still halt at stop 4 and write no PARKED.md -
rem                   :arbstop is untouched
rem    park-fresh     park-one's root run again by a NEW loop
rem                   MUST clear the mark, say so, and let the new run author
rem                   2.3 again - a park is one pass, never a permanent block.
rem                   nice-to-pass
rem    state-header-done
rem                   the header says step 2 in progress; the plan has both of
rem                   its boxes ticked and step 3 open
rem                   MUST treat step 2 as done, aim at step 3, bring the
rem                   header up to done, and run the unit at 3.1
rem    state-header-notstarted
rem                   the header says step 2 not started; the plan has 2.1
rem                   ticked
rem                   MUST treat step 2 as partial, offer 2.2 and 2.3, run the
rem                   unit at 2.3 and record ADVANCED yes
rem    target-not-zero
rem                   steps 0 to 4, step 0 fully ticked but not started in the
rem                   header, work open in 3 and 4 - HamLet's case
rem                   MUST aim the prompt at step 3, never at step 0
rem
rem  Extended  2026-09-23 for: work instructions 083 task 5

rem  084's ARMS - THE PHASE GOAL IS THE PRIME DIRECTIVE. The owner, 2026-09-23:
rem  output.md is a helper, an indicator, a slight modification element, and
rem  drift - reading the indicator as the directive - is the failure. Built as:
rem  the directive first in every arbiter prompt and the report relabelled an
rem  indicator (task 3), a WHY that rests on the last report refused and
rem  redirected (task 4), the drift judge a redirect and a second drift a park
rem  (task 5).
rem
rem    prime-first    the prompt, built: the directive is its FIRST content,
rem                   before the phase goal, and every block's share is printed
rem    prime-label    the same prompt: the report block carries the indicator
rem                   label, sits last, and its share is printed
rem    prime-big      a report five times the prompt's size: the directive and
rem                   the criteria still lead, nothing displaced, share capped
rem    why-mixed      a WHY that quotes 2.3 and gives the last report as its
rem                   reason: REFUSED by task 4's detector, redirected, the loop
rem                   authors again and continues - no halt
rem    why-split      a WHY with a plan sentence of its own beside a report
rem                   sentence: task 4 lets it through and SAYS SO, the unit
rem                   runs, and the stand-in judge's report answer redirects -
rem                   caught by task 5, not task 4
rem    drift-report   the REAL judge reads a WHY that admits the report chose
rem                   the target: FOLLOWS report, and the loop REDIRECTS - it
rem                   does not halt - with redirect.txt written
rem    drift-plan     the REAL judge reads a plan-driven WHY on the one work
rem                   criterion beside two owner's lines: FOLLOWS plan, the unit
rem                   flips it, and the night ends at EXIT 0
rem    drift-twice    the stand-in judge says report twice running on 2.3:
rem                   parked with which=drift, 2.2 authored, night continues
rem    stop-over-why  MOVE: stop beside a WHY that cites only the report: the
rem                   STOP WINS and halts at stop 4, nothing is redirected
rem
rem  THREE ARMS ARE REVERSED, with the old expectation quoted at each:
rem    whyreport      069 asserted a HALT with the ledger line "cites no line
rem                   of the plan". 084: the refusal fires and REDIRECTS.
rem    whyboth        069 asserted it RUNS - "this check is not the guard for
rem                   that, 6.5 is". 084 adds the guard: REFUSED, redirected.
rem    followsreport  069 asserted the real judge answers FOLLOWS report and the
rem                   loop HALTS quoting it. 084's detector refuses that WHY
rem                   before the unit runs, so the judge is never reached.
rem  AND TWO OF 083's ARMS ARE CORRECTED, not reversed: state-header-done and
rem  state-header-notstarted read the target from arbiter-prompt.txt, a file a
rem  --seed run never writes, and failed both passes of unit 084's task 1 while
rem  the launcher did what they demand. They now read the console's own
rem  "target step N" line. The old assertion is quoted at :verdict083b.
rem
rem  Extended  2026-09-24 for: work instructions 084 task 6

rem  085's ARMS - TWO ENDINGS. The owner, 2026-09-23: money is no longer a
rem  thing a night stops for, and a night ends for two things only - a decision
rem  the arbiter must not make in his place, and drift so severe the arbiter
rem  cannot resolve the work back to a phase goal. Built as: --budget prints
rem  and ledgers and halts nothing, with no default (task 2); :drifthalt, the
rem  drift ending, reached from :ownerhalt when every parked criterion was
rem  parked for drift (task 3).
rem
rem    budget-none    --budget omitted: the banner says no ceiling, the spend
rem                   is still printed after the unit, the loop reaches its
rem                   backstop, and nothing says stop 2
rem    drift-all      plan-park-only, one authorable criterion, the stand-in
rem                   judge says report twice on it: parked for drift, and the
rem                   next iteration ENDS at exit 0 - THE ARBITER COULD NOT
rem                   RESOLVE THE WORK BACK TO THE PHASE GOAL - the ledger
rem                   verdict ending and its prose naming drift and 2.2
rem
rem  ONE ARM IS REVERSED, with the old expectation quoted at :verdict070:
rem    redirect-budget  070 asserted "--budget still halts during a redirect,
rem                   exactly as before" on the ledger line stop 2. 085: the
rem                   run goes PAST a --budget of 0, says so on the console,
rem                   carries the figure in the ledger's cost column, and
rem                   reaches its backstop.
rem  AND drift-twice IS THE PROOF THE DRIFT ENDING DOES NOT FIRE while a
rem  criterion the arbiter has not drifted on remains: 2.3 is parked for
rem  drift and 2.2 is authored and run, unchanged from 084.
rem
rem  Extended  2026-09-24 for: work instructions 085 task 5

rem  086's ARMS - THE ARBITER'S OWN PAPERWORK NEVER ENDS A NIGHT. The owner,
rem  2026-09-23: a malformed field is not a decision he reserved and not
rem  drift; the arbiter can fix it by authoring again. Built as: the four
rem  authoring refusals hand back through 084's :redirectnext and park on a
rem  second hit at a criterion (task 2); blocker-twice parks the criterion the
rem  blockers named or redirects (task 3); a report the validator refused is
rem  recorded unjudged with fate not recorded and the loop continues (task 4).
rem
rem    hb-noadv        a decision block with NO ADVANCES line: handed back,
rem                    the stand-in cannot repair it, handed back again,
rem                    nothing launched, the loop reaches its backstop
rem    hb-twice        the owner's 2.3 named by the shipped WI and again by
rem                    the arbiter's first call: the second hand-back PARKS
rem                    2.3 with which=refusal, the next call names 2.2 and
rem                    a unit runs
rem    hb-blocker-park two blocker-clears naming criterion 2.3: 2.3 is PARKED
rem                    with which=blocker, the arbiter is redirected, a third
rem                    unit runs, no halt
rem    hb-unshaped     the unit writes a report with no section 4: run-unit
rem                    exits 5, the entry lands with FATE: not recorded and
rem                    ADVANCED: not recorded, no judge is called, the ledger
rem                    carries a report-refused note, and the loop continues
rem    stop-over-hb    a keying MOVE: stop with no ADVANCES field: THE STOP
rem                    WINS at stop 4 - the dispatch now sits above the
rem                    missing-field refusal - and nothing is handed back
rem
rem  FOUR ARMS ARE REVERSED, with the old expectation quoted at :verdict:
rem    bad             064 asserted a HALT, refused because ADVANCES named no
rem                    criterion. 086: handed back, the arbiter authors 2.2,
rem                    a unit runs, the backstop.
rem    owner-advances  065 asserted a HALT, refused because ADVANCES named an
rem                    owner criterion. 086: handed back, 2.2 authored, a unit
rem                    runs, the backstop.
rem    closed          064 asserted a HALT, refused because step 2 is done
rem                    and closed. 086: handed back twice - the arbiter aimed
rem                    at the closed step again - nothing launched, the
rem                    backstop.
rem    blocker-twice   064 asserted a HALT by the blocker rule. 086: the two
rem                    blocker-clears named only a unit, nothing to park, the
rem                    arbiter is redirected, a third unit runs, the backstop.
rem  NOT BUILT, reported: two no-advance units at one criterion still
rem  REDIRECT to that criterion by a different approach, as 070 built under
rem  the owner's ruling of 2026-09-14 - stop 10 is dead code and never ended
rem  a night, and turning the redirect into a park would act against that
rem  ruling; raised in the report for the owner.
rem
rem  Extended  2026-09-24 for: work instructions 086 task 5

rem  087's ARMS - A HICCUP IS RETRIED, NOT FATAL. The owner, 2026-09-23:
rem  failure has to be the last option, and a first attempt failing is the
rem  first. Built as: the section-4 judge and the arbiter call retry once then
rem  park (task 2); the launch retries once then parks or, for a live lock,
rem  skips the iteration; a unit that ran and wrote no report is recorded
rem  unjudged and never re-run; the reload retries once then skips; --seed
rem  with no instruction is a door check (task 3); a denial parks the
rem  criterion and widens no scope (task 4).
rem
rem    s4-retry        the section-4 judge unreadable on call 1, readable on
rem                    the retry: continues, both attempts on the console and
rem                    a ledger note, nothing parked
rem    arb-retry       the arbiter call fails once, the retry succeeds: a unit
rem                    runs, a ledger note, the backstop
rem    arb-twice       the arbiter call fails twice: the first authorable
rem                    criterion is parked with which=arbiter, the next
rem                    iteration's call succeeds, a unit runs
rem    arb-stop-retry  the retried arbiter returns a keying MOVE: stop: THE
rem                    STOP WINS at stop 4
rem    launch-twice    no repository, every launch exits 7: retried once, then
rem                    the criterion is parked with which=launch, nothing
rem                    appended, the backstop
rem    deny-park       the unit is denied and does not complete: the criterion
rem                    is parked with which=denial, no allowed.txt written,
rem                    the loop continues - no stop 6
rem    fail-noseed     --seed with the shipped instruction deleted: refused at
rem                    the door at exit 2 with a ledger line, nothing called
rem
rem  THREE ARMS ARE REVERSED, with the old expectation quoted at each:
rem    park-unknown    083 asserted an unreadable judge still HALTS at stop 3.
rem                    087: retried once, then the criterion PARKS with
rem                    which=unread, the loop continues.
rem    noreport        067 asserted stop 11 with NOTHING appended. 087: not
rem                    retried, the entry lands with FATE not recorded, a
rem                    ledger note, the loop continues.
rem    leftover        067 asserted stop 11 naming UNIT: 358. 087: the leftover
rem                    is still NOT JUDGED and NOT appended, the root file's
rem                    UNIT: line is still named, and iteration 2 - a launch
rem                    that started nothing twice, the lock - is skipped rather
rem                    than ending the night.
rem  NOT STAGED, said plainly: a launch that fails once and succeeds on the
rem  retry; a reload that leaves nothing, once or twice. A launch cannot be
rem  made transient from outside run-unit.bat, and the retry's success path
rem  is the ordinary success path. A reload that leaves nothing cannot be
rem  staged at all: the first cut put a DIRECTORY where reload.txt goes, and
rem  the launcher's `if exist` accepted the directory as the file - reload.bat
rem  never fails on a good root, so the arm was dropped rather than kept red.
rem
rem  Extended  2026-09-24 for: work instructions 087 task 5

rem  088's ARMS - AN OWNER'S DECISION CAN BE ANSWERED, AND A STALE PREMISE IS
rem  HANDED BACK. HamLet, 2026-09-25: a stop 4 replayed on four launches after
rem  its conditions were resolved, because :arbstop read nothing from disk.
rem  Built as: PARKED.md is read before the halt for a RESOLVED row matched on
rem  the stop's own why line (task 2); a claim the launcher can test - a unit
rem  said to be executing, a phase named in quotes - is tested against the
rem  lock and the plan on the same pass, and a false one is handed back through
rem  :refused, twice at a criterion parking it with which=premise (task 3).
rem
rem    stop-answered      the keying stop with a RESOLVED row whose phrase is
rem                       copied from its why line: NOT re-raised, ANSWERED on
rem                       the console, a stop-answered ledger note, the
rem                       arbiter redirected, the backstop - no stop 4
rem    stop-other-answer  a RESOLVED row for some other question: stop 4 halts
rem    stale-unit         `unit 439 is executing now` while the lock is free:
rem                       HANDED BACK, twice, 2.3 parked with which=premise,
rem                       no stop 4, the backstop
rem    stale-plan-current a phase named in quotes that the record carries:
rem                       the premise HOLDS, stop 4 halts
rem    stale-plan-gone    a phase the plan does not name: HANDED BACK
rem  The keying-without-a-row and untestable cases are exhaust-stop and
rem  exhaust-stop-promise, unchanged and run twice; park-arbstop too.
rem
rem  Extended  2026-09-25 for: work instructions 088 task 4
rem ============================================================

set "HERE=%~dp0"
set "ARM=%~1"
set "PLAN=PHASE_PLAN.md"
set "WI="
set "FLIP="
set "FLIPAT="
set "MAXI=1"
set "CLOSED="
set "SEEDF=--seed"
set "REPORT="
set "REALJUDGE="
set "LEFTOVER="
set "NOREPORT="
set "TAKEPID="
set "PREKEPT="
set "URC="
set "VARY="
set "EXHAUST="
set "NOPLAN="
rem  073: RENAMED FROM SEEDREPORT, which unit 072 reused by accident - that name
rem  already existed in this file, a few lines below, holding a PATH for
rem  promptorder, promptbig and sharecheck, and 072 set it to 1 as a flag. Both
rem  copy lines then fired. The source and destination happened to match, so
rem  nothing broke - which is exactly why a suite passing did not catch it.
set "ROOTREPORT="
set "DROPSTEP="
set "ADDSTEP="
set "SEEDOUT="
set "WI2="
set "PROMPTONLY="
set "SEEDREPORT="
set "BIGREPORT="
set "BUDGETARG="
rem  080: THE OWNER'S CLOCK, WHICH NO ARM HAD EVER PASSED. MINUTESARG sits
rem  beside BUDGETARG and for the same reason: it is empty for every arm but
rem  the one that wants it, so no other arm's command line gains a character.
rem  SLOW is the seconds the UNIT stand-in waits, and it is what makes the
rem  ceiling reachable at all - see the redirect-minutes block below.
set "MINUTESARG="
set "SLOW="
set "CARD="
set "CARDRO="
set "RECONLY="
rem  083: the section-4 judge's answer, which calls get it, and the criteria
rem  the stand-in arbiter names per call. Empty for every arm but the parking
rem  ones, so no other arm's root gains a file.
set "S4="
set "S4AT="
set "ADVF="
rem  084: the stand-in state judge's FOLLOWS answer per call.
set "FOLLOWS="
rem  087: the hiccups - see the root setup below.
set "ARBFAILAT="
set "DENY="
set "NOGIT="
set "NOWI="
rem  088: a resolution row the owner wrote, seeded into PARKED.md before the run.
set "RESOLVED="
if /i "%ARM%"=="advance"        set "WI=WI-criterion.md"
if /i "%ARM%"=="advance"        set "FLIP=2.3"
if /i "%ARM%"=="noadvance"      set "WI=WI-criterion.md"
if /i "%ARM%"=="twice"          set "WI=WI-criterion.md"
if /i "%ARM%"=="twice"          set "MAXI=3"
if /i "%ARM%"=="closed"         set "WI=WI-criterion.md"
if /i "%ARM%"=="closed"         set "CLOSED=1"
rem  083 task 1: THE INPUT MOVES, THE EXPECTATION DOES NOT. Until 083 this arm
rem  seeded a HEADER saying step 2 was done while the plan left 2.2 and 2.3
rem  unticked, and the refusal fired off the header. The owner's ruling of
rem  2026-09-23 is that a step's state is its checkboxes and a header cannot
rem  override them - so that seed now reads step 2 as PARTIAL, the unit ran,
rem  and the arm went red on the first run after the change, as predicted. It
rem  now takes plan-closed.md, in which step 2's three criteria are all ticked
rem  and step 1's is open - step 1 must stay open, or stop 1 fires first, which
rem  is what PHASE_OUTCOME-closed.md's own note records. The refusal, its
rem  ledger line and the verdict below are unchanged word for word.
if /i "%ARM%"=="closed"         set "PLAN=plan-closed.md"
if /i "%ARM%"=="bad"            set "WI=WI-bad.md"
if /i "%ARM%"=="blocker"        set "WI=WI-blocker.md"
if /i "%ARM%"=="blocker-twice"  set "WI=WI-blocker.md"
if /i "%ARM%"=="blocker-twice"  set "MAXI=3"
if /i "%ARM%"=="owner-sheet"    set "WI=WI-criterion2.md"
if /i "%ARM%"=="owner-sheet"    set "PLAN=plan-owner-sheet.md"
if /i "%ARM%"=="owner-sheet"    set "SEEDF="
if /i "%ARM%"=="owner-again"    set "WI=WI-criterion2.md"
if /i "%ARM%"=="owner-again"    set "PLAN=plan-owner-sheet.md"
if /i "%ARM%"=="owner-again"    set "SEEDF="
if /i "%ARM%"=="owner-nosheet"  set "WI=WI-criterion2.md"
if /i "%ARM%"=="owner-nosheet"  set "PLAN=plan-owner-nosheet.md"
if /i "%ARM%"=="owner-nosheet"  set "SEEDF="
if /i "%ARM%"=="one-work"       set "WI=WI-criterion2.md"
if /i "%ARM%"=="one-work"       set "PLAN=plan-one-work.md"
if /i "%ARM%"=="one-work"       set "SEEDF="
if /i "%ARM%"=="owner-advances" set "WI=WI-criterion.md"
if /i "%ARM%"=="owner-advances" set "PLAN=plan-one-work.md"
if /i "%ARM%"=="reversal"       set "WI=WI-criterion.md"
if /i "%ARM%"=="reversal"       set "REPORT=output-reversal.md"
if /i "%ARM%"=="reversal"       set "REALJUDGE=1"
if /i "%ARM%"=="nine"           set "WI=WI-criterion.md"
if /i "%ARM%"=="nine"           set "REPORT=output-nine.md"
if /i "%ARM%"=="nine"           set "REALJUDGE=1"
if /i "%ARM%"=="attempt-one"          set "WI=WI-criterion.md"
if /i "%ARM%"=="attempt-one"          set "FLIP=2.3"
if /i "%ARM%"=="attempt-three"        set "WI=WI-criterion.md"
if /i "%ARM%"=="attempt-three"        set "FLIP=2.3"
if /i "%ARM%"=="attempt-three"        set "FLIPAT=2"
if /i "%ARM%"=="attempt-three"        set "MAXI=3"
if /i "%ARM%"=="attempt-restart"      set "WI=WI-criterion.md"
rem  068: the SECOND loop gets a different instruction. Both loops ran the same
rem  one before, and 068 now refuses the second - correctly, because repeating an
rem  approach at a criterion already recorded as failing is the thing it stops.
rem  What this arm is about is the RECORD surviving a restart, so the second loop
rem  takes a genuinely different route to the same criterion.
if /i "%ARM%"=="attempt-restart"      set "WI2=WI-criterion-b.md"
if /i "%ARM%"=="attempt-blocker-crit" set "WI=WI-blocker-crit.md"
if /i "%ARM%"=="attempt-blocker-unit" set "WI=WI-blocker.md"
rem  067. LEFTOVER and NOREPORT need only a marker file in the root; the
rem  stand-in reads it. The lock arms take the lock BEFORE the loop starts,
rem  which is where a leaked one is found.
if /i "%ARM%"=="leftover"       set "WI=WI-criterion.md"
if /i "%ARM%"=="leftover"       set "FLIP=2.3"
if /i "%ARM%"=="leftover"       set "FLIPAT=1"
if /i "%ARM%"=="leftover"       set "MAXI=2"
if /i "%ARM%"=="leftover"       set "LEFTOVER=1"
if /i "%ARM%"=="lockleak"       set "WI=WI-criterion.md"
if /i "%ARM%"=="lockleak"       set "FLIP=2.3"
if /i "%ARM%"=="lockleak"       set "FLIPAT=2"
if /i "%ARM%"=="lockleak"       set "MAXI=2"
if /i "%ARM%"=="lock-dead"      set "WI=WI-criterion.md"
if /i "%ARM%"=="lock-dead"      set "FLIP=2.3"
if /i "%ARM%"=="lock-dead"      set "TAKEPID=999999"
if /i "%ARM%"=="lock-live"      set "WI=WI-criterion.md"
if /i "%ARM%"=="lock-live"      set "TAKEPID=4"
if /i "%ARM%"=="noreport"       set "WI=WI-criterion.md"
if /i "%ARM%"=="noreport"       set "NOREPORT=1"
if /i "%ARM%"=="kept"           set "WI=WI-criterion.md"
if /i "%ARM%"=="kept"           set "FLIP=2.3"
if /i "%ARM%"=="kept-twice"     set "WI=WI-criterion.md"
if /i "%ARM%"=="kept-twice"     set "FLIP=2.3"
if /i "%ARM%"=="kept-twice"     set "PREKEPT=1"
rem  068: ARMS THAT RUN TWO OR MORE UNITS AT ONE CRITERION NOW SET .vary.
rem  Without it the second iteration repeats the first-s approach and 068
rem  refuses it before the unit runs - correctly. The arms below are about
rem  stop 10, the attempt record and the lock, not about repeating an
rem  approach, so the stand-in authors a different one each call as a real
rem  arbiter would. THE EXPECTATIONS OF twice, blocker-twice AND lockleak ARE
rem  UNCHANGED; attempt-three-s approach strings are not, and its old ones are
rem  quoted in this unit-s commit message.
if /i "%ARM%"=="twice"          set "VARY=1"
if /i "%ARM%"=="attempt-three"  set "VARY=1"
if /i "%ARM%"=="lockleak"       set "VARY=1"

rem  073'S ARMS - THE LOOP SEES EVERY STEP THE PLAN CARRIES.
rem  Each seeds a record, removes or adds a step line in the HEADER ONLY, and
rem  lets the loop reconcile before its first iteration. What is proved is that
rem  the header ends up naming what the plan names, and that nothing already in
rem  it moved.
if /i "%ARM%"=="rec-append"   set "WI=WI-criterion.md"
if /i "%ARM%"=="rec-append"   set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="rec-append"   set "DROPSTEP=2"
if /i "%ARM%"=="rec-append"   set "MAXI=1"
rem  rec-twice runs THREE iterations. The reconciliation must happen once, before
rem  the first, and never again - so exactly one appended line on the console.
if /i "%ARM%"=="rec-twice"    set "WI=WI-criterion.md"
if /i "%ARM%"=="rec-twice"    set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="rec-twice"    set "DROPSTEP=2"
if /i "%ARM%"=="rec-twice"    set "MAXI=3"
if /i "%ARM%"=="rec-twice"    set "VARY=1"
if /i "%ARM%"=="rec-match"    set "WI=WI-criterion.md"
if /i "%ARM%"=="rec-match"    set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="rec-match"    set "MAXI=1"
if /i "%ARM%"=="rec-extra"    set "WI=WI-criterion.md"
if /i "%ARM%"=="rec-extra"    set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="rec-extra"    set "ADDSTEP=9"
if /i "%ARM%"=="rec-extra"    set "MAXI=1"
rem  rec-done: step 1 is already done in the header. Appending must not disturb
rem  its state - the thing ruling 3 forbids and the thing a rewrite would break.
if /i "%ARM%"=="rec-done"     set "WI=WI-criterion.md"
if /i "%ARM%"=="rec-done"     set "SEEDOUT=PHASE_OUTCOME-rec-done.md"
if /i "%ARM%"=="rec-done"     set "DROPSTEP=2"
if /i "%ARM%"=="rec-done"     set "MAXI=1"
rem  rec-owner: the appended step's criteria are UNMET WORK, so the owner-s-
rem  verdict halt must NOT fire. The plan names the appended step and marks
rem  nothing in it the owner-s.
if /i "%ARM%"=="rec-owner"    set "WI=WI-criterion.md"
rem  073: THE DEFAULT PLAN, not plan-owner-sheet.md. That plan marks its only
rem  remaining criteria as the owner-s, so the halt fires there CORRECTLY and
rem  the arm would prove nothing. What this arm is for is that a step appended
rem  by the reconciliation, carrying unmet WORK, does not make the loop think
rem  the phase is waiting on him.
if /i "%ARM%"=="rec-owner"    set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="rec-owner"    set "DROPSTEP=2"
if /i "%ARM%"=="rec-owner"    set "MAXI=1"

rem  074's ARMS - the card. The first six launch nothing; see the header.
if /i "%ARM%"=="card-append"  set "WI=WI-criterion.md"
if /i "%ARM%"=="card-append"  set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-append"  set "DROPSTEP=2"
if /i "%ARM%"=="card-append"  set "CARD=PHASE_STATUS-one.md"
if /i "%ARM%"=="card-append"  set "RECONLY=1"
if /i "%ARM%"=="card-match"   set "WI=WI-criterion.md"
if /i "%ARM%"=="card-match"   set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-match"   set "CARD=PHASE_STATUS-both.md"
if /i "%ARM%"=="card-match"   set "RECONLY=1"
if /i "%ARM%"=="card-extra"   set "WI=WI-criterion.md"
if /i "%ARM%"=="card-extra"   set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-extra"   set "CARD=PHASE_STATUS-extra.md"
if /i "%ARM%"=="card-extra"   set "RECONLY=1"
if /i "%ARM%"=="card-locked"  set "WI=WI-criterion.md"
if /i "%ARM%"=="card-locked"  set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-locked"  set "DROPSTEP=2"
if /i "%ARM%"=="card-locked"  set "CARD=PHASE_STATUS-one.md"
if /i "%ARM%"=="card-locked"  set "CARDRO=1"
if /i "%ARM%"=="card-locked"  set "RECONLY=1"
if /i "%ARM%"=="card-absent"  set "WI=WI-criterion.md"
if /i "%ARM%"=="card-absent"  set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-absent"  set "DROPSTEP=2"
if /i "%ARM%"=="card-absent"  set "RECONLY=1"
if /i "%ARM%"=="card-panel"   set "WI=WI-criterion.md"
if /i "%ARM%"=="card-panel"   set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-panel"   set "DROPSTEP=2"
if /i "%ARM%"=="card-panel"   set "CARD=PHASE_STATUS-one.md"
if /i "%ARM%"=="card-panel"   set "RECONLY=1"
rem  the three that run the loop
if /i "%ARM%"=="card-twice"   set "WI=WI-criterion.md"
if /i "%ARM%"=="card-twice"   set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-twice"   set "DROPSTEP=2"
if /i "%ARM%"=="card-twice"   set "CARD=PHASE_STATUS-one.md"
if /i "%ARM%"=="card-twice"   set "MAXI=3"
if /i "%ARM%"=="card-twice"   set "VARY=1"
if /i "%ARM%"=="card-steps"   set "WI=WI-criterion.md"
if /i "%ARM%"=="card-steps"   set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-steps"   set "DROPSTEP=2"
if /i "%ARM%"=="card-steps"   set "CARD=PHASE_STATUS-one.md"
if /i "%ARM%"=="card-steps"   set "MAXI=1"
if /i "%ARM%"=="card-frozen"  set "WI=WI-criterion.md"
if /i "%ARM%"=="card-frozen"  set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="card-frozen"  set "DROPSTEP=2"
if /i "%ARM%"=="card-frozen"  set "CARD=PHASE_STATUS-one.md"
if /i "%ARM%"=="card-frozen"  set "CARDRO=1"
if /i "%ARM%"=="card-frozen"  set "MAXI=1"

rem  072'S ARMS - WHICH ENDING, OR WHICH STOP.
rem  Each one drives the loop to ONE halt and then reads RUN_LEDGER.md rather
rem  than the console, because the ledger is what the owner reads at breakfast
rem  and the console is gone by then.
if /i "%ARM%"=="end-exhausted"  set "WI=WI-criterion.md"
if /i "%ARM%"=="end-exhausted"  set "PLAN=plan-sheet.md"
if /i "%ARM%"=="end-exhausted"  set "SEEDOUT=PHASE_OUTCOME-ex-three.md"
if /i "%ARM%"=="end-exhausted"  set "EXHAUST=2.3"
if /i "%ARM%"=="end-exhausted"  set "MAXI=2"
if /i "%ARM%"=="end-exhausted"  set "SEEDF="
if /i "%ARM%"=="end-exhausted"  set "ROOTREPORT=1"
rem  THE THREE STOPS ARE ENDINGS, NOT FAILURES - ruling 2 says so in terms, and
rem  this arm is where that is proved. MOVE: stop lands at stop 4.
if /i "%ARM%"=="end-three"      set "WI=WI-stop.md"
if /i "%ARM%"=="end-three"      set "PLAN=plan-sheet.md"
if /i "%ARM%"=="end-three"      set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="end-three"      set "MAXI=2"
if /i "%ARM%"=="end-owner"      set "WI=WI-criterion.md"
if /i "%ARM%"=="end-owner"      set "PLAN=plan-owner-sheet.md"
if /i "%ARM%"=="end-owner"      set "MAXI=2"
rem  THE SAME PHASE ENDED TWICE. Two whole loops over one root; the sheet must
rem  be written by the first and left alone by the second, decided from disk.
if /i "%ARM%"=="end-twice"      set "WI=WI-criterion.md"
if /i "%ARM%"=="end-twice"      set "PLAN=plan-owner-sheet.md"
if /i "%ARM%"=="end-twice"      set "MAXI=2"
if /i "%ARM%"=="fail-stop11"    set "WI=WI-criterion.md"
if /i "%ARM%"=="fail-stop11"    set "NOREPORT=1"
if /i "%ARM%"=="fail-stop11"    set "MAXI=2"
if /i "%ARM%"=="fail-maxiter"   set "WI=WI-criterion.md"
if /i "%ARM%"=="fail-maxiter"   set "SEEDOUT=PHASE_OUTCOME-noadv-repeat.md"
if /i "%ARM%"=="fail-maxiter"   set "SEEDF="
if /i "%ARM%"=="fail-maxiter"   set "MAXI=2"
rem  A HALT THAT WROTE NOTHING BEFORE THIS UNIT. The plan is removed after the
rem  root is built, so the launcher refuses at its door - and now says so in
rem  the ledger instead of leaving the morning with no line at all.
if /i "%ARM%"=="fail-noplan"    set "WI=WI-criterion.md"
if /i "%ARM%"=="fail-noplan"    set "NOPLAN=1"
if /i "%ARM%"=="fail-noplan"    set "MAXI=2"

rem  071'S ARMS - AN ENDING MUST BE DEMONSTRATED, NOT DECLARED.
rem  Every one seeds PHASE_OUTCOME.md by hand and then has the stand-in
rem  DECLARE the criterion exhausted, so what is being proved is that the loop
rem  decides from THE RECORD and never from the claim. None of them launches a
rem  unit, so they finish in seconds - which is the point: an ending that is
rem  refused costs nothing, and one that is permitted spends nothing either.
if /i "%ARM%"=="exhaust-three"    set "WI=WI-criterion.md"
if /i "%ARM%"=="exhaust-three"    set "SEEDOUT=PHASE_OUTCOME-ex-three.md"
if /i "%ARM%"=="exhaust-three"    set "EXHAUST=2.3"
if /i "%ARM%"=="exhaust-three"    set "MAXI=2"
rem  072: exhaust-three now also proves the review sheet at an EXHAUSTION
rem  ending - 065 only ever wrote one at the owner-s verdict - so it needs a
rem  plan that names one.
if /i "%ARM%"=="exhaust-three"    set "PLAN=plan-sheet.md"
if /i "%ARM%"=="exhaust-three"    set "SEEDF="
if /i "%ARM%"=="exhaust-empty"    set "WI=WI-criterion.md"
if /i "%ARM%"=="exhaust-empty"    set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="exhaust-empty"    set "EXHAUST=2.3"
if /i "%ARM%"=="exhaust-empty"    set "MAXI=2"
if /i "%ARM%"=="exhaust-empty"    set "SEEDF="
if /i "%ARM%"=="exhaust-two"      set "WI=WI-criterion.md"
if /i "%ARM%"=="exhaust-two"      set "SEEDOUT=PHASE_OUTCOME-ex-two.md"
if /i "%ARM%"=="exhaust-two"      set "EXHAUST=2.3"
if /i "%ARM%"=="exhaust-two"      set "MAXI=2"
if /i "%ARM%"=="exhaust-two"      set "SEEDF="
if /i "%ARM%"=="exhaust-same"     set "WI=WI-criterion.md"
if /i "%ARM%"=="exhaust-same"     set "SEEDOUT=PHASE_OUTCOME-ex-same.md"
if /i "%ARM%"=="exhaust-same"     set "EXHAUST=2.3"
if /i "%ARM%"=="exhaust-same"     set "MAXI=2"
if /i "%ARM%"=="exhaust-same"     set "SEEDF="
if /i "%ARM%"=="exhaust-noreason" set "WI=WI-criterion.md"
if /i "%ARM%"=="exhaust-noreason" set "SEEDOUT=PHASE_OUTCOME-ex-noreason.md"
if /i "%ARM%"=="exhaust-noreason" set "EXHAUST=2.3"
if /i "%ARM%"=="exhaust-noreason" set "MAXI=2"
if /i "%ARM%"=="exhaust-noreason" set "SEEDF="
rem  THE TWO THAT MUST NOT BE TESTED. Both at a criterion whose record is
rem  EMPTY - so if either were ever put to the exhaustion test it would be
rem  refused and redirected, and the arm would see the loop carry on instead of
rem  halting. That is what makes them proofs of 4.4 and 4.5 rather than
rem  restatements: the empty record is the trap.
if /i "%ARM%"=="exhaust-stop"     set "WI=WI-stop.md"
if /i "%ARM%"=="exhaust-stop"     set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="exhaust-stop"     set "MAXI=2"
if /i "%ARM%"=="exhaust-stop"     set "SEEDF=--seed"

rem  081 task 7: THE OTHER TWO OF THE THREE. 4.4 claims THE THREE STOPS and
rem  WI-stop.md raises only category (a), the keying question. Unit 080 saw
rem  exhaust-stop pass twice and REFUSED to tick 4.4 on it, because one arm
rem  shows one stop. These two raise (b) money past the budget and (c) what
rem  the product promises the operator.
rem
rem  SAME SEED AS exhaust-stop - PHASE_OUTCOME-ex-empty.md, an EMPTY record -
rem  so each, if it were ever put to the exhaustion test, would be REFUSED and
rem  the loop would carry on. That is the trap, and it is what makes these
rem  proofs rather than restatements.
rem
rem  EACH ASSERTS ITS OWN why : LINE on top of everything exhaust-stop
rem  asserts. Without that the three arms are three copies and would prove
rem  once what they claim to prove three times.
rem
rem  WHAT THEY DO NOT PROVE, and the report says so in this sentence: all
rem  three arrive by ONE GUARD - if /i "%%A_MOVE%%"=="stop" goto :arbstop at
rem  about run-phase.bat line 477 - which halts WITHOUT LOOKING at which of
rem  the three was raised. So these prove that WHICHEVER category the arbiter
rem  raises, the loop halts on sight at stop 4, before the exhaustion dispatch
rem  sixty lines below, at a criterion whose record is empty. They do NOT
rem  prove three independent code paths.
if /i "%ARM%"=="exhaust-stop-money"   set "WI=WI-stop-money.md"
if /i "%ARM%"=="exhaust-stop-money"   set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="exhaust-stop-money"   set "MAXI=2"
if /i "%ARM%"=="exhaust-stop-money"   set "SEEDF=--seed"
if /i "%ARM%"=="exhaust-stop-promise" set "WI=WI-stop-promise.md"
if /i "%ARM%"=="exhaust-stop-promise" set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="exhaust-stop-promise" set "MAXI=2"
if /i "%ARM%"=="exhaust-stop-promise" set "SEEDF=--seed"

if /i "%ARM%"=="exhaust-owner"    set "WI=WI-criterion.md"
if /i "%ARM%"=="exhaust-owner"    set "PLAN=plan-owner-sheet.md"
if /i "%ARM%"=="exhaust-owner"    set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="exhaust-owner"    set "MAXI=2"

rem  068-s OWN ARMS. Every one seeds PHASE_OUTCOME.md with a record made by
rem  hand, so what is being proved is how the launcher READS a record rather
rem  than whether it can write one - 066 already proved the writing. The four
rem  refusal arms never launch a unit and finish in seconds.
if /i "%ARM%"=="repeat"         set "WI=WI-criterion.md"
if /i "%ARM%"=="repeat"         set "SEEDOUT=PHASE_OUTCOME-repeat.md"
if /i "%ARM%"=="fresh"          set "WI=WI-criterion.md"
if /i "%ARM%"=="fresh"          set "SEEDOUT=PHASE_OUTCOME-fresh.md"
if /i "%ARM%"=="fresh"          set "FLIP=2.3"
if /i "%ARM%"=="succeeded"      set "WI=WI-criterion.md"
if /i "%ARM%"=="succeeded"      set "SEEDOUT=PHASE_OUTCOME-succeeded.md"
if /i "%ARM%"=="succeeded"      set "FLIP=2.3"
if /i "%ARM%"=="othercrit"      set "WI=WI-criterion.md"
if /i "%ARM%"=="othercrit"      set "SEEDOUT=PHASE_OUTCOME-othercrit.md"
if /i "%ARM%"=="othercrit"      set "FLIP=2.3"
if /i "%ARM%"=="noid"           set "WI=WI-criterion.md"
if /i "%ARM%"=="noid"           set "SEEDOUT=PHASE_OUTCOME-noid.md"
if /i "%ARM%"=="restart-id"     set "WI=WI-criterion.md"
if /i "%ARM%"=="restart-id"     set "SEEDOUT=PHASE_OUTCOME-restartid.md"
rem  arbrecord takes NO --seed, so the arbiter is called on iteration 1 and
rem  writes its prompt. None of the three seeded attempts blocks this
rem  instruction, so the unit runs and the loop reaches its backstop.
if /i "%ARM%"=="arbrecord"      set "WI=WI-criterion.md"
if /i "%ARM%"=="arbrecord"      set "SEEDOUT=PHASE_OUTCOME-three.md"
if /i "%ARM%"=="arbrecord"      set "SEEDF="
if /i "%ARM%"=="arbrecord"      set "FLIP=2.3"
rem  068: an attempt whose FATE is never ran was not tried, so it must not
rem  block. run-unit exit 3 records exactly that - the run failed before it
rem  reached its instruction, and the criterion cannot have flipped, so
rem  ADVANCED is no against an approach nobody attempted.
if /i "%ARM%"=="neverran"       set "WI=WI-criterion.md"
if /i "%ARM%"=="neverran"       set "SEEDOUT=PHASE_OUTCOME-neverran.md"
if /i "%ARM%"=="neverran"       set "FLIP=2.3"

rem  078'S TWO ARMS - SUBSTANCE, NOT WORDING, AND WHAT THAT MISSES.
rem  Criterion 2.4 of the phase plan asks for the match to be on substance
rem  rather than wording, and for the report to say how that was decided and
rem  what it will miss. NO ARM IN THE TREE DECIDED IT. repeat seeds an
rem  approach BYTE-IDENTICAL to the instruction's, which proves an exact
rem  match and nothing at all about substance. These two do.
rem
rem  Both seed the same criterion as repeat and take the same instruction,
rem  WI-criterion.md, whose approach reads
rem      move criterion 2.3 - the fixture is killed at ten quiet looks
rem  and whose six distinctive words are
rem      criterion  fixture  killed  looks  move  quiet
rem
rem    reword     the recorded attempt says the SAME THING IN DIFFERENT
rem               WORDS - reordered, re-punctuated, with question and wants
rem               added, and NOT A SUBSTRING of the instruction's approach.
rem               Its distinctive set is a strict SUPERSET of the six, so
rem               containment holds. MUST REFUSE, and the console's
rem               shared words line is the evidence of HOW it was decided.
rem    nearmiss   the reword seed with KILLED changed to TERMINATED and
rem               nothing else. One of the six is now absent from the
rem               recorded set, containment fails, and the approach is LET
rem               THROUGH. MUST NOT REFUSE - the unit runs.
rem
rem  nearmiss IS NOT A BUG AND IS NOT TO BE FIXED BY TIGHTENING THE MATCHER.
rem  068's ruling, unchanged tonight: a false match blocks a route the
rem  arbiter has never tried, which is the failure this phase exists to
rem  prevent; a missed match costs one repeated unit, which the no-advance
rem  path already catches. This arm MEASURES that cost instead of asserting
rem  it, and a FAIL here means the matcher has been tightened past what 068
rem  ruled. The matcher was READ by 078 and not written.
rem
rem  reword launches no unit and finishes in seconds; nearmiss launches one
rem  and takes about seventy, so it carries FLIP exactly as fresh does.
if /i "%ARM%"=="reword"         set "WI=WI-criterion.md"
if /i "%ARM%"=="reword"         set "SEEDOUT=PHASE_OUTCOME-reword.md"
if /i "%ARM%"=="nearmiss"       set "WI=WI-criterion.md"
if /i "%ARM%"=="nearmiss"       set "SEEDOUT=PHASE_OUTCOME-nearmiss.md"
if /i "%ARM%"=="nearmiss"       set "FLIP=2.3"

rem  069-S ARMS. The three prompt arms and the WHY refusal never launch a unit
rem  and finish in seconds: the prompt ones run --fixture promptonly, which
rem  composes the prompt and stops, and the refusal happens before the launch.
if /i "%ARM%"=="promptorder"    set "WI=WI-criterion.md"
if /i "%ARM%"=="promptorder"    set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="promptorder"    set "PROMPTONLY=1"
if /i "%ARM%"=="promptorder"    set "SEEDREPORT=..\watchdog\fixture-output.md"
if /i "%ARM%"=="promptbig"      set "WI=WI-criterion.md"
if /i "%ARM%"=="promptbig"      set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="promptbig"      set "PROMPTONLY=1"
if /i "%ARM%"=="promptbig"      set "SEEDREPORT=..\watchdog\fixture-output.md"
if /i "%ARM%"=="promptbig"      set "BIGREPORT=5"
if /i "%ARM%"=="sharecheck"     set "WI=WI-criterion.md"
if /i "%ARM%"=="sharecheck"     set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="sharecheck"     set "PROMPTONLY=1"
if /i "%ARM%"=="sharecheck"     set "SEEDREPORT=..\watchdog\fixture-output.md"
if /i "%ARM%"=="sharecheck"     set "BIGREPORT=5"
if /i "%ARM%"=="whyplan"        set "WI=WI-whyplan.md"
if /i "%ARM%"=="whyplan"        set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="whyplan"        set "FLIP=2.3"
if /i "%ARM%"=="whyreport"      set "WI=WI-whyreport.md"
if /i "%ARM%"=="whyreport"      set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="whyboth"        set "WI=WI-whyboth.md"
if /i "%ARM%"=="whyboth"        set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="whyboth"        set "FLIP=2.3"
rem  THE REAL JUDGE, as 065 proved its two ruling arms. A stand-in judge would
rem  prove only that the stand-in says what it was written to say.
if /i "%ARM%"=="followsreport"  set "WI=WI-followsreport.md"
if /i "%ARM%"=="followsreport"  set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="followsreport"  set "REPORT=output-panelq.md"
if /i "%ARM%"=="followsreport"  set "REALJUDGE=1"
if /i "%ARM%"=="followsplan"    set "WI=WI-followsplan.md"
if /i "%ARM%"=="followsplan"    set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="followsplan"    set "REPORT=output-panelq.md"
if /i "%ARM%"=="followsplan"    set "REALJUDGE=1"
if /i "%ARM%"=="followsplan"    set "FLIP=2.3"

rem  070-S ARMS. Every one SEEDS the record, so the redirect fires on iteration
rem  1 rather than after two seventy-second unit runs. The three that launch no
rem  unit at all - redirect-repeat, redirect-maxiter, redirect-owner - finish in
rem  seconds, which is the point of a redirect: nothing is spent.
if /i "%ARM%"=="redirect-same"    set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-same"    set "SEEDOUT=PHASE_OUTCOME-noadv-same.md"
if /i "%ARM%"=="redirect-same"    set "SEEDF="
if /i "%ARM%"=="redirect-same"    set "FLIP=2.3"
if /i "%ARM%"=="redirect-fresh"   set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-fresh"   set "SEEDOUT=PHASE_OUTCOME-noadv-same.md"
if /i "%ARM%"=="redirect-fresh"   set "SEEDF="
if /i "%ARM%"=="redirect-fresh"   set "FLIP=2.3"
if /i "%ARM%"=="redirect-wander"  set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-wander"  set "SEEDOUT=PHASE_OUTCOME-noadv-diff.md"
if /i "%ARM%"=="redirect-wander"  set "SEEDF="
if /i "%ARM%"=="redirect-wander"  set "FLIP=2.3"
rem  no .vary, so the arbiter keeps naming the approach the record already shows
rem  failing and the loop redirects on every iteration without launching anything
if /i "%ARM%"=="redirect-repeat"  set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-repeat"  set "SEEDOUT=PHASE_OUTCOME-noadv-repeat.md"
if /i "%ARM%"=="redirect-repeat"  set "SEEDF="
if /i "%ARM%"=="redirect-repeat"  set "MAXI=2"
if /i "%ARM%"=="redirect-refused" set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-refused" set "SEEDOUT=PHASE_OUTCOME-refuse1.md"
if /i "%ARM%"=="redirect-refused" set "MAXI=2"
if /i "%ARM%"=="redirect-refused" set "VARY=1"
if /i "%ARM%"=="redirect-refused" set "FLIP=2.3"
rem  --budget 0 makes the very first check over budget - SPENT 0 is not less
rem  than BUDGET 0 - so the brake is exercised without the stand-in having to
rem  report a cost it does not report.
if /i "%ARM%"=="redirect-budget" set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-budget" set "SEEDOUT=PHASE_OUTCOME-noadv-same.md"
if /i "%ARM%"=="redirect-budget" set "SEEDF="
if /i "%ARM%"=="redirect-budget" set "MAXI=3"
if /i "%ARM%"=="redirect-budget" set "FLIP=2.3"
if /i "%ARM%"=="redirect-budget" set "BUDGETARG=--budget 0"
if /i "%ARM%"=="redirect-maxiter" set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-maxiter" set "SEEDOUT=PHASE_OUTCOME-noadv-repeat.md"
if /i "%ARM%"=="redirect-maxiter" set "SEEDF="
if /i "%ARM%"=="redirect-maxiter" set "MAXI=3"
if /i "%ARM%"=="redirect-owner"   set "WI=WI-criterion2.md"
if /i "%ARM%"=="redirect-owner"   set "PLAN=plan-owner-sheet.md"
if /i "%ARM%"=="redirect-owner"   set "SEEDOUT=PHASE_OUTCOME-noadv-owner.md"
if /i "%ARM%"=="redirect-owner"   set "SEEDF="
if /i "%ARM%"=="redirect-stop11"  set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-stop11"  set "SEEDOUT=PHASE_OUTCOME-noadv-same.md"
if /i "%ARM%"=="redirect-stop11"  set "SEEDF="
if /i "%ARM%"=="redirect-stop11"  set "MAXI=2"
if /i "%ARM%"=="redirect-stop11"  set "NOREPORT=1"
rem  a blocker-clear must SATISFY a redirect - found by reading the redirect
rem  back against ruling 2, which says find a way through
if /i "%ARM%"=="redirect-blocker" set "WI=WI-blocker.md"
if /i "%ARM%"=="redirect-blocker" set "SEEDOUT=PHASE_OUTCOME-noadv-same.md"
if /i "%ARM%"=="redirect-blocker" set "SEEDF="
rem  080: --minutes, THE ONE BOUND NO FIXTURE HAS EVER EXERCISED. Criterion 3.4
rem  names it beside --budget and a grep of this whole fixtures tree for
rem  --minutes returned nothing, so there was no recorded evidence that the
rem  owner's wall-clock ceiling still fires - least of all on a REDIRECTED
rem  iteration, which is the thing unit 070 changed.
rem
rem  SEEDED LIKE redirect-same, no --seed, so a redirect is in force on
rem  iteration 1 and what is proved is that --minutes still halts A REDIRECTED
rem  LOOP rather than an ordinary one.
rem
rem  WHY THE UNIT HAS TO BE MADE SLOW. AGEMIN is computed only while the tree
rem  is alive: run-unit-watched.bat's look finds the child, and if the child
rem  is already gone it reports TREE=gone and RETURNS BEFORE SETTING AGEMIN,
rem  so :ceiling reads `if not defined AGEMIN goto :poll` and the ceiling is
rem  never tested. The stand-in does not sleep, so on every other arm the
rem  unit is finished long before the watchdog's first look at POLLSEC=60 -
rem  the seventy seconds an arm takes is THE FIRST LOOK, not the unit
rem  working. .slow makes the UNIT call alone wait, so the child is still
rem  there when the look lands.
rem
rem  NINETY SECONDS, and the arithmetic is why. The first look is at
rem  POLLSEC=60 and AGEMIN is a floor of whole minutes, so it reads 1 there
rem  and `if %AGEMIN% GEQ %MINUTES%` fires at --minutes 1 on that first look.
rem  Ninety leaves thirty seconds of margin for a slow Win32_Process query
rem  without adding a further minute to an arm that already costs one.
rem  Author's, overrulable, and bounded at 180 by the instruction.
rem
rem  --minutes 1 AND NOT 0. run-unit-watched.bat validates it as a whole
rem  number AT LEAST 1, so 0 is refused at the door and is not a route to a
rem  ceiling that fires immediately.
rem
rem  WHAT THIS ARM MEASURED AND WHY IT DOES NOT ASSERT stop 5. Unit 080 wrote
rem  this arm's first must line as "the ledger records stop 5", which is what
rem  criterion 3.4 and the instruction both assume. IT FAILED, AND THE CEILING
rem  WAS NOT WHY - the ceiling fired exactly as designed:
rem
rem    [look 1] working - 578.1 ms across 5 process(es), at or over the floor
rem    THE OWNER'S CEILING. This run is 1 min old and --minutes is 1.
rem    Terminating pid 31628 and its children, BY PID.
rem    Ledger line written: killed at the owner's --minutes ceiling: 1 min
rem
rem  but the PHASE recorded the halt as `stop 11: no report was written by this
rem  unit`, not `stop 5: the watchdog fired`. The reason is an ORDERING in
rem  run-phase.bat and it is not this fixture's to repair. Line 752,
rem  `if not "%OWNREPORT%"=="yes" goto :noreport`, runs BEFORE line 1219,
rem  `if "%RUNRC%"=="1" goto :ambiguous1`, which is the only path to stop 5. A
rem  unit killed mid-flight has written no output.md, so the report check
rem  always jumps away first and :ambiguous1 is never reached.
rem
rem  SO stop 5 IS UNREACHABLE FROM A WALL-CLOCK KILL unless the killed unit
rem  happened to leave a fresh report behind. run-phase.bat's own comment at
rem  2357 believes the opposite - "this arm should now be reached only after a
rem  kill". Reported by 080, repaired by nobody: run-phase.bat was parked.
rem
rem  This arm therefore asserts what --minutes ACTUALLY DOES, which is what
rem  3.4 asks - it halts the loop - and PRINTS the stop the phase recorded so
rem  the gap is visible in the arm's own output instead of only in a report.
if /i "%ARM%"=="redirect-minutes" set "WI=WI-criterion.md"
if /i "%ARM%"=="redirect-minutes" set "SEEDOUT=PHASE_OUTCOME-noadv-same.md"
if /i "%ARM%"=="redirect-minutes" set "SEEDF="
if /i "%ARM%"=="redirect-minutes" set "FLIP=2.3"
if /i "%ARM%"=="redirect-minutes" set "MINUTESARG=--minutes 1"
if /i "%ARM%"=="redirect-minutes" set "SLOW=90"

rem  083'S ARMS. The parking arms take NO --seed so the arbiter authors from
rem  iteration 1 and .advances can move it between criteria; the state arms
rem  keep --seed because what they prove is the derivation, not the authoring.
if /i "%ARM%"=="park-one"        set "WI=WI-criterion.md"
if /i "%ARM%"=="park-one"        set "PLAN=plan-park.md"
if /i "%ARM%"=="park-one"        set "SEEDF="
if /i "%ARM%"=="park-one"        set "VARY=1"
if /i "%ARM%"=="park-one"        set "REPORT=output-panelq.md"
if /i "%ARM%"=="park-one"        set "S4=ruling keying"
if /i "%ARM%"=="park-one"        set "S4AT=1"
if /i "%ARM%"=="park-one"        set "ADVF=adv-park-one.txt"
if /i "%ARM%"=="park-one"        set "MAXI=2"
if /i "%ARM%"=="park-twice"      set "WI=WI-criterion.md"
if /i "%ARM%"=="park-twice"      set "PLAN=plan-park.md"
if /i "%ARM%"=="park-twice"      set "SEEDF="
if /i "%ARM%"=="park-twice"      set "VARY=1"
if /i "%ARM%"=="park-twice"      set "REPORT=output-panelq.md"
if /i "%ARM%"=="park-twice"      set "S4=ruling keying"
if /i "%ARM%"=="park-twice"      set "S4AT=1 2"
if /i "%ARM%"=="park-twice"      set "ADVF=adv-park-twice.txt"
if /i "%ARM%"=="park-twice"      set "MAXI=3"
if /i "%ARM%"=="park-last"       set "WI=WI-criterion2.md"
if /i "%ARM%"=="park-last"       set "PLAN=plan-one-work.md"
if /i "%ARM%"=="park-last"       set "SEEDF="
if /i "%ARM%"=="park-last"       set "REPORT=output-panelq.md"
if /i "%ARM%"=="park-last"       set "S4=ruling money"
if /i "%ARM%"=="park-last"       set "MAXI=2"
if /i "%ARM%"=="park-only"       set "WI=WI-criterion2.md"
if /i "%ARM%"=="park-only"       set "PLAN=plan-park-only.md"
if /i "%ARM%"=="park-only"       set "SEEDF="
if /i "%ARM%"=="park-only"       set "REPORT=output-panelq.md"
if /i "%ARM%"=="park-only"       set "S4=ruling promise"
if /i "%ARM%"=="park-only"       set "MAXI=2"
if /i "%ARM%"=="park-unknown"    set "WI=WI-criterion.md"
if /i "%ARM%"=="park-unknown"    set "REPORT=output-panelq.md"
if /i "%ARM%"=="park-unknown"    set "S4=unknown"
if /i "%ARM%"=="park-unknown"    set "MAXI=2"
if /i "%ARM%"=="park-arbstop"    set "WI=WI-stop.md"
if /i "%ARM%"=="park-arbstop"    set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="park-arbstop"    set "MAXI=2"
if /i "%ARM%"=="park-fresh"      set "WI=WI-criterion.md"
if /i "%ARM%"=="park-fresh"      set "PLAN=plan-park.md"
if /i "%ARM%"=="park-fresh"      set "SEEDF="
if /i "%ARM%"=="park-fresh"      set "VARY=1"
if /i "%ARM%"=="park-fresh"      set "REPORT=output-panelq.md"
if /i "%ARM%"=="park-fresh"      set "S4=ruling keying"
if /i "%ARM%"=="park-fresh"      set "S4AT=1"
if /i "%ARM%"=="park-fresh"      set "ADVF=adv-park-fresh.txt"
if /i "%ARM%"=="park-fresh"      set "MAXI=2"
if /i "%ARM%"=="state-header-done"       set "WI=WI-step3.md"
if /i "%ARM%"=="state-header-done"       set "PLAN=plan-steps3.md"
if /i "%ARM%"=="state-header-done"       set "SEEDOUT=PHASE_OUTCOME-hdr-stale.md"
if /i "%ARM%"=="state-header-done"       set "FLIP=3.1"
if /i "%ARM%"=="state-header-done"       set "MAXI=1"
if /i "%ARM%"=="state-header-notstarted" set "WI=WI-criterion.md"
if /i "%ARM%"=="state-header-notstarted" set "SEEDOUT=PHASE_OUTCOME-hdr-notstarted.md"
if /i "%ARM%"=="state-header-notstarted" set "FLIP=2.3"
if /i "%ARM%"=="state-header-notstarted" set "MAXI=1"
if /i "%ARM%"=="target-not-zero" set "WI=WI-step3.md"
if /i "%ARM%"=="target-not-zero" set "PLAN=plan-steps04.md"
if /i "%ARM%"=="target-not-zero" set "SEEDOUT=PHASE_OUTCOME-hdr-zero.md"
if /i "%ARM%"=="target-not-zero" set "PROMPTONLY=1"

rem  084'S ARMS. The prompt arms compose and stop; the real-judge arms take no
rem  --seed so the arbiter's prompt is built and the judge reads a real WHY.
if /i "%ARM%"=="prime-first"    set "WI=WI-criterion.md"
if /i "%ARM%"=="prime-first"    set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="prime-first"    set "PROMPTONLY=1"
if /i "%ARM%"=="prime-first"    set "SEEDREPORT=..\watchdog\fixture-output.md"
if /i "%ARM%"=="prime-label"    set "WI=WI-criterion.md"
if /i "%ARM%"=="prime-label"    set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="prime-label"    set "PROMPTONLY=1"
if /i "%ARM%"=="prime-label"    set "SEEDREPORT=..\watchdog\fixture-output.md"
if /i "%ARM%"=="prime-big"      set "WI=WI-criterion.md"
if /i "%ARM%"=="prime-big"      set "SEEDOUT=PHASE_OUTCOME-plan.md"
if /i "%ARM%"=="prime-big"      set "PROMPTONLY=1"
if /i "%ARM%"=="prime-big"      set "SEEDREPORT=..\watchdog\fixture-output.md"
if /i "%ARM%"=="prime-big"      set "BIGREPORT=5"
if /i "%ARM%"=="why-mixed"      set "WI=WI-whymixed.md"
if /i "%ARM%"=="why-split"      set "WI=WI-whymixed-split.md"
if /i "%ARM%"=="why-split"      set "FOLLOWS=report"
if /i "%ARM%"=="drift-report"   set "WI=WI-whydrift.md"
if /i "%ARM%"=="drift-report"   set "REPORT=output-panelq.md"
if /i "%ARM%"=="drift-report"   set "REALJUDGE=1"
if /i "%ARM%"=="drift-report"   set "SEEDF="
if /i "%ARM%"=="drift-plan"     set "WI=WI-whyplan2.md"
if /i "%ARM%"=="drift-plan"     set "PLAN=plan-one-work.md"
if /i "%ARM%"=="drift-plan"     set "FLIP=2.2"
if /i "%ARM%"=="drift-plan"     set "REPORT=output-panelq.md"
if /i "%ARM%"=="drift-plan"     set "REALJUDGE=1"
if /i "%ARM%"=="drift-plan"     set "SEEDF="
if /i "%ARM%"=="drift-plan"     set "MAXI=2"
if /i "%ARM%"=="drift-twice"    set "WI=WI-criterion.md"
if /i "%ARM%"=="drift-twice"    set "PLAN=plan-park.md"
if /i "%ARM%"=="drift-twice"    set "FOLLOWS=report report"
if /i "%ARM%"=="drift-twice"    set "VARY=1"
if /i "%ARM%"=="drift-twice"    set "ADVF=adv-drift.txt"
if /i "%ARM%"=="drift-twice"    set "SEEDF="
if /i "%ARM%"=="drift-twice"    set "MAXI=3"
if /i "%ARM%"=="stop-over-why"  set "WI=WI-stopwhy.md"
if /i "%ARM%"=="stop-over-why"  set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="stop-over-why"  set "MAXI=2"
rem  085: budget-none is advance's shape with no --budget at all - BUDGETARG
rem  stays empty, which is the default every other arm already runs with, so
rem  what it asserts is the banner and the spend line the default now prints.
if /i "%ARM%"=="budget-none"    set "WI=WI-criterion.md"
if /i "%ARM%"=="budget-none"    set "FLIP=2.3"
rem  085: drift-all is drift-twice on a plan with ONE authorable criterion, so
rem  the second report answer parks the only route and the next iteration has
rem  nothing left but a criterion drifted on.
if /i "%ARM%"=="drift-all"      set "WI=WI-criterion2.md"
if /i "%ARM%"=="drift-all"      set "PLAN=plan-park-only.md"
if /i "%ARM%"=="drift-all"      set "FOLLOWS=report report"
if /i "%ARM%"=="drift-all"      set "VARY=1"
if /i "%ARM%"=="drift-all"      set "SEEDF="
if /i "%ARM%"=="drift-all"      set "MAXI=3"
rem  086: THE FOUR REVERSED ARMS gain a second iteration so the hand-back can
rem  be seen authoring again - bad, owner-advances and closed take .advances
rem  naming 2.2 for the arbiter's one call; blocker-twice keeps its three.
if /i "%ARM%"=="bad"            set "MAXI=2"
if /i "%ARM%"=="bad"            set "ADVF=adv-hb.txt"
if /i "%ARM%"=="owner-advances" set "MAXI=2"
if /i "%ARM%"=="owner-advances" set "ADVF=adv-hb.txt"
if /i "%ARM%"=="closed"         set "MAXI=2"
if /i "%ARM%"=="closed"         set "ADVF=adv-hb.txt"
rem  086's own arms. hb-noadv ships a decision block with no ADVANCES line at
rem  all, which the stand-in's .advances rewrite cannot repair, so it is handed
rem  back twice. hb-twice names the owner's 2.3 twice - the shipped WI, then
rem  the arbiter's first call - and 2.2 on its second. hb-blocker-park is
rem  blocker-twice with the blocker naming a criterion, so there is one to
rem  park. hb-unshaped's unit writes a report with no section 4. stop-over-hb
rem  is a keying stop with no ADVANCES field.
if /i "%ARM%"=="hb-noadv"       set "WI=WI-noadv.md"
if /i "%ARM%"=="hb-noadv"       set "MAXI=2"
if /i "%ARM%"=="hb-twice"       set "WI=WI-criterion.md"
if /i "%ARM%"=="hb-twice"       set "PLAN=plan-one-work.md"
if /i "%ARM%"=="hb-twice"       set "ADVF=adv-hb-twice.txt"
if /i "%ARM%"=="hb-twice"       set "MAXI=3"
if /i "%ARM%"=="hb-blocker-park" set "WI=WI-blocker-crit.md"
if /i "%ARM%"=="hb-blocker-park" set "MAXI=3"
if /i "%ARM%"=="hb-unshaped"    set "WI=WI-criterion.md"
if /i "%ARM%"=="hb-unshaped"    set "REPORT=output-unshaped.md"
if /i "%ARM%"=="stop-over-hb"   set "WI=WI-stop-noadv.md"
if /i "%ARM%"=="stop-over-hb"   set "MAXI=2"
rem  087's own arms. s4-retry: the section-4 judge answers unreadably on its
rem  first call and readably on the retry. arb-retry and arb-twice: the
rem  arbiter's first call fails, and its second succeeds or fails too; no seed,
rem  so the arbiter is called on iteration 1. arb-stop-retry: the retried
rem  arbiter returns a keying MOVE: stop, which must still halt. launch-twice:
rem  no repository, so every launch exits 7. reload-twice: a directory where
rem  reload.txt goes, so every reload leaves nothing. deny-park: the unit is
rem  denied and does not complete. fail-noseed: --seed with the shipped
rem  instruction deleted, refused at the door.
if /i "%ARM%"=="s4-retry"       set "WI=WI-criterion.md"
if /i "%ARM%"=="s4-retry"       set "REPORT=output-panelq.md"
if /i "%ARM%"=="s4-retry"       set "S4=unknown"
if /i "%ARM%"=="s4-retry"       set "S4AT=1"
if /i "%ARM%"=="arb-retry"      set "WI=WI-criterion.md"
if /i "%ARM%"=="arb-retry"      set "ARBFAILAT=1"
if /i "%ARM%"=="arb-retry"      set "SEEDF="
if /i "%ARM%"=="arb-retry"      set "FLIP=2.3"
if /i "%ARM%"=="arb-twice"      set "WI=WI-criterion.md"
if /i "%ARM%"=="arb-twice"      set "ARBFAILAT=1 2"
if /i "%ARM%"=="arb-twice"      set "SEEDF="
if /i "%ARM%"=="arb-twice"      set "MAXI=2"
if /i "%ARM%"=="arb-stop-retry" set "WI=WI-stop.md"
if /i "%ARM%"=="arb-stop-retry" set "ARBFAILAT=1"
if /i "%ARM%"=="arb-stop-retry" set "SEEDF="
if /i "%ARM%"=="arb-stop-retry" set "MAXI=2"
if /i "%ARM%"=="launch-twice"   set "WI=WI-criterion.md"
if /i "%ARM%"=="launch-twice"   set "NOGIT=1"
if /i "%ARM%"=="deny-park"      set "WI=WI-criterion.md"
if /i "%ARM%"=="deny-park"      set "DENY=1"
if /i "%ARM%"=="fail-noseed"    set "WI=WI-criterion.md"
if /i "%ARM%"=="fail-noseed"    set "NOWI=1"
rem  088's own arms. stop-answered: the keying stop of WI-stop.md with a
rem  resolution row whose phrase is copied from its why line - answered, not
rem  re-raised, the arbiter redirected. stop-other-answer: a row for some other
rem  question - halts. stale-unit: WI-stop-stale's claim that unit 439 is
rem  executing, false because the lock is free - handed back, twice parks 2.3.
rem  stale-plan-current: a phase named in quotes that the record's PHASE: line
rem  carries - the premise holds, halts. stale-plan-gone: a phase the plan does
rem  not name - handed back. All seeded so the launcher runs the shipped stop
rem  on iteration 1 and the stand-in re-raises it after.
if /i "%ARM%"=="stop-answered"      set "WI=WI-stop.md"
if /i "%ARM%"=="stop-answered"      set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="stop-answered"      set "MAXI=2"
if /i "%ARM%"=="stop-answered"      set "RESOLVED=changing what the radio does on transmit"
if /i "%ARM%"=="stop-other-answer"  set "WI=WI-stop.md"
if /i "%ARM%"=="stop-other-answer"  set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="stop-other-answer"  set "MAXI=2"
if /i "%ARM%"=="stop-other-answer"  set "RESOLVED=the colour of the panel chassis plate"
if /i "%ARM%"=="stale-unit"         set "WI=WI-stop-stale.md"
if /i "%ARM%"=="stale-unit"         set "SEEDOUT=PHASE_OUTCOME-ex-empty.md"
if /i "%ARM%"=="stale-unit"         set "MAXI=3"
if /i "%ARM%"=="stale-plan-current" set "WI=WI-stop-plan.md"
if /i "%ARM%"=="stale-plan-current" set "SEEDOUT=PHASE_OUTCOME-noadv-same.md"
if /i "%ARM%"=="stale-plan-current" set "MAXI=2"
if /i "%ARM%"=="stale-plan-gone"    set "WI=WI-stop-plan-gone.md"
if /i "%ARM%"=="stale-plan-gone"    set "SEEDOUT=PHASE_OUTCOME-noadv-same.md"
if /i "%ARM%"=="stale-plan-gone"    set "MAXI=2"
if /i "%ARM%"=="transport" goto :transport
if /i "%ARM%"=="dupplan" goto :dupplan
if /i "%ARM%"=="attempt-transport" goto :attempttransport
if not defined WI goto :usage
for %%I in ("%HERE%..\..") do set "ARB=%%~fI\"

set "FROOT=%TEMP%\cps-progress-fixture-%ARM%"
if exist "%FROOT%" rd /s /q "%FROOT%"
mkdir "%FROOT%"
git init -q "%FROOT%"
>"%FROOT%\PROJECT_CARD.md" echo PROJECT: progress-fixture-%ARM%
copy /y "%HERE%%PLAN%" "%FROOT%\PHASE_PLAN.md" >nul
copy /y "%HERE%%WI%" "%FROOT%\WORK_INSTRUCTIONS.md" >nul
copy /y "%HERE%..\seed\ARBITER.md" "%FROOT%\ARBITER.md" >nul
copy /y "%HERE%..\seed\CLAUDE.md" "%FROOT%\CLAUDE.md" >nul
if defined FLIP >"%FROOT%\.flip" echo %FLIP%
if defined FLIPAT >"%FROOT%\.flipat" echo %FLIPAT%
if defined CLOSED copy /y "%HERE%PHASE_OUTCOME-closed.md" "%FROOT%\PHASE_OUTCOME.md" >nul
if defined SEEDOUT copy /y "%HERE%%SEEDOUT%" "%FROOT%\PHASE_OUTCOME.md" >nul
if defined VARY >"%FROOT%\.vary" echo the arbiter authors a different approach on every call
rem  080: GATED ON A FLAG FILE, exactly as .vary and .flip are, so only the arm
rem  that writes it waits. Every other arm's timing is load-bearing for three
rem  nights of recorded results and none of it moves by a second.
if defined SLOW >"%FROOT%\.slow" echo %SLOW%
rem  071: .exhaust makes the stand-in rewrite MOVE: to exhausted. Its body is
rem  a criterion when the arm claims one other than the instruction-s, and the
rem  word none when it claims none at all - which is what the blocker form
rem  does, and what :exhaustnocrit is for.
if defined EXHAUST >"%FROOT%\.exhaust" echo %EXHAUST%
rem  083: the section-4 judge's answer, which section-4 calls get it, and the
rem  criterion the stand-in arbiter names on each call.
if defined S4 >"%FROOT%\.s4" echo %S4%
if defined S4AT for %%N in (%S4AT%) do >>"%FROOT%\.s4at" echo %%N
if defined ADVF copy /y "%HERE%%ADVF%" "%FROOT%\.advances" >nul
if defined FOLLOWS for %%N in (%FOLLOWS%) do >>"%FROOT%\.follows" echo %%N
rem  087: the hiccups. ARBFAILAT lists the arbiter calls that fail; DENY makes
rem  the unit report a denial it could not work around; NOGIT removes the
rem  repository so run-unit exits 7 on every launch; NOWI deletes the shipped
rem  instruction so a --seed has nothing to run.
if defined ARBFAILAT for %%N in (%ARBFAILAT%) do >>"%FROOT%\.arbfailat" echo %%N
if defined DENY >"%FROOT%\.deny" echo the unit is denied a call and does not complete
if defined NOGIT rd /s /q "%FROOT%\.git"
if defined NOWI del /q "%FROOT%\WORK_INSTRUCTIONS.md" 2>nul
rem  088: RESOLVED seeds PARKED.md with one resolution row in the form the owner
rem  writes by hand - RESOLVED: date | phrase from the stop's why line | his
rem  decision - written by PowerShell from the environment, never composed on
rem  a cmd line (CPS-DEC-021).
if defined RESOLVED powershell -NoProfile -Command "$f='%FROOT%\PARKED.md'; $ls=@('# PARKED.md', '', 'Seeded by run-fixture.bat for unit 088: one resolution row the owner wrote.', '', ('RESOLVED: 2026-09-25 13:34 | ' + [string]$env:RESOLVED + ' | fixture owner - proceed, the question is answered; the new plan is at the root and the unit was killed')); [IO.File]::WriteAllText($f, ($ls -join [Environment]::NewLine) + [Environment]::NewLine, (New-Object Text.UTF8Encoding($false)))"
if defined NOPLAN del /q "%FROOT%\PHASE_PLAN.md" 2>nul
rem  072: a report at the root BEFORE the loop runs, so an arm can prove that
rem  an ENDING leaves it exactly where the panel looks for it. Ruling 7 and
rem  CPS-DEC-044: the panel decides a cycle ended from the root output.md-s
rem  mtime, and nothing in this unit may disturb that.
if defined ROOTREPORT copy /y "%HERE%..\watchdog\fixture-output.md" "%FROOT%\output.md" >nul
rem  073: DROPSTEP removes a step line from the OUTCOME HEADER only - the plan
rem  keeps it - so the fixture root looks like this repository did before this
rem  unit: a plan carrying a step the record has never heard of. ADDSTEP puts a
rem  step in the HEADER that the plan does not carry, which is the other
rem  direction and must be left alone. Both edit the seeded copy, never the
rem  fixture's own files.
rem  074: THE CARD, SEEDED AND SNAPSHOTTED. card.orig is the card exactly as
rem  it stood before anything ran, and it is what the byte proof compares
rem  against. CARDRO makes the file READ-ONLY: the read still succeeds and the
rem  WRITE fails, which is the half of a locked file that matters here - it is
rem  the moment a routine that rewrote lines would leave a half-written card.
if defined CARD copy /y "%HERE%%CARD%" "%FROOT%\PHASE_STATUS.md" >nul
if defined CARD copy /y "%FROOT%\PHASE_STATUS.md" "%FROOT%\card.orig" >nul
if defined CARDRO attrib +R "%FROOT%\PHASE_STATUS.md"
if defined DROPSTEP powershell -NoProfile -Command "$f='%FROOT%\PHASE_OUTCOME.md'; $t=[IO.File]::ReadAllText($f); $t=[regex]::Replace($t, '(?m)^STEP: *%DROPSTEP% *\|.*\r?\n', ''); [IO.File]::WriteAllText($f, $t, (New-Object Text.UTF8Encoding($true)))"
if defined ADDSTEP powershell -NoProfile -Command "$f='%FROOT%\PHASE_OUTCOME.md'; $t=[IO.File]::ReadAllText($f); $m=[regex]::Matches($t, '(?m)^STEP: *[0-9]+ *\|.*$'); if($m.Count -gt 0){ $x=$m[$m.Count-1]; $at=$t.IndexOf([char]10, $x.Index) + 1; $t=$t.Substring(0,$at) + 'STEP: %ADDSTEP% | not started | a step the plan does not carry' + [char]13 + [char]10 + $t.Substring($at) }; [IO.File]::WriteAllText($f, $t, (New-Object Text.UTF8Encoding($true)))"
rem  069: a previous report placed at the root before the loop starts, because
rem  the prompt arms compose a prompt and never run a unit to write one. Where
rem  BIGREPORT is set the same report is repeated that many times, so the arm
rem  measures what a report LONG ENOUGH TO CROWD THE PROMPT does - criterion 6.6.
if not defined SEEDREPORT goto :noseedreport
copy /y "%HERE%%SEEDREPORT%" "%FROOT%\output.md" >nul
if not defined BIGREPORT goto :noseedreport
powershell -NoProfile -Command "$f='%FROOT%\output.md'; $t=[IO.File]::ReadAllText($f); $n=[int]'%BIGREPORT%'; $o=$t; for($i=1; $i -lt $n; $i++){ $o=$o + $t }; [IO.File]::WriteAllText($f, $o, (New-Object Text.UTF8Encoding($false))); '  the seeded report was multiplied to ' + $o.Length + ' bytes'"
:noseedreport
if defined REPORT >"%FROOT%\.report" echo %REPORT%
if defined REALJUDGE >"%FROOT%\.realjudge" echo the state judge is the real claude for this arm
rem  067: WHICH arbiter call leaks. With --seed the arbiter is not called on
rem  iteration 1, so iteration 2-s call is the first one the stand-in sees.
if defined LEFTOVER >"%FROOT%\.leftover" echo 1
if defined NOREPORT >"%FROOT%\.noreport" echo the unit writes no output.md at all
rem  067: the destination already occupied, as it is on a restarted loop -
rem  066 measured that the unit number counts from 1 again.
if not defined PREKEPT goto :noprekept
mkdir "%FROOT%\.run-unit\reports"
>"%FROOT%\.run-unit\reports\unit-1-output.md" echo AN EARLIER LOOP-S UNIT 1 REPORT. Nothing may overwrite this.
:noprekept

set "PATH=%HERE%;%PATH%"

rem  067: THE LOCK, TAKEN BEFORE THE LOOP. --pid says who holds it, so the
rem  arm chooses a dead owner - 999999, which is not a process here - or a
rem  live one, 4, the System process. Never an age: the owner-s ruling of
rem  2026-09-12.
if defined TAKEPID call "%ARB%lock.bat" take --pid %TAKEPID% "%FROOT%" >nul
if defined TAKEPID echo  lock pre-taken by pid %TAKEPID% - see SESSION.lock at the root

echo.
echo ============================================================
echo  progress fixture : %ARM%
echo  root             : %FROOT%
echo  plan             : %PLAN%   instruction: %WI%   flip: %FLIP% at unit: %FLIPAT%
echo  iterations       : %MAXI%   seed: %SEEDF%   report: %REPORT%   real judge: %REALJUDGE%
rem  080: printed only where it is set, so no other arm's console gains a line.
if defined MINUTESARG echo  ceiling          : %MINUTESARG%   unit stand-in waits: %SLOW%s
echo  claude resolves to the stand-in:
where claude
echo ============================================================

rem  067: CAPTURED, THEN PRINTED. See the header. The exit code survives a
rem  redirect on a call and is read on the next line as it always was.
rem  069: the prompt arms compose the prompt and stop. --fixture promptonly calls
rem  :writearbprompt itself, so what is proved is the prompt the loop builds.
rem  074: THE TWO RECONCILIATIONS AND NOTHING ELSE. --fixture reconcile calls
rem  :reconcile and :reconcilecard and exits, so no unit launches, no beat is
rem  written and :phasesteps never runs - which is the only state in which
rem  'remove the insertion and the card is byte-identical' says anything.
if not defined RECONLY goto :nocardonly
call "%ARB%run-phase.bat" "%FROOT%" --fixture reconcile >"%FROOT%\console.txt" 2>&1
set "RRC=%ERRORLEVEL%"
if defined CARDRO attrib -R "%FROOT%\PHASE_STATUS.md" >nul 2>&1
if /i "%ARM%"=="card-panel" echo.>>"%FROOT%\console.txt"
if /i "%ARM%"=="card-panel" node "%HERE%read-card.js" "%FROOT%\PHASE_STATUS.md" 2 >>"%FROOT%\console.txt" 2>&1
type "%FROOT%\console.txt"
goto :verdict
:nocardonly
if not defined PROMPTONLY goto :fullrun
call "%ARB%run-phase.bat" "%FROOT%" --fixture promptonly >"%FROOT%\console.txt" 2>&1
set "RRC=%ERRORLEVEL%"
type "%FROOT%\console.txt"
goto :verdict
:fullrun
call "%ARB%run-phase.bat" "%FROOT%" %SEEDF% %BUDGETARG% %MINUTESARG% --max-iterations %MAXI% >"%FROOT%\console.txt" 2>&1
set "RRC=%ERRORLEVEL%"
type "%FROOT%\console.txt"

rem  067: lock-live is refused at run-phase-s door, which does not name the
rem  pid because it never reaches run-unit.bat. So run-unit.bat is ALSO
rem  driven directly, and THAT refusal is the one that must name pid 4 as
rem  alive. --dry-run, so nothing would launch even if the lock were free.
rem  074: the read-only card is released before anything reads it back, so the
rem  next run of this arm can delete the root. The run is over; nothing the
rem  loop does can be affected by it.
if defined CARDRO attrib -R "%FROOT%\PHASE_STATUS.md" >nul 2>&1
if /i not "%ARM%"=="lock-live" goto :nodirect
echo.
echo  ---- lock-live: run-unit.bat driven directly at the same root ----
call "%ARB%run-unit.bat" 1 "%FROOT%" --dry-run >>"%FROOT%\console.txt" 2>&1
set "URC=%ERRORLEVEL%"
type "%FROOT%\console.txt"
call "%ARB%lock.bat" release "%FROOT%" >nul
:nodirect
rem  066: the attempts at 2.3, read back by attempt-read.bat once the loop's
rem  process has exited - in a file, so the verdict reads what the reader
rem  printed rather than a second parse of the record.
if /i "%ARM:~0,8%"=="attempt-" call "%ARB%attempt-read.bat" 2.3 "%FROOT%\PHASE_OUTCOME.md" >"%FROOT%\attempts-1.txt" 2>&1

set "RRC2="
set "SH1=absent"
set "MT1=absent"
set "SH2=absent"
set "MT2=absent"
if /i "%ARM%"=="attempt-restart" goto :restart
rem  072: end-twice takes the same second pass - two whole loops over one root,
rem  proving the sheet is written by the first and left alone by the second.
if /i "%ARM%"=="end-twice" goto :secondpass
rem  083: park-fresh is a NEW loop over park-one's root. The new process must
rem  clear the mark at its door and be free to author the parked criterion.
if /i "%ARM%"=="park-fresh" goto :secondpass
if /i not "%ARM%"=="owner-again" goto :verdict
:secondpass
for /f "usebackq tokens=1,2" %%A in (`powershell -NoProfile -Command "$s='%FROOT%\review\sheet.md'; if(Test-Path -LiteralPath $s){ (Get-FileHash -LiteralPath $s -Algorithm SHA256).Hash + ' ' + (Get-Item -LiteralPath $s).LastWriteTimeUtc.Ticks } else { 'absent absent' }"`) do set "SH1=%%A" & set "MT1=%%B"
echo.
echo  ---- %ARM%: the second pass over the same root ----
call "%ARB%run-phase.bat" "%FROOT%" %SEEDF% --max-iterations %MAXI% >>"%FROOT%\console.txt" 2>&1
set "RRC2=%ERRORLEVEL%"
type "%FROOT%\console.txt"
for /f "usebackq tokens=1,2" %%A in (`powershell -NoProfile -Command "$s='%FROOT%\review\sheet.md'; if(Test-Path -LiteralPath $s){ (Get-FileHash -LiteralPath $s -Algorithm SHA256).Hash + ' ' + (Get-Item -LiteralPath $s).LastWriteTimeUtc.Ticks } else { 'absent absent' }"`) do set "SH2=%%A" & set "MT2=%%B"
goto :verdict

rem  066: THE RESTART. The first loop's process has exited; a new one is
rem  started over the same root. Nothing survives between them but the files.
:restart
echo.
echo  ---- attempt-restart: a new loop over the same root ----
if defined WI2 copy /y "%HERE%%WI2%" "%FROOT%\WORK_INSTRUCTIONS.md" >nul
if defined WI2 echo  the second loop runs %WI2% - a different route to the same criterion
call "%ARB%run-phase.bat" "%FROOT%" %SEEDF% --max-iterations %MAXI% >>"%FROOT%\console.txt" 2>&1
set "RRC2=%ERRORLEVEL%"
type "%FROOT%\console.txt"
call "%ARB%attempt-read.bat" 2.3 "%FROOT%\PHASE_OUTCOME.md" >"%FROOT%\attempts-2.txt" 2>&1

:verdict
echo.
echo ============================================================
echo  RESULT : %ARM%
echo ============================================================
rem  071: 066-s ATTEMPT ARMS NOW HAVE THEIR OWN CALL TOO. See :verdict066
rem  for what tipped the 064 line past 8191 and why reading could not catch it.
if /i "%ARM%"=="attempt-one"          goto :verdict066
if /i "%ARM%"=="attempt-three"        goto :verdict066
if /i "%ARM%"=="attempt-restart"      goto :verdict066
if /i "%ARM%"=="attempt-blocker-crit" goto :verdict066
if /i "%ARM%"=="attempt-blocker-unit" goto :verdict066
rem  067's ARMS ARE JUDGED BY THEIR OWN CALL. The line below is 7139
rem  characters and cmd refuses one past 8191; see the header.
if /i "%ARM%"=="leftover"   goto :verdict067
if /i "%ARM%"=="lockleak"   goto :verdict067
if /i "%ARM%"=="lock-dead"  goto :verdict067
if /i "%ARM%"=="lock-live"  goto :verdict067
if /i "%ARM%"=="noreport"   goto :verdict067
if /i "%ARM%"=="kept"       goto :verdict067
if /i "%ARM%"=="kept-twice" goto :verdict067
if /i "%ARM%"=="repeat"     goto :verdict068
if /i "%ARM%"=="fresh"      goto :verdict068
if /i "%ARM%"=="succeeded"  goto :verdict068
if /i "%ARM%"=="othercrit"  goto :verdict068
if /i "%ARM%"=="noid"       goto :verdict068
if /i "%ARM%"=="restart-id" goto :verdict068
if /i "%ARM%"=="arbrecord"  goto :verdict068
if /i "%ARM%"=="neverran"   goto :verdict068
rem  078's TWO ARMS GET THEIR OWN CALL, at :verdict078, for the reason every
rem  call before it gives: the 064 verdict line is 7267 characters and cmd
rem  refuses one past 8191. Measured tonight rather than assumed - the longest
rem  line in this file is :verdict068's own at 5414, which leaves 2777, and
rem  the two cases below would not fit inside it with their evidence intact.
rem  The proven :verdict068 line is not touched at all.
if /i "%ARM%"=="reword"     goto :verdict078
if /i "%ARM%"=="nearmiss"   goto :verdict078
if /i "%ARM%"=="promptorder"   goto :verdict069
if /i "%ARM%"=="promptbig"     goto :verdict069
if /i "%ARM%"=="sharecheck"    goto :verdict069
if /i "%ARM%"=="whyplan"       goto :verdict069
if /i "%ARM%"=="whyreport"     goto :verdict069
if /i "%ARM%"=="whyboth"       goto :verdict069
if /i "%ARM%"=="followsreport" goto :verdict069
if /i "%ARM%"=="followsplan"   goto :verdict069
if /i "%ARM%"=="redirect-same"    goto :verdict070
if /i "%ARM%"=="redirect-fresh"   goto :verdict070
if /i "%ARM%"=="redirect-wander"  goto :verdict070
if /i "%ARM%"=="redirect-repeat"  goto :verdict070
if /i "%ARM%"=="exhaust-three"  goto :verdict071
if /i "%ARM%"=="end-exhausted"  goto :verdict072
if /i "%ARM%"=="rec-append"  goto :verdict073
if /i "%ARM%"=="rec-twice"  goto :verdict073
if /i "%ARM%"=="rec-match"  goto :verdict073
if /i "%ARM%"=="rec-extra"  goto :verdict073
if /i "%ARM%"=="rec-done"  goto :verdict073
if /i "%ARM%"=="rec-owner"  goto :verdict073
if /i "%ARM%"=="card-append"  goto :verdict074
if /i "%ARM%"=="card-match"   goto :verdict074
if /i "%ARM%"=="card-extra"   goto :verdict074
if /i "%ARM%"=="card-locked"  goto :verdict074
if /i "%ARM%"=="card-absent"  goto :verdict074
if /i "%ARM%"=="card-panel"   goto :verdict074
if /i "%ARM%"=="card-twice"   goto :verdict074
if /i "%ARM%"=="card-steps"   goto :verdict074
if /i "%ARM%"=="card-frozen"  goto :verdict074
if /i "%ARM%"=="end-three"  goto :verdict072
if /i "%ARM%"=="end-owner"  goto :verdict072
if /i "%ARM%"=="end-twice"  goto :verdict072
if /i "%ARM%"=="fail-stop11"  goto :verdict072
if /i "%ARM%"=="fail-maxiter"  goto :verdict072
if /i "%ARM%"=="fail-noplan"  goto :verdict072
if /i "%ARM%"=="exhaust-empty"  goto :verdict071
if /i "%ARM%"=="exhaust-two"  goto :verdict071
if /i "%ARM%"=="exhaust-same"  goto :verdict071
if /i "%ARM%"=="exhaust-noreason"  goto :verdict071
if /i "%ARM%"=="exhaust-stop"  goto :verdict071
if /i "%ARM%"=="exhaust-stop-money"  goto :verdict071
if /i "%ARM%"=="exhaust-stop-promise"  goto :verdict071
if /i "%ARM%"=="exhaust-owner"  goto :verdict071
if /i "%ARM%"=="redirect-refused" goto :verdict070
if /i "%ARM%"=="redirect-budget"  goto :verdict070
if /i "%ARM%"=="redirect-maxiter" goto :verdict070
if /i "%ARM%"=="redirect-owner"   goto :verdict070
if /i "%ARM%"=="redirect-stop11"  goto :verdict070
if /i "%ARM%"=="redirect-blocker" goto :verdict070
if /i "%ARM%"=="redirect-minutes" goto :verdict070
if /i "%ARM%"=="park-one"        goto :verdict083
if /i "%ARM%"=="park-twice"      goto :verdict083
if /i "%ARM%"=="park-last"       goto :verdict083
if /i "%ARM%"=="park-only"       goto :verdict083
if /i "%ARM%"=="park-unknown"    goto :verdict083
if /i "%ARM%"=="park-arbstop"    goto :verdict083
if /i "%ARM%"=="park-fresh"      goto :verdict083
if /i "%ARM%"=="state-header-done"       goto :verdict083b
if /i "%ARM%"=="state-header-notstarted" goto :verdict083b
if /i "%ARM%"=="target-not-zero" goto :verdict083b
if /i "%ARM%"=="prime-first"    goto :verdict084
if /i "%ARM%"=="prime-label"    goto :verdict084
if /i "%ARM%"=="prime-big"      goto :verdict084
if /i "%ARM%"=="why-mixed"      goto :verdict084
if /i "%ARM%"=="why-split"      goto :verdict084
if /i "%ARM%"=="drift-report"   goto :verdict084
if /i "%ARM%"=="drift-plan"     goto :verdict084
if /i "%ARM%"=="drift-twice"    goto :verdict084
if /i "%ARM%"=="stop-over-why"  goto :verdict084
if /i "%ARM%"=="budget-none"    goto :verdict085
if /i "%ARM%"=="drift-all"      goto :verdict085
if /i "%ARM%"=="hb-noadv"       goto :verdict086
if /i "%ARM%"=="hb-twice"       goto :verdict086
if /i "%ARM%"=="hb-blocker-park" goto :verdict086
if /i "%ARM%"=="hb-unshaped"    goto :verdict086
if /i "%ARM%"=="stop-over-hb"   goto :verdict086
if /i "%ARM%"=="s4-retry"       goto :verdict087
if /i "%ARM%"=="arb-retry"      goto :verdict087
if /i "%ARM%"=="arb-twice"      goto :verdict087
if /i "%ARM%"=="arb-stop-retry" goto :verdict087
if /i "%ARM%"=="launch-twice"   goto :verdict087
if /i "%ARM%"=="deny-park"      goto :verdict087
if /i "%ARM%"=="fail-noseed"    goto :verdict087
if /i "%ARM%"=="stop-answered"      goto :verdict088
if /i "%ARM%"=="stop-other-answer"  goto :verdict088
if /i "%ARM%"=="stale-unit"         goto :verdict088
if /i "%ARM%"=="stale-plan-current" goto :verdict088
if /i "%ARM%"=="stale-plan-gone"    goto :verdict088
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r + '\calls.txt')){ $c=@(Get-Content -LiteralPath ($r + '\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $adv=@(); $o=$r + '\PHASE_OUTCOME.md'; $alines=@(); if(Test-Path -LiteralPath $o){ $adv=@(Select-String -Path $o -Pattern '^ADVANCED: ' | ForEach-Object { $_.Line.Substring(10) }); $alines=@(Select-String -Path $o -Pattern '^ATTEMPT: ' | ForEach-Object { $_.Line }) }; $a1=@(); if(Test-Path -LiteralPath ($r + '\attempts-1.txt')){ $a1=@(Get-Content -LiteralPath ($r + '\attempts-1.txt') | Where-Object { $_ -ne '' }) }; $a2=@(); if(Test-Path -LiteralPath ($r + '\attempts-2.txt')){ $a2=@(Get-Content -LiteralPath ($r + '\attempts-2.txt') | Where-Object { $_ -ne '' }) }; $a1=@($a1 | ForEach-Object { $_ -replace ' launched \S+','' }); $a2=@($a2 | ForEach-Object { $_ -replace ' launched \S+','' }); $led=''; $all=''; $l=$r + '\RUN_LEDGER.md'; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $plan=[IO.File]::ReadAllText($r + '\PHASE_PLAN.md'); $flipped=$plan.Contains('- [x] 2.3 '); $sp=$r + '\review\sheet.md'; $sheet=Test-Path -LiteralPath $sp; $st=''; if($sheet){ $st=[IO.File]::ReadAllText($sp) }; $nl=[string][char]10; $ap='move criterion 2.3 - the fixture is killed at ten quiet looks'; $v1='read the watchdog log rather than polling any process tree'; $v2='compare each process creation time against the launch stamp on disk'; $bp='clear what stops criterion 2.3 being counted, moving no criterion'; 'run-phase exit : %RRC%'; if('%RRC2%' -ne ''){ 'second pass    : exit %RRC2%' }; if(('%ARM%' -eq 'owner-again') -or ('%ARM%' -eq 'end-twice')){ 'sheet, pass 1  : %SH1% %MT1%'; 'sheet, pass 2  : %SH2% %MT2%' }; 'calls          : ' + ($c -join ', '); 'units run      : ' + $units; 'ADVANCED lines : ' + ($adv -join ', '); 'plan 2.3 met   : ' + $flipped; 'review sheet   : ' + $sheet; 'halt line      : ' + $led; $back=$led.Contains('backstop'); $owait=$led.Contains('stop 1: the phase is waiting on the owner'); switch($arm){ 'advance' { $ok=($units -eq 1) -and ($adv -join ',') -eq 'yes' -and $back -and $flipped; $must='one unit, ADVANCED yes, the loop continued to its backstop' } 'noadvance' { $ok=($units -eq 1) -and ($adv -join ',') -eq 'no' -and $back -and (-not $flipped); $must='one unit, ADVANCED no, the loop continued to its backstop' } 'twice' { $ok=($units -eq 3) -and ($adv -join ',') -eq 'no,no,no' -and $back -and $all.Contains('redirected - no-advance at one criterion'); $must='THREE units, all no, NO HALT - the loop redirected after the second and ran a third, reaching its backstop, and the ledger carries the redirect note' } 'closed' { $ok=($units -eq 0) -and $back -and $all.Contains('redirected - step 2 is done and closed') -and (-not $led.Contains('refused')); $must='REVERSED BY 086 - 064 asserted: no unit ran, refused because step 2 is done and closed, on the halt line. Now: handed back - the ledger carries redirected - step 2 is done and closed, the arbiter aimed at the closed step again and was handed back again, nothing launched, and the loop reached its backstop' } 'bad' { $ok=($units -eq 1) -and $back -and $all.Contains('redirected - ADVANCES malformed') -and (-not $led.Contains('refused')); $must='REVERSED BY 086 - 064 asserted: no unit ran, refused because ADVANCES named no criterion. Now: handed back with the malformed field quoted, the arbiter authored again naming 2.2, one unit ran, and the loop reached its backstop' } 'blocker' { $ok=($units -eq 1) -and ($adv -join ',') -eq 'blocker' -and $back; $must='one unit, ADVANCED blocker, the loop continued to its backstop' } 'blocker-twice' { $ok=($units -eq 3) -and (($adv -join ',') -eq 'blocker,blocker,blocker') -and $back -and $all.Contains('redirected - two consecutive blocker-clears') -and (-not $all.Contains('halted: two consecutive')) -and (-not $led.Contains('stop 10')); $must='REVERSED BY 086 - 064 asserted: two units, both blocker, STILL HALTED by the blocker rule, not redirected either because a blocker-clear names no criterion to be sent back to. Now: the two blocker-clears named only a unit, so there is nothing to park, the arbiter is redirected to the plan, a third unit runs, and the loop reaches its backstop - no halt' } 'owner-sheet' { $ok=('%RRC%' -eq '0') -and ($c.Count -eq 0) -and $owait -and $led.Contains('2.2, 2.3') -and $sheet -and $st.Contains('2.2 the operator agrees the card reads correctly') -and $st.Contains('2.3 the owner says the phase passed'); $must='exit 0, no call at all, stop 1 waiting on the owner naming 2.2 and 2.3, and the sheet written with both' } 'owner-again' { $ok=('%RRC%' -eq '0') -and ('%RRC2%' -eq '0') -and ($c.Count -eq 0) -and $owait -and ('%SH1%' -ne 'absent') -and ('%SH1%' -eq '%SH2%') -and ('%MT1%' -eq '%MT2%'); $must='both passes exit 0 at stop 1, no call at all, the sheet the same content and the same write time after the second pass' } 'owner-nosheet' { $ok=('%RRC%' -eq '0') -and ($c.Count -eq 0) -and $owait -and (-not (Test-Path -LiteralPath ($r + '\review'))); $must='exit 0, no call at all, stop 1 waiting on the owner, and no sheet' } 'one-work' { $ok=(($c -join ',') -eq 'ARBITER,UNIT,JUDGE') -and $back -and (-not $owait) -and (-not (Test-Path -LiteralPath ($r + '\review'))); $must='no halt: the arbiter authored, the unit ran, the loop reached its backstop, and no sheet' } 'owner-advances' { $ok=($units -eq 1) -and $back -and $all.Contains('redirected - ADVANCES names the owner') -and (-not $led.Contains('refused')); $must='REVERSED BY 086 - 065 asserted: no unit ran, refused because ADVANCES named an owner criterion. Now: handed back naming the owner-s line, the arbiter authored again at 2.2, one unit ran, and the loop reached its backstop' } 'reversal' { $ok=($units -eq 1) -and $led.Contains('a unit report claims a ruling that reverses an earlier arbiter ruling') -and $led.Contains('46'); $must='one unit, halted by the reversal rule, the reversed ruling 46 quoted' } 'nine' { $ok=($units -eq 1) -and $back -and (-not $led.Contains('reverses')); $must='one unit, nothing refused and no reversal halt, the loop reached its backstop' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  066'S ATTEMPT ARMS, JUDGED BY THEIR OWN CALL - THE FIFTH SPLIT, AND THE
rem  REASON IS THE ONE EVERY EARLIER SPLIT GIVES: cmd refuses a command line
rem  past 8191 characters.
rem
rem  WHAT TIPPED IT, said plainly because it is a fault this unit introduced.
rem  071 added a REASON_n to two of these arms' expected blocks and one shared
rem  $cl constant - about 210 characters - taking the 064 call from 7644 to
rem  7855. That still fits. But owner-again expands %SH1%, %SH2%, %MT1% and
rem  %MT2% into the SAME line - two sha256 hashes used five times between them
rem  plus two timestamps - which adds about 375 more, and 8230 is past the
rem  limit. cmd said only 'The input line is too long' and the arm failed at
rem  exit 255 with no verdict at all.
rem
rem  IT WAS NOT CAUGHT BY READING AND COULD NOT HAVE BEEN: the line is under
rem  the limit as written and only one arm's variables push it over. Moving
rem  these five arms out takes the 064 call back under 6200, which leaves room
rem  for the expansion several times over.
:verdict066
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r + '\calls.txt')){ $c=@(Get-Content -LiteralPath ($r + '\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $adv=@(); $o=$r + '\PHASE_OUTCOME.md'; $alines=@(); if(Test-Path -LiteralPath $o){ $adv=@(Select-String -Path $o -Pattern '^ADVANCED: ' | ForEach-Object { $_.Line.Substring(10) }); $alines=@(Select-String -Path $o -Pattern '^ATTEMPT: ' | ForEach-Object { $_.Line }) }; $a1=@(); if(Test-Path -LiteralPath ($r + '\attempts-1.txt')){ $a1=@(Get-Content -LiteralPath ($r + '\attempts-1.txt') | Where-Object { $_ -ne '' }) }; $a2=@(); if(Test-Path -LiteralPath ($r + '\attempts-2.txt')){ $a2=@(Get-Content -LiteralPath ($r + '\attempts-2.txt') | Where-Object { $_ -ne '' }) }; $a1=@($a1 | ForEach-Object { $_ -replace ' launched \S+','' }); $a2=@($a2 | ForEach-Object { $_ -replace ' launched \S+','' }); $led=''; $l=$r + '\RUN_LEDGER.md'; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1) }; $plan=[IO.File]::ReadAllText($r + '\PHASE_PLAN.md'); $flipped=$plan.Contains('- [x] 2.3 '); $nl=[string][char]10; $ap='move criterion 2.3 - the fixture is killed at ten quiet looks'; $v1='read the watchdog log rather than polling any process tree'; $v2='compare each process creation time against the launch stamp on disk'; $bp='clear what stops criterion 2.3 being counted, moving no criterion'; $cl='the unit ran to completion and the criterion did not flip from unmet to met'; $back=$led.Contains('backstop'); 'run-phase exit : %RRC%'; if('%RRC2%' -ne ''){ 'second pass    : exit %RRC2%' }; 'calls          : ' + ($c -join ', '); 'units run      : ' + $units; 'ADVANCED lines : ' + ($adv -join ', '); 'plan 2.3 met   : ' + $flipped; 'halt line      : ' + $led; if($arm.StartsWith('attempt-')){ 'ATTEMPT lines  : ' + $alines.Count; 'attempt-read   :'; foreach($x in $a1){ '    ' + $x }; if($a2.Count -gt 0){ 'after restart  :'; foreach($x in $a2){ '    ' + $x } } }; switch($arm){ 'attempt-one' { $e=@('CRITERION=2.3', 'ATTEMPTS=1', ('ATTEMPT_1=unit 1 | yes | executed | ' + $ap)); $ok=($units -eq 1) -and (($adv -join ',') -eq 'yes') -and $back -and $flipped -and ($alines.Count -eq 1) -and (($a1 -join $nl) -ceq ($e -join $nl)); $must='one unit, ADVANCED yes, and attempt-read.bat returns exactly one attempt at 2.3 - unit 1, yes, executed, the decision block approach' } 'attempt-three' { $e=@('CRITERION=2.3', 'ATTEMPTS=3', ('ATTEMPT_1=unit 1 | no | executed | ' + $ap), ('REASON_1=' + $cl), ('ATTEMPT_2=unit 2 | yes | executed | ' + $v1), ('ATTEMPT_3=unit 3 | no | executed | ' + $v2), ('REASON_3=' + $cl)); $ok=($units -eq 3) -and (($adv -join ',') -eq 'no,yes,no') -and $back -and $flipped -and ($alines.Count -eq 3) -and (($a1 -join $nl) -ceq ($e -join $nl)); $must='three units, and attempt-read.bat returns three attempts at 2.3 in order - no, yes, no' } 'attempt-restart' { $e1=@('CRITERION=2.3', 'ATTEMPTS=1', ('ATTEMPT_1=unit 1 | no | executed | ' + $ap), ('REASON_1=' + $cl)); $ok=($units -eq 2) -and (($adv -join ',') -eq 'no,no') -and $back -and ($a1.Count -eq 4) -and (($a1 -join $nl) -ceq ($e1 -join $nl)) -and ($a2.Count -eq 6) -and ($a2[1] -ceq 'ATTEMPTS=2') -and ($a2[2] -ceq $a1[2]) -and ($a2[3] -ceq $a1[3]) -and ($a2[4] -ceq ('ATTEMPT_2=unit 1 | no | executed | ' + $v1)) -and ($a2[5] -ceq ('REASON_2=' + $cl)); $must='the first loop records one attempt at 2.3; after a new loop over the same root, that attempt reads back unchanged and the second follows it' } 'attempt-blocker-crit' { $e=@('CRITERION=2.3', 'ATTEMPTS=1', ('ATTEMPT_1=unit 1 | blocker | executed | ' + $bp)); $ok=($units -eq 1) -and (($adv -join ',') -eq 'blocker') -and $back -and ($alines.Count -eq 1) -and (($a1 -join $nl) -ceq ($e -join $nl)); $must='one unit, ADVANCED blocker, and one attempt recorded against 2.3, the criterion the blocker-clear names' } 'attempt-blocker-unit' { $e=@('CRITERION=2.3', 'ATTEMPTS=0'); $ok=($units -eq 1) -and (($adv -join ',') -eq 'blocker') -and $back -and ($alines.Count -eq 0) -and (($a1 -join $nl) -ceq ($e -join $nl)); $must='one unit, ADVANCED blocker, no ATTEMPT line anywhere in the record, and the loop reached its backstop' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  071'S ARMS, JUDGED BY THEIR OWN CALL - the fourth, for the reason the
rem  067, 068 and 070 calls each give: cmd refuses a command line past 8191
rem  characters.
rem
rem  WHAT EVERY ONE OF THEM IS REALLY ASKING. The stand-in DECLARES the
rem  criterion exhausted in all five of the first arms, in exactly the same
rem  words. The only thing that differs is THE RECORD ON DISK. So an arm that
rem  passes proves the loop decided from the record and not from the claim -
rem  which is the owner's ruling of 2026-09-14, and the whole unit.
rem ============================================================
rem  072'S ARMS, JUDGED BY THEIR OWN CALL - the sixth split, same reason.
rem
rem  EVERY ONE READS RUN_LEDGER.md'S COLUMNS, not the console. The console is
rem  gone by breakfast and the ledger is what the owner actually reads, so an
rem  arm that passed on console text would prove the wrong thing. Column 5 is
rem  the exit state - the verdict - and column 7 is the prose.
rem
rem  end-three IS THE ONE TO READ FIRST. The instruction's own arm table asks
rem  for the word `failure` on a run halting at one of the three stops. Ruling
rem  2 says an ending IS `one of the three stops`, so the two cannot both
rem  hold. This arm asserts the RULING - ending, not failure - and the report
rem  names the mismatch rather than quietly picking one.
rem ============================================================
rem  073'S ARMS, JUDGED BY THEIR OWN CALL - the seventh split, same reason.
rem
rem  EVERY ONE READS THE HEADER'S BYTES, not just its text. The two defects
rem  task 2 shipped past a reading - a doubled BOM and a bare LF where the
rem  splice landed inside a line terminator - were invisible in the rendered
rem  file and obvious in the bytes. So these arms count bare LFs and check the
rem  BOM appears once, on every arm, whatever else they are about.
:verdict073
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $o=$r+'\PHASE_OUTCOME.md'; $bytes=[IO.File]::ReadAllBytes($o); $bom=($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191); $raw=[Text.Encoding]::UTF8.GetString($bytes); $bare=0; for($i=0;$i -lt $bytes.Length;$i++){ if($bytes[$i] -eq 10 -and ($i -eq 0 -or $bytes[$i-1] -ne 13)){ $bare++ } }; $steps=@(); foreach($x in [regex]::Matches($raw, '(?m)^STEP: *([0-9]+) *\| *([a-z ]+?) *\| *(.*)$')){ $steps += @{ n=$x.Groups[1].Value; st=$x.Groups[2].Value.Trim(); what=$x.Groups[3].Value.Trim() } }; $ids=@($steps | ForEach-Object { $_.n }) -join ','; $appended=@([regex]::Matches($con, 'appended: STEP:')).Count; $matched=$con.Contains('header already matches the plan'); $left=$con.Contains('LEFT ALONE'); $owait=$con.Contains('STOP 1: THE PHASE IS WAITING ON THE OWNER'); $s1=@($steps | Where-Object { $_.n -eq '1' }); $s1st=''; if($s1.Count -gt 0){ $s1st=$s1[0].st }; 'run-phase exit : %RRC%'; 'header steps   : ' + $ids; 'step 1 state   : ' + $s1st; 'appended lines : ' + $appended; 'already matched: ' + $matched; 'left alone said: ' + $left; 'BOM once       : ' + ($bom -and ($bytes.Length -lt 6 -or -not ($bytes[3] -eq 239 -and $bytes[4] -eq 187 -and $bytes[5] -eq 191))); 'bare LF bytes  : ' + $bare; switch($arm){ 'rec-append' { $ok=($ids -eq '1,2') -and ($appended -eq 1) -and $con.Contains('appended: STEP: 2 | not started | the watchdog is proved') -and ($bare -eq 0); $must='the step the header lacked is APPENDED at not started with the plan-s own title, once, and the file gains no bare LF' } 'rec-twice' { $ok=($appended -eq 1) -and (-not $matched) -and ($bare -eq 0); $must='THREE iterations and the reconciliation ran ONCE - one appended line on the whole console, not one per iteration' } 'rec-match' { $ok=($appended -eq 0) -and $matched -and ($bare -eq 0); $must='a header that already matches the plan gains NOTHING, and the console says so rather than staying silent' } 'rec-extra' { $ok=$left -and ($ids -like '*9*') -and $con.Contains('the header carries step 9 and the plan does not'); $must='a step the header has and the plan does not is REPORTED and LEFT IN PLACE - append-only, and its attempt history goes with it' } 'rec-done' { $ok=($s1st -eq 'done') -and ($appended -eq 1) -and ($bare -eq 0); $must='a step appended beside earlier steps already done leaves THEIR STATES UNTOUCHED - step 1 is still done' } 'rec-owner' { $ok=(-not $owait) -and ($appended -ge 1); $must='the appended step-s criteria are unmet WORK, so the owner-s-verdict halt does NOT fire - the phase is not waiting on him' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  074'S ARMS, JUDGED BY THEIR OWN CALL - the eighth split, and the same
rem  reason every one before it gives: cmd refuses a command line past 8191
rem  characters and a fixture that spends its run and then cannot say what it
rem  saw is worse than no fixture.
rem
rem  EVERY ONE READS THE CARD'S BYTES, and this file's conventions are the
rem  OPPOSITE of the record's - LF-only where that is CRLF, no BOM where that
rem  has one. So the readings are inverted too: CR bytes must be ZERO and the
rem  BOM must be ABSENT, where :verdict073 requires the BOM to appear exactly
rem  once. Unit 073 shipped a doubled BOM and an orphaned LF into the record
rem  and caught neither by reading the file; the same reading here would pass
rem  over the same class of defect arriving from the other direction.
rem
rem  AND EVERY ONE THAT TOUCHES THE CARD PROVES THE SPLICE: the card with its
rem  insertion removed must be byte-identical to card.orig, the copy taken
rem  before anything ran. That is the check that means 'nothing else moved',
rem  and it is stronger than any count of lines.
:verdict074
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $p=$r+'\PHASE_STATUS.md'; $op=$r+'\card.orig'; $there=(Test-Path -LiteralPath $p); $b=@(); if($there){ $b=[IO.File]::ReadAllBytes($p) }; $bom=($b.Length -ge 3 -and $b[0] -eq 239 -and $b[1] -eq 187 -and $b[2] -eq 191); $cr=0; for($i=0;$i -lt $b.Length;$i++){ if($b[$i] -eq 13){ $cr++ } }; $raw=''; if($there){ $raw=[Text.Encoding]::UTF8.GetString($b) }; $ids=(@([regex]::Matches($raw, '(?m)^STEP: *([0-9]+) *\|')) | ForEach-Object { $_.Groups[1].Value }) -join ','; $same='n/a'; $inslen=-1; if($there -and (Test-Path -LiteralPath $op)){ $a=[IO.File]::ReadAllBytes($op); $i=0; while($i -lt $a.Length -and $i -lt $b.Length -and $a[$i] -eq $b[$i]){ $i++ }; $j=0; while(($j -lt ($a.Length-$i)) -and ($j -lt ($b.Length-$i)) -and ($a[$a.Length-1-$j] -eq $b[$b.Length-1-$j])){ $j++ }; $inslen=$b.Length-$i-$j; $rest=New-Object byte[] ($b.Length-$inslen); [Array]::Copy($b,0,$rest,0,$i); [Array]::Copy($b,$i+$inslen,$rest,$i,$b.Length-$i-$inslen); $same=($rest.Length -eq $a.Length); if($same -eq $true){ for($k=0;$k -lt $a.Length;$k++){ if($rest[$k] -ne $a[$k]){ $same=$false; break } } } }; $capp=@([regex]::Matches($con, 'appended to the card: STEP:')).Count; $rapp=@([regex]::Matches($con, 'appended: STEP:')).Count; $cmatch=$con.Contains('the card already carries every step the plan does'); $cleft=$con.Contains('the card carries step 9 and the plan does not'); $cfail=$con.Contains('CARD NOT RECONCILED'); $cnone=$con.Contains('no PHASE_STATUS.md at this root'); $cfind=$con.Contains('FINDING: the two headers do not name the same steps'); $ccaught=$con.Contains('card caught up'); $cpanel=$con.Contains('CARD-PANEL: PASS'); $iters=@([regex]::Matches($con, '(?m)^ iteration [0-9]+')).Count; 'run-phase exit  : %RRC%'; 'card present    : ' + $there; 'card step ids   : ' + $ids; 'appended (card) : ' + $capp; 'appended (rec)  : ' + $rapp; 'already matched : ' + $cmatch; 'left alone said : ' + $cleft; 'not reconciled  : ' + $cfail; 'phasesteps wrote: ' + $ccaught; 'phasesteps FIND : ' + $cfind; 'iterations      : ' + $iters; 'BOM             : ' + $bom; 'CR bytes        : ' + $cr; 'insertion bytes : ' + $inslen; 'insert removed  : ' + $same; switch($arm){ 'card-append' { $ok=($ids -eq '1,2') -and ($capp -eq 1) -and $con.Contains('appended to the card: STEP: 2 | not started | the watchdog is proved') -and (-not $bom) -and ($cr -eq 0) -and ($same -eq $true) -and ($inslen -gt 0); $must='the step the card lacked is APPENDED in the card-s own form - LF, no BOM - and the card with that insertion removed is BYTE-IDENTICAL to what it was' } 'card-match' { $ok=($capp -eq 0) -and $cmatch -and ($same -eq $true) -and ($inslen -eq 0) -and ($ids -eq '1,2'); $must='a card that already names every step the plan does gains NOTHING, says so, and NOT ONE BYTE moves' } 'card-extra' { $ok=$cleft -and ($capp -eq 1) -and ($ids -eq '1,9,2') -and ($same -eq $true) -and (-not $bom) -and ($cr -eq 0); $must='a step the card has and the plan does not is REPORTED AND LEFT WHERE IT IS - and the appended step lands AFTER it, out of numeric order, which is what append-only costs' } 'card-locked' { $ok=$cfail -and ($capp -eq 0) -and ($ids -eq '1') -and ($same -eq $true) -and ($inslen -eq 0) -and ('%RRC%' -eq '0'); $must='a card that cannot be WRITTEN prints what could not be done, changes NOTHING, and the run goes on at exit 0 - ruling 3 rejected a halt here by name' } 'card-absent' { $ok=$cnone -and (-not $there) -and ('%RRC%' -eq '0') -and ($rapp -eq 1); $must='no card at the root is said out loud, the RECORD is still reconciled, and the run goes on at exit 0' } 'card-panel' { $ok=$cpanel -and ($ids -eq '1,2') -and (-not $bom) -and ($cr -eq 0); $must='the PANEL-S OWN parser and painter, loaded out of the HTML, read the reconciled card - readable, both steps, both bar segments, nothing normalized' } 'card-twice' { $ok=($capp -eq 1) -and ($iters -ge 3) -and (-not $bom) -and ($cr -eq 0); $must='THREE iterations and the card was reconciled ONCE - one appended line on the whole console, not one per iteration' } 'card-steps' { $ok=$ccaught -and (-not $cfind) -and ($ids -eq '1,2'); $must=':phasesteps WRITES NORMALLY after a successful reconciliation AND PRINTS NO FINDING - which is exactly what unit 073 left broken' } 'card-frozen' { $ok=$cfail -and $cfind -and ($ids -eq '1') -and ($iters -ge 1); $must='where the card could not be reconciled :phasesteps still REFUSES SAFELY, writing nothing and saying why - and the run carries on regardless' } }; 'must            : ' + $must; ''; 'verdict         : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:verdict072
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $l=$r+'\RUN_LEDGER.md'; $led=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1) }; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $sp=$r+'\review\sheet.md'; $sheet=Test-Path -LiteralPath $sp; $st=''; if($sheet){ $st=[IO.File]::ReadAllText($sp) }; $o=$r+'\PHASE_OUTCOME.md'; $ents=0; if(Test-Path -LiteralPath $o){ $ents=@(Select-String -Path $o -Pattern '^## UNIT ').Count }; $ro=Test-Path -LiteralPath ($r+'\output.md'); $col=@($led -split '\|'); $verdict=''; $answer=''; if($col.Count -ge 7){ $verdict=$col[4].Trim(); $answer=$col[6].Trim() }; 'run-phase exit : %RRC%'; 'ledger verdict : ' + $verdict; 'ledger says    : ' + $(if($answer.Length -gt 96){ $answer.Substring(0,96) } else { $answer }); 'review sheet   : ' + $sheet; 'record entries : ' + $ents; 'root output.md : ' + $ro; switch($arm){ 'end-exhausted' { $ok=($verdict -eq 'ending') -and $answer.StartsWith('ENDED - the criterion is exhausted on the record') -and $sheet -and $st.Contains('3 attempt(s)') -and $st.Contains('read the watchdog log rather than polling any process tree') -and $st.Contains('the unit ran to completion and the criterion did not flip') -and $con.Contains('attempts   : criterion 2.3 has 3 attempt(s) on the record') -and $ro; $must='the ledger VERDICT COLUMN says ending and the prose says which kind; the sheet carries all three attempts with their approaches and reasons; the console counted them BEFORE the arbiter authored; and the root output.md is still there. 5.1, 5.3, 5.4' } 'end-three' { $ok=($verdict -eq 'ending') -and $answer.StartsWith('ENDED - the arbiter raised one of the two for the owner') -and $answer.Contains('stop 4') -and (-not $answer.Contains('FAILURE')); $must='CORRECTED BY 086 - 072 asserted the ledger prose ENDED - the arbiter raised one of the three for the owner, and 085 changed the word to two without finding this StartsWith; it FAILED unit 086-s gate on the word alone, the loop having done the right thing. Now: an owner-s-decision stop raised by the arbiter IS AN ENDING, NOT A FAILURE - ruling 2 says so in terms - the ledger names it stop 4 and its prose reads one of the two' } 'end-owner' { $ok=($verdict -eq 'ending') -and $answer.StartsWith('ENDED - nothing is left but the owner-s verdict') -and $sheet -and $st.Contains('YOURS TO JUDGE'); $must='the owner-s verdict is an ending, the ledger says so, and the sheet is written once and marks his lines' } 'end-twice' { $ok=($verdict -eq 'ending') -and $sheet -and ('%SH1%' -ne 'absent') -and ('%SH1%' -eq '%SH2%') -and ('%MT1%' -eq '%MT2%'); $must='the same phase ended twice and THE SHEET WAS NOT WRITTEN TWICE - same content, same write time, decided from the disk' } 'fail-stop11' { $ok=($verdict -eq 'failure') -and $answer.StartsWith('STOPPED, AND A STOP IS FAILURE') -and $answer.Contains('backstop') -and (-not $answer.Contains('stop 11')) -and ($ents -eq 2); $must='REVERSED BY 087 - 072 asserted: STOP 11 is recorded as FAILURE in that word, the ledger names it, and NOTHING was appended. Now: a unit that wrote no report is recorded unjudged with fate not recorded on both iterations, no stop 11 fires, and the night ends at the backstop - which is still a failure in that word, because the backstop is a brake and not an ending. The old must read: STOP 11 is recorded as FAILURE in that word, the ledger names it, and NOTHING was appended to the record. 5.2' } 'fail-maxiter' { $ok=($verdict -eq 'failure') -and $answer.StartsWith('STOPPED, AND A STOP IS FAILURE') -and $answer.Contains('backstop'); $must='--max-iterations is recorded as FAILURE - a cap is not an ending, which is the owner-s ruling of 2026-09-14' } 'fail-noplan' { $ok=($verdict -eq 'failure') -and $answer.Contains('no PHASE_PLAN.md') -and $answer.Contains('refused before the loop started'); $must='a halt that wrote NO LEDGER LINE AT ALL before this unit now writes one, and it is recorded as a failure' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:verdict071
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $o=$r+'\PHASE_OUTCOME.md'; $ents=0; if(Test-Path -LiteralPath $o){ $ents=@(Select-String -Path $o -Pattern '^## UNIT ').Count }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $sheet=Test-Path -LiteralPath ($r+'\review\sheet.md'); $ended=$con.Contains('THE CRITERION IS EXHAUSTED, AND THE RECORD SHOWS IT'); $thin=$con.Contains('REFUSED: THE RECORD DOES NOT SHOW THIS CRITERION IS EXHAUSTED'); $carried=$con.Contains('this iteration launched no unit. The loop carries on'); 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'record entries : ' + $ents; 'ending seen    : ' + $ended; 'refusal seen   : ' + $thin; 'loop carried on: ' + $carried; 'halt line      : ' + $led; if($arm -like 'exhaust-stop*'){ $wq='not printed'; if($con -match '(?m)^\s*why : (.*)$'){ $wq=$Matches[1].Trim() }; 'why :          : ' + $wq }; switch($arm){ 'exhaust-three' { $ok=('%RRC%' -eq '0') -and ($units -eq 0) -and $ended -and $con.Contains('3 distinct closed route') -and $con.Contains('read the watchdog log rather than polling any process tree') -and $con.Contains('compare each process creation time against the launch stamp on disk') -and $con.Contains('ask the operating system which handles the run still holds open') -and $con.Contains('the unit ran to completion and the criterion did not flip') -and $led.Contains('ending: criterion 2.3 is exhausted'); $must='EXIT 0, no unit run, and the console names the criterion, all THREE closed routes and their THREE reasons - the owner can read an ending without opening a file. 4.1 and 4.6' } 'exhaust-empty' { $ok=$thin -and $carried -and ($units -eq 0) -and $con.Contains('THE RECORD FOR THIS CRITERION IS EMPTY') -and $led.Contains('backstop') -and (-not $ended); $must='an ending against an EMPTY record is REFUSED and THE LOOP CARRIES ON - it reaches its backstop rather than halting on the claim. 4.2' } 'exhaust-two' { $ok=$thin -and $carried -and ($units -eq 0) -and $con.Contains('2 distinct closed route') -and $led.Contains('backstop') -and (-not $ended); $must='two closed routes where three are needed is REFUSED and the loop carries on - ruling 3-s bar, and it is not met' } 'exhaust-same' { $ok=$thin -and $carried -and ($units -eq 0) -and $con.Contains('1 distinct closed route') -and $con.Contains('repeated a route already counted') -and $led.Contains('backstop') -and (-not $ended); $must='THREE attempts that 068-s matcher calls the same fold to ONE route, so the ending is refused and the loop carries on - and the console says two were folded. 4.3, on 068-s matcher and not a second one' } 'exhaust-noreason' { $ok=('%RRC%' -eq '0') -and $ended -and $con.Contains('3 distinct closed route') -and $con.Contains('carry no recorded reason') -and $con.Contains('COUNTED rather than discarded'); $must='three distinct routes with NO reasons - every line in the shape written before unit 071 - are COUNTED, the ending is permitted, and the console SAYS they carry no recorded reason rather than hiding it' } 'exhaust-stop' { $ok=$con.Contains('STOP 4: THE ARBITER DECLARED A DECISION THE OWNER') -and ($units -eq 0) -and (-not $thin) -and (-not $ended) -and $led.Contains('stop 4'); $must='one of the three stops, raised at a criterion whose record is EMPTY, HALTS ON SIGHT at stop 4 - it is never put to the exhaustion test, which would have refused it and carried on. 4.4' } 'exhaust-stop-money' { $ok=$con.Contains('STOP 4: THE ARBITER DECLARED A DECISION THE OWNER') -and ($units -eq 0) -and (-not $thin) -and (-not $ended) -and $led.Contains('stop 4') -and $con.Contains('spending past the budget the owner set'); $must='the SECOND of the three - money past the budget - raised at a criterion whose record is EMPTY, HALTS ON SIGHT at stop 4, and the console carries ITS OWN why line rather than the keying one. 4.4' } 'exhaust-stop-promise' { $ok=$con.Contains('STOP 4: THE ARBITER DECLARED A DECISION THE OWNER') -and ($units -eq 0) -and (-not $thin) -and (-not $ended) -and $led.Contains('stop 4') -and $con.Contains('what the product states to the operator about what was logged'); $must='the THIRD of the three - what the product promises the operator, a fact about what was LOGGED and what WENT OUT rather than a label or a hint - HALTS ON SIGHT at stop 4 with ITS OWN why line. 4.4' } 'exhaust-owner' { $ok=($units -eq 0) -and ($c.Count -eq 0) -and $con.Contains('STOP 1: THE PHASE IS WAITING ON THE OWNER') -and $sheet -and (-not $thin) -and (-not $ended) -and $led.Contains('stop 1'); $must='the owner-s verdict, at a criterion whose record is EMPTY, HALTS ON SIGHT with its sheet written once and NO ARBITER CALLED AT ALL - it is decided at the top of the iteration, so the test cannot reach it. 4.5' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:verdict070
rem  070-s arms, judged by their own call - the fifth. The 064 line is 7408
rem  characters and cmd refuses one past 8191.
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $o=$r+'\PHASE_OUTCOME.md'; $ents=0; if(Test-Path -LiteralPath $o){ $ents=@(Select-String -Path $o -Pattern '^## UNIT ').Count }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $pf=$r+'\.run-unit\arbiter-prompt.txt'; $pr=''; if(Test-Path -LiteralPath $pf){ $pr=[IO.File]::ReadAllText($pf) }; $nred=@([regex]::Matches($all, 'redirected - ')).Count; $back=$led.Contains('backstop'); $sheet=Test-Path -LiteralPath ($r+'\review\sheet.md'); $wl=''; $wf=$r+'\.run-unit\watched.log'; if(Test-Path -LiteralPath $wf){ $wl=[IO.File]::ReadAllText($wf) }; $ceil=($wl.Contains('killed at the owner''s --minutes ceiling') -or $con.Contains('killed at the owner''s --minutes ceiling')); $ledceil=$all.Contains('killed at the owner''s --minutes ceiling'); $pstop='none named'; if($led -match 'stop [0-9]+'){ $pstop=$Matches[0] }; 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'record entries : ' + $ents; 'redirect notes : ' + $nred; 'prompt says so : ' + $pr.Contains('YOU HAVE BEEN REDIRECTED'); 'halt line      : ' + $led; if($arm -eq 'redirect-minutes'){ $lk=@([regex]::Matches($con, '(?m)^\s*\[look [0-9]+\].*$') | ForEach-Object { $_.Value.Trim() }); 'watchdog looks : ' + $(if($lk.Count){ $lk -join '  |  ' } else { 'NONE - the tree was gone before the first look, so AGEMIN was never computed and the ceiling was never tested' }); 'ceiling fired  : ' + $ceil; 'ledger ceiling : ' + $ledceil; 'phase stop     : ' + $pstop + '  <- NOT stop 5: see the note at this arm in the header' }; switch($arm){ 'redirect-same' { $ok=($units -eq 1) -and $back -and ($nred -ge 1) -and $all.Contains('no-advance at one criterion') -and $pr.Contains('YOU HAVE BEEN REDIRECTED') -and $pr.Contains('2.3'); $must='no halt: the loop redirected after two no-advances at 2.3, told the arbiter so in its prompt, and ran a third unit' } 'redirect-fresh' { $ok=($units -eq 1) -and $back -and (-not $con.Contains('ALREADY RECORDED AS FAILED')); $must='the redirected instruction named an approach the record does not show failing, so it RAN' } 'redirect-wander' { $ok=($units -eq 1) -and $back -and ($nred -ge 1) -and $all.Contains('no-advance at two different criteria'); $must='two no-advances at DIFFERENT criteria redirect too, pinned to one of the two, and the ledger names which rule fired' } 'redirect-repeat' { $ok=($units -eq 0) -and $back -and ($nred -ge 2) -and $con.Contains('NOTHING WAS SPENT, WHICH IS WHY THIS CARRIES ON'); $must='an instruction naming a recorded failed approach is refused AND REDIRECTED AGAIN rather than halting, launching nothing' } 'redirect-refused' { $ok=($units -eq 1) -and $back -and ($nred -ge 1) -and $all.Contains('approach already recorded as failed') -and $con.Contains('spent on a redirect'); $must='a refused approach on a first attempt redirects, the loop carries on, and the console says which iterations were spent on a redirect' } 'redirect-budget' { $ledcost=($led -match '\|\s*[0-9]+\.[0-9]{4}\s*\|'); $ok=($nred -ge 1) -and ($units -ge 1) -and $back -and (-not $all.Contains('stop 2')) -and $con.Contains('PAST THE --budget FIGURE OF 0') -and $con.Contains('PRINTED AND NOT A STOP') -and $ledcost; $must='REVERSED BY 085 - 070 asserted: --budget still halts during a redirect, exactly as before, on the ledger line stop 2. Now: --budget 0 is passed, the run goes PAST the figure and does not halt - the console says PAST THE --budget FIGURE, PRINTED AND NOT A STOP, the ledger carries the spend in its cost column, a unit runs and the loop reaches its backstop' } 'redirect-maxiter' { $ok=($units -eq 0) -and $back -and ($nred -ge 2); $must='--max-iterations still halts a loop that is doing nothing but redirect' } 'redirect-owner' { $ok=('%RRC%' -eq '0') -and ($nred -eq 0) -and $led.Contains('stop 1: the phase is waiting on the owner') -and $sheet; $must='the owner-s-verdict halt is checked BEFORE the redirect, so it halts, writes its sheet once, and nothing is redirected' } 'redirect-blocker' { $ok=($units -eq 1) -and $back -and ($nred -eq 1) -and (-not $con.Contains('NAMED A CRITERION IT WAS NOT SENT TO')); $must='a blocker-clear SATISFIES a redirect - it is the layer-s own way through, and it is already bounded by the two-in-a-row halt' } 'redirect-stop11' { $ok=($units -eq 2) -and ($ents -eq 4) -and ($nred -ge 1) -and $all.Contains('no report written by unit') -and (-not $all.Contains('stop 11')) -and $back; $must='REVERSED BY 087 - 070 asserted: STOP 11 still halts on a redirected iteration and nothing is appended, the record keeping the two seeded entries. Now: a unit that ran and wrote no report is NOT retried and NOT judged, its entry lands with fate not recorded, the redirected iteration authors again, a second such unit runs and lands the same way, the ledger carries the no-report notes, no stop 11, and the loop reaches its backstop' } 'redirect-minutes' { $ok=$ceil -and $ledceil -and ($nred -ge 1) -and ('%RRC%' -ne '0'); $must='--minutes still halts A REDIRECTED LOOP - the watchdog kills the tree at the owner-s wall-clock ceiling, the ledger carries that kill in those words, a redirect was in force, and the run did not reach a clean ending. IT DOES NOT ASSERT stop 5: see phase stop above and the note at this arm in the header' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  088'S ARMS, JUDGED BY A CALL OF THEIR OWN. They read the console, the
rem  ledger whole, calls.txt and PARKED.md.
:verdict088
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $pkl=@(); if(Test-Path -LiteralPath ($r+'\PARKED.md')){ $pkl=@(Get-Content -LiteralPath ($r+'\PARKED.md') | Where-Object { $_ -like 'PARKED:*' }) }; $nred=@([regex]::Matches($all, 'redirected - ')).Count; $answered=@([regex]::Matches($con, 'ANSWERED: THE OWNER RESOLVED THIS STOP')).Count; $handed=@([regex]::Matches($con, 'HANDED BACK: THE STOP STANDS ON A PREMISE')).Count; $holds=$con.Contains('its premise was tested against this pass and HOLDS'); $untested=$con.Contains('halts untested, as it must'); $stop4=$led.Contains('stop 4'); $back=$led.Contains('backstop'); 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units + '   redirect notes: ' + $nred; 'answered       : ' + $answered + ' time(s)   handed back: ' + $handed + ' time(s)   premise holds: ' + $holds + '   untested: ' + $untested; 'PARKED.md      : ' + $(if($pkl.Count){ $pkl -join '  //  ' } else { 'none' }); 'halt line      : ' + $led; switch($arm){ 'stop-answered' { $ok=($units -eq 0) -and ($answered -ge 1) -and $all.Contains('stop answered - the owner') -and (-not $stop4) -and $back; $must='the keying stop with a resolution row whose phrase is copied from its why line is NOT re-raised: ANSWERED on the console, a stop-answered ledger note, the arbiter redirected, the loop reaches its backstop - no stop 4' } 'stop-other-answer' { $ok=($units -eq 0) -and ($answered -eq 0) -and $stop4 -and $con.Contains('No resolution row in PARKED.md answers it'); $must='a resolution row for some other question does not clear this stop: stop 4 halts, untouched' } 'stale-unit' { $ok=($units -eq 0) -and ($handed -ge 2) -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.3 |') -and $pkl[0].Contains('| premise |') -and $con.Contains('nothing is executing here') -and (-not $stop4) -and $back; $must='a stop claiming unit 439 is executing while the lock is free is HANDED BACK; twice, criterion 2.3 parks with which=premise; no stop 4; the loop reaches its backstop' } 'stale-plan-current' { $ok=($units -eq 0) -and $holds -and $stop4 -and ($handed -eq 0); $must='a stop naming a phase the record carries has a premise that HOLDS: stop 4 halts' } 'stale-plan-gone' { $ok=($units -eq 0) -and ($handed -ge 1) -and $con.Contains('names no such phase') -and (-not $stop4) -and $back; $must='a stop naming a phase the plan does not name is HANDED BACK, the arbiter authors again, the loop reaches its backstop - no stop 4' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  087'S ARMS, JUDGED BY A CALL OF THEIR OWN. They read the console, the
rem  ledger whole, calls.txt, PARKED.md and the record's FATE lines.
:verdict087
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $arbs=@($c | Where-Object { $_ -eq 'ARBITER' }).Count; $j4=@($c | Where-Object { $_ -eq 'JUDGE4' }).Count; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $o=$r+'\PHASE_OUTCOME.md'; $ents=0; $fates=@(); if(Test-Path -LiteralPath $o){ $ents=@(Select-String -Path $o -Pattern '^## UNIT ').Count; $fates=@(Select-String -Path $o -Pattern '^FATE: ' | ForEach-Object { $_.Line.Substring(6).Trim() }) }; $pkl=@(); if(Test-Path -LiteralPath ($r+'\PARKED.md')){ $pkl=@(Get-Content -LiteralPath ($r+'\PARKED.md') | Where-Object { $_ -like 'PARKED:*' }) }; $back=$led.Contains('backstop'); $allowed=Test-Path -LiteralPath ($r+'\.run-unit\allowed.txt'); 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units / arbiter calls / section-4 calls : ' + $units + ' / ' + $arbs + ' / ' + $j4; 'record entries : ' + $ents + '   FATE lines: ' + ($fates -join ', '); 'PARKED.md      : ' + $(if($pkl.Count){ $pkl -join '  //  ' } else { 'none' }); 'allowed.txt    : ' + $allowed; 'halt line      : ' + $led; switch($arm){ 's4-retry' { $ok=($units -eq 1) -and ($j4 -eq 2) -and $con.Contains('RETRYING ONCE, same prompt, same section') -and $con.Contains('the second was used') -and $all.Contains('section-4 judge retried') -and ($pkl.Count -eq 0) -and $back -and (-not $all.Contains('stop 3')); $must='the section-4 judge is unreadable on its first call and read on the retry: both attempts on the console, a ledger note, nothing parked, no stop 3, the loop reaches its backstop' } 'arb-retry' { $ok=($arbs -eq 2) -and ($units -eq 1) -and $con.Contains('RETRYING ONCE, same prompt, same inputs') -and $all.Contains('arbiter retried') -and ($pkl.Count -eq 0) -and $back -and (-not $all.Contains('arbiter session failed')); $must='the arbiter call fails once and the retry is read: a unit runs, the ledger says the arbiter was retried, nothing parked, the loop reaches its backstop' } 'arb-twice' { $ok=($arbs -eq 3) -and ($units -eq 1) -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.2 |') -and $pkl[0].Contains('| arbiter |') -and $con.Contains('FAILED TWICE RUNNING') -and $back -and (-not $all.Contains('arbiter session failed - exit')); $must='the arbiter call fails twice: the first authorable criterion 2.2 is parked with which=arbiter, the next iteration authors and a unit runs, the loop reaches its backstop - no halt' } 'arb-stop-retry' { $ok=($arbs -eq 2) -and ($units -eq 0) -and $con.Contains('STOP 4: THE ARBITER DECLARED A DECISION THE OWNER') -and $led.Contains('stop 4') -and ($pkl.Count -eq 0); $must='the retried arbiter returns a keying MOVE: stop: THE STOP WINS and halts at stop 4, nothing parked' } 'launch-twice' { $ok=($units -eq 0) -and ($ents -eq 0) -and $con.Contains('RETRYING THE LAUNCH ONCE') -and $con.Contains('NOTHING WAS LAUNCHED ON TWO ATTEMPTS') -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.3 |') -and $pkl[0].Contains('| launch |') -and (-not $all.Contains('stop 11')) -and $back; $must='every launch exits 7: retried once, then criterion 2.3 is parked with which=launch, nothing is appended, no stop 11, the loop reaches its backstop' } 'deny-park' { $ok=($units -eq 1) -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.3 |') -and $pkl[0].Contains('| denial |') -and $pkl[0].Contains('the scope was not widened') -and (-not $con.Contains('STOP 6: PERMISSION DENIALS')) -and (-not $all.Contains('stop 6:')) -and $back; $must='the unit is denied a call and does not complete: criterion 2.3 is parked with which=denial and the park line says the scope was not widened, no stop 6 anywhere, the loop reaches its backstop - allowed.txt is run-unit.bat-s own copy of the tools list and is written on every run, so its presence is not the test' } 'fail-noseed' { $ok=('%RRC%' -eq '2') -and ($c.Count -eq 0) -and $led.Contains('refused before the loop started: --seed was given and there is no WORK_INSTRUCTIONS.md') -and (-not $con.Contains('iteration 1')); $must='--seed with no instruction file is refused AT THE DOOR at exit 2 with a ledger line, before any iteration and before any call' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  086'S ARMS, JUDGED BY A CALL OF THEIR OWN. They read the console, the
rem  ledger whole, PARKED.md and the record's FATE and ADVANCED lines.
:verdict086
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $judges=@($c | Where-Object { $_ -eq 'JUDGE' }).Count; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $o=$r+'\PHASE_OUTCOME.md'; $fates=@(); $advs=@(); if(Test-Path -LiteralPath $o){ $fates=@(Select-String -Path $o -Pattern '^FATE: ' | ForEach-Object { $_.Line.Substring(6).Trim() }); $advs=@(Select-String -Path $o -Pattern '^ADVANCED: ' | ForEach-Object { $_.Line.Substring(10).Trim() }) }; $pkl=@(); if(Test-Path -LiteralPath ($r+'\PARKED.md')){ $pkl=@(Get-Content -LiteralPath ($r+'\PARKED.md') | Where-Object { $_ -like 'PARKED:*' }) }; $nred=@([regex]::Matches($all, 'redirected - ')).Count; $hb=@([regex]::Matches($con, 'THE LOOP HANDS THIS BACK RATHER THAN HALTING')).Count; $back=$led.Contains('backstop'); 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units + '   judges: ' + $judges; 'handed back    : ' + $hb + ' time(s)   redirect notes: ' + $nred; 'FATE lines     : ' + ($fates -join ', '); 'ADVANCED lines : ' + ($advs -join ', '); 'PARKED.md      : ' + $(if($pkl.Count){ $pkl -join '  //  ' } else { 'none' }); 'halt line      : ' + $led; switch($arm){ 'hb-noadv' { $ok=($units -eq 0) -and ($hb -ge 2) -and ($nred -ge 2) -and $all.Contains('redirected - decision block names no step and no criterion') -and $back -and (-not $led.Contains('refused')); $must='a decision block with no ADVANCES line is handed back, the arbiter authors again and is handed back again, nothing is launched, and the loop reaches its backstop - never a halt' } 'hb-twice' { $ok=($units -eq 1) -and ($hb -ge 1) -and $con.Contains('handed back 2 time(s) this pass on 2.3') -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.3 |') -and $pkl[0].Contains('| refusal |') -and $con.Contains('REFUSED TWICE THIS PASS ON CRITERION 2.3') -and $back; $must='the owner-s 2.3 named twice this pass is PARKED with which=refusal on the second hand-back, the arbiter names 2.2 next, one unit runs, and the loop reaches its backstop' } 'hb-blocker-park' { $ok=($units -eq 3) -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.3 |') -and $pkl[0].Contains('| blocker |') -and $all.Contains('redirected - two consecutive blocker-clears') -and (-not $all.Contains('halted: two consecutive')) -and $back; $must='two blocker-clears naming criterion 2.3 park it with which=blocker, the arbiter is redirected, a third unit runs, and the loop reaches its backstop - no halt' } 'hb-unshaped' { $ok=($units -eq 1) -and ($judges -eq 0) -and (($fates -join ',') -eq 'not recorded') -and (($advs -join ',') -eq 'not recorded') -and $all.Contains('report refused - validate-output.bat refused') -and (-not $all.Contains('stop 7')) -and $con.Contains('IT IS NOT JUDGED') -and $back; $must='a report with no section 4 is refused by the validator: the entry lands with FATE not recorded and ADVANCED not recorded, no judge is called, the ledger carries the report-refused note, no stop 7, and the loop reaches its backstop' } 'stop-over-hb' { $ok=($units -eq 0) -and ($hb -eq 0) -and ($nred -eq 0) -and $con.Contains('STOP 4: THE ARBITER DECLARED A DECISION THE OWNER') -and $led.Contains('stop 4'); $must='a keying MOVE: stop with no ADVANCES field: THE STOP WINS and halts at stop 4 - nothing is handed back, nothing is redirected' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  085'S ARMS, JUDGED BY A CALL OF THEIR OWN. budget-none reads the banner,
rem  the spend line and the ledger; drift-all reads the console's ending, the
rem  ledger verdict and prose, and PARKED.md.
:verdict085
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $col=@($led -split '\|'); $verdict=''; if($col.Count -ge 7){ $verdict=$col[4].Trim() }; $ledcost=($led -match '\|\s*[0-9]+\.[0-9]{4}\s*\|'); $pkl=@(); if(Test-Path -LiteralPath ($r+'\PARKED.md')){ $pkl=@(Get-Content -LiteralPath ($r+'\PARKED.md') | Where-Object { $_ -like 'PARKED:*' }) }; $drift=@([regex]::Matches($con, 'DRIFTED TO THE OUTPUT: THE CRITERION WAS CHOSEN FROM THE LAST REPORT')).Count; $ended=$con.Contains('ENDED: THE ARBITER COULD NOT RESOLVE THE WORK BACK TO THE PHASE GOAL'); $oldend=$con.Contains('ENDED: EVERY REMAINING CRITERION IS THE OWNER'); $back=$led.Contains('backstop'); $sheet=Test-Path -LiteralPath ($r+'\review\sheet.md'); 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'banner         : ' + $(if($con -match '(?m)^\s*budget\s*:.*$'){ $Matches[0].Trim() } else { 'no budget line' }); 'spend lines    : ' + @([regex]::Matches($con, '(?m)^\s*spent so far:.*$') | ForEach-Object { $_.Value.Trim() }).Count + ' printed'; 'stop 2 anywhere: ' + ($all.Contains('stop 2') -or $con.Contains('STOP 2')); 'drift caught   : ' + $drift + ' time(s)'; 'drift ending   : ' + $ended + '   old parked ending: ' + $oldend; 'PARKED.md      : ' + $(if($pkl.Count){ $pkl -join '  //  ' } else { 'none' }); 'review sheet   : ' + $sheet; 'ledger verdict : ' + $verdict; 'ledger cost col: ' + $ledcost; 'halt line      : ' + $led; switch($arm){ 'budget-none' { $ok=($units -eq 1) -and $back -and $con.Contains('budget    : no ceiling') -and $con.Contains('no --budget ceiling, and none would halt') -and (-not $all.Contains('stop 2')) -and (-not $con.Contains('STOP 2')) -and $ledcost; $must='--budget omitted entirely: the banner says no ceiling, the spend is still printed after the unit, the ledger carries the figure, and nothing anywhere says stop 2 - the loop reaches its backstop' } 'drift-all' { $ok=('%RRC%' -eq '0') -and ($units -eq 2) -and ($drift -eq 2) -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.2 |') -and $pkl[0].Contains('| drift |') -and $ended -and (-not $oldend) -and ($verdict -eq 'ending') -and $led.Contains('drift') -and $led.Contains('2.2') -and (-not $back); $must='the only authorable criterion is drifted on twice and parked for drift, and the next iteration ENDS at exit 0 naming what could not be resolved - the drift ending, not the parked-or-owner ending, the ledger verdict ending and its prose naming drift and 2.2, no backstop reached' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  084'S ARMS, JUDGED BY A CALL OF THEIR OWN - the same reason every split
rem  before it gives. The prompt arms read the composed prompt and the
rem  console's share lines; the WHY and drift arms read the console, the
rem  ledger whole, redirect.txt and PARKED.md.
:verdict084
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $judges=@($c | Where-Object { $_ -eq 'JUDGE' }).Count; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $col=@($led -split '\|'); $verdict=''; if($col.Count -ge 7){ $verdict=$col[4].Trim() }; $pf=$r+'\.run-unit\arbiter-prompt.txt'; $pr=''; if(Test-Path -LiteralPath $pf){ $pr=[IO.File]::ReadAllText($pf) }; $first=''; foreach($ln in ($pr -split '\r?\n')){ if($ln.Trim() -ne ''){ $first=$ln; break } }; $id=$pr.IndexOf('THE PHASE GOAL IS THE PRIME DIRECTIVE'); $ig=$pr.IndexOf('PHASE GOAL:'); $ic=$pr.IndexOf('ITS UNMET CRITERIA'); $ia=$pr.IndexOf('WHAT HAS ALREADY BEEN TRIED'); $ir=$pr.IndexOf('THE PREVIOUS UNIT-S REPORT - AN INDICATOR'); $share=$null; if($con -match 'of which the previous report is ([0-9]+) - ([0-9.]+) pct'){ $share=[double]$Matches[2] }; $shares=$con.Contains('prompt shares:'); $rd=''; if(Test-Path -LiteralPath ($r+'\.run-unit\redirect.txt')){ $rd=[IO.File]::ReadAllText($r+'\.run-unit\redirect.txt') }; $pkl=@(); if(Test-Path -LiteralPath ($r+'\PARKED.md')){ $pkl=@(Get-Content -LiteralPath ($r+'\PARKED.md') | Where-Object { $_ -like 'PARKED:*' }) }; $o=$r+'\PHASE_OUTCOME.md'; $at=@(); if(Test-Path -LiteralPath $o){ $at=@(Select-String -Path $o -Pattern '^ATTEMPT: *([0-9]+\.[0-9]+)' | ForEach-Object { $_.Matches[0].Groups[1].Value }) }; $drv=$con.Contains('REFUSED: THIS INSTRUCTION-S WHY RESTS ON THE LAST REPORT'); $drift=@([regex]::Matches($con, 'DRIFTED TO THE OUTPUT: THE CRITERION WAS CHOSEN FROM THE LAST REPORT')).Count; $let=$con.Contains('so it is let through; the judge decides'); $halted=$con.Contains('HALTED: THE CRITERION WAS CHOSEN FROM THE LAST REPORT'); $back=$led.Contains('backstop'); 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'prompt first   : ' + $first; 'directive/goal/criteria/record/report at : ' + $id + ' / ' + $ig + ' / ' + $ic + ' / ' + $ia + ' / ' + $ir; 'report share   : ' + $(if($null -ne $share){ [string]$share + ' pct' } else { 'not printed' }) + ', block shares printed: ' + $shares; 'WHY refused    : ' + $drv + '   let through, judge decides: ' + $let; 'drift caught   : ' + $drift + ' time(s)   old halt seen: ' + $halted; 'redirect.txt   : ' + $(if($rd){ ($rd -split '\r?\n')[0] } else { 'absent' }); 'PARKED.md      : ' + $(if($pkl.Count){ $pkl -join '  //  ' } else { 'none' }); 'attempts at    : ' + ($at -join ', '); 'ledger verdict : ' + $verdict; 'halt line      : ' + $led; switch($arm){ 'prime-first' { $ok=$first.StartsWith('THE PHASE GOAL IS THE PRIME DIRECTIVE') -and ($id -eq 0) -and ($ig -gt $id) -and $shares; $must='the directive sentence is the FIRST content of the prompt, before the phase goal, and every block share is printed on the console' } 'prime-label' { $ok=($ir -gt 0) -and $pr.Contains('NEVER A SOURCE OF TARGETS') -and $pr.Contains('IT NEVER CHOOSES THE CRITERION') -and ($ir -gt $ic) -and (($ia -lt 0) -or ($ir -gt $ia)) -and ($null -ne $share) -and $con.Contains('the report, an indicator'); $must='the report block carries the indicator label, sits after the criteria and the attempt record - last - and its share is printed, on its own line and in the block shares' } 'prime-big' { $ok=$first.StartsWith('THE PHASE GOAL IS THE PRIME DIRECTIVE') -and ($ig -gt $id) -and ($ic -gt $ig) -and ($ir -gt $ic) -and ($null -ne $share) -and ($share -le 34.0) -and $con.Contains('OVER the'); $must='a report five times the prompt still leaves the directive first, the goal and criteria before it, is cut to its share, and the console says it was over budget' } 'why-mixed' { $ok=($units -eq 0) -and $drv -and $all.Contains('redirected - WHY rests on the last report') -and $back -and (-not $led.Contains('refused')); $must='a WHY that quotes 2.3 and gives the last report as its reason is REFUSED by the detector, REDIRECTED, and the loop continues to its backstop - no halt' } 'why-split' { $ok=($units -eq 1) -and (-not $drv) -and $let -and ($drift -eq 1) -and $all.Contains('redirected - drifted to the output') -and $back; $must='a WHY with a plan sentence of its own is LET THROUGH by the detector, which says so, the unit runs, and the judge-s report answer REDIRECTS - caught by task 5, not task 4, and the night continues' } 'drift-report' { $ok=($units -eq 1) -and ($judges -ge 1) -and ($drift -eq 1) -and (-not $halted) -and $all.Contains('redirected - drifted to the output') -and $rd.Contains('RULE: drifted to the output') -and $back; $must='the REAL judge answers FOLLOWS: report on a WHY that admits the report chose the target, and the loop REDIRECTS with redirect.txt written - it does not halt' } 'drift-plan' { $ok=('%RRC%' -eq '0') -and ($units -eq 1) -and ($judges -ge 1) -and ($drift -eq 0) -and ($verdict -eq 'ending'); $must='the REAL judge answers FOLLOWS: plan on a plan-driven WHY, the unit flips the one work criterion, and the night ends at EXIT 0 as an ending' } 'drift-twice' { $ok=($units -eq 3) -and ($drift -eq 2) -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.3 |') -and $pkl[0].Contains('| drift |') -and (($at -join ',') -eq '2.3,2.3,2.2') -and $back; $must='report twice running on 2.3 PARKS it with drift named, 2.2 is authored and run, and the night continues to its backstop' } 'stop-over-why' { $ok=($units -eq 0) -and $con.Contains('STOP 4: THE ARBITER DECLARED A DECISION THE OWNER') -and $led.Contains('stop 4') -and (-not $all.Contains('redirected')) -and (-not $drv); $must='one of the three stops raised beside a WHY that cites only the report: THE STOP WINS and halts at stop 4, nothing is refused or redirected' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:verdict069
rem  069-s arms, judged by their own call - the fourth. The 064 line is 7267
rem  characters and cmd refuses one past 8191.
rem  084 task 6: THREE OF THESE ARMS ARE REVERSED, and the old expectations are
rem  quoted here, word for word, where a reader will find them:
rem    whyreport     $ok=($units -eq 0) -and $ref -and $led.Contains('cites no line of the plan')
rem                  must: a WHY citing only the previous report is refused before
rem                  the unit runs, and the message names what it cited instead
rem    whyboth       $ok=($units -eq 1) -and (-not $ref) -and $back
rem                  must: a WHY that names a criterion and then reasons from the
rem                  report RUNS - this check is not the guard for that, 6.5 is
rem    followsreport $ok=($units -eq 1) -and $halt -and $led.Contains('chosen from the last report')
rem                  must: the real judge answers FOLLOWS: report, the loop halts,
rem                  and the judge-s own sentence is quoted
rem  What changed: the cites-no-plan refusal REDIRECTS rather than halting, so
rem  whyreport reads the whole ledger for the redirect note and expects the
rem  backstop; task 4's detector refuses whyboth's and followsreport's WHYs
rem  before the unit runs, so neither runs a unit and followsreport never
rem  reaches the judge - the real judge's report answer is proved by the new
rem  drift-report arm instead.
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $l=$r+'\RUN_LEDGER.md'; $led=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1) }; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $pf=$r+'\.run-unit\arbiter-prompt.txt'; $pr=''; if(Test-Path -LiteralPath $pf){ $pr=[IO.File]::ReadAllText($pf) }; $ig=$pr.IndexOf('PHASE GOAL:'); $ic=$pr.IndexOf('ITS UNMET CRITERIA'); $ia=$pr.IndexOf('WHAT HAS ALREADY BEEN TRIED'); $ir=$pr.IndexOf('THE PREVIOUS UNIT-S REPORT'); $order=($ig -ge 0) -and ($ic -gt $ig) -and ($ir -gt $ic) -and (($ia -lt 0) -or ($ir -gt $ia)); $share=$null; $kept=$null; if($con -match 'of which the previous report is ([0-9]+) - ([0-9.]+) pct'){ $kept=[int]$Matches[1]; $share=[double]$Matches[2] }; $tot=0; if($pr -ne ''){ $tot=(Get-Item -LiteralPath $pf).Length }; $real=$null; if(($tot -gt 0) -and ($null -ne $kept)){ $real=[Math]::Round(100.0 * $kept / $tot, 1) }; $back=$led.Contains('backstop'); $ref=$con.Contains('REFUSED: THIS INSTRUCTION-S WHY CITES NO LINE OF THE PLAN'); $halt=$con.Contains('HALTED: THE CRITERION WAS CHOSEN FROM THE LAST REPORT, NOT FROM THE PLAN'); $all=''; if(Test-Path -LiteralPath $l){ $all=[IO.File]::ReadAllText($l) }; $drv=$con.Contains('REFUSED: THIS INSTRUCTION-S WHY RESTS ON THE LAST REPORT'); 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'prompt bytes   : ' + $tot; 'goal/crit/rec/report at : ' + $ig + ' / ' + $ic + ' / ' + $ia + ' / ' + $ir; 'order plan-first: ' + $order; 'printed share  : ' + $(if($null -ne $share){ [string]$share + ' pct of ' + $tot + ' bytes, kept ' + $kept } else { 'not printed' }); 'measured share : ' + $(if($null -ne $real){ [string]$real + ' pct' } else { 'not measurable' }); 'why refused    : ' + $ref; 'follows halt   : ' + $halt; 'halt line      : ' + $led; switch($arm){ 'promptorder' { $ok=$order -and ($null -ne $share) -and $pr.Contains('EVIDENCE, NOT A LIST OF WORK'); $must='the phase goal and the step criteria come before the attempt record and the report, and the report is last under a heading calling it evidence' } 'promptbig' { $ok=$order -and ($null -ne $share) -and ($share -le 34.0) -and $con.Contains('OVER the'); $must='a report five times the size still leaves the goal and criteria leading, is cut to its share, and the console says it was over budget' } 'sharecheck' { $ok=($null -ne $share) -and ($null -ne $real) -and ([Math]::Abs($share - $real) -le 0.2); $must='the share the console printed agrees with the share measured off the prompt file, within 0.2 of a percentage point' } 'whyplan' { $ok=($units -eq 1) -and (-not $ref) -and $back; $must='a WHY citing criterion 2.3 and the plan-s wording runs normally' } 'whyreport' { $ok=($units -eq 0) -and $ref -and $all.Contains('redirected - WHY cites no line of the plan') -and $back; $must='a WHY citing only the previous report is refused before the unit runs, the message names what it cited instead, and THE LOOP REDIRECTS AND CONTINUES to its backstop - REVERSED BY 084, it halted' } 'whyboth' { $ok=($units -eq 0) -and $drv -and $all.Contains('redirected - WHY rests on the last report') -and $back; $must='a WHY that names a criterion and then gives the report as its reason is REFUSED by 084-s detector and redirected - REVERSED BY 084, it ran' } 'followsreport' { $ok=($units -eq 0) -and $drv -and (-not $halt) -and $back; $must='a WHY that takes the nearest criterion to what the last report asked is REFUSED before the unit runs and the judge is never reached - REVERSED BY 084, which proves the real judge-s report answer in drift-report instead' } 'followsplan' { $ok=($units -eq 1) -and (-not $halt) -and $back; $must='the real judge answers FOLLOWS: plan and the loop continues to its backstop' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:verdict068
rem  068-s ARMS, JUDGED BY THEIR OWN CALL. The 064 verdict line is 7267
rem  characters and cmd refuses one past 8191; 067 added a second call for the
rem  same reason and this is the third.
rem
rem  070 REVERSES repeat IN PART, AND THIS SAYS SO OUT LOUD. 068 built the
rem  refusal as a HALT and this arm asserted its ledger line. 070 turns it into
rem  a redirect - nothing was launched and nothing was spent, which is why it
rem  carries on - so that halt no longer exists and the run now ends at its
rem  backstop instead. THE OLD EXPECTATION, WORD FOR WORD:
rem
rem      -and $led.Contains(-refused: the approach is already recorded as
rem                          failed at criterion 2.3-)
rem      must: NO unit ran, the record is untouched at one entry, and the
rem            refusal names criterion 2.3 and the earlier attempt by its
rem            launch identity 2026-09-18T20:00:00.000Z
rem
rem  EVERYTHING 068 PROVED IS STILL ASSERTED - no unit ran, the record is
rem  untouched at one entry, the refusal fires and names the earlier attempt by
rem  its launch identity. What is ADDED is the redirect: the ledger note, the
rem  console counting that iteration as spent on a redirect rather than on a
rem  unit, and the backstop as the ending. THE VERDICT CALL NOW READS THE WHOLE
rem  LEDGER, not only its last line, because the redirect note is the row above
rem  the halt row. The other three 068 refusal arms - noid, restart-id and
rem  othercrit - assert the refusal and the console only, never that halt line,
rem  so they are UNCHANGED and passed unchanged.
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $o=$r+'\PHASE_OUTCOME.md'; $ents=0; if(Test-Path -LiteralPath $o){ $ents=@(Select-String -Path $o -Pattern '^## UNIT ').Count }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $pr=''; $pf=$r+'\.run-unit\arbiter-prompt.txt'; if(Test-Path -LiteralPath $pf){ $pr=[IO.File]::ReadAllText($pf) }; $back=$led.Contains('backstop'); $ref=$con.Contains('REFUSED: THIS APPROACH IS ALREADY RECORDED AS FAILED AT CRITERION 2.3'); 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'record entries : ' + $ents; 'refusal seen   : ' + $ref; 'arbiter prompt : ' + $(if($pr -ne ''){ [string]$pr.Length + ' bytes' } else { 'not written' }); 'halt line      : ' + $led; switch($arm){ 'repeat' { $ok=($units -eq 0) -and ($ents -eq 1) -and $ref -and $con.Contains('2026-09-18T20:00:00.000Z') -and $con.Contains('REDIRECTED, NOT HALTED: approach already recorded as failed') -and $con.Contains('0 launched a unit, 1 spent on a redirect') -and $all.Contains('redirected - approach already recorded as failed') -and $back; $must='NO unit ran, the record is untouched at one entry, and the refusal names criterion 2.3 and the earlier attempt by its launch identity 2026-09-18T20:00:00.000Z - and then the loop REDIRECTS rather than halting: the ledger carries a redirect note, the console counts the iteration as spent on a redirect, and the run ends at its backstop. REVERSED IN PART BY 070' } 'fresh' { $ok=($units -eq 1) -and (-not $ref) -and $back; $must='the approach is not in the record, so the unit ran and the loop reached its backstop' } 'succeeded' { $ok=($units -eq 1) -and (-not $ref) -and $back; $must='the same approach recorded as SUCCEEDING at 2.3 does not block - the unit ran and the loop reached its backstop' } 'othercrit' { $ok=($units -eq 1) -and (-not $ref) -and $back; $must='the same approach recorded as failed against 2.4 does not block an instruction naming 2.3 - the unit ran' } 'noid' { $ok=($units -eq 0) -and $ref -and $con.Contains('THAT ATTEMPT CARRIES NO LAUNCH IDENTITY') -and $con.Contains('It is refused anyway'); $must='an attempt written before 068 still refuses, and the message says so and why - it may be any of the units that have carried that number' } 'restart-id' { $ok=($units -eq 0) -and $ref -and $con.Contains('2026-09-19T09:30:00.000Z') -and (-not $con.Contains('2026-09-18T20:00:00.000Z')); $must='two attempts both called unit 1, and the refusal names 2026-09-19T09:30:00.000Z - the one that failed with this approach - and not 2026-09-18T20:00:00.000Z' } 'neverran' { $ok=($units -eq 1) -and (-not $ref) -and $back; $must='the same approach recorded at 2.3 as failing but with a fate of never ran does NOT block - it was never tried, so the unit ran' } 'arbrecord' { $ok=($units -eq 1) -and (-not $ref) -and $back -and $pr.Contains('WHAT HAS ALREADY BEEN TRIED') -and $pr.Contains('2026-09-18T20:00:00.000Z') -and $pr.Contains('2026-09-19T09:30:00.000Z') -and $pr.Contains('2026-09-19T10:05:00.000Z'); $must='the arbiter prompt carries all three attempts recorded at 2.3, and nothing was refused' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:verdict078
rem  078's ARMS, JUDGED BY THEIR OWN CALL - the reason every call before this
rem  one gives. The 064 verdict line is 7267 characters and cmd refuses one
rem  past 8191. Measured rather than assumed: the longest line in this file
rem  tonight is :verdict068's at 5414, which leaves 2777, and the two cases
rem  below do not fit in it with their evidence intact. :verdict068 is proven
rem  and is not touched.
rem
rem  THIS CALL PRINTS THE TWO LINES THAT CARRY THE ANSWER, not just a verdict.
rem  `it recorded` is what the record held and `shared words` is the set the
rem  matcher actually compared - so the operator can see HOW the decision was
rem  made rather than being told that it was made. They are lifted out of
rem  console.txt, which 067 captures for exactly this reason: a message whose
rem  words are the assertion cannot be asserted from a console nobody kept.
rem
rem  reword ASSERTS THE EXACT SHARED-WORD SET, not merely that some refusal
rem  fired. A refusal that fired on four accidental words would pass a looser
rem  check and would prove nothing about substance.
rem
rem  nearmiss ASSERTS THAT BOTH LINES ARE ABSENT. That is the same fact as
rem  "no refusal" read a second way, and it is what makes the miss MEASURED:
rem  the matcher was reached, it compared, and it let the approach through.
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $o=$r+'\PHASE_OUTCOME.md'; $ents=0; if(Test-Path -LiteralPath $o){ $ents=@(Select-String -Path $o -Pattern '^## UNIT ').Count }; $l=$r+'\RUN_LEDGER.md'; $led=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1) }; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $back=$led.Contains('backstop'); $ref=$con.Contains('REFUSED: THIS APPROACH IS ALREADY RECORDED AS FAILED AT CRITERION 2.3'); $rec='not printed'; $sw='not printed'; if($con -match 'it recorded *: *(.+)'){ $rec=$Matches[1].Trim() }; if($con -match 'shared words *: *(.+)'){ $sw=$Matches[1].Trim() }; $wantrec='after ten quiet looks and no more, the fixture in question is killed - that is the move this criterion wants'; $wantsw='criterion fixture killed looks move quiet'; 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'record entries : ' + $ents; 'refusal seen   : ' + $ref; 'it recorded    : ' + $rec; 'shared words   : ' + $sw; 'halt line      : ' + $led; switch($arm){ 'reword' { $ok=($units -eq 0) -and ($ents -eq 1) -and $ref -and ($rec -ceq $wantrec) -and ($sw -ceq $wantsw); $must='SUBSTANCE, NOT WORDING. The recorded approach says the same thing in different words - reordered, re-punctuated, extra words, and NOT a substring - and it REFUSES anyway: no unit ran, the record is untouched at one entry, and the console prints the reworded text it matched against together with the exact six shared words criterion fixture killed looks move quiet' } 'nearmiss' { $ok=($units -eq 1) -and (-not $ref) -and $back -and ($rec -ceq 'not printed') -and ($sw -ceq 'not printed'); $must='THE MEASURED BLIND SPOT. The same intent with ONE distinctive word swapped for a synonym - killed became terminated - breaks the containment test, so nothing is refused, the unit RUNS, and the loop reaches its backstop. That costs one repeated unit, which the no-advance path catches. 068 ruled this the right direction and 078 did not tighten it. A FAIL HERE MEANS THE MATCHER WAS TIGHTENED' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:verdict067
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $o=$r+'\PHASE_OUTCOME.md'; $ents=0; if(Test-Path -LiteralPath $o){ $ents=@(Select-String -Path $o -Pattern '^## UNIT ').Count }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $lk=Test-Path -LiteralPath ($r+'\SESSION.lock'); $ro=Test-Path -LiteralPath ($r+'\output.md'); $rd=$r+'\.run-unit\reports'; $reps=@(); if(Test-Path -LiteralPath $rd){ $reps=@(Get-ChildItem -LiteralPath $rd | Sort-Object Name | ForEach-Object { $_.Name }) }; $src=(Get-FileHash -LiteralPath ('%HERE%..\watchdog\fixture-output.md') -Algorithm SHA256).Hash; $h1='absent'; $h2='absent'; if(Test-Path -LiteralPath ($rd+'\unit-1-output.md')){ $h1=(Get-FileHash -LiteralPath ($rd+'\unit-1-output.md') -Algorithm SHA256).Hash }; if(Test-Path -LiteralPath ($rd+'\unit-1-output-2.md')){ $h2=(Get-FileHash -LiteralPath ($rd+'\unit-1-output-2.md') -Algorithm SHA256).Hash }; $back=$led.Contains('backstop'); 'run-phase exit : %RRC%'; if('%URC%' -ne ''){ 'run-unit direct: exit %URC%' }; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'record entries : ' + $ents; 'SESSION.lock   : ' + $(if($lk){ 'still at the root' } else { 'gone' }); 'root output.md : ' + $(if($ro){ 'still at the root' } else { 'gone' }); 'reports kept   : ' + $(if($reps.Count){ $reps -join ', ' } else { 'none' }); 'halt line      : ' + $led; switch($arm){ 'leftover' { $ok=($units -eq 1) -and ($ents -eq 1) -and $con.Contains('THAT FILE SAYS  UNIT: 358') -and $con.Contains('NOTHING WAS LAUNCHED ON TWO ATTEMPTS') -and $all.Contains('launch skipped') -and (-not $all.Contains('stop 11')) -and $led.Contains('backstop'); $must='REVERSED BY 087 - 067 asserted stop 11 naming UNIT: 358. Now: one unit ran and ONE entry is in the record - none for iteration 2, whose launch started nothing twice - the leftover file is still NOT JUDGED and its UNIT: 358 line is still named on the console, the iteration is skipped with a ledger note, and the loop reaches its backstop - 067 intact, the night not ended' } 'lockleak' { $ok=($units -eq 2) -and ($ents -eq 2) -and $back -and (-not $lk); $must='two units ran and two entries were recorded, so iteration 1 released on its clean exit, and no SESSION.lock is left behind' } 'lock-dead' { $ok=($units -eq 1) -and ($ents -eq 1) -and $back -and $all.Contains('an orphaned session lock was cleared') -and $all.Contains('999999') -and (-not $lk); $must='the orphaned lock was cleared, the unit ran, the ledger says an orphaned lock naming pid 999999 was cleared, and nothing is left behind' } 'lock-live' { $ok=('%RRC%' -eq '3') -and ($c.Count -eq 0) -and ('%URC%' -eq '1') -and $con.Contains('FOUND RUNNING') -and $con.Contains('holder pid : 4'); $must='run-phase refused at its door with nothing called, and run-unit.bat refused at exit 1 naming pid 4 as FOUND RUNNING' } 'noreport' { $ok=($units -eq 1) -and ($ents -eq 1) -and $all.Contains('no report written by unit 1') -and $con.Contains('NOT RETRIED') -and $con.Contains('NO REPORT OF THIS UNIT') -and (-not $all.Contains('stop 11')) -and $led.Contains('backstop'); $must='REVERSED BY 087 - 067 asserted: the unit ran and wrote nothing, NOTHING was appended, and the halt is stop 11. Now: the unit ran and is NOT retried, one entry lands with its fate not recorded and nothing judged, the ledger carries the no-report note, no stop 11, and the loop reaches its backstop' } 'kept' { $ok=($units -eq 1) -and ($reps.Count -eq 1) -and ($reps[0] -ceq 'unit-1-output.md') -and ($h1 -eq $src) -and $ro; $must='the judged report is at .run-unit\reports\unit-1-output.md, SHA256-identical to what the unit wrote, and the root output.md SURVIVES - the owner-s ruling of 2026-09-19' } 'kept-twice' { $ok=($units -eq 1) -and ($reps.Count -eq 2) -and ($h1 -ne $src) -and ($h1 -ne 'absent') -and ($h2 -eq $src) -and $ro -and $con.Contains('unit-1-output-2.md'); $must='neither file is lost - the earlier unit-1-output.md untouched, the new report beside it as unit-1-output-2.md, the console says where it went, and the root output.md SURVIVES' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem ============================================================
rem  083'S ARMS, JUDGED BY TWO CALLS OF THEIR OWN - the parking arms and the
rem  state arms - for the reason every split before them gives: cmd refuses a
rem  command line past 8191 characters.
rem
rem  EVERY PARKING ARM READS PARKED.md, THE MARK FILE AND THE LEDGER, not only
rem  the console, because the question is what the owner reads at breakfast and
rem  the mark is what the next iteration decides on. The two state arms read the
rem  RECORD HEADER back after the run, because bringing it up to the checkboxes
rem  is half of what task 1 claims.
:verdict083
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $l=$r+'\RUN_LEDGER.md'; $led=''; $all=''; if(Test-Path -LiteralPath $l){ $led=[string](Get-Content -LiteralPath $l -Tail 1); $all=[IO.File]::ReadAllText($l) }; $col=@($led -split '\|'); $verdict=''; $answer=''; if($col.Count -ge 7){ $verdict=$col[4].Trim(); $answer=$col[6].Trim() }; $pkf=$r+'\PARKED.md'; $pkd=''; $pkl=@(); if(Test-Path -LiteralPath $pkf){ $pkd=[IO.File]::ReadAllText($pkf); $pkl=@(Get-Content -LiteralPath $pkf | Where-Object { $_ -like 'PARKED:*' }) }; $mk=@(); if(Test-Path -LiteralPath ($r+'\.run-unit\parked.txt')){ $mk=@(Get-Content -LiteralPath ($r+'\.run-unit\parked.txt')) }; $o=$r+'\PHASE_OUTCOME.md'; $at=@(); if(Test-Path -LiteralPath $o){ $at=@(Select-String -Path $o -Pattern '^ATTEMPT: *([0-9]+\.[0-9]+)' | ForEach-Object { $_.Matches[0].Groups[1].Value }) }; $pf=$r+'\.run-unit\arbiter-prompt.txt'; $pr=''; if(Test-Path -LiteralPath $pf){ $pr=[IO.File]::ReadAllText($pf) }; $sp=$r+'\review\sheet.md'; $sheet=Test-Path -LiteralPath $sp; $st=''; if($sheet){ $st=[IO.File]::ReadAllText($sp) }; $parks=@([regex]::Matches($con, 'PARKED, NOT HALTED')).Count; $back=$led.Contains('backstop'); $ip=$st.IndexOf('PARKED'); $is=$st.IndexOf('## Step 1'); 'run-phase exit : %RRC%'; if('%RRC2%' -ne ''){ 'second pass    : exit %RRC2%' }; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'parks on console: ' + $parks; 'PARKED.md lines: ' + $(if($pkl.Count){ $pkl -join '  //  ' } else { 'none - file ' + $(if(Test-Path -LiteralPath $pkf){ 'present but empty' } else { 'absent' }) }); 'marks this pass: ' + $(if($mk.Count){ $mk -join ' ' } else { 'none' }); 'attempts at    : ' + $(if($at.Count){ $at -join ', ' } else { 'none' }); 'ledger verdict : ' + $verdict; 'ledger says    : ' + $(if($answer.Length -gt 120){ $answer.Substring(0,120) + '...' } else { $answer }); 'review sheet   : ' + $sheet + $(if($sheet){ ' - parked block at ' + $ip + ', first step at ' + $is } else { '' }); switch($arm){ 'park-one' { $ok=($units -eq 2) -and ($parks -eq 1) -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.3 |') -and $pkl[0].Contains('| keying |') -and ($mk -contains '2.3') -and $pr.Contains('PARKED THIS PASS') -and $pr.Contains('2.3 the fixture is killed at ten quiet looks   (parked)') -and $pr.Contains('- [ ] 2.2 the slow fixture survives') -and (($at -join ',') -eq '2.3,2.2') -and $all.Contains('parked - criterion 2.3') -and $back; $must='the hit on 2.3 is PARKED to PARKED.md naming keying, 2.3 is marked, the next prompt lists 2.3 as parked and 2.2 as the work, the second unit runs at 2.2, the ledger carries the park, and the loop CONTINUES to its backstop - no halt on the question' } 'park-twice' { $ok=($units -eq 3) -and ($parks -eq 2) -and ($pkl.Count -eq 2) -and $pkl[0].StartsWith('PARKED: 2.3 |') -and $pkl[1].StartsWith('PARKED: 2.2 |') -and ($mk -contains '2.3') -and ($mk -contains '2.2') -and (($at -join ',') -eq '2.3,2.2,2.4') -and $back; $must='two hits in a row on 2.3 then 2.2 are BOTH parked and BOTH skipped, and a third unit runs at 2.4 - work continues on the rest' } 'park-last' { $ok=('%RRC%' -eq '0') -and ($units -eq 1) -and ($verdict -eq 'ending') -and $answer.Contains('every remaining criterion is the owner-s or parked') -and $answer.Contains('1 parked') -and ($pkl.Count -eq 1) -and $pkl[0].StartsWith('PARKED: 2.2 |') -and $pkl[0].Contains('| money |') -and $sheet -and ($ip -ge 0) -and ($is -gt $ip) -and $st.Contains('2.2') -and $st.Contains('money'); $must='the hit on the ONLY open non-owner criterion ends the night at EXIT 0: the ledger verdict is ending and names 1 parked, and the sheet carries the parked question ABOVE the criteria, naming money' } 'park-only' { $ok=('%RRC%' -eq '0') -and ($units -eq 1) -and ($verdict -eq 'ending') -and $answer.Contains('every remaining criterion is the owner-s or parked') -and $answer.Contains('PARKED.md') -and $con.Contains('PARKED.md') -and (-not $sheet) -and ($pkl.Count -eq 1) -and $pkl[0].Contains('| promise |'); $must='with no owner-s line and no REVIEW_SHEET, the night ends at exit 0 as an ending and PARKED.md-s path is on the console and in the ledger line so the question cannot be lost' } 'park-unknown' { $ok=($units -eq 1) -and (-not $answer.Contains('stop 3')) -and (Test-Path -LiteralPath $pkf) -and ([IO.File]::ReadAllText($pkf).Contains('| unread |')) -and $answer.Contains('backstop'); $must='REVERSED BY 087 - 083 asserted: an UNREADABLE section-4 judge still HALTS at stop 3 as unknown, a failure, nothing parked. Now: retried once, unreadable again, the criterion PARKS with which=unread, no stop 3, and the loop reaches its backstop' } 'park-arbstop' { $ok=($units -eq 0) -and $con.Contains('STOP 4: THE ARBITER DECLARED A DECISION THE OWNER') -and $led.Contains('stop 4') -and (-not (Test-Path -LiteralPath $pkf)) -and ($parks -eq 0); $must='the arbiter raising one of the three itself still HALTS at stop 4 and nothing is parked - :arbstop is untouched' } 'park-fresh' { $n23=@($at | Where-Object { $_ -eq '2.3' }).Count; $ok=($units -ge 3) -and ($parks -eq 1) -and $con.Contains('parked marks from an earlier pass cleared: 1') -and ($n23 -ge 2) -and ($mk -notcontains '2.3'); $must='a NEW loop over the same root clears the mark at its door, says so, and authors the parked criterion 2.3 again - the park was one pass, never a permanent block' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

rem  084 task 6: THE TARGET IS READ FROM THE CONSOLE, NOT FROM THE PROMPT FILE.
rem  The two state arms run with --seed, so the arbiter is never called on
rem  iteration 1 and arbiter-prompt.txt is never written; both failed BOTH
rem  passes of 084's task 1 - "prompt target : step not written" - while the
rem  console showed the launcher doing exactly what the must line demands. The
rem  old source, word for word:
rem      $tgt='not written'; if($pr -match '(?m)^THE STEP TO WORK: step ([0-9]+)'){ $tgt=$Matches[1] }
rem  and state-header-notstarted also asserted $pr.Contains('[partial by its
rem  checkboxes'). Both now read :stepfromcrit's own console line, "target step
rem  N", and the derivation's position string. target-not-zero is PROMPTONLY,
rem  writes the prompt, and keeps reading it.
:verdict083b
powershell -NoProfile -Command "$r='%FROOT%'; $arm='%ARM%'; $c=@(); if(Test-Path -LiteralPath ($r+'\calls.txt')){ $c=@(Get-Content -LiteralPath ($r+'\calls.txt')) }; $units=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $con=''; if(Test-Path -LiteralPath ($r+'\console.txt')){ $con=[IO.File]::ReadAllText($r+'\console.txt') }; $o=$r+'\PHASE_OUTCOME.md'; $hdr=@(); $adv=@(); if(Test-Path -LiteralPath $o){ $hdr=@(Select-String -Path $o -Pattern '^STEP: [0-9]+ \|' | ForEach-Object { $_.Line }); $adv=@(Select-String -Path $o -Pattern '^ADVANCED: ' | ForEach-Object { $_.Line.Substring(10) }) }; $pf=$r+'\.run-unit\arbiter-prompt.txt'; $pr=''; if(Test-Path -LiteralPath $pf){ $pr=[IO.File]::ReadAllText($pf) }; $tgt='not written'; if($pr -match '(?m)^THE STEP TO WORK: step ([0-9]+)'){ $tgt=$Matches[1] }; $ctgt='not printed'; if($con -match 'target step ([0-9]+|none)'){ $ctgt=$Matches[1] }; 'run-phase exit : %RRC%'; 'calls          : ' + $(if($c.Count){ $c -join ', ' } else { 'none' }); 'units run      : ' + $units; 'ADVANCED lines : ' + ($adv -join ', '); 'prompt target  : step ' + $tgt; 'console target : step ' + $ctgt; 'header after   : ' + ($hdr -join '  //  '); switch($arm){ 'state-header-done' { $ok=($units -eq 1) -and ($ctgt -eq '3') -and $con.Contains('record header: step 2 in progress -> done') -and (($hdr | Where-Object { $_ -like 'STEP: 2 | done |*' }).Count -eq 1) -and $con.Contains('position: 1=done,2=done,3=not started') -and (($adv -join ',') -eq 'yes'); $must='a step done by its checkboxes but in progress in the header is treated as DONE - not chosen as the target, and the header is brought up to done; the unit runs at 3.1 and advances' } 'state-header-notstarted' { $ok=($units -eq 1) -and ($ctgt -eq '2') -and $con.Contains('2=partial') -and $con.Contains('record header: step 2 not started -> partial') -and (($hdr | Where-Object { $_ -like 'STEP: 2 | partial |*' }).Count -eq 1) -and (($adv -join ',') -eq 'yes'); $must='a step not started in the header but partly ticked is treated as PARTIAL, its unmet criteria are authorable, the unit runs at 2.3 and advances, and the header reads partial' } 'target-not-zero' { $ok=($tgt -eq '3') -and (-not $pr.Contains('THE STEP TO WORK: step 0')) -and $con.Contains('target step 3') -and $con.Contains('0=done'); $must='HamLet-s case: steps 3 and 4 have open work and step 0 is fully ticked though the header says not started - the prompt aims at step 3, NEVER at step 0' } }; 'must           : ' + $must; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:transport
for %%I in ("%HERE%..\..") do set "ARB=%%~fI\"
set "TP=%TEMP%\cps-progress-fixture-transport.md"
powershell -NoProfile -Command "$t=[IO.File]::ReadAllText('%HERE%PHASE_PLAN.md'); $t=$t.Replace([string][char]13 + [string][char]10, [string][char]10).Replace([string][char]10, [string][char]13); [IO.File]::WriteAllText('%TP%', $t, (New-Object System.Text.UTF8Encoding($true))); $b=[IO.File]::ReadAllBytes('%TP%'); 'written       : ' + $b.Length + ' bytes, BOM ' + ($b[0] -eq 239) + ', LF bytes ' + @($b | Where-Object { $_ -eq 10 }).Count + ', CR bytes ' + @($b | Where-Object { $_ -eq 13 }).Count"
rem  THE VARIABLES ARE T PLUS THE SCRIPT'S OWN KEYS - TMET, TTOTAL, TCRIT.
rem  The first run of this arm read TTOT, a name nothing set, printed an
rem  empty total and failed a count that was right.
set "TMET="
set "TTOTAL="
set "TCRIT="
for /f "usebackq tokens=1,* delims==" %%A in (`call "%ARB%criteria-count.bat" "%TP%" 2 3`) do set "T%%A=%%B"
echo  counted       : MET=%TMET% TOTAL=%TTOTAL% CRIT=%TCRIT%
echo  must          : MET=1 TOTAL=3 CRIT=unmet
set "RC=1"
if "%TMET%%TTOTAL%%TCRIT%"=="13unmet" set "RC=0"
if "%RC%"=="0" echo  verdict       : PASS
if not "%RC%"=="0" echo  verdict       : FAIL
goto :end

:dupplan
for %%I in ("%HERE%..\..") do set "ARB=%%~fI\"
echo.
echo  plan-check.bat on plan-dup.md, which carries criterion 2.2 twice:
call "%ARB%plan-check.bat" "%HERE%plan-dup.md"
set "DRC=%ERRORLEVEL%"
echo.
echo  plan-check exit : %DRC%   (must be 1)
set "RC=1"
if "%DRC%"=="1" set "RC=0"
if "%RC%"=="0" echo  verdict         : PASS
if not "%RC%"=="0" echo  verdict         : FAIL
goto :end

rem  066: THE ATTEMPT RECORD'S TRANSPORT. One record, outcome-attempts.md,
rem  written four ways - LF, CRLF, CR-only, and CR-only with a byte-order
rem  mark - and read by attempt-read.bat from each. All four must print the
rem  same three attempts at 2.3: units 1, 3 and 5, in order, the long
rem  approach with a pipe in it whole, 12.3 and the fenced example not read.
:attempttransport
for %%I in ("%HERE%..\..") do set "ARB=%%~fI\"
set "TB=%TEMP%\cps-progress-fixture-attempt-transport"
if exist "%TB%" rd /s /q "%TB%"
mkdir "%TB%"
powershell -NoProfile -Command "$t=[IO.File]::ReadAllText('%HERE%outcome-attempts.md').Replace([string][char]13 + [string][char]10, [string][char]10); $n=New-Object System.Text.UTF8Encoding($false); $b=New-Object System.Text.UTF8Encoding($true); [IO.File]::WriteAllText('%TB%\lf.md', $t, $n); [IO.File]::WriteAllText('%TB%\crlf.md', $t.Replace([string][char]10, [string][char]13 + [string][char]10), $n); [IO.File]::WriteAllText('%TB%\cr.md', $t.Replace([string][char]10, [string][char]13), $n); [IO.File]::WriteAllText('%TB%\cr-bom.md', $t.Replace([string][char]10, [string][char]13), $b); foreach($v in 'lf','crlf','cr','cr-bom'){ $y=[IO.File]::ReadAllBytes('%TB%\' + $v + '.md'); ' written  ' + $v + ' : ' + $y.Length + ' bytes, BOM ' + ($y[0] -eq 239) + ', CR ' + @($y | Where-Object { $_ -eq 13 }).Count + ', LF ' + @($y | Where-Object { $_ -eq 10 }).Count }"
for %%V in (lf crlf cr cr-bom) do call "%ARB%attempt-read.bat" 2.3 "%TB%\%%V.md" >"%TB%\%%V.out" 2>&1
powershell -NoProfile -Command "$d='%TB%'; $lf=@(Get-Content -LiteralPath ($d + '\lf.out') | Where-Object { $_ -ne '' }); $ok=($lf.Count -eq 5) -and ($lf[0] -ceq 'CRITERION=2.3') -and ($lf[1] -ceq 'ATTEMPTS=3') -and ($lf[2] -ceq 'ATTEMPT_1=unit 1 | no | executed | poll the process tree') -and $lf[3].StartsWith('ATTEMPT_2=unit 3 | no | never ran | read the watchdog') -and $lf[3].EndsWith('so that a restarted watcher resumes the count') -and ($lf[4] -ceq 'ATTEMPT_3=unit 5 | yes | executed | kill by PID and release the lock'); ' read     lf : ' + $lf[1] + ', ' + $lf[2].Substring(0,20) + ' ... ' + $lf[4].Substring(0,20); foreach($v in 'crlf','cr','cr-bom'){ $x=@(Get-Content -LiteralPath ($d + '\' + $v + '.out') | Where-Object { $_ -ne '' }); $same=(($x -join [char]10) -ceq ($lf -join [char]10)); ' read     ' + $v + ' : ' + $(if($same){ 'identical to LF' } else { 'DIFFERS' }); if(-not $same){ $ok=$false } }; ' must     : the same three attempts at 2.3 - units 1, 3 and 5 in order, the long approach whole - from all four transports'; ''; ' verdict  : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:usage
echo.
echo   run-fixture.bat ^<advance ^| noadvance ^| twice ^| closed ^| bad ^| blocker ^| blocker-twice ^| transport^>
echo   run-fixture.bat ^<owner-sheet ^| owner-again ^| owner-nosheet ^| one-work ^| owner-advances ^| reversal ^| nine ^| dupplan^>
echo   run-fixture.bat ^<attempt-one ^| attempt-three ^| attempt-restart ^| attempt-blocker-crit ^| attempt-blocker-unit ^| attempt-transport^>
echo   run-fixture.bat ^<leftover ^| lockleak ^| lock-dead ^| lock-live ^| noreport ^| kept ^| kept-twice^>
echo   run-fixture.bat ^<repeat ^| fresh ^| succeeded ^| othercrit ^| noid ^| restart-id ^| arbrecord ^| neverran^>
echo   run-fixture.bat ^<reword ^| nearmiss^>
echo   run-fixture.bat ^<promptorder ^| promptbig ^| sharecheck ^| whyplan ^| whyreport ^| whyboth^>
echo   run-fixture.bat ^<followsreport ^| followsplan^>
echo   run-fixture.bat ^<redirect-same ^| redirect-fresh ^| redirect-wander ^| redirect-repeat^>
echo   run-fixture.bat ^<redirect-refused ^| redirect-budget ^| redirect-maxiter^>
echo   run-fixture.bat ^<redirect-owner ^| redirect-stop11 ^| redirect-blocker ^| redirect-minutes^>
echo   run-fixture.bat ^<card-append ^| card-match ^| card-extra ^| card-locked ^| card-absent ^| card-panel^>
echo   run-fixture.bat ^<card-twice ^| card-steps ^| card-frozen^>
echo   run-fixture.bat ^<exhaust-three ^| exhaust-empty ^| exhaust-two ^| exhaust-same ^| exhaust-noreason^>
echo   run-fixture.bat ^<exhaust-stop ^| exhaust-stop-money ^| exhaust-stop-promise ^| exhaust-owner^>
echo   run-fixture.bat ^<end-exhausted ^| end-three ^| end-owner ^| end-twice^>
echo   run-fixture.bat ^<fail-stop11 ^| fail-maxiter ^| fail-noplan^>
echo   run-fixture.bat ^<park-one ^| park-twice ^| park-last ^| park-only ^| park-unknown ^| park-arbstop ^| park-fresh^>
echo   run-fixture.bat ^<state-header-done ^| state-header-notstarted ^| target-not-zero^>
echo   run-fixture.bat ^<prime-first ^| prime-label ^| prime-big ^| why-mixed ^| why-split^>
echo   run-fixture.bat ^<drift-report ^| drift-plan ^| drift-twice ^| stop-over-why^>
echo   run-fixture.bat ^<budget-none ^| drift-all^>
echo   run-fixture.bat ^<hb-noadv ^| hb-twice ^| hb-blocker-park ^| hb-unshaped ^| stop-over-hb^>
echo   run-fixture.bat ^<s4-retry ^| arb-retry ^| arb-twice ^| arb-stop-retry ^| launch-twice ^| deny-park ^| fail-noseed^>
echo   run-fixture.bat ^<stop-answered ^| stop-other-answer ^| stale-unit ^| stale-plan-current ^| stale-plan-gone^>
echo.
set "RC=2"

:end
endlocal & exit /b %RC%

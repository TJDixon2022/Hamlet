// ============================================================================
// RETIRED BY WORK INSTRUCTION 323 TASK 1. This file holds no tests.
//
// ThePsk31TabIsInertTests was written by unit 314 and its premise was in its
// name: the PSK31 tab does nothing. That premise ends here. PHASE_PLAN.md §R11
// says the operator sets nothing at the radio and §R12 says a session rewrites
// the tests it wrote while a door was shut, so this unit opened CanTransmitIn
// for PSK31 and the class's two send assertions - that a CQ press reaches no
// send path, and that no row is answerable - now assert the opposite of what
// Hamlet is for.
//
// WHERE ITS SURVIVING ASSERTIONS WENT. Two of them did not expire, because
// PSK31 is still not FT8:
//
//   NoDecoderRunsAndNoSlotGridIsCutUnderPsk31  ->  ThePsk31PanelSpeaksPsk31Tests
//   Ft8AndFt4AreUntouched                      ->  ThePsk31PanelSpeaksPsk31Tests
//
// WHERE THE CLICK RULE WENT. Ft8CanStillReachTheSendDoor and the refusal
// assertions are covered by ThePsk31ConversationCardTests.NothingOnTheCardTransmits,
// rewritten in the same task under §R12: it drives the same one door, asks it
// about FT8, PSK31 and a label with no modulator, and asserts that with no click
// nothing at all is written on the transmit path.
//
// WHY THE FILE IS STILL HERE. This session could not delete a file - every
// removal came back "requires approval" and the session was non-interactive - so
// the class is emptied rather than deleted. The name is off
// docs/carry-forward-tests.txt, and a file of comments compiles to nothing.
// ============================================================================

// Unit 217's temporary reading window onto the pinned clone. Its work is done: everything it printed
// is now asserted in tests/Ft8Sharp.Tests/Message/UpstreamMessageLayerInventoryTests.cs, which is the
// committed record.
//
// It is untracked, it was never committed, and it is emptied to this comment so that what is on disk
// and what is in the commit compile to the same tests. The harness has refused every session asked to
// delete TempEncoderProbe.cs, and refused units 214, 215 and 216 the same for theirs, so this is left
// behind the same way rather than another attempt being made. Reported as a refusal.

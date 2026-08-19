using Xunit;

// **THE RED COUNT HAS TO MEAN SOMETHING** (§0.0.1 applied to the suite itself).
//
// Running both projects at once, one pass reported five failures in this assembly
// and the two runs immediately after reported the one standing failure. The four
// extra were tests that build a real window headless, and a suite that invents
// four failures under load is a suite whose count nobody reads — which is how a
// standing baseline of two became something people learn to look past.
//
// The cause is not subtle and it is not this project's: **an Avalonia headless
// test runs on one process-wide dispatcher**, and xUnit runs test classes in
// parallel by default, so several tests take turns on a thing there is only one
// of. Two classes here also set `LayoutStore.Path`, a mutable static, for the
// same reason and with the same hazard.
//
// Serializing the whole assembly is the blunt fix and the right one at this size:
// this project's tests run in about two seconds, so the cost is not measurable,
// and a targeted collection would have to name every class that touches an
// Avalonia static — which is the list nobody maintains and the reason the flake
// existed.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

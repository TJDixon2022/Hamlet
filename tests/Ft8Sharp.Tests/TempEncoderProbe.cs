// SPENT SCRATCH FILE — NOT COMMITTED, SAFE TO DELETE.
//
// Unit 203, task 2, used a throwaway probe here to enumerate ft8/ in the pinned clone
// and read ft8/encode.c so encode174 could be ported. Its job is done and its contents
// are gone; what replaced it is UpstreamEncoderProvenanceTests.cs, which asserts the same
// provenance permanently instead of printing it once.
//
// The file is still on disk only because this session's working-directory sandbox refuses
// file deletion. It is deliberately empty of code so it compiles to nothing and adds no
// test, and it was never git-added. To be rid of it:
//
//     del tests\Ft8Sharp.Tests\TempEncoderProbe.cs

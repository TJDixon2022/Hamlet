// SPENT SCRATCH FILE — NOT COMMITTED, SAFE TO DELETE.
//
// Unit 289, task 1, used a throwaway probe here to stage the pinned clone's FT4 source
// — ft8/constants.h, ft8/constants.c, ft8/encode.c, ft8/decode.c, demo/gen_ft8.c and
// common/monitor.c — into artifacts/, which .gitignore already excludes, so that the
// C could be read while porting. A session's file tools are confined to this
// repository and the clone is outside it. Unit 203 did exactly the same thing and left
// the same note in TempEncoderProbe.cs.
//
// Its job is done and its contents are gone. What replaces it permanently is the
// provenance the ported files carry at the point of use, and the two comparisons
// against upstream's own binary: Ft4SymbolBitIdentityTests and
// Ft4WaveformComparisonTests.
//
// The file is still on disk only because this session's working-directory sandbox
// refuses file deletion. It is deliberately empty of code so it compiles to nothing and
// adds no test, and it was never git-added. To be rid of it:
//
//     del tests\Ft8Sharp.Tests\Unit289SourceProbe.cs

using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Licensing;

/// <summary>
/// **The license lookup the application uses: callook.info, one polite client per request.**
/// </summary>
/// <remarks>
/// <para>**WHAT `ResolveProfileAsync` DID INLINE, MOVED BEHIND THE SEAM AND NOT CHANGED** (work
/// instruction 355 task 4). Each request builds a <see cref="CallookCallsignLookup"/> that names the
/// app and the operator in its User-Agent, asks once, and disposes it, exactly as before.</para>
/// <para>**IT IS THE DEFAULT AND ONLY THE DEFAULT.** A view model built by a test gets the lookup the
/// test hands in, or the network denied; see <c>MainWindowViewModel.DefaultLicenseLookup</c>.</para>
/// </remarks>
internal sealed class LiveCallookLookup : ICallsignLookup
{
    /// <inheritdoc/>
    public string SourceName => CallookCallsignLookup.ServiceName;

    /// <inheritdoc/>
    public async Task<CallsignLookupResult?> LookupAsync(
        string callsign, CancellationToken cancellationToken = default)
    {
        using var lookup = new CallookCallsignLookup(AboutViewModel.AppVersion, callsign);

        return await lookup.LookupAsync(callsign, cancellationToken);
    }
}

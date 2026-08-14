using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// A menu line that tunes somewhere actually does (HM-DEC-072).
/// </summary>
/// <remarks>
/// The failure this guards against is specific and silent. A menu opens in its
/// own popup, a popup is a separate visual tree, and a binding that walks up to
/// the window for its command resolves to nothing there. The item then compiles,
/// renders correctly, and does nothing at all when clicked. So the command lives
/// on the item, and a test can execute the thing a click would run.
/// </remarks>
public sealed class TuneMenuItemTests
{
    /// <remarks>
    /// Proves HM-DEC-072: the command is on the item and it runs. If somebody
    /// moves it back onto an ancestor binding, the item still builds and this
    /// still passes, so the test is paired with the comment on the class rather
    /// than standing in for it.
    /// </remarks>
    [Fact]
    public void TheItemCarriesTheCommandAndRunsIt()
    {
        var went = 0;
        var item = new TuneMenuItem("7.030, QRP watering hole", () => went++);

        Assert.Equal("7.030, QRP watering hole", item.Label);
        Assert.True(item.Tune.CanExecute(null));

        item.Tune.Execute(null);

        Assert.Equal(1, went);
    }

    /// <remarks>
    /// Proves HM-DEC-072: each line goes to its own place. Building these in a
    /// loop is exactly where a captured variable ends up pointing every line at
    /// the last entry, and the symptom is a menu where every item tunes to the
    /// bottom of the list.
    /// </remarks>
    [Fact]
    public void EveryLineGoesToItsOwnPlace()
    {
        var went = new List<long>();
        var places = new long[] { 7_030_000, 14_074_000, 3_573_000 };

        var items = new List<TuneMenuItem>();
        foreach (var place in places)
        {
            var target = place;
            items.Add(new TuneMenuItem($"{target}", () => went.Add(target)));
        }

        foreach (var item in items)
        {
            item.Tune.Execute(null);
        }

        Assert.Equal(places, went);
    }
}

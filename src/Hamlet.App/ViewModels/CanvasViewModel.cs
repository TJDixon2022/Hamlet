using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.App.Layout;

namespace Hamlet.App.ViewModels;

/// <summary>One widget as it sits on the canvas (HM-DEC-086).</summary>
/// <remarks>
/// The position is here rather than in the control, so the arrangement is a fact
/// the view model holds and the canvas draws, and saving it is reading properties
/// rather than interrogating visuals.
/// </remarks>
public sealed partial class WidgetViewModel : ObservableObject
{
    /// <summary>Places one widget.</summary>
    /// <param name="widget">Which widget.</param>
    /// <param name="placement">Where it goes.</param>
    /// <param name="body">
    /// What the widget shows, which is the main view model. The panels keep the
    /// bindings they already had, because what they are bound to has not changed.
    /// </param>
    public WidgetViewModel(Widget widget, Placement placement, object? body)
    {
        Widget = widget;
        Body = body;
        _x = placement.X;
        _y = placement.Y;
        _width = placement.Width;
        _height = placement.Height;
    }

    /// <summary>What it is.</summary>
    public Widget Widget { get; }

    /// <summary>The stable saved name.</summary>
    public string Id => Widget.Id;

    /// <summary>What its header says.</summary>
    public string Title => Widget.Title;

    /// <summary>Which color family (§0.6).</summary>
    public string Family => Widget.Family;

    /// <summary>What the body binds against.</summary>
    public object? Body { get; }

    /// <summary>Distance from the left of the canvas.</summary>
    [ObservableProperty]
    private double _x;

    /// <summary>Distance from the top.</summary>
    [ObservableProperty]
    private double _y;

    /// <summary>How wide.</summary>
    [ObservableProperty]
    private double _width;

    /// <summary>How tall.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DrawnHeight))]
    private double _height;

    /// <summary>
    /// Whether the panel inside is open (§0.5).
    /// </summary>
    /// <remarks>
    /// **A MIRROR AND NOT A SECOND COPY** (HM-DEC-086). The panel owns this and
    /// goes on persisting it per panel in `settings.json`, exactly as it did
    /// before it gained a position. This follows it so the frame can shrink to
    /// its header when it shuts, because on a canvas a collapsed panel that kept
    /// its height would leave a rectangle of nothing behind (§0).
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DrawnHeight))]
    private bool _isExpanded = true;

    /// <summary>How tall to actually draw it.</summary>
    /// <remarks>
    /// Not a number when it is shut, which is how Avalonia is told to take
    /// whatever height the content wants. The arranged height is kept untouched,
    /// so opening it again puts it back the size it was.
    /// </remarks>
    public double DrawnHeight => IsExpanded ? Height : double.NaN;

    /// <summary>
    /// True once the operator has moved or resized it themselves.
    /// </summary>
    /// <remarks>
    /// **WHAT STOPS A SUMMONED WIDGET VANISHING UNDER SOMEBODY'S HAND**
    /// (HM-DEC-086). The phrasebook arrives on its own when a contact starts and
    /// leaves when it ends, which is right until the operator drags it somewhere
    /// they want it. From that moment it is theirs and Hamlet stops taking it
    /// away.
    /// </remarks>
    public bool Adopted { get; set; }

    /// <summary>
    /// Called when the operator finishes moving or resizing it (HM-DEC-086).
    /// </summary>
    /// <remarks>
    /// Carried on the widget rather than looked up from the canvas, so the
    /// reporting does not depend on when the items panel happens to be built. A
    /// drag that quietly failed to save would look exactly like a drag that
    /// worked, until the next launch.
    /// </remarks>
    public Action<WidgetViewModel>? Settled { get; init; }

    /// <summary>
    /// Called when the operator takes hold of it (HM-DEC-087).
    /// </summary>
    /// <remarks>
    /// What brings it to the front. Dragging one widget over another used to put
    /// the moving one underneath, so it disappeared behind the thing it was
    /// being moved next to, which reads as the drag having failed.
    /// </remarks>
    public Action<WidgetViewModel>? Raised { get; init; }

    /// <summary>Where it is now, for saving.</summary>
    /// <returns>The placement.</returns>
    public Placement Placement() => new(Id, X, Y, Width, Height);
}

/// <summary>
/// How much a widget that is not out wants looking at (HM-DEC-087).
/// </summary>
/// <remarks>
/// **THE TWO USED TO LOOK IDENTICAL**, which made both of them wallpaper. Morse
/// arriving right now with no terminal on the canvas is not the same kind of
/// news as a tally that will still be there in an hour, and a notice that
/// cannot tell them apart teaches the operator to read past both.
/// </remarks>
public enum AbsentUrgency
{
    /// <summary>
    /// Worth knowing, and it will keep.
    /// </summary>
    /// <remarks>
    /// Nothing is being missed. Whatever this is has accumulated and will still
    /// be there whenever the widget comes back, so it is said quietly.
    /// </remarks>
    Quiet,

    /// <summary>
    /// Happening now.
    /// </summary>
    /// <remarks>
    /// Still nothing lost (§0.5, HM-DEC-086), but the operator is missing
    /// something while it is going on, which is a different fact and is said in
    /// a different voice.
    /// </remarks>
    Live,
}

/// <summary>
/// One line about a widget that is not out (HM-DEC-086, HM-DEC-087).
/// </summary>
/// <param name="Id">Which widget would show it.</param>
/// <param name="Title">What that widget is called.</param>
/// <param name="Says">What is happening, in one line.</param>
/// <param name="Urgency">How much it wants looking at.</param>
public sealed record AbsentNote(
    string Id, string Title, string Says, AbsentUrgency Urgency = AbsentUrgency.Quiet)
{
    /// <summary>True when this is happening right now.</summary>
    public bool IsLive => Urgency == AbsentUrgency.Live;

    /// <summary>True when it will keep.</summary>
    public bool IsQuiet => Urgency == AbsentUrgency.Quiet;

    /// <summary>
    /// How the note is drawn, as a style class rather than a brush (HM-DEC-087).
    /// </summary>
    /// <remarks>
    /// **THE COLORS STAY IN THE MARKUP**, where the app's palette already lives.
    /// Writing them here would be a second copy of that palette, and a second
    /// copy drifts: the send panel already carries a hand-typed amber a shade off
    /// the real one, which is exactly the fault §0 exists to prevent. So this
    /// says which of the two a note is and the styles say what that looks like.
    /// </remarks>
    public string Look => IsLive ? "live" : "quiet";
}

/// <summary>
/// The canvas: what is out, what is available, and what is going on off it
/// (HM-DEC-086).
/// </summary>
/// <remarks>
/// <para>**THE MAIN SCREEN WAS ONE VERTICAL SCROLL AND HAD OUTGROWN IT.** Every
/// panel this application has ever grown went into one column in the order it was
/// built, and the operator scrolled past the ten they were not using to reach the
/// two they were. So the panels become widgets, the arrangement becomes theirs,
/// and the app stops deciding what matters this minute.</para>
/// <para>The strip along the top is not part of this. Band, frequency, mode,
/// where you are and whether you may transmit are the things you need before you
/// need anything else, and a canvas you can rearrange is exactly the wrong place
/// for them (§0.5's exemption for the rig display, widened by this ruling to the
/// whole strip).</para>
/// </remarks>
public sealed partial class CanvasViewModel : ObservableObject
{
    private readonly object? _body;
    private readonly Action? _changed;
    private readonly Action<string>? _open;
    private readonly HashSet<string> _summoned = new(StringComparer.Ordinal);

    /// <summary>Builds the canvas over whatever the widgets bind against.</summary>
    /// <param name="body">
    /// What every widget body binds to, which is the main view model.
    /// </param>
    /// <param name="book">What was saved last time.</param>
    /// <param name="changed">
    /// Called whenever the arrangement changes, so it can be kept.
    /// </param>
    /// <param name="open">
    /// How to open a panel that has just arrived, so a widget somebody reached
    /// for shows what they reached for (HM-DEC-087).
    /// </param>
    public CanvasViewModel(
        object? body,
        LayoutBook? book = null,
        Action? changed = null,
        Action<string>? open = null)
    {
        _body = body;
        _changed = changed;
        _open = open;

        foreach (var preset in LayoutPresets.All)
        {
            Presets.Add(preset);
        }

        foreach (var saved in (book ?? new LayoutBook()).Kept)
        {
            Saved.Add(saved);
        }

        // NOBODY EVER STARTS ON AN EMPTY CANVAS (HM-DEC-086). A first run, or a
        // layouts file that could not be read, lands on the furnished preset
        // rather than on a blank rectangle beside a list of things to drag.
        StartedFrom = book?.StartedFrom ?? "";
        Apply(book?.Current ?? Start());
    }

    private CanvasLayout Start()
    {
        StartedFrom = LayoutPresets.FirstRun;

        return LayoutPresets.Start();
    }

    /// <summary>What is on the canvas.</summary>
    public ObservableCollection<WidgetViewModel> Placed { get; } = new();

    /// <summary>What is not, ready to be dragged on.</summary>
    public ObservableCollection<Widget> Tray { get; } = new();

    /// <summary>The arrangements Hamlet ships.</summary>
    public ObservableCollection<CanvasLayout> Presets { get; } = new();

    /// <summary>The arrangements the operator kept.</summary>
    public ObservableCollection<CanvasLayout> Saved { get; } = new();

    /// <summary>
    /// What is happening on widgets that are not out (HM-DEC-086).
    /// </summary>
    public ObservableCollection<AbsentNote> Absent { get; } = new();

    /// <summary>True when anything off the canvas has something to say.</summary>
    public bool HasAbsent => Absent.Count > 0;

    /// <summary>Which preset this arrangement began as, or "".</summary>
    [ObservableProperty]
    private string _startedFrom = "";

    /// <summary>What to call an arrangement being saved.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(KeepCommand))]
    private string _newName = "";

    /// <summary>True when the tray has anything left in it.</summary>
    public bool HasTray => Tray.Count > 0;

    /// <summary>
    /// Load an arrangement, as a fresh copy every time (HM-DEC-086).
    /// </summary>
    /// <param name="layout">Which one.</param>
    /// <remarks>
    /// **A PRESET IS A STARTING POINT AND NEVER A DOCUMENT.** Pressing one gives
    /// back exactly what it gave last time, whatever has been dragged about since,
    /// because what gets loaded is a copy and the original is never written to.
    /// </remarks>
    [RelayCommand]
    public void Load(CanvasLayout? layout)
    {
        if (layout is null)
        {
            return;
        }

        StartedFrom = layout.Preset ? layout.Name : "";
        Apply(layout.Fresh());
        Changed();
    }

    /// <summary>
    /// Put a widget on the canvas, showing its contents (HM-DEC-087).
    /// </summary>
    /// <param name="widget">Which one.</param>
    /// <remarks>
    /// **IT ARRIVES OPEN.** Somebody who reaches into the tray for a thing wants
    /// to see the thing, and every widget used to appear as a title bar with a
    /// triangle on it, so the canvas filled up with names of panels rather than
    /// panels. Collapse stays exactly where it was for something being kept
    /// around and not watched (§0.5).
    /// </remarks>
    [RelayCommand]
    public void Add(Widget? widget)
    {
        if (widget is null || Placed.Any(p => p.Id == widget.Id))
        {
            return;
        }

        Place(widget, Current().Room(widget), adopted: true);
        _open?.Invoke(widget.Id);
        Sync();
        Changed();
    }

    /// <summary>Take a widget off the canvas.</summary>
    /// <param name="widget">Which one.</param>
    /// <remarks>
    /// It goes back to the tray rather than away. Nothing it was showing is lost:
    /// the app keeps working out what it would have said, which is what makes the
    /// absent-widget line possible.
    /// </remarks>
    [RelayCommand]
    public void Remove(WidgetViewModel? widget)
    {
        if (widget is null)
        {
            return;
        }

        Placed.Remove(widget);
        _summoned.Remove(widget.Id);
        Sync();
        Changed();
    }

    /// <summary>True when a name has been typed to save under.</summary>
    public bool CanKeep => !string.IsNullOrWhiteSpace(NewName);

    /// <summary>
    /// Keep this arrangement under a name (HM-DEC-086).
    /// </summary>
    /// <remarks>
    /// **ONE ACTION FROM WHERE YOU ARE.** Not a dialog, not a manager window, not
    /// a mode: a box on the bar you are already looking at, and the arrangement in
    /// front of you is what gets saved. Anything more than that and nobody saves
    /// anything, and the presets become the only arrangements that exist.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(CanKeep))]
    public void Keep()
    {
        var name = NewName.Trim();

        var layout = new CanvasLayout(
            name,
            StartedFrom.Length > 0 ? $"Yours, from {StartedFrom}." : "Yours.",
            Current().Placements);

        var existing = Saved.FirstOrDefault(l => l.Name == name);

        if (existing is not null)
        {
            Saved[Saved.IndexOf(existing)] = layout;
        }
        else
        {
            Saved.Add(layout);
        }

        NewName = "";
        Changed();
    }

    /// <summary>Forget a saved arrangement.</summary>
    /// <param name="layout">Which one.</param>
    [RelayCommand]
    public void Forget(CanvasLayout? layout)
    {
        if (layout is null || layout.Preset)
        {
            return;
        }

        Saved.Remove(layout);
        Changed();
    }

    /// <summary>
    /// Hamlet brings a widget out on its own (HM-DEC-086).
    /// </summary>
    /// <param name="id">Which widget.</param>
    /// <remarks>
    /// <para>**THE MECHANISM IS GENERAL AND THE PHRASEBOOK IS THE FIRST CASE.**
    /// It appears when a contact starts and goes away when the contact ends,
    /// because that is exactly when somebody needs to know what people say and
    /// exactly when they do not.</para>
    /// <para>Only widgets that declare themselves summonable can arrive this way,
    /// so no future wiring can make an arbitrary panel jump onto somebody's
    /// canvas. And one the operator has moved is theirs from then on and is never
    /// taken away again.</para>
    /// </remarks>
    public void Summon(string id)
    {
        var widget = Widgets.Find(id);

        if (widget is not { Summoned: true } || Placed.Any(p => p.Id == id))
        {
            return;
        }

        Place(widget, Current().Room(widget), adopted: false);
        _open?.Invoke(id);
        _summoned.Add(id);
        Sync();
    }

    /// <summary>The reason a summoned widget was out has passed.</summary>
    /// <param name="id">Which widget.</param>
    public void Dismiss(string id)
    {
        if (!_summoned.Contains(id))
        {
            return;
        }

        var placed = Placed.FirstOrDefault(p => p.Id == id);

        _summoned.Remove(id);

        // Adopted means the operator put their hand on it, and from then on it is
        // theirs to remove.
        if (placed is { Adopted: false })
        {
            Placed.Remove(placed);
            Sync();
        }
    }

    /// <summary>
    /// Something happened that a widget would have shown (HM-DEC-086).
    /// </summary>
    /// <param name="id">Which widget would have shown it.</param>
    /// <param name="says">What happened, in one line, or "" when nothing is.</param>
    /// <param name="urgency">How much it wants looking at (HM-DEC-087).</param>
    /// <remarks>
    /// <para>**THE ANSWER TO WHAT HAPPENS WHEN THE WIDGET IS NOT OUT.** Morse
    /// arrives and the terminal is not on the canvas. The app may not swallow it,
    /// and it may not fling the terminal onto somebody's arrangement either.</para>
    /// <para>So the canvas carries a quiet line saying what is happening and
    /// offering to bring the widget out, which is §0.5's rule one level up: a
    /// collapsed panel still carries its summary, and a widget that is not out
    /// still carries its news. **Nothing is lost while it is off** — the app goes
    /// on working out what that widget would say, so bringing it out shows the
    /// history rather than starting from the moment it appeared.</para>
    /// </remarks>
    public void News(string id, string says, AbsentUrgency urgency = AbsentUrgency.Quiet)
    {
        var existing = Absent.FirstOrDefault(a => a.Id == id);
        var gone = string.IsNullOrWhiteSpace(says) || Placed.Any(p => p.Id == id);

        if (gone)
        {
            if (existing is not null)
            {
                Absent.Remove(existing);
                OnPropertyChanged(nameof(HasAbsent));
            }

            return;
        }

        var note = new AbsentNote(id, Widgets.Find(id)?.Title ?? id, says, urgency);

        if (existing is null)
        {
            Absent.Add(note);
            OnPropertyChanged(nameof(HasAbsent));
        }
        else if (existing != note)
        {
            Absent[Absent.IndexOf(existing)] = note;
        }
    }

    /// <summary>Bring out the widget one of those lines is about.</summary>
    /// <param name="note">Which line.</param>
    [RelayCommand]
    public void Show(AbsentNote? note)
    {
        if (note is null)
        {
            return;
        }

        Add(Widgets.Find(note.Id));
        News(note.Id, "");
    }

    /// <summary>
    /// What the last restore could not put back the way it found it.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRestoreNote))]
    private string _restoreNote = "";

    /// <summary>True when there is something to say about the restore.</summary>
    public bool HasRestoreNote => RestoreNote.Length > 0;

    /// <summary>Stop saying it.</summary>
    [RelayCommand]
    public void ClearRestoreNote() => RestoreNote = "";

    /// <summary>
    /// Bring anything that is entirely out of view back onto the canvas
    /// (HM-DEC-089).
    /// </summary>
    /// <param name="width">How wide the visible canvas is.</param>
    /// <param name="height">How tall it is.</param>
    /// <returns>True when anything had to be moved.</returns>
    /// <remarks>
    /// <para>**THIS IS WHY THE LAYOUT LOOKED LIKE IT DID NOT COME BACK.** It
    /// always came back. Position, size, membership and stacking order all
    /// persisted and all restored. What happened is that an arrangement built on
    /// a wide window, reopened on a narrow one, restored every widget faithfully
    /// to coordinates a long way off the right-hand edge, and the operator was
    /// shown an empty canvas and reasonably concluded it had been lost.</para>
    /// <para>**ONLY WHAT IS ENTIRELY OUT OF VIEW MOVES.** A widget hanging over
    /// the edge is a widget the operator can see and grab, and dragging it back
    /// for them would be undoing a choice they made. What gets rescued is what
    /// they could not have found.</para>
    /// <para>The rescue is a fallback and never a layout engine: things land in a
    /// simple cascade from the top left, because somewhere predictable can be
    /// fixed with one drag and somewhere clever cannot be found at all.</para>
    /// </remarks>
    public bool FitInto(double width, double height)
    {
        if (width <= 0 || height <= 0 || Placed.Count == 0)
        {
            return false;
        }

        var stranded = Placed
            .Where(p => p.X >= width - EdgeGrace || p.Y >= height - EdgeGrace)
            .ToList();

        if (stranded.Count == 0)
        {
            return false;
        }

        var x = Gap;
        var y = Gap;

        foreach (var widget in stranded)
        {
            widget.X = Math.Max(0, Math.Min(x, Math.Max(0, width - EdgeGrace)));
            widget.Y = Math.Max(0, Math.Min(y, Math.Max(0, height - EdgeGrace)));

            x += Cascade;
            y += Cascade;

            if (y > height - EdgeGrace)
            {
                y = Gap;
            }

            if (x > width - EdgeGrace)
            {
                x = Gap;
            }
        }

        RestoreNote = stranded.Count == 1
            ? $"{stranded[0].Title} was off the edge of a window this size, so it "
              + "has been moved back into view. Everything else is where you left it."
            : $"{stranded.Count} widgets were off the edge of a window this size, "
              + "so they have been moved back into view. Everything else is where "
              + "you left it.";

        Changed();

        return true;
    }

    /// <summary>How much of a widget has to be on screen for it to count.</summary>
    /// <remarks>
    /// Sixty pixels, which is about a header bar. Less than that and there is
    /// nothing to take hold of.
    /// </remarks>
    private const double EdgeGrace = 60;

    /// <summary>Where a rescued widget starts.</summary>
    private const double Gap = 12;

    /// <summary>How far each rescued widget is offset from the last.</summary>
    private const double Cascade = 46;

    /// <summary>The arrangement as it stands.</summary>
    /// <returns>What is on the canvas and where.</returns>
    public CanvasLayout Current()
        => new(
            StartedFrom, "", Placed.Select(p => p.Placement()).ToList());

    /// <summary>Everything worth writing down.</summary>
    /// <returns>The book.</returns>
    public LayoutBook Book()
        => new(Current(), Saved.ToList(), StartedFrom);

    /// <summary>The operator moved or resized something.</summary>
    /// <param name="widget">Which one.</param>
    /// <remarks>
    /// Their hand on it makes it theirs, which is what stops the phrasebook
    /// disappearing out from under somebody who has just put it where they want
    /// it (HM-DEC-086).
    /// </remarks>
    public void Moved(WidgetViewModel? widget)
    {
        if (widget is not null)
        {
            widget.Adopted = true;
            _summoned.Remove(widget.Id);
        }

        Changed();
    }

    /// <summary>
    /// Bring one widget to the front (HM-DEC-087).
    /// </summary>
    /// <param name="widget">Which one.</param>
    /// <remarks>
    /// Last in the list is drawn last and therefore on top. Done once, when the
    /// operator takes hold of it, rather than continuously: moving the item
    /// rebuilds its container, and a control rebuilt in the middle of a gesture
    /// is how the send buttons came to be dead (HM-DEC-078). The pointer is
    /// captured by the canvas rather than by the widget, so the drag survives it.
    /// </remarks>
    public void Raise(WidgetViewModel? widget)
    {
        if (widget is null)
        {
            return;
        }

        var at = Placed.IndexOf(widget);

        if (at >= 0 && at != Placed.Count - 1)
        {
            Placed.Move(at, Placed.Count - 1);
        }
    }

    private void Apply(CanvasLayout layout)
    {
        Placed.Clear();
        _summoned.Clear();

        var unknown = 0;

        foreach (var placement in layout.Placements)
        {
            // A LAYOUT NAMING A WIDGET THIS BUILD DOES NOT HAVE IS SKIPPED AND
            // NOT GUESSED AT. The placement stays in the file, so going back to a
            // build that has it restores the arrangement whole.
            if (Widgets.Find(placement.Id) is { } widget)
            {
                Place(widget, Sane(widget, placement), adopted: true);
            }
            else
            {
                unknown++;
            }
        }

        // SAY WHAT COULD NOT BE RESTORED RATHER THAN QUIETLY RESTORING LESS
        // (HM-DEC-089). A file written by a later build can name widgets this one
        // has never heard of, and an arrangement that silently comes back one
        // widget short is the operator wondering what they did wrong.
        RestoreNote = unknown switch
        {
            0 => "",
            1 => "One thing in your saved layout is not a widget this version has, "
                 + "so it was left out. It is still in the file.",
            _ => $"{unknown} things in your saved layout are not widgets this "
                 + "version has, so they were left out. They are still in the file.",
        };

        Sync();
    }

    /// <summary>
    /// A placement that can actually be seen and grabbed (HM-DEC-089).
    /// </summary>
    /// <param name="widget">What it is.</param>
    /// <param name="placement">What the file said.</param>
    /// <returns>The placement, with anything nonsensical replaced.</returns>
    /// <remarks>
    /// **A FILE FROM AN OLDER BUILD CAN BE MISSING FIELDS**, and a missing width
    /// deserializes to zero rather than to an error. A widget nought pixels wide
    /// is a widget that has been lost, so the widget's own default size stands in
    /// and everything else about the placement is kept.
    /// </remarks>
    private static Placement Sane(Widget widget, Placement placement)
        => placement with
        {
            X = double.IsFinite(placement.X) ? Math.Max(0, placement.X) : 0,
            Y = double.IsFinite(placement.Y) ? Math.Max(0, placement.Y) : 0,
            Width = placement.Width >= Smallest ? placement.Width : widget.Width,
            Height = placement.Height >= Smallest ? placement.Height : widget.Height,
        };

    /// <summary>The smallest a saved size may be before it is not believed.</summary>
    /// <remarks>Matches the canvas's own resize floor.</remarks>
    private const double Smallest = 160;

    private void Place(Widget widget, Placement placement, bool adopted)
        => Placed.Add(new WidgetViewModel(widget, placement, _body)
        {
            Adopted = adopted,
            Settled = Moved,
            Raised = Raise,
        });

    private void Sync()
    {
        Tray.Clear();

        foreach (var widget in Widgets.All)
        {
            if (Placed.All(p => p.Id != widget.Id))
            {
                Tray.Add(widget);
            }
        }

        OnPropertyChanged(nameof(HasTray));

        // A widget that has come out has nothing left to say from off the canvas.
        foreach (var note in Absent.Where(a => Placed.Any(p => p.Id == a.Id)).ToList())
        {
            Absent.Remove(note);
        }

        OnPropertyChanged(nameof(HasAbsent));
    }

    private void Changed() => _changed?.Invoke();

    partial void OnNewNameChanged(string value) => OnPropertyChanged(nameof(CanKeep));
}

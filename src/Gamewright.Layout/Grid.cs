namespace Gamewright.Layout;

using Microsoft.Extensions.Primitives;

/// <summary>
/// Grid with Pixel / Auto / Star tracks, Min/Max per track, row and column spans,
/// and RowSpacing/ColumnSpacing (WinUI-style). Empty Columns or Rows = one star track.
/// </summary>
public sealed record Grid(IReadOnlyList<Track> Columns, IReadOnlyList<Track> Rows, params Grid.Cell[] Cells) : Node
{
    /// <summary>
    /// Placement data for one grid child. Composition instead of attached properties:
    /// the node knows nothing about the grid it sits in.
    /// </summary>
    public sealed record Cell(int Row, int Column, Node Content)
    {
        public int RowSpan { get; init; } = 1;
        public int ColumnSpan { get; init; } = 1;
    }


    public Grid(string Columns, string Rows, params Cell[] Cells)
        : this(ParseDefinition(Columns), ParseDefinition(Rows), Cells) { }

    public override IReadOnlyList<Node> Children => [.. Cells.Select(c => c.Content)];


    public float RowSpacing { get; init; }

    public float ColumnSpacing { get; init; }

    private readonly IReadOnlyList<Track> _implicitRow = [new Track()];
    private readonly IReadOnlyList<Track> _implicitColumn = [new Track()];
    private IReadOnlyList<Track> EffRows => Rows.Count > 0 ? Rows : _implicitRow;
    private IReadOnlyList<Track> EffColumns => Columns.Count > 0 ? Columns : _implicitColumn;



    // ── Measure ──
    //
    // Cells are measured in groups so that Auto tracks are known before Star tracks are
    // divided up :
    //   G1  no star in either axis   → feeds auto columns and auto rows
    //   G3  star rows only           → measured with ∞ height, feeds auto columns
    //       ── resolve star columns ──
    //   G2  star columns only        → measured with resolved width, feeds auto rows
    //       ── resolve star rows ──
    //   G3  re-measured with the final heights
    //   G4  star in both axes        → measured with the resolved cell size
    // Limitation: if G3 content's width depends on its height (rare), the re-measured
    // width is not fed back into the auto columns. WPF iterates in that case.

    protected override Size MeasureOverride(Size availableSize)
    {
        var rows = EffRows;
        var cols = EffColumns;
        InitTracks(cols, float.IsPositiveInfinity(availableSize.W));
        InitTracks(rows, float.IsPositiveInfinity(availableSize.H));

        List<Cell> g1 = [], g2 = [], g3 = [], g4 = [];
        foreach (var cell in Cells)
        {
            bool starCol = SpansStar(cols, cell.Column, cell.ColumnSpan);
            bool starRow = SpansStar(rows, cell.Row, cell.RowSpan);
            var group = (starCol, starRow) switch
            {
                (false, false) => g1,
                (true, false) => g2,
                (false, true) => g3,
                (true, true) => g4,
            };
            group.Add(cell);
        }

        foreach (var c in g1) { MeasureCell(c, colsResolved: false, rowsResolved: false); }
        ContributeToAuto(g1, columns: true);
        ContributeToAuto(g1, columns: false);

        foreach (var c in g3) { MeasureCell(c, colsResolved: false, rowsResolved: false); }
        ContributeToAuto(g3, columns: true);

        ResolveStars(cols, availableSize.W, ColumnSpacing, useContentMin: false);

        foreach (var c in g2) { MeasureCell(c, colsResolved: true, rowsResolved: false); }
        ContributeToAuto(g2, columns: false);

        ResolveStars(rows, availableSize.H, RowSpacing, useContentMin: false);

        foreach (var c in g3) { MeasureCell(c, colsResolved: true, rowsResolved: true); }
        foreach (var c in g4) { MeasureCell(c, colsResolved: true, rowsResolved: true); }

        RecordContentSizes(cols, rows);

        // Desire: Pixel and Auto tracks at their size, Star tracks at their content size.
        return new Size(DesiredExtent(cols, ColumnSpacing), DesiredExtent(rows, RowSpacing));
    }

    private static void InitTracks(IReadOnlyList<Track> tracks, bool infinite)
    {
        foreach (var t in tracks)
        {
            t.Effective = t.Size.Unit == GridUnit.Star && infinite ? GridUnit.Auto : t.Size.Unit;
            t.ContentSize = 0;
            t.ActualSize = t.Effective == GridUnit.Pixel ? Clamp(t.Size.Value, t.Min, t.Max) : t.Min;
        }
    }

    private void MeasureCell(Cell cell, bool colsResolved, bool rowsResolved)
    {
        var (cs, cn) = Range(cell.Column, cell.ColumnSpan, EffColumns.Count);
        var (rs, rn) = Range(cell.Row, cell.RowSpan, EffRows.Count);
        cell.Content.Measure(new Size(
            SpanConstraint(EffColumns, cs, cn, ColumnSpacing, colsResolved),
            SpanConstraint(EffRows, rs, rn, RowSpacing, rowsResolved)));
    }

    /// <summary>
    /// Constraint for a cell along one axis: ∞ if the span contains an Auto track
    /// (size to content) or a Star track that is not resolved yet.
    /// </summary>
    private static float SpanConstraint(IReadOnlyList<Track> tracks, int start, int count, float spacing, bool starsResolved)
    {
        float sum = spacing * (count - 1);
        for (int i = start; i < start + count; i++)
        {
            var t = tracks[i];
            if (t.Effective == GridUnit.Auto || (t.Effective == GridUnit.Star && !starsResolved))
            {
                return float.PositiveInfinity;
            }
            sum += t.ActualSize;
        }
        return sum;
    }

    /// <summary>
    /// Grow Auto tracks to fit cell desires. Single spans go first; wider spans then
    /// distribute their excess evenly over the Auto tracks they cover.
    /// </summary>
    private void ContributeToAuto(List<Cell> cells, bool columns)
    {
        var tracks = columns ? EffColumns : EffRows;
        float spacing = columns ? ColumnSpacing : RowSpacing;

        foreach (var cell in cells.OrderBy(c => columns ? c.ColumnSpan : c.RowSpan))
        {
            var (start, count) = columns
                ? Range(cell.Column, cell.ColumnSpan, tracks.Count)
                : Range(cell.Row, cell.RowSpan, tracks.Count);
            float desired = columns ? cell.Content.DesiredSize.W : cell.Content.DesiredSize.H;

            float current = spacing * (count - 1);
            List<Track> autos = [];
            for (int i = start; i < start + count; i++)
            {
                current += tracks[i].ActualSize;
                if (tracks[i].Effective == GridUnit.Auto)
                {
                    autos.Add(tracks[i]);
                }
            }
            if (desired <= current || autos.Count == 0)
            {
                continue;
            }

            float extra = (desired - current) / autos.Count;
            foreach (var a in autos)
            {
                a.ActualSize = MathF.Min(a.ActualSize + extra, a.Max);
            }
        }
    }

    /// <summary>
    /// Divide what is left after Pixel/Auto tracks and spacing among the Star tracks by weight.
    /// Tracks whose share violates Min (or Max) are pinned there and removed from the pool,
    /// and the rest is redistributed.
    /// </summary>
    private static void ResolveStars(IReadOnlyList<Track> tracks, float available, float spacing, bool useContentMin)
    {
        var pool = tracks.Where(t => t.Effective == GridUnit.Star).ToList();
        if (pool.Count == 0 || float.IsPositiveInfinity(available))
        {
            return;
        }

        float used = spacing * (tracks.Count - 1)
                   + tracks.Where(t => t.Effective != GridUnit.Star).Sum(t => t.ActualSize);
        float remaining = available - used;

        float MinOf(Track t) => useContentMin ? MathF.Max(t.Min, MathF.Min(t.ContentSize, t.Max)) : t.Min;

        while (pool.Count > 0)
        {
            float totalWeight = pool.Sum(t => MathF.Max(0, t.Size.Value));
            float Share(Track t) => totalWeight > 0 ? MathF.Max(0, remaining) * MathF.Max(0, t.Size.Value) / totalWeight : 0;

            var pinned = pool.Where(t => Share(t) < MinOf(t)).ToList();
            if (pinned.Count > 0)
            {
                foreach (var t in pinned) { t.ActualSize = MinOf(t); remaining -= t.ActualSize; pool.Remove(t); }
                continue;
            }
            pinned = [.. pool.Where(t => Share(t) > t.Max)];
            if (pinned.Count > 0)
            {
                foreach (var t in pinned) { t.ActualSize = t.Max; remaining -= t.ActualSize; pool.Remove(t); }
                continue;
            }
            foreach (var t in pool) { t.ActualSize = Share(t); }
            break;
        }
    }

    private void RecordContentSizes(IReadOnlyList<Track> cols, IReadOnlyList<Track> rows)
    {
        foreach (var cell in Cells)
        {
            var (cs, cn) = Range(cell.Column, cell.ColumnSpan, cols.Count);
            var (rs, rn) = Range(cell.Row, cell.RowSpan, rows.Count);
            if (cn == 1) { cols[cs].ContentSize = MathF.Max(cols[cs].ContentSize, cell.Content.DesiredSize.W); }
            if (rn == 1) { rows[rs].ContentSize = MathF.Max(rows[rs].ContentSize, cell.Content.DesiredSize.H); }
        }
    }

    private static float DesiredExtent(IReadOnlyList<Track> tracks, float spacing) =>
        spacing * (tracks.Count - 1) + tracks.Sum(t =>
            t.Effective == GridUnit.Star ? Clamp(t.ContentSize, t.Min, t.Max) : t.ActualSize);

    // ── Arrange ──
    // Pixel and Auto tracks keep their measured size. Star tracks are re-divided against
    // the final size, which may differ from the measure constraint, and never shrink below
    // their content. This also turns stars that acted as Auto under an infinite constraint
    // back into proportional tracks.

    protected override void ArrangeOverride(Rect bounds)
    {
        var rows = EffRows;
        var cols = EffColumns;
        foreach (var t in cols.Concat(rows))
        {
            if (t.Size.Unit == GridUnit.Star) { t.Effective = GridUnit.Star; }
        }

        ResolveStars(cols, bounds.W, ColumnSpacing, useContentMin: true);
        ResolveStars(rows, bounds.H, RowSpacing, useContentMin: true);
        SetOffsets(cols, bounds.X, ColumnSpacing);
        SetOffsets(rows, bounds.Y, RowSpacing);

        foreach (var cell in Cells)
        {
            var (cs, cn) = Range(cell.Column, cell.ColumnSpan, cols.Count);
            var (rs, rn) = Range(cell.Row, cell.RowSpan, rows.Count);
            var rect = new Rect(
                cols[cs].Offset, rows[rs].Offset,
                SpanSize(cols, cs, cn, ColumnSpacing),
                SpanSize(rows, rs, rn, RowSpacing));

            cell.Content.Arrange(rect);
        }
    }

    private static void SetOffsets(IReadOnlyList<Track> tracks, float start, float spacing)
    {
        float pos = start;
        foreach (var t in tracks)
        {
            t.Offset = pos;
            pos += t.ActualSize + spacing;
        }
    }

    private static float SpanSize(IReadOnlyList<Track> tracks, int start, int count, float spacing)
    {
        float sum = spacing * (count - 1);
        for (int i = start; i < start + count; i++) { sum += tracks[i].ActualSize; }
        return sum;
    }

    // ── Helpers ──

    private static bool SpansStar(IReadOnlyList<Track> tracks, int index, int span)
    {
        var (start, count) = Range(index, span, tracks.Count);
        for (int i = start; i < start + count; i++)
        {
            if (tracks[i].Effective == GridUnit.Star) { return true; }
        }
        return false;
    }

    /// <summary>Clamp a cell's index and span into the defined tracks. As in WPF, out-of-range indices land in the last track.</summary>
    private static (int start, int count) Range(int index, int span, int trackCount)
    {
        int start = Math.Clamp(index, 0, trackCount - 1);
        int count = Math.Clamp(span, 1, trackCount - start);
        return (start, count);
    }


    // parse a string of track definitions into a list of Track objects
    // a definition string might look like: "*,2*,Auto,100"
    // a dingle track definition might be "*", "2*", "Auto", or "100"
    public static IReadOnlyList<Track> ParseDefinition(string definition)
    {
        var list = new List<Track>();
        var tokenizer = new StringTokenizer(definition, [' ']);
        foreach (var def in tokenizer)
        {
            if (def == "") { continue; }
            switch (def[0])
            {
                case '*':

                    list.Add(new Track(GridLength.Star()));

                    break;
                case 'A' when def.StartsWith("Auto", StringComparison.CurrentCultureIgnoreCase):
                    list.Add(new Track(GridLength.Auto));
                    break;
                case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                    if (def.EndsWith("*", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var val = int.Parse(def.AsSpan()[..^1]);
                        list.Add(new Track(GridLength.Star(val)));
                    }
                    else
                    {
                        var val = int.Parse(def);
                        list.Add(new Track(GridLength.Pixels(val)));
                    }
                    break;
                default:
                    throw new FormatException($"Invalid track definition: {def}");
            }
        }
        return list.AsReadOnly();
    }
}

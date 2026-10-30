using Void.Engine.Graphics;
using Void.Engine.Systems;

namespace VoidGame.UI.UIElements;

public sealed class GridPlacement
{
    public UIElement Child { get; init; }
    public int Row { get; init; }
    public int Column { get; init; }
    public int RowSpan { get; init; }
    public int ColumnSpan { get; init; }
}

public class Grid : UIElement<Grid>
{
    private readonly GridPlacement[,] _cells;
    private readonly List<GridPlacement> _placements = [];
    private readonly Dictionary<UIElement, GridPlacement> _byChild = [];

    public override bool Hoverable { get; set; } = false;

    public Grid(int rows, int columns)
    {
        if (rows < 1)
            throw new ArgumentOutOfRangeException(nameof(rows));
        if (columns < 1)
            throw new ArgumentOutOfRangeException(nameof(columns));

        Rows = rows;
        Columns = columns;
        _cells = new GridPlacement[rows, columns];
        Size = new Vect2(1f, 1f);
    }

    public int Rows { get; }
    public int Columns { get; }

    /// <summary>Space between cells in pixels.</summary>
    public float Gap { get; set; }

    /// <summary>Round cell edges to whole pixels. Keeps pixel art crisp.</summary>
    public bool SnapToPixels { get; set; } = true;

    public IReadOnlyList<GridPlacement> Placements => _placements;

    public Vect4 Padding { get; set; }

    public Grid SetPadding(float padding)
    {
        Padding = new Vect4(padding, padding, padding, padding);
        return this;
    }

    public Grid SetPadding(float x, float y, float z, float w)
    {
        Padding = new Vect4(x, y, z, w);
        return this;
    }

    public Grid SetGap(float gap)
    {
        Gap = gap;
        return this;
    }

    public Grid SetSnapToPixels(bool snap = true)
    {
        SnapToPixels = snap;
        return this;
    }

    // ---- Adding / removing ----

    /// <summary>Place a child at an exact cell. Throws if out of range or overlapping.</summary>
    public Grid Add(UIElement child, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        if (!TryAdd(child, row, column, rowSpan, columnSpan))
            throw new ArgumentException(
                $"Cannot place child at ({row},{column}) span {rowSpan}x{columnSpan}: out of range, occupied, or already added."
            );
        return this;
    }

    /// <summary>Place a child in the first free spot (row by row, left to right).</summary>
    public Grid Add(UIElement child, int rowSpan = 1, int columnSpan = 1)
    {
        if (!TryAddFirstFree(child, rowSpan, columnSpan))
            throw new InvalidOperationException("No free space in the grid for this child.");
        return this;
    }

    /// <summary>Same as Add(child), so the generic AddChild keeps working and fills the next free cell.</summary>
    public new Grid AddChild(UIElement child) => Add(child);

    public bool TryAdd(UIElement child, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        if (child is null || _byChild.ContainsKey(child))
            return false;
        if (!IsFree(row, column, rowSpan, columnSpan))
            return false;

        var placement = new GridPlacement
        {
            Child = child,
            Row = row,
            Column = column,
            RowSpan = rowSpan,
            ColumnSpan = columnSpan,
        };

        for (int r = row; r < row + rowSpan; r++)
        for (int c = column; c < column + columnSpan; c++)
            _cells[r, c] = placement;

        _placements.Add(placement);
        _byChild[child] = placement;
        Children.Add(child);

        // Children size themselves as a fraction of their cell. Zero would make them invisible.
        if (child.Size == Vect2.Zero)
            child.Size = new Vect2(1f, 1f);

        return true;
    }

    public bool TryAddFirstFree(UIElement child, int rowSpan = 1, int columnSpan = 1)
    {
        for (int r = 0; r + rowSpan <= Rows; r++)
        for (int c = 0; c + columnSpan <= Columns; c++)
        {
            if (IsFree(r, c, rowSpan, columnSpan))
                return TryAdd(child, r, c, rowSpan, columnSpan);
        }
        return false;
    }

    public bool Remove(UIElement child)
    {
        if (!_byChild.Remove(child, out var p))
            return false;

        for (int r = p.Row; r < p.Row + p.RowSpan; r++)
        for (int c = p.Column; c < p.Column + p.ColumnSpan; c++)
            _cells[r, c] = null;

        _placements.Remove(p);
        Children.Remove(child);
        return true;
    }

    // ---- Queries ----

    public bool InRange(int row, int column) =>
        row >= 0 && row < Rows && column >= 0 && column < Columns;

    /// <summary>The child covering this cell (any cell of a span), or null.</summary>
    public UIElement GetAt(int row, int column) =>
        InRange(row, column) ? _cells[row, column]?.Child : null;

    public GridPlacement GetPlacement(UIElement child) => _byChild.GetValueOrDefault(child);

    public bool IsFree(int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        if (rowSpan < 1 || columnSpan < 1)
            return false;
        if (row < 0 || column < 0)
            return false;
        if (row + rowSpan > Rows || column + columnSpan > Columns)
            return false;

        for (int r = row; r < row + rowSpan; r++)
        for (int c = column; c < column + columnSpan; c++)
        {
            if (_cells[r, c] is not null)
                return false;
        }
        return true;
    }

    // ---- Layout ----

    public override void UpdateLayout(Rect2 parentContent)
    {
        Bounds = new Rect2(
            parentContent.X + Position.X * parentContent.Width,
            parentContent.Y + Position.Y * parentContent.Height,
            Size.X * parentContent.Width,
            Size.Y * parentContent.Height
        );
        Content = new Rect2(
            Bounds.X + Padding.X,
            Bounds.Y + Padding.Y,
            Bounds.Width - Padding.X - Padding.Z,
            Bounds.Height - Padding.Y - Padding.W
        );

        foreach (var p in _placements)
            p.Child.UpdateLayout(CellRect(p));
    }

    /// <summary>Pixel rect of a placement, including the gaps it spans.</summary>
    public Rect2 CellRect(GridPlacement p)
    {
        float left = Edge(p.Column, Content.Width, Columns);
        float right = Edge(p.Column + p.ColumnSpan, Content.Width, Columns) - Gap;
        float top = Edge(p.Row, Content.Height, Rows);
        float bottom = Edge(p.Row + p.RowSpan, Content.Height, Rows) - Gap;

        return new Rect2(
            Content.X + left,
            Content.Y + top,
            MathF.Max(0, right - left),
            MathF.Max(0, bottom - top)
        );
    }

    // Start of line `index` along an axis. Each cell + its gap is one equal step.
    private float Edge(int index, float total, int count)
    {
        float edge = index * (total + Gap) / count;
        return SnapToPixels ? MathF.Round(edge) : edge;
    }

    public override void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        foreach (var p in _placements)
            p.Child.Draw(batcher, frameTime);
    }
}

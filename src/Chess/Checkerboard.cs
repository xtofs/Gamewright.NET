using Gamewright.Layout;

namespace Chess;

record class Checkerboard : Node
{

    public Label[] Right { get; }
    public Label[] Left { get; }
    public Label[] Top { get; }
    public Label[] Bottom { get; }
    public IReadOnlyList<Label> Labels => [.. Left, .. Right, .. Top, .. Bottom];

    public Square[] Squares { get; }

    public Checkerboard()
    {
        Right = [.. from row in Enumerable.Range(0, 8) select new Label(true, row, $"{8 - row}")];
        Left = [.. from row in Enumerable.Range(0, 8) select new Label(true, 8 - 1 - row, $"{8 - row}")];
        Bottom = [.. from col in Enumerable.Range(0, 8) select new Label(false, col, BijectiveBase26.Encode(col + 1))];
        Top = [.. from col in Enumerable.Range(0, 8) select new Label(false, 8 - 1 - col, BijectiveBase26.Encode(col + 1))];
        Squares = [.. from row in Enumerable.Range(0, 8) from col in Enumerable.Range(0, 8) select new Square(col, row)];
    }

    public override IReadOnlyList<Node> Children => [.. Left, .. Right, .. Top, .. Bottom, .. Squares];

    protected override Size MeasureOverride(Size availableSize)
    {
        // the board is square, so we take the minimum of the available width and height
        // no need to consult the child nodes, as the board size is determined solely by the available size
        var sq = MathF.Min(availableSize.W, availableSize.H);
        return new Size(sq, sq);
    }

    protected override void ArrangeOverride(Rect bounds)
    {
        // the label border has a quarter of the width of a square
        // so we need 8 times 4 units plus 2 units for the borders on left and right
        var borderSize = bounds.W / (2 + 8 * 4);
        var squareSize = (bounds.W - 2 * borderSize) / 8;
        for (int row = 0; row < 8; row++)
        {
            var y = bounds.Y + borderSize + row * squareSize;
            Left[row].Arrange(new Rect(bounds.X, y, borderSize, squareSize));
            Right[row].Arrange(new Rect(bounds.MaxX - borderSize, y, borderSize, squareSize));
        }
        for (int col = 0; col < 8; col++)
        {
            var x = bounds.X + borderSize + col * squareSize;
            Top[col].Arrange(new Rect(x, bounds.Y, squareSize, borderSize));
            Bottom[col].Arrange(new Rect(x, bounds.MaxY - borderSize, squareSize, borderSize));
        }
        for (int row = 0; row < 8; row++)
        {
            var y = bounds.Y + borderSize + row * squareSize;
            for (int col = 0; col < 8; col++)
            {
                var x = bounds.X + borderSize + col * squareSize;
                var square = Squares[col + row * 8];
                square.Arrange(new Rect(x, y, squareSize, squareSize));
            }
        }
    }


    public record class Label(bool IsVertical, int Index, string Text) : Node
    {
        public override IReadOnlyList<Node> Children => [];

        public int Parity { get; } = Index % 2;

        protected override void ArrangeOverride(Rect bounds) { }

        protected override Size MeasureOverride(Size availableSize) => availableSize;
    }

    public record class Square(int Col, int Row) : Node
    {
        public override IReadOnlyList<Node> Children => [];

        public int Parity => (Col + Row) % 2;

        public string Label => $"{(char)('a' + Col)}{8 - Row}";

        protected override void ArrangeOverride(Rect bounds) { }

        protected override Size MeasureOverride(Size availableSize) => availableSize;
    }
}

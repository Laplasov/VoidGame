using Void.Engine.Systems;

namespace VoidGame.UI.UIElements;

public class ScrollBar : Box
{
    public Box Thumb { get; set; } = new Box();
    public float VisibleRatio { get; set; } = 0.2f;

    // State passed in from ScrollView
    private float _maxScroll;
    private float _currentOffset;

    public ScrollBar()
    {
        AddChild(Thumb);
        Fill = Color.Black;
    }

    public void SetScrollState(float maxScroll, float currentOffset)
    {
        _maxScroll = maxScroll;
        _currentOffset = currentOffset;
    }

    public override void UpdateLayout(Rect2 parentContent)
    {
        base.UpdateLayout(parentContent);

        float trackHeight = Bounds.Height;
        float thumbHeight = trackHeight * VisibleRatio;

        if (thumbHeight > trackHeight)
            thumbHeight = trackHeight;
        if (thumbHeight < 10f)
            thumbHeight = 10f;

        float maxTravel = trackHeight - thumbHeight;

        // Calculate progress on the fly from the real state!
        float progress = _maxScroll > 0 ? _currentOffset / _maxScroll : 0f;
        float thumbY = Bounds.Y + (maxTravel * progress);

        Thumb.SetBounds(new Rect2(Bounds.X, thumbY, Bounds.Width, thumbHeight));
        Thumb.SetContent(Thumb.Bounds);
    }
}

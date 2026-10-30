using System.Diagnostics;
using Void.Engine.Assets.Loaders;
using Void.Engine.Graphics;
using Void.Engine.Inputs.Mouses;
using Void.Engine.Systems;

namespace VoidGame.UI.UIElements;

public abstract class UIElement
{
    public UIElement Parent { get; private set; }

    private readonly ClipStack _ownClips = new();
    protected ClipStack Clips => Parent?.Clips ?? _ownClips;

    public virtual float MeasureHeight(float parentWidth) => -1f;

    public Vect2 Position { get; set; } = Vect2.Zero;
    public Vect2 Size { get; set; } = Vect2.Zero;
    public Rect2 Content { get; protected set; }
    public Rect2 Bounds { get; protected set; }

    public virtual bool Hoverable { get; set; } = true;

    protected List<UIElement> Children = new List<UIElement>();
    protected static Texture WhitePixel = new Texture(new Vect2(1, 1), Color.White);

    public void AddChild(UIElement child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public abstract void UpdateLayout(Rect2 parentContent);
    public abstract void Draw(SpriteBatcher batcher, FrameTime frameTime);

    /// <summary>
    /// Finds the topmost element under the mouse.
    /// </summary>
    public virtual UIElement MouseUpdate(Vect2 mousePosition)
    {
        bool isInside =
            mousePosition.X >= Bounds.X
            && mousePosition.X <= Bounds.X + Bounds.Width
            && mousePosition.Y >= Bounds.Y
            && mousePosition.Y <= Bounds.Y + Bounds.Height;

        if (!isInside)
            return null;

        // Check children in REVERSE order (last added = drawn on top).
        // The first child that returns a hit is the topmost visual element.
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            var hit = Children[i].MouseUpdate(mousePosition);
            if (hit != null)
                return hit; // A child was hit! Return it immediately.
        }

        return isInside && Hoverable ? this : null;
    }

    protected Vect2 CheckPercentage(Vect2 percentage)
    {
        if (percentage.X > 1.0f)
            percentage.X = 1.0f;
        if (percentage.Y > 1.0f)
            percentage.Y = 1.0f;
        if (percentage.X < 0.0f)
            percentage.X = 0.0f;
        if (percentage.Y < 0.0f)
            percentage.Y = 0.0f;
        return percentage;
    }
}

public abstract class UIElement<T> : UIElement
    where T : UIElement<T>
{
    public T SetPosition(Vect2 position)
    {
        Position = position; //CheckPercentage(position);
        return (T)this;
    }

    public T SetSize(Vect2 percentage)
    {
        Size = CheckPercentage(percentage);
        return (T)this;
    }

    public new T AddChild(UIElement child)
    {
        base.AddChild(child);
        return (T)this;
    }

    public T SetHoverable(bool hoverable = true)
    {
        Hoverable = hoverable;
        return (T)this;
    }
}

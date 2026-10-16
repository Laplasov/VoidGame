using Void.Engine.Assets.Loaders;
using Void.Engine.Graphics;
using Void.Engine.Systems;

namespace VoidGame.UI.UIElements;

public abstract class UIElement
{
    public Vect2 Position { get; set; } = Vect2.Zero;
    public Vect2 Size { get; set; } = Vect2.Zero;
    public Rect2 Content { get; protected set; }
    public Rect2 Bounds { get; protected set; }

    protected List<UIElement> Children = new List<UIElement>();
    protected static Texture WhitePixel = new Texture(new Vect2(1, 1), Color.White);

    public void AddChild(UIElement child) => Children.Add(child);

    public abstract void UpdateLayout(Rect2 parentContent);
    public abstract void Draw(SpriteBatcher batcher, FrameTime frameTime);

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
    public T SetPosition(Vect2 percentage)
    {
        Position = CheckPercentage(percentage);
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
}

using Void.Engine;
using Void.Engine.Inputs.Mouses;
using Void.Engine.Systems;

namespace VoidGame.UI;

public readonly struct TargetPack
{
    public MouseState State { get; }

    public TargetPack(MouseState state)
    {
        State = state;
    }

    // Raw window position
    public Vect2 WindowPosition => State.Position;

    // Is the left mouse button currently down?
    public bool IsMouseDown => State.IsButtonPressed(MouseButton.Left);
    public bool IsRightMouseDown => State.IsButtonPressed(MouseButton.Right);

    // Lazily calculated scaled position (evaluated only when you access it)
    public Vect2 ScaledPosition
    {
        get
        {
            var viewport = GameSettings.Instance.Viewport;
            var window = GameSettings.Instance.Window;
            return new Vect2(
                State.Position.X * viewport.X / window.X,
                State.Position.Y * viewport.Y / window.Y
            );
        }
    }

    public bool IsInside(Rect2 bounds)
    {
        return ScaledPosition.X >= bounds.X
            && ScaledPosition.X <= bounds.X + bounds.Width
            && ScaledPosition.Y >= bounds.Y
            && ScaledPosition.Y <= bounds.Y + bounds.Height;
    }
}

using Void.Engine.Graphics;
using Void.Engine.Systems;

namespace VoidGame.UI.UIElements;

public sealed class ClipStack
{
    private readonly Stack<Rect2> _rects = new();

    public void Push(SpriteBatcher batcher, Rect2 rect)
    {
        batcher.Flush();
        if (_rects.Count > 0)
            rect = Rect2.Intersection(_rects.Peek(), rect);
        _rects.Push(rect);
        batcher.SetScissor(rect);
    }

    public void Pop(SpriteBatcher batcher)
    {
        batcher.Flush();
        _rects.Pop();
        if (_rects.Count > 0)
            batcher.SetScissor(_rects.Peek());
        else
            batcher.ClearScissor();
    }
}

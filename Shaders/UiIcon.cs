using Void.Engine.Assets.Loaders;
using Void.Engine.Systems;

namespace VoidGame.Shaders;

public enum ScaleMode
{
    Stretch,
    Fit,
    Cover,
}

public class UiIcon
{
    public ScaleMode ScaleMode { get; set; } = ScaleMode.Stretch;
    private static Texture _whitePixel = new Texture(new Vect2(1, 1), Color.White);
    public Texture Image { get; set; }
    public Rect2? ImageSource { get; set; }
    public Texture GetTexture => Image ?? _whitePixel;
    public Rect2 GetSource
    {
        get
        {
            var sheet = ImageSource ?? GetTexture.Bounds;
            var frame = _initFrames ? StartFrame : _currentFrame; // before the first SetNextFrame

            float w = sheet.Width / FrameGrid.X;
            float h = sheet.Height / FrameGrid.Y;

            return new Rect2(sheet.X + w * (frame.X - 1), sheet.Y + h * (frame.Y - 1), w, h);
        }
    }

    public Vect4 GetImageVect4
    {
        get
        {
            var tex = GetTexture.Bounds; // whole texture, in pixels
            var src = GetSource; // pixel rect, includes the current frame

            return new Vect4(
                src.X / tex.Width,
                src.Y / tex.Height,
                src.Width / tex.Width,
                src.Height / tex.Height
            );
        }
    }

    public Vect2 StartFrame { get; set; } = new Vect2(1, 1);
    public Vect2 EndFrame { get; set; } = new Vect2(1, 1);
    public Vect2 FrameGrid { get; set; } = new Vect2(1, 1);

    public float TimePerFrame { get; set; } = 0.1f;

    public void SetDuration(float totalSeconds) => TimePerFrame = totalSeconds / FrameCount;

    private float _elapsed = 0f;

    public void Update(FrameTime frameTime)
    {
        _elapsed += frameTime.DeltaTime;

        if (_elapsed >= TimePerFrame)
        {
            _elapsed -= TimePerFrame;
            SetNextFrame();
        }
    }

    private Vect2 _currentFrame;
    private bool _initFrames = true;

    private int ToIndex(Vect2 frame) => ((int)frame.Y - 1) * (int)FrameGrid.X + ((int)frame.X - 1);

    public int FrameCount
    {
        get
        {
            int total = Math.Max(1, (int)(FrameGrid.X * FrameGrid.Y));
            int start = ToIndex(StartFrame);
            int end = ToIndex(EndFrame);

            // distance from start to end, wrapping around the grid, +1 because End is shown too
            return ((end - start) % total + total) % total + 1;
        }
    }

    private void SetNextFrame()
    {
        if (_initFrames)
        {
            _currentFrame = StartFrame;
            _initFrames = !_initFrames;
            return;
        }
        if (_currentFrame == EndFrame)
        {
            _currentFrame = StartFrame;
            return;
        }
        if (_currentFrame.X >= FrameGrid.X)
        {
            _currentFrame.X = 1;
            _currentFrame.Y++;
        }
        else
        {
            _currentFrame.X++;
        }
        if (_currentFrame.Y > FrameGrid.Y)
            _currentFrame = new Vect2(1, 1);
    }
}

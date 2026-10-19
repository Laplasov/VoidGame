using Void.Engine.Graphics.Shaders;
using Void.Engine.Systems;

namespace VoidGame.Shaders;

// A layer definition: create once, reuse. '@' in the GLSL becomes the layer's index in the stack.
// The function must be named layer@ and have this signature.
public sealed class FillLayer(string name, string glsl)
{
    public string Name { get; } = name; // part of the program cache key
    public string Glsl { get; } = glsl;
}

// One use of a layer, with its uniform values. The Action receives the layer's index as a string.
public abstract class FillStep
{
    // Must identify the GLSL uniquely: same Name = same shader code (it is the cache key)
    public virtual string Name => GetType().Name;

    // '@' becomes the step's index. Must define: vec4 layer@(vec2 uv, vec2 pos, vec4 prev)
    public abstract string Glsl { get; }

    // Sends the CURRENT values to the shader. 'id' matches the '@' above.
    public abstract void Apply(IShader shader, string id);
}

public class UiFill
{
    private readonly List<FillStep> _steps = [];
    private string _key;

    public static implicit operator UiFill(Color color) => new(color);

    public static implicit operator Color(UiFill fill) => fill.Color;

    public UiFill(Color color) => Color = color;

    public Color Color { get; set; }
    public Vect2 Offset { get; set; }
    public float Rotation { get; set; }
    public Vect4 RoundedCorners { get; set; } = default;

    public IReadOnlyList<FillStep> Steps => _steps;
    public bool IsPlain => _steps.Count == 0;
    internal string Key => _key ??= string.Join('|', _steps.Select(s => s.Name));

    public UiFill Add(FillStep step)
    {
        _steps.Add(step);
        _key = null;
        return this;
    }

    public bool Remove(FillStep step)
    {
        if (!_steps.Remove(step))
            return false;
        _key = null;
        return true;
    }

    public bool TryGet<T>(out T step, int n = 0)
        where T : FillStep
    {
        step = _steps.OfType<T>().Skip(n).FirstOrDefault();
        return step is not null;
    }

    public UiFill SetOffset(Vect2 offset)
    {
        Offset = offset;
        return this;
    }

    public UiFill SetRotation(float degrees)
    {
        Rotation = degrees;
        return this;
    }

    public UiFill SetRoundedCorners(Vect4 corners)
    {
        RoundedCorners = corners;
        return this;
    }
}

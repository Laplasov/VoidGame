using Void.Engine.Graphics.Shaders;
using Void.Engine.Systems;

namespace VoidGame.Shaders;

public static class UiFills
{
    public static UiFill ToFill(this Color color) => color;

    public static UiFill VerticalGradient(this UiFill fill, Color to) =>
        fill.Add(new GradientStep(to, new Vect2(0, 1)));

    public static UiFill HorizontalGradient(this UiFill fill, Color to) =>
        fill.Add(new GradientStep(to, new Vect2(1, 0)));

    public static UiFill VerticalGradient(this Color from, Color to) =>
        ((UiFill)from).VerticalGradient(to);

    public static UiFill HorizontalGradient(this Color from, Color to) =>
        ((UiFill)from).HorizontalGradient(to);
}

public sealed class GradientStep(Color to, Vect2 direction) : FillStep
{
    public Color To { get; set; } = to;
    public Vect2 Direction { get; set; } = direction; // (0,1) top to bottom, (1,0) left to right

    public override string Glsl =>
        """
            uniform vec4 uGradTo@;
            uniform vec2 uGradDir@;
            vec4 layer@(vec2 uv, vec2 pos, vec4 prev) {
                return mix(prev, uGradTo@, clamp(dot(uv, uGradDir@), 0.0, 1.0));
            }
            """;

    public override void Apply(IShader shader, string id)
    {
        shader.SetUniform("uGradTo" + id, To);
        shader.SetUniform("uGradDir" + id, Direction);
    }
}

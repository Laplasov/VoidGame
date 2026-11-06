using System.Text;
using Void.Engine.Systems;

namespace VoidGame.Shaders;

public static class FillPrograms
{
    private static readonly Dictionary<string, CutoutShaderWrapper> Cache = [];

    public static CutoutShaderWrapper For(UiFill fill)
    {
        if (fill.IsPlain)
            return UiShaders.Cutout; // identity only

        if (!Cache.TryGetValue(fill.Key, out var program))
            Cache[fill.Key] = program = new CutoutShaderWrapper(Build(fill.Steps));

        return program;
    }

    public static void Apply(UiFill fill, CutoutShaderWrapper shader)
    {
        var src = fill.Image.GetSource;

        shader.SetUniform("uColor", fill.Color);
        shader.SetUniform("uFillOffset", fill.Offset);
        shader.SetUniform("uFillRotation", fill.Rotation * (MathF.PI / 180f));
        shader.SetUniform("uImageSource", fill.Image.GetImageVect4);
        shader.SetUniform("uScaleMode", (int)fill.Image.ScaleMode);
        shader.SetUniform("uTextureSize", new Vect2(src.Width, src.Height));

        if (fill.IsPlain)
            return;

        for (int i = 0; i < fill.Steps.Count; i++)
            fill.Steps[i].Apply(shader, i.ToString());
    }

    private static string Build(IReadOnlyList<FillStep> steps)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < steps.Count; i++)
            sb.AppendLine(steps[i].Glsl.Replace("@", i.ToString()));

        sb.AppendLine("vec4 fill(vec2 uv, vec2 pos, vec4 baseColor) {");
        sb.AppendLine("    vec4 c = baseColor;");
        for (int i = 0; i < steps.Count; i++)
            sb.AppendLine($"    c = layer{i}(uv, pos, c);");
        sb.AppendLine("    return c;");
        sb.AppendLine("}");

        return sb.ToString();
    }
}

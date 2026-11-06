using Void.Engine.Cameras;
using Void.Engine.Graphics.Shaders;
using Void.Engine.Systems;

namespace VoidGame.Shaders;

public static class UiShaders
{
    public static readonly CutoutShaderWrapper Cutout = new CutoutShaderWrapper();
}

public class CutoutShaderWrapper : IShader
{
    public ShaderProgram Program { get; }

    public CutoutShaderWrapper(string fillGlsl = IdentityFill)
    {
        string fragment = FragmentHeader + fillGlsl + FragmentMain;
        Program = new ShaderProgram(Vertex, fragment);
    }

    public const string IdentityFill =
        "vec4 fill(vec2 uv, vec2 pos, vec4 baseColor) { return baseColor; }";

    private const string Vertex =
        @"
        #version 330 core
        layout(location = 0) in vec2 aPosition;
        layout(location = 1) in vec2 aTexCoord;
        layout(location = 2) in vec4 aColor;
        uniform mat4 uViewProjection;
        out vec2 vTexCoord;
        out vec4 vColor;
        out vec2 vLocalPos;
        void main() {
            vTexCoord = aTexCoord;
            vColor = aColor;
            vLocalPos = aPosition;
            gl_Position = uViewProjection * vec4(aPosition, 0.0, 1.0);
        }";

    private const string FragmentHeader =
        @"
        #version 330 core
        in vec2 vTexCoord;
        in vec4 vColor;
        in vec2 vLocalPos;

        uniform int uUseTexture;
        uniform sampler2D uTexture;
        uniform vec2 uTextureSize;

        uniform vec4 uCutRect;   // x, y, w, h in UI pixels
        uniform vec4 uRect;      // the whole layer rect, for uv
        uniform vec4 uColor;
        uniform vec2 uFillOffset;     // in layer sizes (0.5 = half the layer)
        uniform float uFillRotation;
        uniform vec4 uCornerRadii;
        uniform vec4 uCutCornerRadii;
        uniform vec4 uImageSource;
        uniform int uScaleMode;

        out vec4 FragColor;
        ";

    private const string FragmentMain =
        @"
    float roundedRectSdf(vec2 p, vec4 rect, vec4 radii) {
        vec2 halfSize = rect.zw * 0.5;
        vec2 q0 = p - (rect.xy + halfSize);

        vec4 r4 = min(radii, vec4(min(halfSize.x, halfSize.y)));
        float r = q0.x < 0.0
            ? (q0.y < 0.0 ? r4.x : r4.w)
            : (q0.y < 0.0 ? r4.y : r4.z);

        vec2 q = abs(q0) - halfSize + r;
        return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
    }

    void main() {
        float px = max(fwidth(vLocalPos.x), 1e-4);

        float outerSdf = roundedRectSdf(vLocalPos, uRect, uCornerRadii);
        float coverage = clamp(0.5 - outerSdf / px, 0.0, 1.0);

        if (uCutRect.z > 0.0 && uCutRect.w > 0.0) {
            float cutSdf = roundedRectSdf(vLocalPos, uCutRect, uCutCornerRadii);
            coverage *= clamp(cutSdf / px + 1.0, 0.0, 1.0);
        }

        if (coverage <= 0.0) discard;

        // 1. Calculate base UV (0.0 to 1.0 across the box)
        vec2 uv = (vLocalPos - uRect.xy) / uRect.zw;

        // 2. SCALE MODE LOGIC (Fit)
        if (uScaleMode == 1) { 
            float rectAspect = uRect.z / uRect.w;
            float texAspect = uTextureSize.x / uTextureSize.y;
            
            if (rectAspect > texAspect) {
                // Box is wider than image. Expand UV X to center the image.
                uv.x = (uv.x - 0.5) * (rectAspect / texAspect) + 0.5;
            } else {
                // Box is taller than image. Expand UV Y to center the image.
                uv.y = (uv.y - 0.5) * (texAspect / rectAspect) + 0.5;
            }
            
            // Discard pixels outside the 0-1 range so the background doesn't repeat
            if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
                discard;
            }
        }

        // 3. Apply Image Source (if any)
        if (uImageSource.z > 0.0) {
            uv = uImageSource.xy + (uv * uImageSource.zw);
        }

        // 4. Apply Rotation and Offset
        float aspect = uRect.z / uRect.w;
        vec2 p = (uv - 0.5) * vec2(aspect, 1.0);
        float c = cos(uFillRotation);
        float s = sin(uFillRotation);
        p = vec2(c * p.x + s * p.y, -s * p.x + c * p.y);
        uv = p / vec2(aspect, 1.0) + 0.5 - uFillOffset;

        // 5. Sample Texture
        vec4 baseColor = uColor;
        if (uUseTexture == 1)
            baseColor *= texture(uTexture, uv);

        vec4 col = fill(uv, vLocalPos, baseColor);
        FragColor = vec4(col.rgb, col.a * coverage);
    }";

    public bool IsValid => Program.IsValid;

    public void SetUniform(string name, float value) => Program.SetUniform(name, value);

    public void SetUniform(string name, int value) => Program.SetUniform(name, value);

    public void SetUniform(string name, Vect2 value) => Program.SetUniform(name, value);

    public void SetUniform(string name, Vect3 value) => Program.SetUniform(name, value);

    public void SetUniform(string name, Vect4 value) => Program.SetUniform(name, value);

    public void SetUniform(string name, Color color) => Program.SetUniform(name, color);

    public void SetUniform(string name, Matrix matrix) => Program.SetUniform(name, matrix);

    public void Bind() => Program.Bind();

    public void Unbind() => Program.Unbind();
}

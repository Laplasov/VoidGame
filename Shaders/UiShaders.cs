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

        out vec4 FragColor;
        ";

    private const string FragmentMain =
        @"
        void main() {
            bool insideX = vLocalPos.x > uCutRect.x && vLocalPos.x < (uCutRect.x + uCutRect.z);
            bool insideY = vLocalPos.y > uCutRect.y && vLocalPos.y < (uCutRect.y + uCutRect.w);
            if (insideX && insideY) discard;

            vec2 relPos = vLocalPos - uRect.xy;
            vec2 size = uRect.zw;
            
            float rTL = uCornerRadii.x;
            float rTR = uCornerRadii.y;
            float rBR = uCornerRadii.z;
            float rBL = uCornerRadii.w;
            
            if (relPos.x < rTL && relPos.y < rTL) {
                if (distance(relPos, vec2(rTL, rTL)) > rTL) discard;
            } 
            else if (relPos.x > size.x - rTR && relPos.y < rTR) {
                if (distance(relPos, vec2(size.x - rTR, rTR)) > rTR) discard;
            } 
            else if (relPos.x > size.x - rBR && relPos.y > size.y - rBR) {
                if (distance(relPos, vec2(size.x - rBR, size.y - rBR)) > rBR) discard;
            } 
            else if (relPos.x < rBL && relPos.y > size.y - rBL) {
                if (distance(relPos, vec2(rBL, size.y - rBL)) > rBL) discard;
            }

            vec2 uv = (vLocalPos - uRect.xy) / uRect.zw;

            float aspect = uRect.z / uRect.w;
            vec2 p = (uv - 0.5) * vec2(aspect, 1.0);
            float c = cos(uFillRotation);
            float s = sin(uFillRotation);
            p = vec2(c * p.x + s * p.y, -s * p.x + c * p.y);
            uv = p / vec2(aspect, 1.0) + 0.5 - uFillOffset;

            vec4 baseColor = uColor;
            if (uUseTexture == 1)
                baseColor *= texture(uTexture, vTexCoord);

            FragColor = fill(uv, vLocalPos, baseColor);
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

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

    public CutoutShaderWrapper()
    {
        string vertex =
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

        string fragment =
            @"
#version 330 core
in vec2 vTexCoord;
in vec4 vColor;
in vec2 vLocalPos;

// Required by Void Engine's SpriteBatcher pipeline
uniform int uUseTexture;
uniform sampler2D uTexture;
uniform vec2 uTextureSize;

// x, y, width, height in UI pixels
uniform vec4 uCutRect;
uniform vec4 uColor;

out vec4 FragColor;

void main() {
    bool insideX = vLocalPos.x > uCutRect.x && vLocalPos.x < (uCutRect.x + uCutRect.z);
    bool insideY = vLocalPos.y > uCutRect.y && vLocalPos.y < (uCutRect.y + uCutRect.w);
    
    if (insideX && insideY) {
        discard;
    } else {
        // Start with the base color passed from C# (vColor)
        vec4 finalColor = uColor;
        
        // If a texture is provided, multiply it with the color (standard sprite batch behavior)
        if (uUseTexture == 1) {
            finalColor *= texture(uTexture, vTexCoord);
        }
        
        FragColor = finalColor;
    }
}";

        Program = new ShaderProgram(vertex, fragment);
    }

    public bool IsValid => Program.IsValid;

    public void SetUniform(string name, float value) => Program.SetUniform(name, value);

    public void SetUniform(string name, int value) => Program.SetUniform(name, value);

    public void SetUniform(string name, Vect2 value) => Program.SetUniform(name, value);

    public void SetUniform(string name, Vect3 value) => Program.SetUniform(name, value);

    public void SetUniform(string name, Vect4 value) => Program.SetUniform(name, value);

    public void SetUniform(string name, Color color) => Program.SetUniform(name, color);

    public void SetUniform(string name, Matrix matrix) => Program.SetUniform(name, matrix);

    public void Bind() { }

    public void Unbind() { }
}

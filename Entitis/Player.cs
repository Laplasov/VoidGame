using Void.Engine.Assets;
using Void.Engine.Assets.Loaders;
using Void.Engine.Assets.Loaders.LDtk;
using Void.Engine.Assets.Loaders.LDtk.Instances;
using Void.Engine.Inputs.Keyboards;
using Void.Engine.Systems;
using VoidGame.managers;

namespace VoidGame.Entitis;

public sealed class Player(LDtkEntityInstance source, LevelManager levelManager)
    : MapEntity(source: source, levelManager: levelManager)
{
    private const float MoveSpeed = 100f;
    private Texture _texture;
    private Rect2 _sourceRect;

    public override void OnUpdate(FrameTime frameTime)
    {
        if (
            _texture == null
            && LDtkSetting.TryGetTileSetting(Source.Settings, SpriteFieldName, out var tile)
        )
        {
            AssetManager.Instance.TryLoadTilesetTexture(
                LevelManagerInstance.Map,
                (uint)tile.TilesetId,
                out _texture
            );
            _sourceRect = tile.Source;
        }

        var state = Keyboard.GetState();
        var movement = Vect2.Zero;

        if (state.IsKeyDown(KeyboardKey.S))
            movement.Y += 1f;
        if (state.IsKeyDown(KeyboardKey.W))
            movement.Y -= 1f;
        if (state.IsKeyDown(KeyboardKey.D))
            movement.X += 1f;
        if (state.IsKeyDown(KeyboardKey.A))
            movement.X -= 1f;
        movement = movement.Normalized();

        var delta = movement * MoveSpeed * frameTime.DeltaTime;
        var pos = Position;

        var tryX = new Vect2(pos.X + delta.X, pos.Y);
        if (!LevelManagerInstance.IsBlocked(new Rect2(tryX, Size)))
            pos = tryX;

        var tryY = new Vect2(pos.X, pos.Y + delta.Y);
        if (!LevelManagerInstance.IsBlocked(new Rect2(tryY, Size)))
            pos = tryY;

        Position = pos;
    }
}

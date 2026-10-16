using Void.Engine.Assets;
using Void.Engine.Assets.Loaders;
using Void.Engine.Assets.Loaders.LDtk;
using Void.Engine.Assets.Loaders.LDtk.Instances;
using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.managers;

namespace VoidGame.Entitis;

public abstract class MapEntity(LDtkEntityInstance source, LevelManager levelManager)
{
    private Texture _texture;
    private Rect2 _sourceRect;
    private bool _spriteLoaded;

    public LDtkEntityInstance Source { get; } = source;
    public Vect2 Position { get; set; } = source.Position;
    public Vect2 Size { get; } = source.Size;
    public string Name { get; } = source.Name;
    protected virtual string SpriteFieldName => "Sprite";

    public LevelManager LevelManagerInstance => levelManager;

    public virtual void OnUpdate(FrameTime frameTime) { }

    public virtual void OnDraw(SpriteBatcher batcher, FrameTime frameTime)
    {
        if (!_spriteLoaded)
        {
            _spriteLoaded = true; // try once, don't retry every frame
            if (LDtkSetting.TryGetTileSetting(Source.Settings, SpriteFieldName, out var tile))
            {
                AssetManager.Instance.TryLoadTilesetTexture(
                    LevelManagerInstance.Map,
                    (uint)tile.TilesetId,
                    out _texture
                );
                _sourceRect = tile.Source;
            }
        }

        if (_texture != null)
            batcher.Draw(_texture, Position, _sourceRect, Color.White);
    }
}

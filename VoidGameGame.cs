using Void.Engine;
using Void.Engine.Assets;
using Void.Engine.Assets.Loaders.LDtk;
using Void.Engine.Assets.Loaders.LDtk.Instances;
using Void.Engine.Cameras;
using Void.Engine.Graphics;
using Void.Engine.Inputs.InputActions;
using Void.Engine.Inputs.Keyboards;
using Void.Engine.Systems;
using VoidGame.Entitis;
using VoidGame.helpers;
using VoidGame.managers;
using VoidGame.UI;

namespace VoidGame;

public sealed class VoidGameGame(GameSettings setting) : Game(setting)
{
    private SpriteBatcher _batcher;
    private Camera _camera;
    private CameraMovement _cameraMovement;

    private MapEntity _player;

    private LevelManager _levelManager;
    private TransitionManager _transitionManager;
    private SaveLoadManager _saveLoadManager;

    private View _view;

    protected override void OnEnter()
    {
        _batcher = new SpriteBatcher();
        _levelManager = new LevelManager();
        _camera = new Camera();
        _cameraMovement = new CameraMovement(_camera);

        _view = new View();

        var playerLayer = _levelManager.Level.Layers.FirstOrDefault(l =>
            l.Type == LDtkLayerType.Entities
        );

        var playerEntity = playerLayer
            ?.InstanceAs<LDtkEntityInstance>()
            .FirstOrDefault(e => e.Name == "Player");

        _player = new Player(playerEntity, _levelManager);

        _transitionManager = new TransitionManager(_levelManager, _player);
        _saveLoadManager = new SaveLoadManager(_levelManager);

        _saveLoadManager.LoadNpcsForCurrentLevel();

        InputAction.AddAction("Enter").AddKey(KeyboardKey.Space);
    }

    protected override void OnUpdate(FrameTime frameTime)
    {
        //_cameraMovement.OnUpdate(frameTime);

        _player.OnUpdate(frameTime);

        _camera.Position = _player.Position + _player.Size * 0.5f;

        var actions = InputAction.GetState();

        if (actions.IsJustPressed("Enter"))
        {
            if (_transitionManager.TryEnterDoor())
                _saveLoadManager.LoadNpcsForCurrentLevel();
        }
    }

    protected override void OnDraw(FrameTime frameTime)
    {
        _batcher.Begin(SortMode.Immediate, camera: _camera);

        foreach (var layer in _levelManager.Level.Layers.Reverse())
        {
            if (layer.Type == LDtkLayerType.Entities)
                continue;

            if (!layer.Visible || layer.Type != LDtkLayerType.Tiles)
                continue;

            if (
                !AssetManager.Instance.TryLoadTilesetTexture(
                    _levelManager.Map,
                    layer.TilesetId,
                    out var texture
                )
            )
                continue;

            foreach (var tile in layer.InstanceAs<LDtkTileInstance>())
                _batcher.Draw(texture, tile.Position, tile.Source, Color.White);
        }

        _player.OnDraw(_batcher, frameTime);

        foreach (var npc in _saveLoadManager.Npcs[_levelManager.Level.Id])
        {
            npc.OnDraw(_batcher, frameTime);
        }

        _batcher.End();

        _batcher.Begin(SortMode.Immediate); // No camera = screen space

        _view.OnDraw(_batcher, frameTime);

        _batcher.End();
    }

    protected override void OnExit()
    {
        _batcher?.Dispose();
    }
}

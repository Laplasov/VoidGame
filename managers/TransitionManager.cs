using Void.Engine.Assets.Loaders.LDtk;
using Void.Engine.Assets.Loaders.LDtk.Instances;
using Void.Engine.Systems;
using VoidGame.Entitis;

namespace VoidGame.managers;

public class TransitionManager(LevelManager levelManager, MapEntity player)
{
    private LevelManager _levelManager = levelManager;
    private MapEntity _player = player;

    public bool TryEnterDoor()
    {
        var entityLayer = _levelManager.Level.Layers.FirstOrDefault(l =>
            l.Type == LDtkLayerType.Entities
        );
        if (entityLayer == null)
            return false;

        var playerRect = new Rect2(
            _player.Position.X,
            _player.Position.Y,
            _player.Size.X,
            _player.Size.Y
        );

        foreach (var door in entityLayer.InstanceAs<LDtkEntityInstance>())
        {
            if (door.Name != "Door")
                continue;

            var doorRect = new Rect2(door.Position.X, door.Position.Y, door.Size.X, door.Size.Y);
            if (!playerRect.Intersects(doorRect))
                continue;

            if (!LDtkSetting.TryGetEntityRefSetting(door.Settings, "Entity_ref", out var target))
                continue;

            if (!_levelManager.Map.TryGetLevelById(target.LevelId, out var targetLevel))
                continue;

            _levelManager.SetLevel(targetLevel);

            if (_levelManager.Map.TryGetEntityById(target.EntityId, out var targetDoor))
            {
                var doorTopLeft = targetDoor.Position - targetDoor.Size * targetDoor.Pivot;
                var doorCenter = doorTopLeft + targetDoor.Size * 0.5f;
                _player.Position = doorCenter - _player.Size * 0.5f;
            }

            return true;
        }

        return false;
    }
}

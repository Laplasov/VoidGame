using Void.Engine.Assets;
using Void.Engine.Assets.Loaders.LDtk;
using Void.Engine.Assets.Loaders.LDtk.Instances;
using Void.Engine.Beacons;
using Void.Engine.Systems;
using VoidGame.Beacon;

namespace VoidGame.managers;

public class LevelManager
{
    private const string CollisionLayerName = "Collision";

    private LDtkMap _map;
    private LDtkLevel _level;

    public LDtkMap Map => _map;
    public LDtkLevel Level => _level;

    private readonly HashSet<(int X, int Y)> _solidCells = new();
    private int _cellSize;

    public LevelManager()
    {
        _map = AssetManager.Instance.Load<LDtkMap>("first_world.ldtk");
        SetLevel(_map.GetLevelByName("Level_0"));
    }

    public void SetLevel(LDtkLevel target)
    {
        _level = target;
        BuildCollision();
        BeaconManager.Instance.Publish(GameBeacons.LevelChanged, target);
    }

    private void BuildCollision()
    {
        _solidCells.Clear();

        var layer = _level.Layers.FirstOrDefault(l =>
            l.Type == LDtkLayerType.IntGrid && l.Name == CollisionLayerName
        );

        if (layer == null)
            return;

        _cellSize = layer.TileSize;

        foreach (var cell in layer.InstanceAs<LDtkIntGridInstance>())
        {
            if (!cell.IsSolid)
                continue;

            _solidCells.Add(
                (
                    (int)MathF.Floor(cell.Position.X / _cellSize),
                    (int)MathF.Floor(cell.Position.Y / _cellSize)
                )
            );
        }
    }

    public bool IsBlocked(Rect2 rect)
    {
        if (
            rect.X < 0
            || rect.Y < 0
            || rect.X + rect.Width > _level.Size.X
            || rect.Y + rect.Height > _level.Size.Y
        )
            return true;

        int minX = (int)MathF.Floor(rect.X / _cellSize);
        int minY = (int)MathF.Floor(rect.Y / _cellSize);
        int maxX = (int)MathF.Floor((rect.X + rect.Width - 0.001f) / _cellSize);
        int maxY = (int)MathF.Floor((rect.Y + rect.Height - 0.001f) / _cellSize);

        for (int y = minY; y <= maxY; y++)
        for (int x = minX; x <= maxX; x++)
            if (_solidCells.Contains((x, y)))
                return true;

        return false;
    }
}

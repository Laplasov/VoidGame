using Void.Engine.Assets.Loaders.LDtk;
using Void.Engine.Assets.Loaders.LDtk.Instances;
using VoidGame.Entitis;

namespace VoidGame.managers;

public class SaveLoadManager(LevelManager levelManager)
{
    private LevelManager _levelManager = levelManager;

    private Dictionary<string, List<Npc>> _npcs = new();
    public Dictionary<string, List<Npc>> Npcs => _npcs;

    public void LoadNpcsForCurrentLevel()
    {
        var layer = _levelManager.Level.Layers.FirstOrDefault(l =>
            l.Type == LDtkLayerType.Entities
        );

        if (layer == null)
            return;

        var id = _levelManager.Level.Id;
        if (!_npcs.ContainsKey(id))
            _npcs.Add(id, new List<Npc>());

        foreach (var source in layer.InstanceAs<LDtkEntityInstance>())
        {
            if (!source.Tags.Contains("Npc"))
                continue;

            var npc = _npcs[id].FirstOrDefault(n => n.Source.Id == source.Id);
            if (npc == null)
            {
                npc = new Npc(source, _levelManager);
                _npcs[id].Add(npc);
            }
        }
    }
}

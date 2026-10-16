using Void.Engine.Assets.Loaders.LDtk.Instances;
using VoidGame.managers;

namespace VoidGame.Entitis;

public sealed class Npc(LDtkEntityInstance source, LevelManager levelManager)
    : MapEntity(source: source, levelManager: levelManager) { }

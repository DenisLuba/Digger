using System;
using System.Diagnostics;
using Avalonia.Input;
using Digger.Architecture;

namespace Digger;

//Напишите здесь классы Player, Terrain и другие.

internal class Terrain : ICreature
{
    public string GetImageFileName() => "Terrain.png";

    public int GetDrawingPriority() => 1;

    public CreatureCommand Act(int x, int y) => new CreatureCommand();

    public bool DeadInConflict(ICreature conflictedObject) => true;
}

internal class Player : ICreature
{
    public string GetImageFileName() => "Digger.png";

    public int GetDrawingPriority() => 0;

    public CreatureCommand Act(int x, int y)
    {
        var deltaX = Game.KeyPressed switch
        {
            Key.Right => (x + 1) < Game.MapWidth ? 1 : 0,
            Key.Left => (x - 1) >= 0 ? -1 : 0,
            _ => 0
        };
        var deltaY = Game.KeyPressed switch
        {
            Key.Up => (y - 1) >= 0 ? -1 : 0,
            Key.Down => (y + 1) < Game.MapHeight ? 1 : 0,
            _ => 0
        };
        return new CreatureCommand { DeltaX = deltaX, DeltaY = deltaY };
    }

    public bool DeadInConflict(ICreature conflictedObject) => false;
}
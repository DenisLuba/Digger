using System;
using System.Diagnostics;
using Avalonia.Input;
using Digger.Architecture;

namespace Digger;

internal class Terrain : ICreature
{
    public string GetImageFileName() => "Terrain.png";

    public int GetDrawingPriority() => 1;

    public CreatureCommand Act(int x, int y) => new();

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
            Key.Right => CanGo(x + 1, y) ? 1 : 0,
            Key.Left => CanGo(x - 1, y) ? -1 : 0,
            _ => 0
        };
        var deltaY = Game.KeyPressed switch
        {
            Key.Up => CanGo(x, y - 1) ? -1 : 0,
            Key.Down => CanGo(x, y + 1) ? 1 : 0,
            _ => 0
        };
        return new CreatureCommand { DeltaX = deltaX, DeltaY = deltaY };

        bool CanGo(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Game.MapWidth || y >= Game.MapHeight)
                return false;
            var obj = Game.Map[x, y];
            return obj is not Sack;
        }
    }

    public bool DeadInConflict(ICreature conflictedObject)
        => conflictedObject switch
        {
            Sack => true,
            _ => false
        };
}

internal class Sack : ICreature
{
    private int _deltaY;
    public string GetImageFileName() => "Sack.png";

    public int GetDrawingPriority() => 2;

    public CreatureCommand Act(int x, int y)
    {
        var command = new CreatureCommand();
        if (y + 1 >= Game.MapHeight
            || Game.Map[x, y + 1] is Sack
            || Game.Map[x, y + 1] is Gold
            || Game.Map[x, y + 1] is Terrain)
        {
            if (_deltaY > 1) command.TransformTo = new Gold();
            _deltaY = 0;
            return command;
        }

        if (Game.Map[x, y + 1] is Player && _deltaY == 0)
            return command;
        
        _deltaY++;
        return new CreatureCommand { DeltaY = 1 };
    }

    public bool DeadInConflict(ICreature conflictedObject) => false;
}

internal class Gold : ICreature
{
    public string GetImageFileName() => "Gold.png";

    public int GetDrawingPriority() => 3;

    public CreatureCommand Act(int x, int y) => new();

    public bool DeadInConflict(ICreature conflictedObject)
    {
        if (conflictedObject is not Player) return false;
        Game.Scores += 10;
        return true;
    }
}
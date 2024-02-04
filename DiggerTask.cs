using System;
using System.Data.SqlTypes;
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
    public static int X { get; private set; }

    public static int Y { get; private set; }

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
        X = deltaX + x;
        Y = deltaY + y;
        return new CreatureCommand { DeltaX = deltaX, DeltaY = deltaY };
    }

    private static bool CanGo(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Game.MapWidth || y >= Game.MapHeight)
            return false;
        var obj = Game.Map[x, y];
        return obj is not Sack;
    }

    public bool DeadInConflict(ICreature conflictedObject)
        => conflictedObject switch
        {
            Monster => true,
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
        if (conflictedObject is not Player
            || conflictedObject is not Monster) return false;
        if (conflictedObject is Monster) return true;
        Game.Scores += 10;
        return true;
    }
}

internal class Monster : ICreature
{
    public string GetImageFileName() => "Monster.png";

    public int GetDrawingPriority() => 4;

    public CreatureCommand Act(int x, int y)
    {
        var deltaX = Player.X.CompareTo(x);
        var deltaY = Player.Y.CompareTo(y);
        var command = new CreatureCommand();
        if (CanGo(x + deltaX, y)) command.DeltaX = deltaX;
        else if (CanGo(x, y + deltaY)) command.DeltaY = deltaY;
        return command;
    }

    private static bool CanGo(int x, int y)
        => x >= 0
            || y >= 0
            || x < Game.MapWidth
            || y < Game.MapHeight
            || Game.Map[x, y] is not Sack
            || Game.Map[x, y] is not Monster
            || Game.Map[x, y] is not Terrain;

    public bool DeadInConflict(ICreature conflictedObject)
        => conflictedObject switch
        {
            Monster => true,
            Sack => true,
            _ => false
        };
}

/*
   Exception on: DiggerTestCase: When Digger picks up Gold, 10 Scores must be assigned 
   Exception while acting in GameState: Creatures Gold and Player claimed the same map cell. 
   
   DETAILS
   Initial Map:
   ####
   #PG#
   ####
   Pressed Keys:
   R
   Your Result Map:
   ####
   #·G#
   #### 
*/
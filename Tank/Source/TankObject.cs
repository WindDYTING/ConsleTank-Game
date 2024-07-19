using System;
using Tank.EventArgs;

namespace Tank;

public class TankObject
{
    public static string[] TankSymbol =
    [
        "╧", "╤", "╢", "╟",
    ];

    private static int[] MoveCols = [0, 0, -1, 1];
    private static int[] MoveRows = [-1, 1, 0, 0];

    public Position CurrentPosition { get; private set; }

    public Direction CurrentDirection { get; private set; }

    public ConsoleColorPair SelfColorPair { get; init; }

    public event EventHandler<TankMovingEventArgs>? TankMoving;
    public event EventHandler<TankMovedEventArgs>? TankMoved;

    public TankObject(Position position, Direction currentDirection)
    {
        CurrentPosition = position;
        CurrentDirection = currentDirection;
    }

    public void SetDirection(Direction direction)
    {
        if (direction == Direction.None)
        {
            return;
        }

        CurrentDirection = direction;
    }

    public void Move(Direction direction)
    {
        if (direction == Direction.None) return;
        
        SetDirection(direction);

        var directionValue = (int)direction;

        var originalPosition = CurrentPosition;
        Position nextPosition = new(CurrentPosition.Col + MoveCols[directionValue], CurrentPosition.Row + MoveRows[directionValue]);

        //if (!canMove(nextPosition)) return;
        var movingEventArgs = new TankMovingEventArgs(originalPosition, nextPosition);
        OnMoving(movingEventArgs);

        if (!movingEventArgs.CanMove) return;

        OnMoved(new TankMovedEventArgs(nextPosition, originalPosition));
    }

    protected virtual void OnMoved(TankMovedEventArgs e)
    {
        CurrentPosition = e.CurrentPosition;
        TankMoved?.Invoke(this, e);
    }

    protected virtual void OnMoving(TankMovingEventArgs e)
    {
        TankMoving?.Invoke(this, e);
    }
}
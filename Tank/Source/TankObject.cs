using System;
using System.Collections.Concurrent;
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

    public virtual int BulletCount => 1;

    public virtual int BulletFlySpeed => 1;

    public virtual int MoveSpeed => 1;

    public ConcurrentQueue<Bullet> BulletsSink { get; set; } = new();

    public Position CurrentPosition { get; private set; }

    public Direction CurrentDirection { get; private set; }


    public event EventHandler<TankMovingEventArgs>? TankMoving;
    public event EventHandler<TankMovedEventArgs>? TankMoved;


    public TankObject(Position position, Direction currentDirection)
    {
        CurrentPosition = position;
        CurrentDirection = currentDirection;
        Reload();
    }

    public void Reload()
    {
        if (BulletsSink.IsEmpty)
        {
            for (int i = 0; i < BulletCount; i++)
            {
                BulletsSink.Enqueue(new Bullet(BulletFlySpeed, this));
            }
        }
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

        for (int i = 0; i < MoveSpeed; i++)
        {
            SetDirection(direction);
            var directionValue = (int)direction;
            var originalPosition = CurrentPosition;
            Position nextPosition = new(CurrentPosition.Col + MoveCols[directionValue], CurrentPosition.Row + MoveRows[directionValue]);
            var movingEventArgs = new TankMovingEventArgs(originalPosition, nextPosition);
            OnMoving(movingEventArgs);
            if (!movingEventArgs.CanMove)
            {
                return;
            }

            OnMoved(new TankMovedEventArgs(nextPosition, originalPosition));
        }
    }

    public Bullet? Fire()
    {
        var hasBullet = BulletsSink.TryDequeue(out var bullet);
        if (hasBullet)
        {
            bullet.FlyDirection = CurrentDirection;
            bullet.CurrentPosition = CurrentPosition;
            return bullet;
        }

        return null;
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
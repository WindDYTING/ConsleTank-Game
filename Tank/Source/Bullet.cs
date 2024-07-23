using System;
using Tank.EventArgs;

namespace Tank
{
    public class Bullet
    {
        private static int[] MoveCols = [0, 0, -1, 1];
        private static int[] MoveRows = [-1, 1, 0, 0];

        public Bullet(int flySpeed, TankObject whosFire)
        {
            FlySpeed = flySpeed;
            WhosFire = whosFire;
        }

        public TankObject WhosFire { get; }

        public Position CurrentPosition { get; set; }

        public Direction FlyDirection { get; set; }

        public event EventHandler<BulletFlyingEventArgs>? HitTest;

        public int FlySpeed { get; }

        public virtual char BulletChar { get; set; } = '·';

        public void Fly()
        {
            var direction = (int)FlyDirection;

            var (col, row) = CurrentPosition;
            col += FlySpeed * MoveCols[direction];
            row += FlySpeed * MoveRows[direction];
            var nextPosition = new Position(col, row);
            var e = new BulletFlyingEventArgs
            {
                CurrentPosition = CurrentPosition,
                NextPosition = nextPosition
            };
            CurrentPosition = nextPosition;
            OnHitTest(e);
        }

        protected virtual void OnHitTest(BulletFlyingEventArgs e)
        {
            HitTest?.Invoke(this, e);
        }
    }
}

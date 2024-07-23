namespace Tank.EventArgs
{
    public class BulletFlyingEventArgs : System.EventArgs
    {
        public Position CurrentPosition { get; internal set; }

        public Position NextPosition { get; internal set; }

        public bool IsHit { get; set; }

        public BulletFlyingEventArgs()
        {
            IsHit = false;
        }
    }
}

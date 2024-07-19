namespace Tank.EventArgs
{
    public class TankMovingEventArgs : System.EventArgs
    {
        public Position CurrentPosition { get; }

        public Position NextPosition { get; }

        public bool CanMove { get; set; }

        public TankMovingEventArgs(Position currentPosition, Position nextPosition)
        {
            CurrentPosition = currentPosition;
            NextPosition = nextPosition;
            CanMove = true;
        }
    }
}

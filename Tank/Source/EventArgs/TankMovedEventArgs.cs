namespace Tank.EventArgs
{
    public class TankMovedEventArgs : System.EventArgs
    {
        public Position CurrentPosition { get; }

        public Position PreviousPosition { get; }

        public TankMovedEventArgs(Position currentPosition, Position previousPosition)
        {
            CurrentPosition = currentPosition;
            PreviousPosition = previousPosition;
        }
    }
}

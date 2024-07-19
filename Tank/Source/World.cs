using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Tank.EventArgs;

namespace Tank;

public class World
{
    #region Stage 1

    /* 0  ╔══════════════════════════════════════════════════════════════════════╗
     * 1  ║                                                                      ║
     * 2  ║                                                                      ║
     * 3  ║                                                                      ║
     * 4  ║                                                                      ║
     * 5  ║                                                                      ║
     * 6  ║                                                                      ║
     * 7  ║                                                                      ║
     * 8  ║                                                                      ║
     * 9  ║                                                                      ║
     * 10 ║                                                                      ║
     * 11 ║                                                                      ║
     * 12 ║                                                                      ║
     * 13 ║                                                                      ║
     * 14 ║                                                                      ║
     * 15 ║                                                                      ║
     * 16 ║                                                                      ║
     * 17 ║                                                                      ║
     * 18 ║                                                                      ║
     * 19 ║                                                                      ║
     * 20 ║                                                                      ║
     * 21 ║                                                                      ║
     * 22 ║                                                                      ║
     * 23 ║                                                                      ║
     * 24 ║                                                                      ║
     * 25 ║                                                                      ║
     * 26 ║                                                                      ║
     * 27 ║                                                                      ║
     * 28 ╚══════════════════════════════════════════════════════════════════════╝
     *    
     */

    public static string[] WallsString = [
        "╔══════════════════════════════════════════════════════════════════════╗",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                 ░░░░░░░                              ║",
        "║                                 ░░░░░░░                              ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "║                                                                      ║",
        "╚══════════════════════════════════════════════════════════════════════╝",
    ];

    #endregion

    #region Misc

    private const char Guess = '░';
    public static Position MyTankPosition = new (47, 27);
    public static Direction MyTankDirection = Direction.Up;
    public static ConsoleColorPair MyTankColorPair = new(ConsoleColor.DarkGray, ConsoleColor.Black);
    public static ConsoleColorPair SpaceColorPair = new(ConsoleColor.Black, ConsoleColor.Black);
    public static ConsoleColorPair WallColorPair = new(ConsoleColor.Cyan, ConsoleColor.Black);
    public static ConsoleColorPair GuessColorPair = new(ConsoleColor.White, ConsoleColor.Green);

    private const int DirectionMaxCount = 4;
    private const char SpaceChar = ' ';

    #endregion

    public ConcurrentQueue<Direction> Directions = new ();

    public TankObject Me { get; set; } = new(MyTankPosition, MyTankDirection)
    {
        SelfColorPair = MyTankColorPair
    };

    public World()
    {
        Me.TankMoving += OnTankMoving;
        Me.TankMoved += OnTankMoved;
    }

    private void OnTankMoved(object? sender, TankMovedEventArgs e)
    {
        var previousPosition = e.PreviousPosition;
        var currentPosition = e.CurrentPosition;
        ShowTerrain(previousPosition);
        ShowMyTankInTerrain(currentPosition);
    }

    private void ShowTerrain(Position position)
    {
        var color = DetermineTerrainColor(position.Row, position.Col);
        Utils.UsingColor(color, () =>
        {
            Console.SetCursorPosition(position.Col, position.Row);
            Console.Write(WallsString[position.Row][position.Col]);
        });
    }

    private void OnTankMoving(object? sender, TankMovingEventArgs e)
    {
        var position = e.NextPosition;
        e.CanMove = CanMove(position);
    }

    public void UpdateGameState(CancellationToken token)
    {
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                UpdateMyPosition();
                await Task.Delay(30, token);
            }
        }, token);
    }

    private void UpdateMyPosition()
    {
        if (Directions.TryDequeue(out var myNextDirection))
        {
            Me.Move(myNextDirection);
        }
    }

    public void ShowStage()
    {
        ShowWalls();
        ShowMyTank(MyTankPosition, Me.SelfColorPair);
    }

    private void ShowMyTankInTerrain(Position position)
    {
        if (WallsString[position.Row][position.Col] is SpaceChar)
        {
            ShowMyTank(position, Me.SelfColorPair);
            return;
        }

        Utils.UsingColor(Me.SelfColorPair with { BackgroundColor = GuessColorPair.BackgroundColor }, () =>
        {
            Console.SetCursorPosition(position.Col, position.Row);
            Console.Write(Guess);
        });
        //ShowTerrain(position);
        //ShowMyTank(position, );
    }

    private void ShowMyTank(Position currentPosition, ConsoleColorPair colorPair)
    {
        Utils.UsingColor(colorPair, () =>
        {
            var direction = (int)Me.CurrentDirection % DirectionMaxCount;
            Console.SetCursorPosition(currentPosition.Col, currentPosition.Row);
            Console.Write(TankObject.TankSymbol[direction]);
        });
    }

    private void ShowWalls()
    {
        for (var row = 0; row < WallsString.Length; row++)
        {
            for (var col = 0; col < WallsString[row].Length; col++)
            {
                var row1 = row;
                var col1 = col;
                var terrainColorPair = DetermineTerrainColor(row1, col1);
                Utils.UsingColor(terrainColorPair, () =>
                {
                    Console.Write(WallsString[row1][col1]);
                });
            }
            Console.WriteLine();
        }
    }

    private ConsoleColorPair DetermineTerrainColor(int row, int col)
    {
        return WallsString[row][col] switch
        {
            SpaceChar => SpaceColorPair,
            Guess => GuessColorPair,
            _ => WallColorPair
        };
    }

    private bool CanMove(Position nextPosition)
    {
        var (col, row) = nextPosition;

        return WallsString[row][col] is SpaceChar or Guess;
    }
}
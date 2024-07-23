using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tank.EventArgs;

namespace Tank;

public class World
{
    #region Stage 1

    /* 0  ╔════════════════════════════════════╗
     * 1  ║                                    ║
     * 2  ║                                    ║
     * 3  ║                                    ║
     * 4  ║                                    ║
     * 5  ║                                    ║
     * 6  ║                                    ║
     * 7  ║                                    ║
     * 8  ║                                    ║
     * 9  ║                                    ║
     * 10 ║                                    ║
     * 11 ║                                    ║
     * 12 ║                                    ║
     * 13 ║                                    ║
     * 14 ║                                    ║
     * 15 ║                                    ║
     * 16 ║                                    ║
     * 17 ║                                    ║
     * 18 ║                                    ║
     * 19 ║                                    ║
     * 20 ║                                    ║
     * 21 ║                                    ║
     * 22 ║                                    ║
     * 23 ║                                    ║
     * 24 ║                                    ║
     * 25 ║                                    ║
     * 26 ║                                    ║
     * 27 ║                                    ║
     * 28 ╚════════════════════════════════════╝
     *     
     */

    public static string[] WallsString = [
        "╔═════════════════════════════════════╗",
        "║                                     ║",
        "║   ██████                  ██████    ║",
        "║     ██                      ██      ║",
        "║   ██████                  ██████    ║",
        "║              ████████               ║",
        "║              ████████               ║",
        "║                                     ║",
        "║  ████████████████████████████████   ║",
        "║                                     ║",
        "║                 █                   ║",
        "║════════════     █                   ║",
        "║████████████     █     ██████████████║",
        "║████████████     █     ██████████████║",
        "║████████████     █     ██████████████║",
        "║                       ══════════════║",
        "║         ████                ██████  ║",
        "║     ║         █████                 ║",
        "║     ║         ██             ████   ║",
        "║     ║             ████              ║",
        "║     ║  ███            ██████        ║",
        "║                                     ║",
        "║              ██████                 ║",
        "║   ██████                   ██████   ║",
        "║                ══════               ║",
        "║       ═══                           ║",
        "║                       ██ ██         ║",
        "║                                     ║",
        "╚═════════════════════════════════════╝",
    ];

    #endregion

    #region Misc

    public const char Grass = '░';
    public const char SpaceChar = ' ';
    public const char Brick = '█';
    public const char Wall1 = '║';
    public const char Wall2 = '═';


    private static readonly Position MyTankPosition = new(25, 27);
    private static readonly Direction MyTankDirection = Direction.Up;
    private static readonly ConsoleColorPair MyTankColorPair = new(ConsoleColor.DarkGray, ConsoleColor.Black);
    private static readonly ConsoleColorPair SpaceColorPair = new(ConsoleColor.Black, ConsoleColor.Black);
    private static readonly ConsoleColorPair WallColorPair = new(ConsoleColor.Cyan, ConsoleColor.Black);
    private static readonly ConsoleColorPair GrassColorPair = new(ConsoleColor.White, ConsoleColor.Green);
    private static readonly ConsoleColorPair BrickColorPair = new(ConsoleColor.DarkRed, ConsoleColor.Black);
    private static readonly ConsoleColorPair BulletColorPair = new(ConsoleColor.White, ConsoleColor.Black);

    private const int DirectionMaxCount = 4;

    #endregion

    public ConcurrentQueue<Direction> Directions = new();
    public ConcurrentQueue<Instruction> Instructions = new();
    public List<Bullet> DisplayBulletsInWorld = new();

    public TankObject Me { get; } = new(MyTankPosition, MyTankDirection);

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
        if (!e.CanMove)
        {
            ShowMyTankOnlyChangeDirection(e.CurrentPosition, MyTankColorPair);
        }
    }

    public void UpdateGameState(CancellationToken token)
    {
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                UpdateMyState();

                await Task.Delay(25, token);
            }
        }, token);
    }

    public void UpdateBulletsState(CancellationToken token)
    {
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                for (var i = 0; i < DisplayBulletsInWorld.Count; i++)
                {
                    DisplayBulletsInWorld[i].Fly();
                }
                
                await Task.Delay(75, token);
            }
        }, token);
    }

    private void UpdateMyState()
    {
        UpdateMyPosition();
        DetectIsFire(Me);
    }

    private void DetectIsFire(TankObject tank)
    {
        if (Instructions.TryDequeue(out var instruction) && instruction == Instruction.Fire)
        {
            var bullet = tank.Fire();
            if (bullet != null)
            {
                bullet.HitTest += OnBulletHitTest;
                DisplayBulletsInWorld.Add(bullet);
            }
        }
    }

    private void OnBulletHitTest(object? sender, BulletFlyingEventArgs e)
    {
        var bullet = sender as Bullet;

        ShowBulletInMap(bullet!, e);
        ClearBulletPreviousPath(bullet!, e);

        if (e.IsHit)
        {
            var idx = DisplayBulletsInWorld.IndexOf(bullet!);
            DisplayBulletsInWorld.RemoveAt(idx);
            bullet!.WhosFire.Reload();
            bullet.HitTest -= OnBulletHitTest;
        }
    }

    private void ShowBulletInMap(Bullet bullet, BulletFlyingEventArgs e)
    {
        var (col, row) = e.NextPosition;
        var (changeChar, colorPair, isHit) = DetermineTerrainChar(bullet, row, col);

        Utils.UsingColor(colorPair, () =>
        {
            Console.SetCursorPosition(col, row);
            Console.Write(changeChar);
        });

        UpdateWorldMap(col, row, changeChar);
        
        e.IsHit = isHit;
    }

    private (char changeChar, ConsoleColorPair colorPair, bool isHit) DetermineTerrainChar(Bullet bullet, int row,
        int col)
    {
        (char changeChar, ConsoleColorPair colorPair, bool isHit) = WallsString[row][col] switch
        {
            SpaceChar => (bullet.BulletChar, BulletColorPair, false),
            Brick => (SpaceChar, SpaceColorPair, true),
            Grass => (Grass, GrassColorPair, false),
            Wall1 => (Wall1, WallColorPair, true),
            Wall2 => (Wall2, WallColorPair, true),
            _ => (SpaceChar, SpaceColorPair, false)
        };
        return (changeChar, colorPair, isHit);
    }

    private void UpdateWorldMap(int col, int row, char changeChar)
    {
        var chars = WallsString[row].ToCharArray();
        chars[col] = changeChar;
        WallsString[row] = new string(chars);
    }

    private void ClearBulletPreviousPath(Bullet bullet, BulletFlyingEventArgs e)
    {
        var (previousCol, previousRow) = e.CurrentPosition;
        if (Me.CurrentPosition != e.CurrentPosition)
        {
            var (terrainChar, colorPair, _) = DetermineTerrainChar(bullet, previousRow, previousCol);
            Utils.UsingColor(colorPair, () =>
            {
                Console.SetCursorPosition(previousCol, previousRow);
                Console.Write(terrainChar);
            });
            UpdateWorldMap(previousCol, previousRow, terrainChar);
        }
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
        ShowMyTank(MyTankPosition, MyTankColorPair);
    }

    private void ShowMyTankInTerrain(Position position)
    {
        if (WallsString[position.Row][position.Col] is Grass)
        {
            Utils.UsingColor(MyTankColorPair with { BackgroundColor = GrassColorPair.BackgroundColor }, () =>
            {
                Console.SetCursorPosition(position.Col, position.Row);
                Console.Write(Grass);
            });
            return;
        }

        ShowMyTank(position, MyTankColorPair);
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

    private void ShowMyTankOnlyChangeDirection(Position position, ConsoleColorPair colorPair)
    {
        ShowMyTank(position, colorPair);
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
            Grass => GrassColorPair,
            Brick => BrickColorPair,
            _ => WallColorPair
        };
    }

    private bool CanMove(Position nextPosition)
    {
        var (col, row) = nextPosition;

        return WallsString[row][col] is SpaceChar or Grass;
    }
}
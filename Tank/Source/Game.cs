using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tank;

public class Game
{
    private readonly World _world;
    private readonly DebounceJob _debounceJob = new (TimeSpan.FromMilliseconds(65));

    public Game()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        _world = new World();
    }

    public async Task RunAsync()
    {
        LoadWorld();

        await WaitStartKeyAsync().ConfigureAwait(false);
        
        var cts = new CancellationTokenSource();

        _world.UpdateGameState(cts.Token);
        _world.UpdateBulletsState(cts.Token);

        await ReadUserKeyAsync(cts).ConfigureAwait(false);
    }

    private void LoadWorld()
    {
        Console.CursorVisible = false;
        Console.Clear();
        _world.ShowStage();
    }

    private Task WaitKeyAsync(ConsoleKey waitKey)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Task.Run(() =>
        {
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            } while (key != waitKey);

            tcs.TrySetResult();
        });

        return tcs.Task;
    }
    
    private Task ReadUserKeyAsync(CancellationTokenSource cts)
    {
        return Task.Run(() =>
        {
            while (!cts.IsCancellationRequested)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Escape)
                {
                    cts.Cancel();
                }

                (Instruction instruction, Direction direction) = key switch
                {
                    ConsoleKey.UpArrow => (Instruction.Move, Direction.Up),
                    ConsoleKey.DownArrow => (Instruction.Move, Direction.Down),
                    ConsoleKey.LeftArrow => (Instruction.Move, Direction.Left),
                    ConsoleKey.RightArrow => (Instruction.Move, Direction.Right),
                    ConsoleKey.Spacebar => (Instruction.Fire, Direction.None),
                    _ => (Instruction.None, Direction.None),
                };

                AddDirectionAndInstruction(direction, instruction);
            }
        });
    }

    private void AddDirectionAndInstruction(Direction direction, Instruction instruction)
    {
        _debounceJob.Run(() =>
        {
            _world.Directions.Enqueue(direction);
            _world.Instructions.Enqueue(instruction);
        });
    }

    private Task WaitStartKeyAsync() => WaitKeyAsync(ConsoleKey.Enter);

    private Task WaitStopKeyAsync() => WaitKeyAsync(ConsoleKey.Escape);
}
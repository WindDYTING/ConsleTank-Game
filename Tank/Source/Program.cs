using System.Threading.Tasks;

namespace Tank;

public class Program
{
    public static async Task Main(string[] args)
    {
        var game = new Game();
        await game.RunAsync();
    }
}

using System;

namespace NetworkedGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new NetworkedGame())
                game.Run();
        }
    }
}

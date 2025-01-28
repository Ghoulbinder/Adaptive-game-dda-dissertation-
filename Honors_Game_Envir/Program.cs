using System;
using Survivor_of_the_Bulge;

namespace Honors_Game_Envir
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}

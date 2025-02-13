using Microsoft.Xna.Framework;

namespace Survivor_of_the_Bulge
{
    public class Transition
    {
        public GameState From { get; }
        public GameState To { get; }
        public Rectangle Zone { get; }

        public Transition(GameState from, GameState to, Rectangle zone)
        {
            From = from;
            To = to;
            Zone = zone;
        }
    }
}

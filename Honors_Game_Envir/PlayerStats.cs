using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Survivor_of_the_Bulge
{
    public class PlayerStats
    {
        public int Health { get; private set; }
        public int Stamina { get; private set; }
        public int Strength { get; private set; }
        public int Experience { get; private set; }
        public int Level { get; private set; }

        private SpriteFont font;

        public PlayerStats(int health, int stamina, int strength, int experience, int level, SpriteFont statFont)
        {
            Health = health;
            Stamina = stamina;
            Strength = strength;
            Experience = experience;
            Level = level;
            font = statFont;
        }

        public void IncreaseExperience(int amount)
        {
            Experience += amount;
            if (Experience >= Level * 100)  // Level up condition
            {
                Level++;
                Experience = 0;
                Strength += 5;  // Example: Increase strength when leveling up
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            string statsText = $"Health: {Health}\nStamina: {Stamina}\nStrength: {Strength}\nLevel: {Level}\nXP: {Experience}";
            spriteBatch.DrawString(font, statsText, position, Color.Black);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class PlayerStats
    {
        public int Health { get; set; }
        public int Lives { get; set; }
        public int AttackDamage { get; set; }  // For testing, set to 50 so one shot kills an enemy.
        public float AttackSpeed { get; set; } // Shots per second.
        public float MovementSpeed { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public int LevelUpThreshold { get; set; } = 100;

        private SpriteFont font;

        public PlayerStats(int health, int lives, int attackDamage, float attackSpeed, float movementSpeed, int experience, int level, SpriteFont font)
        {
            Health = health;
            Lives = lives;
            AttackDamage = attackDamage; // Set to 50 for one-hit kills.
            AttackSpeed = attackSpeed;
            MovementSpeed = movementSpeed;
            Experience = experience;
            Level = level;
            this.font = font;
        }

        public void IncreaseExperience(int amount)
        {
            Experience += amount;
            if (Experience >= Level * LevelUpThreshold)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            Level++;
            Experience = 0;
            AttackDamage += 2;
            AttackSpeed += 0.1f;
            MovementSpeed += 10f;
            Health += 20;
        }

        public void UpdateHealth(int newHealth)
        {
            Health = newHealth;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            string statsText = $"Health: {Health}\nLives: {Lives}\nAttack Damage: {AttackDamage}\nAttack Speed: {AttackSpeed}\nMovement Speed: {MovementSpeed}\nExp: {Experience}\nLevel: {Level}";
            spriteBatch.DrawString(font, statsText, position, Color.Black);
        }
    }
}

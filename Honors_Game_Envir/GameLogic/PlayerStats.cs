using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class PlayerStats
    {
        public int Health { get; private set; }
        public int Lives { get; private set; }
        public int AttackDamage { get; private set; }
        public float AttackSpeed { get; private set; }
        public float MovementSpeed { get; private set; }
        public int Experience { get; private set; }
        public int Level { get; private set; }

        private SpriteFont statFont;

        /// <summary>
        /// Constructs a new PlayerStats object.
        /// Parameters: health, lives, attackDamage, attackSpeed, movementSpeed, experience, level, and the stat display font.
        /// </summary>
        public PlayerStats(int health, int lives, int attackDamage, float attackSpeed, float movementSpeed, int experience, int level, SpriteFont statFont)
        {
            Health = health;
            Lives = lives;
            AttackDamage = attackDamage;
            AttackSpeed = attackSpeed;
            MovementSpeed = movementSpeed;
            Experience = experience;
            Level = level;
            this.statFont = statFont;
        }

        public void IncreaseExperience(int amount)
        {
            Experience += amount;
            if (Experience >= Level * 100)
            {
                Level++;
                Experience = 0;
                // Increase health, movement speed, attack damage, and optionally adjust attack speed.
                Health += 20;
                MovementSpeed += 10f;
                AttackDamage += 5;
                AttackSpeed += 0.1f;
            }
        }

        public void UpdateHealth(int newHealth)
        {
            Health = newHealth;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            string statsText = $"Health: {Health}\nLives: {Lives}\nAttack Damage: {AttackDamage}\nAttack Speed: {AttackSpeed}\nMovement Speed: {MovementSpeed}\nExp: {Experience}\nLevel: {Level}";
            spriteBatch.DrawString(statFont, statsText, position, Color.Black);
        }
    }
}

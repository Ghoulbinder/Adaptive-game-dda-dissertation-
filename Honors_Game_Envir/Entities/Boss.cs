using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    // Boss inherits from Enemy.
    public class Boss : Enemy
    {
        // Scale factor for rendering the boss larger.
        public float Scale { get; set; } = 2.0f;

        public Boss(Texture2D back, Texture2D front, Texture2D left,
            Texture2D bulletHorizontal, Texture2D bulletVertical,
            Vector2 startPosition, Direction startDirection,
            int health, int bulletDamage)
            : base(back, front, left, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            // Override boss-specific modular stats.
            MovementSpeed = 150f;
            FiringInterval = 1.5f;
            BulletRange = 500f;
            CollisionDamage = 30;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;

            // Use the same logic as Enemy.Draw but apply the Scale factor.
            Texture2D currentTexture = frontTexture;
            SpriteEffects spriteEffects = SpriteEffects.None;
            switch (currentDirection)
            {
                case Direction.Left:
                    currentTexture = leftTexture;
                    break;
                case Direction.Right:
                    currentTexture = leftTexture;
                    spriteEffects = SpriteEffects.FlipHorizontally;
                    break;
                case Direction.Up:
                    currentTexture = backTexture;
                    break;
                case Direction.Down:
                    currentTexture = frontTexture;
                    break;
            }

            spriteBatch.Draw(currentTexture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, Scale, spriteEffects, 0f);
            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }
    }
}

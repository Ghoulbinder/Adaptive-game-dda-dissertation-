using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Boss : Enemy
    {
        // Boss-specific scaling factor.
        public float Scale { get; set; } = 2.0f;

        public Boss(Texture2D back, Texture2D front, Texture2D left,
                    Texture2D bulletHorizontal, Texture2D bulletVertical,
                    Vector2 startPosition, Direction startDirection,
                    int health, int bulletDamage)
            : base(back, front, left, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            MovementSpeed = 150f;
            FiringInterval = 1.5f;
            BulletRange = 500f;
            CollisionDamage = 30;
        }

        // Override Update so that each frame the boss updates its stats immediately.
        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            // Apply boss-specific difficulty modifiers.
            ApplyDifficultyModifiers();
            // Then use the standard enemy update logic.
            base.Update(gameTime, viewport, playerPosition, player);
        }

        // Override ApplyDifficultyModifiers for Boss using boss-specific multipliers.
        public override void ApplyDifficultyModifiers()
        {
            // Update MovementSpeed and BulletDamage using boss-specific multipliers.
            MovementSpeed = 150f * DifficultyManager.Instance.BossMovementSpeedMultiplier;
            BulletDamage = (int)(baseDamage * DifficultyManager.Instance.BossDamageMultiplier);
            // Calculate new maximum health.
            int newMaxHealth = (int)(baseHealth * DifficultyManager.Instance.BossHealthMultiplier);
            // Immediately update Health to the new maximum.
            Health = newMaxHealth;
            // Update FiringInterval based on boss attack speed multiplier.
            FiringInterval = 1.5f / DifficultyManager.Instance.BossAttackSpeedMultiplier;
        }

        // Override Bounds with center-based logic using scaling.
        public override Rectangle Bounds
        {
            get
            {
                int frameW, frameH;
                if (currentDirection == Direction.Left || currentDirection == Direction.Right)
                {
                    frameW = leftTexture.Width / totalFrames;
                    frameH = leftTexture.Height;
                }
                else
                {
                    frameW = frontTexture.Width / totalFrames;
                    frameH = frontTexture.Height;
                }

                float scaledW = frameW * Scale;
                float scaledH = frameH * Scale;

                return new Rectangle(
                    (int)(Position.X - scaledW / 2),
                    (int)(Position.Y - scaledH / 2),
                    (int)scaledW,
                    (int)scaledH
                );
            }
        }

        // Override Draw to apply custom scaling.
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead) return;

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

            int frameW = currentTexture.Width / totalFrames;
            int frameH = currentTexture.Height;
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

            spriteBatch.Draw(
                currentTexture,
                Position,
                sourceRectangle,
                Color.White,
                0f,
                origin,
                Scale,
                spriteEffects,
                0f
            );

            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }
    }
}

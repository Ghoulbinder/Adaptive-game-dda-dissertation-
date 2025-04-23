using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Boss : Enemy
    {
        // PSEUDOCODE: Define scaling factor for boss rendering
        public float Scale { get; set; } = 2.0f;

        public Boss(Texture2D back, Texture2D front, Texture2D left,
                    Texture2D bulletHorizontal, Texture2D bulletVertical,
                    Vector2 startPosition, Direction startDirection,
                    int health, int bulletDamage)
            : base(back, front, left, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            // PSEUDOCODE: Initialize default boss behavior parameters
            MovementSpeed = 150f;
            FiringInterval = 1.5f;
            BulletRange = 500f;
            CollisionDamage = 30;
        }

        // PSEUDOCODE: Each frame, adjust stats then perform standard enemy update
        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            ApplyDifficultyModifiers();
            base.Update(gameTime, viewport, playerPosition, player);
        }

        // PSEUDOCODE: Scale boss stats according to current difficulty multipliers
        public override void ApplyDifficultyModifiers()
        {
            MovementSpeed = 150f * DifficultyManager.Instance.BossMovementSpeedMultiplier;
            BulletDamage = (int)(baseDamage * DifficultyManager.Instance.BossDamageMultiplier);

            int newMaxHealth = (int)(baseHealth * DifficultyManager.Instance.BossHealthMultiplier);
            Health = newMaxHealth;

            FiringInterval = 1.5f / DifficultyManager.Instance.BossAttackSpeedMultiplier;
        }

        // PSEUDOCODE: Compute bounding box centered on position, scaled by Scale
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

        // PSEUDOCODE: Draw boss sprite using current direction and scaling
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

            // PSEUDOCODE: Draw all active boss bullets
            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }
    }
}

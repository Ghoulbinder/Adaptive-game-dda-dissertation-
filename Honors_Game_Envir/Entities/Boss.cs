using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Boss : Enemy
    {
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

        // Override Draw method.
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

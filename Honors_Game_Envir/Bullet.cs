using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Bullet
    {
        public Vector2 Position { get; private set; }
        private Vector2 direction;
        private float speed;
        private Texture2D texture;
        private Rectangle sourceRectangle;
        private bool isActive;
        private SpriteEffects spriteEffects;

        public bool IsActive => isActive;

        public Bullet(Texture2D texture, Vector2 startPosition, Vector2 direction, float speed, SpriteEffects spriteEffects)
        {
            this.texture = texture;
            Position = startPosition;
            this.direction = direction;
            this.speed = speed;
            isActive = true;
            this.spriteEffects = spriteEffects;

            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public void Update(GameTime gameTime)
        {
            Position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Position.X < 0 || Position.X > 1600 || Position.Y < 0 || Position.Y > 980)
            {
                isActive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isActive)
            {
                spriteBatch.Draw(texture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
            }
        }
    }

}

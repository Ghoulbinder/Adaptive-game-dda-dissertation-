using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Bullet
    {
        public Vector2 Position { get; private set; }
        private Vector2 direction;
        private float speed;
        private int damage;
        private Texture2D texture;
        private Rectangle sourceRectangle;
        private bool isActive;
        private SpriteEffects spriteEffects;

        // New: maximum travel range and starting position.
        private float maxRange;
        private Vector2 startPosition;

        public bool IsActive => isActive;
        public int Damage => damage;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, texture.Width, texture.Height);

        // Updated constructor now takes maxRange.
        public Bullet(Texture2D texture, Vector2 startPosition, Vector2 direction, float speed, int damage, SpriteEffects spriteEffects, float maxRange)
        {
            this.texture = texture;
            this.startPosition = startPosition;
            Position = startPosition;
            this.direction = direction;
            this.speed = speed;
            this.damage = damage;
            isActive = true;
            this.spriteEffects = spriteEffects;
            this.maxRange = maxRange;
            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public void Update(GameTime gameTime)
        {
            Position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Deactivate if bullet has traveled beyond its max range.
            if (Vector2.Distance(Position, startPosition) > maxRange)
            {
                Deactivate();
            }

            // Also deactivate if the bullet leaves the screen (assuming 1600x980).
            if (Position.X < 0 || Position.X > 1600 || Position.Y < 0 || Position.Y > 980)
            {
                Deactivate();
            }
        }

        public void Deactivate()
        {
            isActive = false;
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

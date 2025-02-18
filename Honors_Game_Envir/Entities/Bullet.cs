using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Bullet
    {
        public Vector2 Position { get; private set; }
        protected Vector2 direction;
        protected float speed;
        protected int damage;
        protected Texture2D texture;
        protected Rectangle sourceRectangle;
        protected bool isActive;
        protected SpriteEffects spriteEffects;
        protected Vector2 startPosition;

        public bool IsActive => isActive;
        // Added public Damage property so that other classes can access bullet damage.
        public int Damage => damage;
        public virtual Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, texture.Width, texture.Height);

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
            // We ignore maxRange now so bullets only disappear when off-screen.
            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public virtual void Update(GameTime gameTime)
        {
            Position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Deactivate bullet if it goes off screen (assuming a screen size of 1600x980).
            if (Position.X < 0 || Position.X > 1600 || Position.Y < 0 || Position.Y > 980)
                Deactivate();
        }

        public virtual void Deactivate()
        {
            isActive = false;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
                spriteBatch.Draw(texture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
        }
    }
}

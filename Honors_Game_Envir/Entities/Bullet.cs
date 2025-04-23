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
        // PSEUDOCODE: Allow external access to bullet damage value
        public int Damage => damage;

        // PSEUDOCODE: Define bounding box for collision detection
        public virtual Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, texture.Width, texture.Height);

        public Bullet(Texture2D texture, Vector2 startPosition, Vector2 direction, float speed, int damage, SpriteEffects spriteEffects, float maxRange)
        {
            // PSEUDOCODE: Initialize bullet properties using constructor parameters
            this.texture = texture;
            this.startPosition = startPosition;
            Position = startPosition;
            this.direction = direction;
            this.speed = speed;
            this.damage = damage;
            isActive = true;
            this.spriteEffects = spriteEffects;

            // PSEUDOCODE: Use full texture by default
            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);

            // NOTE: maxRange is ignored; bullets deactivate only when off-screen
        }

        public virtual void Update(GameTime gameTime)
        {
            // PSEUDOCODE: Move bullet along its direction each frame
            Position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // PSEUDOCODE: Deactivate bullet if it leaves screen bounds
            if (Position.X < 0 || Position.X > 1600 || Position.Y < 0 || Position.Y > 980)
                Deactivate();
        }

        public virtual void Deactivate()
        {
            // PSEUDOCODE: Mark bullet as inactive so it can be removed
            isActive = false;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // PSEUDOCODE: Render bullet only if it remains active
            if (IsActive)
                spriteBatch.Draw(texture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Enemy
    {
        private Texture2D backTexture, frontTexture, leftTexture, rightTexture;
        public Vector2 Position;
        private float speed = 100f;
        private Rectangle sourceRectangle;
        private float frameTime = 0.1f;
        private float timer = 0f;
        private int currentFrame = 0;
        private int totalFrames = 4;

        private enum Direction { Left, Right, Up, Down }
        private Direction currentDirection = Direction.Left;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, leftTexture.Width / totalFrames, leftTexture.Height);

        public Enemy(Texture2D back, Texture2D front, Texture2D left, Texture2D right, Vector2 startPosition)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            rightTexture = right;
            Position = startPosition;

            sourceRectangle = new Rectangle(0, 0, left.Width / totalFrames, left.Height);
        }

        public void Update(GameTime gameTime, Viewport viewport)
        {
            // Patrol movement logic
            if (currentDirection == Direction.Left)
            {
                Position.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (Position.X <= 0) currentDirection = Direction.Right;
            }
            else if (currentDirection == Direction.Right)
            {
                Position.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (Position.X >= viewport.Width - sourceRectangle.Width)
                    currentDirection = Direction.Left;
            }
            else if (currentDirection == Direction.Up)
            {
                Position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (Position.Y <= 0) currentDirection = Direction.Down;
            }
            else if (currentDirection == Direction.Down)
            {
                Position.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (Position.Y >= viewport.Height - sourceRectangle.Height)
                    currentDirection = Direction.Up;
            }

            // Update animation frame
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }

            // Update source rectangle for animation
            sourceRectangle.X = currentFrame * sourceRectangle.Width;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D currentTexture = frontTexture;
            SpriteEffects spriteEffect = SpriteEffects.None;

            switch (currentDirection)
            {
                case Direction.Left:
                    currentTexture = leftTexture;
                    spriteEffect = SpriteEffects.None;
                    break;

                case Direction.Right:
                    currentTexture = leftTexture; // Use left texture and flip
                    spriteEffect = SpriteEffects.FlipHorizontally;
                    break;

                case Direction.Up:
                    currentTexture = backTexture;
                    spriteEffect = SpriteEffects.None;
                    break;

                case Direction.Down:
                    currentTexture = frontTexture;
                    spriteEffect = SpriteEffects.None;
                    break;
            }

            spriteBatch.Draw(currentTexture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffect, 0f);
        }
    }
}

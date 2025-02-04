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

        public enum Direction { Left, Right, Up, Down }
        private Direction currentDirection;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, sourceRectangle.Width, sourceRectangle.Height);

        public Enemy(Texture2D back, Texture2D front, Texture2D left, Texture2D right, Vector2 startPosition, Direction startDirection)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            rightTexture = right;
            Position = startPosition;
            currentDirection = startDirection;

            // Initialize source rectangle size based on direction
            switch (currentDirection)
            {
                case Direction.Up:
                case Direction.Down:
                    sourceRectangle = new Rectangle(0, 0, front.Width / totalFrames, front.Height);
                    break;

                case Direction.Left:
                case Direction.Right:
                    sourceRectangle = new Rectangle(0, 0, left.Width / totalFrames, left.Height);
                    break;
            }
        }

        public void Update(GameTime gameTime, Viewport viewport)
        {
            // Patrol movement logic
            switch (currentDirection)
            {
                case Direction.Left:
                    Position.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Position.X <= 0) currentDirection = Direction.Right;
                    break;

                case Direction.Right:
                    Position.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Position.X >= viewport.Width - sourceRectangle.Width) currentDirection = Direction.Left;
                    break;

                case Direction.Up:
                    Position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Position.Y <= 0) currentDirection = Direction.Down;
                    break;

                case Direction.Down:
                    Position.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Position.Y >= viewport.Height - sourceRectangle.Height) currentDirection = Direction.Up;
                    break;
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
                    currentTexture = rightTexture;
                    spriteEffect = SpriteEffects.None;
                    break;

                case Direction.Up:
                    currentTexture = backTexture;
                    break;

                case Direction.Down:
                    currentTexture = frontTexture;
                    break;
            }

            spriteBatch.Draw(currentTexture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffect, 0f);
        }
    }
}

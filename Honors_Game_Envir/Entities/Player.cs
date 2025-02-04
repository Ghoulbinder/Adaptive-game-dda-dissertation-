using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Player
    {
        private Texture2D backTexture, frontTexture, leftTexture;
        public Vector2 Position;
        private float speed = 200f;

        private Rectangle sourceRectangle;
        private float frameTime = 0.1f;
        private float timer = 0f;
        private int currentFrame = 0;
        private int totalFrames = 4;

        private int frameWidth;  // Dynamic frame width
        private int frameHeight; // Dynamic frame height

        private enum Direction { Left, Right, Up, Down }
        private Direction currentDirection = Direction.Down;

        public Player(Texture2D back, Texture2D front, Texture2D left, Vector2 startPosition)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            Position = startPosition;

            // Set default frame dimensions based on "down" direction (frontTexture)
            frameWidth = frontTexture.Width / totalFrames;
            frameHeight = frontTexture.Height;
            sourceRectangle = new Rectangle(0, 0, frameWidth, frameHeight);
        }

        public void Update(GameTime gameTime, Viewport viewport)
        {
            Vector2 movement = Vector2.Zero;
            var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            // Handle movement and direction
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                movement.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Up;
            }
            else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                movement.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Down;
            }
            else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            {
                movement.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Left;
            }
            else if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                movement.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Right;
            }

            // Update position and clamp to screen bounds
            Position += movement;
            Position.X = MathHelper.Clamp(Position.X, 0, viewport.Width - frameWidth);
            Position.Y = MathHelper.Clamp(Position.Y, 0, viewport.Height - frameHeight);

            // Update animation frame if the player is moving
            if (movement != Vector2.Zero)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= frameTime)
                {
                    currentFrame = (currentFrame + 1) % totalFrames;
                    timer = 0f;
                }
            }

            // Update frame dimensions based on the direction
            switch (currentDirection)
            {
                case Direction.Up:
                    frameWidth = backTexture.Width / totalFrames;
                    frameHeight = backTexture.Height;
                    break;
                case Direction.Down:
                    frameWidth = frontTexture.Width / totalFrames;
                    frameHeight = frontTexture.Height;
                    break;
                case Direction.Left:
                case Direction.Right:
                    frameWidth = leftTexture.Width / totalFrames;
                    frameHeight = leftTexture.Height;
                    break;
            }

            // Update source rectangle
            sourceRectangle = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D currentTexture = frontTexture;
            SpriteEffects spriteEffects = SpriteEffects.None;

            // Determine texture and sprite effect
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

            // Draw the current frame
            spriteBatch.Draw(
                currentTexture,
                Position,
                sourceRectangle,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                spriteEffects,
                0f
            );
        }
    }
}

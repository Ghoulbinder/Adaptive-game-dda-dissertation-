using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        private enum Direction { Idle, Left, Right, Up, Down }
        private Direction currentDirection = Direction.Idle;
        private Direction previousDirection = Direction.Down;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, leftTexture.Width / totalFrames, leftTexture.Height);

        public Player(Texture2D back, Texture2D front, Texture2D left, Vector2 startPosition)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            Position = startPosition;

            sourceRectangle = new Rectangle(0, 0, left.Width / totalFrames, left.Height);
        }

        public void Update(GameTime gameTime, Viewport viewport)
        {
            var keyboardState = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            // Handle Movement and Direction
            if (keyboardState.IsKeyDown(Keys.W))
            {
                movement.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Up;
            }
            else if (keyboardState.IsKeyDown(Keys.S))
            {
                movement.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Down;
            }
            else if (keyboardState.IsKeyDown(Keys.A))
            {
                movement.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Left;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                movement.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Right;
            }
            else
            {
                currentDirection = Direction.Idle;
            }

            // Update position and clamp to screen bounds
            Position += movement;
            Position.X = MathHelper.Clamp(Position.X, 0, viewport.Width - sourceRectangle.Width);
            Position.Y = MathHelper.Clamp(Position.Y, 0, viewport.Height - sourceRectangle.Height);

            // Update animation frame
            if (currentDirection != Direction.Idle)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= frameTime)
                {
                    currentFrame = (currentFrame + 1) % totalFrames;
                    timer = 0f;
                }
                previousDirection = currentDirection; // Save last direction when moving
            }
            else
            {
                currentFrame = 0; // Reset to idle frame
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
                    currentTexture = leftTexture;
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

                case Direction.Idle:
                    // Use the first frame of the last movement direction's texture
                    switch (previousDirection)
                    {
                        case Direction.Up:
                            currentTexture = backTexture;
                            break;
                        case Direction.Down:
                            currentTexture = frontTexture;
                            break;
                        case Direction.Left:
                            currentTexture = leftTexture;
                            spriteEffect = SpriteEffects.None;
                            break;
                        case Direction.Right:
                            currentTexture = leftTexture;
                            spriteEffect = SpriteEffects.FlipHorizontally;
                            break;
                    }
                    break;
            }

            spriteBatch.Draw(currentTexture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffect, 0f);
        }

        public void ResolveCollision(Rectangle obstacleBounds)
        {
            Rectangle playerBounds = Bounds;

            // Determine collision side and adjust position
            if (playerBounds.Right > obstacleBounds.Left && playerBounds.Left < obstacleBounds.Left)
            {
                Position.X = obstacleBounds.Left - playerBounds.Width; // Collision on the right
            }
            else if (playerBounds.Left < obstacleBounds.Right && playerBounds.Right > obstacleBounds.Right)
            {
                Position.X = obstacleBounds.Right; // Collision on the left
            }

            if (playerBounds.Bottom > obstacleBounds.Top && playerBounds.Top < obstacleBounds.Top)
            {
                Position.Y = obstacleBounds.Top - playerBounds.Height; // Collision below
            }
            else if (playerBounds.Top < obstacleBounds.Bottom && playerBounds.Bottom > obstacleBounds.Bottom)
            {
                Position.Y = obstacleBounds.Bottom; // Collision above
            }
        }


    }
}

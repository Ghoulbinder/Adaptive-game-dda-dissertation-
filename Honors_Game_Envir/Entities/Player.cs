using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Player
    {
        private Texture2D backTexture, frontTexture, leftTexture;
        public Vector2 Position;
        public Vector2 Movement; // New Movement vector to track player's movement direction
        private float speed = 200f;

        private Rectangle sourceRectangle;
        private float frameTime = 0.1f;
        private float timer = 0f;
        private int currentFrame = 0;
        private int totalFrames = 4;

        private enum Direction { Idle, Left, Right, Up, Down }
        private Direction currentDirection = Direction.Idle;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, sourceRectangle.Width, sourceRectangle.Height);

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
            Vector2 movement = Vector2.Zero;
            var keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            // Reset direction to Idle
            currentDirection = Direction.Idle;

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
            Position.X = MathHelper.Clamp(Position.X, 0, viewport.Width - sourceRectangle.Width);
            Position.Y = MathHelper.Clamp(Position.Y, 0, viewport.Height - sourceRectangle.Height);

            Movement = movement; // Save the movement vector

            // Update animation frame if moving
            if (currentDirection != Direction.Idle)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= frameTime)
                {
                    currentFrame = (currentFrame + 1) % totalFrames;
                    timer = 0f;
                }
            }
            else
            {
                currentFrame = 0; // Reset animation frame for idle state
            }

            // Update source rectangle
            sourceRectangle.X = currentFrame * sourceRectangle.Width;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(frontTexture, Position, sourceRectangle, Color.White);
        }

        public void ResolveCollision(Rectangle obstacleBounds)
        {
            // Prevent player from moving into obstacles
            if (Bounds.Intersects(obstacleBounds))
            {
                if (Movement.X > 0) // Moving right
                    Position.X = obstacleBounds.Left - Bounds.Width;
                if (Movement.X < 0) // Moving left
                    Position.X = obstacleBounds.Right;
                if (Movement.Y > 0) // Moving down
                    Position.Y = obstacleBounds.Top - Bounds.Height;
                if (Movement.Y < 0) // Moving up
                    Position.Y = obstacleBounds.Bottom;
            }
        }
    }
}

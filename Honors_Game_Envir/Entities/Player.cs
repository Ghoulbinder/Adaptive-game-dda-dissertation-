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

        private int frameWidth;
        private int frameHeight;

        private enum Direction { Left, Right, Up, Down }
        private Direction currentDirection = Direction.Down;

        public PlayerStats Stats { get; private set; }
        private bool showStats = false; // Toggle for showing stats

        public Player(Texture2D back, Texture2D front, Texture2D left, Vector2 startPosition, SpriteFont statFont)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            Position = startPosition;

            frameWidth = frontTexture.Width / totalFrames;
            frameHeight = frontTexture.Height;
            sourceRectangle = new Rectangle(0, 0, frameWidth, frameHeight);

            // Initialize stats with default values
            Stats = new PlayerStats(100, 100, 10, 0, 1, statFont);
        }

        public void Update(GameTime gameTime, Viewport viewport)
        {
            Vector2 movement = Vector2.Zero;
            var keyboardState = Keyboard.GetState();

            // Toggle stats page with 'TAB'
            if (keyboardState.IsKeyDown(Keys.Tab))
            {
                showStats = !showStats;
            }

            if (!showStats)  // Prevent movement when stats page is open
            {
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

                Position += movement;
                Position.X = MathHelper.Clamp(Position.X, 0, viewport.Width - frameWidth);
                Position.Y = MathHelper.Clamp(Position.Y, 0, viewport.Height - frameHeight);
            }

            if (movement != Vector2.Zero)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= frameTime)
                {
                    currentFrame = (currentFrame + 1) % totalFrames;
                    timer = 0f;
                }
            }

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

            sourceRectangle = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
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

            // Draw stats if toggled
            if (showStats)
            {
                Stats.Draw(spriteBatch, new Vector2(Position.X + 50, Position.Y - 100));
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Survivor_of_the_Bulge
{
    public class Player
    {
        private Texture2D backTexture, frontTexture, leftTexture, bulletHorizontalTexture, bulletVerticalTexture;
        public Vector2 Position;
        private float speed = 200f;
        private int health = 100;
        private int bulletDamage = 10;
        private Rectangle sourceRectangle;
        private float frameTime = 0.1f;
        private float timer = 0f;
        private int currentFrame = 0;
        private int totalFrames = 4;
        private int frameWidth;
        private int frameHeight;
        private enum Direction { Left, Right, Up, Down }
        private Direction currentDirection = Direction.Down;
        private List<Bullet> bullets;
        private float bulletSpeed = 500f;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, frameWidth, frameHeight);

        public Player(Texture2D back, Texture2D front, Texture2D left, Texture2D bulletHorizontalTexture, Texture2D bulletVerticalTexture, Vector2 startPosition)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            Position = startPosition;
            this.bulletHorizontalTexture = bulletHorizontalTexture;
            this.bulletVerticalTexture = bulletVerticalTexture;
            frameWidth = frontTexture.Width / totalFrames;
            frameHeight = frontTexture.Height;
            sourceRectangle = new Rectangle(0, 0, frameWidth, frameHeight);
            bullets = new List<Bullet>();
        }

        public void Update(GameTime gameTime, Viewport viewport, List<Enemy> enemies)
        {
            Vector2 movement = Vector2.Zero;
            var keyboardState = Keyboard.GetState();

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

            if (movement != Vector2.Zero)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= frameTime)
                {
                    currentFrame = (currentFrame + 1) % totalFrames;
                    timer = 0f;
                }
            }
            UpdateFrameDimensions();

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                Shoot();
            }

            foreach (var bullet in bullets)
            {
                bullet.Update(gameTime);
                foreach (var enemy in enemies)
                {
                    if (bullet.IsActive && enemy.Bounds.Intersects(new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, 10, 10)))
                    {
                        enemy.TakeDamage(bullet.Damage);
                        bullet.Deactivate();
                    }
                }
            }
            bullets.RemoveAll(b => !b.IsActive);
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            if (health <= 0)
            {
                // Handle player death (respawn or game over)
            }
        }

        private void Shoot()
        {
            Vector2 bulletDirection = Vector2.Zero;
            Texture2D bulletTexture = bulletHorizontalTexture;
            SpriteEffects spriteEffects = SpriteEffects.None;

            switch (currentDirection)
            {
                case Direction.Up:
                    bulletDirection = new Vector2(0, -1);
                    bulletTexture = bulletVerticalTexture;
                    break;
                case Direction.Down:
                    bulletDirection = new Vector2(0, 1);
                    bulletTexture = bulletVerticalTexture;
                    spriteEffects = SpriteEffects.FlipVertically;
                    break;
                case Direction.Left:
                    bulletDirection = new Vector2(-1, 0);
                    bulletTexture = bulletHorizontalTexture;
                    spriteEffects = SpriteEffects.FlipHorizontally;
                    break;
                case Direction.Right:
                    bulletDirection = new Vector2(1, 0);
                    bulletTexture = bulletHorizontalTexture;
                    break;
            }

            float chestOffset = frameHeight / 2f;
            Vector2 bulletPosition = Position + new Vector2(frameWidth / 2, chestOffset);
            bullets.Add(new Bullet(bulletTexture, bulletPosition, bulletDirection, bulletSpeed, bulletDamage, spriteEffects));
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

            spriteBatch.Draw(currentTexture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
            foreach (var bullet in bullets)
            {
                bullet.Draw(spriteBatch);
            }
        }

        private void UpdateFrameDimensions()
        {
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
    }
}

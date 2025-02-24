using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class Player
    {
        // Textures for idle (walking) and attack animations.
        private Texture2D idleUpTexture, idleDownTexture, idleLeftTexture, idleRightTexture;
        private Texture2D attackUpTexture, attackDownTexture, attackLeftTexture, attackRightTexture;

        // Bullet textures.
        private Texture2D bulletHorizontalTexture, bulletVerticalTexture;

        // Player position.
        public Vector2 Position;

        // Player parameters.
        public float MovementSpeed { get; set; } = 200f;
        public float FiringInterval { get; set; } = 1.0f; // seconds between shots
        public float BulletRange { get; set; } = 500f;    // maximum bullet travel distance

        private int health = 100;
        public int Health => health;

        // Bullet damage field.
        private int bulletDamage = 10;

        // Animation variables.
        private float frameTime = 0.1f; // seconds per frame
        private float timer = 0f;
        private int currentFrame = 0;
        private int totalFrames = 4; // assume 4-frame animation
        private int frameWidth;
        private int frameHeight;
        private Rectangle sourceRectangle;

        // Shooting timer.
        private float timeSinceLastShot = 0f;

        // List of bullets fired.
        private List<Bullet> bullets;
        private float bulletSpeed = 500f;

        // Direction for animations.
        private enum Direction { Up, Down, Left, Right }
        private Direction currentDirection = Direction.Down;

        // Public Scale property.
        public float Scale { get; set; } = 1f;

        // Public collision bounds.
        public Rectangle Bounds
        {
            get { return new Rectangle((int)Position.X, (int)Position.Y, frameWidth, frameHeight); }
        }

        // Reference to player stats.
        private PlayerStats stats;

        /// <summary>
        /// Constructs a new Player.
        /// Expects:
        /// 4 idle textures (for walking/idle animation),
        /// 4 attack textures,
        /// 2 bullet textures,
        /// a starting position, and a PlayerStats object.
        /// </summary>
        public Player(
            Texture2D idleUp, Texture2D idleDown, Texture2D idleLeft, Texture2D idleRight,
            Texture2D attackUp, Texture2D attackDown, Texture2D attackLeft, Texture2D attackRight,
            Texture2D bulletHorizontal, Texture2D bulletVertical,
            Vector2 startPosition, PlayerStats stats)
        {
            idleUpTexture = idleUp;
            idleDownTexture = idleDown;
            idleLeftTexture = idleLeft;
            idleRightTexture = idleRight;

            attackUpTexture = attackUp;
            attackDownTexture = attackDown;
            attackLeftTexture = attackLeft;
            attackRightTexture = attackRight;

            bulletHorizontalTexture = bulletHorizontal;
            bulletVerticalTexture = bulletVertical;

            Position = startPosition;
            this.stats = stats;

            // Initialize using idleDown as default.
            frameWidth = idleDownTexture.Width / totalFrames;
            frameHeight = idleDownTexture.Height;
            sourceRectangle = new Rectangle(0, 0, frameWidth, frameHeight);

            bullets = new List<Bullet>();
        }

        public void Update(GameTime gameTime, Viewport viewport, List<Enemy> enemies)
        {
            KeyboardState keyboard = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            // Movement input.
            if (keyboard.IsKeyDown(Keys.W))
            {
                movement.Y -= MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Up;
            }
            if (keyboard.IsKeyDown(Keys.S))
            {
                movement.Y += MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Down;
            }
            if (keyboard.IsKeyDown(Keys.A))
            {
                movement.X -= MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Left;
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                movement.X += MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                currentDirection = Direction.Right;
            }

            Position += movement;
            Position.X = MathHelper.Clamp(Position.X, 0, viewport.Width - frameWidth);
            Position.Y = MathHelper.Clamp(Position.Y, 0, viewport.Height - frameHeight);

            // Handle shooting: if Space is pressed, fire one bullet; if held down, fire continuously.
            timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboard.IsKeyDown(Keys.Space) && timeSinceLastShot >= FiringInterval)
            {
                Shoot();
                timeSinceLastShot = 0f;
            }

            // Update animation.
            if (movement != Vector2.Zero)
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
                currentFrame = 0;
            }
            int sourceX = currentFrame * frameWidth;
            sourceRectangle = new Rectangle(sourceX, 0, frameWidth, frameHeight);

            // Update bullets.
            foreach (var bullet in bullets)
            {
                bullet.Update(gameTime);
            }
            bullets.RemoveAll(b => !b.IsActive);
        }

        private void Shoot()
        {
            Vector2 direction = Vector2.Zero;
            switch (currentDirection)
            {
                case Direction.Up: direction = new Vector2(0, -1); break;
                case Direction.Down: direction = new Vector2(0, 1); break;
                case Direction.Left: direction = new Vector2(-1, 0); break;
                case Direction.Right: direction = new Vector2(1, 0); break;
            }

            // Calculate bullet start position (center of player).
            Vector2 bulletPos = Position + new Vector2(frameWidth / 2, frameHeight / 2);
            Texture2D chosenBullet = (currentDirection == Direction.Left || currentDirection == Direction.Right)
                ? bulletHorizontalTexture
                : bulletVerticalTexture;

            bullets.Add(new Bullet(chosenBullet, bulletPos, direction, bulletSpeed, bulletDamage, SpriteEffects.None, BulletRange));
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            Debug.WriteLine($"Player took {amount} damage. Health now: {health}");
            if (health <= 0)
            {
                Debug.WriteLine("Player died!");
                // Implement respawn or game over logic.
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D currentTexture;
            SpriteEffects effect = SpriteEffects.None;
            // Use attack textures if Space is pressed; else idle textures.
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Space))
            {
                switch (currentDirection)
                {
                    case Direction.Up: currentTexture = attackUpTexture; break;
                    case Direction.Down: currentTexture = attackDownTexture; break;
                    case Direction.Left: currentTexture = attackLeftTexture; break;
                    case Direction.Right: currentTexture = attackRightTexture; effect = SpriteEffects.FlipHorizontally; break;
                    default: currentTexture = attackDownTexture; break;
                }
            }
            else
            {
                switch (currentDirection)
                {
                    case Direction.Up: currentTexture = idleUpTexture; break;
                    case Direction.Down: currentTexture = idleDownTexture; break;
                    case Direction.Left: currentTexture = idleLeftTexture; break;
                    case Direction.Right: currentTexture = idleRightTexture; effect = SpriteEffects.FlipHorizontally; break;
                    default: currentTexture = idleDownTexture; break;
                }
            }

            spriteBatch.Draw(currentTexture, Position, sourceRectangle, Color.White, 0f, Vector2.Zero, Scale, effect, 0f);
            foreach (var bullet in bullets)
            {
                bullet.Draw(spriteBatch);
            }
        }
    }
}

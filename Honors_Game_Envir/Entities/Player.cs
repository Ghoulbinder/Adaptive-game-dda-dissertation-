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
        // Idle textures
        private Texture2D idleUpTexture;
        private Texture2D idleDownTexture;
        private Texture2D idleLeftTexture;
        private Texture2D idleRightTexture;

        // Walk textures
        private Texture2D walkUpTexture;
        private Texture2D walkDownTexture;
        private Texture2D walkLeftTexture;
        private Texture2D walkRightTexture;

        // Attack textures
        private Texture2D attackUpTexture;
        private Texture2D attackDownTexture;
        private Texture2D attackLeftTexture;
        private Texture2D attackRightTexture;

        // Bullet texture for shooting
        private Texture2D bulletTexture;

        // The currently active texture we draw from
        private Texture2D activeTexture;

        // Player properties
        public Vector2 Position;
        public float MovementSpeed { get; set; } = 200f;
        public float FiringInterval { get; set; } = 1.0f; // seconds between shots
        public float BulletRange { get; set; } = 500f;    // maximum bullet travel distance

        private int health = 100;
        private int bulletDamage = 10;
        public int Health => health;

        // Scaling factor to draw the player smaller
        public float Scale { get; set; } = 0.5f;

        // Animation fields
        private float frameTime = 0.1f;   // time per frame
        private float timer = 0f;         // accumulates time
        private int frameIndex = 0;       // 0..23
        private const int framesPerRow = 6;
        private const int rows = 4;       // 24 total frames

        private int frameWidth;
        private int frameHeight;
        private Rectangle sourceRectangle;

        // Bullets
        private List<Bullet> bullets;
        private float bulletSpeed = 500f;
        private float timeSinceLastShot = 0f;

        // Directions and states
        private enum Direction { Up, Down, Left, Right }
        private Direction currentDirection = Direction.Down;

        private enum PlayerState { Idle, Walk, Attack }
        private PlayerState currentState = PlayerState.Idle;

        // Attack duration
        private float attackDuration = 0.5f; // half second
        private float attackTimer = 0f;

        // Collision bounds (scaled)
        public Rectangle Bounds
        {
            get
            {
                int scaledW = (int)(frameWidth * Scale);
                int scaledH = (int)(frameHeight * Scale);
                return new Rectangle((int)Position.X, (int)Position.Y, scaledW, scaledH);
            }
        }

        /// <summary>
        /// Constructs a new Player with 12 sprite sheets:
        ///   Idle (Up,Down,Left,Right),
        ///   Walk (Up,Down,Left,Right),
        ///   Attack (Up,Down,Left,Right),
        /// plus a bullet texture.
        /// Each sheet is 1536×1024, 6 frames wide, 4 rows.
        /// </summary>
        public Player(
            Texture2D idleUp, Texture2D idleDown, Texture2D idleLeft, Texture2D idleRight,
            Texture2D walkUp, Texture2D walkDown, Texture2D walkLeft, Texture2D walkRight,
            Texture2D attackUp, Texture2D attackDown, Texture2D attackLeft, Texture2D attackRight,
            Texture2D bulletTexture,
            Vector2 startPosition
        )
        {
            // Assign textures
            idleUpTexture = idleUp;
            idleDownTexture = idleDown;
            idleLeftTexture = idleLeft;
            idleRightTexture = idleRight;

            walkUpTexture = walkUp;
            walkDownTexture = walkDown;
            walkLeftTexture = walkLeft;
            walkRightTexture = walkRight;

            attackUpTexture = attackUp;
            attackDownTexture = attackDown;
            attackLeftTexture = attackLeft;
            attackRightTexture = attackRight;

            this.bulletTexture = bulletTexture;
            Position = startPosition;

            // Default to Idle Down
            activeTexture = idleDownTexture;
            currentDirection = Direction.Down;
            currentState = PlayerState.Idle;

            // Frame dimensions from the active texture
            frameWidth = activeTexture.Width / framesPerRow;   // 1536/6=256
            frameHeight = activeTexture.Height / rows;         // 1024/4=256

            sourceRectangle = new Rectangle(0, 0, frameWidth, frameHeight);
            bullets = new List<Bullet>();
        }

        public void Update(GameTime gameTime, Viewport viewport, List<Enemy> enemies)
        {
            var keyboard = Keyboard.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 movement = Vector2.Zero;
            bool isMoving = false;

            // Movement input (WASD)
            if (keyboard.IsKeyDown(Keys.W))
            {
                movement.Y -= MovementSpeed * delta;
                currentDirection = Direction.Up;
                isMoving = true;
            }
            else if (keyboard.IsKeyDown(Keys.S))
            {
                movement.Y += MovementSpeed * delta;
                currentDirection = Direction.Down;
                isMoving = true;
            }
            else if (keyboard.IsKeyDown(Keys.A))
            {
                movement.X -= MovementSpeed * delta;
                currentDirection = Direction.Left;
                isMoving = true;
            }
            else if (keyboard.IsKeyDown(Keys.D))
            {
                movement.X += MovementSpeed * delta;
                currentDirection = Direction.Right;
                isMoving = true;
            }

            // Attack input
            bool isAttacking = keyboard.IsKeyDown(Keys.Space);

            // Update position
            Position += movement;
            float maxX = viewport.Width - (frameWidth * Scale);
            float maxY = viewport.Height - (frameHeight * Scale);
            Position.X = MathHelper.Clamp(Position.X, 0, maxX);
            Position.Y = MathHelper.Clamp(Position.Y, 0, maxY);

            // Player state machine
            if (isAttacking)
            {
                currentState = PlayerState.Attack;
                attackTimer = 0f;
            }
            else if (isMoving)
            {
                if (currentState != PlayerState.Attack)
                    currentState = PlayerState.Walk;
            }
            else
            {
                // Not attacking, not moving => Idle
                if (currentState != PlayerState.Attack)
                    currentState = PlayerState.Idle;
            }

            // If in Attack, count down
            if (currentState == PlayerState.Attack)
            {
                attackTimer += delta;
                if (attackTimer >= attackDuration)
                {
                    // Attack ends
                    if (isMoving) currentState = PlayerState.Walk;
                    else currentState = PlayerState.Idle;
                }
            }

            // Choose active texture based on direction + state
            switch (currentState)
            {
                case PlayerState.Idle:
                    switch (currentDirection)
                    {
                        case Direction.Up: activeTexture = idleUpTexture; break;
                        case Direction.Down: activeTexture = idleDownTexture; break;
                        case Direction.Left: activeTexture = idleLeftTexture; break;
                        case Direction.Right: activeTexture = idleRightTexture; break;
                    }
                    break;

                case PlayerState.Walk:
                    switch (currentDirection)
                    {
                        case Direction.Up: activeTexture = walkUpTexture; break;
                        case Direction.Down: activeTexture = walkDownTexture; break;
                        case Direction.Left: activeTexture = walkLeftTexture; break;
                        case Direction.Right: activeTexture = walkRightTexture; break;
                    }
                    break;

                case PlayerState.Attack:
                    switch (currentDirection)
                    {
                        case Direction.Up: activeTexture = attackUpTexture; break;
                        case Direction.Down: activeTexture = attackDownTexture; break;
                        case Direction.Left: activeTexture = attackLeftTexture; break;
                        case Direction.Right: activeTexture = attackRightTexture; break;
                    }
                    break;
            }

            // Animate if walking or attacking
            if (currentState == PlayerState.Walk || currentState == PlayerState.Attack)
            {
                timer += delta;
                if (timer >= frameTime)
                {
                    frameIndex = (frameIndex + 1) % (framesPerRow * rows);
                    timer = 0f;
                }
            }
            else
            {
                // If Idle, show frame 0
                frameIndex = 0;
            }

            // Update frame dimensions (texture can change if we switch states)
            frameWidth = activeTexture.Width / framesPerRow;
            frameHeight = activeTexture.Height / rows;

            // Compute row/col in row-major order
            int rowIndex = frameIndex / framesPerRow;
            int colIndex = frameIndex % framesPerRow;
            int x = colIndex * frameWidth;
            int y = rowIndex * frameHeight;
            sourceRectangle = new Rectangle(x, y, frameWidth, frameHeight);

            // Attack => shoot bullets
            timeSinceLastShot += delta;
            if (currentState == PlayerState.Attack && timeSinceLastShot >= FiringInterval)
            {
                Shoot();
                timeSinceLastShot = 0f;
            }

            // Update bullets
            foreach (var bullet in bullets)
            {
                bullet.Update(gameTime);
                foreach (var enemy in enemies)
                {
                    if (bullet.IsActive && enemy.Bounds.Intersects(bullet.Bounds))
                    {
                        enemy.TakeDamage(bullet.Damage);
                        bullet.Deactivate();
                    }
                }
            }
            bullets.RemoveAll(b => !b.IsActive);
        }

        private void Shoot()
        {
            // For simplicity, always shoot to the right
            Vector2 bulletDirection = new Vector2(1, 0);

            // Start bullet near the player's middle
            float offsetY = (frameHeight * Scale) / 2f;
            Vector2 bulletPosition = Position + new Vector2((frameWidth * Scale) / 2, offsetY);

            bullets.Add(new Bullet(
                bulletTexture,
                bulletPosition,
                bulletDirection,
                bulletSpeed,
                bulletDamage,
                SpriteEffects.None,
                BulletRange
            ));
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            Debug.WriteLine($"Player took {amount} damage. Health now: {health}");
            if (health <= 0)
            {
                Debug.WriteLine("Player has died!");
                // You could set a PlayerState.Death or handle respawn.
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the active texture at the current frame, scaled
            spriteBatch.Draw(
                activeTexture,
                Position,
                sourceRectangle,
                Color.White,
                0f,
                Vector2.Zero,
                Scale,
                SpriteEffects.None,
                0f
            );

            // Draw bullets
            foreach (var bullet in bullets)
            {
                bullet.Draw(spriteBatch);
            }
        }
    }
}

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
        // --------------------------
        // Texture fields.
        // --------------------------
        // Idle
        private Texture2D idleUpTexture;
        private Texture2D idleDownTexture;
        private Texture2D idleLeftTexture;
        private Texture2D idleRightTexture;

        // Walk
        private Texture2D walkUpTexture;
        private Texture2D walkDownTexture;
        private Texture2D walkLeftTexture;
        private Texture2D walkRightTexture;

        // Attack
        private Texture2D attackUpTexture;
        private Texture2D attackDownTexture;
        private Texture2D attackLeftTexture;
        private Texture2D attackRightTexture;

        // Bullet textures.
        private Texture2D bulletTexture1; // used for left/right
        private Texture2D bulletTexture2; // used for up/down

        // Active texture.
        private Texture2D activeTexture;

        // --------------------------
        // Player stats.
        // --------------------------
        public PlayerStats Stats { get; private set; }
        public Vector2 Position;
        public float FiringInterval { get { return 1f / Stats.AttackSpeed; } }
        public float BulletRange { get; set; } = 500f;
        public int Health { get { return Stats.Health; } }

        public float Scale { get; set; } = 0.3f;

        // --------------------------
        // Animation fields.
        // --------------------------
        private float idleFrameTime = 0.15f;
        private float walkFrameTime = 0.06f;
        private float baseAttackFrameTime = 0.04f; // will be divided by AttackSpeed
        private float timer = 0f;
        private int frameIndex = 0;
        private const int framesPerRow = 6;
        private const int rows = 4;
        private int frameWidth;
        private int frameHeight;
        private Rectangle sourceRectangle;

        // --------------------------
        // Bullets.
        // --------------------------
        private List<Bullet> bullets;
        private float bulletSpeed = 500f;

        // --------------------------
        // Direction and state.
        // --------------------------
        private enum Direction { Up, Down, Left, Right }
        private Direction currentDirection = Direction.Down;
        private enum PlayerState { Idle, Walk, Attack }
        private PlayerState currentState = PlayerState.Idle;

        /// <summary>
        /// Constructs a new Player using 12 sprite sheets, two bullet textures, starting position, and optional PlayerStats.
        /// </summary>
        public Player(
            Texture2D idleUp, Texture2D idleDown, Texture2D idleLeft, Texture2D idleRight,
            Texture2D walkUp, Texture2D walkDown, Texture2D walkLeft, Texture2D walkRight,
            Texture2D attackUp, Texture2D attackDown, Texture2D attackLeft, Texture2D attackRight,
            Texture2D bulletTexture1, Texture2D bulletTexture2,
            Vector2 startPosition,
            PlayerStats stats = null)
        {
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

            this.bulletTexture1 = bulletTexture1;
            this.bulletTexture2 = bulletTexture2;
            Position = startPosition;

            activeTexture = idleDownTexture;
            currentDirection = Direction.Down;
            currentState = PlayerState.Idle;

            Stats = stats ?? new PlayerStats(100, 3, 10, 1.0f, 200f, 0, 1, null);

            frameWidth = activeTexture.Width / framesPerRow;
            frameHeight = activeTexture.Height / rows;
            sourceRectangle = new Rectangle(0, 0, frameWidth, frameHeight);
            bullets = new List<Bullet>();
        }

        // Public Bounds property.
        public Rectangle Bounds
        {
            get
            {
                int scaledW = (int)(frameWidth * Scale);
                int scaledH = (int)(frameHeight * Scale);
                return new Rectangle((int)Position.X, (int)Position.Y, scaledW, scaledH);
            }
        }

        public void Update(GameTime gameTime, Viewport viewport, List<Enemy> enemies)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboard = Keyboard.GetState();

            Vector2 movement = Vector2.Zero;
            bool isMoving = false;

            // Movement input.
            if (keyboard.IsKeyDown(Keys.W))
            {
                movement.Y -= Stats.MovementSpeed * delta;
                currentDirection = Direction.Up;
                isMoving = true;
            }
            else if (keyboard.IsKeyDown(Keys.S))
            {
                movement.Y += Stats.MovementSpeed * delta;
                currentDirection = Direction.Down;
                isMoving = true;
            }
            else if (keyboard.IsKeyDown(Keys.A))
            {
                movement.X -= Stats.MovementSpeed * delta;
                currentDirection = Direction.Left;
                isMoving = true;
            }
            else if (keyboard.IsKeyDown(Keys.D))
            {
                movement.X += Stats.MovementSpeed * delta;
                currentDirection = Direction.Right;
                isMoving = true;
            }

            // Attack input.
            bool isAttacking = keyboard.IsKeyDown(Keys.Space);

            Position += movement;
            float maxX = viewport.Width - (frameWidth * Scale);
            float maxY = viewport.Height - (frameHeight * Scale);
            Position.X = MathHelper.Clamp(Position.X, 0, maxX);
            Position.Y = MathHelper.Clamp(Position.Y, 0, maxY);

            // State transitions.
            if (currentState != PlayerState.Attack)
            {
                if (isAttacking)
                {
                    currentState = PlayerState.Attack;
                    frameIndex = 0;
                }
                else if (isMoving)
                {
                    currentState = PlayerState.Walk;
                }
                else
                {
                    currentState = PlayerState.Idle;
                    frameIndex = 0;
                    timer = 0f;
                }
            }

            // Set active texture.
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

            // Choose frame time.
            float currentFrameTime = 0f;
            switch (currentState)
            {
                case PlayerState.Idle: currentFrameTime = idleFrameTime; break;
                case PlayerState.Walk: currentFrameTime = walkFrameTime; break;
                case PlayerState.Attack: currentFrameTime = baseAttackFrameTime / Stats.AttackSpeed; break;
            }

            if (currentState != PlayerState.Idle)
            {
                timer += delta;
                if (timer >= currentFrameTime)
                {
                    frameIndex = (frameIndex + 1) % (framesPerRow * rows);
                    timer = 0f;
                    if (currentState == PlayerState.Attack && frameIndex == (framesPerRow * rows) - 1)
                    {
                        Shoot();
                        frameIndex = 0;
                        currentState = isMoving ? PlayerState.Walk : PlayerState.Idle;
                    }
                }
            }

            // Recalculate frame dimensions.
            frameWidth = activeTexture.Width / framesPerRow;
            frameHeight = activeTexture.Height / rows;
            int rowIdx = frameIndex / framesPerRow;
            int colIdx = frameIndex % framesPerRow;
            int sx = colIdx * frameWidth;
            int sy = rowIdx * frameHeight;
            sourceRectangle = new Rectangle(sx, sy, frameWidth, frameHeight);

            // Update bullets.
            foreach (var bullet in bullets)
            {
                bullet.Update(gameTime);
                foreach (var enemy in enemies)
                {
                    if (bullet.IsActive && enemy.Bounds.Intersects(bullet.Bounds))
                    {
                        enemy.TakeDamage(bullet.Damage);
                        bullet.Deactivate();
                        Stats.IncreaseExperience(10);
                    }
                }
            }
            bullets.RemoveAll(b => !b.IsActive);
        }

        private void Shoot()
        {
            Vector2 bulletDirection;
            Vector2 bulletOrigin;
            SpriteEffects effects = SpriteEffects.None;

            // Adjust bullet spawn based on direction.
            switch (currentDirection)
            {
                case Direction.Up:
                    bulletDirection = new Vector2(0, -1);
                    bulletOrigin = new Vector2(Position.X + (frameWidth * Scale) / 2,
                                                 Position.Y + (frameHeight * Scale) * 0.3f);
                    effects = SpriteEffects.None;
                    break;
                case Direction.Down:
                    bulletDirection = new Vector2(0, 1);
                    bulletOrigin = new Vector2(Position.X + (frameWidth * Scale) / 2,
                                                 Position.Y + (frameHeight * Scale) * 0.3f);
                    effects = SpriteEffects.FlipVertically;
                    break;
                case Direction.Left:
                    bulletDirection = new Vector2(-1, 0);
                    bulletOrigin = new Vector2(Position.X + (frameWidth * Scale) * 0.3f,
                                                 Position.Y + (frameHeight * Scale) / 2);
                    effects = SpriteEffects.None;
                    break;
                case Direction.Right:
                    bulletDirection = new Vector2(1, 0);
                    bulletOrigin = new Vector2(Position.X + (frameWidth * Scale) * 0.7f,
                                                 Position.Y + (frameHeight * Scale) / 2);
                    effects = SpriteEffects.FlipHorizontally;
                    break;
                default:
                    bulletDirection = new Vector2(1, 0);
                    bulletOrigin = Position + new Vector2((frameWidth * Scale) / 2, (frameHeight * Scale) / 2);
                    effects = SpriteEffects.None;
                    break;
            }

            // Use bulletTexture2 for Up/Down, bulletTexture1 for Left/Right.
            Texture2D chosenBulletTexture = (currentDirection == Direction.Up || currentDirection == Direction.Down)
                                              ? bulletTexture2
                                              : bulletTexture1;

            bullets.Add(new Bullet(
                chosenBulletTexture,
                bulletOrigin,
                bulletDirection,
                bulletSpeed,
                Stats.AttackDamage,
                effects,
                BulletRange
            ));
        }

        public void TakeDamage(int amount)
        {
            Stats.Health -= amount;
            Debug.WriteLine($"Player took {amount} damage. Health now: {Stats.Health}");
            if (Stats.Health <= 0)
            {
                Stats.Lives--;
                Debug.WriteLine($"Player lost a life. Lives remaining: {Stats.Lives}");
                if (Stats.Lives > 0)
                {
                    Stats.Health = 100; // Reset health (respawn logic to be handled externally)
                }
                else
                {
                    Debug.WriteLine("Game Over!");
                    // Trigger game over logic.
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
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
            foreach (var bullet in bullets)
            {
                bullet.Draw(spriteBatch);
            }
        }
    }
}

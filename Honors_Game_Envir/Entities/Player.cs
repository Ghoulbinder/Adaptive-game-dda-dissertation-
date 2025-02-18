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
        // Walk textures (used for both idle and movement)
        private Texture2D walkUpTexture, walkDownTexture, walkLeftTexture, walkRightTexture;
        // Attack textures
        private Texture2D attackUpTexture, attackDownTexture, attackLeftTexture, attackRightTexture;
        // Bullet textures
        private Texture2D bulletHorizontalTexture, bulletVerticalTexture;

        // The currently active texture (for animation)
        private Texture2D activeTexture;
        // Position represents the center of the sprite.
        public Vector2 Position;
        public float Scale { get; set; } = 0.3f;
        public float MovementSpeed { get; set; } = 200f;
        // The interval between shots (in seconds)
        public float FiringInterval { get; set; } = 0.8f;
        // Maximum bullet travel distance
        public float BulletRange { get; set; } = 1500f;
        private int bulletDamage = 10;
        private PlayerStats stats;

        // Animation fields (all sheets are 1536x1024: 6 columns x 4 rows)
        private float frameTime = 0.1f;
        private float timer = 0f;
        private int frameIndex = 0;
        private const int framesPerRow = 6;
        private const int rows = 4;
        private int totalFrames => framesPerRow * rows;
        private Texture2D currentSheet;
        private Rectangle sourceRectangle;

        // Define the two states: Walk (which also serves as idle) and Attack.
        private enum Direction { Up, Down, Left, Right }
        private enum PlayerState { Walk, Attack }
        private Direction currentDirection = Direction.Down;
        private PlayerState currentState = PlayerState.Walk;

        // Timer for continuous fire when Space is held down.
        private float attackCycleTimer = 0f;

        // List to hold fired bullets.
        private List<Bullet> bullets;

        // For detecting key presses.
        private KeyboardState prevKeyboardState;

        // Expose a center-based bounding box.
        public Rectangle Bounds
        {
            get
            {
                int sheetWidth = currentSheet.Width;
                int sheetHeight = currentSheet.Height;
                int frameW = sheetWidth / framesPerRow;
                int frameH = sheetHeight / rows;
                float scaledW = frameW * Scale;
                float scaledH = frameH * Scale;
                return new Rectangle((int)(Position.X - scaledW / 2), (int)(Position.Y - scaledH / 2), (int)scaledW, (int)scaledH);
            }
        }

        // Expose current health.
        public int Health => stats.Health;

        /// <summary>
        /// Constructs a new Player.
        /// Parameters (in order):
        /// - 4 walk textures (Up, Down, Left, Right)
        /// - 4 attack textures (Up, Down, Left, Right)
        /// - 2 bullet textures (horizontal and vertical)
        /// - A start position (Vector2)
        /// - A PlayerStats instance
        /// Total: 16 arguments.
        /// </summary>
        public Player(
            Texture2D walkUp, Texture2D walkDown, Texture2D walkLeft, Texture2D walkRight,
            Texture2D attackUp, Texture2D attackDown, Texture2D attackLeft, Texture2D attackRight,
            Texture2D bulletHorizontal, Texture2D bulletVertical,
            Vector2 startPosition,
            PlayerStats stats)
        {
            walkUpTexture = walkUp;
            walkDownTexture = walkDown;
            walkLeftTexture = walkLeft;
            walkRightTexture = walkRight;

            attackUpTexture = attackUp;
            attackDownTexture = attackDown;
            attackLeftTexture = attackLeft;
            attackRightTexture = attackRight;

            bulletHorizontalTexture = bulletHorizontal;
            bulletVerticalTexture = bulletVertical;

            Position = startPosition;
            this.stats = stats;
            bullets = new List<Bullet>();

            // Start with Walk state (used as idle) facing Down.
            currentDirection = Direction.Down;
            currentState = PlayerState.Walk;
            activeTexture = walkDownTexture;
            currentSheet = walkDownTexture;

            prevKeyboardState = Keyboard.GetState();
        }

        public void TakeDamage(int amount)
        {
            stats.Health -= amount;
            Debug.WriteLine($"Player took {amount} damage. Health now: {stats.Health}");
            if (stats.Health <= 0)
            {
                Debug.WriteLine("Player has died!");
                // Add game-over or respawn logic here.
            }
        }

        public void Update(GameTime gameTime, Viewport viewport, List<Enemy> enemies)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 movement = Vector2.Zero;
            KeyboardState keyboard = Keyboard.GetState();

            // --- Movement Input ---
            if (keyboard.IsKeyDown(Keys.W))
            {
                movement.Y -= MovementSpeed * delta;
                currentDirection = Direction.Up;
            }
            else if (keyboard.IsKeyDown(Keys.S))
            {
                movement.Y += MovementSpeed * delta;
                currentDirection = Direction.Down;
            }
            else if (keyboard.IsKeyDown(Keys.A))
            {
                movement.X -= MovementSpeed * delta;
                currentDirection = Direction.Left;
            }
            else if (keyboard.IsKeyDown(Keys.D))
            {
                movement.X += MovementSpeed * delta;
                currentDirection = Direction.Right;
            }

            // --- Firing Input ---
            // If Space is pressed (or held), fire immediately on press and then continuously at the firing interval.
            if (keyboard.IsKeyDown(Keys.Space))
            {
                // If space was just pressed, fire immediately.
                if (!prevKeyboardState.IsKeyDown(Keys.Space))
                {
                    Shoot();
                    attackCycleTimer = 0f;
                }
                else
                {
                    attackCycleTimer += delta;
                    if (attackCycleTimer >= FiringInterval)
                    {
                        Shoot();
                        attackCycleTimer -= FiringInterval;
                    }
                }
                currentState = PlayerState.Attack;
            }
            else
            {
                // When Space is released, return to Walk state.
                if (currentState == PlayerState.Attack)
                {
                    currentState = PlayerState.Walk;
                    attackCycleTimer = 0f;
                }
            }

            // --- Update Position ---
            Position += movement;
            int sheetWidth = currentSheet.Width;
            int sheetHeight = currentSheet.Height;
            int frameW = sheetWidth / framesPerRow;
            int frameH = sheetHeight / rows;
            float halfW = (frameW * Scale) / 2f;
            float halfH = (frameH * Scale) / 2f;
            Position.X = MathHelper.Clamp(Position.X, halfW, viewport.Width - halfW);
            Position.Y = MathHelper.Clamp(Position.Y, halfH, viewport.Height - halfH);

            // --- Select Active Texture Based on State and Direction ---
            if (currentState == PlayerState.Walk)
            {
                switch (currentDirection)
                {
                    case Direction.Up: activeTexture = walkUpTexture; break;
                    case Direction.Down: activeTexture = walkDownTexture; break;
                    case Direction.Left: activeTexture = walkLeftTexture; break;
                    case Direction.Right: activeTexture = walkRightTexture; break;
                }
            }
            else if (currentState == PlayerState.Attack)
            {
                switch (currentDirection)
                {
                    case Direction.Up: activeTexture = attackUpTexture; break;
                    case Direction.Down: activeTexture = attackDownTexture; break;
                    case Direction.Left: activeTexture = attackLeftTexture; break;
                    case Direction.Right: activeTexture = attackRightTexture; break;
                }
            }
            currentSheet = activeTexture;

            // --- Animation Update ---
            if (currentState == PlayerState.Walk || currentState == PlayerState.Attack)
            {
                timer += delta;
                if (timer >= frameTime)
                {
                    frameIndex = (frameIndex + 1) % totalFrames;
                    timer = 0f;
                }
            }
            else
            {
                frameIndex = 0;
            }
            int col = frameIndex % framesPerRow;
            int row = frameIndex / framesPerRow;
            sourceRectangle = new Rectangle(col * frameW, row * frameH, frameW, frameH);

            // --- Update Bullets ---
            foreach (var bullet in bullets)
            {
                bullet.Update(gameTime);
                foreach (var enemy in enemies)
                {
                    if (bullet.IsActive && enemy.Bounds.Intersects(bullet.Bounds))
                    {
                        enemy.TakeDamage(bulletDamage);
                        bullet.Deactivate();
                    }
                }
            }
            bullets.RemoveAll(b => !b.IsActive);

            // Save current keyboard state.
            prevKeyboardState = keyboard;
        }

        private void Shoot()
        {
            // Determine bullet texture, direction, and sprite effects based on player's facing direction.
            Texture2D bulletToUse = null;
            SpriteEffects effect = SpriteEffects.None;
            Vector2 bulletDirection = Vector2.Zero;
            switch (currentDirection)
            {
                case Direction.Up:
                    bulletDirection = new Vector2(0, -1);
                    bulletToUse = bulletVerticalTexture;
                    break;
                case Direction.Down:
                    bulletDirection = new Vector2(0, 1);
                    bulletToUse = bulletVerticalTexture;
                    effect = SpriteEffects.FlipVertically;
                    break;
                case Direction.Left:
                    bulletDirection = new Vector2(-1, 0);
                    bulletToUse = bulletHorizontalTexture;
                    effect = SpriteEffects.FlipHorizontally;
                    break;
                case Direction.Right:
                    bulletDirection = new Vector2(1, 0);
                    bulletToUse = bulletHorizontalTexture;
                    break;
            }
            // Spawn bullet from the player's center.
            Vector2 bulletPos = Position;
            Bullet newBullet = new Bullet(
                bulletToUse,
                bulletPos,
                bulletDirection,
                500f,
                bulletDamage,
                effect,
                BulletRange
            );
            bullets.Add(newBullet);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int sheetW = currentSheet.Width;
            int sheetH = currentSheet.Height;
            int frameW = sheetW / framesPerRow;
            int frameH = sheetH / rows;
            int col = frameIndex % framesPerRow;
            int row = frameIndex / framesPerRow;
            Rectangle srcRect = new Rectangle(col * frameW, row * frameH, frameW, frameH);
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

            spriteBatch.Draw(currentSheet, Position, srcRect, Color.White, 0f, origin, Scale, SpriteEffects.None, 0f);

            foreach (var bullet in bullets)
            {
                bullet.Draw(spriteBatch);
            }
        }
    }
}

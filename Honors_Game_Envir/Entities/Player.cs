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
        // Walk textures (used as idle as well)
        private Texture2D walkUpTexture, walkDownTexture, walkLeftTexture, walkRightTexture;
        // Attack textures
        private Texture2D attackUpTexture, attackDownTexture, attackLeftTexture, attackRightTexture;

        // Bullet textures
        private Texture2D bulletHorizontalTexture, bulletVerticalTexture;

        // The currently active spritesheet.
        private Texture2D activeTexture;

        // Center-based position; Position represents the center of the sprite.
        public Vector2 Position;
        public float Scale { get; set; } = 0.3f;
        public float MovementSpeed { get; set; } = 200f;
        // Full duration of an attack cycle (in seconds)
        public float FiringInterval { get; set; } = 0.8f;
        // Increased bullet range so that bullets remain visible until they hit the boss.
        public float BulletRange { get; set; } = 1500f;

        // Independent timer for the attack cycle.
        private float attackCycleTimer = 0f;
        // Flag to ensure the bullet is fired only once per attack cycle.
        private bool hasFiredAttack = false;

        private int bulletDamage = 10;

        // Animation fields: each spritesheet is 1536x1024 (6 columns x 4 rows)
        private float frameTime = 0.1f;
        private float timer = 0f;
        private int frameIndex = 0;
        private const int framesPerRow = 6;
        private const int rows = 4;
        private int totalFrames => framesPerRow * rows;
        private Texture2D currentSheet;

        // We use two states: Walk (used as idle) and Attack.
        private enum Direction { Up, Down, Left, Right }
        private enum PlayerState { Walk, Attack }
        private Direction currentDirection = Direction.Down;
        private PlayerState currentState = PlayerState.Walk;

        // Reference to player stats.
        private PlayerStats stats;

        // List to track bullets fired by the player.
        private List<Bullet> bullets;

        // Center-based bounding box.
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
                return new Rectangle(
                    (int)(Position.X - scaledW / 2),
                    (int)(Position.Y - scaledH / 2),
                    (int)scaledW,
                    (int)scaledH
                );
            }
        }

        public int Health => stats.Health;

        /// <summary>
        /// Constructs a new Player.
        /// Parameters (in order):
        /// - 4 walk textures (used for both idle and walking)
        /// - 4 attack textures
        /// - 2 bullet textures (horizontal and vertical)
        /// - Start position (Vector2)
        /// - PlayerStats instance
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

            // Default state: Walk (used as idle)
            currentDirection = Direction.Down;
            currentState = PlayerState.Walk;
            activeTexture = walkDownTexture;
            currentSheet = walkDownTexture;
        }

        public void TakeDamage(int amount)
        {
            stats.Health -= amount;
            Debug.WriteLine($"Player took {amount} damage. Health now: {stats.Health}");
            if (stats.Health <= 0)
            {
                Debug.WriteLine("Player has died!");
                // Handle game-over or respawn logic here.
            }
        }

        public void Update(GameTime gameTime, Viewport viewport, List<Enemy> enemies)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 movement = Vector2.Zero;
            var keyboard = Keyboard.GetState();

            // Movement input
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

            bool attacking = keyboard.IsKeyDown(Keys.Space);

            // State transition: Attack if space is pressed; otherwise, Walk.
            if (attacking)
            {
                currentState = PlayerState.Attack;
            }
            else
            {
                currentState = PlayerState.Walk;
                attackCycleTimer = 0f;
                hasFiredAttack = false;
            }

            // Update position (center-based)
            Position += movement;
            int sheetWidth = currentSheet.Width;
            int sheetHeight = currentSheet.Height;
            int frameW = sheetWidth / framesPerRow;
            int frameH = sheetHeight / rows;
            float halfW = (frameW * Scale) / 2f;
            float halfH = (frameH * Scale) / 2f;
            Position.X = MathHelper.Clamp(Position.X, halfW, viewport.Width - halfW);
            Position.Y = MathHelper.Clamp(Position.Y, halfH, viewport.Height - halfH);

            // Select active texture based on state and direction.
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

            // Animate if in Walk or Attack state.
            if (currentState == PlayerState.Walk || currentState == PlayerState.Attack)
            {
                timer += delta;
                if (timer >= frameTime)
                {
                    frameIndex = (frameIndex + 1) % totalFrames;
                    timer = 0f;
                }
            }

            // In Attack state, update attack cycle timer and fire bullet at 75% of the cycle.
            if (currentState == PlayerState.Attack)
            {
                attackCycleTimer += delta;
                if (!hasFiredAttack && attackCycleTimer >= (FiringInterval * 0.75f))
                {
                    Shoot();
                    hasFiredAttack = true;
                }
                if (attackCycleTimer >= FiringInterval)
                {
                    attackCycleTimer = 0f;
                    hasFiredAttack = false;
                }
            }
            else
            {
                attackCycleTimer = 0f;
                hasFiredAttack = false;
            }

            // Update bullets and check for collisions with enemies.
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Update(gameTime);
                foreach (var enemy in enemies)
                {
                    if (bullets[i].IsActive && enemy.Bounds.Intersects(bullets[i].Bounds))
                    {
                        enemy.TakeDamage(bulletDamage);
                        bullets[i].Deactivate();
                    }
                }
            }
            bullets.RemoveAll(b => !b.IsActive);
        }

        private void Shoot()
        {
            Vector2 bulletDirection = Vector2.Zero;
            Texture2D bulletTex = bulletHorizontalTexture;
            SpriteEffects effect = SpriteEffects.None;

            switch (currentDirection)
            {
                case Direction.Up:
                    bulletDirection = new Vector2(0, -1);
                    bulletTex = bulletVerticalTexture;
                    break;
                case Direction.Down:
                    bulletDirection = new Vector2(0, 1);
                    bulletTex = bulletVerticalTexture;
                    effect = SpriteEffects.FlipVertically;
                    break;
                case Direction.Left:
                    bulletDirection = new Vector2(-1, 0);
                    bulletTex = bulletHorizontalTexture;
                    effect = SpriteEffects.FlipHorizontally;
                    break;
                case Direction.Right:
                    bulletDirection = new Vector2(1, 0);
                    bulletTex = bulletHorizontalTexture;
                    break;
            }

            // Spawn bullet from the center of the player's sprite.
            Vector2 bulletPos = Position;

            Bullet newBullet = new Bullet(
                bulletTex,
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

            int row = frameIndex / framesPerRow;
            int col = frameIndex % framesPerRow;
            Rectangle srcRect = new Rectangle(col * frameW, row * frameH, frameW, frameH);
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

            spriteBatch.Draw(
                currentSheet,
                Position,
                srcRect,
                Color.White,
                0f,
                origin,
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

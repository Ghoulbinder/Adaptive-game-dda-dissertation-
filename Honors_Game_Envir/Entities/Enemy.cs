using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class Enemy
    {
        // Textures for movement.
        protected Texture2D backTexture, frontTexture, leftTexture;
        protected Texture2D bulletHorizontalTexture, bulletVerticalTexture;

        public Vector2 Position;
        public float MovementSpeed { get; set; } = 100f;
        public int Health { get; protected set; }
        public int BulletDamage { get; set; } = 5;
        public float FiringInterval { get; set; } = 2f;
        public float BulletRange { get; set; } = 1000f;
        public int CollisionDamage { get; set; } = 15;
        protected float timeSinceLastShot = 0f;
        protected float collisionDamageTimer = 0f;

        // Animation fields.
        protected Rectangle sourceRectangle;
        protected float frameTime = 0.1f;
        protected float timer = 0f;
        protected int currentFrame = 0;
        protected int totalFrames = 4;
        protected List<Bullet> bullets;
        protected float bulletSpeed = 300f;

        public enum EnemyState { Idle, Patrol, Chase, Attack, Dead }
        public enum Direction { Left, Right, Up, Down }
        protected EnemyState currentState;
        protected Direction currentDirection;
        protected const int CollisionPadding = 5;
        protected float shootingRange = 200f;
        protected float chaseRange = 400f;

        public int ExperienceReward { get; set; } = 10;
        private bool experienceAwarded = false;

        // Center-based bounding box.
        public virtual Rectangle Bounds
        {
            get
            {
                int frameW = sourceRectangle.Width;
                int frameH = sourceRectangle.Height;
                return new Rectangle(
                    (int)(Position.X - frameW / 2f),
                    (int)(Position.Y - frameH / 2f),
                    frameW,
                    frameH
                );
            }
        }

        public bool IsDead => isDead;
        protected bool isDead = false;

        // Base stats stored on spawn.
        protected int baseHealth;
        protected int baseDamage;

        public Enemy(
            Texture2D back,
            Texture2D front,
            Texture2D left,
            Texture2D bulletHorizontal,
            Texture2D bulletVertical,
            Vector2 startPosition,
            Direction startDirection,
            int health,
            int bulletDamage)
        {
            // PSEUDOCODE: Store textures for later drawing
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            bulletHorizontalTexture = bulletHorizontal;
            bulletVerticalTexture = bulletVertical;

            // PSEUDOCODE: Initialize position and direction
            Position = startPosition;
            currentDirection = startDirection;

            // PSEUDOCODE: Set initial health and damage
            Health = health;
            BulletDamage = bulletDamage;
            baseHealth = health;
            baseDamage = bulletDamage;

            // PSEUDOCODE: Initialize movement, firing, and animation parameters
            MovementSpeed = 100f;
            bulletSpeed = 300f;
            frameTime = 0.1f;
            FiringInterval = 2f;
            timeSinceLastShot = 0f;
            collisionDamageTimer = 0f;
            currentFrame = 0;
            int frameW = leftTexture.Width / totalFrames;
            sourceRectangle = new Rectangle(0, 0, frameW, leftTexture.Height);

            // PSEUDOCODE: Prepare bullet list and initial state
            bullets = new List<Bullet>();
            currentState = EnemyState.Patrol;
        }

        /// <summary>
        /// Store base health and damage values.
        /// </summary>
        public void SetBaseStats(int health, int damage)
        {
            // PSEUDOCODE: Remember original stats for scaling
            baseHealth = health;
            baseDamage = damage;
        }

        /// <summary>
        /// Applies difficulty modifiers to stats.
        /// </summary>
        public virtual void ApplyDifficultyModifiers()
        {
            // PSEUDOCODE: Scale speed and damage based on current difficulty
            MovementSpeed = 100f * DifficultyManager.Instance.EnemySpeedMultiplier;
            BulletDamage = (int)(baseDamage * DifficultyManager.Instance.EnemyDamageMultiplier);

            // PSEUDOCODE: Cap health to scaled maximum
            int newMaxHealth = (int)(baseHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
            if (Health > newMaxHealth)
                Health = newMaxHealth;
        }

        public virtual void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            // PSEUDOCODE: Skip update if already dead
            if (isDead)
                return;

            // PSEUDOCODE: Apply difficulty-based stat adjustments
            ApplyDifficultyModifiers();

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float distance = Vector2.Distance(Position, playerPosition);

            // PSEUDOCODE: Determine behavior state based on distance
            if (distance <= shootingRange)
                currentState = EnemyState.Attack;
            else if (distance <= chaseRange)
                currentState = EnemyState.Chase;
            else
                currentState = EnemyState.Patrol;

            // PSEUDOCODE: Execute behavior for current state
            switch (currentState)
            {
                case EnemyState.Patrol:
                    Patrol(viewport);
                    break;
                case EnemyState.Chase:
                    ChasePlayer(playerPosition);
                    break;
                case EnemyState.Attack:
                    timeSinceLastShot += delta;
                    if (timeSinceLastShot >= FiringInterval)
                    {
                        Shoot();
                        timeSinceLastShot = 0f;
                    }
                    break;
            }

            // PSEUDOCODE: Advance animation frame
            timer += delta;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }
            UpdateFrameDimensions();

            // PSEUDOCODE: Update bullets and handle collisions with player
            foreach (var bullet in bullets)
            {
                bullet.Update(gameTime);
                if (bullet.IsActive && player.Bounds.Intersects(bullet.Bounds))
                {
                    player.TakeDamage(bullet.Damage);
                    bullet.Deactivate();
                    Game1.Instance.bulletsUsedAgainstEnemiesThisSession++;
                }
            }
            bullets.RemoveAll(b => !b.IsActive);
        }

        protected virtual void UpdateFrameDimensions()
        {
            // PSEUDOCODE: Choose correct texture for current direction
            int textureWidth = 0, textureHeight = 0;
            switch (currentDirection)
            {
                case Direction.Up:
                    textureWidth = backTexture.Width;
                    textureHeight = backTexture.Height;
                    break;
                case Direction.Down:
                    textureWidth = frontTexture.Width;
                    textureHeight = frontTexture.Height;
                    break;
                case Direction.Left:
                case Direction.Right:
                    textureWidth = leftTexture.Width;
                    textureHeight = leftTexture.Height;
                    break;
            }

            // PSEUDOCODE: Compute source rectangle for animation frame
            int frameW = textureWidth / totalFrames;
            int frameH = textureHeight;
            sourceRectangle = new Rectangle(
                currentFrame * frameW,
                0,
                frameW,
                frameH
            );
        }

        public virtual void TakeDamage(int amount, Player player)
        {
            // PSEUDOCODE: Subtract damage and check for death
            if (isDead)
                return;

            Health -= amount;
            if (Health <= 0)
            {
                isDead = true;
                currentState = EnemyState.Dead;
                Debug.WriteLine("Enemy died!");
                AwardExperience(player);
            }
        }

        protected void AwardExperience(Player player)
        {
            // PSEUDOCODE: Give experience only once
            if (!experienceAwarded)
            {
                player.GainExperience(ExperienceReward);
                experienceAwarded = true;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // PSEUDOCODE: Skip drawing if dead
            if (isDead)
                return;

            // PSEUDOCODE: Select texture based on direction
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

            // PSEUDOCODE: Draw character and its bullets
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
            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        protected virtual void Patrol(Viewport viewport)
        {
            // PSEUDOCODE: Default patrol behavior (no-op placeholder)
        }

        protected virtual void ChasePlayer(Vector2 playerPosition)
        {
            // PSEUDOCODE: Move toward player and update direction
            Vector2 dirVec = playerPosition - Position;
            if (dirVec != Vector2.Zero)
            {
                Vector2 move = Vector2.Normalize(dirVec);
                Position += move * MovementSpeed * 0.02f;
                currentDirection = Math.Abs(move.X) > Math.Abs(move.Y)
                    ? (move.X < 0 ? Direction.Left : Direction.Right)
                    : (move.Y < 0 ? Direction.Up : Direction.Down);
            }
        }

        protected virtual void Shoot()
        {
            // PSEUDOCODE: Choose bullet direction and texture
            Vector2 dirVec = currentDirection switch
            {
                Direction.Up => new Vector2(0, -1),
                Direction.Down => new Vector2(0, 1),
                Direction.Left => new Vector2(-1, 0),
                Direction.Right => new Vector2(1, 0),
                _ => new Vector2(1, 0)
            };
            Vector2 spawn = Position;

            // PSEUDOCODE: Fire bullet and add to list
            Bullet b = new Bullet(
                (currentDirection == Direction.Left || currentDirection == Direction.Right)
                    ? bulletHorizontalTexture
                    : bulletVerticalTexture,
                spawn,
                dirVec,
                bulletSpeed,
                BulletDamage,
                SpriteEffects.None,
                BulletRange
            );
            bullets.Add(b);
        }
    }
}

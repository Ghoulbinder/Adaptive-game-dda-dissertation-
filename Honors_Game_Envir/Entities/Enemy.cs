using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public enum GameState
    {
        MainMenu,
        GreenForestCentre,
        ForestTop,
        ForestLeft,
        ForestButtom,
        ForestRight
    }

    public class Enemy
    {
        // Textures for movement – these are the “enemy” sprites.
        protected Texture2D backTexture, frontTexture, leftTexture;
        protected Texture2D bulletHorizontalTexture, bulletVerticalTexture;

        public Vector2 Position;
        public float MovementSpeed { get; set; } = 100f;
        public int Health { get; protected set; }
        public int BulletDamage { get; set; } = 5;
        public float FiringInterval { get; set; } = 2f; // seconds between shots
        public float BulletRange { get; set; } = 1000f; // extended range so bullets only disappear off screen
        public int CollisionDamage { get; set; } = 15;
        public float CollisionDamageInterval { get; set; } = 1f;

        // FSM timers.
        protected float timeSinceLastShot = 0f;
        protected float collisionDamageTimer = 0f;

        // Animation fields.
        protected Rectangle sourceRectangle;
        protected float frameTime = 0.1f; // seconds per frame (can be adjusted per state if desired)
        protected float timer = 0f;
        protected int currentFrame = 0;
        protected int totalFrames = 4; // e.g. 4 frames per animation
        // List of bullets fired by this enemy.
        protected List<Bullet> bullets;
        protected float bulletSpeed = 300f;

        // Finite State Machine for the enemy.
        public enum EnemyState { Idle, Patrol, Chase, Attack, Dead }
        public enum Direction { Left, Right, Up, Down }
        protected EnemyState currentState;
        protected Direction currentDirection;

        protected const int CollisionPadding = 5;

        // Define thresholds.
        protected float shootingRange = 200f; // if player is within 200 pixels, attack
        protected float chaseRange = 400f;    // if player is between 200 and 400, chase

        // **** Experience gain modifications ****
        public int ExperienceReward { get; set; } = 10; // Default exp reward for regular enemy
        private bool experienceAwarded = false;         // Ensure exp is awarded only once

        // Award experience to the player when the enemy dies.
        protected void AwardExperience(Player player)
        {
            if (!experienceAwarded)
            {
                player.GainExperience(ExperienceReward);
                experienceAwarded = true;
            }
        }
        // **** End modifications ****

        // Basic bounding box.
        public virtual Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, sourceRectangle.Width, sourceRectangle.Height);
            }
        }

        public bool IsDead => isDead;
        protected bool isDead = false;

        // Constructor.
        public Enemy(Texture2D back, Texture2D front, Texture2D left,
                     Texture2D bulletHorizontal, Texture2D bulletVertical,
                     Vector2 startPosition, Direction startDirection,
                     int health, int bulletDamage)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            bulletHorizontalTexture = bulletHorizontal;
            bulletVerticalTexture = bulletVertical;

            Position = startPosition;
            currentDirection = startDirection;
            Health = health;
            BulletDamage = bulletDamage;

            MovementSpeed = 100f;
            bulletSpeed = 300f;
            frameTime = 0.1f;
            FiringInterval = 2f;
            timeSinceLastShot = 0f;
            collisionDamageTimer = 0f;
            currentFrame = 0;
            int frameW = leftTexture.Width / totalFrames;
            sourceRectangle = new Rectangle(0, 0, frameW, leftTexture.Height);

            bullets = new List<Bullet>();
            currentState = EnemyState.Patrol;
        }

        /// <summary>
        /// Update the enemy’s behavior.
        /// </summary>
        public virtual void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            // If already dead, nothing to update.
            if (isDead)
                return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float distance = Vector2.Distance(Position, playerPosition);

            if (distance <= shootingRange)
            {
                currentState = EnemyState.Attack;
            }
            else if (distance <= chaseRange)
            {
                currentState = EnemyState.Chase;
            }
            else
            {
                currentState = EnemyState.Patrol;
            }

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

            timer += delta;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }
            UpdateFrameDimensions();

            foreach (var bullet in bullets)
            {
                bullet.Update(gameTime);
                if (bullet.IsActive && player.Bounds.Intersects(bullet.Bounds))
                {
                    player.TakeDamage(bullet.Damage);
                    bullet.Deactivate();
                }
            }
            bullets.RemoveAll(b => !b.IsActive);
        }

        /// <summary>
        /// Update the source rectangle based on the current frame.
        /// </summary>
        protected virtual void UpdateFrameDimensions()
        {
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
            int frameW = textureWidth / totalFrames;
            int frameH = textureHeight;
            sourceRectangle = new Rectangle(currentFrame * frameW, 0, frameW, frameH);
        }

        /// <summary>
        /// Inflict damage on the enemy and award experience if killed.
        /// </summary>
        public virtual void TakeDamage(int amount, Player player)
        {
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

        /// <summary>
        /// Draw the enemy.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (isDead)
                return;

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

        // Helper methods for enemy behavior.
        protected virtual void Patrol(Viewport viewport)
        {
            // Implement patrol behavior if desired.
        }

        protected virtual void ChasePlayer(Vector2 playerPosition)
        {
            Vector2 directionVector = playerPosition - Position;
            if (directionVector != Vector2.Zero)
            {
                Vector2 moveDir = Vector2.Normalize(directionVector);
                Position += moveDir * MovementSpeed * 0.02f;
                if (Math.Abs(moveDir.X) > Math.Abs(moveDir.Y))
                    currentDirection = moveDir.X < 0 ? Direction.Left : Direction.Right;
                else
                    currentDirection = moveDir.Y < 0 ? Direction.Up : Direction.Down;
            }
        }

        protected virtual void Shoot()
        {
            Vector2 bulletDirection;
            switch (currentDirection)
            {
                case Direction.Up:
                    bulletDirection = new Vector2(0, -1);
                    break;
                case Direction.Down:
                    bulletDirection = new Vector2(0, 1);
                    break;
                case Direction.Left:
                    bulletDirection = new Vector2(-1, 0);
                    break;
                case Direction.Right:
                    bulletDirection = new Vector2(1, 0);
                    break;
                default:
                    bulletDirection = new Vector2(1, 0);
                    break;
            }
            Vector2 bulletPos = Position + new Vector2(sourceRectangle.Width / 2f, sourceRectangle.Height / 2f);
            Bullet bullet = new Bullet(
                (currentDirection == Direction.Left || currentDirection == Direction.Right) ? bulletHorizontalTexture : bulletVerticalTexture,
                bulletPos,
                bulletDirection,
                bulletSpeed,
                BulletDamage,
                SpriteEffects.None,
                BulletRange
            );
            bullets.Add(bullet);
        }
    }
}

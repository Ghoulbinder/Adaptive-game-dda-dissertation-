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

        // Define thresholds (these values mimic a mini-GreenBoss)
        protected float shootingRange = 200f; // if player is within 200 pixels, attack
        protected float chaseRange = 400f;    // if player is between 200 and 400, chase
        // (Below 200, enemy attacks; above 400, enemy patrols.)

        // Basic bounding box – you can later add center-based logic if needed.
        public virtual Rectangle Bounds
        {
            get
            {
                // Use the top-left and the source rectangle dimensions.
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

            // Set default values.
            MovementSpeed = 100f;
            bulletSpeed = 300f;
            frameTime = 0.1f;
            FiringInterval = 2f;
            timeSinceLastShot = 0f;
            collisionDamageTimer = 0f;
            currentFrame = 0;
            // Initialize the source rectangle based on the leftTexture.
            int frameW = leftTexture.Width / totalFrames;
            sourceRectangle = new Rectangle(0, 0, frameW, leftTexture.Height);

            bullets = new List<Bullet>();
            currentState = EnemyState.Patrol;
        }

        /// <summary>
        /// Update the enemy’s behavior using a finite state machine.
        /// Mimics a “mini GreenBoss”:
        /// - If the player is within shootingRange, switch to Attack and shoot.
        /// - If the player is between shootingRange and chaseRange, chase.
        /// - Otherwise, patrol.
        /// </summary>
        public virtual void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            if (isDead)
                return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float distance = Vector2.Distance(Position, playerPosition);

            // Update FSM state based on player distance.
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

            // Execute behavior based on state.
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
                    // Idle state can be added if needed.
            }

            // Update animation timer.
            timer += delta;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }
            UpdateFrameDimensions();

            // Update bullets and check collision with the player.
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
        /// Update the source rectangle based on the current animation frame.
        /// </summary>
        protected virtual void UpdateFrameDimensions()
        {
            int textureWidth = 0, textureHeight = 0;
            // Choose the texture based on the current direction.
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
        /// Basic damage handler.
        /// </summary>
        public virtual void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                isDead = true;
                currentState = EnemyState.Dead;
                Debug.WriteLine("Enemy died!");
            }
        }

        /// <summary>
        /// Draw the enemy using its current animation frame.
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

        // Helper behavior methods:

        /// <summary>
        /// Patrol behavior – you can implement your own logic here.
        /// For now, this method does nothing.
        /// </summary>
        protected virtual void Patrol(Viewport viewport)
        {
            // Implement patrol behavior if desired.
            // For example, move back and forth along a set path.
        }

        /// <summary>
        /// Chase the player by moving toward the player's current position.
        /// </summary>
        protected virtual void ChasePlayer(Vector2 playerPosition)
        {
            Vector2 directionVector = playerPosition - Position;
            if (directionVector != Vector2.Zero)
            {
                Vector2 moveDir = Vector2.Normalize(directionVector);
                Position += moveDir * MovementSpeed * 0.02f;
                // Update the current direction based on the dominant axis.
                if (Math.Abs(moveDir.X) > Math.Abs(moveDir.Y))
                    currentDirection = moveDir.X < 0 ? Direction.Left : Direction.Right;
                else
                    currentDirection = moveDir.Y < 0 ? Direction.Up : Direction.Down;
            }
        }

        /// <summary>
        /// Shoot a projectile.
        /// </summary>
        protected virtual void Shoot()
        {
            // For this example, choose the bullet texture based on the enemy’s current facing.
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
            // Place the bullet starting roughly at the center of the enemy.
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

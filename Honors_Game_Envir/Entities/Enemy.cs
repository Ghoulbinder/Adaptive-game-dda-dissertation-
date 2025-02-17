using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class Enemy
    {
        protected Texture2D backTexture, frontTexture, leftTexture;
        protected Texture2D bulletHorizontalTexture, bulletVerticalTexture;

        public Vector2 Position;
        public float MovementSpeed { get; set; } = 100f;
        public int Health { get; protected set; }
        public int BulletDamage { get; set; } = 5;
        public float FiringInterval { get; set; } = 2f;
        public float BulletRange { get; set; } = 400f;
        public int CollisionDamage { get; set; } = 15;
        public float CollisionDamageInterval { get; set; } = 1f;

        protected float timeSinceLastShot = 0f;
        protected float collisionDamageTimer = 0f;

        protected Rectangle sourceRectangle;
        protected float frameTime = 0.1f;
        protected float timer = 0f;
        protected int currentFrame;
        protected int totalFrames = 4; // typical 4-frame walk
        protected List<Bullet> bullets;
        protected float bulletSpeed = 300f;
        protected EnemyState currentState;
        protected float shootingCooldown;
        protected float shootingTimer;
        protected bool isDead = false;

        public enum EnemyState { Idle, Patrol, Chase, Shoot, Charge, Flee, Dead }
        public enum Direction { Left, Right, Up, Down }
        protected Direction currentDirection;

        protected const int CollisionPadding = 5;

        // Mark as virtual so that Boss/ButterflyBoss can override with center-based logic
        public virtual Rectangle Bounds
        {
            get
            {
                // Basic top-left bounding box logic
                // If you want center-based logic for normal enemies, do it here or in a separate approach
                return new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    sourceRectangle.Width,
                    sourceRectangle.Height
                );
            }
        }

        public bool IsDead => isDead;

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

            // Assume leftTexture is typical 4-frame wide
            int frameW = leftTexture.Width / totalFrames;
            sourceRectangle = new Rectangle(0, 0, frameW, leftTexture.Height);

            bullets = new List<Bullet>();
            currentState = EnemyState.Patrol;
        }

        public virtual void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            if (isDead) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float distanceToPlayer = Vector2.Distance(Position, playerPosition);

            // Basic AI or FSM can go here...
            // For demonstration, let's keep minimal logic

            // Update bullets
            foreach (var bullet in bullets)
            {
                bullet.Update(gameTime);
                if (bullet.IsActive && player.Bounds.Intersects(bullet.Bounds))
                {
                    Debug.WriteLine("Enemy bullet hit player!");
                    player.TakeDamage(bullet.Damage);
                    bullet.Deactivate();
                }
            }
            bullets.RemoveAll(b => !b.IsActive);

            // Animate
            timer += delta;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }
            UpdateFrameDimensions();
        }

        protected virtual void UpdateFrameDimensions()
        {
            // Adjust sourceRectangle based on direction
            int textureWidth = 0;
            int textureHeight = 0;
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

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (isDead) return;

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
                bullet.Draw(spriteBatch);
        }

        // Some basic helper AI methods
        protected virtual void Patrol(Viewport viewport)
        {
            // minimal logic
        }
        protected virtual void ChasePlayer(Vector2 playerPosition)
        {
            Vector2 directionVector = playerPosition - Position;
            Vector2 moveDir = Vector2.Normalize(directionVector);
            Position += moveDir * MovementSpeed * 0.02f;
        }
        protected virtual void Shoot()
        {
            // minimal logic
        }
    }
}

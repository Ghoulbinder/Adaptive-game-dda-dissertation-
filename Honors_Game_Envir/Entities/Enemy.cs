using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class Enemy
    {
        // Marked as protected so derived classes (like Boss) can access them.
        protected Texture2D backTexture, frontTexture, leftTexture, bulletHorizontalTexture, bulletVerticalTexture;
        public Vector2 Position;

        // Modular enemy parameters.
        public float MovementSpeed { get; set; } = 100f;
        public int Health { get; protected set; }
        public int BulletDamage { get; set; } = 5;
        public float FiringInterval { get; set; } = 2f;  // Seconds between enemy shots
        public float BulletRange { get; set; } = 400f;
        public int CollisionDamage { get; set; } = 15;
        public float CollisionDamageInterval { get; set; } = 1f;

        // NEW: Charge state multiplier and threshold.
        public float ChargeMultiplier { get; set; } = 2.0f;
        public float ChargeDistanceThreshold { get; set; } = 100f;

        protected float timeSinceLastShot = 0f;
        protected float collisionDamageTimer = 0f;

        protected Rectangle sourceRectangle;
        protected float frameTime = 0.1f;
        protected float timer = 0f;
        protected int currentFrame;
        protected int totalFrames = 4;
        protected List<Bullet> bullets;
        protected float bulletSpeed;
        protected EnemyState currentState;
        protected float shootingCooldown;
        protected float shootingTimer;
        protected bool isDead = false;

        protected const int CollisionPadding = 5;
        public Rectangle Bounds => new Rectangle(
            (int)Position.X - CollisionPadding,
            (int)Position.Y - CollisionPadding,
            sourceRectangle.Width + 2 * CollisionPadding,
            sourceRectangle.Height + 2 * CollisionPadding);

        public bool IsDead => isDead;

        public enum EnemyState { Idle, Patrol, Chase, Shoot, Charge, Flee, Dead }
        public enum Direction { Left, Right, Up, Down }
        protected Direction currentDirection;

        public Enemy(Texture2D back, Texture2D front, Texture2D left,
            Texture2D bulletHorizontal, Texture2D bulletVertical,
            Vector2 startPosition, Direction startDirection, int health, int bulletDamage)
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
            shootingCooldown = FiringInterval;
            shootingTimer = 0f;
            timeSinceLastShot = 0f;
            collisionDamageTimer = 0f;
            currentFrame = 0;
            int frameW = leftTexture.Width / totalFrames;
            sourceRectangle = new Rectangle(0, 0, frameW, leftTexture.Height);
            bullets = new List<Bullet>();
            currentState = EnemyState.Patrol;
        }

        public virtual void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            if (isDead) return;

            float distanceToPlayer = Vector2.Distance(Position, playerPosition);

            switch (currentState)
            {
                case EnemyState.Idle:
                    break;
                case EnemyState.Patrol:
                    Patrol(viewport);
                    if (distanceToPlayer < 300)
                        currentState = EnemyState.Chase;
                    break;
                case EnemyState.Chase:
                    if (distanceToPlayer < ChargeDistanceThreshold)
                    {
                        currentState = EnemyState.Charge;
                    }
                    else
                    {
                        ChasePlayer(playerPosition);
                        if (distanceToPlayer < 150)
                            currentState = EnemyState.Shoot;
                        if (distanceToPlayer > 400)
                            currentState = EnemyState.Flee;
                    }
                    break;
                case EnemyState.Charge:
                    Charge(playerPosition);
                    if (distanceToPlayer > ChargeDistanceThreshold * 1.5f)
                        currentState = EnemyState.Chase;
                    break;
                case EnemyState.Shoot:
                    timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timeSinceLastShot >= FiringInterval)
                    {
                        Shoot();
                        timeSinceLastShot = 0f;
                    }
                    if (distanceToPlayer > 200)
                        currentState = EnemyState.Chase;
                    break;
                case EnemyState.Flee:
                    Flee(playerPosition);
                    if (distanceToPlayer > 450)
                        currentState = EnemyState.Patrol;
                    break;
                case EnemyState.Dead:
                    isDead = true;
                    return;
            }

            // Update bullets and check collision with the player.
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

            // Collision damage if enemy touches player.
            if (Bounds.Intersects(player.Bounds))
            {
                collisionDamageTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (collisionDamageTimer >= CollisionDamageInterval)
                {
                    Debug.WriteLine("Enemy collided with player!");
                    player.TakeDamage(CollisionDamage);
                    collisionDamageTimer = 0f;
                }
            }
            else
            {
                collisionDamageTimer = 0f;
            }

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }
            UpdateFrameDimensions();
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

        protected virtual void Patrol(Viewport viewport)
        {
            if (currentDirection == Direction.Left)
            {
                Position.X -= MovementSpeed * 0.02f;
                if (Position.X <= 0)
                    currentDirection = Direction.Right;
            }
            else if (currentDirection == Direction.Right)
            {
                Position.X += MovementSpeed * 0.02f;
                if (Position.X >= viewport.Width - sourceRectangle.Width)
                    currentDirection = Direction.Left;
            }
        }

        protected virtual void ChasePlayer(Vector2 playerPosition)
        {
            Vector2 directionVector = playerPosition - Position;
            Vector2 moveDirection = Vector2.Normalize(directionVector);
            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
                currentDirection = moveDirection.X < 0 ? Direction.Left : Direction.Right;
            else
                currentDirection = moveDirection.Y < 0 ? Direction.Up : Direction.Down;
            Position += moveDirection * MovementSpeed * 0.02f;
        }

        protected virtual void Charge(Vector2 playerPosition)
        {
            Vector2 directionVector = playerPosition - Position;
            Vector2 moveDirection = Vector2.Normalize(directionVector);
            Position += moveDirection * MovementSpeed * ChargeMultiplier * 0.02f;
            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
                currentDirection = moveDirection.X < 0 ? Direction.Left : Direction.Right;
            else
                currentDirection = moveDirection.Y < 0 ? Direction.Up : Direction.Down;
        }

        protected virtual void Flee(Vector2 playerPosition)
        {
            Vector2 directionVector = Position - playerPosition;
            Vector2 moveDirection = Vector2.Normalize(directionVector);
            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
                currentDirection = moveDirection.X < 0 ? Direction.Left : Direction.Right;
            else
                currentDirection = moveDirection.Y < 0 ? Direction.Up : Direction.Down;
            Position += moveDirection * MovementSpeed * 0.02f;
        }

        protected virtual void Shoot()
        {
            Vector2 bulletDirection = Vector2.Zero;
            Texture2D bulletTexture = bulletHorizontalTexture;
            SpriteEffects spriteEffect = SpriteEffects.None;

            switch (currentDirection)
            {
                case Direction.Up:
                    bulletDirection = new Vector2(0, -1);
                    bulletTexture = bulletVerticalTexture;
                    break;
                case Direction.Down:
                    bulletDirection = new Vector2(0, 1);
                    bulletTexture = bulletVerticalTexture;
                    spriteEffect = SpriteEffects.FlipVertically;
                    break;
                case Direction.Left:
                    bulletDirection = new Vector2(-1, 0);
                    bulletTexture = bulletHorizontalTexture;
                    spriteEffect = SpriteEffects.FlipHorizontally;
                    break;
                case Direction.Right:
                    bulletDirection = new Vector2(1, 0);
                    bulletTexture = bulletHorizontalTexture;
                    break;
            }

            Vector2 bulletPosition = Position + new Vector2(sourceRectangle.Width / 2, sourceRectangle.Height / 2);
            Debug.WriteLine($"Enemy firing bullet with damage: {BulletDamage}");
            bullets.Add(new Bullet(bulletTexture, bulletPosition, bulletDirection, bulletSpeed, BulletDamage, spriteEffect, BulletRange));
        }

        protected virtual void UpdateFrameDimensions()
        {
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
    }
}

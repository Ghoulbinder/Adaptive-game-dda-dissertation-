using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//Enemy not charging towards player

namespace Survivor_of_the_Bulge
{
    public class Enemy
    {
        private Texture2D backTexture, frontTexture, leftTexture, bulletHorizontalTexture, bulletVerticalTexture;
        public Vector2 Position;

        // Modular enemy parameters.
        public float MovementSpeed { get; set; } = 100f;
        public int Health { get; private set; }
        public int BulletDamage { get; set; } = 5;
        public float FiringInterval { get; set; } = 2f;  // Seconds between enemy shots
        public float BulletRange { get; set; } = 400f;
        public int CollisionDamage { get; set; } = 15;       // Damage when enemy collides with player
        public float CollisionDamageInterval { get; set; } = 1f; // Apply collision damage once per second

        // NEW: Charge state multiplier (increases speed when charging)
        public float ChargeMultiplier { get; set; } = 2.0f;
        // NEW: Distance threshold to switch into Charge state.
        public float ChargeDistanceThreshold { get; set; } = 100f;

        private float timeSinceLastShot = 0f;
        private float collisionDamageTimer = 0f;

        private Rectangle sourceRectangle;
        private float frameTime = 0.1f;
        private float timer = 0f;
        private int currentFrame;
        private int totalFrames = 4;
        private List<Bullet> bullets;
        private float bulletSpeed;
        private EnemyState currentState;
        private float shootingCooldown;
        private float shootingTimer;
        private bool isDead = false;

        // Extra collision padding.
        private const int CollisionPadding = 5;
        public Rectangle Bounds => new Rectangle(
            (int)Position.X - CollisionPadding,
            (int)Position.Y - CollisionPadding,
            sourceRectangle.Width + 2 * CollisionPadding,
            sourceRectangle.Height + 2 * CollisionPadding);

        public bool IsDead => isDead;

        // Updated finite state with new Charge state.
        public enum EnemyState { Idle, Patrol, Chase, Shoot, Charge, Flee, Dead }
        public enum Direction { Left, Right, Up, Down }

        private Direction currentDirection;

        public Enemy(Texture2D back, Texture2D front, Texture2D left, Texture2D bulletHorizontal, Texture2D bulletVertical,
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

        public void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            if (isDead)
                return;

            float distanceToPlayer = Vector2.Distance(Position, playerPosition);

            // State transitions and behavior.
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
                    // If close enough, switch to Charge state.
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
                    // In Charge state, enemy runs at an increased speed directly toward the player.
                    Charge(playerPosition);
                    // Optionally, if player gets farther away, revert back to Chase.
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

            // Process enemy bullets hitting the player.
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

            // Apply collision damage if enemy and player intersect.
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

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                isDead = true;
                currentState = EnemyState.Dead;
                Debug.WriteLine("Enemy died!");
            }
        }

        private void Patrol(Viewport viewport)
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

        private void ChasePlayer(Vector2 playerPosition)
        {
            Vector2 directionVector = playerPosition - Position;
            Vector2 moveDirection = Vector2.Normalize(directionVector);
            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
                currentDirection = moveDirection.X < 0 ? Direction.Left : Direction.Right;
            else
                currentDirection = moveDirection.Y < 0 ? Direction.Up : Direction.Down;
            Position += moveDirection * MovementSpeed * 0.02f;
        }

        // NEW: In Charge state, enemy runs faster toward the player.
        private void Charge(Vector2 playerPosition)
        {
            Vector2 directionVector = playerPosition - Position;
            Vector2 moveDirection = Vector2.Normalize(directionVector);
            // When charging, increase movement speed.
            Position += moveDirection * MovementSpeed * ChargeMultiplier * 0.02f;
            // Set current direction based on the movement.
            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
                currentDirection = moveDirection.X < 0 ? Direction.Left : Direction.Right;
            else
                currentDirection = moveDirection.Y < 0 ? Direction.Up : Direction.Down;
        }

        private void Flee(Vector2 playerPosition)
        {
            Vector2 directionVector = Position - playerPosition;
            Vector2 moveDirection = Vector2.Normalize(directionVector);
            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
                currentDirection = moveDirection.X < 0 ? Direction.Left : Direction.Right;
            else
                currentDirection = moveDirection.Y < 0 ? Direction.Up : Direction.Down;
            Position += moveDirection * MovementSpeed * 0.02f;
        }

        private void Shoot()
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

        private void UpdateFrameDimensions()
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

        public void Draw(SpriteBatch spriteBatch)
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

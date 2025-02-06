using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Survivor_of_the_Bulge
{
    public class Enemy
    {
        private Texture2D backTexture, frontTexture, leftTexture, bulletHorizontalTexture, bulletVerticalTexture;
        public Vector2 Position;
        private float speed;
        private int health;
        private int bulletDamage;
        private Rectangle sourceRectangle;
        private float frameTime;  // Time per frame for animation
        private float timer;      // Timer for animation
        private int currentFrame;
        private int totalFrames = 4;
        private List<Bullet> bullets;
        private float bulletSpeed;
        private EnemyState currentState;
        private float shootingCooldown;
        private float shootingTimer;
        private bool isDead = false;

        // Added constant for extra collision padding.
        private const int CollisionPadding = 5;

        // Use a slightly larger rectangle for collision detection.
        public Rectangle Bounds => new Rectangle(
            (int)Position.X - CollisionPadding,
            (int)Position.Y - CollisionPadding,
            sourceRectangle.Width + 2 * CollisionPadding,
            sourceRectangle.Height + 2 * CollisionPadding);

        public bool IsDead => isDead;

        public enum EnemyState { Idle, Patrol, Chase, Shoot, Flee, Dead }
        public enum Direction { Left, Right, Up, Down }

        private Direction currentDirection;

        public Enemy(Texture2D back, Texture2D front, Texture2D left, Texture2D bulletHorizontal, Texture2D bulletVertical, Vector2 startPosition, Direction startDirection, int health, int bulletDamage)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            bulletHorizontalTexture = bulletHorizontal;
            bulletVerticalTexture = bulletVertical;
            Position = startPosition;
            currentDirection = startDirection;
            this.health = Math.Max(1, health);
            this.bulletDamage = Math.Max(1, bulletDamage);

            speed = 100f;
            bulletSpeed = 300f;
            frameTime = 0.1f;
            shootingCooldown = 2f;
            shootingTimer = 0f;
            currentFrame = 0;

            int frameW = leftTexture.Width / totalFrames;
            sourceRectangle = new Rectangle(0, 0, frameW, leftTexture.Height);

            bullets = new List<Bullet>();
            currentState = EnemyState.Patrol;
        }

        public void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            if (isDead) return;

            float distanceToPlayer = Vector2.Distance(Position, playerPosition);

            switch (currentState)
            {
                case EnemyState.Idle:
                    break;
                case EnemyState.Patrol:
                    Patrol(viewport);
                    if (distanceToPlayer < 300) currentState = EnemyState.Chase;
                    break;
                case EnemyState.Chase:
                    ChasePlayer(playerPosition);
                    if (distanceToPlayer < 150) currentState = EnemyState.Shoot;
                    if (distanceToPlayer > 400) currentState = EnemyState.Flee;
                    break;
                case EnemyState.Shoot:
                    shootingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (shootingTimer >= shootingCooldown)
                    {
                        Shoot();
                        shootingTimer = 0;
                    }
                    if (distanceToPlayer > 200) currentState = EnemyState.Chase;
                    break;
                case EnemyState.Flee:
                    Flee(playerPosition);
                    if (distanceToPlayer > 450) currentState = EnemyState.Patrol;
                    break;
                case EnemyState.Dead:
                    isDead = true;
                    return;
            }

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
            health -= amount;
            if (health <= 0)
            {
                isDead = true;
                currentState = EnemyState.Dead;
            }
        }

        private void Patrol(Viewport viewport)
        {
            if (currentDirection == Direction.Left)
            {
                Position.X -= speed * 0.02f;
                if (Position.X <= 0) currentDirection = Direction.Right;
            }
            else if (currentDirection == Direction.Right)
            {
                Position.X += speed * 0.02f;
                if (Position.X >= viewport.Width - sourceRectangle.Width) currentDirection = Direction.Left;
            }
        }

        private void ChasePlayer(Vector2 playerPosition)
        {
            Vector2 directionVector = playerPosition - Position;
            Vector2 moveDirection = Vector2.Normalize(directionVector);

            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
            {
                currentDirection = moveDirection.X < 0 ? Direction.Left : Direction.Right;
            }
            else
            {
                currentDirection = moveDirection.Y < 0 ? Direction.Up : Direction.Down;
            }

            Position += moveDirection * speed * 0.02f;
        }

        private void Flee(Vector2 playerPosition)
        {
            Vector2 directionVector = Position - playerPosition;
            Vector2 moveDirection = Vector2.Normalize(directionVector);

            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
            {
                currentDirection = moveDirection.X < 0 ? Direction.Left : Direction.Right;
            }
            else
            {
                currentDirection = moveDirection.Y < 0 ? Direction.Up : Direction.Down;
            }

            Position += moveDirection * speed * 0.02f;
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
            bullets.Add(new Bullet(bulletTexture, bulletPosition, bulletDirection, bulletSpeed, bulletDamage, spriteEffect));
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

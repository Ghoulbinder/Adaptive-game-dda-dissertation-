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
        private float frameTime;
        private float timer;
        private int currentFrame;
        private int totalFrames;
        private List<Bullet> bullets;
        private float bulletSpeed;
        private EnemyState currentState;
        private float shootingCooldown;
        private float shootingTimer;
        private bool isDead = false; // ✅ Enemy death tracking

        public enum EnemyState { Idle, Patrol, Chase, Shoot, Flee, Dead }
        public enum Direction { Left, Right, Up, Down }

        private Direction currentDirection;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, sourceRectangle.Width, sourceRectangle.Height);
        public bool IsDead => isDead; // ✅ Public property to check if enemy is dead

        public Enemy(Texture2D back, Texture2D front, Texture2D left, Texture2D bulletHorizontal, Texture2D bulletVertical, Vector2 startPosition, Direction startDirection, int health, int bulletDamage)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            bulletHorizontalTexture = bulletHorizontal;
            bulletVerticalTexture = bulletVertical;
            Position = startPosition;
            currentDirection = startDirection;
            this.health = Math.Max(1, health); // ✅ Prevents zero or negative health
            this.bulletDamage = Math.Max(1, bulletDamage); // ✅ Prevents zero or negative damage

            speed = 100f;
            bulletSpeed = 300f;
            frameTime = 0.1f;
            shootingCooldown = 2f;
            shootingTimer = 0f;
            currentFrame = 0;

            totalFrames = Math.Max(1, left.Width / Math.Max(1, left.Height)); // ✅ Prevents divide by zero

            sourceRectangle = new Rectangle(0, 0, left.Width / totalFrames, left.Height);
            bullets = new List<Bullet>();
            currentState = EnemyState.Patrol;
        }

        public void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            if (isDead) return; // ✅ If enemy is dead, stop updating

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

                // Check collision with player
                if (bullet.IsActive && player.Bounds.Intersects(new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, 10, 10)))
                {
                    player.TakeDamage(bullet.Damage);
                    bullet.Deactivate(); // ✅ Properly deactivates bullets
                }
            }

            bullets.RemoveAll(b => !b.IsActive);
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            if (health <= 0)
            {
                isDead = true;
                currentState = EnemyState.Dead; // ✅ Enemy enters Dead state
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
            Vector2 moveDirection = Vector2.Normalize(playerPosition - Position);
            Position += moveDirection * speed * 0.02f;
        }

        private void Flee(Vector2 playerPosition)
        {
            Vector2 moveDirection = Vector2.Normalize(Position - playerPosition);
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

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isDead) return; // ✅ Don't draw if enemy is dead

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
        private void UpdateFrame()
        {
            timer += frameTime;

            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames; // Loop through frames
                timer = 0f;
            }

            sourceRectangle = new Rectangle(currentFrame * (frontTexture.Width / totalFrames), 0, frontTexture.Width / totalFrames, frontTexture.Height);
        }
    }
}

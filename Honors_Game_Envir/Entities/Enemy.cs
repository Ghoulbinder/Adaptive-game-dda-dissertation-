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
        private float speed = 100f;
        private Rectangle sourceRectangle;
        private float frameTime = 0.1f;
        private float timer = 0f;
        private int currentFrame = 0;
        private int totalFrames = 4;
        private List<Bullet> bullets;
        private float bulletSpeed = 300f;
        private EnemyState currentState;
        private float shootingCooldown = 2f;
        private float shootingTimer = 0f;

        public enum EnemyState { Idle, Patrol, Chase, Shoot, Flee }
        public enum Direction { Left, Right, Up, Down }

        private Direction currentDirection = Direction.Left;

        public Enemy(Texture2D back, Texture2D front, Texture2D left, Texture2D bulletHorizontal, Texture2D bulletVertical, Vector2 startPosition, Direction startDirection)
        {
            backTexture = back;
            frontTexture = front;
            leftTexture = left;
            bulletHorizontalTexture = bulletHorizontal;
            bulletVerticalTexture = bulletVertical;
            Position = startPosition;
            currentDirection = startDirection;
            sourceRectangle = new Rectangle(0, 0, left.Width / totalFrames, left.Height);
            bullets = new List<Bullet>();
            currentState = EnemyState.Patrol;
        }

        public void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition)
        {
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
            }

            foreach (var bullet in bullets)
                bullet.Update(gameTime);

            bullets.RemoveAll(b => !b.IsActive);

            // Update animation
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }

            UpdateSourceRectangle();
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

            // Update enemy direction to match movement
            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
            {
                currentDirection = (moveDirection.X > 0) ? Direction.Right : Direction.Left;
            }
            else
            {
                currentDirection = (moveDirection.Y > 0) ? Direction.Down : Direction.Up;
            }
        }

        private void Flee(Vector2 playerPosition)
        {
            Vector2 moveDirection = Vector2.Normalize(Position - playerPosition);
            Position += moveDirection * speed * 0.02f;

            // Update direction while fleeing
            if (Math.Abs(moveDirection.X) > Math.Abs(moveDirection.Y))
            {
                currentDirection = (moveDirection.X > 0) ? Direction.Right : Direction.Left;
            }
            else
            {
                currentDirection = (moveDirection.Y > 0) ? Direction.Down : Direction.Up;
            }
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
            bullets.Add(new Bullet(bulletTexture, bulletPosition, bulletDirection, bulletSpeed, spriteEffect));
        }

        private void UpdateSourceRectangle()
        {
            switch (currentDirection)
            {
                case Direction.Up:
                    sourceRectangle = new Rectangle(currentFrame * backTexture.Width / totalFrames, 0, backTexture.Width / totalFrames, backTexture.Height);
                    break;
                case Direction.Down:
                    sourceRectangle = new Rectangle(currentFrame * frontTexture.Width / totalFrames, 0, frontTexture.Width / totalFrames, frontTexture.Height);
                    break;
                case Direction.Left:
                case Direction.Right:
                    sourceRectangle = new Rectangle(currentFrame * leftTexture.Width / totalFrames, 0, leftTexture.Width / totalFrames, leftTexture.Height);
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
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

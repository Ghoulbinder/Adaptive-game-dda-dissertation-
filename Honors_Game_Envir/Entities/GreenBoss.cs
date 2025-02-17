using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class GreenBoss : Boss
    {
        public enum GreenBossState { Idle, Patrol, Chase, Attack, Enraged, Dead }
        public GreenBossState CurrentState { get; private set; } = GreenBossState.Idle;
        private float stateTimer = 0f;

        public GreenBoss(
            Texture2D back,
            Texture2D front,
            Texture2D left,
            Texture2D bulletHorizontal,
            Texture2D bulletVertical,
            Vector2 startPosition,
            Direction startDirection,
            int health,
            int bulletDamage
        )
            : base(back, front, left, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            MovementSpeed = 140f;
            FiringInterval = 1.2f;
            BulletRange = 550f;
            CollisionDamage = 35;
            CurrentState = GreenBossState.Idle;
            stateTimer = 0f;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            stateTimer += delta;
            float distanceToPlayer = Vector2.Distance(Position, playerPosition);

            switch (CurrentState)
            {
                case GreenBossState.Idle:
                    if (stateTimer >= 2f)
                    {
                        CurrentState = GreenBossState.Patrol;
                        stateTimer = 0f;
                    }
                    break;

                case GreenBossState.Patrol:
                    Patrol(viewport);
                    if (distanceToPlayer < 300)
                    {
                        CurrentState = GreenBossState.Chase;
                        stateTimer = 0f;
                    }
                    break;

                case GreenBossState.Chase:
                    ChasePlayer(playerPosition);
                    if (distanceToPlayer < 150)
                    {
                        CurrentState = GreenBossState.Attack;
                        stateTimer = 0f;
                    }
                    else if (distanceToPlayer > 400)
                    {
                        CurrentState = GreenBossState.Patrol;
                        stateTimer = 0f;
                    }
                    break;

                case GreenBossState.Attack:
                    timeSinceLastShot += delta;
                    if (timeSinceLastShot >= FiringInterval)
                    {
                        Shoot();
                        timeSinceLastShot = 0f;
                    }
                    if (distanceToPlayer > 200)
                    {
                        CurrentState = GreenBossState.Chase;
                        stateTimer = 0f;
                    }
                    if (Health < 0.3 * 300)
                    {
                        CurrentState = GreenBossState.Enraged;
                        stateTimer = 0f;
                    }
                    break;

                case GreenBossState.Enraged:
                    MovementSpeed = 180f;
                    FiringInterval = 0.8f;
                    ChasePlayer(playerPosition);
                    if (distanceToPlayer < 150)
                    {
                        timeSinceLastShot += delta;
                        if (timeSinceLastShot >= FiringInterval)
                        {
                            Shoot();
                            timeSinceLastShot = 0f;
                        }
                    }
                    if (distanceToPlayer > 300)
                    {
                        CurrentState = GreenBossState.Chase;
                        stateTimer = 0f;
                    }
                    break;

                case GreenBossState.Dead:
                    isDead = true;
                    return;
            }

            // Update bullets
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

            timer += delta;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }
            UpdateFrameDimensions();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;

            // Draw center-based
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

            int frameW = currentTexture.Width / totalFrames;
            int frameH = currentTexture.Height;
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

            spriteBatch.Draw(
                currentTexture,
                Position,
                sourceRectangle,
                Color.White,
                0f,
                origin,
                Scale,
                spriteEffects,
                0f
            );

            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        // If we want center-based collisions
        public override Rectangle Bounds
        {
            get
            {
                int frameW = (currentDirection == Direction.Left || currentDirection == Direction.Right)
                    ? leftTexture.Width / totalFrames
                    : frontTexture.Width / totalFrames;
                int frameH = (currentDirection == Direction.Left || currentDirection == Direction.Right)
                    ? leftTexture.Height
                    : frontTexture.Height;

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
    }
}

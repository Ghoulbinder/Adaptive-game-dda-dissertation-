using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class SpiderBoss : Boss
    {
        public enum SpiderBossState { Idle, Walking, Attack, Death }
        public SpiderBossState CurrentState { get; private set; } = SpiderBossState.Idle;
        private float stateTimer = 0f;

        // Boss animation textures:
        // Attack texture: 1280x1280, 4 columns x 4 rows (16 frames)
        private Texture2D attackTexture;
        // Walking texture: 960x960, 3 columns x 3 rows (9 frames)
        private Texture2D walkingTexture;

        // Animation settings for Attack.
        private const int attackFramesPerRow = 4;
        private const int attackRows = 4;
        private const int totalFrames_Attack = attackFramesPerRow * attackRows;
        private float attackFrameTime = 0.1f;

        // Animation settings for Walking.
        private const int walkingFramesPerRow = 3;
        private const int walkingRows = 3;
        private const int totalFrames_Walking = walkingFramesPerRow * walkingRows;
        private float walkingFrameTime = 0.1f;

        private float animTimer = 0f;
        private int frameIndex = 0;

        // Last known target position (used for aiming/rotation).
        private Vector2 lastTargetPosition;

        /// <summary>
        /// Constructs a new SpiderBoss.
        /// Parameters:
        /// 1. idleTexture: (passed to base; not used directly here)
        /// 2. attackTexture: Attack sprite sheet (1280x1280, 4x4)
        /// 3. walkingTexture: Walking sprite sheet (960x960, 3x3)
        /// 4. spiderBulletHorizontal: Bullet texture for horizontal attack (e.g., "Images/Projectile/spider_attack")
        /// 5. spiderBulletVertical: Bullet texture for vertical attack (e.g., "Images/Projectile/spider_attack2")
        /// 6. startPosition: Starting position
        /// 7. startDirection: Initial direction (Boss.Direction)
        /// 8. health: Health value
        /// 9. bulletDamage: Bullet damage value
        /// </summary>
        public SpiderBoss(
            Texture2D idleTexture,
            Texture2D attackTexture,
            Texture2D walkingTexture,
            Texture2D spiderBulletHorizontal,
            Texture2D spiderBulletVertical,
            Vector2 startPosition,
            Direction startDirection,
            int health,
            int bulletDamage)
            : base(idleTexture, idleTexture, idleTexture, spiderBulletHorizontal, spiderBulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            this.attackTexture = attackTexture;
            this.walkingTexture = walkingTexture;
            MovementSpeed = 110f;
            FiringInterval = 1.3f;
            BulletRange = 500f;
            CollisionDamage = 25;
            // Default state: Walking.
            CurrentState = SpiderBossState.Walking;
            animTimer = 0f;
            frameIndex = 0;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            stateTimer += delta;
            lastTargetPosition = playerPosition;
            float distance = Vector2.Distance(Position, playerPosition);

            // Set state based on distance.
            if (distance >= 200)
                CurrentState = SpiderBossState.Attack;
            else
                CurrentState = SpiderBossState.Walking;

            // In Attack state, update currentDirection based on player's relative position.
            if (CurrentState == SpiderBossState.Attack)
            {
                Vector2 diff = playerPosition - Position;
                if (diff != Vector2.Zero)
                {
                    diff.Normalize();
                    // Determine dominant axis to set a cardinal direction.
                    if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                        currentDirection = diff.X < 0 ? Direction.Left : Direction.Right;
                    else
                        currentDirection = diff.Y < 0 ? Direction.Up : Direction.Down;
                }
            }

            // Select animation parameters.
            float frameTime;
            int totalFrames;
            int framesPerRowUsed;
            Texture2D currentTexture;
            if (CurrentState == SpiderBossState.Attack)
            {
                currentTexture = attackTexture;
                framesPerRowUsed = attackFramesPerRow;
                totalFrames = totalFrames_Attack;
                frameTime = attackFrameTime;
            }
            else // Walking state.
            {
                currentTexture = walkingTexture;
                framesPerRowUsed = walkingFramesPerRow;
                totalFrames = totalFrames_Walking;
                frameTime = walkingFrameTime;
            }
            animTimer += delta;
            if (animTimer >= frameTime)
            {
                frameIndex = (frameIndex + 1) % totalFrames;
                animTimer = 0f;
            }

            // Behavior.
            if (CurrentState == SpiderBossState.Walking)
            {
                Vector2 diff = playerPosition - Position;
                if (diff != Vector2.Zero)
                {
                    diff.Normalize();
                    Position += diff * MovementSpeed * 0.02f;
                }
            }
            else if (CurrentState == SpiderBossState.Attack)
            {
                timeSinceLastShot += delta;
                if (timeSinceLastShot >= FiringInterval)
                {
                    if (distance >= 200)
                    {
                        Shoot();
                    }
                    timeSinceLastShot = 0f;
                }
            }

            // Update bullets.
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;

            Texture2D currentTexture;
            int framesPerRowUsed, rowsUsed;
            if (CurrentState == SpiderBossState.Attack)
            {
                currentTexture = attackTexture;
                framesPerRowUsed = attackFramesPerRow;
                rowsUsed = attackRows;
            }
            else
            {
                currentTexture = walkingTexture;
                framesPerRowUsed = walkingFramesPerRow;
                rowsUsed = walkingRows;
            }
            int frameW = currentTexture.Width / framesPerRowUsed;
            int frameH = currentTexture.Height / rowsUsed;
            Rectangle srcRect = new Rectangle((frameIndex % framesPerRowUsed) * frameW,
                                              (frameIndex / framesPerRowUsed) * frameH,
                                              frameW, frameH);

            // Rotate sprite so it faces the player.
            Vector2 diff = lastTargetPosition - Position;
            float angle = 0f;
            if (diff != Vector2.Zero)
                angle = (float)Math.Atan2(diff.Y, diff.X) - MathHelper.PiOver2;
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);
            spriteBatch.Draw(currentTexture, Position, srcRect, Color.White, angle, origin, Scale, SpriteEffects.None, 0f);

            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        public override Rectangle Bounds
        {
            get
            {
                int size = 77;
                return new Rectangle((int)(Position.X - size / 2), (int)(Position.Y - size / 2), size, size);
            }
        }

        protected override void Shoot()
        {
            // Use currentDirection set in Update.
            Vector2 direction;
            switch (currentDirection)
            {
                case Direction.Left:
                    direction = new Vector2(-1, 0);
                    break;
                case Direction.Right:
                    direction = new Vector2(1, 0);
                    break;
                case Direction.Up:
                    direction = new Vector2(0, -1);
                    break;
                case Direction.Down:
                    direction = new Vector2(0, 1);
                    break;
                default:
                    direction = new Vector2(1, 0);
                    break;
            }

            // Set sprite effects: flip horizontally for left, vertically for down.
            SpriteEffects effect = SpriteEffects.None;
            if (currentDirection == Direction.Left)
                effect = SpriteEffects.FlipHorizontally;
            else if (currentDirection == Direction.Down)
                effect = SpriteEffects.FlipVertically;

            Vector2 bulletPos = Position + direction * 20f;
            Bullet bullet = new Bullet(
                (currentDirection == Direction.Left || currentDirection == Direction.Right) ? bulletHorizontalTexture : bulletVerticalTexture,
                bulletPos,
                direction,
                500f,
                BulletDamage,
                effect,
                10000f // Large range so bullet only deactivates off-screen.
            );
            bullets.Add(bullet);
        }
    }
}

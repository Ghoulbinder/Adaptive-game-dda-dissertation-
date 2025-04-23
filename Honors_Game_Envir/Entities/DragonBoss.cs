using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survivor_of_the_Bulge
{
    public class DragonBoss : Boss
    {
        public enum DragonBossState { Projectile, Melee, Death }
        public DragonBossState CurrentState { get; private set; } = DragonBossState.Projectile;

        private Texture2D attackTexture;
        private Texture2D dragonBulletHorizontal;
        private Texture2D dragonBulletVertical;

        private const int framesPerRow = 4;
        private const int rows = 4;
        private int totalFrames => framesPerRow * rows;

        private float projectileFrameTime = 0.1f;
        private float meleeFrameTime = 0.1f;
        private float animTimer = 0f;
        private int frameIndex = 0;

        private float timeSinceLastShot = 0f;
        public float FiringInterval { get; set; } = 1.5f;

        private float meleeAttackInterval = 1.0f;
        private float timeSinceLastMelee = 0f;
        private int meleeDamage = 10;

        private float projectileThreshold = 250f;     // Distance above which boss uses projectiles
        private float meleeRangeThreshold = 50f;      // Distance below which boss switches to melee

        private Vector2 lastTargetPosition;

        /// <summary>
        /// Constructs a DragonBoss with specified textures, position, direction, health, and damage.
        /// </summary>
        public DragonBoss(
            Texture2D idleTexture,
            Texture2D attackTexture,
            Texture2D walkingTexture,
            Texture2D bulletHorizontal,
            Texture2D bulletVertical,
            Vector2 startPosition,
            Direction startDirection,
            int health,
            int bulletDamage)
            : base(idleTexture, idleTexture, idleTexture,
                   bulletHorizontal, bulletVertical,
                   startPosition, startDirection,
                   health, bulletDamage)
        {
            // PSEUDOCODE: Store attack and bullet textures
            this.attackTexture = attackTexture;
            dragonBulletHorizontal = bulletHorizontal;
            dragonBulletVertical = bulletVertical;

            // PSEUDOCODE: Initialize movement and combat parameters
            MovementSpeed = 120f;
            BulletRange = 500f;
            CollisionDamage = 30;
            CurrentState = DragonBossState.Projectile;
            animTimer = 0f;
            frameIndex = 0;
            lastTargetPosition = startPosition;

            // PSEUDOCODE: Set experience reward on defeat
            this.ExperienceReward = 50;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            lastTargetPosition = playerPosition;

            // PSEUDOCODE: Determine behavior state based on distance to player
            float distance = Vector2.Distance(Position, playerPosition);
            if (distance >= projectileThreshold)
            {
                CurrentState = DragonBossState.Projectile;
                timeSinceLastMelee = meleeAttackInterval; // ensure melee timer starts fresh
            }
            else
            {
                CurrentState = DragonBossState.Melee;
            }

            // PSEUDOCODE: Advance animation based on state-specific frame time
            float frameTime = (CurrentState == DragonBossState.Projectile)
                                ? projectileFrameTime
                                : meleeFrameTime;
            animTimer += delta;
            if (animTimer >= frameTime)
            {
                frameIndex = (frameIndex + 1) % totalFrames;
                animTimer = 0f;
            }

            // PSEUDOCODE: Execute projectile attacks if in range
            if (CurrentState == DragonBossState.Projectile)
            {
                timeSinceLastShot += delta;
                if (timeSinceLastShot >= FiringInterval)
                {
                    ShootProjectile();
                    timeSinceLastShot = 0f;
                }
            }
            // PSEUDOCODE: Move toward and melee attack if close enough
            else if (CurrentState == DragonBossState.Melee)
            {
                if (distance > meleeRangeThreshold)
                {
                    Vector2 direction = playerPosition - Position;
                    if (direction != Vector2.Zero)
                    {
                        direction.Normalize();
                        Position += direction * MovementSpeed * delta;
                    }
                }
                timeSinceLastMelee += delta;
                if (Bounds.Intersects(player.Bounds) && timeSinceLastMelee >= meleeAttackInterval)
                {
                    player.TakeDamage(meleeDamage);
                    timeSinceLastMelee = 0f;
                }
            }

            // PSEUDOCODE: Update bullets and handle collisions
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

            // PSEUDOCODE: Award experience upon death
            if (IsDead)
            {
                AwardExperience(player);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;

            // PSEUDOCODE: Calculate source rectangle for current animation frame
            Texture2D currentTexture = attackTexture;
            int frameW = currentTexture.Width / framesPerRow;
            int frameH = currentTexture.Height / rows;
            Rectangle srcRect = new Rectangle(
                (frameIndex % framesPerRow) * frameW,
                (frameIndex / framesPerRow) * frameH,
                frameW, frameH
            );
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

            // PSEUDOCODE: Determine rotation to face last known player position
            Vector2 diff = lastTargetPosition - Position;
            float angle = 0f;
            if (diff != Vector2.Zero)
            {
                diff.Normalize();
                angle = (float)Math.Atan2(diff.Y, diff.X) - MathHelper.PiOver2;
            }

            // PSEUDOCODE: Draw the dragon sprite with rotation and scaling
            spriteBatch.Draw(
                currentTexture,
                Position,
                srcRect,
                Color.White,
                angle,
                origin,
                Scale,
                SpriteEffects.None,
                0f
            );

            // PSEUDOCODE: Draw active projectiles
            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        /// <summary>
        /// Fires a projectile towards the last target position.
        /// </summary>
        protected void ShootProjectile()
        {
            // PSEUDOCODE: Compute normalized direction towards target
            Vector2 direction = lastTargetPosition - Position;
            if (direction != Vector2.Zero)
                direction.Normalize();
            else
                direction = Vector2.UnitX;

            // PSEUDOCODE: Choose horizontal or vertical bullet texture
            Texture2D chosenTexture = (Math.Abs(direction.X) >= Math.Abs(direction.Y))
                ? dragonBulletHorizontal
                : dragonBulletVertical;

            // PSEUDOCODE: Determine sprite flip based on direction
            SpriteEffects effect = SpriteEffects.None;
            if (direction.X < 0) effect |= SpriteEffects.FlipHorizontally;
            if (direction.Y > 0) effect |= SpriteEffects.FlipVertically;

            // PSEUDOCODE: Instantiate and add new bullet
            Vector2 spawnPos = Position + direction * 20f;
            Bullet projectile = new Bullet(
                chosenTexture,
                spawnPos,
                direction,
                500f,               // speed
                BulletDamage,
                effect,
                BulletRange
            );
            bullets.Add(projectile);
        }
    }
}

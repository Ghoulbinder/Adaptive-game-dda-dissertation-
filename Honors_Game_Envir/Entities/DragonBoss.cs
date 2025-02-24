using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class DragonBoss : Boss
    {
        public enum DragonBossState { Projectile, Melee, Death }
        public DragonBossState CurrentState { get; private set; } = DragonBossState.Projectile;

        // Textures for projectile (ranged) and melee modes.
        // In melee mode we now use the attack animation (instead of the walking animation) to show a melee attack.
        private Texture2D attackTexture;
        // For projectile mode, we also use the attackTexture.
        // (If you prefer a separate texture for melee movement, you could add another field here.)

        // Dragon boss bullet textures.
        private Texture2D dragonBulletHorizontal;
        private Texture2D dragonBulletVertical;

        // Animation settings.
        private const int framesPerRow = 4;
        private const int rows = 4;
        private int totalFrames => framesPerRow * rows; // 16 frames total
        private float projectileFrameTime = 0.1f;
        private float meleeFrameTime = 0.1f; // for melee mode
        private float animTimer = 0f;
        private int frameIndex = 0;

        // Firing and melee parameters.
        private float timeSinceLastShot = 0f;
        public float FiringInterval { get; set; } = 1.5f; // seconds between projectile attacks

        // Melee attack parameters.
        private float meleeAttackInterval = 1.0f; // seconds between melee attacks
        private float timeSinceLastMelee = 0f;
        private int meleeDamage = 10;  // reduced melee damage
        // Increase threshold: if player is closer than this distance, then switch to melee.
        private float projectileThreshold = 250f;
        // In melee mode, the boss will stop moving closer if within this range.
        private float meleeRangeThreshold = 50f;

        // Last known player position.
        private Vector2 lastTargetPosition;

        public DragonBoss(
            Texture2D idleTexture, // (passed to base; not used directly here)
            Texture2D attackTexture,
            Texture2D walkingTexture, // not used here because we only use attackTexture in both modes
            Texture2D bulletHorizontal,
            Texture2D bulletVertical,
            Vector2 startPosition,
            Direction startDirection,
            int health,
            int bulletDamage)
            : base(idleTexture, idleTexture, idleTexture, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            // Use the provided attack texture for both projectile and melee modes.
            this.attackTexture = attackTexture;
            // Store dragon-specific bullet textures.
            dragonBulletHorizontal = bulletHorizontal;
            dragonBulletVertical = bulletVertical;

            MovementSpeed = 120f;
            BulletRange = 500f;
            CollisionDamage = 30;
            CurrentState = DragonBossState.Projectile;
            animTimer = 0f;
            frameIndex = 0;
            lastTargetPosition = startPosition;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            lastTargetPosition = playerPosition;
            float distance = Vector2.Distance(Position, playerPosition);

            // Switch to projectile mode if player is far away (>= projectileThreshold), otherwise use melee mode.
            if (distance >= projectileThreshold)
            {
                CurrentState = DragonBossState.Projectile;
                timeSinceLastMelee = meleeAttackInterval; // reset melee timer
            }
            else
            {
                CurrentState = DragonBossState.Melee;
            }

            // Update animation timer.
            float frameTime = (CurrentState == DragonBossState.Projectile) ? projectileFrameTime : meleeFrameTime;
            animTimer += delta;
            if (animTimer >= frameTime)
            {
                frameIndex = (frameIndex + 1) % totalFrames;
                animTimer = 0f;
            }

            if (CurrentState == DragonBossState.Projectile)
            {
                timeSinceLastShot += delta;
                if (timeSinceLastShot >= FiringInterval)
                {
                    ShootProjectile();
                    timeSinceLastShot = 0f;
                }
            }
            else if (CurrentState == DragonBossState.Melee)
            {
                // In melee mode, if the boss is farther than meleeRangeThreshold, chase the player.
                if (distance > meleeRangeThreshold)
                {
                    Vector2 diff = playerPosition - Position;
                    if (diff != Vector2.Zero)
                    {
                        diff.Normalize();
                        Position += diff * MovementSpeed * 0.02f;
                    }
                }
                timeSinceLastMelee += delta;
                // When in melee range (boss's collision box intersects player's), perform a melee attack.
                if (Bounds.Intersects(player.Bounds) && timeSinceLastMelee >= meleeAttackInterval)
                {
                    player.TakeDamage(meleeDamage);
                    timeSinceLastMelee = 0f;
                }
            }

            // Update boss projectiles.
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

            // For both projectile and melee modes, use the attackTexture.
            Texture2D currentTexture = attackTexture;
            int frameW = currentTexture.Width / framesPerRow;
            int frameH = currentTexture.Height / rows;
            Rectangle srcRect = new Rectangle((frameIndex % framesPerRow) * frameW,
                                              (frameIndex / framesPerRow) * frameH,
                                              frameW, frameH);
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

            // Rotate sprite to face the player.
            Vector2 diff = lastTargetPosition - Position;
            float angle = 0f;
            if (diff != Vector2.Zero)
            {
                diff.Normalize();
                angle = (float)Math.Atan2(diff.Y, diff.X) - MathHelper.PiOver2;
            }

            spriteBatch.Draw(currentTexture, Position, srcRect, Color.White, angle, origin, Scale, SpriteEffects.None, 0f);

            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        protected void ShootProjectile()
        {
            Vector2 diff = lastTargetPosition - Position;
            if (diff != Vector2.Zero)
                diff.Normalize();
            else
                diff = new Vector2(1, 0);

            // Choose bullet texture based on dominant axis.
            Texture2D chosenBulletTexture = (Math.Abs(diff.X) >= Math.Abs(diff.Y)) ? dragonBulletHorizontal : dragonBulletVertical;

            SpriteEffects effect = SpriteEffects.None;
            if (diff.X < 0)
                effect = SpriteEffects.FlipHorizontally;
            if (diff.Y > 0)
                effect |= SpriteEffects.FlipVertically;

            Vector2 bulletPos = Position + diff * 20f; // Offset from boss center.
            Bullet bullet = new Bullet(chosenBulletTexture, bulletPos, diff, 500f, BulletDamage, effect, BulletRange);
            bullets.Add(bullet);
        }
    }
}

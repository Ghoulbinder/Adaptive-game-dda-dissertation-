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
        private float projectileThreshold = 250f;
        private float meleeRangeThreshold = 50f;

        private Vector2 lastTargetPosition;

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
            : base(idleTexture, idleTexture, idleTexture, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            this.attackTexture = attackTexture;
            dragonBulletHorizontal = bulletHorizontal;
            dragonBulletVertical = bulletVertical;

            MovementSpeed = 120f;
            BulletRange = 500f;
            CollisionDamage = 30;
            CurrentState = DragonBossState.Projectile;
            animTimer = 0f;
            frameIndex = 0;
            lastTargetPosition = startPosition;

            // **** Experience gain modification: set boss exp reward ****
            this.ExperienceReward = 50;
            // **** End modification ****
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            lastTargetPosition = playerPosition;
            float distance = Vector2.Distance(Position, playerPosition);

            if (distance >= projectileThreshold)
            {
                CurrentState = DragonBossState.Projectile;
                timeSinceLastMelee = meleeAttackInterval; // reset melee timer
            }
            else
            {
                CurrentState = DragonBossState.Melee;
            }

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
                if (Bounds.Intersects(player.Bounds) && timeSinceLastMelee >= meleeAttackInterval)
                {
                    player.TakeDamage(meleeDamage);
                    timeSinceLastMelee = 0f;
                }
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

            // **** Award experience on death ****
            if (IsDead)
            {
                AwardExperience(player);
            }
            // **** End modification ****
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;

            Texture2D currentTexture = attackTexture;
            int frameW = currentTexture.Width / framesPerRow;
            int frameH = currentTexture.Height / rows;
            Rectangle srcRect = new Rectangle((frameIndex % framesPerRow) * frameW,
                                              (frameIndex / framesPerRow) * frameH,
                                              frameW, frameH);
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

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

            Texture2D chosenBulletTexture = (Math.Abs(diff.X) >= Math.Abs(diff.Y)) ? dragonBulletHorizontal : dragonBulletVertical;

            SpriteEffects effect = SpriteEffects.None;
            if (diff.X < 0)
                effect = SpriteEffects.FlipHorizontally;
            if (diff.Y > 0)
                effect |= SpriteEffects.FlipVertically;

            Vector2 bulletPos = Position + diff * 20f;
            Bullet bullet = new Bullet(chosenBulletTexture, bulletPos, diff, 500f, BulletDamage, effect, BulletRange);
            bullets.Add(bullet);
        }
    }
}

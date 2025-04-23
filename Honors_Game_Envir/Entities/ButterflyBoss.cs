using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class ButterflyBoss : Boss
    {
        public enum ButterflyBossState { Walking, Attack, Death }
        public ButterflyBossState CurrentState { get; private set; } = ButterflyBossState.Walking;
        private float stateTimer = 0f;

        // Textures for the ButterflyBoss.
        private Texture2D attackTexture;
        private Texture2D walkingTexture;

        // Animation settings for Attack.
        private const int attackFramesPerRow = 4;
        private const int attackRows = 5;
        private const int attackTotalFrames = attackFramesPerRow * attackRows;
        private float attackFrameTime = 0.08f;

        // Animation settings for Walking.
        private const int walkingFramesPerRow = 4;
        private const int walkingRows = 5;
        private const int walkingTotalFrames = walkingFramesPerRow * walkingRows;
        private float walkingFrameTime = 0.08f;

        private float animTimer = 0f;
        private int frameIndex = 0;

        // Last known player position for aiming.
        private Vector2 lastTargetPosition;

        // Bullet textures for ButterflyBoss.
        private Texture2D butterflyBulletHorizontal;
        private Texture2D butterflyBulletVertical;

        /// <summary>
        /// Constructs a ButterflyBoss with specified textures, position, direction, health, and damage.
        /// </summary>
        public ButterflyBoss(
            Texture2D attackTexture,
            Texture2D walkingTexture,
            Texture2D bulletHorizontal,
            Texture2D bulletVertical,
            Vector2 startPosition,
            Direction startDirection,
            int health,
            int bulletDamage)
            : base(attackTexture, attackTexture, attackTexture,
                   bulletHorizontal, bulletVertical,
                   startPosition, startDirection,
                   health, bulletDamage)
        {
            // PSEUDOCODE: Initialize textures and movement parameters
            this.attackTexture = attackTexture;
            this.walkingTexture = walkingTexture;
            butterflyBulletHorizontal = bulletHorizontal;
            butterflyBulletVertical = bulletVertical;
            MovementSpeed = 120f;                // Base movement speed
            FiringInterval = 1.5f;               // Time between shots
            BulletRange = 500f;                  // Maximum bullet travel
            CollisionDamage = 30;                // Damage on contact
            CurrentState = ButterflyBossState.Walking;
            animTimer = 0f;
            frameIndex = 0;

            // PSEUDOCODE: Set experience reward on defeat
            this.ExperienceReward = 50;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            stateTimer += delta;
            lastTargetPosition = playerPosition;

            // PSEUDOCODE: Determine state based on distance to player
            float distance = Vector2.Distance(Position, playerPosition);
            if (distance > 200)
                CurrentState = ButterflyBossState.Walking;
            else
                CurrentState = ButterflyBossState.Attack;

            // PSEUDOCODE: Select animation settings based on state
            float frameTime;
            int totalFrames;
            int framesPerRowUsed;
            if (CurrentState == ButterflyBossState.Attack)
            {
                frameTime = attackFrameTime;
                totalFrames = attackTotalFrames;
                framesPerRowUsed = attackFramesPerRow;
            }
            else
            {
                frameTime = walkingFrameTime;
                totalFrames = walkingTotalFrames;
                framesPerRowUsed = walkingFramesPerRow;
            }

            // PSEUDOCODE: Advance animation frame when timer elapses
            animTimer += delta;
            if (animTimer >= frameTime)
            {
                frameIndex = (frameIndex + 1) % totalFrames;
                animTimer = 0f;
            }

            // PSEUDOCODE: Execute behavior based on current state
            if (CurrentState == ButterflyBossState.Walking)
            {
                ChasePlayer(playerPosition);
            }
            else if (CurrentState == ButterflyBossState.Attack)
            {
                timeSinceLastShot += delta;
                if (timeSinceLastShot >= FiringInterval)
                {
                    Shoot();
                    timeSinceLastShot = 0f;
                }
            }

            // PSEUDOCODE: Update each bullet and check for collisions
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

            // PSEUDOCODE: Award experience if boss is defeated
            if (IsDead)
            {
                AwardExperience(player);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;

            // PSEUDOCODE: Choose texture and layout based on state
            Texture2D currentTexture;
            int framesPerRowUsed, rowsUsed;
            if (CurrentState == ButterflyBossState.Attack)
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

            // PSEUDOCODE: Calculate source rectangle for current animation frame
            int frameW = currentTexture.Width / framesPerRowUsed;
            int frameH = currentTexture.Height / rowsUsed;
            Rectangle srcRect = new Rectangle(
                (frameIndex % framesPerRowUsed) * frameW,
                (frameIndex / framesPerRowUsed) * frameH,
                frameW, frameH
            );
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

            // PSEUDOCODE: Render boss sprite
            spriteBatch.Draw(
                currentTexture,
                Position,
                srcRect,
                Color.White,
                0f,
                origin,
                Scale,
                SpriteEffects.None,
                0f
            );

            // PSEUDOCODE: Draw active bullets
            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }
    }
}

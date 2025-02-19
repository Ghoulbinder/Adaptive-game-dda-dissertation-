using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class OgreBoss : Boss
    {
        public enum OgreBossState { Idle, Aggro, Attack, Reset, Death }
        public OgreBossState CurrentState { get; private set; } = OgreBossState.Idle;
        private float stateTimer = 0f;

        // Directional textures for Idle, Attack, and Walking animations.
        private Texture2D idleUpTexture, idleDownTexture, idleLeftTexture, idleRightTexture;
        private Texture2D attackUpTexture, attackDownTexture, attackLeftTexture, attackRightTexture;
        private Texture2D walkingUpTexture, walkingDownTexture, walkingLeftTexture, walkingRightTexture;

        // Animation settings for Idle and Attack (common dimensions: 1024x1280, 4 frames per row, 5 rows → 20 frames total)
        private const int commonFramesPerRow = 4;
        private const int commonRows = 5;
        private int TotalFrames_Common => commonFramesPerRow * commonRows; // 20 frames
        private float idleFrameTime = 0.15f;
        private float attackFrameTime = 0.1f;

        // Animation settings for Walking (assumed dimensions: 1024x1024, 4 frames per row, 4 rows → 16 frames total)
        private const int walkingFramesPerRow = 4;
        private const int walkingRows = 4;
        private int TotalFrames_Walking => walkingFramesPerRow * walkingRows; // 16 frames
        private float walkingFrameTime = 0.1f;

        private float animTimer = 0f;
        private int frameIndex = 0;

        // Behavior variables.
        private bool isAggro = false;            // Becomes true when the boss takes damage.
        private float meleeThreshold = 150f;       // Distance at which the boss considers itself "in range" to attack.
        private float runSpeedMultiplier = 2.0f;   // Multiplier when chasing aggressively.
        private int meleeDamage = 25;              // Damage dealt by a melee attack.
        private float resetDuration = 2f;          // Duration after an attack before resetting.
        private float resetTimer = 0f;

        // For melee attack: store the attack target and locked facing.
        private Vector2 attackTarget;            // The player's position when the boss first becomes aggro.
        private Vector2 lastTargetPosition;      // Last known player position (updated in Idle if not locked).
        private float fixedAngle = 0f;           // The angle locked at the moment of attack.
        private bool idleAngleLocked = false;    // When true, the boss does not update its idle angle.

        /// <summary>
        /// Constructs an OgreBoss with separate directional textures for Idle, Attack, and Walking.
        /// Requires 16 texture arguments:
        /// Idle: idleUp, idleDown, idleLeft, idleRight.
        /// Attack: attackUp, attackDown, attackLeft, attackRight.
        /// Walking: walkingUp, walkingDown, walkingLeft, walkingRight.
        /// Plus bullet textures (unused for melee but required by base), startPosition, startDirection, health, and bulletDamage.
        /// </summary>
        public OgreBoss(
            Texture2D idleUp, Texture2D idleDown, Texture2D idleLeft, Texture2D idleRight,
            Texture2D attackUp, Texture2D attackDown, Texture2D attackLeft, Texture2D attackRight,
            Texture2D walkingUp, Texture2D walkingDown, Texture2D walkingLeft, Texture2D walkingRight,
            Texture2D bulletHorizontal, Texture2D bulletVertical,
            Vector2 startPosition, Direction startDirection,
            int health, int bulletDamage)
            : base(idleDown, idleDown, idleDown, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            // Store directional textures.
            idleUpTexture = idleUp;
            idleDownTexture = idleDown;
            idleLeftTexture = idleLeft;
            idleRightTexture = idleRight;

            attackUpTexture = attackUp;
            attackDownTexture = attackDown;
            attackLeftTexture = attackLeft;
            attackRightTexture = attackRight;

            walkingUpTexture = walkingUp;
            walkingDownTexture = walkingDown;
            walkingLeftTexture = walkingLeft;
            walkingRightTexture = walkingRight;

            MovementSpeed = 130f;
            CollisionDamage = 35;
            CurrentState = OgreBossState.Idle;
            animTimer = 0f;
            frameIndex = 0;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            stateTimer += delta;

            // In Idle state, if not locked, update lastTargetPosition and fixedAngle.
            if (CurrentState == OgreBossState.Idle && !idleAngleLocked)
            {
                lastTargetPosition = playerPosition;
                if (playerPosition != Position)
                    fixedAngle = (float)Math.Atan2(playerPosition.Y - Position.Y, playerPosition.X - Position.X);
            }

            float distance = Vector2.Distance(Position, playerPosition);

            // State transitions.
            if (!isAggro)
            {
                CurrentState = OgreBossState.Idle;
            }
            else
            {
                // When first damaged, lock the attack target and fixed angle.
                if (CurrentState == OgreBossState.Idle)
                {
                    attackTarget = playerPosition;
                    fixedAngle = (float)Math.Atan2(playerPosition.Y - Position.Y, playerPosition.X - Position.X);
                    idleAngleLocked = true;
                    CurrentState = OgreBossState.Aggro;
                }
                if (CurrentState == OgreBossState.Aggro)
                {
                    // Move toward the stored attack target.
                    Vector2 diff = attackTarget - Position;
                    if (diff != Vector2.Zero)
                    {
                        diff.Normalize();
                        Position += diff * MovementSpeed * runSpeedMultiplier * delta;
                    }
                    if (Vector2.Distance(Position, attackTarget) < meleeThreshold)
                    {
                        CurrentState = OgreBossState.Attack;
                        animTimer = 0f;
                        frameIndex = 0;
                        timeSinceLastShot = 0f;
                    }
                }
                else if (CurrentState == OgreBossState.Attack)
                {
                    timeSinceLastShot += delta;
                    if (timeSinceLastShot >= FiringInterval)
                    {
                        // Perform melee attack if in collision.
                        if (Bounds.Intersects(player.Bounds))
                        {
                            player.TakeDamage(meleeDamage);
                        }
                        CurrentState = OgreBossState.Reset;
                        resetTimer = 0f;
                        timeSinceLastShot = 0f;
                    }
                }
                else if (CurrentState == OgreBossState.Reset)
                {
                    resetTimer += delta;
                    if (resetTimer >= resetDuration)
                    {
                        CurrentState = OgreBossState.Idle;
                        isAggro = false;
                        idleAngleLocked = false;
                    }
                }
            }

            // Animation update.
            float frameTime;
            int totalFrames;
            int framesPerRowLocal;
            Texture2D currentTexture;

            // Choose directional textures based on the locked fixed angle.
            float angleDeg = MathHelper.ToDegrees(fixedAngle);
            Texture2D chosenIdle, chosenAttack, chosenWalking;
            if (angleDeg >= 45 && angleDeg < 135)
            {
                chosenIdle = idleDownTexture;
                chosenAttack = attackDownTexture;
                chosenWalking = walkingDownTexture;
            }
            else if (angleDeg >= 135 && angleDeg < 225)
            {
                chosenIdle = idleLeftTexture;
                chosenAttack = attackLeftTexture;
                chosenWalking = walkingLeftTexture;
            }
            else if (angleDeg >= 225 && angleDeg < 315)
            {
                chosenIdle = idleUpTexture;
                chosenAttack = attackUpTexture;
                chosenWalking = walkingUpTexture;
            }
            else
            {
                chosenIdle = idleRightTexture;
                chosenAttack = attackRightTexture;
                chosenWalking = walkingRightTexture;
            }

            if (CurrentState == OgreBossState.Idle)
            {
                currentTexture = chosenIdle;
                framesPerRowLocal = commonFramesPerRow;
                totalFrames = TotalFrames_Common;
                frameTime = idleFrameTime;
            }
            else if (CurrentState == OgreBossState.Aggro)
            {
                currentTexture = chosenWalking;
                framesPerRowLocal = walkingFramesPerRow;
                totalFrames = TotalFrames_Walking;
                frameTime = walkingFrameTime;
            }
            else // Attack or Reset.
            {
                currentTexture = chosenAttack;
                framesPerRowLocal = commonFramesPerRow;
                totalFrames = TotalFrames_Common;
                frameTime = attackFrameTime;
            }

            animTimer += delta;
            if (animTimer >= frameTime)
            {
                frameIndex = (frameIndex + 1) % totalFrames;
                animTimer = 0f;
            }

            // Update bullets (if any).
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

            // Choose directional textures based on fixedAngle.
            float angleDeg = MathHelper.ToDegrees(fixedAngle);
            Texture2D chosenIdle, chosenAttack, chosenWalking;
            if (angleDeg >= 45 && angleDeg < 135)
            {
                chosenIdle = idleDownTexture;
                chosenAttack = attackDownTexture;
                chosenWalking = walkingDownTexture;
            }
            else if (angleDeg >= 135 && angleDeg < 225)
            {
                chosenIdle = idleLeftTexture;
                chosenAttack = attackLeftTexture;
                chosenWalking = walkingLeftTexture;
            }
            else if (angleDeg >= 225 && angleDeg < 315)
            {
                chosenIdle = idleUpTexture;
                chosenAttack = attackUpTexture;
                chosenWalking = walkingUpTexture;
            }
            else
            {
                chosenIdle = idleRightTexture;
                chosenAttack = attackRightTexture;
                chosenWalking = walkingRightTexture;
            }

            Texture2D currentTexture;
            int framesPerRowLocal, rowsLocal;
            if (CurrentState == OgreBossState.Idle)
            {
                currentTexture = chosenIdle;
                framesPerRowLocal = commonFramesPerRow;
                rowsLocal = commonRows;
            }
            else if (CurrentState == OgreBossState.Aggro)
            {
                currentTexture = chosenWalking;
                framesPerRowLocal = walkingFramesPerRow;
                rowsLocal = walkingRows;
            }
            else
            {
                currentTexture = chosenAttack;
                framesPerRowLocal = commonFramesPerRow;
                rowsLocal = commonRows;
            }

            int frameW = currentTexture.Width / framesPerRowLocal;
            int frameH = currentTexture.Height / rowsLocal;
            Rectangle srcRect = new Rectangle((frameIndex % framesPerRowLocal) * frameW,
                                              (frameIndex / framesPerRowLocal) * frameH,
                                              frameW, frameH);
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);
            // No rotation applied because textures are directional.
            spriteBatch.Draw(currentTexture, Position, srcRect, Color.White, 0f, origin, Scale, SpriteEffects.None, 0f);

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

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            if (amount > 0 && CurrentState == OgreBossState.Idle)
            {
                isAggro = true;
                attackTarget = lastTargetPosition;
                CurrentState = OgreBossState.Aggro;
                idleAngleLocked = true;
            }
        }

        // OgreBoss is pure melee; override Shoot() to do nothing.
        protected override void Shoot()
        {
            // No projectile firing.
        }
    }
}

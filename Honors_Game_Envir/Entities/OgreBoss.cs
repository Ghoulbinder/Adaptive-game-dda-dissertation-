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

        // Directional textures.
        private Texture2D idleUpTexture, idleDownTexture, idleLeftTexture, idleRightTexture;
        private Texture2D attackUpTexture, attackDownTexture, attackLeftTexture, attackRightTexture;
        private Texture2D walkingUpTexture, walkingDownTexture, walkingLeftTexture, walkingRightTexture;

        // Animation settings for Idle and Attack.
        private const int commonFramesPerRow = 4;
        private const int commonRows = 5;
        private int TotalFrames_Common => commonFramesPerRow * commonRows;
        private float idleFrameTime = 0.15f;
        private float attackFrameTime = 0.1f;

        // Animation settings for Walking.
        private const int walkingFramesPerRow = 4;
        private const int walkingRows = 4;
        private int TotalFrames_Walking => walkingFramesPerRow * walkingRows;
        private float walkingFrameTime = 0.1f;

        private float animTimer = 0f;
        private int frameIndex = 0;

        // Behavior variables.
        private bool isAggro = false;
        private float meleeThreshold = 150f;
        private float runSpeedMultiplier = 2.0f;
        private int meleeDamage = 25;
        private float resetDuration = 2f;
        private float resetTimer = 0f;

        // For melee attack.
        private Vector2 attackTarget;
        private Vector2 lastTargetPosition;
        private float fixedAngle = 0f;
        private bool idleAngleLocked = false;

        /// <summary>
        /// Constructs an OgreBoss.
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

            // **** Experience gain modification: set boss exp reward ****
            this.ExperienceReward = 50;
            // **** End modification ****
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            stateTimer += delta;

            if (CurrentState == OgreBossState.Idle && !idleAngleLocked)
            {
                lastTargetPosition = playerPosition;
                if (playerPosition != Position)
                    fixedAngle = (float)Math.Atan2(playerPosition.Y - Position.Y, playerPosition.X - Position.X);
            }

            float distance = Vector2.Distance(Position, playerPosition);

            if (!isAggro)
            {
                CurrentState = OgreBossState.Idle;
            }
            else
            {
                if (CurrentState == OgreBossState.Idle)
                {
                    attackTarget = playerPosition;
                    fixedAngle = (float)Math.Atan2(playerPosition.Y - Position.Y, playerPosition.X - Position.X);
                    idleAngleLocked = true;
                    CurrentState = OgreBossState.Aggro;
                }
                if (CurrentState == OgreBossState.Aggro)
                {
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

            float frameTime;
            int totalFrames;
            int framesPerRowLocal;
            Texture2D currentTexture;

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
            else
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

        public override void TakeDamage(int amount, Player player)
        {
            base.TakeDamage(amount, player);
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

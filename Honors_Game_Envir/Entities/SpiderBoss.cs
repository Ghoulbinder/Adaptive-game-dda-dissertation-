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

        // Spider Boss textures:
        // Attack: 1280x1280, 4 columns x 4 rows (16 frames)
        private Texture2D attackTexture;
        // Idle: 1280x960, 4 columns x 3 rows (12 frames)
        private Texture2D idleTexture;
        // Walking: 960x960, 3 columns x 3 rows (9 frames)
        private Texture2D walkingTexture;

        // Animation settings for Attack.
        private const int attackFramesPerRow = 4;
        private const int attackRows = 4;
        private const int bossTotalFrames_Attack = attackFramesPerRow * attackRows; // 16 frames
        private float attackFrameTime = 0.1f;

        // Animation settings for Idle.
        private const int idleFramesPerRow = 4;
        private const int idleRows = 3;
        private const int bossTotalFrames_Idle = idleFramesPerRow * idleRows; // 12 frames
        private float idleFrameTime = 0.15f;

        // Animation settings for Walking.
        private const int walkingFramesPerRow = 3;
        private const int walkingRows = 3;
        private const int bossTotalFrames_Walking = walkingFramesPerRow * walkingRows; // 9 frames
        private float walkingFrameTime = 0.1f;

        private float animTimer = 0f;
        private int frameIndex = 0;

        private Vector2 lastTargetPosition;

        public SpiderBoss(
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
            this.idleTexture = idleTexture;
            this.attackTexture = attackTexture;
            this.walkingTexture = walkingTexture;
            MovementSpeed = 110f;
            FiringInterval = 1.3f;
            BulletRange = 500f;
            CollisionDamage = 25;
            CurrentState = SpiderBossState.Idle;
            animTimer = 0f;
            frameIndex = 0;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            stateTimer += delta;
            lastTargetPosition = playerPosition;
            float distance = Vector2.Distance(Position, playerPosition);

            if (distance > 300)
                CurrentState = SpiderBossState.Idle;
            else if (distance > 150)
                CurrentState = SpiderBossState.Walking;
            else
                CurrentState = SpiderBossState.Attack;

            float frameTime;
            int totalFrames;
            int framesPerRowLocal = 0;
            Texture2D currentTexture;
            switch (CurrentState)
            {
                case SpiderBossState.Idle:
                    currentTexture = idleTexture;
                    framesPerRowLocal = idleFramesPerRow;
                    totalFrames = bossTotalFrames_Idle;
                    frameTime = idleFrameTime;
                    break;
                case SpiderBossState.Walking:
                    currentTexture = walkingTexture;
                    framesPerRowLocal = walkingFramesPerRow;
                    totalFrames = bossTotalFrames_Walking;
                    frameTime = walkingFrameTime;
                    break;
                case SpiderBossState.Attack:
                    currentTexture = attackTexture;
                    framesPerRowLocal = attackFramesPerRow;
                    totalFrames = bossTotalFrames_Attack;
                    frameTime = attackFrameTime;
                    break;
                default:
                    currentTexture = idleTexture;
                    framesPerRowLocal = idleFramesPerRow;
                    totalFrames = bossTotalFrames_Idle;
                    frameTime = idleFrameTime;
                    break;
            }
            animTimer += delta;
            if (animTimer >= frameTime)
            {
                frameIndex = (frameIndex + 1) % totalFrames;
                animTimer = 0f;
            }

            if (CurrentState == SpiderBossState.Walking)
            {
                ChasePlayer(playerPosition);
            }
            else if (CurrentState == SpiderBossState.Attack)
            {
                timeSinceLastShot += delta;
                if (timeSinceLastShot >= FiringInterval)
                {
                    Shoot();
                    timeSinceLastShot = 0f;
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
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead) return;

            Texture2D currentTexture;
            int framesPerRowLocal, rowsLocal;
            switch (CurrentState)
            {
                case SpiderBossState.Idle:
                    currentTexture = idleTexture;
                    framesPerRowLocal = idleFramesPerRow;
                    rowsLocal = idleRows;
                    break;
                case SpiderBossState.Walking:
                    currentTexture = walkingTexture;
                    framesPerRowLocal = walkingFramesPerRow;
                    rowsLocal = walkingRows;
                    break;
                case SpiderBossState.Attack:
                    currentTexture = attackTexture;
                    framesPerRowLocal = attackFramesPerRow;
                    rowsLocal = attackRows;
                    break;
                default:
                    currentTexture = idleTexture;
                    framesPerRowLocal = idleFramesPerRow;
                    rowsLocal = idleRows;
                    break;
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

        protected override void Shoot()
        {
            Vector2 direction = lastTargetPosition - Position;
            if (direction != Vector2.Zero)
                direction.Normalize();
            else
                direction = new Vector2(1, 0);
            Vector2 bulletPos = Position;
            bullets.Add(new Bullet(
                bulletHorizontalTexture,
                bulletPos,
                direction,
                500f,
                BulletDamage,
                SpriteEffects.None,
                BulletRange
            ));
        }
    }
}

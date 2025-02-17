using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class DragonBoss : Boss
    {
        public enum DragonBossState { Idle, Walking, Attack, Death }
        public DragonBossState CurrentState { get; private set; } = DragonBossState.Idle;
        private float stateTimer = 0f;

        // Textures: all 1024x1024, 4 rows x 4 columns.
        private Texture2D idleTexture;
        private Texture2D attackTexture;
        private Texture2D walkingTexture;

        // Animation settings.
        private const int framesPerRow = 4;
        private const int rows = 4;
        private const int bossTotalFrames = framesPerRow * rows; // 16 frames
        private float idleFrameTime = 0.15f;
        private float commonFrameTime = 0.1f; // for walking and attack

        private float animTimer = 0f;
        private int frameIndex = 0;

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
            this.idleTexture = idleTexture;
            this.attackTexture = attackTexture;
            this.walkingTexture = walkingTexture;
            MovementSpeed = 120f;
            FiringInterval = 1.5f;
            BulletRange = 500f;
            CollisionDamage = 30;
            CurrentState = DragonBossState.Idle;
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
                CurrentState = DragonBossState.Idle;
            else if (distance > 150)
                CurrentState = DragonBossState.Walking;
            else
                CurrentState = DragonBossState.Attack;

            float frameTime = (CurrentState == DragonBossState.Idle) ? idleFrameTime : commonFrameTime;
            animTimer += delta;
            if (animTimer >= frameTime)
            {
                frameIndex = (frameIndex + 1) % bossTotalFrames;
                animTimer = 0f;
            }

            if (CurrentState == DragonBossState.Walking)
            {
                ChasePlayer(playerPosition);
            }
            else if (CurrentState == DragonBossState.Attack)
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
            if (IsDead)
                return;

            Texture2D currentTexture = (CurrentState == DragonBossState.Idle) ? idleTexture :
                                         (CurrentState == DragonBossState.Walking) ? walkingTexture :
                                         attackTexture;
            int frameW = currentTexture.Width / framesPerRow;
            int frameH = currentTexture.Height / rows;
            Rectangle srcRect = new Rectangle((frameIndex % framesPerRow) * frameW,
                                              (frameIndex / framesPerRow) * frameH,
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

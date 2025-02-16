using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class ButterflyBoss : Boss
    {
        // Define the animation states for the ButterflyBoss.
        public enum ButterflyBossState { Idle, Walking, Attack, Death }
        public ButterflyBossState CurrentState { get; set; } = ButterflyBossState.Idle;

        // The idle animation uses a different sprite sheet.
        private Texture2D idleTexture; // 1536x1024, 6 columns x 4 rows (24 frames)
        // The common animation sprite sheet for Attack, Walking and Death.
        private Texture2D commonTexture; // 1024x1280, 4 columns x 5 rows (20 frames)

        // Animation settings for idle.
        private const int idleFramesPerRow = 6;
        private const int idleRows = 4;
        private const int idleTotalFrames = 24;
        private float idleFrameTime = 0.15f;

        // Animation settings for common animations.
        private const int commonFramesPerRow = 4;
        private const int commonRows = 5;
        private const int commonTotalFrames = 20;
        private float commonFrameTime = 0.08f;

        private float animationTimer = 0f;
        private int currentFrame = 0;

        /// <summary>
        /// Constructs a new ButterflyBoss.
        /// idleTexture is used for Idle state;
        /// commonTexture is used for Attack, Walking, and Death states.
        /// </summary>
        public ButterflyBoss(
            Texture2D idleTexture, Texture2D commonTexture,
            Texture2D bulletHorizontal, Texture2D bulletVertical,
            Vector2 startPosition, Direction startDirection,
            int health, int bulletDamage)
            : base(idleTexture, idleTexture, idleTexture, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            // For the base Boss we pass idleTexture for now.
            // Store our specialized textures.
            this.idleTexture = idleTexture;
            this.commonTexture = commonTexture;

            // Override boss-specific stats.
            MovementSpeed = 120f;
            FiringInterval = 1.5f;
            BulletRange = 500f;
            CollisionDamage = 30;

            CurrentState = ButterflyBossState.Idle;
            currentFrame = 0;
            animationTimer = 0f;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            // For testing, we update only the animation.
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            animationTimer += delta;

            float frameTime = 0f;
            int totalFrames = 0;
            int framesPerRow = 0;

            // Use idle sprite sheet for Idle; common sprite sheet for other states.
            if (CurrentState == ButterflyBossState.Idle)
            {
                frameTime = idleFrameTime;
                totalFrames = idleTotalFrames;
                framesPerRow = idleFramesPerRow;
            }
            else
            {
                frameTime = commonFrameTime;
                totalFrames = commonTotalFrames;
                framesPerRow = commonFramesPerRow;
            }

            if (animationTimer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                animationTimer = 0f;
            }

            // You can add AI behavior for movement, attacking, etc. here.

            // Update bullets as usual.
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

            // Select the texture based on state.
            Texture2D currentTexture = (CurrentState == ButterflyBossState.Idle) ? idleTexture : commonTexture;

            int textureWidth = currentTexture.Width;
            int textureHeight = currentTexture.Height;
            int frameW, frameH, framesPerRow;
            if (CurrentState == ButterflyBossState.Idle)
            {
                framesPerRow = idleFramesPerRow;
                frameW = textureWidth / idleFramesPerRow;    // 1536 / 6 = 256
                frameH = textureHeight / idleRows;             // 1024 / 4 = 256
            }
            else
            {
                framesPerRow = commonFramesPerRow;
                frameW = textureWidth / commonFramesPerRow;    // 1024 / 4 = 256
                frameH = textureHeight / commonRows;           // 1280 / 5 = 256
            }

            Rectangle srcRect = new Rectangle((currentFrame % framesPerRow) * frameW,
                                              (currentFrame / framesPerRow) * frameH,
                                              frameW, frameH);

            spriteBatch.Draw(currentTexture, Position, srcRect, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
        }
    }
}

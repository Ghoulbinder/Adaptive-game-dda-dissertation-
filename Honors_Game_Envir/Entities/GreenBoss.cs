using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class GreenBoss : Boss
    {
        public enum GreenBossState { Idle, Patrol, Chase, Attack, Enraged, Dead }
        public GreenBossState CurrentState { get; private set; } = GreenBossState.Idle;
        private float stateTimer = 0f;

        public GreenBoss(
            Texture2D back,
            Texture2D front,
            Texture2D left,
            Texture2D bulletHorizontal,
            Texture2D bulletVertical,
            Vector2 startPosition,
            Direction startDirection,
            int health,
            int bulletDamage)
            : base(back, front, left, bulletHorizontal, bulletVertical, startPosition, startDirection, health, bulletDamage)
        {
            MovementSpeed = 140f;
            FiringInterval = 1.2f;
            BulletRange = 550f;
            CollisionDamage = 35;
            CurrentState = GreenBossState.Idle;
            stateTimer = 0f;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            stateTimer += delta;
            float distance = Vector2.Distance(Position, playerPosition);

            if (distance <= 200)
            {
                CurrentState = GreenBossState.Attack;
                timeSinceLastShot += delta;
                if (timeSinceLastShot >= FiringInterval)
                {
                    Shoot();
                    timeSinceLastShot = 0f;
                }
            }
            else if (distance > 200 && distance <= 400)
            {
                CurrentState = GreenBossState.Chase;
                this.ChasePlayer(playerPosition);
            }
            else
            {
                CurrentState = GreenBossState.Patrol;
                this.Patrol(viewport);
            }

            timer += delta;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }
            UpdateFrameDimensions();

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
    }
}

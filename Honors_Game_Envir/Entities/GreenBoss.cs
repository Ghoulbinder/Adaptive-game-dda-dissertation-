using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Survivor_of_the_Bulge
{
    public class GreenBoss : Boss
    {
        public enum GreenBossState { Idle, Patrol, Chase, Attack, Enraged, Dead }
        public GreenBossState CurrentState { get; private set; } = GreenBossState.Idle;

        private float stateTimer = 0f;
        private bool isAggro = false;              // Becomes true when the boss takes damage.
        private Vector2 attackTarget;              // Player position when boss was hit.
        private Vector2 lastTargetPosition;        // Updated each frame with the current player's position.

        // Behavior thresholds.
        private readonly float shootingRange = 200f;      // Distance within which the boss attacks.
        private readonly float chaseThreshold = 400f;     // Distance within which the boss chases.
        private readonly float aggroChaseMultiplier = 1.5f; // Speed multiplier when in aggro mode.

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
            // PSEUDOCODE: Initialize base movement, firing, and damage parameters
            MovementSpeed = 140f;
            FiringInterval = 1.2f;
            BulletRange = 550f;
            CollisionDamage = 35;
            CurrentState = GreenBossState.Idle;
            stateTimer = 0f;

            // PSEUDOCODE: Set experience reward on defeat
            this.ExperienceReward = 50;
        }

        public override void Update(GameTime gameTime, Viewport viewport, Vector2 playerPosition, Player player)
        {
            // PSEUDOCODE: Compute time delta and track last player position
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            stateTimer += delta;
            lastTargetPosition = playerPosition;

            // PSEUDOCODE: Calculate distance to player
            float distance = Vector2.Distance(Position, playerPosition);

            if (isAggro)
            {
                // PSEUDOCODE: If aggro, chase the locked target until within shooting range
                if (Vector2.Distance(Position, attackTarget) > shootingRange)
                {
                    Vector2 diff = attackTarget - Position;
                    if (diff != Vector2.Zero)
                    {
                        diff.Normalize();
                        Position += diff * MovementSpeed * aggroChaseMultiplier * delta;
                        currentDirection = (Math.Abs(diff.X) > Math.Abs(diff.Y))
                            ? (diff.X < 0 ? Direction.Left : Direction.Right)
                            : (diff.Y < 0 ? Direction.Up : Direction.Down);
                    }
                    CurrentState = GreenBossState.Chase;
                }
                else
                {
                    // PSEUDOCODE: When within range, attack and then exit aggro mode
                    CurrentState = GreenBossState.Attack;
                    timeSinceLastShot += delta;
                    if (timeSinceLastShot >= FiringInterval)
                    {
                        Shoot();
                        timeSinceLastShot = 0f;
                        isAggro = false;
                        CurrentState = GreenBossState.Idle;
                        stateTimer = 0f;
                    }
                }
            }
            else
            {
                // PSEUDOCODE: Standard behavior: attack, chase or patrol based on distance
                if (distance <= shootingRange)
                {
                    CurrentState = GreenBossState.Attack;
                    Vector2 diff = playerPosition - Position;
                    if (diff != Vector2.Zero)
                    {
                        diff.Normalize();
                        currentDirection = (Math.Abs(diff.X) > Math.Abs(diff.Y))
                            ? (diff.X < 0 ? Direction.Left : Direction.Right)
                            : (diff.Y < 0 ? Direction.Up : Direction.Down);
                    }
                    timeSinceLastShot += delta;
                    if (timeSinceLastShot >= FiringInterval)
                    {
                        Shoot();
                        timeSinceLastShot = 0f;
                    }
                }
                else if (distance > shootingRange && distance <= chaseThreshold)
                {
                    CurrentState = GreenBossState.Chase;
                    ChasePlayer(playerPosition);
                }
                else
                {
                    CurrentState = GreenBossState.Patrol;
                    Patrol(viewport);
                }
            }

            // PSEUDOCODE: Advance animation frame when timer exceeds frameTime
            timer += delta;
            if (timer >= frameTime)
            {
                currentFrame = (currentFrame + 1) % totalFrames;
                timer = 0f;
            }
            UpdateFrameDimensions();

            // PSEUDOCODE: Update bullets and handle collisions with player
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

        public override void TakeDamage(int amount, Player player)
        {
            // PSEUDOCODE: Apply damage and trigger aggro mode when hit
            base.TakeDamage(amount, player);
            if (amount > 0)
            {
                isAggro = true;
                attackTarget = lastTargetPosition;
            }
        }

        // Modified Shoot method: choose bullet texture based on current direction.
        protected override void Shoot()
        {
            // PSEUDOCODE: Determine shooting direction and texture
            Vector2 direction = Vector2.Zero;
            Texture2D bulletTexture = null;
            switch (currentDirection)
            {
                case Direction.Up:
                    direction = new Vector2(0, -1);
                    bulletTexture = bulletVerticalTexture;
                    break;
                case Direction.Down:
                    direction = new Vector2(0, 1);
                    bulletTexture = bulletVerticalTexture;
                    break;
                case Direction.Left:
                    direction = new Vector2(-1, 0);
                    bulletTexture = bulletHorizontalTexture;
                    break;
                case Direction.Right:
                    direction = new Vector2(1, 0);
                    bulletTexture = bulletHorizontalTexture;
                    break;
            }

            // PSEUDOCODE: Spawn and add new bullet
            Vector2 bulletPos = Position + direction * 20f;
            Bullet bullet = new Bullet(
                bulletTexture,
                bulletPos,
                direction,
                500f,
                BulletDamage,
                SpriteEffects.None,
                BulletRange
            );
            bullets.Add(bullet);
        }
    }
}

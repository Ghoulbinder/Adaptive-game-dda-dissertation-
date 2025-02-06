using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survivor_of_the_Bulge
{
    public class SnowFlake
    {
        private Texture2D texture;      // The snowflake texture.
        private Vector2 position;       // Current position.
        private Vector2 velocity;       // Falling speed.
        private float rotation;         // Current rotation.
        private float rotationSpeed;    // How fast the snowflake rotates.
        private Random rng;             // Random generator for resetting.

        // Constructor: initializes the snowflake at a random x-position at the top.
        public SnowFlake(Texture2D texture, Random rng, int maxX)
        {
            this.texture = texture;
            this.rng = rng;
            position = new Vector2(rng.Next(0, maxX), 0);
            // Snow falls slower than leaves.
            velocity = new Vector2(0, (float)rng.NextDouble() * 0.5f + 0.1f);
            rotation = 0f;
            // Snow rotates more slowly.
            rotationSpeed = ((float)rng.NextDouble() - 0.5f) / 8f;
        }

        // Update the snowflake's position and rotation.
        public void Update(int maxX, int maxY)
        {
            position += velocity;
            rotation += rotationSpeed;

            // When the snowflake falls below the screen, reset it.
            if (position.Y > maxY)
            {
                position = new Vector2(rng.Next(0, maxX), 0);
                velocity = new Vector2(0, (float)rng.NextDouble() * 0.5f + 0.1f);
                rotationSpeed = ((float)rng.NextDouble() - 0.5f) / 8f;
            }
        }

        // Draw the snowflake.
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White * 0.9f,
                rotation, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}

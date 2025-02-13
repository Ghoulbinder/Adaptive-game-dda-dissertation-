using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survivor_of_the_Bulge
{
    public class FallingLeaf
    {
        private Texture2D texture;      // The leaf texture.
        private Vector2 position;       // Current position.
        private Vector2 velocity;       // Falling speed.
        private float rotation;         // Current rotation.
        private float rotationSpeed;    // How fast the leaf rotates.
        private Random rng;             // Random generator for resetting.

        // Constructor: initializes the leaf at a random x-position at the top.
        public FallingLeaf(Texture2D texture, Random rng, int maxX)
        {
            this.texture = texture;
            this.rng = rng;
            position = new Vector2(rng.Next(0, maxX), 0);
            // Leaves fall at a vertical speed between ~0.25f and 1.25f.
            velocity = new Vector2(0, (float)rng.NextDouble() + 0.25f);
            rotation = 0f;
            // Rotation speed is between roughly -0.125 and +0.125 radians per update.
            rotationSpeed = ((float)rng.NextDouble() - 0.5f) / 4f;
        }

        // Update the leaf's position and rotation.
        public void Update(int maxX, int maxY)
        {
            position += velocity;
            rotation += rotationSpeed;

            // When the leaf falls below the screen, reset it at a new random x-position at the top.
            if (position.Y > maxY)
            {
                position = new Vector2(rng.Next(0, maxX), 0);
                velocity = new Vector2(0, (float)rng.NextDouble() + 0.25f);
                rotationSpeed = ((float)rng.NextDouble() - 0.5f) / 4f;
            }
        }

        // Draw the leaf with some transparency.
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White * 0.75f,
                rotation, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Map
    {
        public Texture2D Background { get; }
        private List<Enemy> Enemies;

        public Map(Texture2D background, List<Enemy> enemies)
        {
            Background = background;
            Enemies = enemies;
        }

        public void UpdateEnemies(GameTime gameTime, Viewport viewport, Vector2 playerPosition)
        {
            foreach (var enemy in Enemies)
            {
                enemy.Update(gameTime, viewport, playerPosition); // Pass player position
            }
        }

        public void DrawEnemies(SpriteBatch spriteBatch)
        {
            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }
        }
    }
}

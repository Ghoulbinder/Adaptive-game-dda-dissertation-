using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Survivor_of_the_Bulge
{
    public class Map
    {
        // The background texture for the map.
        public Texture2D Background { get; }
        // A public, modifiable list of enemies on this map.
        public List<Enemy> Enemies { get; }

        public Map(Texture2D background, List<Enemy> enemies)
        {
            Background = background;
            Enemies = enemies;
        }

        // NEW: A helper method to easily add an enemy to this map.
        public void AddEnemy(Enemy enemy)
        {
            Enemies.Add(enemy);
        }

        // Draws all enemies that are part of this map.
        public void DrawEnemies(SpriteBatch spriteBatch)
        {
            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class TreesBox
    {
        private Texture2D texture;
        private Vector2 position;
        public Rectangle Bounds => new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);

        public TreesBox(Texture2D texture, int x, int y)
        {
            this.texture = texture;
            position = new Vector2(x, y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }
}

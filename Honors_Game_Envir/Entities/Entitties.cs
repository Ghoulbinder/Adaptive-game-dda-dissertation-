using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Enemy
{
    private Texture2D texture;
    private Vector2 position;
    private Vector2 direction;
    private float speed;

    public Enemy(Texture2D texture, Vector2 startPosition, Vector2 direction, float speed)
    {
        this.texture = texture;
        this.position = startPosition;
        this.direction = direction;
        this.speed = speed;
    }

    public void Update(GameTime gameTime)
    {
        position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, position, Color.White);
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survivor_of_the_Bulge
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player player;
        private enum GameState { MainMenu, GreenForestCentre, ForestTop, ForestButtom, ForestLeft, ForestRight }
        private GameState currentState;

        private Texture2D mainMenuBackground;
        private Texture2D greenForestBackground;
        private Texture2D forestTopBackground;
        private Texture2D forestButtomBackground;
        private Texture2D forestLeftBackground;
        private Texture2D forestRightBackground;
        private Texture2D gridLineTexture;
        private SpriteFont gameFont;

        private MenuState menuState;
        private const int TileSize = 25;
        private Rectangle mapBounds;

        private bool showGrid = false; // Toggle grid visibility

        private List<(Rectangle transitionZone, GameState fromState, GameState toState)> transitions;
        private Dictionary<GameState, int[,]> mapGrids;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1600;
            _graphics.PreferredBackBufferHeight = 980;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            currentState = GameState.MainMenu;
            mapBounds = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            InitializeTransitions();
            InitializeGrids();
            base.Initialize();
        }

        private void InitializeTransitions()
        {
            int screenWidth = _graphics.PreferredBackBufferWidth;
            int screenHeight = _graphics.PreferredBackBufferHeight;

            transitions = new List<(Rectangle, GameState, GameState)>
            {
                (new Rectangle(0, 0, screenWidth, TileSize * 2), GameState.GreenForestCentre, GameState.ForestTop),
                (new Rectangle(0, screenHeight - (TileSize * 2), screenWidth, TileSize * 2), GameState.ForestTop, GameState.GreenForestCentre),
                (new Rectangle(0, screenHeight - (TileSize * 2), screenWidth, TileSize * 2), GameState.GreenForestCentre, GameState.ForestButtom),
                (new Rectangle(0, 0, screenWidth, TileSize * 2), GameState.ForestButtom, GameState.GreenForestCentre),
                (new Rectangle(0, 0, TileSize * 2, screenHeight), GameState.GreenForestCentre, GameState.ForestLeft),
                (new Rectangle(screenWidth - (TileSize * 2), 0, TileSize * 2, screenHeight), GameState.ForestLeft, GameState.GreenForestCentre),
                (new Rectangle(screenWidth - (TileSize * 2), 0, TileSize * 2, screenHeight), GameState.GreenForestCentre, GameState.ForestRight),
                (new Rectangle(0, 0, TileSize * 2, screenHeight), GameState.ForestRight, GameState.GreenForestCentre)
            };
        }

        private void InitializeGrids()
        {
            int gridWidth = _graphics.PreferredBackBufferWidth / TileSize;
            int gridHeight = _graphics.PreferredBackBufferHeight / TileSize;

            mapGrids = new Dictionary<GameState, int[,]>
            {
                { GameState.GreenForestCentre, new int[gridHeight, gridWidth] },
                { GameState.ForestTop, new int[gridHeight, gridWidth] },
                { GameState.ForestButtom, new int[gridHeight, gridWidth] },
                { GameState.ForestLeft, new int[gridHeight, gridWidth] },
                { GameState.ForestRight, new int[gridHeight, gridWidth] }
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            mainMenuBackground = Content.Load<Texture2D>("Images/Maps/mmBackground");
            greenForestBackground = Content.Load<Texture2D>("Images/Maps/greenForestCentre");
            forestTopBackground = Content.Load<Texture2D>("Images/Maps/snowForestTop");
            forestButtomBackground = Content.Load<Texture2D>("Images/Maps/snowForestButtom");
            forestLeftBackground = Content.Load<Texture2D>("Images/Maps/snowForestLeft");
            forestRightBackground = Content.Load<Texture2D>("Images/Maps/snowForestRight");

            gridLineTexture = new Texture2D(GraphicsDevice, 1, 1);
            gridLineTexture.SetData(new[] { Color.White });

            gameFont = Content.Load<SpriteFont>("Fonts/jungleFont");

            player = new Player(
                Content.Load<Texture2D>("Images/Soldier/backWalking"),
                Content.Load<Texture2D>("Images/Soldier/frontWalking"),
                Content.Load<Texture2D>("Images/Soldier/leftWalking"),
                new Vector2(100, 100)
            );

            menuState = new MenuState(gameFont, mainMenuBackground);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.G)) // Toggle grid visibility with 'G'
                showGrid = !showGrid;

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && currentState == GameState.MainMenu)
            {
                currentState = GameState.GreenForestCentre;
            }

            if (currentState != GameState.MainMenu)
            {
                Rectangle playerHitbox = new Rectangle((int)player.Position.X, (int)player.Position.Y, TileSize, TileSize);

                foreach (var transition in transitions)
                {
                    if (transition.transitionZone.Intersects(playerHitbox) && transition.fromState == currentState)
                    {
                        currentState = transition.toState;
                        player.Position = new Vector2(
                            _graphics.PreferredBackBufferWidth / 2,
                            _graphics.PreferredBackBufferHeight / 2
                        );
                        break;
                    }
                }

                player.Update(gameTime, _graphics.GraphicsDevice.Viewport);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            float scaleX = (float)_graphics.PreferredBackBufferWidth / greenForestBackground.Width;
            float scaleY = (float)_graphics.PreferredBackBufferHeight / greenForestBackground.Height;
            Vector2 scale = new Vector2(scaleX, scaleY);

            Texture2D currentBackground = currentState switch
            {
                GameState.GreenForestCentre => greenForestBackground,
                GameState.ForestTop => forestTopBackground,
                GameState.ForestButtom => forestButtomBackground,
                GameState.ForestLeft => forestLeftBackground,
                GameState.ForestRight => forestRightBackground,
                _ => mainMenuBackground
            };

            _spriteBatch.Draw(currentBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            // Draw Grid Overlay (Only if Enabled)
            if (showGrid)
            {
                for (int y = 0; y < _graphics.PreferredBackBufferHeight; y += TileSize)
                {
                    for (int x = 0; x < _graphics.PreferredBackBufferWidth; x += TileSize)
                    {
                        _spriteBatch.Draw(gridLineTexture, new Rectangle(x, 0, 1, _graphics.PreferredBackBufferHeight), Color.White * 0.3f);
                        _spriteBatch.Draw(gridLineTexture, new Rectangle(0, y, _graphics.PreferredBackBufferWidth, 1), Color.White * 0.3f);
                    }
                }
            }

            player.Draw(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

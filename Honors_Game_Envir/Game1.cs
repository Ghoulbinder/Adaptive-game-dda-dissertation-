using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DDA_Tool;

namespace Survivor_of_the_Bulge
{
    public enum GameState { MainMenu, GreenForestCentre, ForestTop, ForestLeft, ForestButtom, ForestRight, Scoreboard }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player player;
        private PlayerStats playerStats;
        private GameState currentState;

        private Texture2D mainMenuBackground;
        private SpriteFont gameFont;

        private MenuState mainMenu;
        private PauseMenu pauseMenu;
        private const int TileSize = 25;

        private KeyboardState previousKeyboardState;

        private List<Transition> transitions;
        private Dictionary<GameState, Map> maps;

        // Weather effects.
        private List<FallingLeaf> fallingLeaves;
        private List<SnowFlake> snowFlakes;
        private Random random = new Random();

        private bool isPaused = false;

        // *** Autosave/Scoreboard Fields ***
        private GameData gameData; // persistent data loaded from XML
        public int currentLevel = 1;
        public int currentLives = 3;
        public int currentScore = 0;
        public int bulletsFiredThisSession = 0;
        public int bulletsUsedAgainstEnemiesThisSession = 0;
        public int bulletsUsedAgainstBossesThisSession = 0;
        public int livesLostThisSession = 0;
        public int deathsThisSession = 0;
        private DateTime sessionStartTime;
        // *** End Autosave/Scoreboard Fields ***

        // New fields for dynamic difficulty.
        private DynamicDifficultyController difficultyController;
        private DifficultyLevel previousDifficulty;

        // New field for displaying a temporary difficulty notification.
        private DifficultyNotification difficultyNotification;

        // New field for scoreboard state.
        private ScoreboardScreen scoreboardScreen;

        // Singleton reference.
        public static Game1 Instance { get; private set; }

        public Game1()
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.PreferredBackBufferWidth = 1600;
            _graphics.PreferredBackBufferHeight = 980;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            currentState = GameState.MainMenu;
            InitializeTransitions();
            previousKeyboardState = Keyboard.GetState();

            // Load persistent game data.
            gameData = SaveLoadManager.LoadGameData();

            // Initialize session tracking variables.
            currentLevel = 1;
            currentLives = 3;
            currentScore = 0;
            bulletsFiredThisSession = 0;
            bulletsUsedAgainstEnemiesThisSession = 0;
            bulletsUsedAgainstBossesThisSession = 0;
            livesLostThisSession = 0;
            deathsThisSession = 0;
            sessionStartTime = DateTime.Now;

            // Initialize our dynamic difficulty controller.
            difficultyController = new DynamicDifficultyController();
            previousDifficulty = difficultyController.CurrentDifficulty;

            base.Initialize();
        }

        private void InitializeTransitions()
        {
            transitions = new List<Transition>
            {
                new Transition(GameState.GreenForestCentre, GameState.ForestTop, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, TileSize * 2)),
                new Transition(GameState.ForestTop, GameState.GreenForestCentre, new Rectangle(0, _graphics.PreferredBackBufferHeight - (TileSize * 2), _graphics.PreferredBackBufferWidth, TileSize * 2)),
                new Transition(GameState.GreenForestCentre, GameState.ForestButtom, new Rectangle(0, _graphics.PreferredBackBufferHeight - (TileSize * 2), _graphics.PreferredBackBufferWidth, TileSize * 2)),
                new Transition(GameState.ForestButtom, GameState.GreenForestCentre, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, TileSize * 2)),
                new Transition(GameState.GreenForestCentre, GameState.ForestLeft, new Rectangle(0, 0, TileSize * 2, _graphics.PreferredBackBufferHeight)),
                new Transition(GameState.ForestLeft, GameState.GreenForestCentre, new Rectangle(_graphics.PreferredBackBufferWidth - (TileSize * 2), 0, TileSize * 2, _graphics.PreferredBackBufferHeight)),
                new Transition(GameState.GreenForestCentre, GameState.ForestRight, new Rectangle(_graphics.PreferredBackBufferWidth - (TileSize * 2), 0, TileSize * 2, _graphics.PreferredBackBufferHeight)),
                new Transition(GameState.ForestRight, GameState.GreenForestCentre, new Rectangle(0, 0, TileSize * 2, _graphics.PreferredBackBufferHeight))
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            mainMenuBackground = Content.Load<Texture2D>("Images/Maps/mmBackground2");
            gameFont = Content.Load<SpriteFont>("Fonts/jungleFont");

            // Load bullet textures.
            Texture2D bulletTexture1 = Content.Load<Texture2D>("Images/Projectile/bullet");
            Texture2D bulletTexture2 = Content.Load<Texture2D>("Images/Projectile/bullet2");

            // Load player textures.
            Texture2D walkUp = Content.Load<Texture2D>("Player_Ranged/PlayerRangeWalking/PlayerRangeWalkingUp");
            Texture2D walkDown = Content.Load<Texture2D>("Player_Ranged/PlayerRangeWalking/PlayerRangeWalkingDown");
            Texture2D walkLeft = Content.Load<Texture2D>("Player_Ranged/PlayerRangeWalking/PlayerRangeWalkingLeft");
            Texture2D walkRight = Content.Load<Texture2D>("Player_Ranged/PlayerRangeWalking/PlayerRangeWalkingRight");

            Texture2D attackUp = Content.Load<Texture2D>("Player_Ranged/PlayerRangeAttack/PlayerRangeAttackUp");
            Texture2D attackDown = Content.Load<Texture2D>("Player_Ranged/PlayerRangeAttack/PlayerRangeAttackDown");
            Texture2D attackLeft = Content.Load<Texture2D>("Player_Ranged/PlayerRangeAttack/PlayerRangeAttackLeft");
            Texture2D attackRight = Content.Load<Texture2D>("Player_Ranged/PlayerRangeAttack/PlayerRangeAttackRight");

            // Create player stats and instantiate the player.
            PlayerStats stats = new PlayerStats(100, 3, 50, 1.0f, 200f, 0, 1, gameFont);
            player = new Player(
                walkUp, walkDown, walkLeft, walkRight,
                attackUp, attackDown, attackLeft, attackRight,
                bulletTexture1, bulletTexture2,
                new Vector2(100, 100),
                stats
            );
            player.Scale = 0.3f;
            playerStats = stats;

            mainMenu = new MenuState(gameFont, mainMenuBackground);
            pauseMenu = new PauseMenu(gameFont, CreatePanelTexture());

            InitializeMaps();

            // Adjust viewport based on the GreenForestCentre background.
            var largestMap = maps[GameState.GreenForestCentre];
            _graphics.PreferredBackBufferWidth = largestMap.Background.Width;
            _graphics.PreferredBackBufferHeight = largestMap.Background.Height;
            _graphics.ApplyChanges();

            // Load weather textures.
            Texture2D leafTexture = Content.Load<Texture2D>("Images/Maps/tinyleaf");
            Texture2D snowTexture = Content.Load<Texture2D>("Images/Maps/snowFlake");
            int particleCount = 20;
            fallingLeaves = new List<FallingLeaf>();
            snowFlakes = new List<SnowFlake>();
            for (int i = 0; i < particleCount; i++)
            {
                fallingLeaves.Add(new FallingLeaf(leafTexture, random, _graphics.PreferredBackBufferWidth));
                snowFlakes.Add(new SnowFlake(snowTexture, random, _graphics.PreferredBackBufferWidth));
            }
        }

        private Texture2D CreatePanelTexture()
        {
            Texture2D panelTexture = new Texture2D(GraphicsDevice, 1, 1);
            panelTexture.SetData(new[] { Color.Gray });
            return panelTexture;
        }

        private void InitializeMaps()
        {
            maps = new Dictionary<GameState, Map>
            {
                { GameState.GreenForestCentre, new Map(Content.Load<Texture2D>("Images/Maps/greenForestCentre2"), new List<Enemy>()) },
                { GameState.ForestTop, new Map(Content.Load<Texture2D>("Images/Maps/snowForestTop2"), new List<Enemy>()) },
                { GameState.ForestLeft, new Map(Content.Load<Texture2D>("Images/Maps/snowForestLeft2"), new List<Enemy>()) },
                { GameState.ForestButtom, new Map(Content.Load<Texture2D>("Images/Maps/snowForestButtom2"), new List<Enemy>()) },
                { GameState.ForestRight, new Map(Content.Load<Texture2D>("Images/Maps/snowForestRight2"), new List<Enemy>()) }
            };

            Texture2D enemyBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/bullet");
            Texture2D enemyBulletVertical = Content.Load<Texture2D>("Images/Projectile/bullet2");
            Texture2D enemyBack = Content.Load<Texture2D>("Images/Enemy/enemyBackWalking");
            Texture2D enemyFront = Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking");
            Texture2D enemyLeft = Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking");

            int initialEnemyCount = 2;
            SpawnEnemiesForMap(maps[GameState.GreenForestCentre], initialEnemyCount,
                               enemyBack, enemyFront, enemyLeft,
                               enemyBulletHorizontal, enemyBulletVertical);

            foreach (var map in maps.Values)
            {
                map.SetEnemySpawnParameters(enemyBack, enemyFront, enemyLeft, enemyBulletHorizontal, enemyBulletVertical);
            }
        }

        private void SpawnEnemiesForMap(Map map, int enemyCount,
                                        Texture2D enemyBack,
                                        Texture2D enemyFront,
                                        Texture2D enemyLeft,
                                        Texture2D enemyBulletHorizontal,
                                        Texture2D enemyBulletVertical)
        {
            Random rng = new Random();
            for (int i = 0; i < enemyCount; i++)
            {
                int x = rng.Next(0, map.Background.Width - 64);
                int y = rng.Next(0, map.Background.Height - 64);
                Array dirs = Enum.GetValues(typeof(Enemy.Direction));
                Enemy.Direction dir = (Enemy.Direction)dirs.GetValue(rng.Next(dirs.Length));

                int baseHealth = 50;
                int baseDamage = 5;
                int finalHealth = (int)(baseHealth * difficultyController.EnemyHealthMultiplier);
                int finalDamage = (int)(baseDamage * difficultyController.EnemyDamageMultiplier);

                Enemy e = new Enemy(
                    enemyBack,
                    enemyFront,
                    enemyLeft,
                    enemyBulletHorizontal,
                    enemyBulletVertical,
                    new Vector2(x, y),
                    dir,
                    finalHealth,
                    finalDamage
                );
                map.AddEnemy(e);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();

            // Handle difficulty key input.
            if (currentKeyboardState.IsKeyDown(Keys.D1))
            {
                difficultyController.HandleKeyInput('1');
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D2))
            {
                difficultyController.HandleKeyInput('2');
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D3))
            {
                difficultyController.HandleKeyInput('3');
            }
            // If difficulty changed, show notification.
            if (difficultyController.CurrentDifficulty != previousDifficulty)
            {
                difficultyNotification = new DifficultyNotification("Game difficulty changed to " + difficultyController.CurrentDifficulty.ToString(), 5.0f, gameFont);
                previousDifficulty = difficultyController.CurrentDifficulty;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Tab) && previousKeyboardState.IsKeyUp(Keys.Tab))
            {
                isPaused = !isPaused;
                IsMouseVisible = isPaused;
            }

            // Instead of exiting immediately, if Escape is pressed, signal game over.
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
            {
                GameOver();
            }

            if (currentState == GameState.MainMenu && currentKeyboardState.IsKeyDown(Keys.Enter))
            {
                currentState = GameState.GreenForestCentre;
            }

            if (isPaused)
            {
                pauseMenu.Update(gameTime);
            }
            else if (currentState != GameState.MainMenu)
            {
                if (currentState == GameState.Scoreboard)
                {
                    scoreboardScreen.Update(gameTime);
                    if (scoreboardScreen.Finished)
                        Environment.Exit(0);
                }
                else
                {
                    var currentMap = maps[currentState];

                    player.Update(gameTime, _graphics.GraphicsDevice.Viewport, currentMap.Enemies);
                    playerStats.UpdateHealth(player.Health);

                    // Remove dead enemies and update kill count.
                    for (int i = currentMap.Enemies.Count - 1; i >= 0; i--)
                    {
                        if (currentMap.Enemies[i].IsDead)
                        {
                            currentMap.IncrementKillCount();
                            currentMap.Enemies.RemoveAt(i);
                        }
                    }

                    // Spawn boss if kill count reaches the value from the difficulty controller and boss hasn't been spawned.
                    if (currentMap.KillCount >= difficultyController.BossSpawnThreshold && !currentMap.BossSpawned)
                    {
                        Vector2 bossPos = new Vector2(
                            (currentMap.Background.Width - 256) / 2,
                            (currentMap.Background.Height - 256) / 2
                        );

                        switch (currentState)
                        {
                            case GameState.GreenForestCentre:
                                {
                                    GreenBoss greenBoss = new GreenBoss(
                                        Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                        Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                        Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                        Content.Load<Texture2D>("Images/Projectile/bullet"),
                                        Content.Load<Texture2D>("Images/Projectile/bullet2"),
                                        bossPos,
                                        Boss.Direction.Up,
                                        300,
                                        15
                                    );
                                    currentMap.AddEnemy(greenBoss);
                                    currentMap.SetBossSpawned();
                                    break;
                                }
                            case GameState.ForestTop:
                                {
                                    Texture2D bossAttack = Content.Load<Texture2D>("Butterfly_Boss/ButterflyBossAttack/ButterflyBossDown");
                                    Texture2D bossWalking = Content.Load<Texture2D>("Butterfly_Boss/ButterflyBossWalking/ButterflyBossWalkingDown");
                                    Texture2D butterflyBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/butterfly_attack");
                                    Texture2D butterflyBulletVertical = Content.Load<Texture2D>("Images/Projectile/butterfly_attack2");
                                    ButterflyBoss butterflyBoss = new ButterflyBoss(
                                        bossAttack,
                                        bossWalking,
                                        butterflyBulletHorizontal, butterflyBulletVertical,
                                        bossPos,
                                        Boss.Direction.Up,
                                        300,
                                        15
                                    );
                                    currentMap.AddEnemy(butterflyBoss);
                                    currentMap.SetBossSpawned();
                                    break;
                                }
                            case GameState.ForestRight:
                                {
                                    Texture2D dragonIdle = Content.Load<Texture2D>("Dragon_Boss/DragoBossIdle/DragoBossIdleDown");
                                    Texture2D dragonAttack = Content.Load<Texture2D>("Dragon_Boss/DragoBossAttack/DragoBossAttackDown");
                                    Texture2D dragonWalking = Content.Load<Texture2D>("Dragon_Boss/DragoBossWalking/DragoBossWalkingDown");
                                    Texture2D dragonBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/Dragon_Fireball");
                                    Texture2D dragonBulletVertical = Content.Load<Texture2D>("Images/Projectile/Dragon_Fireball2");
                                    DragonBoss dragonBoss = new DragonBoss(
                                        dragonIdle,
                                        dragonAttack,
                                        dragonWalking,
                                        dragonBulletHorizontal,
                                        dragonBulletVertical,
                                        bossPos,
                                        Boss.Direction.Up,
                                        300,
                                        15
                                    );
                                    currentMap.AddEnemy(dragonBoss);
                                    currentMap.SetBossSpawned();
                                    break;
                                }
                            case GameState.ForestButtom:
                                {
                                    Texture2D ogreIdleUp = Content.Load<Texture2D>("Ogre_Boss/OgreBossIdle/OgreBossIdleUp");
                                    Texture2D ogreIdleDown = Content.Load<Texture2D>("Ogre_Boss/OgreBossIdle/OgreBossIdleDown");
                                    Texture2D ogreIdleLeft = Content.Load<Texture2D>("Ogre_Boss/OgreBossIdle/OgreBossIdleLeft");
                                    Texture2D ogreIdleRight = Content.Load<Texture2D>("Ogre_Boss/OgreBossIdle/OgreBossIdleRight");

                                    Texture2D ogreAttackUp = Content.Load<Texture2D>("Ogre_Boss/OgreBossAttack/OgreBossAttackUp");
                                    Texture2D ogreAttackDown = Content.Load<Texture2D>("Ogre_Boss/OgreBossAttack/OgreBossAttackDown");
                                    Texture2D ogreAttackLeft = Content.Load<Texture2D>("Ogre_Boss/OgreBossAttack/OgreBossAttackLeft");
                                    Texture2D ogreAttackRight = Content.Load<Texture2D>("Ogre_Boss/OgreBossAttack/OgreBossAttackRight");

                                    Texture2D ogreWalkingUp = Content.Load<Texture2D>("Ogre_Boss/OgreBossWalking/OgreBossWalkingUp");
                                    Texture2D ogreWalkingDown = Content.Load<Texture2D>("Ogre_Boss/OgreBossWalking/OgreBossWalkingDown");
                                    Texture2D ogreWalkingLeft = Content.Load<Texture2D>("Ogre_Boss/OgreBossWalking/OgreBossWalkingLeft");
                                    Texture2D ogreWalkingRight = Content.Load<Texture2D>("Ogre_Boss/OgreBossWalking/OgreBossWalkingRight");

                                    OgreBoss ogreBoss = new OgreBoss(
                                        ogreIdleUp, ogreIdleDown, ogreIdleLeft, ogreIdleRight,
                                        ogreAttackUp, ogreAttackDown, ogreAttackLeft, ogreAttackRight,
                                        ogreWalkingUp, ogreWalkingDown, ogreWalkingLeft, ogreWalkingRight,
                                        Content.Load<Texture2D>("Images/Projectile/bullet"),
                                        Content.Load<Texture2D>("Images/Projectile/bullet2"),
                                        bossPos,
                                        Boss.Direction.Up,
                                        300,
                                        15
                                    );
                                    currentMap.AddEnemy(ogreBoss);
                                    currentMap.SetBossSpawned();
                                    break;
                                }
                            case GameState.ForestLeft:
                                {
                                    Texture2D spiderIdle = Content.Load<Texture2D>("Spider_Boss/SpiderBossIdle/SpiderBossIdleDown");
                                    Texture2D spiderAttack = Content.Load<Texture2D>("Spider_Boss/SpiderBossAttack/SpiderBossAttackDown");
                                    Texture2D spiderWalking = Content.Load<Texture2D>("Spider_Boss/SpiderBossWalking/SpiderBossWalkingDown");
                                    Texture2D spiderBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/spider_attack");
                                    Texture2D spiderBulletVertical = Content.Load<Texture2D>("Images/Projectile/spider_attack2");
                                    SpiderBoss spiderBoss = new SpiderBoss(
                                        spiderIdle,
                                        spiderAttack,
                                        spiderWalking,
                                        spiderBulletHorizontal, spiderBulletVertical,
                                        bossPos,
                                        Boss.Direction.Up,
                                        300,
                                        15
                                    );
                                    currentMap.AddEnemy(spiderBoss);
                                    currentMap.SetBossSpawned();
                                    break;
                                }
                        }
                    }

                    // Update enemies.
                    foreach (var enemy in currentMap.Enemies)
                    {
                        enemy.Update(gameTime, _graphics.GraphicsDevice.Viewport, player.Position, player);
                    }

                    currentMap.UpdateRespawn(gameTime);

                    foreach (var leaf in fallingLeaves)
                        leaf.Update(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                    foreach (var snow in snowFlakes)
                        snow.Update(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

                    // Process map transitions.
                    foreach (var transition in transitions)
                    {
                        if (transition.From == currentState && transition.Zone.Intersects(player.Bounds))
                        {
                            if (maps.ContainsKey(transition.To))
                            {
                                var newMap = maps[transition.To];
                                currentState = transition.To;
                                // Center the player in the new map.
                                player.Position = new Vector2(newMap.Background.Width / 2f, newMap.Background.Height / 2f);
                            }
                            break;
                        }
                    }
                }

                if (currentState != GameState.Scoreboard)
                    previousKeyboardState = currentKeyboardState;
            }

            if (currentState == GameState.Scoreboard)
            {
                scoreboardScreen.Update(gameTime);
                if (scoreboardScreen.Finished)
                    Environment.Exit(0);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (currentState == GameState.Scoreboard)
            {
                scoreboardScreen.Draw(_spriteBatch, GraphicsDevice);
            }
            else
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                _spriteBatch.Begin();

                if (currentState == GameState.MainMenu)
                {
                    _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
                    mainMenu.Draw(_spriteBatch);
                }
                else
                {
                    var currentMap = maps[currentState];
                    _spriteBatch.Draw(currentMap.Background, Vector2.Zero, Color.White);

                    foreach (var snow in snowFlakes)
                        snow.Draw(_spriteBatch);
                    foreach (var leaf in fallingLeaves)
                        leaf.Draw(_spriteBatch);

                    player.Draw(_spriteBatch);

                    foreach (var enemy in currentMap.Enemies)
                        enemy.Draw(_spriteBatch);

                    if (!isPaused)
                        playerStats.Draw(_spriteBatch, new Vector2(10, 10));
                }

                if (isPaused)
                {
                    pauseMenu.Draw(_spriteBatch, playerStats);
                }

                // Draw difficulty notification if active.
                if (difficultyNotification != null)
                {
                    difficultyNotification.Update(gameTime);
                    if (difficultyNotification.IsActive)
                        difficultyNotification.Draw(_spriteBatch);
                    else
                        difficultyNotification = null;
                }

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        // Call this method when game over is triggered (e.g., from Player.TakeDamage)
        public void GameOver()
        {
            currentState = GameState.Scoreboard;
            double timeSpent = (DateTime.Now - sessionStartTime).TotalSeconds;
            scoreboardScreen = new ScoreboardScreen(gameFont,
                timeSpent,
                bulletsFiredThisSession,
                bulletsUsedAgainstEnemiesThisSession,
                bulletsUsedAgainstBossesThisSession,
                livesLostThisSession,
                currentScore,
                currentLevel,
                currentLives,
                deathsThisSession,
                gameData);
        }
    }
}

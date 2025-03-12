using System;
using System.Collections.Generic;
using System.Diagnostics;  // For Debug.WriteLine
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DDA_Tool; // For the DLL types

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

        // New counters for enemy and boss kills.
        public int totalEnemiesKilled = 0;
        public int totalBossesKilled = 0;

        // Expose playerStats and sessionStartTime via public properties.
        public PlayerStats PlayerStatsInstance => playerStats;
        public DateTime SessionStartTime => sessionStartTime;

        // Track previous difficulty for on-screen notification.
        private DifficultyLevel previousDifficulty;

        // On-screen notification for difficulty changes.
        private DifficultyNotification difficultyNotification;

        // Scoreboard state is now a separate game state.
        private ScoreboardState scoreboardState;

        // To ensure GameOver() is triggered only once.
        private bool gameOverTriggered = false;

        // For the debug overlay: FPS calculation and public test hooks.
        private double totalTime = 0;
        private int frameCount = 0;
        private int fps = 0;
        public int FPS => fps;

        // Expose current game state for testing.
        public GameState CurrentGameState => currentState;

        // New helper property: expose the current map.
        public Map CurrentMap => (currentState != GameState.MainMenu && maps.ContainsKey(currentState)) ? maps[currentState] : null;

        // Helper method: returns enemy count in the current map (if applicable).
        public int GetCurrentEnemyCount()
        {
            if (currentState != GameState.MainMenu && maps.ContainsKey(currentState))
                return maps[currentState].Enemies.Count;
            return 0;
        }

        // Helper method: returns count of bosses in the current map.
        public int GetBossCount()
        {
            return totalBossesKilled;
        }

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

            // Set default difficulty using our DifficultyManager.
            DifficultyManager.Instance.SetDifficulty(DifficultyLevel.Default);
            previousDifficulty = DifficultyManager.Instance.CurrentDifficulty;

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

            // Initialize kill counters.
            totalEnemiesKilled = 0;
            totalBossesKilled = 0;

            Debug.WriteLine("Game initialized in state: " + currentState.ToString());

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

            int initialEnemyCount = DifficultyManager.Instance.BaseEnemyCount; // should be 2 at Default.
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
                int finalHealth = (int)(baseHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
                int finalDamage = (int)(baseDamage * DifficultyManager.Instance.EnemyDamageMultiplier);

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
                e.SetBaseStats(baseHealth, baseDamage);
                map.AddEnemy(e);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // Update FPS counter.
            totalTime += gameTime.ElapsedGameTime.TotalSeconds;
            frameCount++;
            if (totalTime >= 1)
            {
                fps = frameCount;
                frameCount = 0;
                totalTime -= 1;
            }

            var currentKeyboardState = Keyboard.GetState();

            // Toggle pause on Tab key (edge detection).
            if (currentKeyboardState.IsKeyDown(Keys.Tab) && previousKeyboardState.IsKeyUp(Keys.Tab))
            {
                isPaused = !isPaused;
                IsMouseVisible = isPaused;
                Debug.WriteLine("Pause toggled. isPaused = " + isPaused);
            }

            // Handle difficulty key input.
            if (currentKeyboardState.IsKeyDown(Keys.D0))
            {
                DifficultyManager.Instance.HandleKeyInput('0');
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D1))
            {
                DifficultyManager.Instance.HandleKeyInput('1');
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D2))
            {
                DifficultyManager.Instance.HandleKeyInput('2');
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D3))
            {
                DifficultyManager.Instance.HandleKeyInput('3');
            }

            // If difficulty changed, show notification.
            if (DifficultyManager.Instance.CurrentDifficulty != previousDifficulty)
            {
                difficultyNotification = new DifficultyNotification("Game difficulty changed to " + DifficultyManager.Instance.CurrentDifficulty.ToString(), 5.0f, gameFont);
                previousDifficulty = DifficultyManager.Instance.CurrentDifficulty;
            }

            // If Escape is pressed (edge detection) OR if player's lives are 0, trigger GameOver once.
            if ((!gameOverTriggered && currentKeyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                || (!gameOverTriggered && playerStats.Lives <= 0))
            {
                GameOver();
                gameOverTriggered = true;
                return; // Early exit to avoid processing map logic
            }

            // If we are in Scoreboard state, update only the scoreboard.
            if (currentState == GameState.Scoreboard)
            {
                scoreboardState.Update(gameTime);
                if (scoreboardState.Finished)
                    Environment.Exit(0);
                return; // Skip rest of update
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
                var currentMap = maps[currentState];

                player.Update(gameTime, _graphics.GraphicsDevice.Viewport, currentMap.Enemies);
                playerStats.UpdateHealth(player.Health);

                // Remove dead enemies and update kill counters.
                for (int i = currentMap.Enemies.Count - 1; i >= 0; i--)
                {
                    if (currentMap.Enemies[i].IsDead)
                    {
                        if (currentMap.Enemies[i] is Boss)
                            totalBossesKilled++;
                        else
                            totalEnemiesKilled++;
                        currentMap.IncrementKillCount();
                        currentMap.Enemies.RemoveAt(i);
                    }
                }

                // Spawn boss if kill count exceeds threshold and boss is not spawned.
                if (currentMap.KillCount >= DifficultyManager.Instance.BossSpawnThreshold && !currentMap.BossSpawned)
                {
                    Vector2 bossPos = new Vector2(
                        (currentMap.Background.Width - 256) / 2,
                        (currentMap.Background.Height - 256) / 2
                    );

                    int baseBossHealth = 300;
                    int baseBossDamage = 15;

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
                                    (int)(baseBossHealth * DifficultyManager.Instance.BossHealthMultiplier),
                                    (int)(baseBossDamage * DifficultyManager.Instance.BossDamageMultiplier)
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
                                    (int)(baseBossHealth * DifficultyManager.Instance.BossHealthMultiplier),
                                    (int)(baseBossDamage * DifficultyManager.Instance.BossDamageMultiplier)
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
                                    (int)(baseBossHealth * DifficultyManager.Instance.BossHealthMultiplier),
                                    (int)(baseBossDamage * DifficultyManager.Instance.BossDamageMultiplier)
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
                                    (int)(baseBossHealth * DifficultyManager.Instance.BossHealthMultiplier),
                                    (int)(baseBossDamage * DifficultyManager.Instance.BossDamageMultiplier)
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
                                    (int)(baseBossHealth * DifficultyManager.Instance.BossHealthMultiplier),
                                    (int)(baseBossDamage * DifficultyManager.Instance.BossDamageMultiplier)
                                );
                                currentMap.AddEnemy(spiderBoss);
                                currentMap.SetBossSpawned();
                                break;
                            }
                    }
                }

                // Update enemies.
                foreach (var enemy in currentMap.Enemies)
                    enemy.Update(gameTime, _graphics.GraphicsDevice.Viewport, player.Position, player);

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

            if (currentState == GameState.Scoreboard)
            {
                scoreboardState.Update(gameTime);
                if (scoreboardState.Finished)
                    Environment.Exit(0);
            }
            else
            {
                previousKeyboardState = currentKeyboardState;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (currentState == GameState.Scoreboard)
            {
                scoreboardState.Draw(_spriteBatch, GraphicsDevice);
            }
            else
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                _spriteBatch.Begin();

                if (currentState == GameState.MainMenu)
                {
                    _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
                    mainMenu.Draw(_spriteBatch);

                    string introStory =
                        @"In the twilight of war, you were a hardened soldier in the Battle of the Bulge.
Your mission was daring, to infiltrate a secret Nazi stronghold and plant enchanted bombs 
deep within its iron heart. Along the way, you witnessed visions of eerie, otherworldly marvels, 
creatures and phenomena that defied mortal understanding.

When the alarms blared and your escape was cut off, you ignited the bombs in a desperate act of defiance.
In the ensuing cataclysm, reality itself shattered.
You awoke to find that you had been transformed, no longer a mere soldier, but a fierce huntress 
wielding a mystical bow with uncanny precision.

Now, in a forest pulsing with ancient magic, spectral German soldiers march as if haunted by the past,
and dark, powerful entities lurk in the shadows. Trapped in a relentless time loop, your destiny 
is entwined with supernatural forces that challenge every fiber of your being.

Can you unravel the mystery of your transformation and restore the balance between worlds?

Press Enter to step into the legend...";
                    Vector2 textSize = gameFont.MeasureString(introStory);
                    float margin = 50f;
                    float posX = GraphicsDevice.Viewport.Width - textSize.X - margin;
                    float posY = (GraphicsDevice.Viewport.Height - textSize.Y) / 2;
                    Vector2 position = new Vector2(posX, posY);
                    _spriteBatch.DrawString(gameFont, introStory, position, Color.Yellow);
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
                    // Draw pause menu including debug overlay.
                    pauseMenu.Draw(_spriteBatch, playerStats);
                }

                // Draw difficulty notification if active.
                if (difficultyNotification != null)
                {
                    difficultyNotification.Update(gameTime);
                    if (difficultyNotification.IsActive)
                        difficultyNotification.Draw(_spriteBatch, GraphicsDevice.Viewport);
                    else
                        difficultyNotification = null;
                }

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        // Call this method when game over is triggered.
        public void GameOver()
        {
            // Capture the current difficulty level before resetting.
            DifficultyLevel endingDifficulty = DifficultyManager.Instance.CurrentDifficulty;

            // Reset dynamic difficulty to Normal (now renamed to Medium).
            DifficultyManager.Instance.SetDifficulty(DifficultyLevel.Medium);

            currentState = GameState.Scoreboard;
            double timeSpent = (DateTime.Now - sessionStartTime).TotalSeconds;
            // Pass the captured endingDifficulty into ScoreboardState.
            scoreboardState = new ScoreboardState(gameFont,
                timeSpent,
                bulletsFiredThisSession,
                bulletsUsedAgainstEnemiesThisSession,
                bulletsUsedAgainstBossesThisSession,
                livesLostThisSession,
                currentScore,
                currentLevel,
                currentLives,
                deathsThisSession,
                gameData,
                endingDifficulty);
        }

        // Testing hook: Reset game state for unit/integration testing.
        public void ResetGameForTesting()
        {
            currentState = GameState.MainMenu;
            gameOverTriggered = false;
            // Reset session variables.
            currentLevel = 1;
            currentLives = 3;
            currentScore = 0;
            bulletsFiredThisSession = 0;
            bulletsUsedAgainstEnemiesThisSession = 0;
            bulletsUsedAgainstBossesThisSession = 0;
            livesLostThisSession = 0;
            deathsThisSession = 0;
            sessionStartTime = DateTime.Now;
            // Also reset kill counters.
            totalEnemiesKilled = 0;
            totalBossesKilled = 0;
            Debug.WriteLine("Game state reset for testing.");
        }
    }
}

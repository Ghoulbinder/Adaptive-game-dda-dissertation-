# Adaptive Game Design: Dynamic Difficulty Adjustment System

An honors dissertation project exploring rule-based Dynamic Difficulty Adjustment (DDA) in interactive game environments, developed using C# and Microsoft XNA/MonoGame.

## 📋 About The Project

This project investigates how rule-based dynamic difficulty adjustment systems influence player engagement, performance, and satisfaction in video games. The system monitors player performance in real-time and adapts enemy behavior, spawn rates, and challenge levels to maintain optimal player flow.

### Research Question
*"How does the implementation of a rule-based dynamic difficulty adjustment system influence player engagement, performance, and overall satisfaction in interactive game environments?"*

## 🎮 Key Features

- **Dynamic Difficulty Adjustment**: Real-time adaptation of game parameters based on player performance
- **Multiple Difficulty Levels**: Default, Easy, Medium, and Hard modes with distinct thresholds
- **Boss Spawn System**: Enemies spawn when players reach specific kill thresholds (8, 10, 15, or 25 kills)
- **Performance Metrics**: Comprehensive debug logging system tracking:
  - Efficiency Index (kills per second)
  - Damage Ratio (damage taken per bullet fired)
  - Session duration and performance data
- **Adaptive Enemy Behavior**: Enemy health, speed, attack speed, and damage scale based on difficulty
- **Five Unique Maps**: Center, Top, Bottom, Left, and Right environments with distinct layouts
- **Real-time Debug Overlay**: In-game performance visualization

## 🛠️ Built With

- **C#** - Core game logic and DDA implementation
- **Microsoft XNA / MonoGame** - Game framework
- **Tiled Map Editor** - Level design
- **XML** - Data persistence and debug logging
- **Visual Studio** - Development environment

## 📊 Technical Implementation

### DDA System Architecture

The system consists of two core components:

1. **DynamicDifficultyController**: Stores difficulty rules, thresholds, and stat multipliers
2. **DifficultyManager**: Enforces rules in real-time during gameplay

### Difficulty Thresholds
- **Easy**: 8 enemy kills per boss spawn (1.0× multiplier)
- **Default**: 10 enemy kills per boss spawn (1.0× multiplier)
- **Medium**: 15 enemy kills per boss spawn (1.5× multiplier)
- **Hard**: 25 enemy kills per boss spawn (2.0× multiplier)

### Performance Metrics

**Efficiency Index (EI):**
```
EI = (Enemies Killed + Bosses Killed) / Time Taken (seconds)
```

**Damage Ratio (DR):**
```
DR = Total Damage Taken / Total Bullets Fired
```

## 🎯 Research Findings

Testing with 8 participants across multiple difficulty levels revealed:

- **Optimal Flow State**: Achieved at Efficiency Index of 0.10-0.15 kills/second
- **Default & Medium**: Best balance between challenge and engagement
- **Easy Mode**: Quick confidence building but risk of boredom
- **Hard Mode**: High intensity but potential for frustration without proper scaling

## 📁 Project Structure

```
adaptive-game-dda/
├── Game1.cs                 # Main game loop
├── Player.cs                # Player mechanics
├── Enemy.cs                 # Enemy behavior
├── Boss.cs                  # Boss mechanics
├── DifficultyManager.cs     # DDA system manager
├── DynamicDifficultyController.cs  # DDA rule logic
├── Maps/                    # Tiled map files
├── Assets/                  # Game sprites and resources
├── DebugLogger/             # XML-based logging system
└── Documentation/           # Dissertation and research materials
```

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2019 or later
- MonoGame 3.8 or later
- .NET Framework 4.7.2+

### Installation

1. Clone the repository
```bash
git clone https://github.com/yourusername/adaptive-game-dda-dissertation.git
```

2. Open the solution file in Visual Studio

3. Restore NuGet packages

4. Build and run the project

### Controls
- **Arrow Keys / WASD**: Move player
- **Spacebar**: Shoot/Attack
- **1, 2, 3, 0**: Switch difficulty (Easy, Medium, Hard, Default)
- **F1**: Toggle debug overlay

## 📈 Evaluation Methodology

The system was evaluated using:

- **Quantitative Analysis**: Debug logs analyzing 20+ gameplay sessions
- **Qualitative Feedback**: Post-session questionnaires from 8 testers
- **Metrics Tracking**: Frame rate, memory usage, response times
- **Agile Development**: Iterative sprints with continuous feedback

## 🎓 Academic Context

This project was developed as an honors dissertation exploring:
- Flow theory in game design
- Rule-based vs AI-driven adaptive systems
- Player engagement and satisfaction metrics
- Comparative analysis with commercial games (Resident Evil 4, Left 4 Dead, Celeste)

## 📝 Key Contributions

1. Novel **Efficiency Index** metric for measuring adaptive system performance
2. Comprehensive rule-based DDA framework with modular architecture
3. Integrated debug logging pipeline for quantitative analysis
4. Evidence-based recommendations for difficulty scaling

## 🔮 Future Enhancements

- Hybrid AI-driven + rule-based adaptation
- Physiological sensor integration (EEG, heart rate)
- Enhanced UI feedback for difficulty transitions
- Expanded playtesting with larger sample sizes
- Smoother multiplier scaling (1.2×, 1.6×, 1.8×)

## 📚 References

Built upon research in dynamic difficulty adjustment, flow theory (Csikszentmihalyi, 1990), and adaptive game design principles. Full bibliography available in the dissertation document.

## 👤 Author

**Romeo Mcdonald**
- GitHub: [@Ghoulbinder](https://github.com/Ghoulbinde)
- Dissertation: Honors Project in Interactive Computing

## 📄 License

This project is part of academic coursework. Please contact for usage permissions.

## 🙏 Acknowledgments

- University supervisors and module leaders
- Eight playtesters who provided valuable feedback
- MonoGame community and documentation
- Research foundations from Silva (2017), Massaro (2023), and others in the field

---

⭐ This repository represents academic research in adaptive game design. For detailed methodology, results, and analysis, please refer to the full dissertation document.

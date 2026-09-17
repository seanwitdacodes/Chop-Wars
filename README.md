# Chop Wars

A Unity 2D endless arcade game: dodge falling unhealthy food, collect rare bonus pickups, and chase a lasting high score as the kitchen gets progressively busier.

## Open and play

1. Open this folder through Unity Hub with **Unity 6000.5.3f1**, the version in `ProjectSettings/ProjectVersion.txt`.
2. Let Unity finish importing packages and assets.
3. Open `Assets/Scenes/MainMenu.unity` and press **Play**.
4. Choose **Play → Levels → Ghana**. You can also open `MainGame.unity` directly for development.

The six scenes and URP 2D rendering configuration are included in version control. No manual button wiring is needed.

## Controls and rules

| Action / pickup | Effect |
| --- | --- |
| Left/right arrows or A/D | Move horizontally |
| Hold and drag the mouse, or drag a finger | Move toward the pointer |
| Escape or pause button | Pause/resume; menu also offers restart and exit |
| Unhealthy food | Lose one heart, slow down, and get fatter |
| Healthy fruit/vegetables | Speed up and get slimmer; does not restore hearts |
| Heart | Recover one heart, up to ten |
| Coin | Earn 50 points |
| Survival | Earn one point per second while hazards arrive more frequently and fall faster |
| Healthy streak | Builds a score multiplier; unhealthy food breaks the streak |
| Zero hearts | Brief cutting-board reset, then return with full health |

There is no win or loss screen: the run continues until the player chooses to pause, restart, or return to the menu. Speed and body size have limits. Pointer movement follows the same speed bonuses and penalties as keyboard movement. Food spawns above the visible play area with natural drift and rotation, then is removed after falling off-screen. The best score and sound preference persist between sessions.

An original marimba, bass, and light-percussion soundtrack loops continuously through menus and gameplay. The Settings sound button mutes/unmutes it and remembers the choice. On the web, music starts after the first click, tap, or key press, as required by browsers. Menu2's wooden Back button returns to the main title page.

The soundtrack uses no third-party samples. Regenerate `Assets/Resources/JungleGroove.wav` with `python3 Tools/generate_jungle_music.py`.

## Character

The former white ball is an animated chef with a face, chef hat, scarf, apron, hands, and shoes. Unhealthy food widens the torso; healthy food slims it down. The head keeps recognizable proportions, the feet stay grounded, and the collider and movement limits follow the changing body size.

- `PlayerGrow`: growth/shrink amount, size limits, transition duration, and height change.
- `PlayerCharacterVisual`: character colors and layered sprite animation; visible in both the Scene view and Play Mode.
- `Spawner`: the Ghana scene starts with 4% healthy food, 2% coins, 1.2% hearts, and 92.8% hazards. Spawn intervals gradually drop from 1.6 seconds toward 0.32 seconds, while falling gravity ramps toward 3×. Bonus odds shrink with the interval so their frequency stays rare: approximately one fruit per 40 seconds, one coin per 80 seconds, and one heart per 133 seconds on average, not a guaranteed schedule. Pausing freezes progression; health recovery does not reset it.

The character reuses the project's circle sprite and material, so there are no external art downloads or runtime dependencies.

## Validate in Unity

Save open scenes, leave Play Mode, then select **Tools → Chop Wars → Validate Game**. The editor runs the shipped scenes and real 2D trigger collisions, checking navigation, UI click targets, health, score, size limits, movement boundaries, pause/restart, endless recovery, and saved settings. It also checks the difficulty curve and actual falling physics, samples 180,000 bonus rolls at different run times, and checks the live high-score display. It restores the original high score and sound preference after the checks.

Results: `Logs/ChopWarsValidation.json`. Rendered scene previews: `Logs/ValidationScreenshots/`.

For a batch run on macOS, close the project in the editor first:

```sh
"/Applications/Unity/Hub/Editor/6000.5.3f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "$PWD" \
  -executeMethod ChopWarsValidation.RunAll \
  -logFile "$PWD/Logs/validation.log"
```

Do not add `-quit`: the validator exits after its Play Mode checks finish. Keep graphics enabled to produce previews.

## Build

Use **Tools → Chop Wars → Build macOS** to create `Builds/Chop Wars.app`. Build output and validation logs are local generated files and are excluded from version control. Use Unity's Build Profiles window for other platforms; they need their platform support modules and device testing.

Use **Tools → Chop Wars → Build Web** after installing Unity's Web Build Support module to create a browser build in `Builds/Web`. Serve that folder through a web server for local testing, or zip its contents for an HTML-game host such as itch.io.

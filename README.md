# What's a Breakpoint?

> See whether your attack speed actually hits the Kinetic Fusillade breakpoint -- or wastes projectiles.

Part of the **WhatsA** plugin family for ExileApi.

## What It Does

- Displays an animated timing bar showing your attack speed against the Kinetic Fusillade salvo cycle
- Shows one of three states: **Perfect** (green), **Too Fast** (red, losing projectiles), or **Suboptimal** (yellow, wasting potential)
- Reads your attack speed automatically from in-game stats and updates in real time as you swap gear or flasks
- Manual override mode lets you test theoretical values for theorycrafting without being in combat

## Getting Started

1. Download and place in `Plugins/Source/What's a Breakpoint/`
2. HUD auto-compiles on next launch
3. Enable in plugin list
4. Equip Kinetic Fusillade and the timing bar appears on screen

## Settings

| Setting | Default | Description |
|---------|---------|-------------|
| Show Visual | On | Display the timing bar overlay |
| Position X / Y | 960 / 540 | Screen position of the bar |
| Visual Scale | 1.0 | Scale the entire display up or down |
| Bar Width / Height | 400 / 40 | Dimensions of the timing bar |
| Use Manual Values | Off | Enter custom values instead of reading from game |
| Projectiles | 5 | Manual override: number of projectiles fired |
| Duration Reduction | 0% | Manual override: skill duration modifier |
| Attack Speed | 5.0 | Manual override: attacks per second |

<details>
<summary>Technical Details</summary>

### Breakpoint Formula

The plugin calculates the salvo cycle from game mechanics:

- **Hovering time** = 0.7s x duration modifier
- **Firing time** = 0.05s x duration modifier x projectile count
- **Salvo time** = hovering + firing
- **Target APS** = 1 / salvo time

A 5% tolerance window determines "Perfect" timing.

### Architecture

- Reads player stats from `LocalStats` component with a 500ms cache to balance accuracy and performance
- Uses a `Stopwatch` for smooth cursor animation independent of frame rate
- Automatically reinitializes stat reading on zone changes
- Color settings are fully customizable (perfect/warning/danger/zone colors)

</details>

## About This Project

These plugins are built with AI-assisted development using Claude Code and the
ExileApiScaffolding (private development workspace) workspace.

The developer works professionally in cybersecurity and high-risk software --
AI compensates for a C# knowledge gap specifically, not engineering judgment.
Plugin data comes from the PoE Wiki and PoEDB data mining.

The focus is on UX: friction points and missing expected features that the
existing plugin ecosystem doesn't address. Every hour spent developing is an
hour not spent on league progression, so feedback is the best way to support
the project.

## WhatsA Plugin Family

| Plugin | Description |
|--------|-------------|
| **What's a Breakpoint?** | Kinetic Fusillade attack speed breakpoint visualizer |
| [What's a Crowd Control?](https://github.com/ParogDev/WhatsACrowdControl) | OmniCC-style CC effect overlay with timers |
| [What's a Mirage?](https://github.com/ParogDev/WhatsAMirage) | League mechanic overlay for spawners, chests, and wishes |
| [What's a Tincture?](https://github.com/ParogDev/WhatsATincture) | Automated tincture management with burn stack tracking |
| [What's a Tooltip?](https://github.com/ParogDev/WhatsATooltip) | Shared rich tooltip service for WhatsA plugins |
| [What's an AI Bridge?](https://github.com/ParogDev/WhatsAnAiBridge) | File-based IPC for AI-assisted plugin development |
| [What's an Unbound Avatar?](https://github.com/ParogDev/WhatsAnUnboundAvatar) | Auto-activation for Avatar of the Wilds at 100 fury |

Built with ExileApiScaffolding (private development workspace)

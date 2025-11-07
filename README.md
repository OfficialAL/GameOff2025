# 🏴‍☠️ Pirate Sailing Game - GitHub Game Off 2025

A multiplayer top-down pirate sailing adventure built for GitHub Game Off 2025! Manage your ship, coordinate with your crew, and defend against sea monsters in this lighthearted cooperative sailing experience.

## 🎮 Game Off 2025 Theme: [SECRETS]

*[Add how your game incorporates the theme once announced]*

## 🚢 Game Overview

A multiplayer pirate sailing game where players work together as a crew to:
- **Navigate dangerous waters** in their pirate ship
- **Manage ship systems** - hull integrity, flooding, and repairs (inspired by Barotrauma)
- **Fight sea monsters** that attack and damage the ship
- **Coordinate crew roles** - Captain, Gunner, Engineer, Lookout
- **Survive and explore** in a lighthearted pirate adventure

## 🎯 Core Features

### **Cooperative Ship Management**
- Multi-zone ship damage system
- Flooding and repair mechanics
- Role-based crew gameplay
- Real-time multiplayer coordination

### **Combat System**
- AI-driven sea creatures with different behaviors
- Ship-to-ship combat potential
- Damage and health systems

### **Multiplayer Foundation**
- Built on Unity Netcode for GameObjects
- Supports 2-4 players
- Client-server architecture
- Real-time synchronization

## 🛠️ Technical Implementation

### ✅ Implemented Systems

1. **Ship Management System**
   - `ShipController.cs` - Handles ship movement and physics
   - `ShipHealth.cs` - Manages hull integrity, flooding, and damage zones
   - `ShipWheel.cs` - Interactive steering wheel with role-based access

2. **Player System**
   - `PlayerController.cs` - Player movement and interaction system
   - Role-based crew management (Captain, Gunner, Engineer, Lookout)
   - Interaction system for ship components

3. **Creature System**
   - `SeaCreature.cs` - AI-driven sea monsters with attack behaviors
   - Different creature types (Passive, Aggressive, Territorial)
   - Health and combat system

4. **Networking Foundation**
   - Built on Unity Netcode for GameObjects
   - Client-server architecture
   - Network synchronization for all major systems

5. **Game Management**
   - `GameManager.cs` - Session management and game state
   - Player connection/disconnection handling
   - Game timer and scoring foundation

## Required Unity Packages

To get started, you'll need to install these packages via the Package Manager:

### Essential Packages
```
1. Unity Netcode for GameObjects (com.unity.netcode.gameobjects)
2. Input System (com.unity.inputsystem)
3. 2D Animation (com.unity.2d.animation)
4. 2D Sprite (com.unity.2d.sprite)
5. Universal Render Pipeline (com.unity.render-pipelines.universal)
```

### Recommended Packages
```
6. Cinemachine (com.unity.cinemachine) - For camera management
7. Post Processing (com.unity.postprocessing) - Visual effects
8. Audio (com.unity.modules.audio) - Sound system
```

## Setup Instructions

### 1. Install Required Packages
1. Open Package Manager (Window → Package Manager)
2. Install the packages listed above
3. Restart Unity if prompted

### 2. Configure Network Settings
1. Create a NetworkManager GameObject in your scene
2. Add the NetworkManager component
3. Configure network transport settings
4. Set up player prefabs and spawn points

### 3. Scene Setup
1. Set up your main camera for 2D orthographic view
2. Create a Canvas for UI elements
3. Add ship spawn points
4. Configure lighting for top-down gameplay

### 4. Input System Configuration
1. Use the existing `InputSystem_Actions.inputactions` file
2. Configure input actions for:
   - Player movement (WASD)
   - Interaction (E key)
   - Ship controls (Arrow keys or WASD when at wheel)

## Folder Structure

```
Assets/
├── Scripts/
│   ├── Networking/          # Network-related scripts
│   ├── Ship/               # Ship systems and components
│   ├── Player/             # Player movement and interaction
│   ├── Creatures/          # Sea creature AI and behaviors
│   ├── Managers/           # Game management and coordination
│   └── UI/                 # User interface scripts
├── Prefabs/
│   ├── Ship/               # Ship and ship component prefabs
│   └── Creatures/          # Creature prefabs
├── Sprites/                # 2D artwork and sprites
└── Audio/                  # Music and sound effects
```

## Key Features Implemented

### Ship Physics & Damage
- Realistic boat movement with momentum and steering
- Multi-zone damage system (6 damage zones)
- Flooding mechanics with repair system
- Water pumping system

### Multiplayer Coordination
- Role-based crew system
- Interactive ship components
- Network synchronized damage and flooding
- Real-time player collaboration

### Combat System
- AI-driven sea creatures
- Multiple creature behaviors
- Ship attack and defense mechanics
- Health and damage systems

## Next Steps for Development

### Immediate Next Steps
1. **Create ship and player prefabs** with proper NetworkObject components
2. **Set up basic scene** with ship, spawn points, and camera
3. **Configure input actions** for movement and interaction
4. **Test basic networking** with 2+ players

### Medium-term Development
1. **Visual Assets** - Create sprites for ship, players, creatures
2. **UI System** - Health bars, role indicators, interaction prompts
3. **Audio System** - Ocean sounds, attack sounds, ship creaking
4. **Balancing** - Damage values, creature strength, repair rates

### Long-term Features
1. **Advanced Ship Components** - Cannons, sails, anchor systems
2. **Weather System** - Storms, wind effects, visibility
3. **Progression System** - Ship upgrades, crew skills
4. **Multiple Ship Types** - Different vessels with unique properties

## Important Notes

- All scripts use Unity's new Input System
- Networking is built for dedicated server or host/client setup
- Physics are tuned for top-down gameplay
- System is designed to be modular and expandable

## Debugging Tips

1. **Networking Issues**: Check NetworkManager logs and connection status
2. **Physics Problems**: Verify Rigidbody2D settings and collision layers
3. **Input Not Working**: Ensure Input System package is installed and configured
4. **Performance**: Monitor NetworkVariable updates and optimize as needed

This foundation provides a solid starting point for your multiplayer pirate sailing adventure!
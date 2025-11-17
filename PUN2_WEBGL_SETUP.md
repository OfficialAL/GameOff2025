# Unity WebGL + Photon PUN2 Setup Guide

## Quick Start

### 1. Install Photon PUN2

1. Open Unity
2. Go to **Window > Asset Store**
3. Search for "**Photon PUN 2 - FREE**"
4. Download and Import
   - Or download from: <https://assetstore.unity.com/packages/tools/network/pun-2-free-119922>

### 2. Photon Setup

1. Create account at <https://dashboard.photonengine.com/>
2. Create new app (Type: **Photon PUN2**)
3. Copy your **App ID**
4. In Unity: **Window > Photon Unity Networking > Highlight Server Settings**
5. Paste App ID in Inspector

### 3. WebGL Build Settings

1. Go to **File > Build Settings**
2. Select **WebGL** platform
│   │   ├── MenuUI.cs            # Main menu
3. Click **Switch Platform**
4. Set these WebGL settings:
   - **Compression Format**: Gzip
   - **Development Build**: Checked (for testing)
   - **Strip Engine Code**: Unchecked (for PUN2 compatibility)

## Project Structure for WebGL + PUN2

Assets/
├── Scripts/
│   ├── Network/
│   │   ├── NetworkManager.cs     # PUN2 connection management
│   │   ├── PlayerNetwork.cs      # Player networking
│   │   └── GameNetwork.cs        # Game state sync
│   ├── Player/
│   │   ├── PlayerController.cs   # Player movement
│   │   └── PlayerInput.cs        # Input handling
│   ├── UI/
│   │   ├── MenuUI.cs            # Main menu
│   │   ├── LobbyUI.cs           # Room/lobby UI
│   │   └── GameUI.cs            # In-game UI
│   └── Game/
│       ├── GameManager.cs       # Game logic
│       └── GameState.cs         # Game state management
├── Prefabs/
│   ├── Player.prefab            # Networked player prefab
│   ├── NetworkManager.prefab    # PUN2 manager
│   └── UI/                      # UI prefabs
├── Scenes/
│   ├── Menu.unity               # Main menu scene
│   ├── Lobby.unity              # Lobby/room scene
│   └── Game.unity               # Main game scene
└── StreamingAssets/             # WebGL resources

## WebGL Considerations

### Performance

- Target 30-60 FPS for web browsers
- Optimize textures (compress, reduce size)
- Use object pooling for frequent spawns
- Limit particle effects

### PUN2 WebGL Limitations

- No UDP (uses WebSocket)
- Slightly higher latency than native
- Limited to ~20 concurrent players per room
- Some threading limitations

### Browser Compatibility

- Modern browsers (Chrome, Firefox, Safari, Edge)
- Requires HTTPS for production deployment
- Mobile browsers supported but limited performance

## Ready for GameOff 2025! 🎮

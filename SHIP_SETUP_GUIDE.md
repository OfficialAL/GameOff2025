# Ship Prefab Setup Guide

This guide will help you create a functional pirate ship prefab that 4 players can cooperatively operate.

## Required Unity Setup

### 1. Create the Ship GameObject
1. Create an empty GameObject named `PirateShip`
2. Add these components:
   - `Rigidbody2D` (set to not freeze any rotations)
   - `Collider2D` (Polygon or Composite for ship outline)
   - `ShipController` script
   - `ShipHealth` script
   - `ShipPlatform` script
   - `NetworkObject` (for multiplayer)

### 2. Ship Visuals
1. Create a child object named `ShipSprite`
2. Add `SpriteRenderer` component
3. Assign your ship sprite from `Assets/Sprites/Ships/Ships Mockup.png`
4. Set the sprite to the ship layer (create layer if needed)

### 3. Ship Wheel (Steering)
1. Create child object named `ShipWheel`
2. Position it where the wheel should be on your ship
3. Add components:
   - `Collider2D` (Circle or Box, set as Trigger)
   - `ShipWheel` script
4. Set the layer to "Interactable" (create if needed)
5. In ShipWheel script, set:
   - Interaction Range: 2.0
   - Requires Captain Role: false (for now)

### 4. Cannons Setup
For each cannon position on your ship:
1. Create child object named `Cannon_Left_1`, `Cannon_Right_1`, etc.
2. Position at cannon locations
3. Add components:
   - `Collider2D` (set as Trigger)
   - `ShipCannon` script
4. In ShipCannon script, configure:
   - Interaction Range: 2.0
   - Reload Time: 3.0 seconds
   - Cannon Power: 20.0
   - Max Ammo: 10
   - Create a child "FirePoint" Transform for projectile spawn

### 5. Repair Stations
1. Create 2-3 child objects named `RepairStation_1`, etc.
2. Position around the ship
3. Add components:
   - `Collider2D` (set as Trigger)
   - `RepairStation` script
4. Configure:
   - Interaction Range: 2.0
   - Repair Rate: 10.0 HP/second

### 6. Ship Platform Collision
1. Create child object named `ShipDeck`
2. Add `Collider2D` (covers the walkable area of ship)
3. Set as Trigger
4. Set layer to "ShipPlatform"

### 7. Player Spawn Points
1. Create 4 empty child objects named `PlayerSpawn_1` through `PlayerSpawn_4`
2. Position them around the ship where players should start
3. These will be used by the multiplayer spawn system

## Layer Setup
Create these layers in your project:
- Layer 6: Player
- Layer 7: Ship
- Layer 8: Interactable
- Layer 9: ShipPlatform
- Layer 10: Projectile

## Physics Settings
1. Go to Edit > Project Settings > Physics 2D
2. Set up layer collision matrix:
   - Players should collide with Ship and Interactables
   - Ship should collide with environment
   - Projectiles should collide with everything except Ship (friendly fire off)

## Prefab Creation
1. Drag the complete `PirateShip` GameObject to `Assets/Prefabs/Ship/`
2. Name it `PirateShip.prefab`

## Network Manager Setup
1. Add the PirateShip prefab to NetworkManager's NetworkPrefabsList
2. Set it as a network prefab so it can be spawned in multiplayer

## Player Prefab Requirements
Your player prefab needs:
- `PlayerController` script
- `Rigidbody2D`
- `Collider2D`
- `NetworkObject`
- Layer set to "Player"

## Testing the Ship
1. Create a test scene with:
   - Water background using your water sprites
   - The ship prefab
   - 4 player spawns
   - NetworkManager with Netcode for GameObjects

2. Test interactions:
   - Players can walk around the ship
   - Ship wheel can be operated (WASD/Arrow keys when interacting)
   - Cannons can be aimed and fired (Mouse when interacting)
   - Repair stations work when ship is damaged

## Animation Integration
Your ship sprites include animations - to use them:
1. Create an Animator Controller for the ship
2. Set up animation states for:
   - Idle floating
   - Moving (sailing)
   - Damaged states
3. Connect animations to ship state (speed, health, etc.)

## Audio Integration
Add AudioSource components for:
- Ship creaking sounds
- Water splashing
- Cannon fire
- Repair sounds

## Next Steps
Once basic functionality works:
1. Add visual feedback for interactions (UI prompts)
2. Implement ship-to-ship combat
3. Add treasure and exploration mechanics
4. Create multiplayer lobby system
5. Add ship customization options

This setup gives you a solid foundation for a 4-player cooperative pirate ship game!
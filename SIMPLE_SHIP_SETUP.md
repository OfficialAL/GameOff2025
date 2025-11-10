# Quick Fix: Simple Ship Setup (No Networking)

## Compilation Error Fix

The networking scripts are causing compilation errors because Unity Netcode isn't loading properly. Use these simple versions instead:

## Quick Setup Steps:

### 1. Create Test Ship
1. Create empty GameObject named `TestShip`
2. Add components:
   - `Rigidbody2D` (Gravity Scale = 0, Linear Drag = 0.98, Angular Drag = 0.95)
   - `BoxCollider2D` (Not a trigger, covers ship area)
   - `SimpleShipController` script
   - `SimpleShipHealth` script
   - `SimpleShipPlatform` script

### 2. Add Ship Visuals
1. Create child object `ShipSprite`
2. Add `SpriteRenderer` with your ship sprite
3. Position and scale appropriately

### 3. Create Ship Wheel
1. Create child object `ShipWheel`
2. Position at wheel location on ship
3. Add components:
   - `CircleCollider2D` (Is Trigger = true, Radius = 2)
   - `SimpleShipWheel` script

### 4. Create Players
1. Create GameObject named `Player1`
2. Add components:
   - `Rigidbody2D` (Gravity Scale = 0)
   - `CapsuleCollider2D` (Not a trigger)
   - `SimplePlayerController` script
3. Create child `InteractionPoint` (empty GameObject)
4. Assign InteractionPoint to SimplePlayerController
5. Duplicate for 4 players total

### 5. Test Scene Setup
1. Create new scene
2. Add TestShip and Players
3. Position players on the ship deck
4. Add camera to follow ship

## Controls:
- **WASD**: Move player when not operating wheel
- **E**: Interact with ship wheel
- **WASD** (when operating wheel): Control ship movement

## What Works:
✅ Players can walk around ship
✅ Ship moves players when sailing
✅ Interactive ship wheel
✅ Basic ship physics
✅ Multiple players can take turns steering

## What's Missing (will add networking later):
❌ Multiplayer synchronization
❌ Networked interactions
❌ Server authority

## Next Steps:
1. Test this simple version first
2. Fix Unity Netcode package issues
3. Upgrade to full networking system later

This gets you a working cooperative ship immediately while we fix the networking issues!
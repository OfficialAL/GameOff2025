# Runtime Error Troubleshooting Guide

## Fixed Issues ✅

### 1. PlayerRole System Removed
- **Issue**: ShipWheel was trying to access PlayerRole components that didn't exist
- **Fix**: Removed rigid role requirements - anyone can operate any ship component
- **Result**: No more "PlayerRole component not found" errors

### 2. Meta File Warnings
- **Issue**: ShaderGraph meta file warnings
- **Status**: These are Unity internal warnings and don't affect gameplay
- **Solution**: Refresh Unity assets (cleared PackageCache)

## Common Runtime Issues & Solutions

### 1. "NetworkManager not found" Errors
**Symptoms**: Network-related errors in console
**Solution**:
1. Add a GameObject to your scene
2. Attach `NetworkManager` component
3. Add `Unity Transport` component
4. Set Connection Type to "UnityTransport"

### 2. "Component not found" Errors
**Symptoms**: Missing component references
**Solution**:
1. Check all SerializeField references in inspector
2. Ensure GameObjects have required components:
   - Ship needs: Rigidbody2D, Collider2D, ShipController, ShipHealth, ShipPlatform
   - Players need: Rigidbody2D, Collider2D, PlayerController
   - Interactables need: Collider2D (set as Trigger)

### 3. "Object reference not set" Errors
**Symptoms**: NullReferenceException errors
**Common Causes**:

#### ShipController Issues:
```csharp
// Check these references are assigned:
- rudder Transform
- sails Transform[]
- Rigidbody2D component
```

#### PlayerController Issues:
```csharp
// Check these references are assigned:
- interactionPoint Transform
- Rigidbody2D component
```

#### ShipCannon Issues:
```csharp
// Check these references are assigned:
- firePoint Transform
- cannonballPrefab GameObject
- muzzleFlash ParticleSystem (optional)
```

### 4. Movement/Physics Issues
**Symptoms**: Objects not moving correctly
**Solution**:
1. Check Rigidbody2D settings:
   - Body Type: Dynamic
   - Gravity Scale: 0 (for ships)
   - Linear Drag: 0.98
   - Angular Drag: 0.95

2. Check Collider2D settings:
   - Ship: Not a trigger
   - Players: Not a trigger
   - Interactables: Is a trigger

### 5. Network Synchronization Issues
**Symptoms**: Objects not syncing between clients
**Solution**:
1. Ensure NetworkObject is attached to:
   - Ship prefab
   - Player prefab
   - Any networked interactables

2. Check NetworkManager prefab list includes all networked prefabs

## Testing Your Setup

### Use the Test Script
1. Create an empty GameObject in your scene
2. Attach the `ShipSystemTest` script
3. Assign your ship and player references
4. Check the console for test results

### Quick Setup Test
1. Create a scene with:
   - NetworkManager GameObject
   - Ship prefab instance
   - 1-4 player instances
   - Camera

2. Run the scene and check console for errors

### Layer Configuration
Make sure these layers exist:
- Layer 6: Player
- Layer 7: Ship  
- Layer 8: Interactable
- Layer 9: ShipPlatform

## Still Having Issues?

### Debug Steps:
1. Check Unity Console for specific error messages
2. Verify all SerializeField references are assigned in Inspector
3. Ensure proper layer assignments
4. Test with NetworkManager in offline mode first
5. Use the ShipSystemTest script to identify missing components

### Common Error Messages:
- `"GetComponent<X> returned null"` → Missing component on GameObject
- `"Object reference not set"` → Unassigned SerializeField reference
- `"NetworkObject not found"` → Missing NetworkObject component
- `"No NetworkManager found"` → Missing NetworkManager in scene

The role system has been removed, so the ship should work with natural cooperation now - any player can operate any system based on proximity and need!
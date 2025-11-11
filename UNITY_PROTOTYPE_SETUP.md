# Unity Prototype Setup Guide 🚀

## Prerequisites ✅
- Unity package cache has been cleared and regenerated
- Unity Editor is open and console shows no "file not found" errors

## Step 1: Create the Test Scene

### 1.1 New Scene Setup
1. **File → New Scene** → Choose "2D Core"
2. **Save Scene** as `TestScene` in `Assets/Scenes/`
3. **Set Camera**:
   - Position: (0, 0, -10)
   - Projection: Orthographic
   - Size: 15 (to see more of the ship)

### 1.2 Add Background
1. Create **Empty GameObject** → Name it `Ocean`
2. Add **SpriteRenderer** component
3. Use a blue color or water texture if available
4. Scale to fill camera view

## Step 2: Create the Ship

### 2.1 Ship GameObject
1. **Create Empty GameObject** → Name it `TestShip`
2. **Add Components**:
   ```
   - Rigidbody2D (Gravity Scale = 0, Linear Drag = 0.98, Angular Drag = 0.95)
   - BoxCollider2D (Not a trigger, covers ship area)
   - SimpleShipController (from your existing scripts)
   - SimpleShipHealth (from your existing scripts)  
   - SimpleShipPlatform (from your existing scripts)
   ```
3. **Position**: (0, 0, 0)
4. **Layer**: Ship (Layer 7)

### 2.2 Ship Visuals
1. **Create child object** → Name it `ShipSprite`
2. **Add SpriteRenderer** component
3. **Assign sprite** from `Assets/Sprites/Ships/Ships Mockup.png`
4. **Scale appropriately** (start with scale 1,1,1 and adjust)

### 2.3 Ship Wheel (Interaction Point)
1. **Create child of TestShip** → Name it `ShipWheel`
2. **Position** at wheel location on ship (approximate center-back)
3. **Add Components**:
   ```
   - CircleCollider2D (Is Trigger = true, Radius = 2)
   - SimpleShipWheel (from your existing scripts)
   ```
4. **Layer**: Interactable (Layer 8)

## Step 3: Create Players

### 3.1 First Player
1. **Create Empty GameObject** → Name it `Player1`
2. **Add Components**:
   ```
   - Rigidbody2D (Gravity Scale = 0)
   - CapsuleCollider2D (Not a trigger)
   - SimplePlayerController (from your existing scripts)
   ```
3. **Create child empty GameObject** → Name it `InteractionPoint`
4. **Position InteractionPoint** slightly in front of player
5. **Assign InteractionPoint** to SimplePlayerController in inspector
6. **Position Player1** on ship deck: (-2, 1, 0)
7. **Layer**: Player (Layer 6)

### 3.2 Add Player Visuals
1. **Create child of Player1** → Name it `PlayerSprite`
2. **Add SpriteRenderer** with colored square or player sprite
3. **Scale to appropriate size** (0.5, 0.5, 1)

### 3.3 Duplicate Players
1. **Duplicate Player1** three times → Name them Player2, Player3, Player4
2. **Position them** around the ship:
   - Player2: (2, 1, 0)
   - Player3: (-2, -1, 0) 
   - Player4: (2, -1, 0)

## Step 4: Add Input Controls

### 4.1 Test Controls (Manual)
For quick testing, add this to a test script:

```csharp
// TestShipControls.cs - Attach to any GameObject
public class TestShipControls : MonoBehaviour 
{
    public SimpleShipController ship;
    
    void Update() 
    {
        if (ship == null) return;
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Send input to ship (simulate player at wheel)
        ship.SetControls(vertical, horizontal);
    }
}
```

### 4.2 Player Input
Players use WASD to move around ship, E to interact with ship wheel.

## Step 5: Layer Setup

### 5.1 Create Layers
**Tags & Layers → Layers**:
- Layer 6: Player
- Layer 7: Ship  
- Layer 8: Interactable
- Layer 9: ShipPlatform

### 5.2 Assign Layers
- TestShip → Ship (Layer 7)
- Players → Player (Layer 6)
- ShipWheel → Interactable (Layer 8)

## Step 6: Physics Settings

### 6.1 Layer Collision Matrix
**Edit → Project Settings → Physics 2D → Layer Collision Matrix**:
- ✅ Player collides with: Ship, Interactable
- ✅ Ship collides with: Default (environment)
- ✅ Interactable collides with: Player

## Step 7: Testing

### 7.1 Basic Test
1. **Press Play**
2. **Check Console** - Should see no errors
3. **Test Movement**:
   - Use arrow keys/WASD to move ship (if TestShipControls added)
   - Use WASD to move players around ship
   - Move player to ship wheel, press E to interact

### 7.2 Add Test Script
1. **Create Empty GameObject** → Name it `SystemTester`
2. **Add ShipSystemTest script** (from your existing scripts)
3. **Assign references** in inspector:
   - Ship Controller → TestShip
   - Ship Health → TestShip  
   - Ship Platform → TestShip
   - Players → All 4 players
4. **Check console** for test results

## Step 8: Add Camera Follow

```csharp
// CameraFollow.cs - Attach to Main Camera
public class CameraFollow : MonoBehaviour 
{
    public Transform target; // Assign TestShip
    public Vector3 offset = new Vector3(0, 0, -10);
    
    void LateUpdate() 
    {
        if (target != null)
            transform.position = target.position + offset;
    }
}
```

## Expected Results ✅

After setup, you should have:
- Ship that moves with arrow keys/WASD
- 4 players that can walk around the ship  
- Players can interact with ship wheel (E key)
- Camera follows the ship
- No console errors
- Smooth ship physics with drag

## Troubleshooting 🔧

### Missing Script References?
- Check all SimpleXXX scripts exist in `Assets/Scripts/`
- Verify script names match exactly
- Check for compilation errors in Console

### Players Fall Through Ship?
- Verify ship has Collider2D (not trigger)
- Check Layer Collision Matrix settings
- Ensure players have Rigidbody2D with Gravity Scale = 0

### Ship Won't Move?
- Check Rigidbody2D settings on ship
- Verify SimpleShipController is attached
- Add TestShipControls script for manual testing

### Interaction Not Working?
- Verify ShipWheel has trigger collider
- Check SimplePlayerController has InteractionPoint assigned  
- Make sure layers are set correctly

## Next Steps 🎯

Once this basic prototype works:
1. **Add more ship components** (cannons, sails, repair stations)
2. **Add sea creatures** for interaction
3. **Implement networking** using the NetworkBehaviour versions
4. **Add UI elements** for health, speed, etc.
5. **Create proper sprites and animations**

The goal is to get a working cooperative ship where players can walk around and control it together!
# Quick Unity Scene Setup Checklist

## Step 1: Camera Setup ✅
- Position: (0, 0, -10)
- Projection: Orthographic  
- Size: 15

## Step 2: Create Ship GameObject
1. **Create Empty GameObject** → Name: `TestShip`
2. **Position**: (0, 0, 0)
3. **Add Components**:
   - Rigidbody2D (Gravity Scale = 0, Linear Drag = 0.98, Angular Drag = 0.95)
   - BoxCollider2D (Not trigger, size around 6x3)
   - SimpleShipController
   - SimpleShipHealth  
   - SimpleShipPlatform
4. **Set Layer**: Ship (Layer 7)

## Step 3: Add Ship Visual
1. **Create child of TestShip** → Name: `ShipSprite`
2. **Add SpriteRenderer**
3. **Set Sprite**: Use Ships Mockup.png or create colored rectangle
4. **Color**: Blue or ship color
5. **Scale**: Adjust to fit collider

## Step 4: Create Ship Wheel
1. **Create child of TestShip** → Name: `ShipWheel`
2. **Position**: (0, -2, 0) - at back of ship
3. **Add Components**:
   - CircleCollider2D (Is Trigger = true, Radius = 2)
   - SimpleShipWheel
4. **Add SpriteRenderer**:
   - Sprite: "Knob" (Unity built-in)
   - Color: Brown (#8B4513)
   - Scale: (1.5, 1.5, 1)
5. **Set Layer**: Interactable (Layer 8)

## Step 5: Create Players
For each player (make 4 total):

1. **Create Empty GameObject** → Name: `Player1` (then Player2, 3, 4)
2. **Add Components**:
   - Rigidbody2D (Gravity Scale = 0)
   - CapsuleCollider2D (Not trigger)
   - SimplePlayerController
3. **Create child** → Name: `InteractionPoint`
4. **Position InteractionPoint**: (0, 1, 0) - slightly in front
5. **Assign InteractionPoint** to SimplePlayerController in inspector
6. **Add Player Visual**:
   - Create child → Name: `PlayerSprite`
   - Add SpriteRenderer
   - Sprite: "Knob" or any shape
   - Colors: Red, Blue, Green, Yellow for each player
   - Scale: (0.8, 0.8, 1)
7. **Position Players**:
   - Player1: (-2, 1, 0)
   - Player2: (2, 1, 0)  
   - Player3: (-2, -1, 0)
   - Player4: (2, -1, 0)
8. **Set Layer**: Player (Layer 6)

## Step 6: Layer Setup
Create these layers if they don't exist:
- Layer 6: Player
- Layer 7: Ship
- Layer 8: Interactable
- Layer 9: ShipPlatform

## Step 7: Physics Settings
Edit → Project Settings → Physics 2D → Layer Collision Matrix:
- ✅ Player collides with: Ship, Interactable
- ✅ Ship collides with: Default
- ✅ Interactable collides with: Player

## Testing Checklist:
- [ ] Press Play - no console errors
- [ ] WASD moves players around ship
- [ ] Players stay on ship when ship moves
- [ ] Walk player to wheel, press E to interact
- [ ] When operating wheel, WASD controls ship
- [ ] Ship wheel rotates when steering
- [ ] Press E again to stop operating wheel

## Quick Visual Check:
- Blue ship in center
- Brown wheel at back of ship
- 4 colored circles (players) on ship deck
- Players should be small relative to ship
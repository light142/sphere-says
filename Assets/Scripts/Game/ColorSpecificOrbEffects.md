# Color-Specific Orb Effects Setup

## What's New

I've added **color-specific orb effects** so each colored sphere can have its own unique orb effect prefab assigned directly!

## Setup in Unity

### Step 1: Assign Color-Specific Orb Effect Prefabs
1. Open your `ARObjectSpawner` GameObject in the scene
2. In the Inspector, you'll see: **"Color-Specific Orb Effect Prefabs"**
3. Drag the specific orb effect prefabs for each color:
   - **Red Orb Effect**: Drag any orb prefab (e.g., `Bubble Orb .prefab`)
   - **Blue Orb Effect**: Drag any orb prefab (e.g., `Light Orb 2.prefab`)
   - **Green Orb Effect**: Drag any orb prefab (e.g., `Multiple Earth Orb.prefab`)
   - **Yellow Orb Effect**: Drag any orb prefab (e.g., `Bubble Orb .prefab`)

## How It Works

- **Red spheres** will get the prefab you assigned to `redOrbEffect`
- **Blue spheres** will get the prefab you assigned to `blueOrbEffect`
- **Green spheres** will get the prefab you assigned to `greenOrbEffect`
- **Yellow spheres** will get the prefab you assigned to `yellowOrbEffect`

## Example Configuration

Here's a suggested setup for visual variety:
- **Red**: `Bubble Orb .prefab` (magical/mystical feel)
- **Blue**: `Light Orb 2.prefab` (glowing/energy feel)
- **Green**: `Multiple Earth Orb.prefab` (grounded/natural feel)
- **Yellow**: `Bubble Orb .prefab` (bright/cheerful feel)

## Customization

You can assign any orb effect prefab to any color! You can even:
- Use the same prefab for multiple colors
- Create custom prefab variants for each color
- Mix and match different effects

The system automatically detects the sphere color based on the material name and applies the corresponding orb effect prefab!

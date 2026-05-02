# BoxStack Prototype Art Brief

Last updated: 2026-05-02

## Purpose

Create the first AI-generated PNG asset pass for a Toss app-in-app casual game about stacking delivery boxes.

The prototype has already validated the basic 2D stacking loop. This brief keeps the next step focused on visual clarity, fast iteration, and mobile readability rather than full production art.

## Visual Direction

- Style: clean 2D/2.5D app-game illustration.
- Camera feel: front-facing with a slight top face visible, similar to a simple isometric product icon but still readable as a 2D sprite.
- Mood: light, friendly, quick-session casual game.
- Shape language: chunky rectangular parcels with clear edges, soft shadows, and readable contact surfaces.
- Color direction: warm cardboard base with small accent colors from tape, labels, or stickers. Avoid heavy realism, dark grime, and noisy texture.
- Toss fit: polished, lightweight, modern, and legible on a small phone screen.

## Asset List

### Parcel Box Variants

1. Basic parcel box
   - Standard cardboard box, clean tape, simple label area.
   - Used as the default falling and stacking object.

2. Wide parcel box
   - Slightly wider rectangle for visual variety.
   - Same collision-friendly silhouette as the default box family.

3. Tall parcel box
   - Slightly taller upright package.
   - Useful for later difficulty or score variety.

4. Slightly dented parcel box
   - Minor corner bend or side compression.
   - Should still have a clean rectangular silhouette for physics readability.

5. Label and tape detail overlays
   - Small delivery label, barcode-like marks, tape strip, fragile sticker.
   - Optional separate PNG overlays if direct box variants become too repetitive.

### Ground / Support

6. Stack base
   - A simple platform, delivery mat, or conveyor-like floor.
   - Must clearly show where the first box should land.

### Background

7. App-game background
   - Bright, simple background that does not compete with the boxes.
   - Suggested direction: soft neutral interior, delivery counter, or abstract app-game space.

### UI Visual Tone

8. Result accent art
   - Small visual motifs for success/fail states, such as sparkles, check marks, or gentle burst shapes.
   - Keep this secondary until the core sprite replacement works.

## PNG Production Rules

- Format: PNG with transparent background for object sprites.
- Box sprite target size: 512 x 512 px source image, with the visible box centered and padded.
- Background target size: portrait mobile ratio, 1080 x 1920 px source image.
- Keep box edges crisp enough to see where one parcel touches another.
- Use a consistent light direction across all box variants.
- Avoid extreme perspective, photorealistic texture, deep shadows, and tiny unreadable details.
- Keep the collision silhouette close to a rectangle even when adding dents or labels.

## First AI Prompt Set

### Basic Parcel Box

Prompt:

```text
clean 2D mobile game sprite of a cardboard delivery box, front view with slight top face visible, soft rounded corners, clear dark edge lines, simple packing tape, small blank shipping label, warm cardboard color, polished casual fintech app style, centered object, transparent background, crisp silhouette, soft shadow under edges, high readability on phone screen
```

Negative prompt:

```text
photorealistic, 3D render, complex background, messy texture, torn box, dirty cardboard, dramatic lighting, extreme perspective, tiny unreadable text, watermark, logo
```

### Box Variation Pack

Prompt:

```text
set of three clean 2D mobile game cardboard parcel box sprites, same style and camera angle, one standard box, one slightly wide box, one slightly tall box, front view with slight top face visible, warm cardboard color, clear edge lines, simple tape and shipping label details, transparent background, consistent lighting, crisp rectangular silhouettes for a physics stacking game
```

Negative prompt:

```text
photorealistic, 3D render, background scene, inconsistent perspective, heavy damage, dirty texture, unreadable tiny text, brand logos, watermark
```

### Stack Base

Prompt:

```text
clean 2D mobile game platform for stacking parcel boxes, simple delivery counter or conveyor belt base, front view with slight top surface visible, modern fintech app casual style, muted neutral colors, clear landing area, soft shadow, transparent background, designed for a portrait phone game
```

Negative prompt:

```text
photorealistic, complex warehouse, dark background, clutter, people, vehicles, brand logos, unreadable text, watermark
```

### Background

Prompt:

```text
bright simple portrait mobile game background for a parcel stacking mini game, soft modern delivery counter environment, clean fintech app feel, light neutral colors with subtle blue and mint accents, enough empty space in the center for stacked boxes, no text, no logos, gentle depth, polished casual game illustration
```

Negative prompt:

```text
photorealistic, dark warehouse, cluttered scene, people, vehicles, text, logos, watermark, high contrast pattern, distracting details
```

## Import Plan

- Place generated prototype PNGs under `Assets/Art/Prototype/Parcel/`.
- Keep filenames simple and descriptive, such as `parcel_box_basic_01.png`.
- Import as Sprite assets in Unity.
- Replace the runtime-generated placeholder parcel texture after the first PNG set is available.
- Preserve the current 2D physics loop while swapping visuals so the next playtest isolates visual clarity and app-fit.

## Next Playtest Focus

- Can a new player understand the parcel-stacking goal within three seconds?
- Do the boxes look pleasant enough to keep stacking?
- Are the contact edges clear when boxes overlap or lean?
- Does the game feel like it belongs inside a lightweight Toss app-in-app experience?

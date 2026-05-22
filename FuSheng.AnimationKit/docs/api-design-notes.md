# API design notes

This project extracts animation logic from FuShengSDK V5.0 into reusable WinForms classes.

## Extracted classes

### ShineButton

Source idea: the original ModernButton control.

Purpose:

- Gradient button surface
- Hover color transition
- Press animation
- Light sweep / shine animation

### ShineHeaderPanel

Source idea: the original HeaderPanel control.

Purpose:

- Gradient header
- Ice-reflection highlight
- Optional shine sweep
- Optional glow and shadow

### FragmentTransitionManager

Source idea: the original page transition methods:

- StartPageMorphAnimation
- CaptureTransitionSprites
- CreateScatterRect
- FinishPageMorphAnimation

Purpose:

- Switch between two WinForms pages
- Capture elements as image fragments
- Animate displacement, alpha, and rotation
- Draw gradient fusion background

## API boundary

The library should not know about specific FuShengSDK pages. It should only know:

```csharp
Control host;
Control fromPage;
Control toPage;
FragmentTransitionOptions options;
```

This makes the animation reusable in other WinForms projects.

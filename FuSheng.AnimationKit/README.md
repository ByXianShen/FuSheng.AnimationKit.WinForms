# FuSheng AnimationKit for WinForms

FuSheng AnimationKit is a lightweight WinForms animation library extracted from the FuShengSDK V5.0 animation design language.

It contains two animation systems:

1. **Luminous Shine Animation**
   - Ice-reflection style header highlight
   - Button hover shine sweep
   - Gradient surface and soft shadow
   - Press animation

2. **Fragment Transition Animation**
   - Captures page elements as independent image fragments
   - Randomized movement and optional rotation
   - Fade-in / fade-out composition
   - Gradient fusion background
   - Quality levels for different hardware

## Project structure

```text
src/
  FuSheng.AnimationKit.WinForms/   reusable animation API library
  DemoApp/                         runnable WinForms demo
```

## Quick start

### Shine button

```csharp
using FuSheng.AnimationKit.WinForms;

ShineButton button = new ShineButton();
button.Text = "Next";
button.BaseColor = Color.FromArgb(101, 222, 124);
button.HoverColor = Color.FromArgb(64, 208, 98);
button.PressedColor = Color.FromArgb(43, 174, 73);
button.Dock = DockStyle.Fill;
```

### Shine header

```csharp
ShineHeaderPanel header = new ShineHeaderPanel();
header.TitleText = "My App";
header.SubtitleText = "A beautiful WinForms application";
header.VersionText = "V1.0";
header.Dock = DockStyle.Top;
header.Height = 110;
```

### Fragment page transition

```csharp
FragmentTransitionManager transition = new FragmentTransitionManager(pageHost);
transition.Options.Duration = 1680;
transition.Options.Quality = AnimationQuality.Standard;
transition.Switch(pageA, pageB);
```

## Quality modes

```csharp
transition.Options.Quality = AnimationQuality.High;
transition.Options.Quality = AnimationQuality.Standard;
transition.Options.Quality = AnimationQuality.Low;
transition.Options.Quality = AnimationQuality.Minimal;
```

Use `Low` or `Minimal` on low-end computers to reduce GDI+ workload.

## Build

Open `FuSheng.AnimationKit.sln` in Visual Studio 2022 or later.

The library project targets:

- .NET Framework 4.8
- .NET 8 Windows

The demo project targets .NET 8 Windows.

## Suggested GitHub repository description

> Lightweight WinForms animation library providing luminous shine effects and fragment-based page transitions.

## License

MIT License.

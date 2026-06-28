# KSP Optimizer

Performance mod for Kerbal Space Program 1.12.x. One DLL, no config, press the toolbar button or F2.

## Features

- **GC Stutter Fix** — Spreads garbage collection across frames instead of blocking
- **Part Welding** — Merges 3+ identical parts (fuel tanks, struts, structural) into one physics object
- **Joint Reinforcement** — Stiffens joints on launch so you need fewer struts
- **Adaptive Physics LOD** — Reduces physics/rendering on distant parts

## Install

### CKAN
Search "KSP Optimizer" in CKAN, install.

### Manual
1. Download `KspOptimizer.dll` from [Releases](https://github.com/YourName/ksp-optimizer/releases)
2. Put it in `KSP/GameData/KspOptimizer/`
3. Launch KSP

## Usage

- **Toolbar button** (green square) in flight/map view — click to open settings
- **F2** — toggle settings window
- Toggle any feature on/off

## Build

```powershell
set KSP=C:\Program Files\Epic Games\KerbalSpaceProgram\English
dotnet build --configuration Release
```

## License

MIT

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build entire solution
dotnet build Solo.sln

# Build specific game
dotnet build games/Solocaster/Solocaster.csproj

# Run a game
dotnet run --project games/Solocaster/Solocaster.csproj
dotnet run --project games/Monoroids/Monoroids.csproj
```

## Project Structure

- **Solo/** - Core 2D game engine library built on MonoGame (DesktopGL)
- **games/** - Example games using the engine (Monoroids, Snake, Tetris, SpaceInvaders, Pacman, Solocaster)

## Engine Architecture

### Core Patterns

**GameObject-Component System**: GameObjects form a hierarchy (parent/children) and contain components via `ComponentsCollection`. Components must have a constructor taking `GameObject owner` and are created via `ComponentsFactory`.

**Scene Management**: Games define scenes that inherit from `Scene`. Scenes have a `Root` GameObject and lifecycle methods (`EnterCore`, `ExitCore`, `Update`). `SceneManager` handles scene transitions and calls `Enter()`/`Exit()` automatically.

**Services**: Global services implement `IGameService` and register with `GameServicesManager.Instance.AddService()`. Retrieve services with `GetRequired<T>()`. Services have `Initialize()` and `Step()` lifecycle methods.

### Rendering

Components implement `IRenderable` to participate in rendering. The `RenderService` collects all `IRenderable` components from the scene graph each frame and renders them sorted by `LayerIndex`. Games define render layer constants (e.g., `RenderLayers.cs`).

### Transform System

`TransformComponent` provides local/world transforms. `Transform` class holds Position (Vector2), Direction/Rotation (synced), and Scale. World transform incorporates parent's position.

### Collision System

`CollisionService` uses spatial bucketing. Add `BoundingBoxComponent` to GameObjects for collision detection. Register colliders with the service; it handles position-change events automatically.

### Messaging

`MessageBus` service provides pub/sub via typed `MessageTopic<TM>` where `TM : IMessage`.

### Object Pooling

`Spawner` wraps `Pool<GameObject>` for efficient object reuse. Objects return to pool when disabled (`Enabled = false`).

### AI

`StateMachine` for FSM-based AI with `State` and typed `StateTransition`. `Pathfinder` available for pathfinding.

## Creating a New Game

1. Create project referencing Solo.csproj
2. Create a `Game` subclass that:
   - Initializes `RenderService`, `SceneManager` in `Initialize()`
   - Adds scenes in `LoadContent()`
   - Calls `GameServicesManager.Instance.Step(gameTime)` in `Update()`
   - Calls `_renderService.Render()` in `Draw()`
3. Create scenes inheriting from `Scene`, override `EnterCore()` to build the scene graph
4. Create components inheriting from `Component`, override `UpdateCore()` for logic and implement `IRenderable` for rendering


## Coding Conventions
- write comments only when strictly necessary (eg. the implementation is not obvious and the code is not self-explanatory)
- prefer clear and descriptive names for classes, methods, variables
- organize code into small, single-responsibility methods
- use consistent formatting and indentation
- follow SOLID principles and best practices
- classes should be small and focused on a single responsibility
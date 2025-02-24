# Solo

Solo is a very simple game engine built on top of [Monogame](https://monogame.net/).

It's based on the Components idea, more or less like Unity: each [GameObject](/Solo/GameObject.cs) has a collection of [Components](/Solo/Components/Component.cs) that define its behavior. Components can be anything: a renderer, a physics system, a "brain" and so on.

GameObjects can be added to the SceneGraph, which in turn handles the tree of objects that build up your game.

You can also define [Scenes](./Solo/Services/Scene.cs), which can be used to handle things like the actual game, the menu screen, a game over screen, and so on.

### Getting started
To get started, clone the repository and open the solution in Visual Studio. You can then dig into any of the Sample projects to get an idea of how to use the engine.

In order to be able to run the samples, you need to install the MonoGame VS templates:
```
dotnet new install MonoGame.Templates.CSharp
```

along with the [Content Editor](https://docs.monogame.net/articles/getting_started/2_choosing_your_ide_visual_studio.html#install-monogame-extension-for-visual-studio-2022).

### Samples
I have built a few sample games to show how to use the engine. Those are very simple examples and there are (for sure) a lot of bugs :)

- [Monoroids](./games/Monoroids/)
- [Snake](./games/Snake/)
- [Tetris](./games/Tetris/)
- [SpaceInvaders](./games/SpaceInvaders/)

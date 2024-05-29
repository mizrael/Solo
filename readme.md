# Solo

Solo is a very simple game engine built on top of [Monogame](https://monogame.net/).

It's based on the Components idea, more or les like Unity: each [GameObject](/Solo/GameObject.cs) has a collection of [Components](/Solo/Components/Component.cs) that define its behavior. Components can be anything: a renderer, a physics system, a "brain" and so on.

GameObjects can be added to the SceneGraph, which in turn handles the tree of objects that build up your game.

You can also define [Scenes](./Solo/Services/Scene.cs), which can be used to handle things like the actual game, the menu screen, the gameover screen and so on.

### Samples
I have built few same games to show how to use the engine. Those are simple examples and there are (for sure) a lot of bugs :)

- [Monoroids](./games/Monoroids/)
- [Snake](./games/Snake/)
- [Tetris](./games/Tetris/)

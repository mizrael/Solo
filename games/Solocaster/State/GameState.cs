using Solocaster.Character;

namespace Solocaster.State;

public static class GameState
{
    public static CharacterData? CurrentCharacter { get; set; }

    public static void Clear()
    {
        CurrentCharacter = null;
    }

    public static void EnsureCharacter()
    {
        CurrentCharacter ??= new CharacterData();
    }
}

using System.Collections.Generic;

namespace Solocaster.Character;

public class CharacterData
{
    public string RaceId { get; set; } = "human";
    public string ClassId { get; set; } = "warrior";
    public Sex Sex { get; set; } = Sex.Male;
    public string AvatarSpriteName { get; set; } = "human_warrior_male";
    public string Name { get; set; } = "The Nameless One";
    public Dictionary<Skills, int> SkillPointAllocations { get; set; } = new();
}

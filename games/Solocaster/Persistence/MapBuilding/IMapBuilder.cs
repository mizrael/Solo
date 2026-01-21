namespace Solocaster.Persistence.MapBuilding;

public interface IMapBuilder
{
    MapBuildResult Build(MapBuildContext context);
}

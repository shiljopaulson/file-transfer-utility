namespace Sphere.FileTransfer.Cli.Mappers;

public interface IMap<TFromType, TToType>
{
  TToType Map(TFromType fromType);
}

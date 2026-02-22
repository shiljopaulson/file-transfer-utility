namespace Sphere.FileTransfer.Cli.Mappers;

internal interface IMap<in TFromType, out TToType>
{
  TToType Map(TFromType fromType);
}
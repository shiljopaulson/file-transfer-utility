using System.CommandLine;

using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

internal sealed class ColumnOption : Option<byte>
{
  public ColumnOption() : base(OptionNames.Column, OptionNames.ColumnAlias)
  {
    Description = $"Column number (1-based).";
    Arity = ArgumentArity.ExactlyOne;
    Required = true;
    DefaultValueFactory = (result) =>
    {
      return (byte)1;
    };
    AddValidators();
  }

  private void AddValidators()
  {
    Validators.Add(result =>
    {
      if (result.GetValue<byte>(Name) < 1)
      {
        result.AddError($"Option '{Name}' provided value must be greater than or equal to 1");
      }
    });
  }
}
using System.CommandLine;

namespace FileSegregator.Cli.Options;

public sealed class ColumnOption : Option<byte>
{
  public ColumnOption() : base("--column", "-c")
  {
    Description = $"Column number (1-based), default: {DefaultValueFactory}";
    Arity = ArgumentArity.ExactlyOne;
    Required = true;
    DefaultValueFactory = (result) =>
    {
      return 1;
    };
    AddValidators();
  }

  private void AddValidators()
  {
    Validators.Add(result =>
    {
      if (result.GetValue<byte>(Name) < 1)
      {
        result.AddError($"Option '{Name}' provided value must be greater than or equal to {DefaultValueFactory}");
      }
    });
  }
}

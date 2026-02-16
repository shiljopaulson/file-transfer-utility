using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

public sealed class SearchPatternOption : Option<string>
{
  public SearchPatternOption() : base(OptionNames.SearchPattern, OptionNames.SearchPatternAlias)
  {
    Description = $"Search pattern (Eg: *.txt, AB*.txt, A*8.txt).";
    Arity = ArgumentArity.ExactlyOne;
    DefaultValueFactory = (result) =>
    {
      return "*.*";
    };
    AddValidators();
  }
  private void AddValidators()
  {
    Validators.Add(result =>
    {
      var searchPattern = result.GetValue<string>(Name);

      if (string.IsNullOrWhiteSpace(searchPattern))
      {
        result.AddError($"Option '{Name}' is required");
        return;
      }

      // Check for characters that are illegal in filenames
      // Note: We exclude '*' and '?' because they are valid for search patterns
      var invalidChars = Path.GetInvalidFileNameChars()
          .Where(c => c != '*' && c != '?');

      if (searchPattern.Any(c => invalidChars.Contains(c)))
      {
        result.AddError($"Option '{Name}' provided with invalid Search Pattern");
      }
    });
  }
}
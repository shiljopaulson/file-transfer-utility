using FluentValidation;

namespace Sphere.FileTransfer.Services.Validators;

public static class RuleBuilderExtensions
{
  public static IRuleBuilderOptions<T, DirectoryInfo[]> NoDuplicates<T>(
        this IRuleBuilder<T, DirectoryInfo[]> ruleBuilder)
  {
    return ruleBuilder.Must(static (_, directories) =>
    {
      if (directories == null || directories.Length == 0)
        return true;

      var normalized = directories.Select(Normalize);

      var stringComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
      var normalizedCount = normalized
              .Distinct(stringComparer)
              .Count();
      return normalizedCount == directories.Length;
    })
    .WithMessage("Duplicate directories are not allowed.");
  }

  public static IRuleBuilderOptions<T, DirectoryInfo> DirectoryMustExist<T>(
        this IRuleBuilder<T, DirectoryInfo> ruleBuilder)
  {
    return ruleBuilder.Must((_, directory) =>
    {
      return directory is not null && directory.Exists;
    })
    .WithMessage("Directory must exist.");
  }

  public static IRuleBuilderOptions<T, FileInfo> FileMustExist<T>(
        this IRuleBuilder<T, FileInfo> ruleBuilder)
  {
    return ruleBuilder.Must((_, fileInfo) =>
    {
      return fileInfo is not null && fileInfo.Exists;
    })
    .WithMessage("File must exist.");
  }

  public static IRuleBuilderOptionsConditions<T, DirectoryInfo[]> AllDirectoriesMustExist<T>(
          this IRuleBuilder<T, DirectoryInfo[]> ruleBuilder)
  {
    return ruleBuilder.Custom((directories, context) =>
    {
      if (directories == null) return;

      var nonExisting = directories
          .Where(d => !d.Exists)
          .Select(d => d.FullName)
          .ToList();

      if (nonExisting.Any())
      {
        context.AddFailure(
            $"The following directories do not exist: {string.Join(", ", nonExisting)}");
      }
    });
  }

  private static string Normalize(DirectoryInfo directory)
  {
    return Path.GetFullPath(
        directory.FullName.TrimEnd(Path.DirectorySeparatorChar));
  }
}
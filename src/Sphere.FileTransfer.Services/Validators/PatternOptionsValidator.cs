using FluentValidation;
using Sphere.FileTransfer.Models;

namespace Sphere.FileTransfer.Services.Validators;

public class PatternOptionsValidator : AbstractValidator<PatternOptions>
{
  public PatternOptionsValidator()
  {
    RuleFor(x => x.Sources).NotNull();
    RuleFor(x => x.Sources.Length).GreaterThan(0);
    RuleFor(x => x.Sources).NoDuplicates();
    RuleFor(x => x.Sources).AllDirectoriesMustExist();
    RuleFor(x => x.Destination).NotNull();
    RuleFor(x => x.Destination).DirectoryMustExist();
    RuleFor(x => x.SearchPattern).NotNull().NotEmpty();
  }
}
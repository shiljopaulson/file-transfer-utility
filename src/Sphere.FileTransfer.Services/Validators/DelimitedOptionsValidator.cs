using FluentValidation;
using Sphere.FileTransfer.Models;

namespace Sphere.FileTransfer.Services.Validators;

public class DelimitedOptionsValidator : AbstractValidator<DelimitedOptions>
{
  public DelimitedOptionsValidator()
  {
    RuleFor(x => x.Sources).NotNull();
    RuleFor(x => x.Sources.Length).GreaterThan(0);
    RuleFor(x => x.Sources).NoDuplicates();
    RuleFor(x => x.Sources).AllDirectoriesMustExist();
    RuleFor(x => x.Destination).NotNull();
    RuleFor(x => x.Destination).DirectoryMustExist();
    RuleFor(x => x.File).NotNull();
    RuleFor(x => x.File).FileMustExist();
    RuleFor<int>(x => x.Column).GreaterThan(0);
    RuleFor(x => x.Delimiter).NotEmpty().NotNull();
  }
}
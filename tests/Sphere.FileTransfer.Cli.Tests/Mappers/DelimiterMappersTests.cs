using Sphere.FileTransfer.Cli.Mappers;
using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli.Tests.Mappers;

public sealed class DelimiterMappersTests
{
  // ── DelimiterToChar ──────────────────────────────────────────────────────

  [Theory]
  [InlineData(0, ',')] // Comma
  [InlineData(1, '\t')] // Tab
  [InlineData(2, '|')] // Pipe
  public void DelimiterToChar_Map_ReturnsExpectedChar(int input, char expected)
    => Assert.Equal(expected, new DelimiterToChar().Map((Delimiter)input));

  // ── CharToDelimiter ──────────────────────────────────────────────────────

  [Theory]
  [InlineData(',', 0)] // Comma
  [InlineData('\t', 1)] // Tab
  [InlineData('|', 2)] // Pipe
  [InlineData(';', 0)] // unknown char falls back to Comma
  public void CharToDelimiter_Map_ReturnsExpectedDelimiter(char input, int expected)
    => Assert.Equal((Delimiter)expected, new CharToDelimiter().Map(input));
}
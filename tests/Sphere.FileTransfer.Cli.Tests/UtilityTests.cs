using System.Text;

namespace Sphere.FileTransfer.Cli.Tests;

public sealed class UtilityTests : IDisposable
{
  private readonly string _tempRoot;

  public UtilityTests()
  {
    _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(_tempRoot);
  }

  public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

  private string WriteFile(byte[] bytes, string name = "test.txt")
  {
    var path = Path.Combine(_tempRoot, name);
    File.WriteAllBytes(path, bytes);
    return path;
  }

  // ── ToJson ───────────────────────────────────────────────────────────────

  [Fact]
  public void ToJson_NullObject_ReturnsEmptyString()
    => Assert.Equal(string.Empty, Utility.ToJson<object>(null!));

  [Fact]
  public void ToJson_SimpleObject_ReturnsValidJson()
  {
    var json = Utility.ToJson(new { Name = "test", Value = 42 });
    Assert.Contains("\"Name\"", json);
    Assert.Contains("\"test\"", json);
    Assert.Contains("42", json);
  }

  // ── IsAscii ──────────────────────────────────────────────────────────────

  [Fact]
  public void IsAscii_AsciiOnlyFile_ReturnsTrue()
  {
    var path = WriteFile(Encoding.ASCII.GetBytes("hello,world\nfoo,bar"));
    Assert.True(Utility.IsAscii(path));
  }

  [Fact]
  public void IsAscii_FileWithNonAsciiBytes_ReturnsFalse()
  {
    var path = WriteFile([0x48, 0xC3, 0xA9, 0x6C, 0x6C, 0x6F]); // "Héllo" in UTF-8
    Assert.False(Utility.IsAscii(path));
  }

  [Fact]
  public void IsAscii_EmptyFile_ReturnsTrue()
  {
    var path = WriteFile([]);
    Assert.True(Utility.IsAscii(path));
  }

  [Fact]
  public void IsAscii_NonExistentFile_ReturnsFalse()
    => Assert.False(Utility.IsAscii(Path.Combine(_tempRoot, "missing.txt")));

  // ── GetEncoding ──────────────────────────────────────────────────────────

  [Fact]
  public void GetEncoding_Utf8BomFile_ReturnsUtf8()
  {
    var path = WriteFile([0xEF, 0xBB, 0xBF, 0x61, 0x62, 0x63]); // UTF-8 BOM + "abc"
    Assert.Equal(Encoding.UTF8, Utility.GetEncoding(path));
  }

  [Fact]
  public void GetEncoding_Utf16LeBomFile_ReturnsUnicode()
  {
    var path = WriteFile([0xFF, 0xFE, 0x61, 0x00]); // UTF-16 LE BOM + "a"
    Assert.Equal(Encoding.Unicode, Utility.GetEncoding(path));
  }

  [Fact]
  public void GetEncoding_AsciiFile_ReturnsAscii()
  {
    var path = WriteFile(Encoding.ASCII.GetBytes("hello"));
    Assert.Equal(Encoding.ASCII, Utility.GetEncoding(path));
  }
}
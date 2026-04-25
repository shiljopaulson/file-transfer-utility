using Microsoft.Extensions.Logging.Abstractions;

using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;

namespace Sphere.FileTransfer.Services.Tests.Readers;

public sealed class DelimitedReaderTests : IDisposable
{
  private readonly string _tempRoot;
  private readonly DelimitedReader _reader;

  public DelimitedReaderTests()
  {
    _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(_tempRoot);
    _reader = new DelimitedReader(NullLogger<DelimitedReader>.Instance);
  }

  public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

  private string WriteFile(string content, string name = "test.csv")
  {
    var path = Path.Combine(_tempRoot, name);
    File.WriteAllText(path, content);
    return path;
  }

  [Fact]
  public void Read_CsvWithHeader_FirstLineMarkedSkipped()
  {
    var path = WriteFile("name,size\nfile1.txt,100\nfile2.txt,200");
    var result = _reader.Read(path, ',', hasHeader: true, CancellationToken.None);
    Assert.Equal(LineStatus.Skipped, result.Lines[0].Status);
    Assert.Equal(LineStatus.Unprocessed, result.Lines[1].Status);
    Assert.Equal(LineStatus.Unprocessed, result.Lines[2].Status);
  }

  [Fact]
  public void Read_CsvWithoutHeader_AllLinesUnprocessed()
  {
    var path = WriteFile("file1.txt,100\nfile2.txt,200");
    var result = _reader.Read(path, ',', hasHeader: false, CancellationToken.None);
    Assert.Equal(2, result.Lines.Length);
    Assert.All(result.Lines, l => Assert.Equal(LineStatus.Unprocessed, l.Status));
  }

  [Fact]
  public void Read_TabDelimited_ParsesFieldsCorrectly()
  {
    var path = WriteFile("file1.txt\t100\nfile2.txt\t200");
    var result = _reader.Read(path, '\t', hasHeader: false, CancellationToken.None);
    Assert.Equal("file1.txt", result.Lines[0].DelimitedFields[0]);
    Assert.Equal("100", result.Lines[0].DelimitedFields[1]);
  }

  [Fact]
  public void Read_PipeDelimited_ParsesFieldsCorrectly()
  {
    var path = WriteFile("file1.txt|100\nfile2.txt|200");
    var result = _reader.Read(path, '|', hasHeader: false, CancellationToken.None);
    Assert.Equal("file1.txt", result.Lines[0].DelimitedFields[0]);
    Assert.Equal(2, result.Lines[0].DelimitedFields.Length);
  }

  [Fact]
  public void Read_EmptyFile_ReturnsNoLines()
  {
    var path = WriteFile("");
    var result = _reader.Read(path, ',', hasHeader: false, CancellationToken.None);
    Assert.Equal(FileStatus.Unprocessed, result.Status);
    Assert.Empty(result.Lines);
  }

  [Fact]
  public void Read_NonExistentFile_ReturnsErrorStatus()
  {
    var result = _reader.Read(Path.Combine(_tempRoot, "missing.csv"), ',', hasHeader: false, CancellationToken.None);
    Assert.Equal(FileStatus.Error, result.Status);
    Assert.NotNull(result.Message);
  }

  [Fact]
  public void Read_CancelledToken_ReturnsCanceledStatus()
  {
    var path = WriteFile("file1.txt\nfile2.txt\nfile3.txt");
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result = _reader.Read(path, ',', hasHeader: false, cts.Token);
    Assert.Equal(FileStatus.Canceled, result.Status);
  }

  [Fact]
  public void Read_LineNumbers_AreOneBasedAndSequential()
  {
    var path = WriteFile("a\nb\nc");
    var result = _reader.Read(path, ',', hasHeader: false, CancellationToken.None);
    Assert.Equal(1, result.Lines[0].Number);
    Assert.Equal(2, result.Lines[1].Number);
    Assert.Equal(3, result.Lines[2].Number);
  }

  [Fact]
  public void Read_StoredDelimiterAndHeader_MatchInput()
  {
    var path = WriteFile("a\tb\nc\td");
    var result = _reader.Read(path, '\t', hasHeader: true, CancellationToken.None);
    Assert.Equal('\t', result.Delimiter);
    Assert.True(result.HasHeader);
  }
}
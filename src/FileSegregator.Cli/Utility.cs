using System.Text;
using System.Text.Json;

namespace FileSegregator.Cli;


public static class Utility
{
  public static void WriteLine(string message, ConsoleColor consoleColor)
  {
    Console.ForegroundColor = consoleColor;
    Console.WriteLine(message);
    Console.ResetColor();
  }

  public static string ToJson<T>(T tObj, bool writeIndented = true) where T : class
  {
    var jsonOptions = new JsonSerializerOptions { WriteIndented = writeIndented };
    string prettyJson = JsonSerializer.Serialize(tObj, jsonOptions);
    return prettyJson;
  }

  /// <summary>
  /// Determines a text file's encoding by analyzing its byte order mark (BOM).
  /// Defaults to null when detection of the text file's endianness fails.
  /// </summary>
  /// <param name="fileFullName">The text file to analyze.</param>
  /// <returns>The detected encoding.</returns>
  public static Encoding? GetEncoding(string fileFullName)
  {
    // Read the BOM
    var bom = new byte[4];
    using (var file = new FileStream(fileFullName, FileMode.Open, FileAccess.Read))
    {
      file.ReadExactly(bom, 0, 4);
    }

    // Analyze the BOM
    //if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
    if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
    if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
    if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
    if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
    if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE
    if (IsAscii(fileFullName)) { return Encoding.ASCII; }

    return null;
  }

  public static bool IsAscii(string filePath)
  {
    // Define the maximum value for an ASCII byte
    const byte maxAsciiValue = 127;

    try
    {
      // Read all bytes from the file
      byte[] bytes = File.ReadAllBytes(filePath);

      // Iterate through the bytes to check if any are non-ASCII
      foreach (byte b in bytes)
      {
        if (b > maxAsciiValue)
        {
          // Found a non-ASCII character
          return false;
        }
      }

      // All bytes are within the ASCII range
      return true;
    }
    catch (Exception)
    {
      // Handle potential file access errors
      return false;
    }
  }
}

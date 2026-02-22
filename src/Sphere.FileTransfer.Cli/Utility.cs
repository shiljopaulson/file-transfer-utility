using System.Reflection;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http.Json;

using Sphere.FileTransfer.Cli.Models;

namespace Sphere.FileTransfer.Cli;


internal static class Utility
{
  private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

  public static void WriteLine(string message, ConsoleColor consoleColor)
  {
    Console.ForegroundColor = consoleColor;
    Console.WriteLine(message);
    Console.ResetColor();
  }

  public static string ToJson<T>(T tObj)
  {
    if (tObj is null)
    {
      return string.Empty;
    }
    string prettyJson = JsonSerializer.Serialize(tObj, JsonSerializerOptions);
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
      for (int i = 0; i < bytes.Length; i++)
      {
        byte b = bytes[i];
        if (b > maxAsciiValue)
        {
          // Found a non-ASCII character
          return false;
        }
      }

      // All bytes are within the ASCII range
      return true;
    }
    catch (OperationCanceledException)
    {
      return false;
    }
    catch (ArgumentNullException)
    {
      return false;
    }
    catch (ArgumentException)
    {
      return false;
    }
    catch (PathTooLongException)
    {
      return false;
    }
    catch (DirectoryNotFoundException)
    {
      return false;
    }
    catch (FileNotFoundException)
    {
      return false;
    }
    catch (IOException)
    {
      return false;
    }
    catch (UnauthorizedAccessException)
    {
      return false;
    }
    catch (NotSupportedException)
    {
      return false;
    }
    catch (System.Security.SecurityException)
    {
      return false;
    }
  }

  internal static AssemblyDetails? GetAssemblyDetails()
  {
    var entryAssembly = Assembly.GetEntryAssembly();
    if (entryAssembly is null)
    {
      return null;
    }
    var informationalVersion = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    var assemblyDetails = new AssemblyDetails
    {
      Trademark = entryAssembly.GetCustomAttribute<AssemblyTrademarkAttribute>()?.Trademark,
      Copyright = entryAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright,
      Company = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company,
      Title = entryAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title,
      Description = entryAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description,
      Product = entryAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
      Contributors = entryAssembly.GetMetadata("Contributors"),
      MoreInfo = entryAssembly.GetMetadata("MoreInfo"),
      License = entryAssembly.GetMetadata("License"),
      InformationalVersion = informationalVersion,
      Version = informationalVersion?.Split("+")[0],
    };
    return assemblyDetails;
  }

  private static string? GetMetadata(this Assembly assembly, string key)
  {
    return assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == key)?.Value;
  }
}

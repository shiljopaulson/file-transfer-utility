# File Transfer Utility (ftu)

A data driven file transfer utility which copies or moves files from one or multiple source directories to the destination directory based on the `delimited` (.csv, .tsv, .psv) file or directory `search pattern` (Example: \*.png, \*.\*).

## Prerequisites

- [.NET SDK 10.0.103](https://dotnet.microsoft.com/download) or later

## Installation

```bash
dotnet publish -c Release
```

The `ftu` executable will be produced as a single-file binary in the publish output directory.

## How does it work

Provide the `--sources` directory/directories from which files need to be copied/moved to the `--destination` directory. Choose `delimited` as a sub-command if you are relying on a delimited file (.csv, .tsv, .psv), or use `pattern` to match files by wildcard.

### Sub-commands

- delimited
- pattern

### delimited

```
Description:
  Copies or Moves files based on the file name entries found in the delimited file (comma,tab,pipe)

Usage:
  ftu delimited [options]

Options:
  -s, --sources <sources> (REQUIRED)          Sources are the directories to search for files to be copied or moved.
  -d, --destination <destination> (REQUIRED)  Destination directory to (copy or move) files from one of the Sources
                                              (--sources) directory.
  -f, --file <file> (REQUIRED)                Delimited file (Refer --delimiter for supported delimiters).
  -c, --column <column> (REQUIRED)            Column number (1-based, minimum 1). [default: 1]
  --delimiter <Comma|Pipe|Tab>                Field delimiter character (Comma|Tab|Pipe). [default: Comma]
  --operation <Copy|Move>                     File operation (Copy|Move). [default: Copy]
  --no-header                                 Input file has no header row.
  --output-format <Json|Text>                 Output format (Text|Json). [default: Text]
  --overwrite                                 Overwrite existing files in destination.
  --dry-run                                   Do not actually perform any file operations.
  --quiet                                     Execute without printing.
  -?, -h, --help                              Show help and usage information
```

#### Example usage — copy files listed in a CSV:

```
delimited
--sources ~/Photos/sources/01
--sources ~/Photos/sources/02
--destination ~/ProjectA/destination
-f ~/ProjectA/delimited_file.csv
-c 11
```

This will copy files from `~/Photos/sources/01` and `~/Photos/sources/02` to `~/ProjectA/destination` where the filename matches an entry in column 11 of `~/ProjectA/delimited_file.csv`.

#### Example usage — dry-run move with tab-delimited file:

```
delimited
--sources ~/Photos/sources/01
--destination ~/ProjectA/destination
-f ~/ProjectA/file_list.tsv
-c 1
--delimiter Tab
--operation Move
--dry-run
```

Using `--dry-run` previews which files would be moved without making any changes.

### pattern

```
Description:
  Copies or Moves files based on the search patterns (Example: *.png, *.txt, *.*)

Usage:
  ftu pattern [options]

Options:
  -s, --sources <sources> (REQUIRED)          Sources are the directories to search for files to be copied or moved.
  -d, --destination <destination> (REQUIRED)  Destination directory to (copy or move) files from one of the Sources
                                              (--sources) directory.
  -sp, --search-pattern <search-pattern>      Search pattern (Eg: *.txt, AB*.txt, A*8.txt). [default: *.* (all files)]
  --operation <Copy|Move>                     File operation (Copy|Move). [default: Copy]
  --output-format <Json|Text>                 Output format (Text|Json). [default: Text]
  --overwrite                                 Overwrite existing files in destination.
  --dry-run                                   Do not actually perform any file operations.
  --quiet                                     Execute without printing.
  -?, -h, --help                              Show help and usage information
```

#### Example usage — copy files by pattern:

```
pattern
--sources ~/Photos/sources/01
--sources ~/Photos/sources/02
--destination ~/ProjectA/destination
-sp "*.png"
```

This will copy all `.png` files from `~/Photos/sources/01` and `~/Photos/sources/02` to `~/ProjectA/destination`.

#### Example usage — move all files with dry-run preview:

```
pattern
--sources ~/Photos/sources/01
--destination ~/ProjectA/destination
--operation Move
--dry-run
```

Omitting `-sp` defaults to `*.*` (all files). Using `--dry-run` previews which files would be moved without making any changes.

# File Transfer Utility (ftu)

A data driven file transfer utility which copies or moves files from one or multiple source directories to the destination directory based on the `delimited file` (.csv, .tsv, .psv) or directory `search pattern` (Example: \*.png, \*.\*).

## How does it work

Provide the `--sources` directory/directories from which files needs to be copied/moved to the `--destination` directory. In case if you rely on delimited file (.csv, .tsv, .psv) choose `delimited` as sub-command or use `pattern`.

### Sub-commands

- delimited
- pattern

### delimited

```
Description:
  Segregate files using a file names found delimited files (Comma,Tab,Pipe)

Usage:
  FileSegregator delimited [options]

Options:
  -s, --sources <sources> (REQUIRED)                 Sources are the directories to search for files to be copied or moved.
  -d, --destination <destination> (REQUIRED)         Destination directory to (copy or move) files from one of the Sources
                                                     (--sources) directory.
  -df, --delimited-file <delimited-file> (REQUIRED)  Delimited file (Refer --delimiter for supported delimiters).
  -c, --column <column> (REQUIRED)                   Column number (1-based). [default: 1]
  --delimiter <Comma|Pipe|Tab>                       Field delimiter character (Comma|Tab|Pipe). [default: Comma]
  --operation <Copy|Move>                            File operation (Copy|Move). [default: Copy]
  --no-header                                        Input file has no header row.
  --output-format <JSON|Text>                        Output format (Text|JSON). [default: Text]
  --overwrite                                        Overwrite existing files in destination.
  --dry-run                                          Do not actually perform any file operations.
  --quiet                                            Execute without printing.
  -?, -h, --help                                     Show help and usage information
```

#### Example usage:

```
delimited
--sources ~/Photos/sources/01
--sources ~/Photos/sources/02
--destination ~/ProjectA/destination
-df ~/ProjectA/delimited_file.csv
--column 11
```

This will copy files from `~/Photos/sources/01` and `~/Photos/sources/02` to `~/ProjectA/destination` and if there is an entry in `~/ProjectA/delimited_file.csv` the delimited file.

### pattern

```
Description:
  Segregate files using a search patterns (*.*)

Usage:
  FileSegregator pattern [options]

Options:
  -s, --sources <sources> (REQUIRED)          Sources are the directories to search for files to be copied or moved.
  -d, --destination <destination> (REQUIRED)  Destination directory to (copy or move) files from one of the Sources
                                              (--sources) directory.
  -sp, --search-pattern <search-pattern>      Search pattern (Eg: *.txt, AB*.txt, A*8.txt). [default: *.*]
  --operation <Copy|Move>                     File operation (Copy|Move). [default: Copy]
  --output-format <JSON|Text>                 Output format (Text|JSON). [default: Text]
  --overwrite                                 Overwrite existing files in destination.
  --dry-run                                   Do not actually perform any file operations.
  --quiet                                     Execute without printing.
  -?, -h, --help                              Show help and usage information
```

#### Example usage:

```
pattern
--sources ~/Photos/sources/01
--sources ~/Photos/sources/02
--destination ~/ProjectA/destination
-sp "*.png"
```

This will copy files from `~/Photos/sources/01` and `~/Photos/sources/02` to `~/ProjectA/destination` and if it matches the search pattern `*.png`. Here it will copy all files with extension `*.png` to the destination directory.

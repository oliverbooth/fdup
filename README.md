# Find Duplicates (fdup)

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/oliverbooth/fdup/dotnet.yml?style=flat-square)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/oliverbooth/fdup?style=flat-square)
![GitHub License](https://img.shields.io/github/license/oliverbooth/fdup?style=flat-square)

## About
fdup is a small command-line utility written in C# to quickly and easily find duplicate files. It can also search recursively to find duplicate files in child directories.

## Usage
```bash
$ fdup --help
USAGE:
    fdup <path> [OPTIONS]

ARGUMENTS:
    <path>    The path to search

OPTIONS:
    -h, --help         Prints help information
    -v, --version      Prints version information
    -r, --recursive    When this flag is set, the directory will be scanned recursively. This may take longer
```

## Example
```bash
$ echo "Hello World" > file1
$ echo "Goodbye World" > file2
$ fdup .
Searching /home/user/example
Recursive mode is OFF
Checking hash for file2
Checking hash for file1

No duplicates found!
$ echo "Hello World" > file2
$ fdup .
Searching /home/user/example
Recursive mode is OFF
Checking hash for file2
Checking hash for file1

Found 2 identical files
SHA512 E1C112FF908FEBC3B98B1693A6CD3564EAF8E5E6CA629D084D9F0EBA99247CACDD72E369FF8941397C2807409FF66BE64BE908DA17AD7B8A49A2A26C0E8086AA:
- /home/user/example/file1
- /home/user/example/file2

Found 2 duplicates!
```

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

X10D is released under the MIT License. See [here](https://github.com/oliverbooth/X10D/blob/main/LICENSE.md) for more details.

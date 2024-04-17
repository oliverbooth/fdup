using System.Security.Cryptography;

namespace FindDuplicates;

internal static class AlgorithmExtensions
{
    public static int GetByteCount(this Algorithm algorithm)
    {
        return algorithm switch
        {
            Algorithm.Sha512 => SHA512.HashSizeInBytes,
            Algorithm.Sha384 => SHA384.HashSizeInBytes,
            Algorithm.Sha256 => SHA256.HashSizeInBytes,
            Algorithm.Sha3512 => SHA3_512.HashSizeInBytes,
            Algorithm.Sha3384 => SHA3_384.HashSizeInBytes,
            Algorithm.Sha3256 => SHA3_256.HashSizeInBytes,
            Algorithm.Sha1 => SHA1.HashSizeInBytes,
            Algorithm.Md5 => MD5.HashSizeInBytes,
            _ => 0
        };
    }

    public static int HashData(this Algorithm algorithm, Stream source, Span<byte> destination)
    {
        // I'd love to use a dictionary to cache the function map, but you can't use Span<> as a type argument,
        // probably due to the fact that a lambda heap allocs, and we can't have that for ref structs, oh no (!)
        // so enjoy this absolutely cursed switch expression which checks each algorithm separately. I hate it too.

        return algorithm switch
        {
            Algorithm.Sha512 => SHA512.HashData(source, destination),
            Algorithm.Sha384 => SHA384.HashData(source, destination),
            Algorithm.Sha256 => SHA256.HashData(source, destination),
            Algorithm.Sha3512 => SHA3_512.HashData(source, destination),
            Algorithm.Sha3384 => SHA3_384.HashData(source, destination),
            Algorithm.Sha3256 => SHA3_256.HashData(source, destination),
            Algorithm.Sha1 => SHA1.HashData(source, destination),
            Algorithm.Md5 => MD5.HashData(source, destination),
            _ => -1
        };
    }
}

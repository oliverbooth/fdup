using System.ComponentModel;

namespace FindDuplicates;

internal enum Algorithm
{
    [Description("SHA512")] Sha512,
    [Description("SHA384")] Sha384,
    [Description("SHA256")] Sha256,
    [Description("SHA3-512")] Sha3512,
    [Description("SHA3-384")] Sha3384,
    [Description("SHA3-256")] Sha3256,
    [Description("SHA1")] Sha1,
    [Description("MD5")] Md5
}

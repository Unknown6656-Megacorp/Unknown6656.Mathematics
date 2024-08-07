﻿///////////////////////////////////////////////////////////////////////
//             AUTOGENERATED 2024-07-28 23:18:31.077098              //
//   All your changes to this file will be lost upon re-generation.  //
///////////////////////////////////////////////////////////////////////

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System;

namespace Unknown6656.Mathematics.Cryptography;


public static partial class HashFunction
{
    public static MD5Hash MD5 { get; } = new();
    public static SHA1Hash SHA1 { get; } = new();
    public static SHA256Hash SHA256 { get; } = new();
    public static SHA384Hash SHA384 { get; } = new();
    public static SHA512Hash SHA512 { get; } = new();
}

public sealed class MD5Hash
    : HashFunction<MD5Hash>
    , IDisposable
{
    private readonly MD5 _md5;

    public override int HashSize { get; } = 16;


    public MD5Hash()
    {
        _md5 = MD5.Create();
        _md5.Initialize();
    }

    public void Dispose()
    {
        _md5.Clear();
        _md5.Dispose();
    }

    public override byte[] Hash(byte[] data) => _md5.ComputeHash(data);
}

public sealed class SHA1Hash
    : HashFunction<SHA1Hash>
    , IDisposable
{
    private readonly SHA1 _sha1;

    public override int HashSize { get; } = 20;


    public SHA1Hash()
    {
        _sha1 = SHA1.Create();
        _sha1.Initialize();
    }

    public void Dispose()
    {
        _sha1.Clear();
        _sha1.Dispose();
    }

    public override byte[] Hash(byte[] data) => _sha1.ComputeHash(data);
}

public sealed class SHA256Hash
    : HashFunction<SHA256Hash>
    , IDisposable
{
    private readonly SHA256 _sha256;

    public override int HashSize { get; } = 32;


    public SHA256Hash()
    {
        _sha256 = SHA256.Create();
        _sha256.Initialize();
    }

    public void Dispose()
    {
        _sha256.Clear();
        _sha256.Dispose();
    }

    public override byte[] Hash(byte[] data) => _sha256.ComputeHash(data);
}

public sealed class SHA384Hash
    : HashFunction<SHA384Hash>
    , IDisposable
{
    private readonly SHA384 _sha384;

    public override int HashSize { get; } = 48;


    public SHA384Hash()
    {
        _sha384 = SHA384.Create();
        _sha384.Initialize();
    }

    public void Dispose()
    {
        _sha384.Clear();
        _sha384.Dispose();
    }

    public override byte[] Hash(byte[] data) => _sha384.ComputeHash(data);
}

public sealed class SHA512Hash
    : HashFunction<SHA512Hash>
    , IDisposable
{
    private readonly SHA512 _sha512;

    public override int HashSize { get; } = 64;


    public SHA512Hash()
    {
        _sha512 = SHA512.Create();
        _sha512.Initialize();
    }

    public void Dispose()
    {
        _sha512.Clear();
        _sha512.Dispose();
    }

    public override byte[] Hash(byte[] data) => _sha512.ComputeHash(data);
}

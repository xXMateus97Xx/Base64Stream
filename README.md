# Base64Stream
A C# stream implementation to decode base64 strings

# Purpose

Avoid to allocate memory at LOH when recieving files encoded in base64.

Base64Stream allows to decode base64 string in chuncks, could be used Upload files with HttpClient without allocate a big array.

# Benchmarks

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 5 3600, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  DefaultJob : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT


```
|                  Method |                Value |          Mean |        Error |       StdDev |   Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|------------------------ |--------------------- |--------------:|-------------:|-------------:|--------:|--------:|--------:|----------:|
| **UsingConvertAndEncoding** | **77u(...)A== [261584]** | **287,548.86 ns** | **1,701.180 ns** | **1,591.284 ns** | **55.1758** | **55.1758** | **55.1758** | **196,234 B** |
|       UsingBase64Stream | 77u(...)A== [261584] | 265,635.23 ns |   716.655 ns |   670.360 ns |       - |       - |       - |   1,112 B |
| **UsingConvertAndEncoding** |                 **MQ==** |      **21.28 ns** |     **0.091 ns** |     **0.085 ns** |  **0.0038** |       **-** |       **-** |      **32 B** |
|       UsingBase64Stream |                 MQ== |      52.23 ns |     0.055 ns |     0.046 ns |  0.1329 |       - |       - |   1,112 B |
| **UsingConvertAndEncoding** |  **TG9y(...)Lg== [768]** |     **834.47 ns** |     **1.932 ns** |     **1.808 ns** |  **0.0715** |       **-** |       **-** |     **600 B** |
|       UsingBase64Stream |  TG9y(...)Lg== [768] |     803.56 ns |     8.090 ns |     6.316 ns |  0.1326 |       - |       - |   1,112 B |

# Usage

Upload to the Internet

```cs
string base64 = Convert.ToBase64String(GetSomeBytes());

using var stream = new Base64Stream(base64);

UploadToBlob(stream);
```

Read in chuncks
```cs
string base64 = Convert.ToBase64String(GetSomeBytes());

using var stream = new Base64Stream(base64);

byte[] bytes = new byte[1024];

int read;
while ((read = stream.Read(bytes, 0, bytes.Length) > 0)
{
    //Use content...
}

```

# Comments

Base64Stream throws exception at constructor when string is null or empty or length is not multiple of 4.

Read method throws exception if buffer length is less than 3 or if there are some invalid character on string.

Thread safety is not guaranteed.

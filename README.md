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
| **UsingConvertAndEncoding** | **77u(...)A== [261584]** | **283,070.82 ns** | **1,104.666 ns** | **1,033.306 ns** | **55.1758** | **55.1758** | **55.1758** | **196,234 B** |
|       UsingBase64Stream | 77u(...)A== [261584] | 270,927.78 ns |   258.727 ns |   242.013 ns |       - |       - |       - |   1,104 B |
| **UsingConvertAndEncoding** |                 **MQ==** |      **21.91 ns** |     **0.106 ns** |     **0.099 ns** |  **0.0038** |       **-** |       **-** |      **32 B** |
|       UsingBase64Stream |                 MQ== |      55.13 ns |     0.240 ns |     0.224 ns |  0.1320 |       - |       - |   1,104 B |
| **UsingConvertAndEncoding** |  **TG9y(...)Lg== [768]** |     **836.40 ns** |     **2.270 ns** |     **2.123 ns** |  **0.0715** |       **-** |       **-** |     **600 B** |
|       UsingBase64Stream |  TG9y(...)Lg== [768] |     838.72 ns |    16.663 ns |    15.587 ns |  0.1316 |       - |       - |   1,104 B |

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

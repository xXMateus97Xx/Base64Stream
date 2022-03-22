using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text;

BenchmarkRunner.Run<Base64StreamBenchmarks>();

[MemoryDiagnoser]
public class Base64StreamBenchmarks
{
    [ParamsSource(nameof(Values))]
    public string Value { get; set; }

    public string[] Values { get; set; }

    public Base64StreamBenchmarks()
    {
        Values = new[]
        {
            Convert.ToBase64String(Encoding.UTF8.GetBytes("1")),
            Convert.ToBase64String(Encoding.UTF8.GetBytes("Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.")),
            Convert.ToBase64String(File.ReadAllBytes("hamlet.txt"))
        };
    }

    [Benchmark]
    public int UsingConvertAndEncoding()
    {
        var bytes = Convert.FromBase64String(Value);

        return bytes.Length;
    }

    [Benchmark]
    public long UsingBase64Stream()
    {
        var stream = new Base64Stream.Base64Stream(Value);
        var bytes = new byte[1024];

        while (stream.Read(bytes, 0, 1024) > 0) ;

        return stream.Position;
    }
}

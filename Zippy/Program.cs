// See https://aka.ms/new-console-template for more information
using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;

const int DATA_SIZE = 50 * 1024;
const int DATA_COUNT = 50 * 1024;

byte[] NextDataBytes()
{
    var bytes = new byte[DATA_SIZE];
    Random.Shared.NextBytes(bytes);
    return bytes;
};

void DatasWriter(IEnumerable<byte[]> datas, string fileName, CompressionLevel CompressionLevel)
{
    using var zipStream = File.Open(fileName, FileMode.Create);
    using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create);

    int count = 0;
    foreach (byte[] data in datas)
    {
        var zipEntry = zipArchive.CreateEntry($"{count++}", CompressionLevel);
        using var zipWriter = new BinaryWriter(zipEntry.Open());
        zipWriter.Write(data, 0, data.Length);
    }
}

void DatasWriterFiles(IEnumerable<byte[]> datas, string fileName)
{
    Parallel.For(0, datas.Count(), new ParallelOptions() { MaxDegreeOfParallelism = 20 }, i =>
    {
        File.WriteAllBytes($"{fileName}{i}", datas.ElementAt((int)i));
    });
}

IEnumerable<byte[]> DatasReader(string fileName)
{
    var datas = new List<byte[]>();

    using var zipStream = File.Open(fileName, FileMode.Open);
    using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

    return zipArchive.Entries.Select(v => (new BinaryReader(v.Open())).ReadBytes((int)v.Length)).ToArray();
};

void StopWatchBenchmark(Action action, string name)
{
    var sw = Stopwatch.StartNew();
    action.Invoke();
    Console.WriteLine($"Runtime {name}: {sw.ElapsedMilliseconds}mS");
};

Console.WriteLine("Zippy :-)");

var datas = Enumerable.Range(0, DATA_COUNT).Select(v => NextDataBytes()).ToArray();

StopWatchBenchmark(() => DatasWriter(datas, "ZippyUncompressed.zip", CompressionLevel.NoCompression), "write NoCompression");

StopWatchBenchmark(() => DatasWriterFiles(datas, "Zippy"), "write files");

IEnumerable<byte[]> readDatas = null;
StopWatchBenchmark(() => readDatas = DatasReader("ZippyUncompressed.zip"), "read NoCompression");

StopWatchBenchmark(() =>
{
    bool datasIsEqual = true;
    for (int i = 0; i < DATA_COUNT && datasIsEqual; i++)
    {
        datasIsEqual = Enumerable.SequenceEqual(readDatas?.ElementAt(i) ?? Enumerable.Empty<byte>(), datas[i]);
    }

    Console.WriteLine($"Data sets are equal: {datasIsEqual}");
}, "data sets compare");

Console.WriteLine("Enter to Exit...");
Console.ReadLine();
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

IEnumerable<byte[]> DatasReader(string fileName)
{
    var datas = new List<byte[]>();

    using var zipStream = File.Open(fileName, FileMode.Open);
    using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

    return zipArchive.Entries.Select(v => (new BinaryReader(v.Open())).ReadBytes((int)v.Length)).ToArray();
};

void DatasWriterFiles(IEnumerable<byte[]> datas, string fileName)
{
    Parallel.For(0, datas.Count(), new ParallelOptions() { MaxDegreeOfParallelism = 20 }, i =>
    {
        File.WriteAllBytes($"{fileName}{i}", datas.ElementAt((int)i));
    });
}

IEnumerable<byte[]> DatasReaderFiles(string fileName)
{
    var datas = new byte[DATA_COUNT][];
    Parallel.For(0, DATA_COUNT, new ParallelOptions() { MaxDegreeOfParallelism = 20 }, i =>
    {
        datas[i] = File.ReadAllBytes($"{fileName}{i}");
    });
    return datas;
}

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

IEnumerable<byte[]> readFileDatas = null;
StopWatchBenchmark(() => readFileDatas = DatasReaderFiles("Zippy"), "read files");

StopWatchBenchmark(() =>
{
    bool datasIsEqual = true;
    for (int i = 0; i < DATA_COUNT && datasIsEqual; i++)
    {
        datasIsEqual = Enumerable.SequenceEqual(readDatas?.ElementAt(i) ?? Enumerable.Empty<byte>(), datas[i]);
    }

    Console.WriteLine($"NoCompression data sets are equal: {datasIsEqual}");
}, "NoCompression data sets compare");

StopWatchBenchmark(() =>
{
    bool datasIsEqual = true;
    for (int i = 0; i < DATA_COUNT && datasIsEqual; i++)
    {
        datasIsEqual = Enumerable.SequenceEqual(readFileDatas?.ElementAt(i) ?? Enumerable.Empty<byte>(), datas[i]);
    }

    Console.WriteLine($"Files data sets are equal: {datasIsEqual}");
}, "Files data sets compare");

Console.WriteLine("Enter to Exit...");
Console.ReadLine();
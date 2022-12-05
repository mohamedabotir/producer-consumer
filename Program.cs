using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

class DataflowProducerConsumer
{
    static void Produce(ITargetBlock<byte[]> target)
    {
        var rand = new Random();

        for (int i = 0; i < 100; ++i)
        {
            var buffer = new byte[1024];
            rand.NextBytes(buffer);
            target.Post(buffer);

        }

        target.Complete();
    }

    static async Task<int> ConsumeSignleConsumerAsync(ISourceBlock<byte[]> source)
    {
        int bytesProcessed = 0;

        while (await source.OutputAvailableAsync())
        {
            byte[] data = await source.ReceiveAsync();
            bytesProcessed += data.Length;
            Console.WriteLine($"Processed {bytesProcessed:#,#} bytes. Consumer");
            Thread.Sleep(1000);
        }

        return bytesProcessed;
    }
    static async Task<int> ConsumeAsync(IReceivableSourceBlock<byte[]> source)
    {
        int bytesProcessed = 0;
        while (await source.OutputAvailableAsync())
        {
            while (source.TryReceive(out byte[] data))
            {
                bytesProcessed += data.Length;
                Console.WriteLine($"Processed {bytesProcessed:#,#} bytes. consumer {Thread.GetCurrentProcessorId()}");
                Thread.Sleep(500);
            }
        }
        return bytesProcessed;
    }
    static async Task Main()
    {
        var buffer = new BufferBlock<byte[]>();
        var consumerTask = ConsumeAsync(buffer);
        var consumer1Task = ConsumeAsync(buffer);
        Produce(buffer);

        var bytesProcessed = await consumerTask;
        Console.WriteLine($"Processed {bytesProcessed:#,#} bytes. Producer");
        var bytesProcessed1 = await consumer1Task;
        Console.WriteLine($"Processed {bytesProcessed1:#,#} bytes. Producer");

    }
}

// Sample  output:
//     Processed 102,400 bytes.
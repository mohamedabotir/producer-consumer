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
            Console.WriteLine("Produce 1024");
            Thread.Sleep(100);
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
    static async Task<int> ConsumeMultiConsumerAsync(IReceivableSourceBlock<byte[]> source)
    {
        int bytesProcessed = 0;
        while (await source.OutputAvailableAsync())
        {
            while (source.TryReceive(out byte[] data))
            {
                bytesProcessed += data.Length;
                Console.WriteLine($"Processed {bytesProcessed:#,#} bytes. consumer {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(500);
            }
        }
        return bytesProcessed;
    }
    static async Task Main()
    {
        var buffer = new BufferBlock<byte[]>();
        var consumerTask = ConsumeMultiConsumerAsync(buffer);
        var consumer1Task = ConsumeMultiConsumerAsync(buffer);
        Produce(buffer);

        var bytesProcessed = await consumerTask;
        var bytesProcessed1 = await consumer1Task;

    }
}

// Sample  output:
//     Processed 102,400 bytes.
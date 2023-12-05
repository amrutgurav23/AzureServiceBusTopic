using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

// the sender used to publish messages to the topic
ServiceBusSender sender;

// number of messages to be sent to the topic
const int numOfMessages = 3;

// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.
//TODO: Replace the "<NAMESPACE-CONNECTION-STRING>" and "<TOPIC-NAME>" placeholders.
client = new ServiceBusClient("Endpoint=sb://az204servicebusdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=HeEkEA2PGIabw48I2tgRGyca0QknIYPUL+ASbD+4ubQ=");


#region Send messages to the topic
//sender = client.CreateSender("mytopic");

//// create a batch 
//using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

//for (int i = 1; i <= numOfMessages; i++)
//{
//    // try adding a message to the batch
//    if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
//    {
//        // if it is too large for the batch
//        throw new Exception($"The message {i} is too large to fit in the batch.");
//    }
//}

//try
//{
//    // Use the producer client to send the batch of messages to the Service Bus topic
//    await sender.SendMessagesAsync(messageBatch);
//    Console.WriteLine($"A batch of {numOfMessages} messages has been published to the topic.");
//}
//finally
//{
//    // Calling DisposeAsync on client types is required to ensure that network
//    // resources and other unmanaged objects are properly cleaned up.
//    await sender.DisposeAsync();
//    await client.DisposeAsync();
//} 
#endregion

#region Receive messages from the subscription
// create a processor that we can use to process the messages
// TODO: Replace the <TOPIC-NAME> and <SUBSCRIPTION-NAME> placeholders
// the processor that reads and processes messages from the subscription
ServiceBusProcessor processor;
processor = client.CreateProcessor("mytopic", "sub1", new ServiceBusProcessorOptions());

try
{
    // add handler to process messages
    processor.ProcessMessageAsync += MessageHandler;

    // add handler to process any errors
    processor.ProcessErrorAsync += ErrorHandler;

    // start processing 
    await processor.StartProcessingAsync();

    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.ReadKey();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await processor.DisposeAsync();
    await client.DisposeAsync();
}

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body} from subscription.");

    // complete the message. messages is deleted from the subscription. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}
#endregion

Console.WriteLine("Press any key to end the application");
Console.ReadKey();
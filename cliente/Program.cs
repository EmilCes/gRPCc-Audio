using Grpc.Core;
using Grpc.Net.Client;
//using NAudio.Wave;
using System.Media;
using static AudioService;


static async Task<MemoryStream> descargaStreamAsync(AudioServiceClient stub, string nombre_archivo) 
{
    using var call = stub.downloadAudio(new DownloadFileRequest 
    {
        Nombre = nombre_archivo
    });

    Console.WriteLine($"Recibiendo el archivo: {nombre_archivo}");
    var writeStream = new MemoryStream();
    await foreach (var message in call.ResponseStream.ReadAllAsync())
    {
        if (message.Data != null) 
        {
            var bytes = message.Data.Memory;
            Console.WriteLine(".");
            await writeStream.WriteAsync(bytes);
        }
    }

    // Se recibieron todos los datos
    Console.WriteLine("\nRecepción de datos correcta.\n\n");
    return writeStream;
}

// static async Task playStream(MemoryStream stream, string nombre_archivo)
// {
//     if (stream != null) 
//     {
//         Console.WriteLine($"Reproduciendo el archivo: {nombre_archivo}");

//         // Posiciona el MemoryStream al principio de los datos de audio
//         stream.Seek(0, SeekOrigin.Begin);

//         // Inicializa el lector de audio
//         using (var audioFile = new WaveFileReader(stream))
//         {
//             using (var outputDevice = new WaveOutEvent())
//             {
//                 // Inicializa el dispositivo de salida de audio
//                 outputDevice.Init(audioFile);

//                 // Reproduce el audio
//                 outputDevice.Play();

//                 // Espera hasta que se complete la reproducción
//                 while (outputDevice.PlaybackState == PlaybackState.Playing)
//                 {
//                     await Task.Delay(100);
//                 }
//             }
//         }
//     }
// }

static void playStream(MemoryStream stream, string nombre_archivo) 
{
    if (stream != null) 
    {
        Console.WriteLine($"Reproduciendo el archivo: {nombre_archivo}");
        SoundPlayer player = new(stream);
        player.Stream?.Seek(0, SeekOrigin.Begin);
        player.Play();
    }
}

// Establece el servidor gRPC
using var channel = GrpcChannel.ForAddress("http://localhost:8080");

// Crea el canal de comunicación
AudioServiceClient stub = new(channel);

string nombre_archivo = "sample.wav";

// Descarga el stream
MemoryStream stream = await descargaStreamAsync(stub, nombre_archivo);

// Reproduce el stream
playStream(stream, nombre_archivo);

Console.WriteLine("Presione cualquier tecla para terminar el programa..."); Console.ReadKey();
stream.Close();
Console.WriteLine("Apagando...");
channel.ShutdownAsync().Wait();
using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

public interface ICommand
{
    Task ExecuteAsync();
}

public class VideoInfoCommand : ICommand
{
    private readonly string _url;

    public VideoInfoCommand(string url)
    {
        _url = url;
    }

    public async Task ExecuteAsync()
    {
        var youtube = new YoutubeClient();
        var video = await youtube.Videos.GetAsync(_url);
        Console.WriteLine($"Название: {video.Title}");
        Console.WriteLine($"Описание: {video.Description}");
    }
}

public class DownloadVideoCommand : ICommand
{
    private readonly string _url;

    public DownloadVideoCommand(string url)
    {
        _url = url;
    }

    public async Task ExecuteAsync()
    {
        var youtube = new YoutubeClient();
        var video = await youtube.Videos.GetAsync(_url);
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
        var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestBitrate();

        if (streamInfo != null)
        {
            var filePath = $"{video.Title}.{streamInfo.Container}";
            await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
            Console.WriteLine($"Видео '{video.Title}' успешно скачано в '{filePath}'!");
        }
    }
}

public class CommandInvoker
{
    private readonly ICommand _infoCommand;
    private readonly ICommand _downloadCommand;

    public CommandInvoker(ICommand infoCommand, ICommand downloadCommand)
    {
        _infoCommand = infoCommand;
        _downloadCommand = downloadCommand;
    }

    public async Task ExecuteCommand(string command)
    {
        switch (command)
        {
            case "info":
                await _infoCommand.ExecuteAsync();
                break;
            case "download":
                await _downloadCommand.ExecuteAsync();
                break;
            default:
                Console.WriteLine("Команда не найдена.");
                break;
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Введите ссылку на YouTube-видео: ");
        var url = Console.ReadLine();

        var infoCommand = new VideoInfoCommand(url);
        var downloadCommand = new DownloadVideoCommand(url);
        var invoker = new CommandInvoker(infoCommand, downloadCommand);

        while (true)
        {
            Console.Write("Введите команду (info/download/exit): ");
            var command = Console.ReadLine();

            if (command == "exit")
                break;

            await invoker.ExecuteCommand(command);
        }
    }
}
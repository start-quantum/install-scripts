using System.Net;
using ShellProgressBar;

namespace StartQuantum;

public static class Download
{
    public static async Task<string> File(string uri, string baseName)
    {
        var target = Path.Join(Path.GetTempPath(), baseName);
        var handler = new HttpClientHandler();
        using var client = new HttpClient(handler);
        using var response = await client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        var downloadSize = response.Content.Headers.ContentLength;
        using var fileStream = new FileStream(target, FileMode.Create);


        var responseStream = await response.Content.ReadAsStreamAsync();
        var buffer = new Byte[8192];
        var nTotalBytesRead = 0;

        using var progressBar = new ProgressBar(
            100,
            $"Downloading {baseName}...",
            new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true,
                ShowEstimatedDuration = true
            }
        );
        var progress = progressBar.AsProgress<double>();

        do
        {
            var nBytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length);
            if (nBytesRead == 0)
            {
                break;
            }

            await fileStream.WriteAsync(buffer, 0, nBytesRead);
            nTotalBytesRead += nBytesRead;
            if (downloadSize != null)
            {
                progress.Report(nTotalBytesRead / (double)(downloadSize));
            }
        }
        while (true);

        return target;
    }
}

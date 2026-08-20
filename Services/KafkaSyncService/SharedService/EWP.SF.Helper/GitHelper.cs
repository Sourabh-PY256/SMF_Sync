using System.Diagnostics;

namespace EWP.SF.Helper;

/// <summary>
/// A helper class for Git operations.
/// </summary>
public static class GitHelper
{
    /// <summary>
    /// Gets the current Git user.
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetCurrentGitUser()
    {
#if DEBUG
        //return await GitCommand().ConfigureAwait(false);
        return await Task.FromResult(string.Empty).ConfigureAwait(false);
#else
        return await Task.FromResult(string.Empty).ConfigureAwait(false);
#endif
    }

    private static async Task<string> GitCommand(int retryCount = 0)
    {
        const int maxRetries = 3;

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "git",
                Arguments = "config user.email",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(startInfo);
            string output = process is not null ? (await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false)).Trim() : string.Empty;
            string error = process is not null ? (await process.StandardError.ReadToEndAsync().ConfigureAwait(false)).Trim() : string.Empty;

            if (!string.IsNullOrWhiteSpace(output))
            {
                return output.Split('@')[0]; // Extract user email before '@'
            }

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"[Warning] Git error: {error}");
            }
        }
        catch
        {
            Console.WriteLine("[Warning] Git not found or not installed.");
        }

        if (retryCount < maxRetries)
        {
            Console.WriteLine($"[Info] Retry attempt {retryCount + 1} of {maxRetries}...");
            await Task.Delay(500).ConfigureAwait(false); // Optional delay between retries
            return await GitCommand(retryCount + 1).ConfigureAwait(false);
        }

        Console.WriteLine($"[Warning] Failed to get Git user after {maxRetries} attempts.");
        throw new InvalidOperationException($"Failed to get Git user after {maxRetries} attempts.");
    }
}

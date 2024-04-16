using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FFmpeg.AutoGen;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoEditingController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<VideoEditingController> _logger;

        public VideoEditingController(ILogger<VideoEditingController> logger, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

  

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertVideo()
        {
            string rootPath = _hostingEnvironment.ContentRootPath;
            string videosFolderPath = Path.Combine(rootPath, "videos");
            string inputFilePath = Path.Combine(videosFolderPath, "input.avi");
            string outputFilePath = Path.Combine(videosFolderPath, "result.m3u8");

            //-c:v copy -c:a aac -hls_time 10 - hls_list_size 0
            string ffmpegArgs = $"-i {inputFilePath} -c:v libx264 -c:a aac -hls_time 10 -hls_list_size 0 {outputFilePath}";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process ffmpegProcess = Process.Start(psi))
            {
                Task<string> readOutputTask = ffmpegProcess.StandardOutput.ReadToEndAsync();
                Task<string> readErrorTask = ffmpegProcess.StandardError.ReadToEndAsync();

                string output = await readOutputTask;
                string error = await readErrorTask;

                await Task.WhenAll(readOutputTask, readErrorTask);

                await ffmpegProcess.WaitForExitAsync();


                if (ffmpegProcess.ExitCode == 0)
                {
                    return Ok("Video conversion successful!");
                }
                else
                {
                    return BadRequest($"Video conversion failed! Error: {error}");
                }
            }
        }
    }
}

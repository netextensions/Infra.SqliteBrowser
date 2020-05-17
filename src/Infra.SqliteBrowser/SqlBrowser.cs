using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetExtensions.Infra.SqliteBrowser
{
    public class SqlBrowser : BackgroundService
    {
        private readonly ILogger<SqlBrowser> _logger;
        private readonly Process _process;
        private readonly bool _runnable = true;
        public SqlBrowser(ILogger<SqlBrowser> logger, SqlBrowserParameters parameters)
        {
            _logger = logger;
            if (string.IsNullOrWhiteSpace(parameters.FullDbPath))
            {
                logger.LogWarning("SqlBrowser won't be executed, reason: empty db path");
                _runnable = false;
                return;
            }
            if (string.IsNullOrWhiteSpace(parameters.FullDbPath) || !File.Exists(parameters.FullDbPath))
            {
                logger.LogWarning($"SqlBrowser won't be executed, reason db file doesn't exist path: {parameters.FullDbPath}");
                _runnable = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(parameters.Port))
            {
                parameters.Port = "8888";
            }
            if (string.IsNullOrWhiteSpace(parameters.Host))
            {
                parameters.Host = "127.0.0.1";
            }

            var args = $"{parameters.FullDbPath} -p {parameters.Port} -H {parameters.Host}  -x ";
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sqlite_web",
                    Arguments = args, 
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_runnable)
            {
                return;
            }

            var started = false;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!started)
                {
                    try
                    {
                        started = _process.Start();
                    }
                    catch (Exception e)
                    {
                        started = false;
                        _logger.LogError(10000, e, "SqlBrowser - Cannot Execute sqlite_web Process");

                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
        

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_runnable)
            {
                return;
            }
            _process.Close();
            await Task.CompletedTask;
        }
    }
}
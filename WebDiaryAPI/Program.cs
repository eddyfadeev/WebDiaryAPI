using WebDiaryAPI;

var builder = await CreateHostBuilder(args);
await builder.Build().RunAsync();
return;

static async Task<IHostBuilder> CreateHostBuilder(string[] args) => 
    await Task.Run(() => Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        }));
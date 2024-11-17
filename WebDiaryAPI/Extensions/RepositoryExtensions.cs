using WebDiaryAPI.Data.Repository;

namespace WebDiaryAPI.Extensions;

public static class RepositoryExtensions
{
    public static void AddRepository(this IServiceCollection services) =>
        services.AddScoped<IDiaryEntriesRepository, DiaryEntriesRepository>();
}
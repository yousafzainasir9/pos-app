using POS.Infrastructure.Data.Interceptors;

namespace POS.Migrator.Services;

public class MigratorCurrentUserService : ICurrentUserService
{
    public long? UserId => null;

    public string? Username => "Migrator";

    public string? Email => null;
}

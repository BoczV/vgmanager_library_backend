using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Repositories.Boilerplate;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.Interfaces.VGRepositories;

namespace VGManager.Library.Repositories.VGRepositories;

public class VGAddColdRepository : SqlRepository<VGAddEntity>, IVGAddColdRepository
{
    public VGAddColdRepository(OperationsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task AddEntityAsync(VGAddEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<VGAddEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAllAsync(new AdditionSpecification(), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGAddEntity>();
    }

    public async Task<IEnumerable<VGAddEntity>> GetAsync(
        string organization,
        string project,
        string user,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new AdditionSpecification(organization, project, user, from, to.AddDays(1)), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGAddEntity>();
    }

    public async Task<IEnumerable<VGAddEntity>> GetAsync(
        string organization,
        string project,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new AdditionSpecification(organization, project, from, to.AddDays(1)), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGAddEntity>();
    }

    public class AdditionSpecification : SpecificationBase<VGAddEntity>
    {
        public AdditionSpecification() : base(additionEntity => !string.IsNullOrEmpty(additionEntity.Id))
        {
        }

        public AdditionSpecification(string organization, string project, string user, DateTime from, DateTime to) : base(
            additionEntity => additionEntity.Date >= from &&
            additionEntity.Date <= to &&
            additionEntity.Organization == organization &&
            additionEntity.Project == project &&
            additionEntity.User.Contains(user)
            )
        {
        }

        public AdditionSpecification(string organization, string project, DateTime from, DateTime to) : base(
            additionEntity => additionEntity.Date >= from &&
            additionEntity.Date <= to &&
            additionEntity.Organization == organization &&
            additionEntity.Project == project
            )
        {
        }
    }
}
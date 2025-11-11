using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TKV.Model.DbContext;
using TKV.Model.DbModels;
using TKV.Model.JsonModels;
using TKV.Service;
using Xunit;

namespace TKV.Test;

public class MainServicesTests
{
    private MyDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MyDbContext(options);
    }

    [Fact]
    public async Task CreateRequest_Should_Save_Request_And_Types_When_Valid()
    {
        await using var db = CreateInMemoryContext();
        var service = new MainServices(db);
        var model = new RequestModel
        {
            Title = "Health Coverage",
            Surgery = true,
            SurgeryBudget = 10000,
            Dentistry = true,
            DentistryBudget = 5000,
            Hospitalization = true,
            HospitalizationBudget = 3000
        };

        var result = await service.CreateRequest(model);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("saved successfully");

        var request = await db.Request.FirstOrDefaultAsync();
        request.Should().NotBeNull();
        request!.Title.Should().Be("Health Coverage");

        var types = await db.RequestType.ToListAsync();
        types.Should().HaveCount(Enum.GetValues(typeof(CoverageType)).Length);
        types.Should().Contain(t => t.Budget == 10000);
        types.Should().Contain(t => t.Budget == 5000);
        types.Should().Contain(t => t.Budget == 3000);
    }

    [Theory]
    [InlineData(true, 1000, false, 0, false, 0, "You cant put less than 5000 budget for Surgery coverage.")]
    [InlineData(true, 600000000, false, 0, false, 0, "You cant put more than 500000000 budget for Surgery coverage.")]
    [InlineData(false, 0, true, 3000, false, 0, "You cant put less than 4000 budget for Dentistry coverage.")]
    [InlineData(false, 0, true, 500000000, false, 0, "You cant put more than 400000000 budget for Dentistry coverage.")]
    [InlineData(false, 0, false, 0, true, 1000, "You cant put less than 2000 budget for Hospitalization coverage.")]
    [InlineData(false, 0, false, 0, true, 500000000, "You cant put more than 200000000 budget for Hospitalization coverage.")]
    public async Task CreateRequest_Should_Return_Error_For_Invalid_Budgets(
        bool surgery, int surgeryBudget,
        bool dentistry, int dentistryBudget,
        bool hospitalization, int hospitalizationBudget,
        string expectedMessage)
    {
        await using var db = CreateInMemoryContext();
        var service = new MainServices(db);
        var model = new RequestModel
        {
            Surgery = surgery,
            SurgeryBudget = surgeryBudget,
            Dentistry = dentistry,
            DentistryBudget = dentistryBudget,
            Hospitalization = hospitalization,
            HospitalizationBudget = hospitalizationBudget
        };

        var result = await service.CreateRequest(model);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task GetRequests_Should_Return_List_Of_RequestListModels()
    {
        await using var db = CreateInMemoryContext();

        var coverageList = new List<Coverage>
        {
            new() { Id = (int)CoverageType.Surgery, Title = "Surgery", ProfitCoefficient = (double)0.1M },
            new() { Id = (int)CoverageType.Dentistry, Title = "Dentistry", ProfitCoefficient = (double)0.2M },
            new() { Id = (int)CoverageType.Hospitalization, Title = "Hospitalization", ProfitCoefficient = (double)0.3M }
        };

        await db.Coverage.AddRangeAsync(coverageList);

        var request = new Request { Title = "Health Request" };
        await db.Request.AddAsync(request);
        await db.SaveChangesAsync();

        await db.RequestType.AddRangeAsync(new[]
        {
            new RequestType { RequestId = request.Id, CoverageId = (int)CoverageType.Surgery, Budget = 10000 },
            new RequestType { RequestId = request.Id, CoverageId = (int)CoverageType.Dentistry, Budget = 20000 },
            new RequestType { RequestId = request.Id, CoverageId = (int)CoverageType.Hospitalization, Budget = 30000 }
        });
        await db.SaveChangesAsync();

        var service = new MainServices(db);

        var result = await service.GetRequests();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var list = result.Data as List<RequestListModel>;
        list.Should().HaveCount(1);
        var model = list!.First();

        model.Surgery.Should().BeTrue();
        model.Dentistry.Should().BeTrue();
        model.Hospitalization.Should().BeTrue();
        model.TotalNetPremium.Should().BeApproximately((double)(10000 * 0.1M + 20000 * 0.2M + 30000 * 0.3M), (double)0.001M);
        result.Message.Should().Contain("loaded successfully");
    }
}

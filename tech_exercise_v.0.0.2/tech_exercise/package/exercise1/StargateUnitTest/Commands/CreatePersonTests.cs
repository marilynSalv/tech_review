using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

namespace StargateUnitTest.Commands;

[TestClass]
public class CreatePersonTests
{
    private DbContextOptions<StargateContext> _options;
    [TestInitialize]
    public void SetUp()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        _options = new DbContextOptionsBuilder<StargateContext>().UseSqlite(connection).Options;

        using (var context = new StargateContext(_options))
        {
            context.Database.EnsureCreated();
        }
    }

    [TestMethod]
    public async Task CreatePersonHandlerTest_NewPerson()
    {
        using var context = new StargateContext(_options);
        var handler = new CreatePersonHandler(context);

        var result = await handler.Handle(new CreatePerson() { Name = "Teresa Gonzales" }, default);

        var expectedResult = new CreatePersonResult
        {
            Id = 1,
        };
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task CreatePersonPreProcessorTest_PersonDoesNotExist()
    {
        using var context = new StargateContext(_options);

        var preProcessor = new CreatePersonPreProcessor(context);

        var param = new CreatePerson() { Name = "Teresa Gonzales" };
        var task = preProcessor.Process(param, default);
        await task;
        task.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task CreatePersonPreProcessorTest_PersonExists()
    {
        using (var context = new StargateContext(_options))
        {
            context.People.Add(new Person { Id = 1, Name = "Teresa Gonzales" });
            await context.SaveChangesAsync();
        }

        using (var context = new StargateContext(_options))
        {
            var preProcessor = new CreatePersonPreProcessor(context);

            var param = new CreatePerson() { Name = "Teresa Gonzales" };
            var act = async () => await preProcessor.Process(param, default);

            await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage("This person already exists");
        }
    }
}

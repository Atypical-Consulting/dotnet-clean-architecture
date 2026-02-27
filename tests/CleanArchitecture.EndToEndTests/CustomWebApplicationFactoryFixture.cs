namespace CleanArchitecture.EndToEndTests;

using System.Threading.Tasks;
using Xunit;

/// <summary>
/// </summary>
public sealed class CustomWebApplicationFactoryFixture : IAsyncLifetime
{
    public CustomWebApplicationFactoryFixture() =>
        this.CustomWebApplicationFactory = new CustomWebApplicationFactory();

    /// <summary>
    /// </summary>
    public CustomWebApplicationFactory CustomWebApplicationFactory { get; }

    public async Task InitializeAsync() =>
        await this.CustomWebApplicationFactory.InitializeAsync();

    public async Task DisposeAsync() =>
        await this.CustomWebApplicationFactory.DisposeAsync();
}

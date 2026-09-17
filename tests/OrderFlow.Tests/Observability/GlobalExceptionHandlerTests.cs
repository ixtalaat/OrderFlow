using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OrderFlow.API.ExceptionHandling;
using OrderFlow.Application.Common.Exceptions;

namespace OrderFlow.Tests.Observability;

public sealed class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        var problemDetails = new ServiceCollection().AddProblemDetails().AddOptions()
            .BuildServiceProvider().GetRequiredService<IProblemDetailsService>();
        _handler = new GlobalExceptionHandler(
            Substitute.For<ILogger<GlobalExceptionHandler>>(),
            problemDetails);
    }

    [Theory]
    [InlineData(typeof(NotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(ValidationException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(ConflictException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(ConcurrencyConflictException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(DbUpdateConcurrencyException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(UnauthorizedException), StatusCodes.Status401Unauthorized)]
    [InlineData(typeof(ForbiddenException), StatusCodes.Status403Forbidden)]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status500InternalServerError)]
    public async Task Should_Map_Exception_To_Status_Code(Type exceptionType, int statusCode)
    {
        var context = new DefaultHttpContext();
        var exception = (Exception)Activator.CreateInstance(exceptionType, "test failure")!;

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(statusCode);
    }
}

using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Customers;

public class CustomerEntityTests
{
    [Fact]
    public void Create_Should_Initialize_Customer_With_Valid_Values()
    {
        // Arrange & Act
        var customer = Customer.Create("user-123", "+1234567890", "123 Main St, Springfield");

        // Assert
        customer.Should().NotBeNull();
        customer.UserId.Should().Be("user-123");
        customer.PhoneNumber.Should().Be("+1234567890");
        customer.Address.Should().Be("123 Main St, Springfield");
        customer.IsActive.Should().BeTrue();
        customer.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        customer.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData("", "+1234567890", "123 Main St")]
    [InlineData("   ", "+1234567890", "123 Main St")]
    [InlineData("user-123", "", "123 Main St")]
    [InlineData("user-123", "   ", "123 Main St")]
    [InlineData("user-123", "+1234567890", "")]
    [InlineData("user-123", "+1234567890", "   ")]
    public void Create_Should_Throw_ArgumentException_When_Parameters_Invalid(string userId, string phone, string address)
    {
        // Act
        var act = () => Customer.Create(userId, phone, address);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_Should_Modify_Phone_And_Address_And_Set_UpdatedAtUtc()
    {
        // Arrange
        var customer = Customer.Create("user-123", "+1234567890", "123 Main St");

        // Act
        customer.UpdateDetails("+9876543210", "456 Oak Ave");

        // Assert
        customer.PhoneNumber.Should().Be("+9876543210");
        customer.Address.Should().Be("456 Oak Ave");
        customer.UpdatedAtUtc.Should().NotBeNull();
        customer.UpdatedAtUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Deactivate_And_Activate_Should_Change_IsActive_State()
    {
        // Arrange
        var customer = Customer.Create("user-123", "+1234567890", "123 Main St");
        customer.IsActive.Should().BeTrue();

        // Act - Deactivate
        customer.Deactivate();

        // Assert
        customer.IsActive.Should().BeFalse();
        customer.UpdatedAtUtc.Should().NotBeNull();

        // Act - Activate
        customer.Activate();

        // Assert
        customer.IsActive.Should().BeTrue();
    }
}

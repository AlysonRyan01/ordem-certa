using FluentAssertions;
using OrdemCerta.Domain.Customers;

namespace OrdemCerta.Tests.Domain.ValueObjects;

public class CustomerPhoneTests
{
    [Theory]
    [InlineData("11987654321")]
    [InlineData("(11) 98765-4321")]
    [InlineData("11 98765-4321")]
    public void Create_WithValidCelular_ReturnsSuccess(string phone)
    {
        var result = CustomerPhone.Create(phone);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AreaCode.Should().Be("11");
        result.Value.Value.Should().Be("11987654321");
    }

    [Theory]
    [InlineData("1134567890")]
    [InlineData("(11) 3456-7890")]
    public void Create_WithValidFixo_ReturnsSuccess(string phone)
    {
        var result = CustomerPhone.Create(phone);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AreaCode.Should().Be("11");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPhone_ReturnsFailure(string? phone)
    {
        var result = CustomerPhone.Create(phone);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("Telefone é obrigatório");
    }

    [Fact]
    public void Create_WithCelularNotStartingWith9_ReturnsFailure()
    {
        var result = CustomerPhone.Create("11887654321");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("Telefone celular deve começar com 9");
    }

    [Fact]
    public void Create_WithFixoStartingWith9_ReturnsFailure()
    {
        var result = CustomerPhone.Create("1193456789");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("Telefone fixo não deve começar com 9");
    }

    [Fact]
    public void Create_WithInvalidLength_ReturnsFailure()
    {
        var result = CustomerPhone.Create("119876");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("Telefone deve conter 10 dígitos (fixo) ou 11 dígitos (celular)");
    }

    [Fact]
    public void GetFormatted_Celular_ReturnsCorrectFormat()
    {
        var phone = CustomerPhone.Create("11987654321").Value!;

        phone.GetFormatted().Should().Be("(11) 98765-4321");
    }

    [Fact]
    public void GetFormatted_Fixo_ReturnsCorrectFormat()
    {
        var phone = CustomerPhone.Create("1134567890").Value!;

        phone.GetFormatted().Should().Be("(11) 3456-7890");
    }
}

using FluentAssertions;
using OrdemCerta.Domain.ServiceOrders.ValueObjects;

namespace OrdemCerta.Tests.Domain.ValueObjects;

public class BudgetTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = Budget.Create(150.00m, "Troca de tela");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be(150.00m);
        result.Value.Description.Should().Be("Troca de tela");
    }

    [Fact]
    public void Create_TrimsDescription()
    {
        var result = Budget.Create(100m, "  Troca de bateria  ");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("Troca de bateria");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_WithInvalidValue_ReturnsFailure(decimal value)
    {
        var result = Budget.Create(value, "Descrição válida");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("O valor do orçamento deve ser maior que zero");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_ReturnsFailure(string? description)
    {
        var result = Budget.Create(100m, description!);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("A descrição do orçamento é obrigatória");
    }

    [Fact]
    public void Create_WithDescriptionExceeding500Chars_ReturnsFailure()
    {
        var longDescription = new string('a', 501);

        var result = Budget.Create(100m, longDescription);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("A descrição deve ter no máximo 500 caracteres");
    }

    [Fact]
    public void TwoBudgets_WithSameValues_AreEqual()
    {
        var b1 = Budget.Create(200m, "Reparo na placa").Value!;
        var b2 = Budget.Create(200m, "Reparo na placa").Value!;

        b1.Should().Be(b2);
    }

    [Fact]
    public void TwoBudgets_WithDifferentValues_AreNotEqual()
    {
        var b1 = Budget.Create(200m, "Reparo na placa").Value!;
        var b2 = Budget.Create(300m, "Reparo na placa").Value!;

        b1.Should().NotBe(b2);
    }
}

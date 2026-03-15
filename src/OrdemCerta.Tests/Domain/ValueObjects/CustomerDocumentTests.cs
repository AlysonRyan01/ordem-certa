using FluentAssertions;
using OrdemCerta.Domain.Customers.Enums;
using OrdemCerta.Domain.Customers.ValueObjects;

namespace OrdemCerta.Tests.Domain.ValueObjects;

public class CustomerDocumentTests
{
    // CPF válido: 529.982.247-25
    private const string ValidCpf = "52998224725";
    private const string ValidCpfFormatted = "529.982.247-25";

    // CNPJ válido: 11.222.333/0001-81
    private const string ValidCnpj = "11222333000181";
    private const string ValidCnpjFormatted = "11.222.333/0001-81";

    [Theory]
    [InlineData(ValidCpf)]
    [InlineData(ValidCpfFormatted)]
    public void Create_WithValidCpf_ReturnsSuccess(string cpf)
    {
        var result = CustomerDocument.Create(cpf);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Type.Should().Be(CustomerDocumentType.Cpf);
        result.Value.Value.Should().Be(ValidCpf);
    }

    [Theory]
    [InlineData(ValidCnpj)]
    [InlineData(ValidCnpjFormatted)]
    public void Create_WithValidCnpj_ReturnsSuccess(string cnpj)
    {
        var result = CustomerDocument.Create(cnpj);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Type.Should().Be(CustomerDocumentType.Cnpj);
        result.Value.Value.Should().Be(ValidCnpj);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDocument_ReturnsFailure(string? document)
    {
        var result = CustomerDocument.Create(document);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("Documento é obrigatório");
    }

    [Fact]
    public void Create_WithInvalidCpf_ReturnsFailure()
    {
        var result = CustomerDocument.Create("12345678901");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("CPF inválido");
    }

    [Fact]
    public void Create_WithAllSameDigitsCpf_ReturnsFailure()
    {
        var result = CustomerDocument.Create("11111111111");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("CPF inválido");
    }

    [Fact]
    public void Create_WithInvalidCnpj_ReturnsFailure()
    {
        var result = CustomerDocument.Create("12345678000100");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("CNPJ inválido");
    }

    [Fact]
    public void Create_WithWrongLength_ReturnsFailure()
    {
        var result = CustomerDocument.Create("123456789");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("Documento deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ)");
    }

    [Fact]
    public void GetFormatted_Cpf_ReturnsCorrectFormat()
    {
        var doc = CustomerDocument.Create(ValidCpf).Value!;

        doc.GetFormatted().Should().Be(ValidCpfFormatted);
    }

    [Fact]
    public void GetFormatted_Cnpj_ReturnsCorrectFormat()
    {
        var doc = CustomerDocument.Create(ValidCnpj).Value!;

        doc.GetFormatted().Should().Be(ValidCnpjFormatted);
    }
}

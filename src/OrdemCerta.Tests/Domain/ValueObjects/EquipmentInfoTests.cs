using FluentAssertions;
using OrdemCerta.Domain.ServiceOrders.ValueObjects;

namespace OrdemCerta.Tests.Domain.ValueObjects;

public class EquipmentInfoTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = EquipmentInfo.Create("Smartphone", "Samsung", "Galaxy S24", "Tela quebrada");

        result.IsSuccess.Should().BeTrue();
        result.Value!.DeviceType.Should().Be("Smartphone");
        result.Value.Brand.Should().Be("Samsung");
        result.Value.Model.Should().Be("Galaxy S24");
        result.Value.ReportedDefect.Should().Be("Tela quebrada");
        result.Value.Accessories.Should().BeNull();
        result.Value.Observations.Should().BeNull();
    }

    [Fact]
    public void Create_WithOptionalFields_SetsCorrectly()
    {
        var result = EquipmentInfo.Create(
            "Notebook", "Dell", "Inspiron 15", "Não liga",
            "Carregador", "Cliente relatou queda");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Accessories.Should().Be("Carregador");
        result.Value.Observations.Should().Be("Cliente relatou queda");
    }

    [Fact]
    public void Create_TrimsAllStringFields()
    {
        var result = EquipmentInfo.Create(
            "  Tablet  ", "  Apple  ", "  iPad Pro  ", "  Bateria ruim  ");

        result.IsSuccess.Should().BeTrue();
        result.Value!.DeviceType.Should().Be("Tablet");
        result.Value.Brand.Should().Be("Apple");
        result.Value.Model.Should().Be("iPad Pro");
        result.Value.ReportedDefect.Should().Be("Bateria ruim");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDeviceType_ReturnsFailure(string? deviceType)
    {
        var result = EquipmentInfo.Create(deviceType!, "Samsung", "A54", "Tela");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("O tipo de aparelho é obrigatório");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyBrand_ReturnsFailure(string? brand)
    {
        var result = EquipmentInfo.Create("Smartphone", brand!, "A54", "Tela");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("A marca é obrigatória");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyModel_ReturnsFailure(string? model)
    {
        var result = EquipmentInfo.Create("Smartphone", "Samsung", model!, "Tela");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("O modelo é obrigatório");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyReportedDefect_ReturnsFailure(string? defect)
    {
        var result = EquipmentInfo.Create("Smartphone", "Samsung", "A54", defect!);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("O defeito relatado é obrigatório");
    }

    [Fact]
    public void Create_WithDeviceTypeExceeding100Chars_ReturnsFailure()
    {
        var result = EquipmentInfo.Create(new string('x', 101), "Samsung", "A54", "Tela");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("O tipo de aparelho deve ter no máximo 100 caracteres");
    }

    [Fact]
    public void Create_WithReportedDefectExceeding500Chars_ReturnsFailure()
    {
        var result = EquipmentInfo.Create("Smartphone", "Samsung", "A54", new string('x', 501));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("O defeito relatado deve ter no máximo 500 caracteres");
    }

    [Fact]
    public void TwoEquipmentInfos_WithSameValues_AreEqual()
    {
        var e1 = EquipmentInfo.Create("Smartphone", "Samsung", "A54", "Tela").Value!;
        var e2 = EquipmentInfo.Create("Smartphone", "Samsung", "A54", "Tela").Value!;

        e1.Should().Be(e2);
    }
}

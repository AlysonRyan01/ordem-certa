using FluentAssertions;
using OrdemCerta.Domain.ServiceOrders;
using OrdemCerta.Domain.ServiceOrders.Enums;

namespace OrdemCerta.Tests.Domain.Aggregates;

public class ServiceOrderTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();

    private static ServiceOrder CreateOrder(string? technicianName = null) =>
        ServiceOrder.Create(CompanyId, CustomerId, 1, "Smartphone", "Samsung", "A54", "Tela quebrada", null, null, technicianName).Value!;

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ServiceOrder.Create(CompanyId, CustomerId, 1, "Smartphone", "Samsung", "A54", "Tela quebrada", null, null, "João");

        result.IsSuccess.Should().BeTrue();
        result.Value!.CompanyId.Should().Be(CompanyId);
        result.Value.CustomerId.Should().Be(CustomerId);
        result.Value.OrderNumber.Should().Be(1);
        result.Value.Status.Should().Be(ServiceOrderStatus.UnderAnalysis);
        result.Value.TechnicianName.Should().Be("João");
        result.Value.BudgetValue.Should().BeNull();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithoutTechnician_SetsNullTechnician()
    {
        var order = CreateOrder();

        order.TechnicianName.Should().BeNull();
    }

    [Fact]
    public void ChangeStatus_UpdatesStatus()
    {
        var order = CreateOrder();

        var result = order.ChangeStatus(ServiceOrderStatus.UnderAnalysis);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(ServiceOrderStatus.UnderAnalysis);
    }

    [Fact]
    public void CreateBudget_WithValidData_SetsBudgetAndStatus()
    {
        var order = CreateOrder();

        var result = order.CreateBudget(350m, "Troca de tela", RepairResult.CanBeRepaired);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(ServiceOrderStatus.AwaitingApproval);
        order.BudgetValue.Should().Be(350m);
        order.BudgetDescription.Should().Be("Troca de tela");
    }

    [Fact]
    public void ApproveBudget_WhenWaitingApproval_ApprovesSuccessfully()
    {
        var order = CreateOrder();
        order.CreateBudget(350m, "Troca de tela", RepairResult.CanBeRepaired);
        order.MarkBudgetAsWaiting();

        var result = order.ApproveBudget();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(ServiceOrderStatus.BudgetApproved);
    }

    [Fact]
    public void ApproveBudget_WhenNotWaitingApproval_ReturnsFailure()
    {
        var order = CreateOrder();

        var result = order.ApproveBudget();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RefuseBudget_WhenWaitingApproval_RefusesSuccessfully()
    {
        var order = CreateOrder();
        order.CreateBudget(350m, "Troca de tela", RepairResult.CanBeRepaired);
        order.MarkBudgetAsWaiting();

        var result = order.RefuseBudget();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(ServiceOrderStatus.BudgetRefused);
    }

    [Fact]
    public void RefuseBudget_WhenNotWaitingApproval_ReturnsFailure()
    {
        var order = CreateOrder();

        var result = order.RefuseBudget();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateEquipment_ReplacesEquipment()
    {
        var order = CreateOrder();

        order.UpdateEquipment("Notebook", "Dell", "XPS 15", "Não liga", null, null);

        order.DeviceType.Should().Be("Notebook");
        order.Brand.Should().Be("Dell");
    }

    [Fact]
    public void UpdateTechnician_ReplacesTechnicianName()
    {
        var order = CreateOrder("João");

        order.UpdateTechnician("Maria");

        order.TechnicianName.Should().Be("Maria");
    }
}

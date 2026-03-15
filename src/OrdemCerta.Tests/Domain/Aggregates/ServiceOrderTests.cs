using FluentAssertions;
using OrdemCerta.Domain.ServiceOrders;
using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Domain.ServiceOrders.Events;
using OrdemCerta.Domain.ServiceOrders.ValueObjects;

namespace OrdemCerta.Tests.Domain.Aggregates;

public class ServiceOrderTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();

    private static EquipmentInfo ValidEquipment() =>
        EquipmentInfo.Create("Smartphone", "Samsung", "A54", "Tela quebrada").Value!;

    private static ServiceOrder CreateOrder(string? technicianName = null) =>
        ServiceOrder.Create(CompanyId, CustomerId, 1, ValidEquipment(), technicianName).Value!;

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ServiceOrder.Create(CompanyId, CustomerId, 1, ValidEquipment(), "João");

        result.IsSuccess.Should().BeTrue();
        result.Value!.CompanyId.Should().Be(CompanyId);
        result.Value.CustomerId.Should().Be(CustomerId);
        result.Value.OrderNumber.Should().Be(1);
        result.Value.Status.Should().Be(ServiceOrderStatus.Received);
        result.Value.TechnicianName.Should().Be("João");
        result.Value.Budget.Should().BeNull();
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
    public void ChangeStatus_ToReadyForPickup_RaisesServiceReadyEvent()
    {
        var order = CreateOrder();

        order.ChangeStatus(ServiceOrderStatus.ReadyForPickup);

        order.DomainEvents.Should().ContainSingle(e => e is ServiceReadyEvent);
        var evt = (ServiceReadyEvent)order.DomainEvents.First();
        evt.ServiceOrderId.Should().Be(order.Id);
        evt.OrderNumber.Should().Be(1);
        evt.CompanyId.Should().Be(CompanyId);
        evt.CustomerId.Should().Be(CustomerId);
    }

    [Fact]
    public void ChangeStatus_ToOtherStatus_DoesNotRaiseServiceReadyEvent()
    {
        var order = CreateOrder();

        order.ChangeStatus(ServiceOrderStatus.UnderRepair);

        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void CreateBudget_WithValidBudget_SetsBudgetAndStatus()
    {
        var order = CreateOrder();
        var budget = Budget.Create(350m, "Troca de tela").Value!;

        var result = order.CreateBudget(budget);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(ServiceOrderStatus.WaitingApproval);
        order.Budget.Should().NotBeNull();
        order.Budget!.Value.Should().Be(350m);
    }

    [Fact]
    public void CreateBudget_RaisesBudgetCreatedEvent()
    {
        var order = CreateOrder();
        var budget = Budget.Create(350m, "Troca de tela").Value!;

        order.CreateBudget(budget);

        order.DomainEvents.Should().ContainSingle(e => e is BudgetCreatedEvent);
        var evt = (BudgetCreatedEvent)order.DomainEvents.First();
        evt.OrderNumber.Should().Be(1);
        evt.BudgetValue.Should().Be(350m);
    }

    [Fact]
    public void ApproveBudget_WhenWaitingApproval_ApprovesAndRaisesEvent()
    {
        var order = CreateOrder();
        order.CreateBudget(Budget.Create(350m, "Troca de tela").Value!);
        order.ClearDomainEvents();

        var result = order.ApproveBudget();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(ServiceOrderStatus.BudgetApproved);
        order.DomainEvents.Should().ContainSingle(e => e is BudgetRespondedEvent);
        var evt = (BudgetRespondedEvent)order.DomainEvents.First();
        evt.Approved.Should().BeTrue();
        evt.OrderNumber.Should().Be(1);
    }

    [Fact]
    public void ApproveBudget_WhenNotWaitingApproval_ReturnsFailure()
    {
        var order = CreateOrder();

        var result = order.ApproveBudget();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("A ordem não está aguardando aprovação");
    }

    [Fact]
    public void RefuseBudget_WhenWaitingApproval_RefusesAndRaisesEvent()
    {
        var order = CreateOrder();
        order.CreateBudget(Budget.Create(350m, "Troca de tela").Value!);
        order.ClearDomainEvents();

        var result = order.RefuseBudget();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(ServiceOrderStatus.BudgetRefused);
        order.DomainEvents.Should().ContainSingle(e => e is BudgetRespondedEvent);
        var evt = (BudgetRespondedEvent)order.DomainEvents.First();
        evt.Approved.Should().BeFalse();
    }

    [Fact]
    public void RefuseBudget_WhenNotWaitingApproval_ReturnsFailure()
    {
        var order = CreateOrder();

        var result = order.RefuseBudget();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain("A ordem não está aguardando aprovação");
    }

    [Fact]
    public void UpdateEquipment_ReplacesEquipment()
    {
        var order = CreateOrder();
        var newEquipment = EquipmentInfo.Create("Notebook", "Dell", "XPS 15", "Não liga").Value!;

        order.UpdateEquipment(newEquipment);

        order.Equipment.DeviceType.Should().Be("Notebook");
        order.Equipment.Brand.Should().Be("Dell");
    }

    [Fact]
    public void UpdateTechnician_ReplacesTechnicianName()
    {
        var order = CreateOrder("João");

        order.UpdateTechnician("Maria");

        order.TechnicianName.Should().Be("Maria");
    }
}

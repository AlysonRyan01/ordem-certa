using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Domain.ServiceOrders.ValueObjects;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.ServiceOrders;

public class ServiceOrder : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public EquipmentInfo Equipment { get; private set; } = null!;
    public ServiceOrderStatus Status { get; private set; }
    public DateTime EntryDate { get; private set; }
    public string? TechnicianName { get; private set; }

    protected ServiceOrder() { }

    private ServiceOrder(Guid companyId, Guid customerId, EquipmentInfo equipment, string? technicianName)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CustomerId = customerId;
        Equipment = equipment;
        Status = ServiceOrderStatus.Received;
        EntryDate = DateTime.UtcNow;
        TechnicianName = technicianName;
    }

    public static Result<ServiceOrder> Create(Guid companyId, Guid customerId, EquipmentInfo equipment, string? technicianName)
    {
        return new ServiceOrder(companyId, customerId, equipment, technicianName);
    }

    public Result UpdateEquipment(EquipmentInfo equipment)
    {
        Equipment = equipment;
        return Result.Success();
    }

    public Result UpdateTechnician(string? technicianName)
    {
        TechnicianName = technicianName;
        return Result.Success();
    }

    public Result ChangeStatus(ServiceOrderStatus status)
    {
        Status = status;
        return Result.Success();
    }
}

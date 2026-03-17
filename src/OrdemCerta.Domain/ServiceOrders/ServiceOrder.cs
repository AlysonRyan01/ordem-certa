using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Domain.ServiceOrders.ValueObjects;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.ServiceOrders;

public class ServiceOrder : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int OrderNumber { get; private set; }
    public EquipmentInfo Equipment { get; private set; } = null!;
    public ServiceOrderStatus Status { get; private set; }
    public RepairResult? RepairResult { get; private set; }
    public Warranty? Warranty { get; private set; }
    public DateTime EntryDate { get; private set; }
    public string? TechnicianName { get; private set; }
    public Budget? Budget { get; private set; }

    protected ServiceOrder() { }

    private ServiceOrder(Guid companyId, Guid customerId, int orderNumber, EquipmentInfo equipment, string? technicianName)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CustomerId = customerId;
        OrderNumber = orderNumber;
        Equipment = equipment;
        Status = ServiceOrderStatus.Received;
        EntryDate = DateTime.UtcNow;
        TechnicianName = technicianName;
    }

    public static Result<ServiceOrder> Create(Guid companyId, Guid customerId, int orderNumber, EquipmentInfo equipment, string? technicianName)
    {
        return new ServiceOrder(companyId, customerId, orderNumber, equipment, technicianName);
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

    public Result SetWarranty(Warranty warranty)
    {
        Warranty = warranty;
        return Result.Success();
    }

    public Result SetRepairResult(RepairResult result)
    {
        if (Status == ServiceOrderStatus.Delivered || Status == ServiceOrderStatus.Cancelled)
            return "Não é possível registrar o resultado de uma ordem finalizada.";

        RepairResult = result;
        return Result.Success();
    }

    public Result CreateBudget(Budget budget, RepairResult? repairResult = null, Warranty? warranty = null)
    {
        Budget = budget;
        Status = ServiceOrderStatus.WaitingApproval;
        if (repairResult.HasValue) RepairResult = repairResult;
        if (warranty is not null) Warranty = warranty;
        return Result.Success();
    }

    public Result UpdateBudget(Budget budget, RepairResult? repairResult = null, Warranty? warranty = null)
    {
        if (Status == ServiceOrderStatus.BudgetApproved)
            return "Não é possível alterar o orçamento após aprovação.";

        if (Status == ServiceOrderStatus.Delivered || Status == ServiceOrderStatus.Cancelled)
            return "Não é possível alterar o orçamento de uma ordem finalizada.";

        Budget = budget;
        Status = ServiceOrderStatus.WaitingApproval;
        if (repairResult.HasValue) RepairResult = repairResult;
        if (warranty is not null) Warranty = warranty;
        return Result.Success();
    }

    public Result ApproveBudget()
    {
        if (Status != ServiceOrderStatus.WaitingApproval)
            return "A ordem não está aguardando aprovação";

        Status = ServiceOrderStatus.BudgetApproved;
        return Result.Success();
    }

    public Result RefuseBudget()
    {
        if (Status != ServiceOrderStatus.WaitingApproval)
            return "A ordem não está aguardando aprovação";

        Status = ServiceOrderStatus.BudgetRefused;
        return Result.Success();
    }
}

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
    public ServiceOrderRepairStatus? BudgetStatus { get; private set; }
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
        Status = ServiceOrderStatus.UnderAnalysis;
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

    public Result CreateBudget(Budget budget, RepairResult repairResult, Warranty? warranty = null)
    {
        Budget = budget;
        RepairResult = repairResult;

        if (repairResult is Enums.RepairResult.NoFix or Enums.RepairResult.NoDefectFound)
        {
            Status = ServiceOrderStatus.ReadyForPickup;
            BudgetStatus = null;
        }
        else
        {
            Status = ServiceOrderStatus.AwaitingApproval;
            BudgetStatus = ServiceOrderRepairStatus.Entered;
            if (warranty is not null) Warranty = warranty;
        }

        return Result.Success();
    }

    public Result UpdateBudget(Budget budget, RepairResult repairResult, Warranty? warranty = null)
    {
        if (BudgetStatus == ServiceOrderRepairStatus.Approved)
            return "Não é possível alterar o orçamento após aprovação.";

        if (Status == ServiceOrderStatus.Delivered || Status == ServiceOrderStatus.Cancelled)
            return "Não é possível alterar o orçamento de uma ordem finalizada.";

        Budget = budget;
        RepairResult = repairResult;

        if (repairResult is Enums.RepairResult.NoFix or Enums.RepairResult.NoDefectFound)
        {
            Status = ServiceOrderStatus.ReadyForPickup;
            BudgetStatus = null;
            Warranty = null;
        }
        else
        {
            BudgetStatus = ServiceOrderRepairStatus.Entered;
            if (warranty is not null) Warranty = warranty;
        }

        return Result.Success();
    }

    public Result MarkBudgetAsWaiting()
    {
        if (Budget is null)
            return "A ordem não possui orçamento.";

        if (BudgetStatus is ServiceOrderRepairStatus.Approved or ServiceOrderRepairStatus.Disapproved)
            return "O orçamento já foi respondido pelo cliente.";

        BudgetStatus = ServiceOrderRepairStatus.Waiting;
        return Result.Success();
    }

    public Result ApproveBudget()
    {
        if (BudgetStatus is null or ServiceOrderRepairStatus.Approved or ServiceOrderRepairStatus.Disapproved)
            return "O orçamento não está disponível para aprovação.";

        BudgetStatus = ServiceOrderRepairStatus.Approved;
        Status = ServiceOrderStatus.BudgetApproved;
        return Result.Success();
    }

    public Result RefuseBudget()
    {
        if (BudgetStatus is null or ServiceOrderRepairStatus.Approved or ServiceOrderRepairStatus.Disapproved)
            return "O orçamento não está disponível para recusa.";

        BudgetStatus = ServiceOrderRepairStatus.Disapproved;
        Status = ServiceOrderStatus.BudgetRefused;
        return Result.Success();
    }

    public Result Rollback()
    {
        switch (Status)
        {
            case ServiceOrderStatus.AwaitingApproval:
                Status = ServiceOrderStatus.UnderAnalysis;
                Budget = null;
                BudgetStatus = null;
                RepairResult = null;
                Warranty = null;
                return Result.Success();

            case ServiceOrderStatus.BudgetApproved:
                Status = ServiceOrderStatus.AwaitingApproval;
                BudgetStatus = ServiceOrderRepairStatus.Waiting;
                return Result.Success();

            case ServiceOrderStatus.BudgetRefused:
                Status = ServiceOrderStatus.AwaitingApproval;
                BudgetStatus = ServiceOrderRepairStatus.Waiting;
                return Result.Success();

            case ServiceOrderStatus.UnderRepair:
                Status = ServiceOrderStatus.BudgetApproved;
                return Result.Success();

            case ServiceOrderStatus.ReadyForPickup:
                if (BudgetStatus == ServiceOrderRepairStatus.Disapproved)
                {
                    Status = ServiceOrderStatus.BudgetRefused;
                }
                else
                {
                    Status = RepairResult is Enums.RepairResult.NoFix or Enums.RepairResult.NoDefectFound
                        ? ServiceOrderStatus.AwaitingApproval
                        : ServiceOrderStatus.UnderRepair;
                }
                return Result.Success();

            default:
                return "Não é possível desfazer esta etapa.";
        }
    }
}

using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.ServiceOrders;

public class ServiceOrder : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int OrderNumber { get; private set; }
    public string DeviceType { get; private set; } = null!;
    public string Brand { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public string ReportedDefect { get; private set; } = null!;
    public string? Accessories { get; private set; }
    public string? Observations { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public ServiceOrderRepairStatus? BudgetStatus { get; private set; }
    public RepairResult? RepairResult { get; private set; }
    public int? WarrantyDuration { get; private set; }
    public WarrantyUnit? WarrantyUnit { get; private set; }
    public DateTime EntryDate { get; private set; }
    public string? TechnicianName { get; private set; }
    public decimal? BudgetValue { get; private set; }
    public string? BudgetDescription { get; private set; }

    protected ServiceOrder() { }

    private ServiceOrder(
        Guid companyId,
        Guid customerId,
        int orderNumber,
        string deviceType,
        string brand,
        string model,
        string reportedDefect,
        string? accessories,
        string? observations,
        string? technicianName)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CustomerId = customerId;
        OrderNumber = orderNumber;
        DeviceType = deviceType;
        Brand = brand;
        Model = model;
        ReportedDefect = reportedDefect;
        Accessories = accessories;
        Observations = observations;
        Status = ServiceOrderStatus.UnderAnalysis;
        EntryDate = DateTime.UtcNow;
        TechnicianName = technicianName;
    }

    public static Result<ServiceOrder> Create(
        Guid companyId,
        Guid customerId,
        int orderNumber,
        string deviceType,
        string brand,
        string model,
        string reportedDefect,
        string? accessories,
        string? observations,
        string? technicianName)
    {
        return new ServiceOrder(companyId, customerId, orderNumber, deviceType, brand, model, reportedDefect, accessories, observations, technicianName);
    }

    public Result UpdateEquipment(
        string deviceType,
        string brand,
        string model,
        string reportedDefect,
        string? accessories,
        string? observations)
    {
        DeviceType = deviceType;
        Brand = brand;
        Model = model;
        ReportedDefect = reportedDefect;
        Accessories = accessories;
        Observations = observations;
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

    public Result SetWarranty(int duration, WarrantyUnit unit)
    {
        WarrantyDuration = duration;
        WarrantyUnit = unit;
        return Result.Success();
    }

    public Result SetRepairResult(RepairResult result)
    {
        if (Status == ServiceOrderStatus.Delivered || Status == ServiceOrderStatus.Cancelled)
            return "Não é possível registrar o resultado de uma ordem finalizada.";

        RepairResult = result;
        return Result.Success();
    }

    public Result CreateBudget(decimal value, string description, RepairResult repairResult, int? warrantyDuration = null, WarrantyUnit? warrantyUnit = null)
    {
        BudgetValue = value;
        BudgetDescription = description;
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
            if (warrantyDuration.HasValue && warrantyUnit.HasValue)
            {
                WarrantyDuration = warrantyDuration;
                WarrantyUnit = warrantyUnit;
            }
        }

        return Result.Success();
    }

    public Result UpdateBudget(decimal value, string description, RepairResult repairResult, int? warrantyDuration = null, WarrantyUnit? warrantyUnit = null)
    {
        if (BudgetStatus == ServiceOrderRepairStatus.Approved)
            return "Não é possível alterar o orçamento após aprovação.";

        if (Status == ServiceOrderStatus.Delivered || Status == ServiceOrderStatus.Cancelled)
            return "Não é possível alterar o orçamento de uma ordem finalizada.";

        BudgetValue = value;
        BudgetDescription = description;
        RepairResult = repairResult;

        if (repairResult is Enums.RepairResult.NoFix or Enums.RepairResult.NoDefectFound)
        {
            Status = ServiceOrderStatus.ReadyForPickup;
            BudgetStatus = null;
            WarrantyDuration = null;
            WarrantyUnit = null;
        }
        else
        {
            BudgetStatus = ServiceOrderRepairStatus.Entered;
            if (warrantyDuration.HasValue && warrantyUnit.HasValue)
            {
                WarrantyDuration = warrantyDuration;
                WarrantyUnit = warrantyUnit;
            }
        }

        return Result.Success();
    }

    public Result MarkBudgetAsWaiting()
    {
        if (BudgetValue is null)
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
                BudgetValue = null;
                BudgetDescription = null;
                BudgetStatus = null;
                RepairResult = null;
                WarrantyDuration = null;
                WarrantyUnit = null;
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

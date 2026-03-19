using OrdemCerta.Domain.ServiceOrders.Enums;

namespace OrdemCerta.Application.Inputs.ServiceOrderInputs;

public record CreateBudgetInput(
    decimal Value,
    string Description,
    RepairResult RepairResult,
    int? WarrantyDuration = null,
    WarrantyUnit? WarrantyUnit = null);

using OrdemCerta.Domain.ServiceOrders.Enums;

namespace OrdemCerta.Application.Inputs.ServiceOrderInputs;

public record CreateBudgetInput(
    decimal Value,
    string Description,
    RepairResult? RepairResult = null,
    int? WarrantyDuration = null,
    WarrantyUnit? WarrantyUnit = null);

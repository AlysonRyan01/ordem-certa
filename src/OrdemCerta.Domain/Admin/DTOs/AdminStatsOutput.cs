namespace OrdemCerta.Domain.Admin.DTOs;

public record AdminStatsOutput(
    int TotalCompanies,
    int PaidCompanies,
    int DemoCompanies,
    int NewLast30Days,
    int TotalServiceOrders);

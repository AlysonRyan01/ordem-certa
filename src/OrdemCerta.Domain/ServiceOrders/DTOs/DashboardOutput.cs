namespace OrdemCerta.Domain.ServiceOrders.DTOs;

public record StatusCountOutput(string Status, int Count);

public record DashboardOutput(
    int OpenOrders,
    int ReadyForPickup,
    int WaitingApproval,
    int TotalOrders,
    string Plan,
    List<ServiceOrderOutput> RecentOrders,
    List<StatusCountOutput> OrdersByStatus,
    List<StatusCountOutput> AllOrdersByStatus);

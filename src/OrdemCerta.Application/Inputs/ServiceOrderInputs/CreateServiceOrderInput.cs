namespace OrdemCerta.Application.Inputs.ServiceOrderInputs;

public record CreateServiceOrderInput(
    Guid CustomerId,
    string DeviceType,
    string Brand,
    string Model,
    string ReportedDefect,
    string? Accessories,
    string? Observations,
    string? TechnicianName);

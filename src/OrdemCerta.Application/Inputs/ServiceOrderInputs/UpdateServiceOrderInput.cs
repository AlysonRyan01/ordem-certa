namespace OrdemCerta.Application.Inputs.ServiceOrderInputs;

public record UpdateServiceOrderInput(
    string DeviceType,
    string Brand,
    string Model,
    string ReportedDefect,
    string? Accessories,
    string? Observations,
    string? TechnicianName);

using OrdemCerta.Shared;

namespace OrdemCerta.Domain.ServiceOrders.ValueObjects;

public class EquipmentInfo : ValueObject
{
    public string DeviceType { get; private set; }
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public string ReportedDefect { get; private set; }
    public string? Accessories { get; private set; }
    public string? Observations { get; private set; }

    private EquipmentInfo(string deviceType, string brand, string model, string reportedDefect, string? accessories, string? observations)
    {
        DeviceType = deviceType;
        Brand = brand;
        Model = model;
        ReportedDefect = reportedDefect;
        Accessories = accessories;
        Observations = observations;
    }

    public static Result<EquipmentInfo> Create(string deviceType, string brand, string model, string reportedDefect, string? accessories = null, string? observations = null)
    {
        if (string.IsNullOrWhiteSpace(deviceType))
            return "O tipo de aparelho é obrigatório";
        if (deviceType.Length > 100)
            return "O tipo de aparelho deve ter no máximo 100 caracteres";

        if (string.IsNullOrWhiteSpace(brand))
            return "A marca é obrigatória";
        if (brand.Length > 100)
            return "A marca deve ter no máximo 100 caracteres";

        if (string.IsNullOrWhiteSpace(model))
            return "O modelo é obrigatório";
        if (model.Length > 100)
            return "O modelo deve ter no máximo 100 caracteres";

        if (string.IsNullOrWhiteSpace(reportedDefect))
            return "O defeito relatado é obrigatório";
        if (reportedDefect.Length > 500)
            return "O defeito relatado deve ter no máximo 500 caracteres";

        return new EquipmentInfo(
            deviceType.Trim(),
            brand.Trim(),
            model.Trim(),
            reportedDefect.Trim(),
            accessories?.Trim(),
            observations?.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DeviceType;
        yield return Brand;
        yield return Model;
        yield return ReportedDefect;
        yield return Accessories ?? string.Empty;
        yield return Observations ?? string.Empty;
    }
}

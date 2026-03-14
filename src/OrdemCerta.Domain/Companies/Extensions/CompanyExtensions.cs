using OrdemCerta.Domain.Companies.DTOs;

namespace OrdemCerta.Domain.Companies.Extensions;

public static class CompanyExtensions
{
    public static CompanyOutput ToOutput(this Company company)
    {
        return new CompanyOutput
        {
            Id = company.Id,
            Name = company.Name.Value,
            Cnpj = company.Cnpj?.Value,
            CnpjFormatted = company.Cnpj?.GetFormatted(),
            Phone = company.Phone.Value,
            PhoneFormatted = company.Phone.GetFormatted(),
            Street = company.Street,
            Number = company.Number,
            City = company.City,
            State = company.State,
            Plan = company.Plan.ToString()
        };
    }

    public static List<CompanyOutput> ToOutput(this IEnumerable<Company> companies)
    {
        return companies.Select(c => c.ToOutput()).ToList();
    }
}

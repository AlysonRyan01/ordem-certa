using OrdemCerta.Domain.Companies.DTOs;

namespace OrdemCerta.Domain.Companies.Extensions;

public static class CompanyExtensions
{
    public static CompanyOutput ToOutput(this Company company)
    {
        return new CompanyOutput
        {
            Id = company.Id,
            Name = company.Name,
            Email = company.Email,
            Cnpj = company.Cnpj,
            CnpjFormatted = company.GetCnpjFormatted(),
            Phone = company.Phone,
            PhoneFormatted = company.GetPhoneFormatted(),
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

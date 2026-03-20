using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Domain.Customers.DTOs;
using OrdemCerta.Domain.Sales.DTOs;
using OrdemCerta.Domain.ServiceOrders.DTOs;

namespace OrdemCerta.Application.Services.PdfService;

public interface IPdfService
{
    byte[] GenerateEntryReceipt(ServiceOrderOutput order, CustomerOutput customer, CompanyOutput company);
    byte[] GenerateWarrantyCard(ServiceOrderOutput order, CustomerOutput customer, CompanyOutput company);
    byte[] GenerateReturnReceipt(ServiceOrderOutput order, CustomerOutput customer, CompanyOutput company);
    byte[] GenerateSaleReceipt(SaleOutput sale, CompanyOutput company, string? customerName, string? customerPhone, string? customerDocument);
    byte[] GenerateSaleWarranty(SaleOutput sale, CompanyOutput company, string? customerName, string? customerPhone, string? customerDocument);
}

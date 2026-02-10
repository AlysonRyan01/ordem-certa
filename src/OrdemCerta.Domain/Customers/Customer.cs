using OrdemCerta.Domain.Customers.ValueObjects;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Customers;

public class Customer : AggregateRoot
{
    public CustomerName Name { get; private set; } = null!;
    public List<CustomerPhone> Phones { get; private set; } = new();
    public CustomerAddress? Address { get; private set; }
    public CustomerDocument? Document { get; private set; }

    private Customer() { }

    private Customer(
        CustomerName name,
        List<CustomerPhone>? phones = null,
        CustomerAddress? address = null,
        CustomerDocument? document = null)
    {
        Name = name;
        Phones = phones ?? new List<CustomerPhone>();
        Address = address;
        Document = document;
    }

    public static Result<Customer> Create(
        CustomerName name,
        List<CustomerPhone>? phones = null,
        CustomerAddress? address = null,
        CustomerDocument? document = null)
    {
        var customer = new Customer(name, phones, address, document);
        return customer;
    }

    public Result AddPhone(CustomerPhone phone)
    {
        if (Phones.Any(p => p == phone))
            return Result.Failure("Este telefone já está cadastrado");

        Phones.Add(phone);
        return Result.Success();
    }

    public Result RemovePhone(CustomerPhone phone)
    {
        if (!Phones.Remove(phone))
            return Result.Failure("Telefone não encontrado");

        return Result.Success();
    }

    public Result UpdateAddress(CustomerAddress? address)
    {
        Address = address;
        return Result.Success();
    }

    public Result UpdateDocument(CustomerDocument? document)
    {
        Document = document;
        return Result.Success();
    }

    public Result UpdateName(CustomerName name)
    {
        Name = name;
        return Result.Success();
    }
}
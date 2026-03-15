namespace OrdemCerta.Domain.ServiceOrders;

public class CompanyOrderSequence
{
    public Guid CompanyId { get; private set; }
    public int LastNumber { get; private set; }

    protected CompanyOrderSequence() { }

    public static CompanyOrderSequence Create(Guid companyId)
    {
        return new CompanyOrderSequence { CompanyId = companyId, LastNumber = 0 };
    }

    public int Increment()
    {
        LastNumber++;
        return LastNumber;
    }
}

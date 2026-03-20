namespace OrdemCerta.Domain.Sales;

public class CompanySaleSequence
{
    public Guid CompanyId { get; private set; }
    public int LastNumber { get; private set; }

    protected CompanySaleSequence() { }

    public static CompanySaleSequence Create(Guid companyId)
    {
        return new CompanySaleSequence { CompanyId = companyId, LastNumber = 0 };
    }

    public int Increment()
    {
        LastNumber++;
        return LastNumber;
    }
}

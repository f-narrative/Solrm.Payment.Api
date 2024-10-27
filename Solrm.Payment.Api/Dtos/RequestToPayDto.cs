namespace Solrm.Payment.Api.Dtos;

public class RequestToPayDto
{
    public string Amount { get; set; }

    public string Currency { get; set; }

    public string ExternalId { get; set; }

    public Guid ReferenceId { get; set; }

    public PayerDto Payer { get; set; }
}
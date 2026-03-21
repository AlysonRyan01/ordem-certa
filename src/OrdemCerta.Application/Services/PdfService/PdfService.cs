using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Domain.Customers.DTOs;
using OrdemCerta.Domain.Sales.DTOs;
using OrdemCerta.Domain.Sales.Enums;
using OrdemCerta.Domain.ServiceOrders.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrdemCerta.Application.Services.PdfService;

public class PdfService : IPdfService
{
    // ── Brand tokens ───────────────────────────────────────────────────────────
    private const string Navy        = "#0F1629";
    private const string NavyMid     = "#1E2D47";
    private const string Amber       = "#F59E0B";
    private const string AmberLight  = "#FEF3C7";
    private const string Green       = "#059669";
    private const string GreenLight  = "#ECFDF5";
    private const string Orange      = "#EA580C";
    private const string OrangeLight = "#FFF7ED";
    private const string TextDark    = "#1E293B";
    private const string TextMuted   = "#64748B";
    private const string BgLight     = "#F8FAFC";
    private const string Border      = "#E2E8F0";
    private const string White       = "#FFFFFF";
    private const string White60     = "#FFFFFF99";
    private const string White30     = "#FFFFFF4D";

    // ══════════════════════════════════════════════════════════════════════════
    // Entry Receipt
    // ══════════════════════════════════════════════════════════════════════════
    public byte[] GenerateEntryReceipt(ServiceOrderOutput order, CustomerOutput customer, CompanyOutput company)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextDark));

                page.Content().Column(col =>
                {
                    Header(col, company, order, "COMPROVANTE DE ENTRADA", order.EntryDate.ToString("dd/MM/yyyy"), Amber);

                    col.Item().Padding(32).Column(body =>
                    {
                        // Cliente
                        SectionLabel(body, "DADOS DO CLIENTE");
                        body.Item().PaddingTop(6).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1); });
                            DataCell(t, "Nome", customer.FullName);
                            DataCell(t, "Documento", customer.Document?.Formatted ?? "—");
                            DataCell(t, "Telefone", customer.PhoneFormatted);
                        });

                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "EQUIPAMENTO");
                            sec.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                                DataCell(t, "Tipo", order.DeviceType);
                                DataCell(t, "Marca", order.Brand);
                                DataCell(t, "Modelo", order.Model);
                            });

                            if (!string.IsNullOrEmpty(order.TechnicianName))
                                sec.Item().PaddingTop(14).Column(c =>
                                {
                                    c.Item().Text("Técnico responsável").FontSize(8).FontColor(TextMuted);
                                    c.Item().PaddingTop(2).Text(order.TechnicianName).Bold();
                                });

                            sec.Item().PaddingTop(14).Column(c =>
                            {
                                c.Item().Text("Defeito relatado").FontSize(8).FontColor(TextMuted);
                                c.Item().PaddingTop(4)
                                    .BorderLeft(3).BorderColor(Amber)
                                    .Background(BgLight)
                                    .Padding(10)
                                    .Text(order.ReportedDefect).FontSize(10);
                            });

                            if (!string.IsNullOrEmpty(order.Accessories))
                                sec.Item().PaddingTop(12).Column(c =>
                                {
                                    c.Item().Text("Acessórios").FontSize(8).FontColor(TextMuted);
                                    c.Item().PaddingTop(2).Text(order.Accessories);
                                });

                            if (!string.IsNullOrEmpty(order.Observations))
                                sec.Item().PaddingTop(12).Column(c =>
                                {
                                    c.Item().Text("Observações").FontSize(8).FontColor(TextMuted);
                                    c.Item().PaddingTop(2).Text(order.Observations);
                                });
                        });

                        Signatures(body);
                    });
                });

                Footer(page, order);
            });
        }).GeneratePdf();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Warranty Card
    // ══════════════════════════════════════════════════════════════════════════
    public byte[] GenerateWarrantyCard(ServiceOrderOutput order, CustomerOutput customer, CompanyOutput company)
    {
        var warrantyText = order.WarrantyDuration.HasValue && !string.IsNullOrEmpty(order.WarrantyUnit)
            ? FormatWarranty(order.WarrantyDuration.Value, order.WarrantyUnit)
            : "—";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextDark));

                page.Content().Column(col =>
                {
                    Header(col, company, order, "CERTIFICADO DE GARANTIA", DateTime.Today.ToString("dd/MM/yyyy"), Green);

                    col.Item().Padding(32).Column(body =>
                    {
                        // Warranty highlight
                        body.Item().Background(GreenLight)
                            .BorderLeft(4).BorderColor(Green)
                            .Padding(16).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("PRAZO DE GARANTIA").Bold().FontSize(8).FontColor(Green);
                                    c.Item().PaddingTop(4).Text(warrantyText).Bold().FontSize(28).FontColor(Green);
                                });
                                row.AutoItem().AlignMiddle().AlignRight().Column(c =>
                                {
                                    c.Item().Text("Emitido em").FontSize(8).FontColor(TextMuted);
                                    c.Item().Text(DateTime.Today.ToString("dd/MM/yyyy")).Bold().FontSize(11).FontColor(TextDark);
                                });
                            });

                        // Cliente
                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "DADOS DO CLIENTE");
                            sec.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1); });
                                DataCell(t, "Nome", customer.FullName);
                                DataCell(t, "Documento", customer.Document?.Formatted ?? "—");
                                DataCell(t, "Telefone", customer.PhoneFormatted);
                            });
                        });

                        // Equipamento
                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "EQUIPAMENTO");
                            sec.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                                DataCell(t, "Tipo", order.DeviceType);
                                DataCell(t, "Marca", order.Brand);
                                DataCell(t, "Modelo", order.Model);
                            });
                        });

                        // Serviço
                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "SERVIÇO REALIZADO");
                            sec.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });
                                DataCell(t, "Descrição", order.BudgetDescription ?? "—");
                                DataCell(t, "Valor", $"R$ {order.BudgetValue:N2}");
                            });
                        });

                        // Termos
                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "TERMOS DA GARANTIA");
                            sec.Item().PaddingTop(8).Background(BgLight).Padding(12)
                                .Text($"A garantia de {warrantyText} cobre exclusivamente o defeito descrito acima e o serviço realizado. " +
                                      "Não cobre danos físicos, líquidos, mau uso ou defeitos não relacionados ao serviço executado. " +
                                      "Em caso de dúvidas, entre em contato com a empresa.")
                                .FontSize(9).FontColor(TextMuted);
                        });

                        Signatures(body);
                    });
                });

                Footer(page, order);
            });
        }).GeneratePdf();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Return Receipt
    // ══════════════════════════════════════════════════════════════════════════
    public byte[] GenerateReturnReceipt(ServiceOrderOutput order, CustomerOutput customer, CompanyOutput company)
    {
        var reason = order.RepairResult switch
        {
            "NoFix"         => "Sem conserto — equipamento não pôde ser reparado após avaliação técnica.",
            "NoDefectFound" => "Nenhum defeito detectado — o equipamento foi avaliado e não apresentou falha reproduzível.",
            _               => "Devolução do equipamento.",
        };

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextDark));

                page.Content().Column(col =>
                {
                    Header(col, company, order, "COMPROVANTE DE DEVOLUÇÃO", DateTime.Today.ToString("dd/MM/yyyy"), Orange);

                    col.Item().Padding(32).Column(body =>
                    {
                        // Reason highlight
                        body.Item().Background(OrangeLight)
                            .BorderLeft(4).BorderColor(Orange)
                            .Padding(16).Column(c =>
                            {
                                c.Item().Text("MOTIVO DA DEVOLUÇÃO").Bold().FontSize(8).FontColor(Orange);
                                c.Item().PaddingTop(6).Text(reason).FontSize(11).FontColor(TextDark).Bold();
                                c.Item().PaddingTop(6).Text("Este documento não possui cláusula de garantia.")
                                    .Italic().FontSize(9).FontColor(TextMuted);
                            });

                        // Cliente
                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "DADOS DO CLIENTE");
                            sec.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1); });
                                DataCell(t, "Nome", customer.FullName);
                                DataCell(t, "Documento", customer.Document?.Formatted ?? "—");
                                DataCell(t, "Telefone", customer.PhoneFormatted);
                            });
                        });

                        // Equipamento
                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "EQUIPAMENTO");
                            sec.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                                DataCell(t, "Tipo", order.DeviceType);
                                DataCell(t, "Marca", order.Brand);
                                DataCell(t, "Modelo", order.Model);
                            });

                            if (!string.IsNullOrEmpty(order.Observations))
                                sec.Item().PaddingTop(12).Column(c =>
                                {
                                    c.Item().Text("Observações").FontSize(8).FontColor(TextMuted);
                                    c.Item().PaddingTop(2).Text(order.Observations);
                                });
                        });

                        Signatures(body);
                    });
                });

                Footer(page, order);
            });
        }).GeneratePdf();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Sale Receipt
    // ══════════════════════════════════════════════════════════════════════════
    public byte[] GenerateSaleReceipt(SaleOutput sale, CompanyOutput company, string? customerName, string? customerPhone, string? customerDocument)
    {
        var paymentLabel = sale.PaymentMethod switch
        {
            SalePaymentMethod.Cash         => "Dinheiro",
            SalePaymentMethod.CreditCard   => "Cartão de Crédito",
            SalePaymentMethod.DebitCard    => "Cartão de Débito",
            SalePaymentMethod.Pix          => "Pix",
            SalePaymentMethod.BankTransfer => "Transferência Bancária",
            SalePaymentMethod.Check        => "Cheque",
            _                              => "Outro"
        };

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextDark));

                page.Content().Column(col =>
                {
                    SaleHeader(col, company, sale, "COMPROVANTE DE VENDA", sale.SaleDate.ToString("dd/MM/yyyy"), Amber);

                    col.Item().Padding(32).Column(body =>
                    {
                        if (!string.IsNullOrEmpty(customerName))
                        {
                            SectionLabel(body, "DADOS DO CLIENTE");
                            body.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1); });
                                DataCell(t, "Nome", customerName);
                                DataCell(t, "Documento", customerDocument ?? "—");
                                DataCell(t, "Telefone", customerPhone ?? "—");
                            });
                            body.Item().PaddingTop(24);
                        }

                        SectionLabel(body, "ITENS DA VENDA");
                        body.Item().PaddingTop(6).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.ConstantColumn(60);
                                c.ConstantColumn(80);
                                c.ConstantColumn(80);
                            });

                            t.Header(h =>
                            {
                                h.Cell().Background(BgLight).PaddingVertical(6).PaddingHorizontal(4).Text("Descrição").Bold().FontSize(8).FontColor(TextMuted);
                                h.Cell().Background(BgLight).PaddingVertical(6).PaddingHorizontal(4).AlignCenter().Text("Qtd").Bold().FontSize(8).FontColor(TextMuted);
                                h.Cell().Background(BgLight).PaddingVertical(6).PaddingHorizontal(4).AlignRight().Text("Unitário").Bold().FontSize(8).FontColor(TextMuted);
                                h.Cell().Background(BgLight).PaddingVertical(6).PaddingHorizontal(4).AlignRight().Text("Total").Bold().FontSize(8).FontColor(TextMuted);
                            });

                            foreach (var item in sale.Items)
                            {
                                t.Cell().BorderBottom(0.5f).BorderColor(Border).PaddingVertical(8).PaddingHorizontal(4).Text(item.Description).FontSize(10);
                                t.Cell().BorderBottom(0.5f).BorderColor(Border).PaddingVertical(8).PaddingHorizontal(4).AlignCenter().Text(item.Quantity.ToString()).FontSize(10);
                                t.Cell().BorderBottom(0.5f).BorderColor(Border).PaddingVertical(8).PaddingHorizontal(4).AlignRight().Text($"R$ {item.UnitPrice:N2}").FontSize(10);
                                t.Cell().BorderBottom(0.5f).BorderColor(Border).PaddingVertical(8).PaddingHorizontal(4).AlignRight().Text($"R$ {item.TotalPrice:N2}").FontSize(10);
                            }
                        });

                        body.Item().PaddingTop(12).AlignRight().Row(row =>
                        {
                            row.AutoItem().Column(c =>
                            {
                                c.Item().Text("Forma de pagamento").FontSize(8).FontColor(TextMuted);
                                c.Item().PaddingTop(2).Text(paymentLabel).Bold();
                            });
                            row.ConstantItem(32);
                            row.AutoItem().Column(c =>
                            {
                                c.Item().Text("TOTAL").FontSize(8).FontColor(TextMuted);
                                c.Item().PaddingTop(2).Text($"R$ {sale.TotalValue:N2}").Bold().FontSize(16).FontColor(Amber);
                            });
                        });

                        if (!string.IsNullOrEmpty(sale.Notes))
                        {
                            body.Item().PaddingTop(20).Column(sec =>
                            {
                                SectionLabel(sec, "OBSERVAÇÕES");
                                sec.Item().PaddingTop(6).Background(BgLight).Padding(10).Text(sale.Notes).FontSize(10);
                            });
                        }

                        Signatures(body);
                    });
                });

                SaleFooter(page, sale);
            });
        }).GeneratePdf();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Sale Warranty
    // ══════════════════════════════════════════════════════════════════════════
    public byte[] GenerateSaleWarranty(SaleOutput sale, CompanyOutput company, string? customerName, string? customerPhone, string? customerDocument)
    {
        var warrantyText = sale.WarrantyDuration.HasValue && !string.IsNullOrEmpty(sale.WarrantyUnit)
            ? FormatWarranty(sale.WarrantyDuration.Value, sale.WarrantyUnit)
            : "—";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextDark));

                page.Content().Column(col =>
                {
                    SaleHeader(col, company, sale, "CERTIFICADO DE GARANTIA", DateTime.Today.ToString("dd/MM/yyyy"), Green);

                    col.Item().Padding(32).Column(body =>
                    {
                        body.Item().Background(GreenLight)
                            .BorderLeft(4).BorderColor(Green)
                            .Padding(16).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("PRAZO DE GARANTIA").Bold().FontSize(8).FontColor(Green);
                                    c.Item().PaddingTop(4).Text(warrantyText).Bold().FontSize(28).FontColor(Green);
                                });
                                row.AutoItem().AlignMiddle().AlignRight().Column(c =>
                                {
                                    c.Item().Text("Emitido em").FontSize(8).FontColor(TextMuted);
                                    c.Item().Text(DateTime.Today.ToString("dd/MM/yyyy")).Bold().FontSize(11).FontColor(TextDark);
                                });
                            });

                        if (!string.IsNullOrEmpty(customerName))
                        {
                            body.Item().PaddingTop(24).Column(sec =>
                            {
                                SectionLabel(sec, "DADOS DO CLIENTE");
                                sec.Item().PaddingTop(6).Table(t =>
                                {
                                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1); });
                                    DataCell(t, "Nome", customerName);
                                    DataCell(t, "Documento", customerDocument ?? "—");
                                    DataCell(t, "Telefone", customerPhone ?? "—");
                                });
                            });
                        }

                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "ITENS COBERTOS");
                            sec.Item().PaddingTop(6).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(3); c.ConstantColumn(80); });
                                t.Header(h =>
                                {
                                    h.Cell().Background(BgLight).PaddingVertical(6).PaddingHorizontal(4).Text("Descrição").Bold().FontSize(8).FontColor(TextMuted);
                                    h.Cell().Background(BgLight).PaddingVertical(6).PaddingHorizontal(4).AlignRight().Text("Total").Bold().FontSize(8).FontColor(TextMuted);
                                });
                                foreach (var item in sale.Items)
                                {
                                    t.Cell().BorderBottom(0.5f).BorderColor(Border).PaddingVertical(8).PaddingHorizontal(4).Text(item.Description).FontSize(10);
                                    t.Cell().BorderBottom(0.5f).BorderColor(Border).PaddingVertical(8).PaddingHorizontal(4).AlignRight().Text($"R$ {item.TotalPrice:N2}").FontSize(10);
                                }
                            });
                        });

                        body.Item().PaddingTop(24).Column(sec =>
                        {
                            SectionLabel(sec, "TERMOS DA GARANTIA");
                            sec.Item().PaddingTop(8).Background(BgLight).Padding(12)
                                .Text($"A garantia de {warrantyText} cobre exclusivamente os itens descritos acima. " +
                                      "Não cobre danos físicos, mau uso ou defeitos não relacionados ao produto adquirido. " +
                                      "Em caso de dúvidas, entre em contato com a empresa.")
                                .FontSize(9).FontColor(TextMuted);
                        });

                        Signatures(body);
                    });
                });

                SaleFooter(page, sale);
            });
        }).GeneratePdf();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Shared helpers
    // ══════════════════════════════════════════════════════════════════════════

    private static void Header(
        ColumnDescriptor col,
        CompanyOutput company,
        ServiceOrderOutput order,
        string docType,
        string date,
        string accentColor)
    {
        col.Item().Background(Navy).PaddingHorizontal(32).PaddingVertical(24).Row(row =>
        {
            // Company info
            row.RelativeItem().Column(c =>
            {
                c.Item().Text(company.Name).Bold().FontSize(20).FontColor(White);
                if (!string.IsNullOrEmpty(company.Street))
                    c.Item().PaddingTop(4).Text($"{company.Street}, {company.Number}  ·  {company.City}/{company.State}")
                        .FontSize(9).FontColor(White60);
                c.Item().PaddingTop(2).Text(company.PhoneFormatted).FontSize(9).FontColor(White60);
            });

            // Doc type + order number
            row.AutoItem().AlignRight().Column(c =>
            {
                c.Item().AlignRight()
                    .Background(accentColor)
                    .PaddingHorizontal(10).PaddingVertical(4)
                    .Text(docType).Bold().FontSize(7.5f).FontColor(Navy);
                c.Item().AlignRight().PaddingTop(8)
                    .Text($"#{order.OrderNumber}").Bold().FontSize(24).FontColor(accentColor);
                c.Item().AlignRight().PaddingTop(2)
                    .Text(date).FontSize(9).FontColor(White30);
            });
        });

        // Thin amber separator line
        col.Item().Height(2).Background(accentColor);
    }

    private static void SectionLabel(ColumnDescriptor col, string title)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(3).Background(Amber);
            row.RelativeItem().PaddingLeft(8)
                .Text(title).Bold().FontSize(8).FontColor(TextMuted);
        });
        col.Item().Height(1).Background(Border);
    }

    private static void DataCell(TableDescriptor table, string label, string value)
    {
        table.Cell()
            .BorderBottom(0.5f).BorderColor(Border)
            .PaddingVertical(10).PaddingRight(8)
            .Column(c =>
            {
                c.Item().Text(label).FontSize(8).FontColor(TextMuted);
                c.Item().PaddingTop(2).Text(value).Bold().FontSize(10.5f).FontColor(TextDark);
            });
    }

    private static void Signatures(ColumnDescriptor col)
    {
        col.Item().PaddingTop(56).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().BorderBottom(0.5f).BorderColor(TextMuted).Height(28).Text(string.Empty);
                c.Item().PaddingTop(5).AlignCenter().Text("Assinatura do cliente").FontSize(8.5f).FontColor(TextMuted);
            });
            row.ConstantItem(52);
            row.RelativeItem().Column(c =>
            {
                c.Item().BorderBottom(0.5f).BorderColor(TextMuted).Height(28).Text(string.Empty);
                c.Item().PaddingTop(5).AlignCenter().Text("Assinatura do técnico").FontSize(8.5f).FontColor(TextMuted);
            });
        });
    }

    private static void Footer(PageDescriptor page, ServiceOrderOutput order)
    {
        page.Footer()
            .Background(Navy)
            .PaddingHorizontal(32).PaddingVertical(10)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text($"Documento gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(White30);
                row.AutoItem()
                    .Text($"OrdemCerta  ·  Ordem #{order.OrderNumber}")
                    .FontSize(8).FontColor(White60);
            });
    }

    private static string FormatWarranty(int duration, string unit) => unit switch
    {
        "Days"   => $"{duration} {(duration == 1 ? "dia" : "dias")}",
        "Months" => $"{duration} {(duration == 1 ? "mês" : "meses")}",
        "Years"  => $"{duration} {(duration == 1 ? "ano" : "anos")}",
        _        => $"{duration} {unit}",
    };

    private static void SaleHeader(
        ColumnDescriptor col,
        CompanyOutput company,
        SaleOutput sale,
        string docType,
        string date,
        string accentColor)
    {
        col.Item().Background(Navy).PaddingHorizontal(32).PaddingVertical(24).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text(company.Name).Bold().FontSize(20).FontColor(White);
                if (!string.IsNullOrEmpty(company.Street))
                    c.Item().PaddingTop(4).Text($"{company.Street}, {company.Number}  ·  {company.City}/{company.State}")
                        .FontSize(9).FontColor(White60);
                c.Item().PaddingTop(2).Text(company.PhoneFormatted).FontSize(9).FontColor(White60);
            });

            row.AutoItem().AlignRight().Column(c =>
            {
                c.Item().AlignRight()
                    .Background(accentColor)
                    .PaddingHorizontal(10).PaddingVertical(4)
                    .Text(docType).Bold().FontSize(7.5f).FontColor(Navy);
                c.Item().AlignRight().PaddingTop(8)
                    .Text($"#{sale.SaleNumber}").Bold().FontSize(24).FontColor(accentColor);
                c.Item().AlignRight().PaddingTop(2)
                    .Text(date).FontSize(9).FontColor(White30);
            });
        });

        col.Item().Height(2).Background(accentColor);
    }

    private static void SaleFooter(PageDescriptor page, SaleOutput sale)
    {
        page.Footer()
            .Background(Navy)
            .PaddingHorizontal(32).PaddingVertical(10)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text($"Documento gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(White30);
                row.AutoItem()
                    .Text($"OrdemCerta  ·  Venda #{sale.SaleNumber}")
                    .FontSize(8).FontColor(White60);
            });
    }
}

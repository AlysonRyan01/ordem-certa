using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Domain.Customers.DTOs;
using OrdemCerta.Domain.ServiceOrders.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrdemCerta.Application.Services.PdfService;

public class PdfService : IPdfService
{
    public byte[] GenerateEntryReceipt(ServiceOrderOutput order, CustomerOutput customer, CompanyOutput company)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(company.Name).Bold().FontSize(16);
                            if (!string.IsNullOrEmpty(company.Street))
                                c.Item().Text($"{company.Street}, {company.Number} — {company.City}/{company.State}").FontColor(Colors.Grey.Darken1);
                            c.Item().Text(company.PhoneFormatted).FontColor(Colors.Grey.Darken1);
                        });
                        row.ConstantItem(120).AlignRight().Column(c =>
                        {
                            c.Item().Text("COMPROVANTE DE ENTRADA").Bold().FontSize(11);
                            c.Item().Text($"Ordem #{order.OrderNumber}").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Data: {order.EntryDate:dd/MM/yyyy}").FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingTop(16).Column(section =>
                    {
                        // Cliente
                        section.Item().Text("DADOS DO CLIENTE").Bold().FontSize(9).FontColor(Colors.Grey.Darken2).LetterSpacing(1);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Nome").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.FullName).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Documento").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.Document?.Formatted ?? "—").Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Telefone").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.Phones.FirstOrDefault()?.Formatted ?? "—").Bold();
                            });
                        });
                    });

                    col.Item().PaddingTop(16).Column(section =>
                    {
                        // Equipamento
                        section.Item().Text("EQUIPAMENTO").Bold().FontSize(9).FontColor(Colors.Grey.Darken2).LetterSpacing(1);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Tipo").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.DeviceType).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Marca").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Brand).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Modelo").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Model).Bold();
                            });
                        });

                        section.Item().PaddingTop(8).Column(c =>
                        {
                            c.Item().Text("Defeito relatado").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Background(Colors.Grey.Lighten4).Padding(8).Text(order.ReportedDefect);
                        });

                        if (!string.IsNullOrEmpty(order.Accessories))
                            section.Item().PaddingTop(8).Column(c =>
                            {
                                c.Item().Text("Acessórios").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Accessories);
                            });

                        if (!string.IsNullOrEmpty(order.Observations))
                            section.Item().PaddingTop(8).Column(c =>
                            {
                                c.Item().Text("Observações").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Observations);
                            });
                    });

                    // Signature
                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(4).Text(string.Empty);
                            c.Item().PaddingTop(4).AlignCenter().Text("Assinatura do cliente").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                        row.ConstantItem(40);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(4).Text(string.Empty);
                            c.Item().PaddingTop(4).AlignCenter().Text("Assinatura do técnico").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingTop(8).AlignCenter()
                        .Text($"Documento gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Lighten1);
                });
            });
        }).GeneratePdf();
    }

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
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(company.Name).Bold().FontSize(16);
                            if (!string.IsNullOrEmpty(company.Street))
                                c.Item().Text($"{company.Street}, {company.Number} — {company.City}/{company.State}").FontColor(Colors.Grey.Darken1);
                            c.Item().Text(company.PhoneFormatted).FontColor(Colors.Grey.Darken1);
                        });
                        row.ConstantItem(120).AlignRight().Column(c =>
                        {
                            c.Item().Text("CERTIFICADO DE GARANTIA").Bold().FontSize(11);
                            c.Item().Text($"Ordem #{order.OrderNumber}").Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                            c.Item().Text($"Data: {DateTime.Today:dd/MM/yyyy}").FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // Garantia destaque
                    col.Item().PaddingTop(12).Background(Colors.Green.Lighten4).Border(0.5f).BorderColor(Colors.Green.Lighten2).Padding(12).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PRAZO DE GARANTIA").Bold().FontSize(9).FontColor(Colors.Green.Darken3).LetterSpacing(1);
                            c.Item().Text(warrantyText).Bold().FontSize(22).FontColor(Colors.Green.Darken2);
                        });
                    });

                    col.Item().PaddingTop(16).Column(section =>
                    {
                        // Cliente
                        section.Item().Text("DADOS DO CLIENTE").Bold().FontSize(9).FontColor(Colors.Grey.Darken2).LetterSpacing(1);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Nome").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.FullName).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Documento").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.Document?.Formatted ?? "—").Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Telefone").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.Phones.FirstOrDefault()?.Formatted ?? "—").Bold();
                            });
                        });
                    });

                    col.Item().PaddingTop(16).Column(section =>
                    {
                        // Equipamento
                        section.Item().Text("EQUIPAMENTO").Bold().FontSize(9).FontColor(Colors.Grey.Darken2).LetterSpacing(1);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Tipo").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.DeviceType).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Marca").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Brand).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Modelo").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Model).Bold();
                            });
                        });
                    });

                    col.Item().PaddingTop(16).Column(section =>
                    {
                        // Serviço
                        section.Item().Text("SERVIÇO REALIZADO").Bold().FontSize(9).FontColor(Colors.Grey.Darken2).LetterSpacing(1);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Descrição").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.BudgetDescription ?? "—").Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Valor").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"R$ {order.BudgetValue:N2}").Bold();
                            });
                        });
                    });

                    // Termos
                    col.Item().PaddingTop(16).Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                    {
                        c.Item().Text("TERMOS DA GARANTIA").Bold().FontSize(9).FontColor(Colors.Grey.Darken2).LetterSpacing(1);
                        c.Item().PaddingTop(4).Text(
                            $"A garantia de {warrantyText} cobre exclusivamente o defeito descrito acima e o serviço realizado. " +
                            "Não cobre danos físicos, líquidos, mau uso ou defeitos não relacionados ao serviço executado. " +
                            "Em caso de dúvidas, entre em contato com a empresa."
                        ).FontSize(9).FontColor(Colors.Grey.Darken2);
                    });

                    // Signature
                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(4).Text(string.Empty);
                            c.Item().PaddingTop(4).AlignCenter().Text("Assinatura do cliente").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                        row.ConstantItem(40);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(4).Text(string.Empty);
                            c.Item().PaddingTop(4).AlignCenter().Text("Assinatura do técnico").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingTop(8).AlignCenter()
                        .Text($"Documento gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Lighten1);
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateReturnReceipt(ServiceOrderOutput order, CustomerOutput customer, CompanyOutput company)
    {
        var reason = order.RepairResult switch
        {
            "NoFix" => "Sem conserto — equipamento não pôde ser reparado após avaliação técnica.",
            "NoDefectFound" => "Nenhum defeito detectado — o equipamento foi avaliado e não apresentou falha reproduzível.",
            _ => "Devolução do equipamento.",
        };

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(company.Name).Bold().FontSize(16);
                            if (!string.IsNullOrEmpty(company.Street))
                                c.Item().Text($"{company.Street}, {company.Number} — {company.City}/{company.State}").FontColor(Colors.Grey.Darken1);
                            c.Item().Text(company.PhoneFormatted).FontColor(Colors.Grey.Darken1);
                        });
                        row.ConstantItem(120).AlignRight().Column(c =>
                        {
                            c.Item().Text("COMPROVANTE DE DEVOLUÇÃO").Bold().FontSize(11);
                            c.Item().Text($"Ordem #{order.OrderNumber}").Bold().FontSize(14).FontColor(Colors.Orange.Darken2);
                            c.Item().Text($"Data: {DateTime.Today:dd/MM/yyyy}").FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // Motivo destaque
                    col.Item().PaddingTop(12).Background(Colors.Orange.Lighten4).Border(0.5f).BorderColor(Colors.Orange.Lighten2).Padding(12).Column(c =>
                    {
                        c.Item().Text("MOTIVO DA DEVOLUÇÃO").Bold().FontSize(9).FontColor(Colors.Orange.Darken3).LetterSpacing(1);
                        c.Item().PaddingTop(4).Text(reason).FontSize(11).FontColor(Colors.Orange.Darken2);
                        c.Item().PaddingTop(4).Text("Este documento não possui cláusula de garantia.").Italic().FontSize(9).FontColor(Colors.Grey.Darken2);
                    });

                    col.Item().PaddingTop(16).Column(section =>
                    {
                        // Cliente
                        section.Item().Text("DADOS DO CLIENTE").Bold().FontSize(9).FontColor(Colors.Grey.Darken2).LetterSpacing(1);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Nome").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.FullName).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Documento").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.Document?.Formatted ?? "—").Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Telefone").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(customer.Phones.FirstOrDefault()?.Formatted ?? "—").Bold();
                            });
                        });
                    });

                    col.Item().PaddingTop(16).Column(section =>
                    {
                        // Equipamento
                        section.Item().Text("EQUIPAMENTO").Bold().FontSize(9).FontColor(Colors.Grey.Darken2).LetterSpacing(1);
                        section.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Tipo").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.DeviceType).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Marca").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Brand).Bold();
                            });
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Column(c =>
                            {
                                c.Item().Text("Modelo").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Model).Bold();
                            });
                        });

                        if (!string.IsNullOrEmpty(order.Observations))
                            section.Item().PaddingTop(8).Column(c =>
                            {
                                c.Item().Text("Observações").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(order.Observations);
                            });
                    });

                    // Signature
                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(4).Text(string.Empty);
                            c.Item().PaddingTop(4).AlignCenter().Text("Assinatura do cliente").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                        row.ConstantItem(40);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).PaddingBottom(4).Text(string.Empty);
                            c.Item().PaddingTop(4).AlignCenter().Text("Assinatura do técnico").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingTop(8).AlignCenter()
                        .Text($"Documento gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Lighten1);
                });
            });
        }).GeneratePdf();
    }

    private static string FormatWarranty(int duration, string unit) => unit switch
    {
        "Days"   => $"{duration} {(duration == 1 ? "dia" : "dias")}",
        "Months" => $"{duration} {(duration == 1 ? "mês" : "meses")}",
        "Years"  => $"{duration} {(duration == 1 ? "ano" : "anos")}",
        _        => $"{duration} {unit}",
    };
}

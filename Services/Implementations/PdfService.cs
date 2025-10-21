using Microsoft.Extensions.Configuration;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;

namespace PegasusBackend.Services.Implementations
{
    public class PdfService : IPdfService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string FontHeading = "Oswald";
        private const string FontBody = "Open Sans";

        public PdfService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<byte[]> GenerateReceiptPdfAsync(BookingConfirmationRequestDto variabel)
        {

            var logoUrl = _configuration["PdfSettings:LogoUrl"];
            var companyName = _configuration["PdfSettings:CompanyName"] ?? "Pegasus Transport";
            var companyEmail = _configuration["PdfSettings:CompanyEmail"] ?? "info@pegasustransport.se";
            var companyPhone = _configuration["PdfSettings:CompanyPhone"] ?? "08-123 45 67";

            var backgroundColor = _configuration["PdfSettings:BackgroundColor"] ?? "#f9f0df";
            var headerColor = _configuration["PdfSettings:HeaderColor"] ?? "#032240";
            var labelColor = _configuration["PdfSettings:LabelColor"] ?? "#1ea896";
            var priceColor = _configuration["PdfSettings:PriceColor"] ?? "#e5723a";
            var defaultTextColor = _configuration["PdfSettings:DefaultTextColor"] ?? "#000000";

            var svSE = new CultureInfo("sv-SE");
            byte[] logoData = null;

            if (!string.IsNullOrEmpty(logoUrl))
            {
                try
                {
                    using (var client = _httpClientFactory.CreateClient())
                    {
                        logoData = await client.GetByteArrayAsync(logoUrl);
                    }
                }
                catch (Exception) { }
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(backgroundColor); 
                    page.DefaultTextStyle(x => x.FontFamily(FontBody).FontSize(12).FontColor(defaultTextColor));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Pegasus Transport")
                                .FontFamily(FontHeading)
                                .FontSize(32).Bold().FontColor(headerColor); 
                            col.Item().Text("Kvitto på utförd resa")
                                .FontFamily(FontHeading)
                                .FontSize(18).SemiBold().FontColor(headerColor);
                        });

                        if (logoData != null)
                        {
                            row.ConstantItem(120).Image(logoData).FitArea();
                        }
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(30);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(companyCol =>
                            {
                                companyCol.Spacing(2);
                                companyCol.Item().Text("Från:")
                                    .FontFamily(FontHeading)
                                    .FontSize(14).Bold().FontColor(labelColor); 
                                companyCol.Item().Text(companyName).Bold();
                                companyCol.Item().Text(companyEmail);
                                companyCol.Item().Text(companyPhone);
                            });

                            row.RelativeItem().Column(customerCol =>
                            {
                                customerCol.Spacing(2);
                                customerCol.Item().Text("Till:")
                                    .FontFamily(FontHeading)
                                    .FontSize(14).Bold().FontColor(labelColor);
                                customerCol.Item().Text(variabel.Firstname)
                                    .FontSize(14).Bold();
                            });
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        col.Item().Column(detailsCol =>
                        {
                            detailsCol.Spacing(5);
                            detailsCol.Item().Text("Resedetaljer")
                                .FontFamily(FontHeading)
                                .FontSize(16).Bold().FontColor(headerColor); 
                            detailsCol.Item().Text(text =>
                            {
                                text.Span("Upphämtning: ").Bold();
                                text.Span(variabel.PickupAddress);
                            });
                            detailsCol.Item().Text(text =>
                            {
                                text.Span("Stops: ").Bold();
                                text.Span(variabel.Stops);
                            });
                            detailsCol.Item().Text(text =>
                            {
                                text.Span("Destination: ").Bold();
                                text.Span(variabel.Destination);
                            });
                            detailsCol.Item().Text(text =>
                            {
                                text.Span("Datum: ").Bold();
                                text.Span($"{DateTime.Now:yyyy-MM-dd HH:mm}");
                            });
                        });

                        col.Item().AlignRight()
                           .Text($"Totalt pris: {variabel.TotalPrice.ToString("C", svSE)}")
                           .FontFamily(FontHeading)
                           .FontSize(20).Bold().FontColor(priceColor);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(10));
                            text.Span("Tack för att du reser med ");
                            text.Span(companyName).Bold();
                            text.Span("!");
                        });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return pdfBytes;
        }
    }
}
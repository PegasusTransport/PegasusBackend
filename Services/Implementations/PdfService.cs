using Microsoft.Extensions.Configuration;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net.Http;
using System.Threading.Tasks;

namespace PegasusBackend.Services.Implementations
{
    public class PdfService : IPdfService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

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
            var backgroundColor = _configuration["PdfSettings:BackgroundColor"] ?? "#FFFFFF"; // Get color!

            byte[] logoData = null!;
            if (!string.IsNullOrEmpty(logoUrl))
            {
                try
                {
                    using (var client = _httpClientFactory.CreateClient())
                    {
                        logoData = await client.GetByteArrayAsync(logoUrl);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(backgroundColor); 

                    page.Header().Row(row =>
                    {

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Pegasus Transport")
                                .FontSize(32).Bold().FontColor(Colors.Blue.Darken4); // H1
                            col.Item().Text("Kvitto på utförd resa")
                                .FontSize(18).SemiBold();
                        });

                        if (logoData != null)
                        {
                            row.ConstantItem(120)
                               .Image(logoData)
                               .FitArea();
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
                                companyCol.Item().Text("Från:").FontSize(14).Bold();
                                companyCol.Item().Text(companyName);
                                companyCol.Item().Text(companyEmail);
                                companyCol.Item().Text(companyPhone);
                            });

                            row.RelativeItem().Column(customerCol =>
                            {
                                customerCol.Spacing(2);
                                customerCol.Item().Text("Till:").FontSize(14).Bold();
                                customerCol.Item().Text(variabel.Firstname);
                            });
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Column(detailsCol =>
                        {
                            detailsCol.Spacing(5);
                            detailsCol.Item().Text("Resedetaljer").FontSize(16).Bold();
                            detailsCol.Item().Text($"Upphämtning: {variabel.PickupAddress}");
                            detailsCol.Item().Text($"Stops: {variabel.Stops}");
                            detailsCol.Item().Text($"Destination: {variabel.Destination}");
                            detailsCol.Item().Text($"Datum: {DateTime.Now:yyyy-MM-dd HH:mm}");
                        });

                        col.Item().AlignRight()
                           .Text($"Totalt pris: {variabel.TotalPrice:C}")
                           .FontSize(20).Bold();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
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
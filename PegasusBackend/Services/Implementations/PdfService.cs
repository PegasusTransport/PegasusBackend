using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace PegasusBackend.Services.Implementations
{
    public class PdfService : IPdfService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PdfService> _logger;
        private const string FontHeading = "Oswald";
        private const string FontBody = "Open Sans";

        public PdfService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<PdfService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ServiceResponse<byte[]>> GenerateReceiptPdfAsync(ReceiptRequestDto receipt)
        {
            try
            {
                _logger.LogInformation("[PDF] Generating receipt for {CustomerName}", receipt.CustomerFirstname);

                var settings = LoadPdfSettings();
                var logoData = await TryDownloadLogoAsync(settings.LogoUrl);
                var totalDuration = ParseDurationInMinutes(receipt.DurationMinutes);
                var pdfDocument = BuildReceiptDocument(receipt, settings, logoData, totalDuration);
                var pdfBytes = GeneratePdf(pdfDocument);

                _logger.LogInformation("[PDF] Successfully generated receipt for {CustomerName}", receipt.CustomerFirstname);

                return ServiceResponse<byte[]>.SuccessResponse(
                    HttpStatusCode.OK,
                    pdfBytes,
                    "PDF generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PDF] Error generating receipt for {CustomerName}", receipt.CustomerFirstname);
                return ServiceResponse<byte[]>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    $"Error generating PDF: {ex.Message}");
            }
        }

        private PdfSettingsModel LoadPdfSettings()
        {
            return new PdfSettingsModel
            {
                LogoUrl = _configuration["PdfSettings:LogoUrl"],
                CompanyName = _configuration["PdfSettings:CompanyName"] ?? "Pegasus Transport AB",
                CompanyEmail = _configuration["PdfSettings:CompanyEmail"] ?? "info@pegasustransport.se",
                CompanyPhone = _configuration["PdfSettings:CompanyPhone"] ?? "08-123 45 67",
                HeaderColor = _configuration["PdfSettings:HeaderColor"] ?? "#032240",
                LabelColor = _configuration["PdfSettings:LabelColor"] ?? "#1ea896",
                PriceColor = _configuration["PdfSettings:PriceColor"] ?? "#e5723a",
                DefaultTextColor = _configuration["PdfSettings:DefaultTextColor"] ?? "#000000",
                BackgroundColor = _configuration["PdfSettings:BackgroundColor"] ?? "#f9f0df"
            };
        }

        private async Task<byte[]?> TryDownloadLogoAsync(string? logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl)) return null;

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                return await client.GetByteArrayAsync(logoUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PDF] Failed to download logo from {LogoUrl}", logoUrl);
                return null;
            }
        }

        private int ParseDurationInMinutes(string durationInput)
        {
            if (string.IsNullOrWhiteSpace(durationInput)) return 0;

            durationInput = durationInput.Replace(" t", "").Trim();
            var parts = durationInput.Split(':');

            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int hours) &&
                int.TryParse(parts[1], out int minutes))
                return (hours * 60) + minutes;

            if (int.TryParse(durationInput, out int totalMinutes))
                return totalMinutes;

            _logger.LogWarning("[PDF] Could not parse duration input: {DurationInput}", durationInput);
            return 0;
        }

        private Document BuildReceiptDocument(
            ReceiptRequestDto receipt,
            PdfSettingsModel settings,
            byte[]? logoData,
            int totalDuration)
        {
            var svSE = new CultureInfo("sv-SE");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePageDefaults(page, settings);

                    BuildHeader(page, settings, logoData);
                    BuildContent(page, receipt, settings, totalDuration, svSE);
                    BuildFooter(page, settings);
                });
            });
        }

        private void ConfigurePageDefaults(PageDescriptor page, PdfSettingsModel settings)
        {
            page.MarginVertical(30);
            page.MarginHorizontal(40);
            page.Size(PageSizes.A4);
            page.PageColor(settings.BackgroundColor);
            page.DefaultTextStyle(x => x.FontFamily(FontBody).FontSize(9f).FontColor(settings.DefaultTextColor));
        }

        private void BuildHeader(PageDescriptor page, PdfSettingsModel settings, byte[]? logoData)
        {
            page.Header().PaddingBottom(5).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(settings.CompanyName)
                        .FontFamily(FontHeading)
                        .FontSize(28).Bold().FontColor(settings.HeaderColor);
                    col.Item().Text("Receipt for Completed Trip")
                        .FontFamily(FontHeading)
                        .FontSize(14).SemiBold().FontColor(settings.HeaderColor);
                });

                if (logoData != null)
                    row.ConstantItem(80).AlignRight().Image(logoData).FitArea();
            });
        }

        private void BuildContent(
            PageDescriptor page,
            ReceiptRequestDto receipt,
            PdfSettingsModel settings,
            int totalDuration,
            CultureInfo svSE)
        {
            page.Content().PaddingVertical(5).Column(col =>
            {
                col.Spacing(10);
                BuildSenderReceiverSection(col, settings, receipt);
                col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                BuildTripDetailsSection(col, receipt, settings, totalDuration, svSE);
                col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                BuildDriverSection(col, receipt, settings);
                BuildTotalPriceSection(col, receipt, settings, svSE);
            });
        }

        private void BuildSenderReceiverSection(ColumnDescriptor col, PdfSettingsModel settings, ReceiptRequestDto receipt)
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(companyCol =>
                {
                    companyCol.Spacing(1);
                    companyCol.Item().Text("From:")
                        .FontFamily(FontHeading).FontSize(10).Bold().FontColor(settings.LabelColor);
                    companyCol.Item().Text(settings.CompanyName).Bold();
                    companyCol.Item().Text(settings.CompanyEmail);
                    companyCol.Item().Text(settings.CompanyPhone);
                });

                row.RelativeItem().Column(customerCol =>
                {
                    customerCol.Spacing(1);
                    customerCol.Item().Text("To:")
                        .FontFamily(FontHeading).FontSize(10).Bold().FontColor(settings.LabelColor);
                    customerCol.Item().Text(receipt.CustomerFirstname).FontSize(10).Bold();
                });
            });
        }

        private void BuildTripDetailsSection(
            ColumnDescriptor col,
            ReceiptRequestDto receipt,
            PdfSettingsModel settings,
            int totalDuration,
            CultureInfo svSE)
        {
            col.Item().Column(details =>
            {
                details.Spacing(2);
                details.Item().Text("Trip Details")
                    .FontFamily(FontHeading).FontSize(12).Bold().FontColor(settings.HeaderColor);

                details.Item().Text(t => { t.Span("Pickup: ").Bold(); t.Span(receipt.PickupAddress); });
                if (!string.IsNullOrWhiteSpace(receipt.Stops))
                    details.Item().Text(t => { t.Span("Stops: ").Bold(); t.Span(receipt.Stops); });
                details.Item().Text(t => { t.Span("Destination: ").Bold(); t.Span(receipt.Destination); });
                details.Item().Text(t => { t.Span("Pickup Time: ").Bold(); t.Span(receipt.PickupTime.ToString("yyyy-MM-dd HH:mm", svSE)); });
                details.Item().Text(t => { t.Span("Distance: ").Bold(); t.Span($"{receipt.DistanceKm:N1} km"); });
                details.Item().Text(t => { t.Span("Travel Time: ").Bold(); t.Span($"{totalDuration} min"); });
            });
        }

        private void BuildDriverSection(ColumnDescriptor col, ReceiptRequestDto receipt, PdfSettingsModel settings)
        {
            col.Item().Column(driver =>
            {
                driver.Spacing(2);
                driver.Item().Text("Driver & Vehicle")
                    .FontFamily(FontHeading).FontSize(12).Bold().FontColor(settings.HeaderColor);
                driver.Item().Text(t => { t.Span("Driver: ").Bold(); t.Span(receipt.DriverFirstname); });
                driver.Item().Text(t => { t.Span("License Plate: ").Bold(); t.Span(receipt.LicensePlate); });
            });
        }

        private void BuildTotalPriceSection(ColumnDescriptor col, ReceiptRequestDto receipt, PdfSettingsModel settings, CultureInfo svSE)
        {
            col.Item().AlignRight().Text($"Total Price: {receipt.TotalPrice.ToString("C", svSE)}")
                .FontFamily(FontHeading).FontSize(14).Bold().FontColor(settings.PriceColor);
        }

        private void BuildFooter(PageDescriptor page, PdfSettingsModel settings)
        {
            page.Footer()
                .AlignCenter()
                .Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                    text.Span("Thank you for traveling with ");
                    text.Span(settings.CompanyName).Bold();
                    text.Span("!");
                });
        }

        protected virtual byte[] GeneratePdf(Document doc)
        {
            return doc.GeneratePdf();
        }
    }

    internal class PdfSettingsModel
    {
        public string? LogoUrl { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string HeaderColor { get; set; } = string.Empty;
        public string LabelColor { get; set; } = string.Empty;
        public string PriceColor { get; set; } = string.Empty;
        public string DefaultTextColor { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
    }
}

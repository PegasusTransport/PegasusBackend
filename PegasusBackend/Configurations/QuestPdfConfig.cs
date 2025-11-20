using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace PegasusBackend.Configurations
{
    public static class QuestPdfConfig
    {
        public static IServiceCollection AddQuestPdfConfiguration(this IServiceCollection services)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("QuestPdfConfig");

            try
            {
                logger?.LogInformation("Registering custom fonts for QuestPDF...");

                FontManager.RegisterFont(File.OpenRead("Fonts/Oswald.ttf"));
                FontManager.RegisterFont(File.OpenRead("Fonts/OpenSans.ttf"));
                FontManager.RegisterFont(File.OpenRead("Fonts/Oswald-Bold.ttf"));
                FontManager.RegisterFont(File.OpenRead("Fonts/OpenSans-Bold.ttf"));

                logger?.LogInformation("Custom fonts registered successfully.");
            }
            catch (FileNotFoundException ex)
            {
                logger?.LogError(ex, "FONT ERROR: Could not find font file: {FileName}. Ensure 'Copy to Output Directory' is set.", ex.FileName);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "FONT ERROR: An unexpected error occurred while loading fonts.");
            }

            return services;
        }
    }
}
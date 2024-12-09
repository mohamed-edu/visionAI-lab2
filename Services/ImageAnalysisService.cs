﻿using visionAI_lab2.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;


namespace visionAI_lab2.Services
{
    public class ImageAnalysisService
    {
        private readonly ComputerVisionClient cvClient;

        public ImageAnalysisService(IConfiguration configuration)
        {
            // Hämta endpoint och key från appsettings.json
            string Endpoint = configuration["CognitiveServices:Endpoint"];
            string Key = configuration["CognitiveServices:Key"];

            // Konfigurera klienten
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(Key);
            cvClient = new ComputerVisionClient(credentials)
            {
                Endpoint = Endpoint
            };
        }

        public async Task<Models.ImageAnalysis> AnalyzeImageAsync(string imageUrlOrPath)
        {
            Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.ImageAnalysis analysis = null;
            var result = new Models.ImageAnalysis();

            if (Uri.IsWellFormedUriString(imageUrlOrPath, UriKind.Absolute))
            {
                // Analysera bild från URL
                analysis = await cvClient.AnalyzeImageAsync(imageUrlOrPath, GetFeatures());
            }
            else if (File.Exists(imageUrlOrPath))
            {
                // Analysera bild från lokal filsökväg
                using (var imageData = File.OpenRead(imageUrlOrPath))
                {
                    analysis = await cvClient.AnalyzeImageInStreamAsync(imageData, GetFeatures());
                }
            }

            // Kontrollera om analysobjektet är null
            if (analysis != null)
            {
                // Extrahera resultatet med null-kontroller
                result.Description = analysis.Description?.Captions.FirstOrDefault()?.Text ?? "Ingen beskrivning tillgänglig";
                result.Tags = analysis.Tags?.Select(t => t.Name).ToList() ?? new List<string>();
                result.Categories = analysis.Categories?.Select(c => c.Name).ToList() ?? new List<string>();
                result.Objects = analysis.Objects?.Select(o =>
                    $"{o.ObjectProperty} (X: {o.Rectangle.X}, Y: {o.Rectangle.Y}, Width: {o.Rectangle.W}, Height: {o.Rectangle.H})").ToList()
                    ?? new List<string>();
            }
            else
            {
                result.Description = "Ingen analys kunde utföras.";
            }

            return result;
        }

        public async Task<string> GetThumbnailAsync(string imageUrlOrPath, int width, int height)
        {
            Stream thumbnailStream = null;
            string thumbnailFileName;

            if (Uri.IsWellFormedUriString(imageUrlOrPath, UriKind.Absolute))
            {
                thumbnailStream = await cvClient.GenerateThumbnailAsync(width, height, imageUrlOrPath, true);
            }
            else if (File.Exists(imageUrlOrPath))
            {
                using (var imageData = File.OpenRead(imageUrlOrPath))
                {
                    thumbnailStream = await cvClient.GenerateThumbnailInStreamAsync(width, height, imageData, true);
                }
            }

            thumbnailFileName = Path.Combine("wwwroot", "thumbnails", $"{Guid.NewGuid()}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(thumbnailFileName));

            using (Stream thumbnailFile = File.Create(thumbnailFileName))
            {
                await thumbnailStream.CopyToAsync(thumbnailFile);
            }

            return $"/thumbnails/{Path.GetFileName(thumbnailFileName)}";
        }

        private List<VisualFeatureTypes?> GetFeatures()
        {
            return new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Adult
            };
        }
    }
}

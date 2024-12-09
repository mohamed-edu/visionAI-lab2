using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using visionAI_lab2.Services;

namespace visionAI_lab2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ImageAnalysisService _imageAnalysisService;

        public HomeController(ImageAnalysisService imageAnalysisService)
        {
            _imageAnalysisService = imageAnalysisService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(string imageUrl, string imagePath, int thumbnailWidth = 500, int thumbnailHeight = 500, IFormFile file = null)
        {
            string input = !string.IsNullOrEmpty(imageUrl) ? imageUrl : imagePath;

            if (file != null && file.Length > 0)
            {
                // hämtar fil bilden
                string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                input = tempFilePath;
            }
            else if (string.IsNullOrEmpty(input))
            {
                ModelState.AddModelError("", "Please provide either an image URL or a local image path.");
                return View("Index");
            }

            var analysisResult = await _imageAnalysisService.AnalyzeImageAsync(input);
            analysisResult.ThumbnailUrl = await _imageAnalysisService.GetThumbnailAsync(input, thumbnailWidth, thumbnailHeight);

            // Rensa temporär fil om en sådan användes
            if (file != null)
            {
                System.IO.File.Delete(input);
            }

            return View("Result", analysisResult);
        }
    }
}

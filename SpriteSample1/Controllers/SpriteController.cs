using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;
using SpriteSample1.Contracts;
using System.Xml;

namespace SpriteSample1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpriteController : ControllerBase
    {
        [HttpPost(Name = "CreateSprite")]
        public async Task<IActionResult> Post()
        {
            try
            {
                createSvgFromSvgFolder(@"samples", @"Results");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return Ok();
        }

        private string[] createSpriteFromDimension(int filesPathLength, int width, int height, int xpadding, int ypadding)
        {
            string[] resLines = new string[filesPathLength + 2];
            int svgWidth = 500;
            int svgHeight = (int)Math.Ceiling(filesPathLength * 1.0 / (svgWidth / (width + xpadding))) * (height + ypadding);
            resLines[0] = String.Format("<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/2000/xlink\" width=\"{0}\" height=\"{1}\">", svgWidth, svgHeight);
            resLines[filesPathLength + 1] = "</svg>";
            return resLines;
        }

        private string[] createSpriteSymbolFromDimension(int filesPathLength)
        {
            string[] resLines = new string[filesPathLength + 4];
            resLines[0] = String.Format("<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/2000/xlink\">");
            resLines[1] = "<defs>";
            resLines[filesPathLength + 2] = "</defs>";
            resLines[filesPathLength + 3] = "</svg>";
            return resLines;
        }

        private void createJsonFile(List<ImageList> res, string outputFolderPath)
        {
            var jsonFormattedImageList = Newtonsoft.Json.JsonConvert.SerializeObject(res);

            string jsonFileName = Path.Combine(outputFolderPath, "ImageListJson.json");

            System.IO.File.WriteAllText(jsonFileName, jsonFormattedImageList);
        }

        private string createContentFromDimension(string id, string viewBox, int width, int height, int x, int y)
        {
            return String.Format("<svg xmlns=\"http://www.w3.org/2000/svg\" id=\"{0}\" viewBox=\"{1}\" width=\"{2}\" height=\"{3}\" x=\"{4}\" y=\"{5}\" >", id, viewBox, width, height, x, y);
        }

        private string createSymbolContentFromDimension(string id, string viewBox)
        {
            return String.Format("<symbol xmlns=\"http://www.w3.org/2000/svg\" id=\"{0}\" viewBox=\"{1}\">", id, viewBox);
        }

        private string[] createSprite(string[] filesPath, List<ImageList> res)
        {
            int width = 23;
            int height = 23;
            int xpadding = 0;
            int ypadding = 0;

            int filesPathLength = filesPath.Length;
            string[] resLines = createSpriteFromDimension(filesPathLength, width, height, xpadding, ypadding);

            int x = 0;
            int y = 0;
            int id = 1;

            for (int i = 0; i < filesPath.Length; i++)
            {
                string iconPath = filesPath[i];
                string fileName = Path.GetFileNameWithoutExtension(iconPath);
                int index = fileName.IndexOf('-');
                string identifier = "Misc";

                if (index != -1)
                {
                    identifier = fileName.Substring(0, index);
                }

                XmlDocument overlayDoc = new XmlDocument();
                overlayDoc.Load(iconPath);
                string viewBox = overlayDoc.DocumentElement.GetAttribute("viewBox");

                if (string.IsNullOrEmpty(viewBox))
                {
                    continue;
                }

                string content = createContentFromDimension(fileName, viewBox, width, height, x, y);

                // Find SVG node
                foreach (XmlNode onode in overlayDoc.ChildNodes)
                {
                    if (onode.Name == "svg")
                    {
                        // Copy all the children of the SVG node
                        // over to the document in memory
                        foreach (XmlNode on in onode.ChildNodes)
                        {
                            content = content + on.OuterXml;
                        }
                    }
                }
                content = content + "</svg>";
                resLines[i + 1] = content;

                ImageList imageDetails = new ImageList()
                {
                    Id = fileName,
                    FileName = fileName,
                    Identifier = identifier,
                    Coordinate = new Location() { X = x, Y = y },
                    Dimension = new Dimension() { Height = height, Width = width },
                };

                res.Add(imageDetails);

                x += width + xpadding;

                if (x + width + xpadding > 500)
                {
                    x = 0;
                    y += height + ypadding;
                }
            }
            return resLines;
        }

        private string[] createSpriteSymbol(string[] filesPath)
        {
            int filesPathLength = filesPath.Length;
            string[] resLines = createSpriteSymbolFromDimension(filesPathLength);

            for (int i = 0; i < filesPath.Length; i++)
            {
                string iconPath = filesPath[i];
                string fileName = Path.GetFileNameWithoutExtension(iconPath);

                XmlDocument overlayDoc = new XmlDocument();
                overlayDoc.Load(iconPath);
                string viewBox = overlayDoc.DocumentElement.GetAttribute("viewBox");

                if (string.IsNullOrEmpty(viewBox))
                {
                    continue;
                }

                string content = createSymbolContentFromDimension(fileName, viewBox);

                // Find SVG node
                foreach (XmlNode onode in overlayDoc.ChildNodes)
                {
                    if (onode.Name == "svg")
                    {
                        // Copy all the children of the SVG node
                        // over to the document in memory
                        foreach (XmlNode on in onode.ChildNodes)
                        {
                            content = content + on.OuterXml;
                        }
                    }
                }
                content = content + "</symbol>";
                resLines[i + 2] = content;

            }
            return resLines;
        }

        private void createSvgFromSvgFolder(string inputFolderPath, string outputFolderPath)
        {
            var filesPath = Directory.GetFiles(inputFolderPath);

            List<ImageList> res = new List<ImageList>();

            string[] resLines = createSprite(filesPath, res);

            string[] resLines2 = createSpriteSymbol(filesPath);


            string svgFileName = Path.Combine(outputFolderPath, "generators.svg");
            string svgFileName2 = Path.Combine(outputFolderPath, "generatorsSymbol.svg");
            System.IO.File.WriteAllLines(svgFileName, resLines);
            System.IO.File.WriteAllLines(svgFileName2, resLines2);

            createJsonFile(res, outputFolderPath);

        }
    }
}

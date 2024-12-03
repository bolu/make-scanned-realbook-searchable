using Syncfusion.Pdf;
using Syncfusion.Pdf.Interactive;
using System.IO;
using Syncfusion.Drawing;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.Collections.Generic;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
    "redacted - put yours here");

// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

const string sourcePath =
    @"C:/Users/Borek/Documents/tablet/Fakebooks/Hal Leonard The Real Book Sixth Edition volume 1.pdf";
const string indexPath = @"C:\Users\Borek\Documents\hal-leonard-realbook-sixth-index.txt";

// Read and load the original PDF
var pdfBytes = File.ReadAllBytes(sourcePath);
var loadedDocument = new PdfLoadedDocument(pdfBytes);
var indexLines = File.ReadAllLines(indexPath);

var songMap = new Dictionary<int, string>();
var pageNumber = 1;
foreach (var line in indexLines)
{
    if (line.StartsWith("="))
    {
        if (int.TryParse(line.AsSpan(1), out var newNumber))
        {
            pageNumber = newNumber;
        }
    }
    else
    {
        if (!songMap.TryAdd(pageNumber, line)) throw new Exception();
        pageNumber++;
    }
}

// Create a new document with the same page size as original
var newDocument = new PdfDocument();
var pageSettings = new PdfPageSettings
{
    Size = loadedDocument.Pages[10].Size,
    Margins =
    {
        All = 0
    }
};
newDocument.PageSettings=pageSettings;

var pagesToProcess =loadedDocument.Pages.Count;

// Loop through the pages
for (var i = 0; i < pagesToProcess; i++)
{
    Console.WriteLine("Page: " + i);
    var loadedPage = loadedDocument.Pages[i];
    
    var newPage = newDocument.Pages.Add();
    var graphics = newPage.Graphics;

    // Draw the template at exact coordinates without offset
    var template = loadedPage.CreateTemplate();
    graphics.DrawPdfTemplate(template, new PointF(0, 0), loadedPage.Size);

    // Draw the text on each page
    if (songMap.TryGetValue(i+2, out var text))
    {
        var textElement = new PdfTextElement(text)
        {
            Font = new PdfStandardFont(PdfFontFamily.Helvetica, 30),
            Brush = new PdfSolidBrush(Color.DarkKhaki)
        };
        
        // Calculate text size to position it correctly
        var textSize = textElement.Font.MeasureString(text);
        // Position text in top right corner with 10 units padding
        var textPosition = new PointF(
            loadedPage.Size.Width - textSize.Width - 10,
            10
        );
        
        textElement.Draw(graphics, textPosition);
    }
}

loadedDocument.Close();

// Save the new document
using (FileStream outputStream = new FileStream("output.pdf", FileMode.Create))
{
    newDocument.Save(outputStream);
}
newDocument.Close();

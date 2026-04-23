using System.Globalization;
using System.Text;

namespace Coolzo.Api.Utilities;

internal static class SimplePdfDocumentBuilder
{
    public static byte[] Build(IEnumerable<string> lines)
    {
        var sanitizedLines = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(EscapePdfText)
            .ToArray();

        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 12 Tf");
        contentBuilder.AppendLine("50 760 Td");

        for (var index = 0; index < sanitizedLines.Length; index++)
        {
            if (index > 0)
            {
                contentBuilder.AppendLine("0 -18 Td");
            }

            contentBuilder.Append('(')
                .Append(sanitizedLines[index])
                .AppendLine(") Tj");
        }

        contentBuilder.AppendLine("ET");
        var content = contentBuilder.ToString();

        var objects = new[]
        {
            "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
            "2 0 obj\n<< /Type /Pages /Count 1 /Kids [3 0 R] >>\nendobj\n",
            "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>\nendobj\n",
            $"4 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream\nendobj\n",
            "5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n"
        };

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("%PDF-1.4\n");
        writer.Flush();

        var offsets = new List<long> { 0 };

        foreach (var pdfObject in objects)
        {
            offsets.Add(stream.Position);
            writer.Write(pdfObject);
            writer.Flush();
        }

        var xrefPosition = stream.Position;
        writer.Write($"xref\n0 {objects.Length + 1}\n");
        writer.Write("0000000000 65535 f \n");

        foreach (var offset in offsets.Skip(1))
        {
            writer.Write(offset.ToString("D10", CultureInfo.InvariantCulture));
            writer.Write(" 00000 n \n");
        }

        writer.Write($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\n");
        writer.Write($"startxref\n{xrefPosition}\n%%EOF");
        writer.Flush();

        return stream.ToArray();
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }
}

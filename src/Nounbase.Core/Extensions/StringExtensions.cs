namespace Nounbase.Core.Extensions
{
    public static class StringExtensions
    {
        public static string? AOrAn(this string? source) =>
            source == null ? null : (new[] { 'a', 'e', 'i', 'o', 'u' }.Contains(source.FirstOrDefault()) ? "an" : "a") + " " + source;

        public static string? ReplaceSpecialCharacters(this string? source, char with = '_') =>
            source == null ? null : new string(source.Select(c => char.IsLetterOrDigit(c) ? c : with).ToArray());

        public static string? RemoveJsonLabels(this string? source) =>
            source?.StartsWith("```json") == true ? source[7..^3] : source;

        public static string GetImageMimeType(this string base64Image)
        {
            ArgumentNullException.ThrowIfNull(base64Image, nameof(base64Image));

            string mimeType = "unknown";
            string base64Header = base64Image.Substring(0, Math.Min(30, base64Image.Length));

            if (base64Header.StartsWith("iVBORw0KGgo"))
            {
                mimeType = "image/png";
            }
            else if (base64Header.StartsWith("/9j/"))
            {
                mimeType = "image/jpeg";
            }
            else if (base64Header.StartsWith("R0lGOD"))
            {
                mimeType = "image/gif";
            }
            else if (base64Header.StartsWith("UklGR"))
            {
                mimeType = "image/webp";
            }
            else if (base64Header.StartsWith("Qk02"))
            {
                mimeType = "image/bmp";
            }

            return mimeType;
        }
    }
}

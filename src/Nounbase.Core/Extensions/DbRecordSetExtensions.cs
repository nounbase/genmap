using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;
using System.Text;

namespace Nounbase.Core.Extensions
{
    public static class DbRecordSetExtensions
    {
        public static DbRecordSet Skip(this DbRecordSet source, int rowCt)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return new DbRecordSet
            {
                ColumnNames = source.ColumnNames,
                Rows = source.Rows?.Skip(rowCt).ToArray()
            };
        }

        public static DbRecordSet Take(this DbRecordSet source, int rowCt)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return new DbRecordSet
            {
                ColumnNames = source.ColumnNames,
                Rows = source.Rows?.Take(rowCt).ToArray()
            };
        }

        public static DbRecordSet SelectColumns(this DbRecordSet source, params string[] columnNames)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(source!.ColumnNames, $"{nameof(source)}.{nameof(source.ColumnNames)}");

            var columnIndexes = new List<int>();

            foreach (var columnName in columnNames)
            {
                var columnIndex = Array.IndexOf(columnNames, columnName);

                if (columnIndex == -1)
                {
                    throw new InvalidOperationException(
                        $"The specified column name [{columnName}] does not exist in the source record set.");
                }

                columnIndexes.Add(columnIndex);
            }

            return new DbRecordSet
            {
                ColumnNames = columnNames,
                Rows = source.Rows?.Select(row => row.Where((cell, index) => columnIndexes.Contains(index)).ToArray()).ToArray()
            };
        }

        public static string ToHtmlTable(this DbRecordSet source) =>
            $$"""
              <table class="table table-striped">
                <thead>
                  <tr>
                    {{
                        string.Join(
                            Environment.NewLine, 
                            source.ColumnNames!.Select(columnName => $"<th scope=\"col\">{columnName}</th>"))
                    }}
                  </tr>
                <thead>
                <tbody>
                    {{
                        string.Join(
                            Environment.NewLine, 
                            source.Rows!.Select(row => $"<tr>{string.Join(
                                Environment.NewLine, 
                                row.Select(cell => $"<td>{cell}</td>"))}</tr>"))
                    }}
                </tbody>
              </table>
              """;

        public static DbRecordSet UseTitlesAsColumnHeaders(this DbRecordSet source, Noun noun)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(noun, nameof(noun));

            for (int i = 0; i < source.ColumnNames!.Length; i++)
            {
                var columnName = source.ColumnNames[i];

                var property = noun.Properties!.FirstOrDefault(
                    p => string.Equals(columnName, p.Name, StringComparison.InvariantCultureIgnoreCase));

                if (!string.IsNullOrEmpty(property?.Title))
                {
                    source.ColumnNames[i] = property!.Title!;
                }
            }

            return source;
        }

        public static string ToTsvTable(this DbRecordSet source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(source!.ColumnNames, $"{nameof(source)}.{nameof(source.ColumnNames)}");

            var tsvBuilder = new StringBuilder(string.Join('\t', source.ColumnNames!));

            if (source.Rows?.Any() == true)
            {
                tsvBuilder.AppendLine();

                foreach (var row in source.Rows!)
                {
                    tsvBuilder.AppendLine(string.Join('\t', row));
                }
            }

            return tsvBuilder.ToString();
        }
    }
}

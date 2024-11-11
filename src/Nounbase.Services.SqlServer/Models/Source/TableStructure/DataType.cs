using Nounbase.Core.Models.Schema;
using System.Text.Json.Serialization;

namespace Nounbase.Services.SqlServer.Models.Source.TableStructure
{
    public class DataType
    {
        [JsonPropertyName("column_data_type")]
        public string? TypeName { get; set; }

        public string ToCoreDataType() => 
            TypeName?.ToLower() switch
            {
                "bit" => ColumnTypes.Boolean,
                "datetime" => ColumnTypes.DateTime,
                "datetime2" => ColumnTypes.DateTime,
                "smalldatetime" => ColumnTypes.DateTime,
                "int" => ColumnTypes.Number,
                "bigint" => ColumnTypes.Number,
                "decimal" => ColumnTypes.Number,
                "float" => ColumnTypes.Number,
                "money" => ColumnTypes.Number,
                "numeric" => ColumnTypes.Number,
                "real" => ColumnTypes.Number,
                "smallint" => ColumnTypes.Number,
                "smallmoney" => ColumnTypes.Number,
                "tinyint" => ColumnTypes.Number,
                "char" => ColumnTypes.String,
                "nchar" => ColumnTypes.String,
                "ntext" => ColumnTypes.String,
                "nvarchar" => ColumnTypes.String,
                "text" => ColumnTypes.String,
                "varchar" => ColumnTypes.String,
                "xml" => ColumnTypes.String,
                _ => ColumnTypes.Other
            };
    }
}
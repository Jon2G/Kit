using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;

namespace Kit.Services.Excel
{
    public class ExcelService : IDisposable
    {
        public string FilePath { get; private set; }
        public ClosedXML.Excel.XLWorkbook Document { get; private set; }

        public ExcelService(string FilePath, ClosedXML.Excel.XLWorkbook Document)
        {
            this.FilePath = FilePath;
            this.Document = Document;
        }

        public static ExcelService GenerateExcel(String fileName)
        {
            // Creating the SpreadsheetDocument in the indicated FilePath
            string FilePath = Path.Combine(Kit.Tools.Instance.LibraryPath, fileName);
            ExcelService xls = new(FilePath, new XLWorkbook());
            return xls;
        }

        public Cell ConstructCell(object value)
        {
            if (value is Formula formula)
            {
                Cell cell = new Cell();
                CellFormula cellformula = new CellFormula();
                cellformula.Text = formula.Text;
                CellValue cellValue = new CellValue();
                cellValue.Text = "0";
                cell.Append(cellformula);
                cell.Append(cellValue);
                return cell;
            }
            return ConstructCell(value.ToString(), GetType(value));
        }

        public CellValues GetType(object value)
        {
            switch (value)
            {
                case int:
                case float:
                case decimal:
                case double:
                    return CellValues.Number;

                case string:
                    return CellValues.String;

                case TimeSpan:
                case DateTime:
                    return CellValues.Date;

                case bool:
                    return CellValues.Boolean;

                default:
                    return CellValues.String;
            }
        }

        public Cell ConstructCell(string value, CellValues dataTypes) =>
            new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataTypes)
            };

        public CalculationCell ConstructCell(string formula) =>
             new CalculationCell()
             {
                 InnerXml = formula
             };

        public ExcelService AddWorkSheet(string Name = null, int SheetId = 0)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Name = $"Sheet{(Document.Worksheets?.Count ?? 0) + 1}";
            }
            var worksheet = Document.Worksheets.Add(Name, SheetId);
            return this;
        }

        public ExcelService InsertDataIntoSheet(string sheetName, ExcelStructureDataTable data)
        {
            Document.Worksheets.Add(data.Table, sheetName);
            var table = data.Table;
            return this;
        }

        public void Dispose()
        {
            Document.SaveAs(FilePath);
            this.Document.Dispose();
        }
    }
}
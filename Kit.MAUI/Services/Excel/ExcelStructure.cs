using System.Data;

namespace Kit.Services.Excel
{
    public abstract class ExcelStructure
    {
    }

    public class ExcelStructureDataTable : ExcelStructure
    {
        public DataTable Table { get; set; }

        public ExcelStructureDataTable(DataTable Table)
        {
            this.Table = Table;
        }
    }
}
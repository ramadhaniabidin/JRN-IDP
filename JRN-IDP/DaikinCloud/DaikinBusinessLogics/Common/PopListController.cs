using Daikin.BusinessLogics.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common
{
    public class PopListController
    {
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        private readonly DataTable dt = new DataTable();
        private readonly string ConnString = string.Empty;
        public PopListController(string _ConnString)
        {
            ConnString = _ConnString;
        }
        public List<dynamic> ListData(PopList_Input input, PopList_Output output)
        {
            db.OpenConnection(ref conn, ConnString, true);

            db.cmd.CommandText = "usp_NWC_PopUpListData";
            db.cmd.CommandType = CommandType.StoredProcedure;
            db.cmd.Parameters.Clear();

            db.AddInParameter(db.cmd, "searchTabl", input.searchTabl);
            db.AddInParameter(db.cmd, "searchCol", input.searchCol);
            db.AddInParameter(db.cmd, "searchVal", input.searchVal);
            db.AddInParameter(db.cmd, "searchLike", input.searchLike);
            db.AddInParameter(db.cmd, "pageIndx", input.pageIndx);
            db.AddInParameter(db.cmd, "pageSize", input.pageSize);
            db.AddOutParameter(db.cmd, "@out_resultCount", SqlDbType.Int);

            System.Diagnostics.Debug.WriteLine($"usp_NWC_PopUpListData param : \n {string.Join(", ", db.cmd.Parameters.Cast<SqlParameter>().Select(x => x.Value))}");

            reader = db.cmd.ExecuteReader();
            dt.Load(reader);

            List<dynamic> dynamicDt = dt.ToDynamic();

            output.RecordCount = Convert.ToInt32(db.cmd.Parameters["@out_resultCount"].Value);

            db.CloseDataReader(reader);
            db.CloseConnection(ref conn);
            return dt.Rows.Count > 0 ? dynamicDt : new List<dynamic>();
        }
    }

    public static class DataTableExtensions
    {
        public static List<dynamic> ToDynamic(this DataTable dt)
        {
            var dynamicDt = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add(dyn);
                foreach (DataColumn column in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
            }
            return dynamicDt;
        }
    }
}

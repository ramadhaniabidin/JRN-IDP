using Daikin.BusinessLogics.Apps.ScheduledPayment.Model;
using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.ScheduledPayment.Controller
{
    public class ScheduledPaymentController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();

        public List<ScheduledPaymentHeader> ListData_ScheduledPayment(FilterHeader model, out int RecordCount, out decimal GrandTotal)
        {
            dt = new DataTable();
            RecordCount = 0;
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.[usp_ScheduledPaymentList_ListData]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "SearchBy", model.SearchBy);
                db.AddInParameter(db.cmd, "Keywords", model.Keywords);
                db.AddInParameter(db.cmd, "PageIndex", model.PageIndex);
                db.AddInParameter(db.cmd, "PageSize", model.PageSize);
                db.AddInParameter(db.cmd, "Payment_Date_Start", model.Payment_Date_Start);
                db.AddInParameter(db.cmd, "Payment_Date_End", model.Payment_Date_End);
                db.AddInParameter(db.cmd, "BankName", model.BankName);

                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                db.AddOutParameter(db.cmd, "@GrandTotal", SqlDbType.Decimal);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);
                GrandTotal = Convert.ToDecimal(db.cmd.Parameters["@GrandTotal"].Value);
                db.CloseConnection(ref conn);
                return dt.Rows.Count > 0 ? Utility.ConvertDataTableToList<ScheduledPaymentHeader>(dt) : new List<ScheduledPaymentHeader>();
            }
            catch(Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
    }
}

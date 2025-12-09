using Daikin.BusinessLogics.Apps.Commercials.Model;
using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Commercial.Controller
{
    public class InvoiceCommercialNumberController
    {
        DataTable dt = new DataTable();
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;

        public List<ICNModel> ListCommercialNumber(int PageIndex, string Keywords, out int RecordCount)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_CostInbounds_GetList";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "PageIndex", PageIndex);
                db.AddInParameter(db.cmd, "Keywords", Keywords);
                db.AddOutParameter(db.cmd, "@RecordCount", SqlDbType.Int);
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);
                RecordCount = Convert.ToInt32(db.cmd.Parameters["@RecordCount"].Value);

                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<ICNModel>(dt);

            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }
        public List<InvoiceCommercialNumber> ListCommercialNumberOnly(string Keywords)
        {
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_CostInboundsInvoice_GetList";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Keywords", Keywords);
                reader = db.cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(reader);
                db.CloseDataReader(reader);

                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<InvoiceCommercialNumber>(dt);

            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
        }

    }
}

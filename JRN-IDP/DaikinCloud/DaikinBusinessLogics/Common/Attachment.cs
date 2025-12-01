using Daikin.BusinessLogics.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Daikin.BusinessLogics.Common
{
    public static class Attachment
    {
        static DatabaseManager db = new DatabaseManager();
        static SqlConnection conn = new SqlConnection();

        public static List<AttachmentBRModel> GetAttachmentSelfie(int Item_ID)
        {
            var dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_NWC_GetAttachmentSelfie";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Item_ID", Item_ID);

                SqlDataReader reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                var dataAttachment = Utility.ConvertDataTableToList<AttachmentBRModel>(dt);
                return dataAttachment;
            }
            catch (Exception)
            {
                db.CloseConnection(ref conn);
                throw;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Daikin.BusinessLogics.Apps.Batch.Model;
using System.IO;
using System.Net;
using Daikin.BusinessLogics.Common;

namespace Daikin.BusinessLogics.Apps.Batch.Controller {

    public class Batch2Controller {
        private readonly DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();

        public List<BatchModel> GetBatchFileContents(string transactionNo, bool isOpen = false) {
            dt = new DataTable();
            try {
                if (!isOpen)
                    db.OpenConnection(ref conn);

                db.cmd.CommandText = "usp_Utility_CreateBatchFile2";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Transaction_No", transactionNo);

                SqlDataReader reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                return Utility.ConvertDataTableToList<BatchModel>(dt);
            }
            catch (Exception ex) {
                throw ex;
            }
            finally {
                if (!isOpen)
                    db.CloseConnection(ref conn);
            }
        }

        public void SaveBatchFileHistory(string transctionNo, string currentUser, bool isOpen = false) {
            var isTrans = true;
            try {
                if (!isOpen)
                    db.OpenConnection(ref conn, isTrans);
                db.cmd.CommandText = "usp_BatchFile2History_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Transaction_No", transctionNo);
                db.AddInParameter(db.cmd, "Modified_By", currentUser);

                db.cmd.ExecuteNonQuery();
            }
            catch (Exception ex) {
                isTrans = false;
                throw ex;
            }
            finally {
                if (!isOpen)
                    db.CloseConnection(ref conn, isTrans);
            }
        }
    }
}

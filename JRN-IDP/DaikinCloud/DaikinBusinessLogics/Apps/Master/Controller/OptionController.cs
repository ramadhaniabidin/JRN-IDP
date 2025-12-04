using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Master.Controller
{
    public class OptionController
    {
        DatabaseManager db = new DatabaseManager();
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();

        public List<MasterModuleOptionModel> ModuleTransactionList()
        {
            //[usp_MasterModule_GetOptionsByTransaction]
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "dbo.[usp_MasterModule_GetOptionsByTransaction]";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);
                return Utility.ConvertDataTableToList<MasterModuleOptionModel>(dt);

            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }

        public List<MasterModuleOptionModel> ModuleOptions(string SPList)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_MasterModule_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "SPList", SPList);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                return Utility.ConvertDataTableToList<MasterModuleOptionModel>(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<OptionModel> GetOptions(string Table, string Code, string Name, string FilterBy, string FilterValue, string Extra)
        {
            dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_Utility_GetOptions";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "Table", Table);
                db.AddInParameter(db.cmd, "Code", Code);
                db.AddInParameter(db.cmd, "Name", Name);
                db.AddInParameter(db.cmd, "FilterBy", FilterBy);
                db.AddInParameter(db.cmd, "FilterValue", FilterValue);
                db.AddInParameter(db.cmd, "Extra", Extra);

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                return Utility.ConvertDataTableToList<OptionModel>(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<OptionModel> GetMasterRoleApproverCR()
        {
            try
            {
                List<OptionModel> listOption = new List<OptionModel>();
                dt = new DataTable();

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_MasterRoleApproverCR_GetList";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();

                reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                OptionModel data = new OptionModel();

                foreach (DataRow row in dt.Rows)
                {
                    data = new OptionModel();

                    data.Code = Utility.GetStringValue(row, "Name");
                    data.Name = Utility.GetStringValue(row, "Name");
                    listOption.Add(data);
                }

                return listOption.OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

    }
}
using Daikin.BusinessLogics.Apps.Master.Model;
using Daikin.BusinessLogics.Apps.Master.Repository;
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
        private readonly DatabaseManager db;
        SqlConnection conn = new SqlConnection();
        SqlDataReader reader = null;
        DataTable dt = new DataTable();
        private readonly string connString = Utility.GetSqlConnection();
        private readonly OptionRepository repo;

        public OptionController()
        {
            db = new DatabaseManager();
            repo = new OptionRepository(db, connString);
        }

        public List<MasterModuleOptionModel> ModuleTransactionList()
        {
            return repo.ModuleTransactionList().GetAwaiter().GetResult();
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
            finally
            {
                db.CloseConnection(ref conn);
            }
        }

        public List<BussPlaceModel> GetMasterBussPlace()
        {
            DataTable dTable = new DataTable();
            using (var con = new SqlConnection(connString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_Utility_GetMasterBussPlace", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        dTable.Load(reader);
                        return Utility.ConvertDataTableToList<BussPlaceModel>(dTable);
                    }
                }
            }
        }

        public async Task<List<BussPlaceModel>> GetMasterBussPlaceAsync()
        {
            List<BussPlaceModel> list = new List<BussPlaceModel>();
            using (SqlConnection _conn = new SqlConnection(connString))
            {
                await _conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("usp_Utility_GetMasterBussPlace", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync())
                    {
                        return await Utility.MapReaderToList<BussPlaceModel>(r).ConfigureAwait(false);
                    }
                }
            }
        }

        public List<OptionModel> GetMasterClaimCategory()
        {
            DataTable dTable = new DataTable();
            using (var con = new SqlConnection(connString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_Utility_GetMasterClaimCategory", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        dTable.Load(reader);
                        return Utility.ConvertDataTableToList<OptionModel>(dTable);
                    }
                }
            }
        }

        public List<VendorAffiliateModel> GetMasterVendorAffiliate()
        {
            DataTable dTable = new DataTable();
            using (var con = new SqlConnection(connString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_Utility_GetMasterVendorAffiliate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        dTable.Load(reader);
                        return Utility.ConvertDataTableToList<VendorAffiliateModel>(dTable);
                    }
                }
            }
        }

        public async Task<List<VendorAffiliateModel>> GetMasterVendorAffiliateAsync()
        {
            using (SqlConnection _conn = new SqlConnection(connString))
            {
                await _conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("usp_Utility_GetMasterVendorAffiliate", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync())
                    {
                        return await Utility.MapReaderToList<VendorAffiliateModel>(r);
                    }
                }
            }
        }

        public List<VendorBankAffiliateModel> GetMasterVendorBankAffiliate()
        {
            DataTable dTable = new DataTable();
            using (var con = new SqlConnection(connString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_GetVendorBankAffiliate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        dTable.Load(reader);
                        return Utility.ConvertDataTableToList<VendorBankAffiliateModel>(dTable);
                    }
                }
            }
        }

        public async Task<List<VendorBankAffiliateModel>> GerMasterVendorBankAffiliateAsync()
        {
            using (SqlConnection _conn = new SqlConnection(connString))
            {
                await _conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("usp_GetVendorBankAffiliate", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync())
                    {
                        return await Utility.MapReaderToList<VendorBankAffiliateModel>(r);
                    }
                }
            }
        }

        public List<VendorAffiliateModel> GetMasterCustomerAffiliate()
        {
            DataTable dTable = new DataTable();
            using (var con = new SqlConnection(connString))
            {
                con.Open();
                using (var cmd = new SqlCommand("usp_Utility_GetMasterCustomerAffiliate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        dTable.Load(reader);
                        return Utility.ConvertDataTableToList<VendorAffiliateModel>(dTable);
                    }
                }
            }
        }

        public async Task<List<VendorAffiliateModel>> GetMasterCustomerAffiliateAsync()
        {
            using (SqlConnection _conn = new SqlConnection(connString))
            {
                await _conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("usp_Utility_GetMasterCustomerAffiliate", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader r = await cmd.ExecuteReaderAsync())
                    {
                        return await Utility.MapReaderToList<VendorAffiliateModel>(r);
                    }
                }
            }
        }

    }
}
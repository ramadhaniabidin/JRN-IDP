using Daikin.BusinessLogics.Apps.NonCommercials.Model;
using Daikin.BusinessLogics.Common;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.NonCommercials
{
    public class ContractRepository
    {
        private readonly DatabaseManager db = new DatabaseManager();
        private readonly string connString = Utility.GetSqlConnection();
        private readonly string dateFormat = "yyMM";

        public async Task<List<MasterUserProcDept>> GetBranchesAsync()
        {
            var list = new List<MasterUserProcDept>();
            using(var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                using(var cmd = new SqlCommand("dbo.usp_MasterUserProcDept_GetBranch", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(db.AddInParameter("Title", SPContext.Current.Web.CurrentUser.LoginName));
                    using(var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        list.Add(new MasterUserProcDept
                        {
                            Code = reader.GetString(reader.GetOrdinal("Branch_Title")),
                            Name = reader.GetString(reader.GetOrdinal("Branch_Title")),
                            Branch_ID = reader.GetInt32(reader.GetOrdinal("Branch_ID")),
                            Branch_Title = reader.GetString(reader.GetOrdinal("Branch_Title")),
                            Branch_Code = reader.GetString(reader.GetOrdinal("Branch_Code")),
                            Branch_BusinessArea = reader.GetString(reader.GetOrdinal("Branch_BusinessArea"))
                        });
                    }
                    return list;
                }
            }
        }

        public async Task<string> GetDataHeaderFormNo(string tableName, string code)
        {
            using(var con = new SqlConnection(connString))
            {
                await con.OpenAsync().ConfigureAwait(false);
                string d = DateTime.Now.ToString(dateFormat);
                int c = 1;
                string n = c.ToString().PadLeft(4, '0');

                string formNo = await db.AutoCounterAsync("Form_No", tableName, "Form_No", d, 4).ConfigureAwait(false);
                return (string.IsNullOrEmpty(formNo)) ? (code + d + n) : formNo;
            }
        }






    }
}

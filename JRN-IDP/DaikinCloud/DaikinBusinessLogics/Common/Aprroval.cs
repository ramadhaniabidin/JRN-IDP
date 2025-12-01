using Daikin.BusinessLogics.Common.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class Aprroval
    {
        static DatabaseManager db = new DatabaseManager();
        static SqlConnection conn = new SqlConnection();
        public static string GetToken()
        {
            string url = ConfigurationManager.AppSettings["NAC_TOKEN_URL"];

            HttpClient client = new HttpClient();
            var requestBody = new
            {
                client_id = "bd3797e5-2c76-4834-90f1-70060fe10844",
                client_secret = "sKtUsKRNNtWsJLROQtUsPMtWVsMNtRTsLOtWRWsPtVsRtRsNtSPsMLItT2IsRtVsFRtTsNtUsFMOtUsOFtRsQRJFtT2utUsItRsOtSVsO2N",
                grant_type = "client_credentials"
            };
            var jsonBody = new JavaScriptSerializer().Serialize(requestBody);
            var HttpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = client.PostAsync(url, HttpContent).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
            Console.WriteLine(responseObject);
            string accessToken = responseObject["access_token"];

            return accessToken;
        }

        public static List<ListDataID> GetListDataID(string ListName, int ItemID, string currentLoginName)
        {
            DataTable dt = new DataTable();
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_NWC_GetDataByItemID";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListName", ListName);
                db.AddInParameter(db.cmd, "ItemID", ItemID);
                db.AddInParameter(db.cmd, "CurrenLoginName", currentLoginName);
                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);

                var list = Utility.ConvertDataTableToList<ListDataID>(dt);
                return list;
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

        public static IEnumerable<dynamic> GetTasks(string token, string NAC_Guid)
        {
            string url = $"{ConfigurationManager.AppSettings["NAC_TASKS_URL"]}&workflowInstanceId={NAC_Guid}";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = client.GetAsync(url);
            var responseJson = response.Result.Content.ReadAsStringAsync().Result;
            dynamic responseObject = new JavaScriptSerializer().Deserialize<dynamic>(responseJson);
            var tasks = responseObject["tasks"];
            return tasks;
        }

        public async Task StartNWC(NintexWorkflowCloud nwc)
        {

            string sBody = new JavaScriptSerializer().Serialize(nwc.param);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(nwc.url);
            string token = Aprroval.GetToken();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT Header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, nwc.url);

            request.Content = new StringContent(sBody, Encoding.UTF8, "application/json");

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                //return result; //instance guid
            }

        }

        public static List<ListDataID> GetListDataIDByHeaderID(string ListName, int ID)
        {
            DataTable dt = new DataTable();
            db.CloseConnection(ref conn);
            try
            {
                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_NWC_GetDataByHeaderID";
                db.cmd.CommandType = CommandType.StoredProcedure;

                db.cmd.Parameters.Clear();
                db.AddInParameter(db.cmd, "ListName", ListName);
                db.AddInParameter(db.cmd, "HeaderID", ID);
                var reader = db.cmd.ExecuteReader();
                dt.Load(reader);
                db.CloseDataReader(reader);
                db.CloseConnection(ref conn);

                var list = Utility.ConvertDataTableToList<ListDataID>(dt);
                return list;
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
        public static List<ListDataID> GetListDataID_New(string ListName, int ItemID)
        {
            DataTable dt = new DataTable();
            string connString = Utility.GetSqlConnection();
            try
            {
                using(SqlConnection connection = new SqlConnection(connString))
                {
                    connection.Open();
                    using(SqlCommand command = new SqlCommand("usp_NWC_GetDataByItemID", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@ListName", ListName);
                        command.Parameters.AddWithValue("@ItemID", ItemID);
                        using(SqlDataReader dataReader = command.ExecuteReader())
                        {
                            dt.Load(dataReader);
                        }
                    }
                }
                var list = Utility.ConvertDataTableToList<ListDataID>(dt);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<ListDataID> GetListDataIDByHeaderID_New(string ListName, int ID)
        {
            DataTable dt = new DataTable();
            string connString = Utility.GetSqlConnection();
            try
            {
                using(SqlConnection _conn = new SqlConnection(connString))
                {
                    _conn.Open();
                    using(SqlCommand cmd = new SqlCommand("usp_NWC_GetDataByHeaderID", _conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@ListName", ListName);
                        cmd.Parameters.AddWithValue("@HeaderID", ID);

                        using (SqlDataReader dataReader = cmd.ExecuteReader())
                        {
                            dt.Load(dataReader);
                        }
                    }
                }
                var list = Utility.ConvertDataTableToList<ListDataID>(dt);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

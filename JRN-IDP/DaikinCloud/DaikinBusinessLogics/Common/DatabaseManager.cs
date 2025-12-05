using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Common
{
    public class DatabaseManager
    {
        public SqlCommand cmd;
        public SqlDataReader dReader;
        public SqlTransaction trans;
        public string GetSQLConnectionString()
        {
            return Utility.GetSqlConnection();
        }

        public void OpenConnection(ref SqlConnection connection, string ConnString, bool IsTrans = false)
        {
            if(connection == null || connection.State == ConnectionState.Closed)
            {
                connection = new SqlConnection(ConnString);
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandTimeout = 0;
                if (IsTrans)
                {
                    trans = connection.BeginTransaction();
                    cmd.Transaction = trans;
                }
            }
        }

        public void OpenConnection(ref SqlConnection connection, bool IsTrans = false)
        {
            if(connection == null || connection.State == ConnectionState.Closed)
            {
                connection = new SqlConnection(GetSQLConnectionString());
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandTimeout = 0;
                if (IsTrans)
                {
                    trans = connection.BeginTransaction();
                    cmd.Transaction = trans;
                }
            }
        }
        public void CloseConnection(ref SqlConnection connection, bool IsTrans = false)
        {
            if (connection.State == ConnectionState.Open)
            {
                if (IsTrans)
                {
                    trans.Commit();
                }
                connection.Close();
            }
        }

        public void ClearParameter(SqlCommand command)
        {
            command.Parameters.Clear();
        }
        public void ChangeParameter(SqlCommand command, string name, object value)
        {
            command.Parameters[name].Value = value;
        }
        public void AddInParameter(SqlCommand command, string name, object value)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            parameter.Direction = ParameterDirection.Input;

            command.Parameters.Add(parameter);
        }
        public void AddOutParameter(SqlCommand command, string name, SqlDbType type)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = name;
            parameter.SqlDbType = type;
            parameter.Direction = ParameterDirection.Output;

            command.Parameters.Add(parameter);
        }

        public void CloseDataReader(SqlDataReader dataReader)
        {
            if (dataReader == null)
                return;
            dataReader.Close();
            dataReader.Dispose();
        }

        public bool isRecordExists(string SPName, string ParamName, string ParamValue)
        {
            SqlDataReader dr = null;
            cmd.CommandText = SPName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue(ParamName, ParamValue);
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                CloseDataReader(dr);
                return true;
            }
            else
            {
                CloseDataReader(dr);
                return false;
            }
        }
        public bool isRecordExists(string query)
        {
            SqlDataReader dr = null;
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                CloseDataReader(dr);
                return true;
            }
            else
            {
                CloseDataReader(dr);
                return false;
            }
        }

        public string Autocounter(string fieldName, string TableName, string fieldCriteria, string valueCriteria, int LengthOfString)
        {
            string autoCode = "";
            using (var con = new SqlConnection(GetSQLConnectionString()))
            {
                con.Open();
                using (var command = new SqlCommand("[usp_Utility_AutoCounter]", con))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter { ParameterName = "FieldName", Value = fieldName, Direction = ParameterDirection.Input });
                    command.Parameters.Add(new SqlParameter { ParameterName = "TableName", Value = TableName, Direction = ParameterDirection.Input });
                    command.Parameters.Add(new SqlParameter { ParameterName = "FieldCriteria", Value = fieldCriteria, Direction = ParameterDirection.Input });
                    command.Parameters.Add(new SqlParameter { ParameterName = "ValueCriteria", Value = valueCriteria, Direction = ParameterDirection.Input });
                    command.Parameters.Add(new SqlParameter { ParameterName = "LengthOfString", Value = LengthOfString, Direction = ParameterDirection.Input });
                    autoCode = cmd.ExecuteScalar().ToString();
                    return autoCode;
                }
            }
        }


        public DataTable GetValueFromSP(string SPName, string ParamName, string ParamValue)
        {
            DataTable dt = new DataTable();
            cmd.CommandText = SPName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            AddInParameter(cmd, ParamName, ParamValue);
            dReader = cmd.ExecuteReader();
            dt.Load(dReader);
            CloseDataReader(dReader);
            return dt;
        }
        public string GetValueFromQuery(string Query, string Field_Name)
        {
            string result = "";
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = Query;
            dReader = cmd.ExecuteReader();
            while (dReader.Read())
            {
                result = dReader[Field_Name].ToString();
            }
            CloseDataReader(dReader);
            return result;
        }
    }
}
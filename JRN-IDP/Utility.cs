using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JRN_IDP
{
    public class Utility
    {
        public static string DecryptString(string encrString)
        {
            byte[] b;
            string decrypted;
            try
            {
                b = Convert.FromBase64String(encrString);
                decrypted = System.Text.ASCIIEncoding.ASCII.GetString(b);
            }
            catch (FormatException fe)
            {
                decrypted = fe.Message;
            }
            return decrypted;
        }

        public static string EnryptString(string strEncrypted)
        {
            byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(strEncrypted);
            string encrypted = Convert.ToBase64String(b);
            return encrypted;
        }

        public static string ConvertDataTableToJSON(DataTable table)
        {
            var JSONString = new StringBuilder();
            if (table.Rows.Count > 0)
            {
                JSONString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JSONString.Append("{");
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j < table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JSONString.Append("}");
                    }
                    else
                    {
                        JSONString.Append("},");
                    }
                }
                JSONString.Append("]");
            }
            return JSONString.ToString();
        }

        public static string GetParentUriString(Uri uri)
        {
            return uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);
        }

        public static string GetStringValue(DataRow value, string key)
        {
            string result = value[key].ToString();
            try
            {
                return string.IsNullOrEmpty(result) ? "" : result;
            }
            catch
            {
                return string.IsNullOrEmpty(result) ? "" : result;
            }
        }

        public static int GetIntValue(DataRow value, string key)
        {
            try
            {
                string val = value[key].ToString().Split('.')[0];
                return int.Parse(val);
            }
            catch
            {
                return 0;
            }
        }

        public static decimal GetDecimalValue(DataRow value, string key)
        {
            try
            {
                string val = value[key].ToString();
                return Convert.ToDecimal(val);
            }
            catch
            {
                return 0;
            }
        }

        public static DateTime GetDateValue(DataRow value, string key)
        {
            DateTime date = new DateTime();
            try
            {
                return DateTime.Parse(value[key].ToString());
            }
            catch
            {
                return date;
            }
        }

        public static string StripHTML(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", String.Empty).Replace("&nbsp;", " ").Replace("&amp;", "&");
        }

        public static string GetUntilOrEmpty(string text, string stopAt = "", string orStopAt = "")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);
                int charLocation2 = text.IndexOf(orStopAt, StringComparison.Ordinal);


                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

        public static DataTable ToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static string ToHtmlTable(DataTable dt)
        {
            string strHtml = "<table><tr>" + Environment.NewLine;
            foreach (DataColumn col in dt.Columns)
            {
                strHtml += Environment.NewLine + "<th>" + col.ColumnName + "</th>";
            }
            strHtml += Environment.NewLine + "</tr>";
            foreach (DataColumn dc in dt.Columns)
            {
                strHtml += Environment.NewLine + "<tr>";
                foreach (DataRow row in dt.Rows)
                {
                    strHtml += Environment.NewLine + "<td>" + row[dc] + "</td>";
                }
                strHtml += Environment.NewLine + "</tr>";
            }
            strHtml += "</table>";
            return strHtml;
        }

        public static List<T> ConvertDataTableToList<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        public static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    var value = dr[column.ColumnName] == DBNull.Value ? null : dr[column.ColumnName];
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, value, null);
                    else
                        continue;
                }
            }
            return obj;
        }
    }
}

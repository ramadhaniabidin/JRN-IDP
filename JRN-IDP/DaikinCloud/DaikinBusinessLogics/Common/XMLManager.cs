using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Daikin.BusinessLogics.Common
{
    public class DefaultAttrModel
    {
        public int id { get; set; }
        public int item_id { get; set; }
        public string code { get; set; }
        public string TableName { get; set; }
        public string SPColumnName { get; set; }
        public string XMLString { get; set; }
    }
    public class XMLManager
    {
        public static string GetValue(XElement item, string Attribute)
        {
            try
            {
                return item.Element(Attribute).Value;
            }
            catch
            {
                return "NOT EXISTS";
            }
        }

        public static void AddAttribute(XElement item, string Attribute, string Value)
        {
            item.Add(new XElement(Attribute, Value));
        }

        public static List<string> ConvertDataTableToSQL(DataTable dtAttributesFilter, DefaultAttrModel attr, string vendorName)
        {
            string currProcess = "";
            string currAtt = "";
            try
            {
                List<string> listQuery = new List<string>();
                string queryValues = string.Empty;
                string fullQuery = string.Empty;
                string queryInit = "";

                List<string> dbFields = new List<string>();
                List<string> updateValues = new List<string>();

                currProcess = "Get DB Columng Mapping";
                foreach (DataRow rowAttr in dtAttributesFilter.Rows)
                {
                    string dbColumnName = Utility.GetStringValue(rowAttr, "Database_Column");
                    dbFields.Add(dbColumnName);
                }
                XElement root = XElement.Parse(attr.XMLString);

                //loop every single item in detail Repeating Section
                foreach (var item in root.Descendants("Item"))
                {
                    //collect all attributes from xml RS
                    currProcess = "Collect all attributes value from XML";
                    List<string> xmlElementValues = new List<string>();
                    foreach (DataRow rowAttr in dtAttributesFilter.Rows)
                    {
                        currAtt = Utility.GetStringValue(rowAttr, "Sharepoint_Column");
                        string val = String.IsNullOrEmpty(Utility.GetStringValue(rowAttr, "Sharepoint_Column")) ? "NULL" :
                                                          Utility.GetStringValue(rowAttr, "Sharepoint_Column");
                        if (attr.TableName == "UnloadingFeeDetail" && val == "Description")
                        {
                            xmlElementValues.Add("'UNLOADING FEE-" + vendorName.ToUpper() + "'");
                        }
                        else
                        {
                            xmlElementValues.Add("'" + item.Element(val).Value + "'");                            
                        };
                    }

                    #region Collect SQL Queries
                    queryInit = "Insert into " + attr.TableName + " (";
                    queryInit += string.Join(",", dbFields);
                    queryInit += ")";

                    queryValues = " VALUES (";
                    queryValues += string.Join(",", xmlElementValues);
                    queryValues += ")";

                    fullQuery = queryInit + queryValues;
                    listQuery.Add(fullQuery);
                    #endregion


                }


                return listQuery;
            }
            catch (Exception ex)
            {
                Exception err = new Exception(ex.Message + " | Error at " + currProcess + (currProcess == "Collect all attributes value from XML" ? " at Attribute " + currAtt : "."), ex);
                throw err;
            }

        }
        public static DataTable ConvertXMLRepeatingSectionToDataTable(string xmlString)
        {
            var doc = XDocument.Parse(xmlString);
            var result = doc.Descendants("Item");
            IEnumerable<XElement> data = result.ToList();
            var table = new DataTable();
            // create the columns
            foreach (var xe in data.First().Descendants())
                table.Columns.Add(xe.Name.LocalName, typeof(string));
            // fill the data
            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var xe in item.Descendants())
                    row[xe.Name.LocalName] = xe.Value;
                table.Rows.Add(row);
            }
            return table;
        }
        public void ReadXML(string xmlString, DataTable dtColumns)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            string xpath = "RepeaterData/Items/Item";
            var nodes = xmlDoc.SelectNodes(xpath);

            foreach (XmlNode childrenNode in nodes)
            {
                foreach (DataRow col in dtColumns.Rows)
                {
                    Console.WriteLine(
                        "No: " + childrenNode.SelectSingleNode(".//No").InnerText +
                        ", Item Text: " + childrenNode.SelectSingleNode(".//Item_Text").InnerText);
                }
            }
        }
    }
}

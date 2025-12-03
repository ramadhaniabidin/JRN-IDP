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
        //        var xml = @"<?xml version='1.0' encoding='utf-8'?><RepeaterData><Version/><Items><Item><No type='System.String'>1</No><Item_Text type='System.String'>BASIC SALARY SECURITY DID BEKASI</Item_Text><ddl_MaterialAnaplan type='System.String'>ZGA010001007</ddl_MaterialAnaplan><GL type='System.String'>TEMPORARY STAFFING EXPENSE</GL><ddl_CostCenter type='System.String'>BKS0300500 - General Affair </ddl_CostCenter><Tax_Type type='System.String'>Non WHT</Tax_Type><Qty type='System.String'>3</Qty><Unit_Price type='System.Double'>0</Unit_Price><Currency type='System.String'>IDR</Currency><cvAmount type='System.Double'>17478237.0000</cvAmount><cost_center_code type='System.String'>BKS0300500 - General Affair </cost_center_code><Material_Anaplan_Name type='System.String'>ZGA010001007 Basic Salary (outsourcing)</Material_Anaplan_Name><material_anaplan_code type='System.String'>ZGA010001007</material_anaplan_code><Amount type='System.String'>17478237.0000</Amount></Item><Item><No type='System.String'>2</No><Item_Text type='System.String'>MANAGEMENT FEE</Item_Text><ddl_MaterialAnaplan type='System.String'>ZGA010001010</ddl_MaterialAnaplan><GL type='System.String'>TEMPORARY STAFFING EXPENSE</GL><ddl_CostCenter type='System.String'>BKS0300500 - General Affair </ddl_CostCenter><Tax_Type type='System.String'>WHT</Tax_Type><Qty type='System.String'>1</Qty><Unit_Price type='System.Double'>1</Unit_Price><Currency type='System.String'>IDR</Currency><cvAmount type='System.Double'>1747824.0000</cvAmount><cost_center_code type='System.String'>BKS0300500 - General Affair </cost_center_code><Material_Anaplan_Name type='System.String'>ZGA010001010 Management Fee (outsourcing)</Material_Anaplan_Name><material_anaplan_code type='System.String'>ZGA010001010</material_anaplan_code><Amount type='System.String'>1747824.0000</Amount></Item></Items></RepeaterData>";
        //        var doc = XDocument.Parse(xml);
        //        var result = doc.Descendants("Item");
        //        var data = result.ToList();
        //        var table = ConvertToDataTable(data);
        //		foreach(DataRow row in table.Rows){
        //			foreach(DataColumn col in table.Columns)
        //			{
        //				Console.WriteLine(col + " : " + row[col.ColumnName].ToString());
        //			}
        //}

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
            try
            {
                item.Add(new XElement(Attribute, Value));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<string> ConvertDataTableToSQL(DataTable dtAttributesFilter, DefaultAttrModel attr, string vendorName)
        {
            string currProcess = "";
            string currAtt = "";
            try
            {
                //DataTable dtSources = ConvertXMLRepeatingSectionToDataTable(attr.XMLString);
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
            //Sample
            //        xmlString =
            //"<?xml version='1.0' encoding='utf-8'?><RepeaterData><Version/><Items><Item><No type='System.String'>1</No><Item_Text type='System.String'>BASIC SALARY SECURITY DID BEKASI</Item_Text><ddl_MaterialAnaplan type='System.String'>ZGA010001007</ddl_MaterialAnaplan><GL type='System.String'>TEMPORARY STAFFING EXPENSE</GL><ddl_CostCenter type='System.String'>BKS0300500 - General Affair </ddl_CostCenter><Tax_Type type='System.String'>Non WHT</Tax_Type><Qty type='System.String'>3</Qty><Unit_Price type='System.Double'>0</Unit_Price><Currency type='System.String'>IDR</Currency><cvAmount type='System.Double'>17478237.0000</cvAmount><cost_center_code type='System.String'>BKS0300500 - General Affair </cost_center_code><Material_Anaplan_Name type='System.String'>ZGA010001007 Basic Salary (outsourcing)</Material_Anaplan_Name><material_anaplan_code type='System.String'>ZGA010001007</material_anaplan_code><Amount type='System.String'>17478237.0000</Amount></Item><Item><No type='System.String'>2</No><Item_Text type='System.String'>MANAGEMENT FEE</Item_Text><ddl_MaterialAnaplan type='System.String'>ZGA010001010</ddl_MaterialAnaplan><GL type='System.String'>TEMPORARY STAFFING EXPENSE</GL><ddl_CostCenter type='System.String'>BKS0300500 - General Affair </ddl_CostCenter><Tax_Type type='System.String'>WHT</Tax_Type><Qty type='System.String'>1</Qty><Unit_Price type='System.Double'>1</Unit_Price><Currency type='System.String'>IDR</Currency><cvAmount type='System.Double'>1747824.0000</cvAmount><cost_center_code type='System.String'>BKS0300500 - General Affair </cost_center_code><Material_Anaplan_Name type='System.String'>ZGA010001010 Management Fee (outsourcing)</Material_Anaplan_Name><material_anaplan_code type='System.String'>ZGA010001010</material_anaplan_code><Amount type='System.String'>1747824.0000</Amount></Item></Items></RepeaterData>";

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

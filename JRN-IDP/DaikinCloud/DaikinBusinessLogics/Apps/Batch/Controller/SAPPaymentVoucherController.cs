using Daikin.BusinessLogics.Apps.Batch.Model;
using Daikin.BusinessLogics.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daikin.BusinessLogics.Apps.Batch.Controller
{
    public class SAPPaymentVoucherController
    {
        public static int IDX_NINTEX_NO = 0;
        public static int IDX_MIRO_NO = 1;
        public static int IDX_PAYMENT_DATE = 2;
        public static int IDX_PAYMENT_REFF_NO = 3;
        public static int IDX_PAYMENT_NO = 4;
        public static int IDX_DOC_PAYMENT_NO = 5;

        public static Utility util = new Utility();
        public static DatabaseManager db = new DatabaseManager();
        public static void Save(SAPPaymentVoucherModel data)
        {
            SqlConnection conn = new SqlConnection();
            try
            {

                db.OpenConnection(ref conn);
                db.cmd.CommandText = "usp_BatchFile3History_Save";
                db.cmd.CommandType = CommandType.StoredProcedure;
                db.cmd.Parameters.Clear();

                db.AddInParameter(db.cmd, "Nintex_No", data.Nintex_No);
                db.AddInParameter(db.cmd, "MIRO_No", data.MIRO_No);
                db.AddInParameter(db.cmd, "Payment_Date", data.Payment_Date);
                db.AddInParameter(db.cmd, "Payment_No", data.Payment_No);
                db.AddInParameter(db.cmd, "Payment_Reff_No", data.Payment_Reff_No);
                db.AddInParameter(db.cmd, "Doc_Payment_No", data.Doc_Payment_No);
                db.AddInParameter(db.cmd, "Source_File", data.Source_File);

                db.cmd.ExecuteNonQuery();

                db.CloseConnection(ref conn);
            }
            catch (Exception ex)
            {
                db.CloseConnection(ref conn);
                throw ex;
            }
        }
        public static void ReadAllLines()
        {
            try
            {
                string folder = util.GetConfigValue("NetworkPath");
                folder += @"\Batch 3";
                string Nintex_No = "";
                foreach (string file in Directory.EnumerateFiles(folder, "*.txt"))
                {
                    string file_name = Path.GetFileName(file);
                    try
                    {
                        string[] lines = File.ReadAllLines(file);
                        foreach (string line in lines)
                        {
                            string[] split_data = line.Split(';');
                            try
                            {
                                #region Read & Save
                                SAPPaymentVoucherModel data = new SAPPaymentVoucherModel();
                                data.Doc_Payment_No = split_data[IDX_DOC_PAYMENT_NO];
                                data.MIRO_No = split_data[IDX_MIRO_NO];
                                data.Payment_Date = split_data[IDX_PAYMENT_DATE];
                                data.Payment_No = split_data[IDX_PAYMENT_NO];
                                data.Payment_Reff_No = split_data[IDX_PAYMENT_REFF_NO];
                                data.Nintex_No = split_data[IDX_NINTEX_NO];
                                data.Source_File = file_name;
                                Nintex_No = data.Nintex_No;
                                if (!string.IsNullOrEmpty(data.Payment_No))
                                {
                                    Save(data);
                                }
                                #endregion

                                Utility.SaveLog("SAP Payment Voucher", split_data[IDX_NINTEX_NO], file, "", 1);
                                Console.WriteLine(line);
                            }
                            catch (Exception ex)
                            {
                                Utility.SaveLog("SAP Payment Voucher", split_data[IDX_NINTEX_NO], file, ex.Message, 0);
                            }


                        }

                        System.IO.File.Move(folder + "\\" + file_name, folder + "\\DONE\\" + file_name);

                    }
                    catch (Exception ex)
                    {
                        Utility.SaveLog("SAP Payment Voucher", Nintex_No, file, ex.Message, 0);
                        System.IO.File.Move(folder + "\\" + file_name, folder + "\\ERROR\\" + file_name);
                    }

                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("SAP Payment Voucher - ReadAllLines", "", "", ex.Message, 0);
            }
        }
    }
}

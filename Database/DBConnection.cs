﻿using QuanLyQuanCaPhe.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLyQuanCaPhe.Database
{
    public class DBConnection
    {
        // Tạo một đối tượng kết nối Database 

        public static SqlConnection conn = null;
        //public static SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=QuanLyQuanCaPhe;");
        //public static SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-IR97G8JC;Initial Catalog=QuanLyQuanCaPhe;User Id=" + GLOBAL.tenDangNhap + ";Password=" + GLOBAL.matKhau+ ";");
        // public static SqlConnection conn = new SqlConnection(@"Data Source=LAPTOP-KN9ENH3A\SQLEXPRESS01;Initial Catalog=QuanLyQuanCaPhe;Persist Security Info=True;" + "User ID=" + GLOBAL.tenDangNhap + ";Password=" + GLOBAL.matKhau + ";");
        
        
        // List tham số truyền vào proc và function
        public static List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
        public static void ClearParameters()
        {
            parameters.Clear();
        }

        public static void AddParameters(string key, object value)
        {
            parameters.Add(new KeyValuePair<string, object>(key, value));
        }

        public DBConnection() 
        {
            
        }

        public static DataTable LoadTableVaView(string tenTable)
        {
            try
            {
                moKetNoi();
                string truyVan = string.Format($"SELECT * FROM {tenTable}");

                SqlDataAdapter adapter = new SqlDataAdapter(truyVan, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                dongKetNoi();
            }
        }

        public static object ThucThiFunction_Scalar(string tenFunction, List<KeyValuePair<string, object>> parameters)
        {
            try
            {
                moKetNoi();

                string truyVan = $@"SELECT dbo.{tenFunction}(";
                for (int i = 0; i < parameters.Count; i++)
                {
                    truyVan += parameters[i].Key;
                    if (i < parameters.Count - 1)
                    {
                        truyVan += ", ";
                    }
                }
                truyVan += ")";

                SqlCommand cmd = new SqlCommand(truyVan, conn);

                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thất bại\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                dongKetNoi();
            }
        }

        public static DataTable ThucThiFunction_InlineVaMultiStatement(string tenFunction, List<KeyValuePair<string, object>> parameters)
        {
            try
            {
                moKetNoi();
                string truyVan = $@"SELECT * FROM {tenFunction}(";
                for (int i = 0; i < parameters.Count; i++)
                {
                    truyVan += parameters[i].Key;
                    if (i < parameters.Count - 1)
                    {
                        truyVan += ", ";
                    }
                    else
                    {
                        truyVan += ")";
                    }    
                }

                //MessageBox.Show(truyVan);

                SqlCommand cmd = new SqlCommand(truyVan, conn);

                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thất bại\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                dongKetNoi();
            }
        }


        public static Dictionary<string, object> ThucThiProc_CoThamSoOutput(string tenProc, List<KeyValuePair<string, object>> parameters, List<SqlParameter> outputParams)
        {
            try
            {
                moKetNoi();
                SqlCommand cmd = new SqlCommand(tenProc, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Thêm các tham số đầu vào
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                // Thêm các tham số OUTPUT
                foreach (var outputParam in outputParams)
                {
                    cmd.Parameters.Add(outputParam);
                }

                // Thực thi stored procedure
                cmd.ExecuteNonQuery();

                // Lấy giá trị của tất cả các tham số OUTPUT
                Dictionary<string, object> result = new Dictionary<string, object>();
                foreach (var outputParam in outputParams)
                {
                    result.Add(outputParam.ParameterName, cmd.Parameters[outputParam.ParameterName].Value);
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thất bại\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                dongKetNoi();
            }
        }

        public static void ThucThiProc_CoThamSoVaKhongCoThamSo(string tenProc, List<KeyValuePair<string, object>> parameters)  
        {
            try
            {
                moKetNoi();
                SqlCommand cmd = new SqlCommand(tenProc, conn);

                cmd.CommandType = CommandType.StoredProcedure;

                // Kiểm tra nếu có tham số
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var param in parameters)
                    {
                        if (param.Value.Equals(""))
                        {
                            cmd.Parameters.AddWithValue(param.Key, null);

                        }
                        else
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }
                }

                cmd.ExecuteNonQuery();
                //if (cmd.ExecuteNonQuery() > 0)
                //MessageBox.Show("Thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thất bại\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dongKetNoi();
            }
        }

        public static SqlDataReader ThucThiProc_CoReader(string tenProc, List<KeyValuePair<string, object>> parameters, bool traVeReader = false)
        {
            SqlDataReader reader = null;
            try
            {
                moKetNoi();
                SqlCommand cmd = new SqlCommand(tenProc, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Thêm tham số vào cmd
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                if (traVeReader)
                {
                    // Trả về SqlDataReader nếu yêu cầu
                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                else
                {
                    // Thực thi không trả về dữ liệu
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thất bại\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return reader;
        }
         
        public static void moKetNoi()
        {
            try
            {
                conn = new SqlConnection(@"Data Source=LAPTOP-0BAFMQ9R\VANTRI;Initial Catalog=QuanLyQuanCaPhe;User Id=" + GLOBAL.tenDangNhap + ";Password=" + GLOBAL.matKhau + ";");
                //conn = new SqlConnection(@"Data Source=LAPTOP-IR97G8JC;Initial Catalog=QuanLyQuanCaPhe;User Id=" + GLOBAL.tenDangNhap + ";Password=" + GLOBAL.matKhau + ";");
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } 
        }

        public static void dongKetNoi()
        {
            if(conn.State == ConnectionState.Open && conn != null)
            {
                conn.Close();
            }
            else
            {
                MessageBox.Show("Chua ket noi");
            }
        }

    }
}

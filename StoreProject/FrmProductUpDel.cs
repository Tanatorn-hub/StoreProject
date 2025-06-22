using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreProject
{
    public partial class FrmProductUpDel : Form
    {

        //สร้างตัวแปรเก็บ ProId ที่ส่งมาจาก  FrmProductShow
        int proId;

        //สร้างตัวแปรเก็บรูปเป็น Binary/Byte Array เอาไว้บันทึกลง DB
        byte[] proImage;


        public FrmProductUpDel(int proId)
        {
            InitializeComponent();
            this.proId = proId;
        }
        //เมธอด แปลง binary  เป็นรูป  
        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }

        //สร้างเมธอดแสดงข้อความเตือน
        private void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //สร้างเมธอดแปลงรูปเป็น Binary/Byte Array
        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }


        private void FrmProductUpDel_Load(object sender, EventArgs e)
        {
            // เอา proId ที่ส่งมาไปค้นหาใน db แล้วเอามาแสดงที่หน้า FrmProductUpDel เพื่อแก้ไขฝลบ ต่อไป
            //กำหนด connect String เพื่อติดต่อไปยังฐานข้อมูล
            string connectionstring = @"server=DESKTOP-6F6L1NQ\SQLEXPRESS2022;Database=store_db;Trusted_Connection=True;";

            // สร้าง connection ไปยังฐานข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
            {
                try
                {
                    sqlConnection.Open(); //เปิดการเชื่อมต่อไปยังฐานข้อมูล
                    // สร้างคำสั่ง SQL ในที่นี้คือ ดึงข้อมูลทั้งหมดจากตาราง product_tb
                    string strSQL = "SELECT proId,proName,proPrice,proQuan,proUnit,proStatus,proImage FROM product_tb " +
                        "WHERE proId = @proId";

                    //จัดการให้ SQL ทำงาน
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        // กำหนดค่าให้กับ SQL Parameter
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@proId", proId);

                        //เอาข้อมูลที่ได้จาก strSQL ซึ่งเป็นก้อนใน dataadapter มาทำให้เป็นตารางโดยใส่ไว้ใน datatable
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // เอาข้อมูลจาก datatable มาใช้โดยใส่ไว้ใน datarow
                        DataRow row = dataTable.Rows[0];

                        //เอาข้อมูลใน datarow มาใช้งาน
                        tbProId.Text = row["proId"].ToString();
                        tbProName.Text = row["proName"].ToString();
                        tbProPrice.Text = row["proPrice"].ToString();
                        tbProUnit.Text = row["proUnit"].ToString();
                        nudProQuan.Value = int.Parse(row["ProQuan"].ToString());
                        if (row["proStatus"].ToString()== "พร้อมขาย")
                        {
                            rdoProStatusOn.Checked = true;
                        }
                        else
                        {
                            rdoProStatusOff.Checked = true;
                        }
                        // เอารูปมาแสดง โดย ต้องแปลงรูปซึ่งเป็น binary/byte array ให้เป็นรูป
                        if (row["proImage"] == DBNull.Value)
                        {
                            pcbProImage.Image =null;
                        }
                        else
                        {
                            pcbProImage.Image = convertByteArrayToImage((byte[])row["proImage"]);
                        }

                    }


                }

                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด  กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }

            }

        }
       


        private void btProDelete_Click(object sender, EventArgs e)
        {
            //ลบข้อมูลสินค้าออกจากตารางใน DB เงื่อนไขคือ proId
            //กำหนด connect String เพื่อติดต่อไปยังฐานข้อมูล
            string connectionstring = @"server=DESKTOP-6F6L1NQ\SQLEXPRESS2022;Database=store_db;Trusted_Connection=True;";

            // สร้าง connection ไปยังฐานข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
            {
                try
                {
                    sqlConnection.Open();

                    SqlTransaction sqlTransaction = sqlConnection.BeginTransaction(); // ใช้กับ Insert/update/delete

                    //คำสั่ง SQL
                    string strSQL = "DELETE FROM product_tb WHERE proId=@proId";


                    // บันทึกลง DB -> แสดง MSG -> ปิดหน้าจอ -> กลับไปยังหน้า FrmProductShow
                    // กำหนดค่าให้กับ SQL Parameter  และสั่งให้คำสั่ง SQL ทำงาน  แล้วมีข้อความแจ้งเมื่อทำงานเสร็จแล้ว
                    using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                    {
                        sqlCommand.Parameters.Add("@proId", SqlDbType.Int).Value = tbProId.Text;
                       
                        //สั่งให้คำสั่ง sql ทำงาน
                        sqlCommand.ExecuteNonQuery();
                        sqlTransaction.Commit();

                        //ข้อความแจ้ง
                        MessageBox.Show("บันทึกเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //ปิดหน้าต่างนี้
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด  กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }
            }

        }

        private void btProUpdate_Click(object sender, EventArgs e)
        {
            //Validate UI แสดงข้อความเตือนด้วย เมื่อ  Validate แล้วก้เอาข้อมูลไปบันทึกลง DB
            //พอบันทึกเสร็จแสดงข้อความบอกผู้ใช้ และปิดหน้าจอ FrmProductCreate และกลับไปหน้าจอ FrmProductShow

            if (proImage == null)
            {
                showWarningMSG("เลือกรูปด้วย");
            }
            else if (tbProName.Text.Length == 0)
            {
                showWarningMSG("ป้อนสินค้าด้วย");
            }
            else if (tbProPrice.Text.Length == 0)
            {
                showWarningMSG("ป้อนราคาสินค้าด้วย");
            }
            else if (int.Parse(nudProQuan.Value.ToString()) <= 0)
            {
                showWarningMSG("จำนวนสินค้าต้องมากกว่า 0 ");
            }
            else if (tbProUnit.Text.Length == 0)
            {
                showWarningMSG("ป้อนหน่วยสินค้าด้วย");
            }
            else
            {
                // บันทึกลง DB -> แสดง MSG -> ปิดหน้าจอ -> กลับไปยังหน้า FrmProductShow
                //กำหนด connect String เพื่อติดต่อไปยังฐานข้อมูล
                string connectionstring = @"server=DESKTOP-6F6L1NQ\SQLEXPRESS2022;Database=store_db;Trusted_Connection=True;";

                // สร้าง connection ไปยังฐานข้อมูล
                using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        sqlConnection.Open();

                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction(); // ใช้กับ Insert/update/delete

                        //คำสั่ง SQL
                        string strSQL = @"UPDATE product_tb  
                                  SET proName = @proName,
                                      proPrice = @proPrice,
                                      proQuan = @proQuan,
                                      proUnit = @proUnit,
                                      proStatus = @proStatus,
                                      proImage = @proImage,
                                      updateAt = @updateAt
                                  WHERE proId = @proId";

                        // กำหนดค่าให้กับ SQL Parameter  และสั่งให้คำสั่ง SQL ทำงาน  แล้วมีข้อความแจ้งเมื่อทำงานเสร็จแล้ว
                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@proName", SqlDbType.NVarChar, 300).Value = tbProName.Text;
                            sqlCommand.Parameters.Add("@proPrice", SqlDbType.Float).Value = float.Parse(tbProPrice.Text);
                            sqlCommand.Parameters.Add("@proQuan", SqlDbType.Int).Value = int.Parse(nudProQuan.Value.ToString());
                            sqlCommand.Parameters.Add("@proUnit", SqlDbType.NVarChar, 50).Value = tbProUnit.Text;
                            if (rdoProStatusOn.Checked == true)
                            {
                                sqlCommand.Parameters.Add("@proStatus", SqlDbType.NVarChar, 50).Value = "พร้อขาย";
                            }
                            else
                            {
                                sqlCommand.Parameters.Add("@proStatus", SqlDbType.NVarChar, 50).Value = "ไม่พร้อขาย";
                            }
                            sqlCommand.Parameters.Add("@proImage", SqlDbType.Image).Value = proImage;
                            sqlCommand.Parameters.Add("@createAt", SqlDbType.Date).Value = DateTime.Now.Date;
                            sqlCommand.Parameters.Add("@updateAt", SqlDbType.Date).Value = DateTime.Now.Date;
                            sqlCommand.Parameters.Add("@proId", SqlDbType.Int).Value = int.Parse(tbProId.Text);

                            //สั่งให้คำสั่ง sql ทำงาน
                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            //ข้อความแจ้ง
                            MessageBox.Show("บันทึกแก้ไขเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            //ปิดหน้าต่างนี้
                            this.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด  กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }

            }
        }

        private void btProImage_Click(object sender, EventArgs e)
        {

            //เปิดไฟล์ dialog  ให้เลือกรูปโดยฟิลเตอร์เฉพาะไฟล์ jpg/png
            //แล้วก็แปลงเป็น Binary/Byte เก็บในตัวแปรเพื่อเอาไว้บันทึกลง DB
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"c:\";
            openFileDialog.Filter = "Image File (*.jpg;*.png)|*.jpg;*.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //เอารูปที่เลือกไปแสดงที่ pcbProImage
                pcbProImage.Image = Image.FromFile(openFileDialog.FileName);
                // ตรวจสอบ format ของรูป แล้วส่งรูปไปแปลงเป็น Binary/Byte Array

                if (pcbProImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    proImage = convertImageToByteArray(pcbProImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    proImage = convertImageToByteArray(pcbProImage.Image, ImageFormat.Png);
                }

            }
        }
    }
    
}

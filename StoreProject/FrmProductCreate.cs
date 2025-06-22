using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Data;
namespace StoreProject
{
    public partial class FrmProductCreate : Form
    {
        //สร้างตัวแปรเก็บรูปเป็น Binary/Byte Array เอาไว้บันทึกลง DB
        byte[] proImage;



 //สร้างเมธอดแปลงรูปเป็น Binary/Byte Array
        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }
        public FrmProductCreate()
        {
            InitializeComponent();
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

        private void tbProPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // ตรวจสอบว่าเป็นตัวเลขหรือไม่
            if (char.IsDigit(e.KeyChar))
            {
                // ยอมให้พิมพ์ตัวเลขได้
                e.Handled = false;
            }
            // ตรวจสอบว่าเป็นจุดทศนิยมหรือไม่
            else if (e.KeyChar == '.')
            {
                // ถ้ามีจุดอยู่แล้ว ไม่ให้พิมพ์เพิ่ม
                if (((TextBox)sender).Text.Contains("."))
                {
                    e.Handled = true;
                }
                else
                {
                    // อนุญาตให้ใส่จุดได้ 1 จุด
                    e.Handled = false;
                }
            }
            // อนุญาตให้ใช้ backspace ได้
            else if (e.KeyChar == (char)Keys.Back)
            {
                e.Handled = false;
            }
            // ถ้าไม่ใช่ตัวเลข จุด หรือ backspace ป้องกันการป้อน
            else
            {
                e.Handled = true;
            }

        }
        //สร้างเมธอดแสดงข้อความเตือน
        private void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }



        private void btSave_Click(object sender, EventArgs e)
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
                        string strSQL = "INSERT INTO product_tb (proName,proPrice,proQuan,proUnit,proStatus,proImage,createAt,updateAt) " +
                                        "VALUES (@proName,@proPrice,@proQuan,@proUnit,@proStatus,@proImage,@createAt,@updateAt)";

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
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            //clear หน้าจอ
            proImage = null;
            tbProName.Clear();
            tbProPrice.Clear();
            nudProQuan.Value = 0 ;
            tbProUnit.Clear();
            pcbProImage.Image = null;
        }
    }
}

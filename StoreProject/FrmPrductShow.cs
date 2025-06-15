using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreProject
{
    public partial class FrmPrductShow : Form
    {
        public FrmPrductShow()
        {
            InitializeComponent();
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

        //สร้างเมธอดดึงข้อมูลจากตาราง product_tb มาแสดงที่ ListView
        private void getAllProductToLV()
        {
            //กำหนด connect String เพื่อติดต่อไปยังฐานข้อมูล
            string connectionstring = @"server=DESKTOP-6F6L1NQ\SQLEXPRESS2022;Database=store_db;Trusted_Connection=True;";

            // สร้าง connection ไปยังฐานข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
            {
                try
                { 
                    sqlConnection.Open(); //เปิดการเชื่อมต่อไปยังฐานข้อมูล

                    //การทำงานกับตารางในฐานข้อมูล (select,insert,update, delete)
                    // สร้างคำสั่ง SQL ในที่นี้คือ ดึงข้อมูลทั้งหมดจากตาราง product_tb
                    string strSQL = "SELECT proId,proName,proPrice,proQuan,proUnit,proStatus,proImage FROM product_tb";

                    //จัดการให้ SQL ทำงาน
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    { 
                        //เอาข้อมูลที่ได้จาก strSQL ซึ่งเป็นก้อนใน dataadapter มาทำให้เป็นตารางโดยใส่ไว้ใน datatable
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // ตั้งค่า ListView
                        lvAllProduct.Items.Clear();
                        lvAllProduct.Columns.Clear();
                        lvAllProduct.FullRowSelect = true;
                        lvAllProduct.View = View.Details;

                        //ตั้งค่าการแสดงรูปใน ListView
                        if (lvAllProduct.SmallImageList == null)
                        {
                            lvAllProduct.SmallImageList = new ImageList();
                            lvAllProduct.SmallImageList.ImageSize = new Size(50, 50);
                            lvAllProduct.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
                        }
                        lvAllProduct.SmallImageList.Images.Clear();

                        //กำหนดรายละเอียดของ column ใน ListView
                        lvAllProduct.Columns.Add("รูปภาพ", 80, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("รหัสสินค้า", 80, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("ชื่อสินค้า", 150, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("ราคา", 80, HorizontalAlignment.Right);
                        lvAllProduct.Columns.Add("จำนวน", 80, HorizontalAlignment.Right);
                        lvAllProduct.Columns.Add("หน่วย", 80, HorizontalAlignment.Left);
                        lvAllProduct.Columns.Add("สถานะ", 100, HorizontalAlignment.Left);


                        // loop วนเข้าไปใน datatable 
                        foreach (DataRow dataRow in dataTable.Rows)
                        { 
                            ListViewItem item = new ListViewItem(); // สร้าง item เพื่อเก็บแต่ละข้อมูลในแต่ละรายการ
                            //เอารูปใส่ใน item
                            Image proImage = null;
                            if (dataRow["proImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["proImage"];
                                //แปลงข้อมูลรูปจากฐานข้อมูลซึ่งเป็น binary ให้เป็นรูป
                                proImage = convertByteArrayToImage(imgByte);
                            }
                            string imageKey = null;
                            if (proImage != null)
                            {
                                imageKey = $"pro_{dataRow["proId"]}";
                                lvAllProduct.SmallImageList.Images.Add(imageKey, proImage);
                                item.ImageKey = imageKey;
                            }
                            else
                            { 
                                item.ImageIndex = -1;
                            }
                            // เอาแต่ละรายการมาใส่ใน item
                            item.SubItems.Add(dataRow["proId"].ToString());
                            item.SubItems.Add(dataRow["proName"].ToString());
                            item.SubItems.Add(dataRow["proPrice"].ToString());
                            item.SubItems.Add(dataRow["proQuan"].ToString());
                            item.SubItems.Add(dataRow["proUnit"].ToString());
                            item.SubItems.Add(dataRow["prostatus"].ToString());

                            //เอาข้อมูลลงใน item
                            lvAllProduct.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex) 
                {
                    MessageBox.Show("พบข้อผิดพลาด  กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }
            }
        }
     
        // From_load จะทำงานทุกครั้งที่ from ถูกเปิดขึ้นมา
        private void FrmPrductShow_Load(object sender, EventArgs e)
        {
            //ให้ไปดึงข้อมูลจาก product_tb มาแสดง
            getAllProductToLV();
        }
    }
}

using SEC.Util;
using SEC.Driver;
using System.Text;
using SEC.Driver.ModebusTcp;
using System.Net;
using Microsoft.Data.Sqlite;
using Dapper;
using SEC.Util.Helper;
using System.Windows.Forms;
using static SEC.Test.Form3;
using System.Net.Sockets;
using SEC.Driver.MC3E;
using SEC.Driver.Fins;
using SEC.Driver.ModbusRtu;

namespace SEC.Test
{
    public partial class Form2 : Form
    {
        private void InitListView(ListView listView, ImageList imageList)
        {
            listView.SmallImageList = imageList;
            ColumnHeader columnHeader1 = new ColumnHeader() { Name = "dateTime", Text = "日志时间", Width = 150 };
            ColumnHeader columnHeader2 = new ColumnHeader() { Name = "infoString", Text = "日志信息", Width = 1000 };
            listView.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });

            listView.HeaderStyle = ColumnHeaderStyle.None;
            listView.View = View.Details;
            listView.HideSelection = false;
            listView.SmallImageList = imageList;
        }

        private void Addlog(int imageIndex, string info)
        {
            Addlog(listView1, imageList1, imageIndex, info, 1000);
        }

        private void Addlog(ListView listView, ImageList imageList, int imageIndex, string info, int maxDisplayItems)
        {
            if (listView.InvokeRequired)
            {
                listView.Invoke(new Action(() =>
                {
                    if (listView.Items.Count > maxDisplayItems)
                    {
                        listView.Items.RemoveAt(maxDisplayItems);
                    }

                    ListViewItem lstItem = new ListViewItem(" " + DateTime.Now.ToString(), imageIndex);
                    lstItem.SubItems.Add(info);
                    listView.Items.Insert(0, lstItem);
                }));
            }
            else
            {
                if (listView.Items.Count > maxDisplayItems)
                {
                    listView.Items.RemoveAt(maxDisplayItems);
                }

                ListViewItem lstItem = new ListViewItem(" " + DateTime.Now.ToString(), imageIndex);
                lstItem.SubItems.Add(info);
                listView.Items.Insert(0, lstItem);
            }
        }


        string mesIP = string.Empty;

        public Form2()
        {
            InitializeComponent();
            InitListView(listView1, imageList1);

            mesIP = File.ReadAllText(Application.StartupPath + "MesIP");




            // var connectionString = new SqliteConnectionStringBuilder()
            // {
            //     Mode = SqliteOpenMode.ReadWriteCreate,
            //     DataSource = "C:\\Users\\su\\Desktop\\waa.s3db" 
            // }.ToString(); 
            // var _connection = new SqliteConnection(connectionString);
            // _connection.Open(); 
            //
            //
            //
            //var asd= _connection.Query("select * from Tag").ToList();

            SqlBuilder sqlBuilder = new SqlBuilder(); 
        }
        private void Form2_Load(object sender, EventArgs e)
        { 
             
            byte[] bytes = new byte[10] { 0x02, 0x03, 0x11, 0x11, 0x03, 0x02, 0x03, 0x12, 0x12, 0x03 };

           var ad= bytes.Capture(new byte[1] { 0x02}, 1,1);

            //创建扫码枪服务
           // new SocketServer(30000).ReceiveEvent += Ad_ReceiveEvent;
            //return;
            //读取配置文件
            var config = File.ReadAllText(Application.StartupPath + "MC3E.json");
            if (config.TryToObject(out EquInfo? _EQUValue))
            {
                string? ConnectionString = _EQUValue?.ConnectionString;
                if (ConnectionString != null)
                {
                    modbusRtu = new MC3E(ConnectionString);
                    modbusRtu.AddTags(_EQUValue.Tags);
                    this.dataGridView1.AutoGenerateColumns = false;
                    dataGridView1.DataSource = _EQUValue.Tags;
                    Tag? TotalQTY = modbusRtu?.AllTags.Find(p => p.TagName == "TotalQTY");
                    if (TotalQTY != null)
                    {
                        TotalQTY.ValueChangeEvent += UpdateData;
                    }
                    modbusRtu?.AllTags.ForEach(p =>
                    {
                        p.ValueChangeEvent += new Tag.ValueChangeDelegate((Tag tag) =>
                        {
                            Addlog(0, tag.Description + "       " + tag.Value);
                        });
                    });
                    modbusRtu?.Start();
                }
            }
        }

        private void Ad_ReceiveEvent(Socket client, byte[] bytes)
        {
            Addlog(0, "     " + bytes.To0XString());
            return;
            if (bytes.Length > 2)
            { 
                try
                {
                    string data = Encoding.ASCII.GetString(bytes);
                    Addlog(0, "     " + data);
                    var response = HttpRequest.PostAsyncJson($"{mesIP}/api/Packaging/GetInnerBoxInfoByNoFromEB", @$"{{ ""InnerPackageNo"":""{data}"" }}").Result;
                    var Jresponse = response.ToJObject();
                    if ((bool?)Jresponse["Success"] ?? false)
                    {
                        var result = Jresponse["Result"]?.ToString().ToJObject();
                        if (result != null)
                        {
                            if (result["ResponseResult"]?["ResultCode"]?.ToString() == "0")
                            {
                                Addlog(1, result["ResponseResult"]?["ResultMsg"]?.ToString() ?? string.Empty);
                            }
                            else
                            {
                                var ceBagWeightDown = (double?)result["Data"]?["CeBagWeightDown"];
                                var ceBagWeightUp = (double?)result["Data"]?["CeBagWeightUp"];
                                var standardWeight = (double?)result["Data"]?["StandardWeight"];
                                var pnSubstr = result["Data"]?["PnSubstr"]?.ToString();
                                InnerPackageNo = result["Data"]?["InnerPackageNo"]?.ToString() ?? string.Empty;
                                Tag? CeBagWeightDown = modbusRtu?.AllTags.Find(p => p.TagName == "CeBagWeightDown");
                                Tag? CeBagWeightUp = modbusRtu?.AllTags.Find(p => p.TagName == "CeBagWeightUp");
                                Tag? StandardWeight = modbusRtu?.AllTags.Find(p => p.TagName == "StandardWeight");
                                Tag? PnSubstr = modbusRtu?.AllTags.Find(p => p.TagName == "PnSubstr");
                                if (CeBagWeightDown != null
                                    && CeBagWeightUp != null
                                    && StandardWeight != null
                                    && PnSubstr != null)
                                {
                                    CeBagWeightDown.Value = ceBagWeightDown;
                                    CeBagWeightUp.Value = ceBagWeightUp;
                                    StandardWeight.Value = standardWeight;
                                    PnSubstr.Value = pnSubstr;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Addlog(1, e.Message);
                }
            }
            else
            {
                Addlog(0, "     接收到心跳");
            }
        }
        string InnerPackageNo = string.Empty;
        BaseDriver? modbusRtu = null;
        private void UpdateData(Tag tag)
        {
            try
            {
                Tag? RealTimeWeight = modbusRtu?.AllTags.Find(p => p.TagName == "RealTimeWeight");
                int? TotalQTY = (int?)tag.Value;
                int? OldTotalQTY = (int?)tag.OldValue;
                if (RealTimeWeight != null && TotalQTY != 0 && TotalQTY - OldTotalQTY == 1 && InnerPackageNo != string.Empty)
                {
                    var response = HttpRequest.PostAsyncJson($"{mesIP}/api/Packaging/InnerWeighConfirmFromEB", @$"{{ ""InnerPackageNo"":""{InnerPackageNo}"",""ActualWeight"":""{RealTimeWeight.Value}""  }}").Result;
                    Addlog(0, tag.Description + "       " + response);
                    InnerPackageNo = string.Empty;
                }
            }
            catch (Exception e)
            {
                Addlog(1, e.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dataGridView1.Refresh();
        }
    }
}

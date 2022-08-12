using SEC.Util;
using SEC.Driver;
using System.Text;
using SEC.Driver.ModebusTcp;
using System.Net;
using Microsoft.Data.Sqlite;
using Dapper; 

namespace SEC.Test
{
    public partial class Form2 : Form
    {
        private void InitListView(ListView listView, ImageList imageList)
        {
            listView.SmallImageList = imageList;
            ColumnHeader columnHeader1 = new ColumnHeader() { Name = "dateTime", Text = "日志时间", Width = 150 };
            ColumnHeader columnHeader2 = new ColumnHeader() { Name = "infoString", Text = "日志信息", Width = 220 };
            listView.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });

            listView.HeaderStyle = ColumnHeaderStyle.None;
            listView.View = View.Details;
            listView.HideSelection = false;
            listView.SmallImageList = imageList;
        }

        private void Addlog(int imageIndex, string info)
        {
            Addlog(listView1, imageList1, imageIndex, info, 20);
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

        public Form2()
        { 
            InitializeComponent();
            InitListView(listView1, imageList1);
            var connectionString = new SqliteConnectionStringBuilder()
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                DataSource = "C:\\Users\\su\\Desktop\\waa.s3db" 
            }.ToString(); 
            var _connection = new SqliteConnection(connectionString);
            _connection.Open(); 



           var asd= _connection.Query("select * from Tag").ToList();
        }
        readonly HttpClient client = new HttpClient();
        public async Task<string> Post(string url, string data)
        {
            HttpClient client = new HttpClient();
            HttpContent content = new StringContent(data);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            TCPServer ad = new TCPServer(30000);
            ad.ReceiveEvent += Ad_ReceiveEvent;
            var config = File.ReadAllText(Application.StartupPath + "Config.json");
            if (config.TryToObject(out EquInfo? _EQUValue))
            {
                modbusRtu = new ModbusTcp(new TCPClient("127.0.0.1", 502));
                modbusRtu.AddTags(_EQUValue.Tags);
            }
        }

        private void Ad_ReceiveEvent(ListenSocket client, byte[] bytes)
        {
            if (bytes.Length > 2)
            {
                string data = Encoding.ASCII.GetString(bytes);
                var response = Post("http://10.164.19.18:8087/api/Packaging/GetInnerBoxInfoByNoFromEB", @$"{{ ""InnerPackNo"":""{data}"" }}").Result;
                var Jresponse = response.ToJObject();
                if ((bool?)Jresponse["Success"] ?? false)
                {
                    var result = Jresponse["Result"]?.ToString().ToJObject();
                    if (result != null)
                    {
                        if (result["ResponseResult"]["ResultCode"].ToString() == "0")
                        {
                            Addlog(1, result["ResponseResult"]["ResultCode"].ToString());
                        }
                        else
                        {
                            var ceBagWeightDown = (float?)result["Data"]["CeBagWeightDown"];
                            var ceBagWeightUp = (float?)result["Data"]["CeBagWeightUp"];
                        }
                    }

                }

                Addlog(0, "       " + data);
            }
        }

        BaseDriver? modbusRtu = null;
        private void button1_Click(object sender, EventArgs e)
        {

            modbusRtu.AllTags.FirstOrDefault(p => p.Address == textBox1.Text).Value = textBox2.Text;

            // Form1 form1 = new Form1();
            // form1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            modbusRtu.AllTags.ForEach(p =>
            {
                p.ValueChangeEvent += aa;
            });
            modbusRtu.Start();
        }

        private void aa(Tag tag)
        {
            Addlog(0, tag.Address + "   " + tag.Value);
        }
    }
}

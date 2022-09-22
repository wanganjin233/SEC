using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SEC.Util;
using SEC.Util.Helper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SEC.Test
{

    public partial class Form3 : Form
    {
        public class Data
        {
            /// <summary>
            /// 
            /// </summary>
            public int StatusCode { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool Success { get; set; }
            /// <summary>
            /// 治具号校验失败,设备没有提供治具号！
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Result { get; set; }
        }

        public class Root
        {
            /// <summary>
            /// 
            /// </summary>
            public Data Data { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Timestamp { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string RequestId { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Error { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool IsSuccess { get; set; }
        }
        string mesIP = string.Empty;
        public Form3()
        {
            InitializeComponent();
            mesIP = File.ReadAllText(Application.StartupPath + "MesIP");
        }

         
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                textBox2.Select();
                textBox2.SelectAll();
            }

        }

        private void Form3_Activated(object sender, EventArgs e)
        {
            textBox1.Select();
            textBox1.SelectAll();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                Task.Run(async () =>
                {
                    string responsestr = await HttpRequest.PostAsyncJson($"http://{mesIP}/DataService/webapi/RpcInvoke", @$"{{
                                ""RpcPara"":{{
                                    ""MethodName"":""GetParameterRequest"",  //接口方法名
                                    ""Paras"":[
                                        {{
                                            ""Type"":""2"", 
                                            ""Lot"":""{textBox1.Text}"", 
                                            ""Carrier"":""{textBox2.Text}"", 
                                            ""Equipment"":""M-ME-ET-001"", 
                                            ""SetQty"":0,  
                                            ""PanelQty"":0, 
                                            ""UserCode"":""CEE_1000021""
                                        }}
                                    ]
                                }}
                            }}");
                    Root? response = responsestr.ToObject<Root>();
                    if (response != null)
                    {
                        this.Invoke(() =>
                        {
                            label3.Text = response.Data.Message;
                        });
                    }
                });
            }
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;    //取消"关闭窗口"事件
                this.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效果 
                return;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }
    }
}

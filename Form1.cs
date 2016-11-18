using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WLANManager
{
    public partial class Form1 : Form
    {
        int status;//0获取列表

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                status = 1;
                cmd("wlan show profile name=\""+"*********" +"\" key=clear");
            }
            else
            {
                MessageBox.Show("请选择项目~");
            }
        }

        public delegate void ClearCallback(); 

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (status == 0)
                {
                    if (e.Data.IndexOf("所有用户配置文件 : ")>=0)
                    {
                        AppendText(e.Data.Substring(15, e.Data.Length - 15));
                    }
                }
                if (status == 1)
                {
                    if (e.Data.IndexOf("安全密钥               : 不存在") >= 0)
                    {
                        MessageBox.Show("该网络无密码...");
                    }
                    if (e.Data.IndexOf("    关键内容            : ") >= 0)
                    {
                        MessageBox.Show("该网络密码为："+e.Data.Substring(22, e.Data.Length - 22));
                        status = -1;
                    }
                }
                if (status == 2)
                {
                    MessageBox.Show(e.Data);
                    ClearCallback d = new ClearCallback(RefreshList);
                    this.listBox1.Invoke(d);
                }
                if (status == 3)
                {
                    MessageBox.Show(e.Data);
                }
            }

        }

        public delegate void AppendTextCallback(string text); 
        public void AppendText(string text)
        {
            if (this.listBox1.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(AppendText);
                this.listBox1.Invoke(d, text);
            }
            else
            {
                listBox1.Items.Add(text);
            } 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshList();
        }

        public void RefreshList()
        {
            listBox1.Items.Clear();
            status = 0;
            cmd("wlan show profiles");
        }

        public void cmd(string str)
        {
            using (Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "netsh";
                process.StartInfo.Arguments = str;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                process.BeginOutputReadLine();
                process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
                process.EnableRaisingEvents=true;
                process.Exited += new EventHandler(test);
            }  
        }
        static void test(object sender, EventArgs e)
        {
            MessageBox.Show("11");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            RefreshList();
            Delay(2000);
            button3.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("请选择项目~");
                return;
            }
            if (MessageBox.Show("确认删除网络  " + listBox1.Items[listBox1.SelectedIndex] + " ？此删除不可恢复", "确认删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                status = 2;
                cmd("wlan delete profile name=\""+listBox1.Items[listBox1.SelectedIndex]+"\"");
            }
            else
            {
                return;
            }
        }

        private void Delay(int Millisecond) //延迟系统时间，但系统又能同时能执行其它任务；
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(Millisecond) > DateTime.Now)
            {
                Application.DoEvents();//转让控制权            
            }
            return;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                status = 1;
                cmd("wlan show profile name=\"" + listBox1.Items[listBox1.SelectedIndex] + "\" key=clear");
            }
            else
            {
                MessageBox.Show("请选择项目~");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("请选择项目~");
                return;
            }
            if (MessageBox.Show("确认取消网络  " + listBox1.Items[listBox1.SelectedIndex] + " 的自动连接？", "确认设置", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                status = 3;
                cmd("wlan set profileparameter name=\"" + listBox1.Items[listBox1.SelectedIndex] + "\" connectionmode=manual");
            }
            else
            {
                return;
            }
        }


    }
}

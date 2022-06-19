using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrabSS.StartupBashGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            UpdateLog(log, "程序已启动，环境配置完毕\n对于小白，我们仍然建议使用 CrabSS 标准版，开服更快并支持彩色回显，上手难度更低，还用有插件管理等多项功能！点击这里立刻下载 https://crabss.heycrab.xyz");
        }

        private void UpdateLog(RichTextBox obj, string text) {
            string datetime = DateTime.Now.ToString();
            log.AppendText("[" + datetime + "] " + text + "\n");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确认开始生成？","开始打洞？（",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1);
            log.Text = "";
            UpdateLog(log, "开始打洞...");
            string script = null;
            if (textBox1.Text != "java")
            {
                if (checkBox1.Checked == true)
                {
                    script = "'" + textBox1.Text + "' -Xms" + numericUpDown1.Value + "M -Xmx" + numericUpDown2.Value + "M -jar " + textBox2.Text + " " + textBox3.Text;
                }
                else
                {
                    script = "'" + textBox1.Text + "' -Xms" + numericUpDown1.Value + "M -Xmx" + numericUpDown2.Value + "M -jar " + textBox2.Text + " nogui " + textBox3.Text;
                }
            }
            else {
                if (checkBox1.Checked == true)
                {
                    script = "java -Xms" + numericUpDown1.Value + "M -Xmx" + numericUpDown2.Value + "M -jar " + textBox2.Text + " " + textBox3.Text;
                }
                else
                {
                    script = "java -Xms" + numericUpDown1.Value + "M -Xmx" + numericUpDown2.Value + "M -jar " + textBox2.Text + " nogui " + textBox3.Text;
                }
            }
            UpdateLog(log, "启动脚本内容：" + script);
            UpdateLog(log, "写入到文件：" + @Environment.CurrentDirectory + @"\start.cmd");
            try
            {
                File.WriteAllText(Environment.CurrentDirectory + "\\start.cmd", script);
                UpdateLog(log, "✅ 写入成功！");
            }
            catch (Exception ex)
            {
                UpdateLog(log, "⛔ 洞没打成，尝试用管理员权限再试试？\n" + ex);
            }
            UpdateLog(log, "打完了，可以关闭程序了");
        }
    }
}

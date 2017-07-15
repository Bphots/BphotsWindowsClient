using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotsBpHelper.Pages
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void ShowBallowNotify(int percent)
        {
            
            //设置托盘的各个属性
            notifyIcon1.BalloonTipText = "软件更新中..."+ percent.ToString() + "%";
            notifyIcon1.BalloonTipTitle = "背锅助手";
            //notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;

            notifyIcon1.ShowBalloonTip(2000);
        }

        public void ShowBallowNotify()
        {

            //设置托盘的各个属性
            notifyIcon1.BalloonTipText = "软件更新中...请稍等";
            notifyIcon1.BalloonTipTitle = "背锅助手";
            //notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(2000);
        }

        private void update_Load(object sender, EventArgs e)
        {

        }
    }
}

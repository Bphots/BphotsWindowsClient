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
            this.Text= ViewModelBase.L("UpdateFullText");
            notifyIcon1.Visible = false;
        }

        public void ShowBallowNotify(int percent)
        {
            
            //设置托盘的各个属性
            notifyIcon1.BalloonTipText = ViewModelBase.L("UpdateFullText")+"——"+ percent.ToString() + "%";
            notifyIcon1.BalloonTipTitle = ViewModelBase.L("HotsBpHelper"); ;
            //notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;

            notifyIcon1.ShowBalloonTip(2000);
        }

        public void ShowBallowNotify()
        {

            //设置托盘的各个属性
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = ViewModelBase.L("UpdateFullText");
            notifyIcon1.BalloonTipTitle = ViewModelBase.L("HotsBpHelper");
            //notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(1000);
        }

        public void xuMing()
        {
            notifyIcon1.InitializeLifetimeService();
        }

        public void kill()
        {
            notifyIcon1.Visible = false;
        }

        private void update_Load(object sender, EventArgs e)
        {

        }
    }
}

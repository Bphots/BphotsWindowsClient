﻿using System;
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
            notifyIcon1.Text= ViewModelBase.L("UpdateFullText");
            notifyIcon1.Visible = false;
        }

        public void ShowBallowNotify(int percent)
        {
            
            //设置托盘的各个属性
            notifyIcon1.BalloonTipText = ViewModelBase.L("UpdateFullText")+"——"+ percent.ToString() + "%";
            notifyIcon1.BalloonTipTitle = ViewModelBase.L("HotsBpHelper");
            //notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            if (notifyIcon1.BalloonTipText == null || notifyIcon1.BalloonTipText == null)
            {
                notifyIcon1.BalloonTipText = "Updating..";
                notifyIcon1.BalloonTipTitle = "Updating..";
            }

            notifyIcon1.ShowBalloonTip(2000);
        }

        public void ShowBallowNotify(string title,string text)
        {
            notifyIcon1.Visible = true;
            //设置托盘的各个属性
                notifyIcon1.BalloonTipText = text;
                notifyIcon1.BalloonTipTitle = title;
            if (notifyIcon1.BalloonTipText == null || notifyIcon1.BalloonTipText == null)
            {
                notifyIcon1.BalloonTipText = "Updating..";
                notifyIcon1.BalloonTipTitle = "Updating..";
            }

            //notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(2000);
            notifyIcon1.Visible = false;
        }

        public void ShowBallowNotify()
        {

            //设置托盘的各个属性
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = ViewModelBase.L("UpdateFullText");
            notifyIcon1.BalloonTipTitle = ViewModelBase.L("HotsBpHelper");
            if (notifyIcon1.BalloonTipText == null || notifyIcon1.BalloonTipText == null)
            {
                notifyIcon1.BalloonTipText = "Updating..";
                notifyIcon1.BalloonTipTitle = "Updating..";
            }
           
            //notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(1000);
            notifyIcon1.Visible = false;
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
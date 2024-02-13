using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace CaptureStation_3
{
    public partial class Viewer : Form
    {
        int frames = 0;
        Stopwatch stopwatch = new Stopwatch();
        int pragmaheight = 0;
        FilterInfoCollection info;
        List<VideoCaptureDevice> devices;
        VideoCaptureDevice _active_vcd = null;
        Bitmap thisFrame = null;
        int mode = 0;
        bool shoot = false;
        public Viewer()
        {
            InitializeComponent();

            start_read();
        }
        private void show_error(string str) => MessageBox.Show(str, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        private void start_read()
        {
            if (mode != 0)
            {
                return;
            }
            else
            {
                info = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
                devices = new List<VideoCaptureDevice>();
                foreach (FilterInfo filter in info)
                {
                    devices.Add(new VideoCaptureDevice(filter.MonikerString));
                    Console.WriteLine("- {0}", filter.MonikerString);
                }

            }
        }
        private void attach()
        {
            _active_vcd.NewFrame += Device_NewFrame;
            //_active_vcd.VideoResolution = _active_vcd.VideoCapabilities.Where((s,ss)=>s.FrameSize.Width<=1400).ToArray().FirstOrDefault();
            _active_vcd.Start();
        }

        private void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Invoker.Invoke(this, () =>
            {
                thisFrame = eventArgs.Frame.Clone() as Bitmap;
                this.pictureBox1.Image = thisFrame; ;
                this.pictureBox1.BringToFront();
                frames++;
                if (shoot)
                {
                    _active_vcd.SignalToStop();
                    shoot = false;
                }

            });
        }

        private void Viewer_Paint(object sender, PaintEventArgs e)
        {
            pragmaheight = 0;
            void pragma(string s) { e.Graphics.DrawString(s, Font, System.Drawing.Brushes.White, 0, pragmaheight); pragmaheight += 30; }

            if (mode == 0)
            {
                foreach (FilterInfo i in info)
                {
                    pragma(i.Name);

                }
                int height = 10;
                int index = 0;
                foreach (VideoCaptureDevice c in devices)
                {
                    int sub = 100;
                    foreach (var res in c.VideoCapabilities)
                    {
                        var btn = new Button();
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.Location = new Point(sub, height);
                        btn.ForeColor = Color.White;
                        btn.Text = string.Format("{0}x{1} {2}f", res.FrameSize.Width, res.FrameSize.Height, res.MaximumFrameRate);
                        sub += btn.Width;
                        btn.Click += (ss, ee) =>
                        {
                            mode = 1;
                            _active_vcd = c;
                            this.pictureBox1.Visible = true;
                            _active_vcd.VideoResolution = res;
                            attach();
                        };
                        btn.MouseClick += (s, ee) =>
                        {
                            if (ee.Button == MouseButtons.Middle)
                            {

                                shoot = true;
                                mode = 1;
                                _active_vcd = c;
                                this.pictureBox1.Visible = true;
                                _active_vcd.VideoResolution = res;
                                attach();
                            }
                        };
                        
                        this.Controls.Add(btn);
                    }
                    height = index * 55;
                    index++;
                }
            }
        }

        private void Viewer_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (mode == 1)
            {
                if ((Keys)e.KeyChar == Keys.Escape)
                {
                    _active_vcd.SignalToStop();
                    this.pictureBox1.Visible = false;
                    mode = 0;

                }
            }
            int numeric = ((int)e.KeyChar) - 48;
            if (numeric > 0 && numeric < 10)
            {
                if (devices == null)
                {
                    return;
                }
                else
                {
                    if (devices.Count > numeric)
                    {
                        mode = 1;
                        _active_vcd = devices[numeric];
                        this.pictureBox1.Visible = true;
                        attach();
                    }
                }
            }
            else show_error("Not in range.\nSelect a capture card from 1 to 10");
        }

        private void Viewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_active_vcd != null)
            {
                if (_active_vcd.IsRunning)
                {

                    _active_vcd.SignalToStop();
                    
                }
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _active_vcd.SignalToStop();
                this.pictureBox1.Visible = false;
                mode = 0;
                Invalidate();



            }
        }
    }
}

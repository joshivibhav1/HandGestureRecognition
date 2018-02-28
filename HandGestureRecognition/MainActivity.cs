using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HandGestureRecognition
{
    public partial class MainActivity : Form
    {
        public MainActivity()
        {
            InitializeComponent();
        }
        
        /*
Boolean maximized = false;

int posX;
int posY;
bool drag;

private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
{
   if (e.Button == MouseButtons.Left)
   {
       if (maximized)
       {
           this.WindowState = FormWindowState.Normal;
           maximized = false;
       }
       else
       {
           this.WindowState = FormWindowState.Maximized;
           maximized = true;
       }
   }
}

private void panel1_MouseDown(object sender, MouseEventArgs e)
{
   if (e.Button == MouseButtons.Left)
   {
       drag = true;
       posX = Cursor.Position.X - this.Left;
       posY = Cursor.Position.Y - this.Top;
   }
}

private void panel1_MouseUp(object sender, MouseEventArgs e)
{
   drag = false;
}

private void panel1_MouseMove(object sender, MouseEventArgs e)
{
   if (drag)
   {
       this.Top = System.Windows.Forms.Cursor.Position.Y - posY;
       this.Left = System.Windows.Forms.Cursor.Position.X - posX;
   }
   this.Cursor = Cursors.Default;
}

private void exit_Click(object sender, EventArgs e)
{
   this.Close();
}

private void minimize_Click(object sender, EventArgs e)
{
   this.WindowState = FormWindowState.Minimized;
}

private void maximize_Click(object sender, EventArgs e)
{
   if (maximized)
   {
       maximized = false;
       this.WindowState = FormWindowState.Normal;
   }
   else
   {
       maximized = true;
       this.WindowState = FormWindowState.Maximized;
   }
}*/
    }
}

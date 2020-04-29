namespace Cresij_Control_Manager
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ipGrid = new System.Windows.Forms.DataGridView();
            this.ip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Location = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WorkStat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Timer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PCStat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProjectorStat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProjHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CurtainStat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ScreenStat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LightStat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Media = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CentralLock = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PodiumLock = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClassLock = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cresijdatabaseDataSet = new Cresij_Control_Manager.cresijdatabaseDataSet();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextmenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeform = new System.Windows.Forms.ToolStripMenuItem();
            this.viewdevices = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.totalmachinelabel = new System.Windows.Forms.Label();
            this.onlinemachinelbl = new System.Windows.Forms.Label();
            this.offlinemachinelbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ipGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cresijdatabaseDataSet)).BeginInit();
            this.contextmenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // ipGrid
            // 
            this.ipGrid.AllowUserToAddRows = false;
            this.ipGrid.AllowUserToDeleteRows = false;
            this.ipGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ipGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ip,
            this.Location,
            this.Status,
            this.WorkStat,
            this.Timer,
            this.PCStat,
            this.ProjectorStat,
            this.ProjHour,
            this.CurtainStat,
            this.ScreenStat,
            this.LightStat,
            this.Media,
            this.CentralLock,
            this.PodiumLock,
            this.ClassLock});
            this.ipGrid.Location = new System.Drawing.Point(0, 34);
            this.ipGrid.Name = "ipGrid";
            this.ipGrid.RowTemplate.Height = 23;
            this.ipGrid.Size = new System.Drawing.Size(1084, 528);
            this.ipGrid.TabIndex = 0;
            // 
            // ip
            // 
            this.ip.HeaderText = "IP";
            this.ip.Name = "ip";
            // 
            // Location
            // 
            this.Location.HeaderText = "Location";
            this.Location.Name = "Location";
            this.Location.Width = 70;
            // 
            // Status
            // 
            this.Status.HeaderText = "MachineStat";
            this.Status.Name = "Status";
            this.Status.Width = 70;
            // 
            // WorkStat
            // 
            this.WorkStat.HeaderText = "WorkStat";
            this.WorkStat.Name = "WorkStat";
            this.WorkStat.Width = 70;
            // 
            // Timer
            // 
            this.Timer.HeaderText = "TimerService";
            this.Timer.Name = "Timer";
            this.Timer.Width = 60;
            // 
            // PCStat
            // 
            this.PCStat.HeaderText = "PcStat";
            this.PCStat.Name = "PCStat";
            this.PCStat.Width = 60;
            // 
            // ProjectorStat
            // 
            this.ProjectorStat.HeaderText = "ProjectorStat";
            this.ProjectorStat.Name = "ProjectorStat";
            this.ProjectorStat.Width = 70;
            // 
            // ProjHour
            // 
            this.ProjHour.HeaderText = "ProjectorHours";
            this.ProjHour.Name = "ProjHour";
            this.ProjHour.Width = 80;
            // 
            // CurtainStat
            // 
            this.CurtainStat.HeaderText = "CurtainStat";
            this.CurtainStat.Name = "CurtainStat";
            this.CurtainStat.Width = 60;
            // 
            // ScreenStat
            // 
            this.ScreenStat.HeaderText = "ScreenStat";
            this.ScreenStat.Name = "ScreenStat";
            this.ScreenStat.Width = 60;
            // 
            // LightStat
            // 
            this.LightStat.HeaderText = "LightStat";
            this.LightStat.Name = "LightStat";
            this.LightStat.Width = 50;
            // 
            // Media
            // 
            this.Media.HeaderText = "MediaSignal";
            this.Media.Name = "Media";
            this.Media.Width = 80;
            // 
            // CentralLock
            // 
            this.CentralLock.HeaderText = "CentralLock";
            this.CentralLock.Name = "CentralLock";
            this.CentralLock.Width = 70;
            // 
            // PodiumLock
            // 
            this.PodiumLock.HeaderText = "PodiumLock";
            this.PodiumLock.Name = "PodiumLock";
            this.PodiumLock.Width = 70;
            // 
            // ClassLock
            // 
            this.ClassLock.HeaderText = "ClassLock";
            this.ClassLock.Name = "ClassLock";
            this.ClassLock.Width = 70;
            // 
            // cresijdatabaseDataSet
            // 
            this.cresijdatabaseDataSet.DataSetName = "cresijdatabaseDataSet";
            this.cresijdatabaseDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextmenu;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Server App Notifier";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon1_MouseDoubleClick);
            // 
            // contextmenu
            // 
            this.contextmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeform,
            this.viewdevices});
            this.contextmenu.Name = "contextmenu";
            this.contextmenu.Size = new System.Drawing.Size(164, 48);
            // 
            // closeform
            // 
            this.closeform.Name = "closeform";
            this.closeform.Size = new System.Drawing.Size(163, 22);
            this.closeform.Text = "close server app";
            // 
            // viewdevices
            // 
            this.viewdevices.Name = "viewdevices";
            this.viewdevices.Size = new System.Drawing.Size(163, 22);
            this.viewdevices.Text = "View Devices List";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(982, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("SimSun", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(13, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 19);
            this.label1.TabIndex = 2;
            this.label1.Text = "Total Machines:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("SimSun", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(246, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 19);
            this.label2.TabIndex = 3;
            this.label2.Text = "Online:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("SimSun", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(456, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 19);
            this.label3.TabIndex = 4;
            this.label3.Text = "Offline:";
            // 
            // totalmachinelabel
            // 
            this.totalmachinelabel.AutoSize = true;
            this.totalmachinelabel.BackColor = System.Drawing.Color.Transparent;
            this.totalmachinelabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.totalmachinelabel.Font = new System.Drawing.Font("SimSun", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.totalmachinelabel.Location = new System.Drawing.Point(181, 4);
            this.totalmachinelabel.Name = "totalmachinelabel";
            this.totalmachinelabel.Size = new System.Drawing.Size(23, 24);
            this.totalmachinelabel.TabIndex = 5;
            this.totalmachinelabel.Text = "0";
            // 
            // onlinemachinelbl
            // 
            this.onlinemachinelbl.AutoSize = true;
            this.onlinemachinelbl.BackColor = System.Drawing.Color.Transparent;
            this.onlinemachinelbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.onlinemachinelbl.Font = new System.Drawing.Font("SimSun", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.onlinemachinelbl.Location = new System.Drawing.Point(333, 4);
            this.onlinemachinelbl.Name = "onlinemachinelbl";
            this.onlinemachinelbl.Size = new System.Drawing.Size(23, 24);
            this.onlinemachinelbl.TabIndex = 6;
            this.onlinemachinelbl.Text = "0";
            // 
            // offlinemachinelbl
            // 
            this.offlinemachinelbl.AutoSize = true;
            this.offlinemachinelbl.BackColor = System.Drawing.Color.Transparent;
            this.offlinemachinelbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.offlinemachinelbl.Font = new System.Drawing.Font("SimSun", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.offlinemachinelbl.Location = new System.Drawing.Point(555, 4);
            this.offlinemachinelbl.Name = "offlinemachinelbl";
            this.offlinemachinelbl.Size = new System.Drawing.Size(23, 24);
            this.offlinemachinelbl.TabIndex = 7;
            this.offlinemachinelbl.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 562);
            this.Controls.Add(this.offlinemachinelbl);
            this.Controls.Add(this.onlinemachinelbl);
            this.Controls.Add(this.totalmachinelabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ipGrid);
            this.Name = "Form1";
            this.Text = "Server Manager";
            this.Icon = Properties.Resources.icon1;
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ipGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cresijdatabaseDataSet)).EndInit();
            this.contextmenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView ipGrid;
        private cresijdatabaseDataSet cresijdatabaseDataSet;
        private System.Windows.Forms.DataGridViewTextBoxColumn ip;
#pragma warning disable CS0108 // 'Form1.Location' hides inherited member 'Form.Location'. Use the new keyword if hiding was intended.
        private System.Windows.Forms.DataGridViewTextBoxColumn Location;
#pragma warning restore CS0108 // 'Form1.Location' hides inherited member 'Form.Location'. Use the new keyword if hiding was intended.
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn WorkStat;
        private System.Windows.Forms.DataGridViewTextBoxColumn Timer;
        private System.Windows.Forms.DataGridViewTextBoxColumn PCStat;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProjectorStat;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProjHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn CurtainStat;
        private System.Windows.Forms.DataGridViewTextBoxColumn ScreenStat;
        private System.Windows.Forms.DataGridViewTextBoxColumn LightStat;
        private System.Windows.Forms.DataGridViewTextBoxColumn Media;
        private System.Windows.Forms.DataGridViewTextBoxColumn CentralLock;
        private System.Windows.Forms.DataGridViewTextBoxColumn PodiumLock;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClassLock;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextmenu;
        private System.Windows.Forms.ToolStripMenuItem closeform;
        private System.Windows.Forms.ToolStripMenuItem viewdevices;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label totalmachinelabel;
        private System.Windows.Forms.Label onlinemachinelbl;
        private System.Windows.Forms.Label offlinemachinelbl;
    }
}


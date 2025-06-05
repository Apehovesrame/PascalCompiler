namespace PascalCompiler
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtLexErrors = new System.Windows.Forms.TextBox();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.dgvTokens = new System.Windows.Forms.DataGridView();
            this.txtSyntaxErrors = new System.Windows.Forms.TextBox();
            this.txtSemanticErrors = new System.Windows.Forms.TextBox();
            this.listViewQuads = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listBoxIntermediateCode = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTokens)).BeginInit();
            this.SuspendLayout();
            // 
            // txtLexErrors
            // 
            this.txtLexErrors.Location = new System.Drawing.Point(9, 239);
            this.txtLexErrors.Multiline = true;
            this.txtLexErrors.Name = "txtLexErrors";
            this.txtLexErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLexErrors.Size = new System.Drawing.Size(684, 45);
            this.txtLexErrors.TabIndex = 8;
            this.txtLexErrors.Text = "txtLexErrors";
            // 
            // txtSource
            // 
            this.txtSource.Location = new System.Drawing.Point(9, 12);
            this.txtSource.Multiline = true;
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(189, 212);
            this.txtSource.TabIndex = 7;
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(140, 432);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(109, 46);
            this.btnLoadFile.TabIndex = 6;
            this.btnLoadFile.Text = "Открыть файл";
            this.btnLoadFile.UseVisualStyleBackColor = true;
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click_1);
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Location = new System.Drawing.Point(15, 432);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(109, 46);
            this.btnAnalyze.TabIndex = 5;
            this.btnAnalyze.Text = "Анализировать";
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // dgvTokens
            // 
            this.dgvTokens.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTokens.Location = new System.Drawing.Point(204, 12);
            this.dgvTokens.Name = "dgvTokens";
            this.dgvTokens.Size = new System.Drawing.Size(489, 212);
            this.dgvTokens.TabIndex = 10;
            // 
            // txtSyntaxErrors
            // 
            this.txtSyntaxErrors.Location = new System.Drawing.Point(9, 290);
            this.txtSyntaxErrors.Multiline = true;
            this.txtSyntaxErrors.Name = "txtSyntaxErrors";
            this.txtSyntaxErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSyntaxErrors.Size = new System.Drawing.Size(684, 45);
            this.txtSyntaxErrors.TabIndex = 11;
            this.txtSyntaxErrors.Text = "txtSyntaxErrors";
            // 
            // txtSemanticErrors
            // 
            this.txtSemanticErrors.Location = new System.Drawing.Point(9, 341);
            this.txtSemanticErrors.Multiline = true;
            this.txtSemanticErrors.Name = "txtSemanticErrors";
            this.txtSemanticErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSemanticErrors.Size = new System.Drawing.Size(684, 45);
            this.txtSemanticErrors.TabIndex = 12;
            this.txtSemanticErrors.Text = "txtSemanticErrors";
            // 
            // listViewQuads
            // 
            this.listViewQuads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.listViewQuads.HideSelection = false;
            this.listViewQuads.Location = new System.Drawing.Point(309, 396);
            this.listViewQuads.Margin = new System.Windows.Forms.Padding(2);
            this.listViewQuads.Name = "listViewQuads";
            this.listViewQuads.Size = new System.Drawing.Size(384, 114);
            this.listViewQuads.TabIndex = 13;
            this.listViewQuads.UseCompatibleStateImageBehavior = false;
            this.listViewQuads.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Оператор";
            this.columnHeader4.Width = 80;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Операнд 1";
            this.columnHeader5.Width = 80;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Операнд 2";
            this.columnHeader6.Width = 80;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Результат";
            this.columnHeader7.Width = 80;
            // 
            // listBoxIntermediateCode
            // 
            this.listBoxIntermediateCode.FormattingEnabled = true;
            this.listBoxIntermediateCode.Location = new System.Drawing.Point(715, 12);
            this.listBoxIntermediateCode.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxIntermediateCode.Name = "listBoxIntermediateCode";
            this.listBoxIntermediateCode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.listBoxIntermediateCode.Size = new System.Drawing.Size(394, 498);
            this.listBoxIntermediateCode.TabIndex = 14;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1115, 549);
            this.Controls.Add(this.listBoxIntermediateCode);
            this.Controls.Add(this.listViewQuads);
            this.Controls.Add(this.txtSemanticErrors);
            this.Controls.Add(this.txtSyntaxErrors);
            this.Controls.Add(this.dgvTokens);
            this.Controls.Add(this.txtLexErrors);
            this.Controls.Add(this.txtSource);
            this.Controls.Add(this.btnLoadFile);
            this.Controls.Add(this.btnAnalyze);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Транслятор языка Pascal";
            ((System.ComponentModel.ISupportInitialize)(this.dgvTokens)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtLexErrors;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Button btnLoadFile;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.DataGridView dgvTokens;
        private System.Windows.Forms.TextBox txtSyntaxErrors;
        private System.Windows.Forms.TextBox txtSemanticErrors;
        private System.Windows.Forms.ListView listViewQuads;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ListBox listBoxIntermediateCode;
    }
}


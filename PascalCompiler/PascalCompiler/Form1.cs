using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using static PascalCompiler.ParserLRPascal;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PascalCompiler
{
    public partial class MainForm : Form
    {
        //private List<Token> tokens;
        private ParserLRPascal parser;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            txtLexErrors.Clear();
            txtSyntaxErrors.Clear();
            txtSemanticErrors.Clear();
            listBox4.Items.Clear();

            string sourceCode = txtSource.Text;
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                txtSyntaxErrors.AppendText("Исходный код пуст.\r\n");
                txtLexErrors.AppendText("Исходный код пуст.\r\n");
                txtSemanticErrors.AppendText("Исходный код пуст.\r\n");
                return;
            }

            var lexer = new Lexer();
            var tokens = lexer.Tokenize(sourceCode, dgvTokens, txtLexErrors);

            parser = new ParserLRPascal(tokens, txtSyntaxErrors, listBox4);

            parser.Parse();

            var semanticAnalyzer = new SemanticAnalyzer(tokens, txtSemanticErrors);
            semanticAnalyzer.Analyze();

            var quads = parser.GetIntermediateCode();
            listViewQuads.Items.Clear();
            foreach (var quad in quads)
            {
                var item = new ListViewItem(quad.Operator);
                item.SubItems.Add(quad.Operand1);
                item.SubItems.Add(quad.Operand2);
                item.SubItems.Add(quad.Result);
                listViewQuads.Items.Add(item);
            }
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pascal files (*.pas)|*.pas|All files (*.*)|*.*",
                Title = "Открыть файл с кодом"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Читаем текст файла
                    string fileContent = File.ReadAllText(openFileDialog.FileName);

                    // Отображаем его в txtSource
                    txtSource.Text = fileContent;

                    // Автоматически запускаем лексический и синтаксический анализ, как при кнопке Analyze
                    btnAnalyze_Click(sender, e); // имитируем нажатие кнопки Analyze

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}",
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLoadFile_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файл для анализа";
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {// Чтение всего текста из файла
                    string text = File.ReadAllText(openFileDialog.FileName);
                    // Отображение содержимого файла в textBox1
                    txtSource.Text = text;

                    // Выполняем лексический анализ после загрузки файла
                    btnAnalyze_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogicGatesSimulator.Core.Logic;
using LogicGatesSimulator.Core.Simulation;
using LogicGatesSimulator.Core.Models;
using LogicGatesSimulator.Data;
namespace LogicGatesSimulator.UI
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }

    public partial class MainForm : Form
    {
        private Circuit circuit = new Circuit();

        // Переменные для перетаскивания
        private bool isDragging = false;
        private ILogicGate draggedGate = null;
        private Point dragOffset = Point.Empty;

        // Для соединений
        private ILogicGate selectedGate = null;
        private bool isConnecting = false;

        // Элементы управления
        private DoubleBufferedPanel workspace;
        private Panel toolPanel;
        private Panel inputPanel;
        private Panel truthTablePanel;
        private DataGridView truthTableGrid;
        private Button btnConnect;
        private Button btnGenerateTruthTable;
        private Button btnSimulate;
        private Label lblTruthTableStatus;
        private FlowLayoutPanel inputValuePanel;
        private List<InputValueControl> inputControls = new List<InputValueControl>();

        private class InputValueControl
        {
            public InputGate InputGate { get; set; }
            public Label Label { get; set; }
            public RadioButton Radio0 { get; set; }
            public RadioButton Radio1 { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();

            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (isConnecting)
                {
                    selectedGate = null;
                    isConnecting = false;
                    btnConnect.BackColor = Color.LightBlue;
                    workspace.Invalidate();
                }
            }
        }

        private void InitializeCustomComponents()
        {
            this.Size = new Size(1200, 700);
            this.Text = "Симулятор логических схем";
            this.BackColor = Color.FromArgb(245, 245, 255);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Заголовок формы
            Label titleLabel = new Label
            {
                Text = "Симулятор логических вентилей (Орехова Карина 06-452)",
                Location = new Point(220, 5),
                Size = new Size(750, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue,
                BackColor = Color.Transparent
            };
            this.Controls.Add(titleLabel);
            titleLabel.BringToFront();

            // Левая панель инструментов
            toolPanel = new Panel
            {
                Location = new Point(10, 40),
                Size = new Size(200, 650),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Рабочая область с двойной буферизацией
            workspace = new DoubleBufferedPanel
            {
                Location = new Point(220, 40),
                Size = new Size(650, 650),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Панель входных данных
            inputPanel = new Panel
            {
                Location = new Point(880, 40),
                Size = new Size(300, 220),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Панель таблицы истинности
            truthTablePanel = new Panel
            {
                Location = new Point(880, 270),
                Size = new Size(300, 420),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            CreateToolPanel();
            CreateInputPanel();
            CreateTruthTablePanel();

            // Подписка на события
            workspace.MouseDown += Workspace_MouseDown;
            workspace.MouseMove += Workspace_MouseMove;
            workspace.MouseUp += Workspace_MouseUp;
            workspace.Paint += Workspace_Paint;

            // Добавляем на форму
            this.Controls.Add(workspace);
            this.Controls.Add(truthTablePanel);
            this.Controls.Add(inputPanel);
            this.Controls.Add(toolPanel);
        }

        private void CreateToolPanel()
        {
            int yPos = 20;
            int buttonHeight = 40;
            int buttonWidth = 180;

            Label title = new Label
            {
                Text = "КОМПОНЕНТЫ",
                Location = new Point(10, yPos),
                Size = new Size(buttonWidth, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue
            };
            toolPanel.Controls.Add(title);
            yPos += 35;

            var components = new[]
            {
                new { Text = "И (AND)", Color = Color.LightBlue },
                new { Text = "ИЛИ (OR)", Color = Color.LightSkyBlue },
                new { Text = "НЕ (NOT)", Color = Color.RoyalBlue },
                new { Text = "XOR", Color = Color.DeepSkyBlue },
                new { Text = "Вход", Color = Color.LightPink },
                new { Text = "Выход", Color = Color.Pink }
            };

            foreach (var component in components)
            {
                Button btn = new Button
                {
                    Text = component.Text,
                    Location = new Point(10, yPos),
                    Size = new Size(buttonWidth, buttonHeight),
                    Font = new Font("Segoe UI", 10),
                    BackColor = component.Color,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 1, BorderColor = Color.Gray }
                };

                btn.Click += (sender, e) => AddComponent(component.Text);
                toolPanel.Controls.Add(btn);
                yPos += buttonHeight + 5;
            }

            yPos += 10;

            btnConnect = new Button
            {
                Text = "🔗 Режим соединения",
                Location = new Point(10, yPos),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.Gray }
            };
            btnConnect.Click += BtnConnect_Click;
            toolPanel.Controls.Add(btnConnect);
            yPos += buttonHeight + 5;

            btnSimulate = new Button
            {
                Text = "▶ Запустить симуляцию",
                Location = new Point(10, yPos),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.Gray }
            };
            btnSimulate.Click += BtnSimulate_Click;
            toolPanel.Controls.Add(btnSimulate);
            yPos += buttonHeight + 5;

            btnGenerateTruthTable = new Button
            {
                Text = "📊 Сгенерировать таблицу",
                Location = new Point(10, yPos),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.Gray }
            };
            btnGenerateTruthTable.Click += BtnGenerateTruthTable_Click;
            toolPanel.Controls.Add(btnGenerateTruthTable);
            yPos += buttonHeight + 5;

            Button btnClear = new Button
            {
                Text = "🗑 Очистить схему",
                Location = new Point(10, yPos),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.LightCoral,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.Gray }
            };
            btnClear.Click += BtnClear_Click;
            toolPanel.Controls.Add(btnClear);
        }

        private void CreateInputPanel()
        {
            Label title = new Label
            {
                Text = "ЗНАЧЕНИЯ ВХОДОВ",
                Location = new Point(10, 10),
                Size = new Size(280, 30),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue
            };
            inputPanel.Controls.Add(title);

            Label instruction = new Label
            {
                Text = "Установите значения для каждого входа:",
                Location = new Point(10, 45),
                Size = new Size(280, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            inputPanel.Controls.Add(instruction);

            inputValuePanel = new FlowLayoutPanel
            {
                Location = new Point(10, 70),
                Size = new Size(280, 140),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            inputPanel.Controls.Add(inputValuePanel);
        }

        private void CreateTruthTablePanel()
        {
            Label title = new Label
            {
                Text = "ТАБЛИЦА ИСТИННОСТИ",
                Location = new Point(10, 10),
                Size = new Size(280, 30),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue
            };
            truthTablePanel.Controls.Add(title);

            lblTruthTableStatus = new Label
            {
                Text = "Таблица будет сгенерирована после создания схемы",
                Location = new Point(10, 45),
                Size = new Size(280, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            truthTablePanel.Controls.Add(lblTruthTableStatus);

            truthTableGrid = new DataGridView
            {
                Location = new Point(10, 70),
                Size = new Size(280, 340),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9),
                    SelectionBackColor = Color.LightBlue,
                    SelectionForeColor = Color.Black
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    BackColor = Color.LightBlue,
                    ForeColor = Color.Black,
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(240, 248, 255)
                }
            };
            truthTablePanel.Controls.Add(truthTableGrid);
        }

        private void AddComponent(string componentType)
        {
            ILogicGate gate = componentType switch
            {
                "И (AND)" => new AndGate(),
                "ИЛИ (OR)" => new OrGate(),
                "НЕ (NOT)" => new NotGate(),
                "XOR" => new XorGate(),
                "Вход" => new InputGate($"Вход {circuit.Inputs.Count + 1}"),
                "Выход" => new OutputGate($"Выход {circuit.Outputs.Count + 1}"),
                _ => null
            };

            if (gate != null)
            {
                int centerX = workspace.Width / 2 - 40 + (circuit.Gates.Count % 4) * 90;
                int centerY = workspace.Height / 2 - 40 + (circuit.Gates.Count / 4) * 90;
                gate.Location = new Point(centerX, centerY);

                circuit.AddGate(gate);
                workspace.Invalidate();

                if (gate is InputGate)
                    UpdateInputValueControls();

                lblTruthTableStatus.Text = "Создайте соединения и нажмите 'Сгенерировать таблицу'";
            }
        }

        private void UpdateInputValueControls()
        {
            inputValuePanel.Controls.Clear();
            inputControls.Clear();

            for (int i = 0; i < circuit.Inputs.Count; i++)
            {
                var inputGate = circuit.Inputs[i];

                Panel inputControlPanel = new Panel
                {
                    Size = new Size(260, 35),
                    Margin = new Padding(0, 5, 0, 5)
                };

                Label label = new Label
                {
                    Text = $"Вход {i + 1}:",
                    Location = new Point(5, 5),
                    Size = new Size(70, 25),
                    Font = new Font("Segoe UI", 10),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                RadioButton radio0 = new RadioButton
                {
                    Text = "0",
                    Location = new Point(80, 5),
                    Size = new Size(50, 25),
                    Font = new Font("Segoe UI", 10),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                radio0.CheckedChanged += (sender, e) =>
                {
                    if (radio0.Checked)
                    {
                        inputGate.SetValue(false);
                        workspace.Invalidate();

                        if (truthTableGrid.Rows.Count > 0)
                        {
                            UpdateTruthTableForCurrentInputs();
                        }
                    }
                };

                RadioButton radio1 = new RadioButton
                {
                    Text = "1",
                    Location = new Point(140, 5),
                    Size = new Size(50, 25),
                    Font = new Font("Segoe UI", 10),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                radio1.CheckedChanged += (sender, e) =>
                {
                    if (radio1.Checked)
                    {
                        inputGate.SetValue(true);
                        workspace.Invalidate();

                        if (truthTableGrid.Rows.Count > 0)
                        {
                            UpdateTruthTableForCurrentInputs();
                        }
                    }
                };

                bool currentValue = inputGate.GetOutput();
                radio0.Checked = !currentValue;
                radio1.Checked = currentValue;

                inputControlPanel.Controls.Add(label);
                inputControlPanel.Controls.Add(radio0);
                inputControlPanel.Controls.Add(radio1);

                inputValuePanel.Controls.Add(inputControlPanel);

                inputControls.Add(new InputValueControl
                {
                    InputGate = inputGate,
                    Label = label,
                    Radio0 = radio0,
                    Radio1 = radio1
                });
            }

            if (circuit.Inputs.Count == 0)
            {
                Label noInputsLabel = new Label
                {
                    Text = "Добавьте входные элементы на рабочую область",
                    Size = new Size(260, 50),
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                inputValuePanel.Controls.Add(noInputsLabel);
            }
        }

        private void UpdateTruthTableForCurrentInputs()
        {
            if (circuit.Inputs.Count == 0 || circuit.Outputs.Count == 0)
                return;

            bool[] currentInputs = new bool[circuit.Inputs.Count];
            for (int i = 0; i < circuit.Inputs.Count; i++)
            {
                currentInputs[i] = circuit.Inputs[i].GetOutput();
            }

            int currentCombination = 0;
            for (int i = 0; i < currentInputs.Length; i++)
            {
                if (currentInputs[i])
                {
                    currentCombination |= (1 << (currentInputs.Length - 1 - i));
                }
            }

            if (currentCombination >= 0 && currentCombination < truthTableGrid.Rows.Count)
            {
                bool[] outputs = circuit.Simulate(currentInputs);

                for (int i = 0; i < outputs.Length; i++)
                {
                    int outputColumnIndex = circuit.Inputs.Count + 2 + i;
                    if (outputColumnIndex < truthTableGrid.Columns.Count)
                    {
                        truthTableGrid.Rows[currentCombination].Cells[outputColumnIndex].Value = outputs[i] ? "1" : "0";
                    }
                }

                foreach (DataGridViewRow row in truthTableGrid.Rows)
                {
                    row.DefaultCellStyle.BackColor = row.Index == currentCombination ?
                        Color.LightYellow :
                        Color.White;
                }
            }
        }

        private void Workspace_MouseDown(object sender, MouseEventArgs e)
        {
            if (isConnecting)
            {
                ILogicGate clickedGate = null;
                foreach (var gate in circuit.Gates)
                {
                    Rectangle gateBounds = new Rectangle(
                        gate.Location.X - 40,
                        gate.Location.Y - 40,
                        80, 80);

                    if (gateBounds.Contains(e.Location))
                    {
                        clickedGate = gate;
                        break;
                    }
                }

                if (clickedGate != null)
                {
                    if (selectedGate == null)
                    {
                        selectedGate = clickedGate;
                        workspace.Invalidate();
                    }
                    else if (selectedGate != clickedGate)
                    {
                        try
                        {
                            circuit.ConnectGates(selectedGate, clickedGate);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при создании соединения: {ex.Message}",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        selectedGate = null;
                        workspace.Invalidate();

                        lblTruthTableStatus.Text = "Соединение создано. Нажмите 'Сгенерировать таблицу'";
                    }
                }
                else if (selectedGate != null)
                {
                    selectedGate = null;
                    workspace.Invalidate();
                }
            }
            else
            {
                foreach (var gate in circuit.Gates)
                {
                    Rectangle gateBounds = new Rectangle(
                        gate.Location.X - 40,
                        gate.Location.Y - 40,
                        80, 80);

                    if (gateBounds.Contains(e.Location))
                    {
                        isDragging = true;
                        draggedGate = gate;
                        dragOffset = new Point(e.X - gate.Location.X, e.Y - gate.Location.Y);
                        break;
                    }
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                foreach (var gate in circuit.Gates)
                {
                    Rectangle gateBounds = new Rectangle(
                        gate.Location.X - 40,
                        gate.Location.Y - 40,
                        80, 80);

                    if (gateBounds.Contains(e.Location))
                    {
                        if (MessageBox.Show($"Удалить элемент {GetGateDisplayName(gate)}?", "Подтверждение",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            bool wasInput = gate is InputGate;
                            bool wasOutput = gate is OutputGate;

                            circuit.RemoveGate(gate);
                            workspace.Invalidate();

                            if (wasInput)
                                UpdateInputValueControls();

                            ClearTruthTable();
                        }
                        break;
                    }
                }
            }
        }

        private void Workspace_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedGate != null)
            {
                draggedGate.Location = new Point(e.X - dragOffset.X, e.Y - dragOffset.Y);
                workspace.Invalidate();
            }
        }

        private void Workspace_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
                draggedGate = null;
            }
        }

        private void Workspace_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            foreach (var connection in circuit.Connections)
            {
                if (connection.SourceGate != null && connection.TargetGate != null)
                {
                    using (Pen pen = new Pen(Color.Blue, 2))
                    {
                        g.DrawLine(pen, connection.SourceGate.Location, connection.TargetGate.Location);
                        DrawArrow(g, pen, connection.SourceGate.Location, connection.TargetGate.Location);
                    }
                }
            }

            foreach (var gate in circuit.Gates)
            {
                DrawGate(g, gate);
            }

            if (selectedGate != null)
            {
                Rectangle highlight = new Rectangle(
                    selectedGate.Location.X - 45,
                    selectedGate.Location.Y - 45,
                    90, 90);
                using (Pen pen = new Pen(Color.Red, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.DrawRectangle(pen, highlight);
                }
            }

            if (circuit.Connections.Count > 0)
            {
                foreach (var connection in circuit.Connections)
                {
                    if (connection.SourceGate != null && connection.TargetGate != null)
                    {
                        Point midPoint = new Point(
                            (connection.SourceGate.Location.X + connection.TargetGate.Location.X) / 2,
                            (connection.SourceGate.Location.Y + connection.TargetGate.Location.Y) / 2);

                        bool value = connection.SourceGate.GetOutput();

                        using (Font font = new Font("Segoe UI", 10, FontStyle.Bold))
                        {
                            string valueText = value ? "1" : "0";
                            Color textColor = value ? Color.Green : Color.Red;

                            Rectangle circleRect = new Rectangle(midPoint.X - 12, midPoint.Y - 12, 24, 24);
                            g.FillEllipse(Brushes.White, circleRect);
                            g.DrawEllipse(new Pen(textColor, 2), circleRect);

                            SizeF textSize = g.MeasureString(valueText, font);
                            g.DrawString(valueText, font, new SolidBrush(textColor),
                                midPoint.X - textSize.Width / 2,
                                midPoint.Y - textSize.Height / 2);
                        }
                    }
                }
            }
        }

        private void DrawArrow(Graphics g, Pen pen, Point from, Point to)
        {
            float arrowSize = 8;
            float angle = (float)Math.Atan2(to.Y - from.Y, to.X - from.X);

            PointF arrowPoint1 = new PointF(
                to.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
                to.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6));

            PointF arrowPoint2 = new PointF(
                to.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
                to.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6));

            g.DrawLine(pen, to, arrowPoint1);
            g.DrawLine(pen, to, arrowPoint2);
        }

        private void DrawGate(Graphics g, ILogicGate gate)
        {
            Color gateColor = GetGateColor(gate);
            Rectangle rect = new Rectangle(gate.Location.X - 40, gate.Location.Y - 40, 80, 80);

            Rectangle shadowRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
            g.FillRectangle(new SolidBrush(Color.FromArgb(30, Color.Gray)), shadowRect);

            using (Brush brush = new SolidBrush(gateColor))
            {
                g.FillRectangle(brush, rect);
            }
            g.DrawRectangle(new Pen(Color.Black, 2), rect);

            string displayText = GetGateDisplayName(gate);
            using (Font font = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(displayText, font);

                if (textSize.Width > 70 || textSize.Height > 40)
                {
                    font.Dispose();
                    using (Font smallerFont = new Font("Segoe UI", 9, FontStyle.Bold))
                    {
                        textSize = g.MeasureString(displayText, smallerFont);
                        g.DrawString(displayText, smallerFont, Brushes.Black,
                            gate.Location.X - textSize.Width / 2,
                            gate.Location.Y - textSize.Height / 2);
                    }
                }
                else
                {
                    g.DrawString(displayText, font, Brushes.Black,
                        gate.Location.X - textSize.Width / 2,
                        gate.Location.Y - textSize.Height / 2);
                }
            }

            if (gate is InputGate || gate is OutputGate)
            {
                string value = gate.GetOutput() ? "1" : "0";
                using (Font valueFont = new Font("Segoe UI", 11, FontStyle.Bold))
                {
                    Rectangle valueRect = new Rectangle(gate.Location.X + 15, gate.Location.Y - 35, 30, 30);
                    g.FillEllipse(gate.GetOutput() ? Brushes.LightGreen : Brushes.LightCoral, valueRect);
                    g.DrawEllipse(new Pen(Color.Black, 1), valueRect);

                    g.DrawString(value, valueFont,
                        gate.GetOutput() ? Brushes.Green : Brushes.Red,
                        gate.Location.X + 20,
                        gate.Location.Y - 30);
                }
            }
        }

        private Color GetGateColor(ILogicGate gate)
        {
            if (gate is AndGate) return Color.LightBlue;
            if (gate is OrGate) return Color.LightSkyBlue;
            if (gate is NotGate) return Color.RoyalBlue;
            if (gate is XorGate) return Color.DeepSkyBlue;
            if (gate is InputGate) return Color.LightPink;
            if (gate is OutputGate) return Color.Pink;
            return Color.White;
        }

        private string GetGateDisplayName(ILogicGate gate)
        {
            if (gate is AndGate) return "AND";
            if (gate is OrGate) return "OR";
            if (gate is NotGate) return "NOT";
            if (gate is XorGate) return "XOR";
            if (gate is InputGate input)
            {
                int index = circuit.Inputs.IndexOf(input);
                return $"IN {index + 1}";
            }
            if (gate is OutputGate output)
            {
                int index = circuit.Outputs.IndexOf(output);
                return $"OUT {index + 1}";
            }
            return "GATE";
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            isConnecting = !isConnecting;
            if (isConnecting)
            {
                btnConnect.BackColor = Color.Yellow;
                btnConnect.Text = "🔗 Режим соединения (ВКЛ)";
                workspace.Invalidate();
            }
            else
            {
                btnConnect.BackColor = Color.LightBlue;
                btnConnect.Text = "🔗 Режим соединения";
                selectedGate = null;
                workspace.Invalidate();
            }
        }

        private void BtnSimulate_Click(object sender, EventArgs e)
        {
            if (circuit.Inputs.Count == 0 || circuit.Outputs.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один вход и один выход.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (circuit.Connections.Count == 0)
            {
                MessageBox.Show("Создайте соединения между элементами.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool[] inputValues = new bool[circuit.Inputs.Count];
            for (int i = 0; i < circuit.Inputs.Count; i++)
            {
                inputValues[i] = circuit.Inputs[i].GetOutput();
            }

            try
            {
                bool[] results = circuit.Simulate(inputValues);

                workspace.Invalidate();

                string resultMessage = "Результаты симуляции:\n\n";
                for (int i = 0; i < inputValues.Length; i++)
                {
                    resultMessage += $"Вход {i + 1}: {(inputValues[i] ? "1" : "0")}\n";
                }
                resultMessage += "\n";
                for (int i = 0; i < results.Length; i++)
                {
                    resultMessage += $"Выход {i + 1}: {(results[i] ? "1" : "0")}\n";
                }

                MessageBox.Show(resultMessage, "Результат симуляции",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (truthTableGrid.Rows.Count > 0)
                {
                    UpdateTruthTableForCurrentInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при симуляции: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerateTruthTable_Click(object sender, EventArgs e)
        {
            if (circuit.Inputs.Count == 0)
            {
                MessageBox.Show("Для построения таблицы истинности нужны входные элементы.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (circuit.Outputs.Count == 0)
            {
                MessageBox.Show("Для построения таблицы истинности нужны выходные элементы.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (circuit.Connections.Count == 0)
            {
                MessageBox.Show("Для построения таблицы истинности создайте соединения между элементами.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            GenerateAndDisplayTruthTable();
        }

        private void GenerateAndDisplayTruthTable()
        {
            try
            {
                int inputCount = circuit.Inputs.Count;
                int outputCount = circuit.Outputs.Count;

                truthTableGrid.Rows.Clear();
                truthTableGrid.Columns.Clear();

                truthTableGrid.Columns.Add("Combination", "№");
                truthTableGrid.Columns["Combination"].Width = 40;

                for (int i = 0; i < inputCount; i++)
                {
                    string colName = $"Input{i + 1}";
                    truthTableGrid.Columns.Add(colName, $"I{i + 1}");
                    truthTableGrid.Columns[colName].DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 245);
                    truthTableGrid.Columns[colName].Width = 40;
                }

                truthTableGrid.Columns.Add("Separator", "→");
                truthTableGrid.Columns["Separator"].Width = 30;
                truthTableGrid.Columns["Separator"].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                truthTableGrid.Columns["Separator"].DefaultCellStyle.BackColor = Color.LightGray;
                truthTableGrid.Columns["Separator"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                for (int i = 0; i < outputCount; i++)
                {
                    string colName = $"Output{i + 1}";
                    truthTableGrid.Columns.Add(colName, $"O{i + 1}");
                    truthTableGrid.Columns[colName].DefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);
                    truthTableGrid.Columns[colName].Width = 40;
                }

                int combinations = (int)Math.Pow(2, inputCount);

                for (int i = 0; i < combinations; i++)
                {
                    bool[] inputs = new bool[inputCount];
                    for (int j = 0; j < inputCount; j++)
                    {
                        inputs[j] = ((i >> (inputCount - 1 - j)) & 1) == 1;
                    }

                    for (int j = 0; j < inputCount; j++)
                    {
                        circuit.Inputs[j].SetValue(inputs[j]);
                    }

                    bool[] outputs = circuit.Simulate(inputs);

                    object[] rowValues = new object[inputCount + outputCount + 2];
                    rowValues[0] = i + 1;

                    for (int j = 0; j < inputCount; j++)
                    {
                        rowValues[j + 1] = inputs[j] ? "1" : "0";
                    }

                    rowValues[inputCount + 1] = "→";

                    for (int j = 0; j < outputCount; j++)
                    {
                        rowValues[j + inputCount + 2] = outputs[j] ? "1" : "0";
                    }

                    truthTableGrid.Rows.Add(rowValues);

                    DataGridViewRow row = truthTableGrid.Rows[truthTableGrid.Rows.Count - 1];
                    bool hasOutputOne = false;
                    for (int j = 0; j < outputCount && j < outputs.Length; j++)
                    {
                        if (outputs[j])
                        {
                            hasOutputOne = true;
                            row.Cells[j + inputCount + 2].Style.ForeColor = Color.Green;
                            row.Cells[j + inputCount + 2].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                        }
                    }

                    if (hasOutputOne)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
                    }
                }

                UpdateInputValueControls();

                lblTruthTableStatus.Text = $"Таблица сгенерирована: {inputCount} вход(ов), {outputCount} выход(ов), {combinations} комбинаций";
                truthTableGrid.AutoResizeColumns();

                UpdateTruthTableForCurrentInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации табЙЙёлицы истинности: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearTruthTable()
        {
            truthTableGrid.Rows.Clear();
            truthTableGrid.Columns.Clear();
            lblTruthTableStatus.Text = "Таблица будет сгенерирована после создания схемы";
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите очистить схему?\nВсе элементы и соединения будут удалены.", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                circuit = new Circuit();
                UpdateInputValueControls();
                ClearTruthTable();
                selectedGate = null;
                isConnecting = false;
                btnConnect.BackColor = Color.LightBlue;
                btnConnect.Text = "🔗 Режим соединения";
                workspace.Invalidate();
            }
        }
    }
}
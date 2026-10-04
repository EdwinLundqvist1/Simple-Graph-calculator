using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Drawing2D;
using System.Security.Cryptography.X509Certificates;

namespace GrafRaknare;

public partial class Form1 : Form
{
    private List<(double X, double Y)> graphPoints = new();
    private double viewMinX = -10f;
    private double viewMaxX = 10f;
    private double viewMinY = -10f;
    private double viewMaxY = 10f;
    private TextBox inputTextBox = null!;
    private Button drawButton = null!;
    private ListBox coordinateListBox = null!;
    private PictureBox graphPictureBox = null!;
    private bool isPanning;
    private Point lastMousePosition;
    public Form1()
    {
        InitializeComponent();
        SetupUI();
        // Gör fönstret lite större så vi får plats med grafen
        this.Width = 1000;
        this.Height = 750;
        this.Text = "GrafRäknare v1.0";
    }


    // Detta är programmets viktigaste "rit-event" som körs varje gång fönstret behöver ritas
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
    }

    private void SetupUI()
    {
        // 1. Etikett ovanför textrutan
        Label inputLabel = new Label();
        inputLabel.Text = "Skriv in funktion:";
        inputLabel.Location = new Point(10, 10);
        inputLabel.AutoSize = true;
        this.Controls.Add(inputLabel);

        // 2. Textruta där användaren skriver sin formel
        inputTextBox = new TextBox();
        inputTextBox.Location = new Point(10, 30);
        inputTextBox.Size = new Size(180, 25);
        inputTextBox.Text = "x * 2 - 3"; // Standardtext
        this.Controls.Add(inputTextBox);

        // 3. Knappen för att starta beräkningen
        drawButton = new Button();
        drawButton.Text = "Rita Graf";
        drawButton.Location = new Point(10, 65);
        drawButton.Size = new Size(180, 45);
        drawButton.Font = new Font(this.Font.FontFamily, 11, FontStyle.Bold);
        // Koppla klick-eventet till en metod
        drawButton.Click += DrawButton_Click!;
        this.Controls.Add(drawButton);



        // 4. Etikett ovanför koordinatlistan
        Label listLabel = new Label();
        listLabel.Text = "Koordinater (X, Y):";
        listLabel.Location = new Point(10, 125);
        listLabel.AutoSize = true;
        this.Controls.Add(listLabel);

        // 5. ListBox för att lista alla uträknade punkter
        coordinateListBox = new ListBox();
        coordinateListBox.Location = new Point(10, 145);
        coordinateListBox.Size = new Size(180, 330);
        this.Controls.Add(coordinateListBox);
        List<object> Coordinate = AddWholeNumebersToListBox();
        for (int i = 0; i < Coordinate.Count; i++)
        {
            coordinateListBox.Items.Add(Coordinate[i]);
        }
        

        // 6. Ritytan (PictureBox) till höger där grafen hamnar
        graphPictureBox = new PictureBox();
        graphPictureBox.Location = new Point(200, 10);
        graphPictureBox.Size = new Size(670, 465);
        graphPictureBox.BackColor = Color.White;
        graphPictureBox.BorderStyle = BorderStyle.FixedSingle;
        // Koppla PictureBoxens rit-event till vår metod
        graphPictureBox.Paint += GraphPictureBox_Paint!;
        graphPictureBox.MouseWheel += GraphPictureBox_MouseWheel!;
        graphPictureBox.MouseDown += GraphPictureBox_MouseDown!;
        graphPictureBox.MouseMove += GraphPictureBox_MouseMove!;
        graphPictureBox.MouseUp += GraphPictureBox_MouseUp!;
        this.Controls.Add(graphPictureBox);
    }

    private void DrawButton_Click(object sender, EventArgs e)
    {
        // Töm listan på gamla koordinater
        try
        {
            coordinateListBox.Items.Clear();
            string expression = inputTextBox.Text;
            int width = graphPictureBox.Width;
            int height = graphPictureBox.Height;
            foreach (object coordinate in AddWholeNumebersToListBox())
            {
                coordinateListBox.Items.Add(coordinate);
            }
            graphPoints =  BuildGraphPoints(expression, width, height);
            AutoScaleY();
            graphPictureBox.Invalidate();
            }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not graph this expression: {ex.Message}");
        }

    }

    private void AutoScaleY()
    {
        List<double> yValues = graphPoints
            .Where(point => double.IsFinite(point.Y))
            .Select(point => point.Y)
            .ToList();
        if (yValues.Count == 0)
        {
            return;
        }

        double minY = yValues.Min();
        double maxY = yValues.Max();
        double padding = minY == maxY ? Math.Max(1, Math.Abs(minY) * 0.05) : (maxY - minY) * 0.05;
        
        viewMinY = minY - padding;
        viewMaxY = maxY + padding;
    }

    // Denna metod ritar allt som syns inne i den vita rutan
    private void GraphPictureBox_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        Pen blackPen = new Pen(Color.Black, 2);

        int width = graphPictureBox.Width;
        int height = graphPictureBox.Height;
        double xAxisY = height - (0 - viewMinY) / (viewMaxY - viewMinY) * height;
        double yAxisX = (0 - viewMinX) / (viewMaxX - viewMinX) * width;


        for (int i = 0; i < graphPoints.Count; i++)
        {
            double x = graphPoints[i].X;
            double y = graphPoints[i].Y;

            double scaledX = (x - viewMinX) / (viewMaxX - viewMinX) * width;
            double scaledY = height - (y - viewMinY) / (viewMaxY - viewMinY) * height;

            if (i > 0)
            {
                double previousX = graphPoints[i-1].X;
                double previousY = graphPoints[i-1].Y;
                
                double previousScaledX = (previousX - viewMinX) / (viewMaxX - viewMinX) * width;
                double previousScaledY = height - (previousY - viewMinY) / (viewMaxY - viewMinY) * height;
                if (IsVisiblePoint(previousScaledX, previousScaledY, width, height) && IsVisiblePoint(scaledX, scaledY, width, height))
                {
                    g.DrawLine(Pens.Blue,(float)previousScaledX,(float)previousScaledY,(float)scaledX,(float)scaledY);
                }
            }
            else if (IsVisiblePoint(scaledX, scaledY, width, height))
            {
                g.FillEllipse(Brushes.Blue, (float)scaledX - 2, (float)scaledY - 2, 4, 4);
            }
        }

        
        // Rita X-axel (vågrät) och Y-axel (lodrät) i mitten av den vita ritan
        if (double.IsFinite(xAxisY) && xAxisY >= 0 && xAxisY <= height)
        {
            g.DrawLine(blackPen, 0, (float)xAxisY, width, (float)xAxisY);
        }
        if (double.IsFinite(yAxisX) && yAxisX >= 0 && yAxisX <= width)
        {
        g.DrawLine(blackPen, (float)yAxisX, 0, (float)yAxisX, height);
        }
    }
    private static bool IsVisiblePoint(double x, double y, int width, int height)
    {
        return double.IsFinite(x) && double.IsFinite(y)
            && x >= 0 && x <= width
            && y >= 0 && y <= height;
    }

    public List<(double X, double Y)> BuildGraphPoints(string expression, int width, int height)
    {
        List<(double X, double Y)> points = new();
        double step = (viewMaxX - viewMinX) / width;
        object[] tokenizedExpression = new ExpressionParser().ExpressionFormatter(expression);
        for (double x = viewMinX; x <= viewMaxX; x += step)
        {
            double y = new Kalkylering().CheckExpression(tokenizedExpression, x);
            points.Add(new (x,y));
        }
        return points;
    }

    private List<object> AddWholeNumebersToListBox()
    {
        List<object> coordinates = new List<object>();

        for (double i = viewMinX; i <= viewMaxX; i++)
        {
            double y = new Kalkylering().CheckExpression(new ExpressionParser().ExpressionFormatter(inputTextBox.Text), i);
            coordinates.Add($"({i}, {y})");
        }
        return coordinates;
    }

    private void GraphPictureBox_MouseWheel(object sender, MouseEventArgs e)
    {
        double zoomFactor = e.Delta > 0 ? 0.8f : 1.25f;

        double centerX = (viewMinX + viewMaxX) / 2;
        double centerY = (viewMinY + viewMaxY) / 2;

        double halfWidth = (viewMaxX - viewMinX) / 2 * zoomFactor;
        double halfHeight = (viewMaxY - viewMinY) / 2 * zoomFactor;

        viewMinX = centerX - halfWidth;
        viewMaxX = centerX + halfWidth;
        viewMinY = centerY - halfHeight;
        viewMaxY = centerY + halfHeight;

        graphPoints = BuildGraphPoints(inputTextBox.Text, graphPictureBox.Width, graphPictureBox.Height);
        coordinateListBox.Items.Clear();

        foreach (object coordinate in AddWholeNumebersToListBox())
        {
            coordinateListBox.Items.Add(coordinate);
        }


        graphPictureBox.Invalidate();
    }

    private void GraphPictureBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            graphPictureBox.Capture = true;
            isPanning = true;
            lastMousePosition = e.Location;
        }
    }

    private void GraphPictureBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            graphPictureBox.Capture = false;
            isPanning = false;
        }
    }

    private void GraphPictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isPanning)
        {
            return;
        }

        double deltaX = e.X - lastMousePosition.X;
        double deltaY = e.Y - lastMousePosition.Y;

        double graphWidth = viewMaxX - viewMinX;
        double graphHeight = viewMaxY - viewMinY;

        double unitsPerPixelX = graphWidth / graphPictureBox.Width;
        double unitsPerPixelY = graphHeight / graphPictureBox.Height;

        viewMinX -= deltaX * unitsPerPixelX;
        viewMaxX -= deltaX * unitsPerPixelX;

        viewMinY += deltaY * unitsPerPixelY;
        viewMaxY += deltaY * unitsPerPixelY;

        lastMousePosition = e.Location;

        graphPoints = BuildGraphPoints(inputTextBox.Text, graphPictureBox.Width, graphPictureBox.Height);
        graphPictureBox.Invalidate();
    }
}
using System.Drawing.Drawing2D;
using System.Security.Cryptography.X509Certificates;

namespace GrafRaknare;

public partial class Form1 : Form
{
    private List<PointF> graphPoints = new();
    private float viewMinX = -10f;
    private float viewMaxX = 10f;
    private float viewMinY = -10f;
    private float viewMaxY = 10f;
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
        coordinateListBox.Items.Clear();
        string expression = inputTextBox.Text;
        int width = graphPictureBox.Width;
        int height = graphPictureBox.Height;
        foreach (object coordinate in AddWholeNumebersToListBox())
        {
            coordinateListBox.Items.Add(coordinate);
        }
        graphPoints =  BuildGraphPoints(expression, width, height);
        graphPictureBox.Invalidate();

    }

    // Denna metod ritar allt som syns inne i den vita rutan
    private void GraphPictureBox_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        Pen blackPen = new Pen(Color.Black, 2);

        int width = graphPictureBox.Width;
        int height = graphPictureBox.Height;
        float xAxisY = height - (0 - viewMinY) / (viewMaxY - viewMinY) * height;
        float yAxisX = (0 - viewMinX) / (viewMaxX - viewMinX) * width;


        for (int i = 0; i < graphPoints.Count; i++)
        {
            float x = graphPoints[i].X;
            float y = graphPoints[i].Y;

            float scaledX = (x - viewMinX) / (viewMaxX - viewMinX) * width;
            float scaledY = height - (y - viewMinY) / (viewMaxY - viewMinY) * height;



            if (i > 0)
            {
                float previousX = graphPoints[i-1].X;
                float previousY = graphPoints[i-1].Y;
                
                float previousScaledX = (previousX - viewMinX) / (viewMaxX - viewMinX) * width;
                float previousScaledY = height - (previousY - viewMinY) / (viewMaxY - viewMinY) * height;

                g.DrawLine(Pens.Blue, previousScaledX, previousScaledY, scaledX, scaledY);
            }
            else
            {
                g.FillEllipse(Brushes.Blue, scaledX - 2, scaledY - 2, 4, 4);
            }
        }

        
        // Rita X-axel (vågrät) och Y-axel (lodrät) i mitten av den vita ritan
        g.DrawLine(blackPen, 0, xAxisY, width, xAxisY);
        g.DrawLine(blackPen, yAxisX, 0, yAxisX, height);
    }

    public List<PointF> BuildGraphPoints(string expression, int width, int height)
    {
        List<PointF> points = new List<PointF>();
        double step = (viewMaxX - viewMinX) / width;
        object[] tokenizedExpression = new ExpressionParser().ExpressionFormatter(expression);
        for (double x = viewMinX; x <= viewMaxX; x += step)
        {
            double y = new Kalkylering().CheckExpression(tokenizedExpression, x);
            points.Add(new PointF((float)x, (float)y));
        }
        return points;
    }

    private List<object> AddWholeNumebersToListBox()
    {
        List<object> coordinates = new List<object>();

        for (int i = (int)viewMinX; i <= (int)viewMaxX; i++)
        {
            int y = (int)new Kalkylering().CheckExpression(new ExpressionParser().ExpressionFormatter(inputTextBox.Text), i);
            coordinates.Add($"({i}, {y})");
        }
        return coordinates;
    }

    private void GraphPictureBox_MouseWheel(object sender, MouseEventArgs e)
    {
        float zoomFactor = e.Delta > 0 ? 0.8f : 1.25f;

        float centerX = (viewMinX + viewMaxX) / 2;
        float centerY = (viewMinY + viewMaxY) / 2;

        float halfWidth = (viewMaxX - viewMinX) / 2 * zoomFactor;
        float halfHeight = (viewMaxY - viewMinY) / 2 * zoomFactor;

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

        float deltaX = e.X - lastMousePosition.X;
        float deltaY = e.Y - lastMousePosition.Y;

        float graphWidth = viewMaxX - viewMinX;
        float graphHeight = viewMaxY - viewMinY;

        float unitsPerPixelX = graphWidth / graphPictureBox.Width;
        float unitsPerPixelY = graphHeight / graphPictureBox.Height;

        viewMinX -= deltaX * unitsPerPixelX;
        viewMaxX -= deltaX * unitsPerPixelX;

        viewMinY += deltaY * unitsPerPixelY;
        viewMaxY += deltaY * unitsPerPixelY;

        lastMousePosition = e.Location;

        graphPoints = BuildGraphPoints(inputTextBox.Text, graphPictureBox.Width, graphPictureBox.Height);
        graphPictureBox.Invalidate();
    }
}
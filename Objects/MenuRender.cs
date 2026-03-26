using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class MenuRenderer : ToolStripProfessionalRenderer
{
    // Colori Windows 11 Dark Mode
    private Color BackgroundColor = Color.FromArgb(32, 32, 32);
    private Color SelectionColor = Color.FromArgb(60, 60, 60);
    private Color BorderColor = Color.FromArgb(45, 45, 45);
    private Color TextColor = Color.White;

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item.Selected)
        {
            // Disegna il rettangolo di selezione con angoli arrotondati (hover)
            Rectangle rect = new Rectangle(3, 1, e.Item.Width - 6, e.Item.Height - 2);
            using (GraphicsPath path = GetRoundedRect(rect, 4))
            using (SolidBrush brush = new SolidBrush(SelectionColor))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            }
        }
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        // Colore della linea (un grigio scuro che stacca appena dallo sfondo #202020)
        Color separatorColor = Color.FromArgb(55, 55, 55); 
        
        // Definiamo l'area della linea
        // Togliamo 10 pixel a destra e sinistra per non far toccare i bordi (stile Win11)
        int padding = 5;
        int y = e.Item.Height / 2;
        int width = e.Item.Width - (padding * 2);

        using (Pen pen = new Pen(separatorColor, 1)) // Spessore 1 pixel
        {
            e.Graphics.DrawLine(pen, padding, y, padding + width, y);
        }
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        // Sfondo principale del menu
        e.Graphics.FillRectangle(new SolidBrush(BackgroundColor), e.AffectedBounds);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        // Rimuove la colonna laterale standard delle icone per un look più pulito
        e.Graphics.FillRectangle(new SolidBrush(BackgroundColor), e.AffectedBounds);
    }

    // Helper per creare i rettangoli arrotondati
    private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        float diameter = radius * 2f;
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = Color.White; // Forza la freccia a essere bianca
        base.OnRenderArrow(e);
    }
}
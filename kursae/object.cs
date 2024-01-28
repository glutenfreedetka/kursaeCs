using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Object
{
    Point position;
    Point scale;
    public void SetPosition(Point value) => position = value;
    public void SetScale(Point value) => scale = value;
    public int X => position.X;
    public int Y => position.Y;
    public int Width => scale.X;
    public int Height => scale.Y;
    public abstract void Draw(Graphics g);

}
public class Ship
{
    static Image image;
    static int Width, Height;
    static public void SetImage(Image value) => image = value; 
    static public void SetSize(Point size)
    {
        Width = size.X;
        Height = size.Y;
    }
    public static void Draw(Graphics g, int X, int Y) => g.DrawImage(image, X, Y, Width, Height); //рисуем
}
public class Cell : Object
{
    bool breaking = false; //ячейка сломана
    bool forPlayer = false; //ячейка игрока
    bool ship = false; //на ячейки корабль
    public Cell(Point position, int size, bool forPlayer) //инициализируем
    {
        SetPosition(position);
        SetScale(new Point(size, size));
        this.forPlayer = forPlayer;
    }
    public void SetShip(bool value) => ship = value; //устанавливаем корабль
    public bool isShip => ship;
    public bool isBreaking => breaking;
    public override void Draw(Graphics g) //рисуем
    {
        if (!breaking) // проверяем ячейку на попадание
        {
            g.DrawRectangle(new Pen(Color.White, 1), X, Y, Width, Height); //рисуем сетку
            if (forPlayer && ship) //если ячейка игрока и на ней корабль, то рисуем на ней корабль
                Ship.Draw(g, X, Y);
        }
        else
            g.FillRectangle(Brushes.AliceBlue, X, Y, Width, Height); //если ячека повреждена то закрашиваем её
    }
    public bool AttackCell() => breaking = true; //попадаем в ячейку
    public bool AttackCell(Point pos) //проверяем попадание в ячейку по координатам
    {
        if (pos.X > X && pos.X < X + Width && pos.Y > Y && pos.Y < Y + Height && !breaking)
            return AttackCell();
        return false; //если ячейка уже разбита или игрок не нажал на неё, то не производим с ней действий
    }
}
class Effect : Object
{
    static Image image; //изображение взрыва
    public static void SetImage(Image value) => image = value; //установка изображения
    public Effect(Point position, Point size) //инициализация
    {
        SetPosition(position);
        SetScale(size);
    }

    public override void Draw(Graphics g) => g.DrawImage(image, X, Y, Width, Height); //рисуем
}

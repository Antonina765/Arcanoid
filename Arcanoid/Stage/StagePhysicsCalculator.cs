using System;
using Arcanoid.Models;

namespace Arcanoid.Stage;

public class StagePhysicsCalculator
{
    /// <summary>
    /// Расчитывает время столкновения двух объектов (если они будут двигаться равномерно).
    /// Возвращает время (в секундах) до столкновения или null, если столкновения не предвидится.
    /// </summary>
    public static double? PredictCollisionTime(DisplayObject obj1, DisplayObject obj2) 
    { 
        // Предполагаем, что позиции задаются как верхний левый угол, поэтому добавляем смещение для центра.
        double x1 = obj1.X + obj1.Size[0] / 2.0; 
        double y1 = obj1.Y + obj1.Size[0] / 2.0; 
        double x2 = obj2.X + obj2.Size[0] / 2.0; 
        double y2 = obj2.Y + obj2.Size[0] / 2.0;
        
        // Определяем скорости в декартовой форме.
        double vx1 = obj1.Speed * Math.Cos(obj1.AngleSpeed); 
        double vy1 = obj1.Speed * Math.Sin(obj1.AngleSpeed); 
        double vx2 = obj2.Speed * Math.Cos(obj2.AngleSpeed); 
        double vy2 = obj2.Speed * Math.Sin(obj2.AngleSpeed);
        
        // Относительные координаты и скорость: рассматриваем движение obj1 относительно obj2.
        double dx = x1 - x2; 
        double dy = y1 - y2; 
        double dvx = vx1 - vx2; 
        double dvy = vy1 - vy2;
        
        // Радиусы (предполагаем, что фигура круглая, радиус – половина размера).
        double r1 = obj1.Size[0] / 2.0; 
        double r2 = obj2.Size[0] / 2.0; 
        double rSum = r1 + r2;
        
        // Решаем квадратное уравнение a*t^2 + b*t + c = 0.
        double a = dvx * dvx + dvy * dvy; 
        double b = 2 * (dx * dvx + dy * dvy); 
        double c = dx * dx + dy * dy - rSum * rSum;
        
        // Если относительная скорость равна нулю, объекты либо уже столкнулись, либо не будут сближаться.
        if (a == 0) 
        { 
            return (c <= 0) ? 0 : (double?)null;
        }
        
        double discriminant = b * b - 4 * a * c; 
        if (discriminant < 0) 
            return null;  // Нет вещественных корней – столкновения не предвидится.
            
        double sqrtDisc = Math.Sqrt(discriminant); 
        double t1 = (-b - sqrtDisc) / (2 * a); 
        double t2 = (-b + sqrtDisc) / (2 * a);
        
        // Выбираем самое раннее положительное время столкновения.
        double collisionTime = double.MaxValue; 
        if (t1 >= 0 && t1 < collisionTime) collisionTime = t1; 
        if (t2 >= 0 && t2 < collisionTime) collisionTime = t2;
        
        return (collisionTime == double.MaxValue) ? null : (double?)collisionTime;
    }
}


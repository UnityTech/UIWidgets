namespace UIWidgets.math
{
    public class MathUtil
    {
        public static double Clamp(double value, double min, double max)
        {
            if ((double) value < (double) min)
                value = min;
            else if ((double) value > (double) max)
                value = max;
            return value;
        }
    }
}
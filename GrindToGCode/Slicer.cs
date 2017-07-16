using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrindToGCode
{
    public class Slicer
    {
        public Slicer()
        {
            xMin = 0;
            xMax = 275;

            yMin = 0;
            yMax = 230;

            zStart = 23.2;
            zLayerHeight = 0.01;
            zNumLayers = 1;
            traceWidth = 2;

            layerRepeat = 10;
            travelSpeed = 150;
            grindPattern = GrindPattern.Diagonal_RectX;

            grindPatternValues = new Dictionary<GrindPattern, string>();
            grindPatternValues.Add(GrindPattern.RectX, "Rectangle X");
            grindPatternValues.Add(GrindPattern.RectY, "Rectangle Y");
            grindPatternValues.Add(GrindPattern.RectX_RectY, "Rectangle X+Y");
            grindPatternValues.Add(GrindPattern.Diagonal1, "Diagonal 1");
            grindPatternValues.Add(GrindPattern.Diagonal2, "Diagonal 2");
            grindPatternValues.Add(GrindPattern.Diagonal1_Diagonal2, "Diagonal 1+2");
            grindPatternValues.Add(GrindPattern.RectX_Diagonal1_Diagonal2_RectX, "RectX + Diagonal 1+2 + RectX");
            grindPatternValues.Add(GrindPattern.Diagonal_RectX, "Diagonal + Rectangle");
        }

        #region public properties

        public enum GrindPattern
        {
            RectX,
            RectY,
            RectX_RectY,
            Diagonal1,
            Diagonal2,
            Diagonal1_Diagonal2,
            RectX_Diagonal1_Diagonal2_RectX,
            Diagonal_RectX,
        }
        
        public int xMin { get; set; }
        public int xMax { get; set; }

        public int yMin { get; set; }
        public int yMax { get; set; }

        public double zStart { get; set; }
        public double zLayerHeight { get; set; }
        public int zNumLayers { get; set; }

        public int layerRepeat { get; set; }
        public double traceWidth { get; set; }

        public GrindPattern grindPattern { get; set; }
        public Dictionary<GrindPattern, string> grindPatternValues { get; set; }

        public int travelSpeed { get; set; }

        #endregion

        private string slice_pattern_diagonal1()
        {
            string gcode = "";
            string speed = "F" + (travelSpeed * 60).ToString();

            double offset_x = xMin;
            double offset_y = yMin;

            double y1 = yMax - traceWidth;
            double x1 = xMin;
            double y2 = yMax;
            double x2 = xMin + traceWidth;
            
            gcode += "G1 X" + xMin.ToString("F3") + " Y" + yMin.ToString("F3") + " " + speed + Environment.NewLine;
            gcode += "G1 X" + x1.ToString("F3") + " Y" + y1.ToString("F3") + " " + speed + Environment.NewLine;

            //von links hinten aus diaogonal
            while (y1 >= yMin && x2 <= xMax)
            {
                gcode += "G1 X" + x2.ToString("F3") + " Y" + y2.ToString("F3") + " " + speed + Environment.NewLine;
                gcode += "G1 X" + x1.ToString("F3") + " Y" + y1.ToString("F3") + " " + speed + Environment.NewLine;

                y1 -= traceWidth;                
                x2 += traceWidth;                
            }

            if(y1 < yMin)
            {
                //y ist kürzer als X
                y1 = yMin;
                while(x1 <= xMax && x2 <= xMax)
                {
                    gcode += "G1 X" + x2.ToString("F3") + " Y" + y2.ToString("F3") + " " + speed + Environment.NewLine;
                    gcode += "G1 X" + x1.ToString("F3") + " Y" + y1.ToString("F3") + " " + speed + Environment.NewLine;

                    x1 += traceWidth;
                    x2 += traceWidth;
                }
            }
            else if(x2 > xMax)
            {
                //x ist kürzer als y
                x2 = xMax;
                while (y1 >= yMin && y2 >= yMin)
                {
                    gcode += "G1 X" + x2.ToString("F3") + " Y" + y2.ToString("F3") + " " + speed + Environment.NewLine;
                    gcode += "G1 X" + x1.ToString("F3") + " Y" + y1.ToString("F3") + " " + speed + Environment.NewLine;

                    y1 -= traceWidth;
                    y2 -= traceWidth;
                }
            }

            if(x2 > xMax)
            {
                x2 = xMax;
                while(x1 <= xMax && y2 >= yMin)
                {
                    gcode += "G1 X" + x2.ToString("F3") + " Y" + y2.ToString("F3") + " " + speed + Environment.NewLine;
                    gcode += "G1 X" + x1.ToString("F3") + " Y" + y1.ToString("F3") + " " + speed + Environment.NewLine;

                    x1 += traceWidth;
                    y2 -= traceWidth;
                }
            }
            else if(y1 < yMin)
            {
                y1 = yMin;
                while(x1 <= xMax && y2 >= yMin)
                {
                    gcode += "G1 X" + x2.ToString("F3") + " Y" + y2.ToString("F3") + " " + speed + Environment.NewLine;
                    gcode += "G1 X" + x1.ToString("F3") + " Y" + y1.ToString("F3") + " " + speed + Environment.NewLine;

                    x1 += traceWidth;
                    y2 -= traceWidth;
                }
            }            
            
            gcode += "G1 X" + xMin.ToString("F3") + " Y" + yMin.ToString("F3") + " " + speed + Environment.NewLine;

            return gcode;
        }

        public class Rect
        {
            public double x1 { get; set; } = 0;
            public double y1 { get; set; } = 0;

            public double x2 { get; set; } = 0;
            public double y2 { get; set; } = 0;

            public double offset_x { get; private set; } = 0;
            public double offset_y { get; private set; } = 0;            

            public double x_offs(double x)
            {
                return x + offset_x;
            }

            public double y_offs(double y)
            {
                return y + offset_y;
            }

            public void UpdateOffset()
            {
                double temp;

                if (x1 < 0)
                    x1 = 0;
                if (x2 < 0)
                    x2 = 0;
                if (y1 < 0)
                    y1 = 0;
                if (y2 < 0)
                    y2 = 0;

                if (x1 > x2)
                {
                    temp = x1;
                    x1 = x2;
                    x2 = temp;
                }
                
                if(y1 > y2)
                {
                    temp = y1;
                    y1 = y2;
                    y2 = temp;
                }

                //update x offset;
                if (x1 > 0)
                {
                    offset_x = x1;
                    x1 = 0;
                    x2 -= offset_x;
                }

                if (y1 > 0)
                {
                    offset_y = y1;
                    y1 = 0;
                    y2 -= offset_y;
                }
            }
        }

        private string slice_pattern_diagonal2()
        {
            string gcode = "";
            string speed = "F" + (travelSpeed * 60).ToString();

            Rect bed = new Rect();
            bed.x1 = xMin;
            bed.y1 = yMin;
            bed.x2 = xMax;
            bed.y2 = yMax;
            bed.UpdateOffset();

            double y1 = bed.y1;
            double x1 = bed.x1 + traceWidth;
            double y2 = bed.y1 + traceWidth;
            double x2 = bed.x1;
            
            gcode += "G1 X" + bed.x_offs(bed.x1).ToString("F3") + " Y" + bed.y_offs(bed.y1).ToString("F3") + " " + speed + Environment.NewLine;
            gcode += "G1 X" + bed.x_offs(x1).ToString("F3") + " Y" + bed.y_offs(y1).ToString("F3") + " " + speed + Environment.NewLine;

            //von vorne links aus diagonal
            while(x1 <= bed.x2 && y2 <= bed.y2)
            {
                gcode += "G1 X" + bed.x_offs(x2).ToString("F3") + " Y" + bed.y_offs(y2).ToString("F3") + " " + speed + Environment.NewLine;
                gcode += "G1 X" + bed.x_offs(x1).ToString("F3") + " Y" + bed.y_offs(y1).ToString("F3") + " " + speed + Environment.NewLine;

                x1 += traceWidth;
                y2 += traceWidth;
            }

            if(y2 > bed.y2)
            {
                //yMin ist am limit
                y2 = bed.y2;
                while (x1 <= bed.x2 && x2 <= bed.x2)
                {
                    gcode += "G1 X" + bed.x_offs(x2).ToString("F3") + " Y" + bed.y_offs(y2).ToString("F3") + " " + speed + Environment.NewLine;
                    gcode += "G1 X" + bed.x_offs(x1).ToString("F3") + " Y" + bed.y_offs(y1).ToString("F3") + " " + speed + Environment.NewLine;

                    x1 += traceWidth;
                    x2 += traceWidth;
                }
            }
            else if(x1 > bed.x2)
            {
                //x1 am limit
                x1 = bed.x2;
                while (y2 <= bed.y2 && y1 <= bed.y2)
                {
                    gcode += "G1 X" + bed.x_offs(x2).ToString("F3") + " Y" + bed.y_offs(y2).ToString("F3") + " " + speed + Environment.NewLine;
                    gcode += "G1 X" + bed.x_offs(x1).ToString("F3") + " Y" + bed.y_offs(y1).ToString("F3") + " " + speed + Environment.NewLine;

                    y2 += traceWidth;
                    y1 += traceWidth;
                }
            }

            if(y2 > bed.y2)
            {
                y2 = bed.y2;
                while(x2 < bed.x2 && y1 < bed.y2)
                {
                    gcode += "G1 X" + bed.x_offs(x2).ToString("F3") + " Y" + bed.y_offs(y2).ToString("F3") + " " + speed + Environment.NewLine;
                    gcode += "G1 X" + bed.x_offs(x1).ToString("F3") + " Y" + bed.y_offs(y1).ToString("F3") + " " + speed + Environment.NewLine;

                    y1 += traceWidth;
                    x2 += traceWidth;
                }
            }
            else if(x1 > bed.x2 && y1 < bed.y2)
            {
                x1 = bed.x2;
                while (y1 < bed.y2 && x2 < bed.x2)
                {
                    gcode += "G1 X" + bed.x_offs(x2).ToString("F3") + " Y" + bed.y_offs(y2).ToString("F3") + " " + speed + Environment.NewLine;
                    gcode += "G1 X" + bed.x_offs(x1).ToString("F3") + " Y" + bed.y_offs(y1).ToString("F3") + " " + speed + Environment.NewLine;

                    y1 += traceWidth;
                    x2 += traceWidth;
                }
            }
            
            return gcode;
        }

        private string slice_pattern_rect_x()
        {
            string gcode = "";
            double y = yMin;

            string speed = "F" + (travelSpeed * 60).ToString();

            gcode += "G1 X" + xMin.ToString("F3") + " Y" + y.ToString("F3") + " " + speed + Environment.NewLine;

            while (y < yMax)
            {
                gcode += "G1 X" + xMax.ToString("F3") + " Y" + y.ToString("F3") + " " + speed + Environment.NewLine;
                gcode += "G1 X" + xMin.ToString("F3") + " Y" + y.ToString("F3") + " " + speed + Environment.NewLine;
                y += traceWidth;
            }
            return gcode;
        }

        private string slice_pattern_rect_y()
        {
            string gcode = "";
            double x = xMin;

            string speed = "F" + (travelSpeed * 60).ToString();
            gcode += "G1 X" + x.ToString("F3") + " Y" + yMin.ToString("F3") + " " + speed + Environment.NewLine;

            while (x < xMax)
            {
                gcode += "G1 X" + x.ToString("F3") + " Y" + yMax.ToString("F3") + " " + speed + Environment.NewLine;
                gcode += "G1 X" + x.ToString("F3") + " Y" + yMin.ToString("F3") + " " + speed + Environment.NewLine;
                x += traceWidth;
            }
            return gcode;
        }

        public string GenerateLayer(GrindPattern pattern, double z)
        {
            string gcode = "";
            string pattern_g_code_segment = "";
            string pattern_g_code = "";

            gcode += "G1 Z" + z.ToString("F3") + " ;Update Z height" + Environment.NewLine;

            pattern_g_code_segment = "";
            pattern_g_code = "";
            switch (pattern)
            {
                case GrindPattern.RectX:
                    pattern_g_code_segment += slice_pattern_rect_x();
                    break;
                case GrindPattern.RectY:
                    pattern_g_code_segment += slice_pattern_rect_y();
                    break;
                case GrindPattern.RectX_RectY:
                    pattern_g_code_segment += slice_pattern_rect_x();
                    pattern_g_code_segment += slice_pattern_rect_y();
                    break;

                case GrindPattern.Diagonal1:
                    pattern_g_code_segment += slice_pattern_diagonal1();
                    break;
                case GrindPattern.Diagonal2:
                    pattern_g_code_segment += slice_pattern_diagonal2();
                    break;
                case GrindPattern.Diagonal1_Diagonal2:
                    pattern_g_code_segment += slice_pattern_diagonal1();
                    pattern_g_code_segment += slice_pattern_diagonal2();
                    break;

                case GrindPattern.RectX_Diagonal1_Diagonal2_RectX:
                    pattern_g_code_segment += slice_pattern_rect_x();
                    pattern_g_code_segment += slice_pattern_diagonal1();
                    pattern_g_code_segment += slice_pattern_diagonal2();
                    pattern_g_code_segment += slice_pattern_rect_x();
                    break;

                case GrindPattern.Diagonal_RectX:
                    pattern_g_code_segment += slice_pattern_diagonal1();
                    pattern_g_code_segment += slice_pattern_rect_x();
                    pattern_g_code_segment += slice_pattern_diagonal2();
                    pattern_g_code_segment += slice_pattern_rect_x();
                    break;
            }

            for(int i=0; i< layerRepeat; i++)
            {
                pattern_g_code += ";Pattern ID:" + i.ToString() + Environment.NewLine;
                pattern_g_code += pattern_g_code_segment;
                pattern_g_code += Environment.NewLine;
            }

            gcode += pattern_g_code;            
            return gcode;
        }        

        public string toGCode()
        {
            string gcode = "";
            double z;
            gcode += ";X1:" + xMin.ToString() + ", ";
            gcode += "X2:" + xMax.ToString() + ", ";
            gcode += "Y1:" + yMin.ToString() + ", ";
            gcode += "Y2:" + yMax.ToString() + Environment.NewLine;
            gcode += ";Z-Start:" + zStart.ToString("F3") + " mm, ";
            gcode += "Step:" + zLayerHeight.ToString("F3") + " mm, ";
            gcode += "Trace with:" + traceWidth.ToString("F2") + " mm, ";
            gcode += "Layers:" + zNumLayers.ToString() + Environment.NewLine;
            gcode += ";Layer repeat:" + layerRepeat.ToString() + ", ";
            gcode += "Travel speed:" + travelSpeed.ToString() + " mm/s, ";            
            gcode += "Pattern:" + grindPattern.ToString() + Environment.NewLine;
            gcode += ";" + Environment.NewLine;

            z = zStart;
            for (int i=0; i< zNumLayers; i++)
            {
                gcode += GenerateLayer(grindPattern, z);
                z -= zLayerHeight;
            }
            return gcode;
        }
    }      
}

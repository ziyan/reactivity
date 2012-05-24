using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Reactivity.UI
{
    public class ColorPalette
    {
        public List<Color> colorPalette = new List<Color>();
        public List<Brush> brushPalette = new List<Brush>();
        public ColorPalette()
        {
            byte[] ColorGroups = new byte[] {   47, 170, 255,   // Bright blue
                                                146, 206, 0,    // Lime green
                                                181, 82, 48,    // Red
                                                143, 51, 201,   // Violet                                                
                                                229, 174, 20,   // Orange                                                
                                                243, 255, 61    // Yellow
                                            };
            Color colorTemp = new Color();
            SolidColorBrush brushTemp = new SolidColorBrush();
            colorTemp.A = 255;  // Set alpha channel of color to fully opaque
            for (int i = 0; i < (ColorGroups.Length / 3); i++)
            {
                colorTemp.R = ColorGroups[i * 3];
                colorTemp.G = ColorGroups[i * 3 + 1];
                colorTemp.B = ColorGroups[i * 3 + 2];

                brushTemp.Color = colorTemp;

                brushPalette.Add(brushTemp);
                colorPalette.Add(colorTemp);
            }
        }
    }
}

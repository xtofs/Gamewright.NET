namespace ImmediateCanvas.Gfx;

public readonly record struct Color(uint RGBA);


///<summary>
///  a set of predefined colors.
/// <see href= "https://en.wikipedia.org/wiki/Web_colors#Extended_colors">Basic Colors</see> and
/// <see href= "https://en.wikipedia.org/wiki/Web_colors#Extended_colors">Extended Colors</see>
/// </summary>

public static class Colors
{
    //     public static readonly Color White = new Color(0xFFFFFFFF);
    //     public static readonly Color Silver = new Color(0xC0C0C0FF);
    //     public static readonly Color Gray = new Color(0x808080FF);
    //     public static readonly Color Black = new Color(0x000000FF);
    //     public static readonly Color Red = new Color(0xFF0000FF);
    //     public static readonly Color Maroon = new Color(0x800000FF);
    //     public static readonly Color Yellow = new Color(0xFFFF00FF);
    //     public static readonly Color Olive = new Color(0x808000FF);
    //     public static readonly Color Lime = new Color(0x00FF00FF);
    //     public static readonly Color Green = new Color(0x008000FF);
    //     public static readonly Color Aqua = new Color(0x00FFFFFF);
    //     public static readonly Color Teal = new Color(0x008080FF);
    //     public static readonly Color Blue = new Color(0x0000FFFF);
    //     public static readonly Color Navy = new Color(0x000080FF);
    //     public static readonly Color Fuchsia = new Color(0xFF00FFFF);
    //     public static readonly Color Purple = new Color(0x800080FF);




    public static readonly Color OrangeRed = new Color(0xFF4500FF); //   255, 69, 0
    public static readonly Color Tomato = new Color(0xFF6347FF); //   255, 99, 71
    public static readonly Color DarkOrange = new Color(0xFF8C00FF); //   255, 140, 0
    public static readonly Color Coral = new Color(0xFF7F50FF); //   255, 127, 80
    public static readonly Color Orange = new Color(0xFFA500FF); //   255, 165, 0

    public static readonly Color DarkKhaki = new Color(0xBDB76BFF); //   189, 183, 107
    public static readonly Color Gold = new Color(0xFFD700FF); //   255, 215, 0
    public static readonly Color Khaki = new Color(0xF0E68CFF); //   240, 230, 140
    public static readonly Color PeachPuff = new Color(0xFFDAB9FF); //   255, 218, 185
    public static readonly Color Yellow = new Color(0xFFFF00FF); //   255, 255, 0
    public static readonly Color PaleGoldenrod = new Color(0xEEE8AAFF); //   238, 232, 170
    public static readonly Color Moccasin = new Color(0xFFE4B5FF); //   255, 228, 181
    public static readonly Color PapayaWhip = new Color(0xFFEFD5FF); //   255, 239, 213
    public static readonly Color LightGoldenrodYellow = new Color(0xFAFAD2FF); //   250, 250, 210
    public static readonly Color LemonChiffon = new Color(0xFFFACDFF); //   255, 250, 205
    public static readonly Color LightYellow = new Color(0xFFFFE0FF); //   255, 255, 224

    // brown colors
    public static readonly Color Maroon = new Color(0x800000FF); //   128, 0, 0
    public static readonly Color Brown = new Color(0xA52A2AFF); //   165, 42, 42
    public static readonly Color SaddleBrown = new Color(0x8B4513FF); //   139, 69, 19
    public static readonly Color Sienna = new Color(0xA0522DFF); //   160, 82, 45
    public static readonly Color Chocolate = new Color(0xD2691EFF); //   210, 105, 30
    public static readonly Color DarkGoldenrod = new Color(0xB8860BFF); //   184, 134, 11
    public static readonly Color Peru = new Color(0xCD853FFF); //   205, 133, 63
    public static readonly Color RosyBrown = new Color(0xBC8F8FFF); //   188, 143, 143
    public static readonly Color Goldenrod = new Color(0xDAA520FF); //   218, 165, 32
    public static readonly Color SandyBrown = new Color(0xF4A460FF); //   244, 164, 96
    public static readonly Color Tan = new Color(0xD2B48CFF); //   210, 180, 140
    public static readonly Color Burlywood = new Color(0xDEB887FF); //   222, 184, 135
    public static readonly Color Wheat = new Color(0xF5DEB3FF); //   245, 222, 179
    public static readonly Color NavajoWhite = new Color(0xFFDEADFF); //   255, 222, 173
    public static readonly Color Bisque = new Color(0xFFE4C4FF); //   255, 228, 196
    public static readonly Color BlanchedAlmond = new Color(0xFFEBCDFF); //   255, 235, 205
    public static readonly Color Cornsilk = new Color(0xFFF8DCFF); //   255, 248, 220

    // Purple, violet, and magenta colors 
    public static readonly Color Indigo = new Color(0x4B0082FF); //   75, 0, 130
    public static readonly Color Purple = new Color(0x800080FF); //   128, 0, 128
    public static readonly Color DarkMagenta = new Color(0x8B008BFF); //   139, 0, 139
    public static readonly Color DarkViolet = new Color(0x9400D3FF); //   148, 0, 211
    public static readonly Color DarkSlateBlue = new Color(0x483D8BFF); //   72, 61, 139
    public static readonly Color BlueViolet = new Color(0x8A2BE2FF); //   138, 43, 226
    public static readonly Color DarkOrchid = new Color(0x9932CCFF); //   153, 50, 204
    public static readonly Color Fuchsia = new Color(0xFF00FFFF); //   255, 0, 255
    public static readonly Color Magenta = new Color(0xFF00FFFF); //   255, 0, 255
    public static readonly Color SlateBlue = new Color(0x6A5ACDFF); //   106, 90, 205
    public static readonly Color MediumSlateBlue = new Color(0x7B68EEFF); //   123, 104, 238
    public static readonly Color MediumOrchid = new Color(0xBA55D3FF); //   186, 85, 211
    public static readonly Color MediumPurple = new Color(0x9370DBFF); //   147, 112, 219
    public static readonly Color Orchid = new Color(0xDA70D6FF); //   218, 112, 214
    public static readonly Color Violet = new Color(0xEE82EEFF); //   238, 130, 238
    public static readonly Color Plum = new Color(0xDDA0DDFF); //   221, 160, 221
    public static readonly Color Thistle = new Color(0xD8BFD8FF); //   216, 191, 216
    public static readonly Color Lavender = new Color(0xE6E6FAFF); //   230, 230, 250
    public static readonly Color MidnightBlue = new Color(0x191970FF); //   25, 25, 112
    public static readonly Color Navy = new Color(0x000080FF); //   0, 0, 128
    public static readonly Color DarkBlue = new Color(0x00008BFF); //   0, 0, 139
    public static readonly Color MediumBlue = new Color(0x0000CDFF); //   0, 0, 205
    public static readonly Color Blue = new Color(0x0000FFFF); //   0, 0, 255
    public static readonly Color RoyalBlue = new Color(0x4169E1FF); //   65, 105, 225
    public static readonly Color SteelBlue = new Color(0x4682B4FF); //   70, 130, 180
    public static readonly Color DodgerBlue = new Color(0x1E90FFFF); //   30, 144, 255
    public static readonly Color DeepSkyBlue = new Color(0x00BFFFFF); //   0, 191, 255
    public static readonly Color CornflowerBlue = new Color(0x6495EDFF); //   100, 149, 237
    public static readonly Color SkyBlue = new Color(0x87CEEBFF); //   135, 206, 235
    public static readonly Color LightSkyBlue = new Color(0x87CEFAFF); //   135, 206, 250
    public static readonly Color LightSteelBlue = new Color(0xB0C4DEFF); //   176, 196, 222
    public static readonly Color LightBlue = new Color(0xADD8E6FF); //   173, 216, 230
    public static readonly Color PowderBlue = new Color(0xB0E0E6FF); //   176, 224, 230
    public static readonly Color Teal = new Color(0x008080FF); //   0, 128, 128
    public static readonly Color DarkCyan = new Color(0x008B8BFF); //   0, 139, 139
    public static readonly Color LightSeaGreen = new Color(0x20B2AAFF); //   32, 178, 170
    public static readonly Color CadetBlue = new Color(0x5F9EA0FF); //   95, 158, 160
    public static readonly Color DarkTurquoise = new Color(0x00CED1FF); //   0, 206, 209
    public static readonly Color MediumTurquoise = new Color(0x48D1CCFF); //   72, 209, 204
    public static readonly Color Turquoise = new Color(0x40E0D0FF); //   64, 224, 208
    public static readonly Color Aqua = new Color(0x00FFFFFF); //   0, 255, 255
    public static readonly Color Cyan = new Color(0x00FFFFFF); //   0, 255, 255
    public static readonly Color Aquamarine = new Color(0x7FFFD4FF); //   127, 255, 212
    public static readonly Color PaleTurquoise = new Color(0xAFEEEEFF); //   175, 238, 238
    public static readonly Color LightCyan = new Color(0xE0FFFFFF); //   224, 255, 255
    public static readonly Color Green = new Color(0x008000FF); //   0, 128, 0
    public static readonly Color DarkOliveGreen = new Color(0x556B2FFF); //   85, 107, 47
    public static readonly Color ForestGreen = new Color(0x228B22FF); //   34, 139, 34
    public static readonly Color SeaGreen = new Color(0x2E8B57FF); //   46, 139, 87
    public static readonly Color Olive = new Color(0x808000FF); //   128, 128, 0
    public static readonly Color OliveDrab = new Color(0x6B8E23FF); //   107, 142, 35
    public static readonly Color MediumSeaGreen = new Color(0x3CB371FF); //   60, 179, 113
    public static readonly Color LimeGreen = new Color(0x32CD32FF); //   50, 205, 50
    public static readonly Color Lime = new Color(0x00FF00FF); //   0, 255, 0
    public static readonly Color SpringGreen = new Color(0x00FF7FFF); //   0, 255, 127
    public static readonly Color MediumSpringGreen = new Color(0x00FA9AFF); //   0, 250, 154
    public static readonly Color DarkSeaGreen = new Color(0x8FBC8FFF); //   143, 188, 143
    public static readonly Color MediumAquamarine = new Color(0x66CDAAFF); //   102, 205, 170
    public static readonly Color YellowGreen = new Color(0x9ACD32FF); //   154, 205, 50
    public static readonly Color LawnGreen = new Color(0x7CFC00FF); //   124, 252, 0
    public static readonly Color Chartreuse = new Color(0x7FFF00FF); //   127, 255, 0
    public static readonly Color LightGreen = new Color(0x90EE90FF); //   144, 238, 144
    public static readonly Color GreenYellow = new Color(0xADFF2FFF); //   173, 255, 47
    public static readonly Color PaleGreen = new Color(0x98FB98FF); //   152, 251, 152
    public static readonly Color MistyRose = new Color(0xFFE4E1FF); //   255, 228, 225
    public static readonly Color AntiqueWhite = new Color(0xFAEBD7FF); //   250, 235, 215
    public static readonly Color Linen = new Color(0xFAF0E6FF); //   250, 240, 230
    public static readonly Color Beige = new Color(0xF5F5DCFF); //   245, 245, 220
    public static readonly Color WhiteSmoke = new Color(0xF5F5F5FF); //   245, 245, 245
    public static readonly Color LavenderBlush = new Color(0xFFF0F5FF); //   255, 240, 245
    public static readonly Color OldLace = new Color(0xFDF5E6FF); //   253, 245, 230
    public static readonly Color AliceBlue = new Color(0xF0F8FFFF); //   240, 248, 255
    public static readonly Color Seashell = new Color(0xFFF5EEFF); //   255, 245, 238
    public static readonly Color GhostWhite = new Color(0xF8F8FFFF); //   248, 248, 255
    public static readonly Color Honeydew = new Color(0xF0FFF0FF); //   240, 255, 240
    public static readonly Color FloralWhite = new Color(0xFFFAF0FF); //   255, 250, 240
    public static readonly Color Azure = new Color(0xF0FFFFFF); //   240, 255, 255
    public static readonly Color MintCream = new Color(0xF5FFFAFF); //   245, 255, 250
    public static readonly Color Snow = new Color(0xFFFAFAFF); //   255, 250, 250
    public static readonly Color Ivory = new Color(0xFFFFF0FF); //   255, 255, 240
    public static readonly Color White = new Color(0xFFFFFFFF); //   255, 255, 255
    public static readonly Color Black = new Color(0x000000FF); //   0, 0, 0
    public static readonly Color DarkSlateGray = new Color(0x2F4F4FFF); //   47, 79, 79
    public static readonly Color DimGray = new Color(0x696969FF); //   105, 105, 105
    public static readonly Color SlateGray = new Color(0x708090FF); //   112, 128, 144
    public static readonly Color Gray = new Color(0x808080FF); //   128, 128, 128
    public static readonly Color LightSlateGray = new Color(0x778899FF); //   119, 136, 153
    public static readonly Color DarkGray = new Color(0xA9A9A9FF); //   169, 169, 169
    public static readonly Color Silver = new Color(0xC0C0C0FF); //   192, 192, 192
    public static readonly Color LightGray = new Color(0xD3D3D3FF); //   211, 211, 211
    public static readonly Color Gainsboro = new Color(0xDCDCDCFF); //   220, 220, 220
}
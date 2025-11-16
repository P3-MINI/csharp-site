using Figgle.Fonts;
using Plugin.Common;

namespace Plugin.Figgle;

public class FigglePlugin : ITextPlugin
{
    public string ApplyOperation(string input)
    {
        return FiggleFonts.Standard.Render(input);
    }
}
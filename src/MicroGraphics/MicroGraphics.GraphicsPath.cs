using Meadow.TinyCLR.Core;

namespace Meadow.Foundation.Graphics
{
    public partial class GraphicsLibrary
    {
        public void DrawPath(GraphicsPath path, bool colored)
        {
            DrawPath(path, (colored ? Color.White : Color.Black));
        }

        public void DrawPath(GraphicsPath path, Color color)
        {
            //simple for now 
            for (int i = 0; i < path.PointCount; i++)
            {
                if (((PathAction)path.PathActions[i]).Verb == VerbType.Move || i == 0)
                {
                    continue;
                }

                DrawLine(((PathAction)path.PathActions[i - 1]).PathPoint.X,
                         ((PathAction)path.PathActions[i - 1]).PathPoint.Y,
                         ((PathAction)path.PathActions[i]).PathPoint.X,
                         ((PathAction)path.PathActions[i]).PathPoint.Y,
                        color);
            }
        }
    }
}

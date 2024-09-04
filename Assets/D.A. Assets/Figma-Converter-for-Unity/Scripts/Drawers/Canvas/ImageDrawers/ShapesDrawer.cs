using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if FREYA_SHAPES_EXISTS
using Shapes;
#endif

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class ShapesDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, Sprite sprite, GameObject target)
        {
            if (fobject.Type == NodeType.REGULAR_POLYGON)
            {
#if FREYA_SHAPES_EXISTS
                target.TryAddComponent(out Polygon poly);
                var svgPath = fobject.FillGeometry[0].Path;
                var v3s = ConvertSvgPolygonToPoints(svgPath);
                poly.points = new List<Vector2>();
                foreach (var v3 in v3s)
                {
                    poly.AddPoint(v3);
                }
#endif
            }

        }
        private Vector3[] ConvertSvgPolygonToPoints(string svgPolygon)
        {
            svgPolygon = svgPolygon.Trim('M', 'Z').Trim();

            string[] coordinatePairs = svgPolygon.Split(new[] { 'L' }, System.StringSplitOptions.RemoveEmptyEntries);

            List<Vector3> points = new List<Vector3>();

            foreach (string pair in coordinatePairs)
            {
                string[] coordinates = pair.Trim().Split(' ');
                if (coordinates.Length == 2)
                {
                    float x = float.Parse(coordinates[0], CultureInfo.InvariantCulture);
                    float y = float.Parse(coordinates[1], CultureInfo.InvariantCulture);
                    points.Add(new Vector3(x, -y, 0));
                }
            }

            return points.ToArray();
        }
    }
}
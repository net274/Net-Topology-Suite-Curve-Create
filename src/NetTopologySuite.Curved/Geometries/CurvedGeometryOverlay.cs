using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;

namespace NetTopologySuite.Geometries
{
    public class CurvedGeometryOverlay
    {
        public static GeometryOverlay CurveV2 => new CurvedGeometryOverlayV2();


        private sealed class CurvedGeometryOverlayV2 : GeometryOverlay
        {
            protected override Geometry Overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
            {
                return OverlayNGRobust.Overlay(Flatten(geom0), Flatten(geom1), opCode);
            }


            public override Geometry Union(Geometry a)
            {
                return OverlayNGRobust.Union(Flatten(a));
            }

            /// <summary>
            /// Flattens a possibly curved geometry. If <paramref name="geom"/> does
            /// not consist of any curved elements, <paramref name="geom"/> is returned
            /// unchanged.
            /// </summary>
            /// <param name="geom">The geometry to flatten</param>
            /// <returns>A flattened geometry.</returns>
            private Geometry Flatten(Geometry geom)
            {
                if (geom == null)
                    return geom;

                if (!HasCurved(geom))
                    return geom;

                var factory = geom.Factory;
                var geometries = new Geometry[geom.NumGeometries];
                for (int i = 0; i < geom.NumGeometries; i++)
                {
                    var testGeom = geom.GetGeometryN(i);
                    switch (testGeom)
                    {
                        case GeometryCollection _:
                            geometries[i] = Flatten(testGeom);
                            break;
                        case ICurvedGeometry curve:
                            geometries[i] = curve.Flatten();
                            break;
                        default:
                            geometries[i] = testGeom;
                            break;
                    }
                }

                return factory.BuildGeometry(geometries);
            }

            /// <summary>
            /// Predicate check if <paramref cref="geom"/> has any curved elements
            /// </summary>
            /// <param name="geom">The geometry to check for curved elements</param>
            /// <returns><c>true</c> if a curved geometry was found.</returns>
            private bool HasCurved(Geometry geom)
            {
                for (int i = 0; i < geom.NumGeometries; i++)
                {
                    var testGeom = geom.GetGeometryN(i);
                    switch (testGeom)
                    {
                        case GeometryCollection _:
                            if (HasCurved(testGeom))
                                return true;
                            break;
                        case ICurvedGeometry _:
                            return true;
                    }
                }

                return false;
            }

            public override string ToString()
            {
                return "CurveNG";
            }
        }

    }
}
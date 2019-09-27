using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ArcGISControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //readonly string shapeFilePath = @"C:\Map Data\Shape File\countries_shp\countries.shp";
        //string modelPath = @"C:\Users\mfathin.CSYSINT\Documents\Visual Studio 2017\Projects\ArcGISControl\ArcGISControl\Icon\icons8-man-filled-100";

        string shapeFilePath = @"C:\Users\mfathin.CSYSINT\Documents\Visual Studio 2017\Projects\ArcGISControl\ArcGISControl\MapData\countries_shp\countries.shp";
        string rasterMapMosaicPath = @"C:\Map Data\MosaicSqliteFile\RasterMapMosaic.sqlite";
        string dtedDirectory = @"C:\Users\mfathin.CSYSINT\Documents\Visual Studio 2017\Projects\ArcGISControl\ArcGISControl\MapData\N03.dt2";


        public static MapPoint start, point1, point2, point3;

        private LocationDistanceMeasurement _distanceMeasurement;

        double azimuthPoint;
        double distancePoint;
        double maxZoomIn = 115000;
        double maxZoomOut = 115000;
        double currentScale;

        Graphic graphic1, _pathGraphic, _startLocationGraphic, _endLocationGraphic;
        GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
        Surface sceneSurface = new Surface();

        List<double> azimuthList = new List<double>();
        List<double> distanceList = new List<double>();
        List<MapPoint> mapPoints = new List<MapPoint>();


        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

       
        private async void Initialize()
        {
            //MyMapView.Map = new Map(Basemap.CreateImagery());

            MyMapView.Map = new Map(new SpatialReference(3375));

            //MyMapView.InteractionOptions = new MapViewInteractionOptions { IsZoomEnabled = false };

            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(shapeFilePath);

            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

            // Add the feature layer to the map
            MyMapView.Map.OperationalLayers.Add(newFeatureLayer);

            await myShapefile.LoadAsync();

            //Get mosaic dataset names in the SQLite database.
            var names = MosaicDatasetRaster.GetNames(rasterMapMosaicPath);
            var rasterName = names[0];

            //Create a raster from a mosaic dataset
            Raster mapRaster = new MosaicDatasetRaster(rasterMapMosaicPath, "RasterMapTable");

            // Create a RasterLayer to display the Raster
            RasterLayer rasterMapLayer = new RasterLayer(mapRaster);           

            MyMapView.Map.OperationalLayers.Add(rasterMapLayer);

            rasterMapLayer.MinScale = 100000;

            rasterMapLayer.MaxScale = 2000;

            await rasterMapLayer.LoadAsync();


            MyMapView.Map.MinScale = 3000000;

            MyMapView.Map.MaxScale = 2000;


            MyMapView.GraphicsOverlays.Add(graphicsOverlay);



            // Add a graphic at JFK to serve as the origin.
            start = new MapPoint(564968.155634, 431946.116905, new SpatialReference(3168));

            mapPoints.Add(start);

            SimpleMarkerSymbol startMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Blue, 15);
            //PictureMarkerSymbol markerSym = new PictureMarkerSymbol(new Uri("C:\\Users\\mfathin.CSYSINT\\Documents\\Visual Studio 2017\\Projects\\ArcGISControl\\ArcGISControl\\Icon\\icons8-man-filled-100.PNG"));
            // markerSym.Opacity = 0.5;
            _startLocationGraphic = new Graphic(start, startMarker);

            // Create the graphic for the destination.
            _endLocationGraphic = new Graphic
            {
                Symbol = startMarker
            };

            // Create the graphic for the path.
            _pathGraphic = new Graphic
            {
                Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 1)
            };

            graphic1 = new Graphic
            {
                Symbol = startMarker
            };
            // Add the graphics to the overlay.
            graphicsOverlay.Graphics.Add(_startLocationGraphic);
            graphicsOverlay.Graphics.Add(_endLocationGraphic);
            graphicsOverlay.Graphics.Add(graphic1);
            graphicsOverlay.Graphics.Add(_pathGraphic);

            //// Update end location when the user taps.
            MyMapView.GeoViewTapped += MyMapViewOnGeoViewTapped;

            await MyMapView.SetViewpointAsync(new Viewpoint(start, 4000000));

            // Update the extent to encompass all of the symbols
            //SetExtent();

            // Add the graphics overlay to the map view
            //MyMapView.GraphicsOverlays.Add(_overlay);
            plotButton.Click += (sender, e) =>
            { TryPlot(); };

            TrueCalculation();

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ////await MyMapView.SetViewpointScaleAsync(2000);

            //bool btnClick = false;

            //double a = 15000;

            //btnClick = !btnClick;

            //if (btnClick == true)
            //{
            //    if(maxZoomIn > 12000 )
            //    {
            //        await MyMapView.SetViewpointScaleAsync(maxZoomIn = maxZoomIn - a);

            //        currentScale = maxZoomIn;

            //    }
            //}

            //else
            //{
            //    return;
            //}


            var main = new MainWindow();

            bool btnClick = false;

            double a = 15000;

            btnClick = !btnClick;

            if (btnClick == true)
            {
                if (currentScale > 12000)
                {
                    await MyMapView.SetViewpointScaleAsync(maxZoomIn = maxZoomIn - a);

                    currentScale = maxZoomIn;

                }
            }

            else
            {
                return;
            }
        }

        private async void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            bool btnOutClick = false;

            double a = 15000;

            btnOutClick = !btnOutClick;

            if (btnOutClick == true)
            {
                if (currentScale < 12000 && currentScale > 115000)
                {
                    await MyMapView.SetViewpointScaleAsync(currentScale + a);

                    currentScale = maxZoomIn;

                }
            }

            else
            {
                return;
            }


        }

        private void TrueCalculation()
        {
            // Create a scene with elevation.
            Surface sceneSurface = new Surface();

            var Dted1Files = new List<string>();

            Dted1Files.Add(dtedDirectory);

            RasterElevationSource dted1ElevSource = new RasterElevationSource(Dted1Files);

            sceneSurface.ElevationSources.Add(dted1ElevSource);

            AnalysisOverlay measureAnalysisOverlay = new AnalysisOverlay();
            MySceneView.AnalysisOverlays.Add(measureAnalysisOverlay);

          

        }

        private void TryPlot()
        {
            // Add a graphic at JFK to serve as the origin.
           MapPoint pointIt = new MapPoint(410175.775805, 552181.881083, new SpatialReference(3168));

            mapPoints.Add(start);

            SimpleMarkerSymbol startMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Red, 20);
            //PictureMarkerSymbol markerSym = new PictureMarkerSymbol(new Uri("C:\\Users\\mfathin.CSYSINT\\Documents\\Visual Studio 2017\\Projects\\ArcGISControl\\ArcGISControl\\Icon\\icons8-man-filled-100.PNG"));
            // markerSym.Opacity = 0.5;
            Graphic startLocatic = new Graphic(start, startMarker);

            graphicsOverlay.Graphics.Add(startLocatic);
        }

        private async void MyMapView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (MyMapView != null)
            {
                Point screenPoint = e.GetPosition(MyMapView);

                MapPoint mapPoint = MyMapView.ScreenToLocation(screenPoint);

                if (mapPoint != null)
                {
                    // point = GetDesiredElevationLocation();
                    double result = await sceneSurface.GetElevationAsync(mapPoint);

                    if (MyMapView.IsWrapAroundEnabled)
                    {


                        mapPoint = GeometryEngine.Project(mapPoint, SpatialReferences.Wgs84) as MapPoint;

                        textBox.Text = String.Format("x: {0}, y: {1} | Lat:{2},Lon:{3} | Scale:{4} | Elevation : {5} m", screenPoint.X, screenPoint.Y, Math.Round(mapPoint.Y, 6), Math.Round(mapPoint.X, 6), MyMapView.MapScale, result);
                    }

                }
                else
                    return;
            }
        }


        private void CreatePolygon()
        {
            // Create a green simple line symbol
            SimpleLineSymbol outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.FromArgb(0xFF, 0x00, 0x50, 0x00), 1);

            // Create a green mesh simple fill symbol
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, System.Drawing.Color.FromArgb(0xFF, 0x00, 0x50, 0x00), outlineSymbol);

            // Create a new point collection for polygon
            Esri.ArcGISRuntime.Geometry.PointCollection points = new Esri.ArcGISRuntime.Geometry.PointCollection(SpatialReferences.Wgs84)
            {
                // Create and add points to the point collection
                new MapPoint(start.X,start.Y),
                new MapPoint(-2.6430, 56.0763),
                new MapPoint(-2.6410, 56.0759),
                new MapPoint(-2.6380, 56.0765),
                new MapPoint(-2.6380, 56.0784),
                new MapPoint(-2.6410, 56.0786)
            };

            // Create the polyline from the point collection
            Polygon polygon = new Polygon(points);

            // Create the graphic with polyline and symbol
            Graphic graphic = new Graphic(polygon, fillSymbol);

            // Add graphic to the graphics overlay
            graphicsOverlay.Graphics.Add(graphic);
        }

        public void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            azimuthPoint = double.Parse(textAzimuth.Text);

            distancePoint = double.Parse(textDistance.Text);
            //distanceList.Add(distancePoint);


            if (azimuthPoint <= 360)
            {
                if (azimuthPoint <= 180)
                {
                    var result = azimuthPoint;
                    //azimuthList.Add(result);
                    //TraverseData.AzimuthList.Add(result);
                    //azimuthList.Add(result);
                }

                if (azimuthPoint > 180)
                {
                    var result = 180 - (azimuthPoint);
                    //azimuthList.Add(result);
                    //TraverseData.AzimuthList.Add(result);
                    //azimuthList.Add(result);
                }

                textAzimuth.Text = "";
                textDistance.Text = "";

            }

            double result1;
            double result2;

            //GeodeticDistanceResult distance = GeometryEngine.DistanceGeodetic(start, destination, LinearUnits.Meters, AngularUnits.Degrees, GeodeticCurveType.NormalSection);

            IReadOnlyList<MapPoint> coordinateList = GeometryEngine.MoveGeodetic(mapPoints, distancePoint, LinearUnits.Meters, azimuthPoint, AngularUnits.Degrees, GeodeticCurveType.NormalSection);
            GeodeticDistanceResult travel = GeometryEngine.DistanceGeodetic(start, coordinateList[0], LinearUnits.Meters, AngularUnits.Degrees, GeodeticCurveType.NormalSection);


           
                graphic1.Geometry = coordinateList[0];

                if (travel.Azimuth1 >= 0)
                {
                    result1 = travel.Azimuth1;
                }
                else
                {
                    result1 = travel.Azimuth1 + 360;
                }
                if (travel.Azimuth2 >= 0)
                {
                    result2 = travel.Azimuth2;
                }
                else
                {
                    result2 = travel.Azimuth2 + 360;
                }
                textBox.Text = string.Format($"Distance:{travel.Distance} | Azimuth1:{result1} | Azimuth2:{result2} |Initial Point :410175.775805, 552181.881083");


                //textBox.Text = string.Format("{0} kilometers", (int)distance);
                // Get the points that define the route polyline.
                Esri.ArcGISRuntime.Geometry.PointCollection polylinePoints = new Esri.ArcGISRuntime.Geometry.PointCollection(new SpatialReference(3168))
                {

                (MapPoint)_startLocationGraphic.Geometry,
                coordinateList[0]

                };

                // Create the polyline for the two points.
                Polyline routeLine = new Polyline(polylinePoints);


                // Densify the polyline to show the geodesic curve.
                Esri.ArcGISRuntime.Geometry.Geometry pathGeometry = GeometryEngine.DensifyGeodetic(routeLine, 1, LinearUnits.Kilometers, GeodeticCurveType.Geodesic);
                // Apply the curved line to the path graphic.
                _pathGraphic.Geometry = pathGeometry;


                
        }

        private void MyMapViewOnGeoViewTapped(object sender, GeoViewInputEventArgs geoViewInputEventArgs)
        {
            if (MyMapView != null)
            {
                Point screenPoint = geoViewInputEventArgs.Position;

                MapPoint mapPoint = MyMapView.ScreenToLocation(screenPoint);

                if (mapPoint != null)
                {
                    //// point = GetDesiredElevationLocation();
                    //double result = await Dted1Surface.GetElevationAsync(mapPoint);

                    if (MyMapView.IsWrapAroundEnabled)
                    {
                        mapPoint = GeometryEngine.Project(mapPoint, new SpatialReference(3168)) as MapPoint;
                    }
                }

                else
                    return;

                // Get the tapped point, projected to WGS84.
                MapPoint destination = (MapPoint)GeometryEngine.Project(geoViewInputEventArgs.Location, new SpatialReference(3168));



                //// Calculate and show the distance.
                //double distance = GeometryEngine.LengthGeodetic(pathGeometry, LinearUnits.Kilometers, GeodeticCurveType.Geodesic);


                graphic1.Geometry = destination;

              
            }


            //}



            //private void CalculatePointDistance()
            //{
            //    SpatialReference equidistantSpatialRef = SpatialReference.Create(3168);

            //    // Project the points from geographic to the projected coordinate system.
            //    MapPoint mapPointProjected1 = GeometryEngine.Project(mapPoint1, equidistantSpatialRef) as MapPoint;
            //    MapPoint mapPointProjected2 = GeometryEngine.Project(mapPoint2, equidistantSpatialRef) as MapPoint;

            //    double planarDistanceMeters = GeometryEngine.Distance(mapPointProjected1, mapPointProjected2);

            //    //textBox.Text = string.Format("{0} Meters", planarDistanceMeters);


            //}








        }
    }
}

// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ListTransformations
{
    [Register("SpatialRelationships")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Spatial relationships",
        "Geometry",
        "This sample demonstrates how to use the GeometryEngine to evaluate the spatial relationships (for example, polygon a contains line b) between geometries.",
        "Tap a graphic to select it. The display will update to show the relationships with the other graphics.")]
    public class SpatialRelationships : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITextView _resultTextView;
        private UIStackView _stackView;

        // References to the graphics and graphics overlay.
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _polygonGraphic;
        private Graphic _polylineGraphic;
        private Graphic _pointGraphic;

        public SpatialRelationships()
        {
            Title = "Spatial relationships";
        }

        private void Initialize()
        {
            // Configure the basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Create the graphics overlay and set the selection color.
            _graphicsOverlay = new GraphicsOverlay
            {
                SelectionColor = System.Drawing.Color.Yellow
            };

            // Add the overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Create the point collection that defines the polygon.
            PointCollection polygonPoints = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-5991501.677830, 5599295.131468),
                new MapPoint(-6928550.398185, 2087936.739807),
                new MapPoint(-3149463.800709, 1840803.011362),
                new MapPoint(-1563689.043184, 3714900.452072),
                new MapPoint(-3180355.516764, 5619889.608838)
            };

            // Create the polygon.
            Polygon polygonGeometry = new Polygon(polygonPoints);

            // Define the symbology of the polygon.
            SimpleLineSymbol polygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 2);
            SimpleFillSymbol polygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, System.Drawing.Color.Green, polygonOutlineSymbol);

            // Create the polygon graphic and add it to the graphics overlay.
            _polygonGraphic = new Graphic(polygonGeometry, polygonFillSymbol);
            _graphicsOverlay.Graphics.Add(_polygonGraphic);

            // Create the point collection that defines the polyline.
            PointCollection polylinePoints = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-4354240.726880, -609939.795721),
                new MapPoint(-3427489.245210, 2139422.933233),
                new MapPoint(-2109442.693501, 4301843.057130),
                new MapPoint(-1810822.771630, 7205664.366363)
            };

            // Create the polyline.
            Polyline polylineGeometry = new Polyline(polylinePoints);

            // Create the polyline graphic and add it to the graphics overlay.
            _polylineGraphic = new Graphic(polylineGeometry, new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Red, 4));
            _graphicsOverlay.Graphics.Add(_polylineGraphic);

            // Create the point geometry that defines the point graphic.
            MapPoint pointGeometry = new MapPoint(-4487263.495911, 3699176.480377, SpatialReferences.WebMercator);

            // Define the symbology for the point.
            SimpleMarkerSymbol locationMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 10);

            // Create the point graphic and add it to the graphics overlay.
            _pointGraphic = new Graphic(pointGeometry, locationMarker);
            _graphicsOverlay.Graphics.Add(_pointGraphic);

            // Listen for taps; the spatial relationships will be updated in the handler.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Set the viewpoint to center on the point.
            _myMapView.SetViewpointCenterAsync(pointGeometry, 200000000);
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Identify the tapped graphics.
            IdentifyGraphicsOverlayResult result = await _myMapView.IdentifyGraphicsOverlayAsync(_graphicsOverlay, e.Position, 1, false);

            // Return if there are no results.
            if (result.Graphics.Count < 1)
            {
                return;
            }

            // Get the smallest identified graphic.
            Graphic identifiedGraphic = result.Graphics.OrderBy(graphic => GeometryEngine.Area(graphic.Geometry)).First();

            // Clear any existing selection, then select the tapped graphic.
            _graphicsOverlay.ClearSelection();
            identifiedGraphic.IsSelected = true;

            // Get the selected graphic's geometry.
            Geometry selectedGeometry = identifiedGraphic.Geometry;

            // Perform the calculation and show the results.
            _resultTextView.Text = GetOutputText(selectedGeometry);
        }

        private string GetOutputText(Geometry selectedGeometry)
        {
            string output = "";

            // Get the relationships.
            List<SpatialRelationship> polygonRelationships = GetSpatialRelationships(selectedGeometry, _polygonGraphic.Geometry);
            List<SpatialRelationship> polylineRelationships = GetSpatialRelationships(selectedGeometry, _polylineGraphic.Geometry);
            List<SpatialRelationship> pointRelationships = GetSpatialRelationships(selectedGeometry, _pointGraphic.Geometry);

            // Add the point relationships to the output.
            if (selectedGeometry.GeometryType != GeometryType.Point)
            {
                output += "Point:\n";
                foreach (SpatialRelationship relationship in pointRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }

            // Add the polygon relationships to the output.
            if (selectedGeometry.GeometryType != GeometryType.Polygon)
            {
                output += "Polygon:\n";
                foreach (SpatialRelationship relationship in polygonRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }

            // Add the polyline relationships to the output.
            if (selectedGeometry.GeometryType != GeometryType.Polyline)
            {
                output += "Polyline:\n";
                foreach (SpatialRelationship relationship in polylineRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }

            return output;
        }

        /// <summary>
        /// Returns a list of spatial relationships between two geometries.
        /// </summary>
        /// <param name="a">The 'a' in "a contains b".</param>
        /// <param name="b">The 'b' in "a contains b".</param>
        /// <returns>A list of spatial relationships that are true for a and b.</returns>
        private static List<SpatialRelationship> GetSpatialRelationships(Geometry a, Geometry b)
        {
            List<SpatialRelationship> relationships = new List<SpatialRelationship>();
            if (GeometryEngine.Crosses(a, b))
            {
                relationships.Add(SpatialRelationship.Crosses);
            }

            if (GeometryEngine.Contains(a, b))
            {
                relationships.Add(SpatialRelationship.Contains);
            }

            if (GeometryEngine.Disjoint(a, b))
            {
                relationships.Add(SpatialRelationship.Disjoint);
            }

            if (GeometryEngine.Intersects(a, b))
            {
                relationships.Add(SpatialRelationship.Intersects);
            }

            if (GeometryEngine.Overlaps(a, b))
            {
                relationships.Add(SpatialRelationship.Overlaps);
            }

            if (GeometryEngine.Touches(a, b))
            {
                relationships.Add(SpatialRelationship.Touches);
            }

            if (GeometryEngine.Within(a, b))
            {
                relationships.Add(SpatialRelationship.Within);
            }

            return relationships;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            _myMapView = new MapView();
            _resultTextView = new UITextView
            {
                TextColor = UIColor.Black,
                Text = "Tap a shape to see its relationship with the others.",
                Editable = false,
                ScrollEnabled = false
            };

            _stackView = new UIStackView(new UIView[] { _myMapView, _resultTextView });
            _stackView.Distribution = UIStackViewDistribution.FillEqually;
            _stackView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView();
            View.AddSubview(_stackView);

            _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _stackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                // Landscape
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                // Portrait
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace StbHopper {
    public class StbHopperLine : GH_Component {

        private string path;
        private int NodeID, xNode_start, xNode_end, Nodeindex_start, Nodeindex_end;
        private double xPos, yPos, zPos;
        private List<Point3d> RhinoNodes = new List<Point3d>();
        private List<int>  RhinoNodeIDs = new List<int>();
        private List<Line> RhinoColumns = new List<Line>();
        private List<Line> RhinoGirders = new List<Line>();
        private List<Line> RhinoPosts = new List<Line>();
        private List<Line> RhinoBeams = new List<Line>();
        private List<Line> RhinoBraces  = new List<Line>();

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StbHopperLine()
          : base("StbtoLine", "StbtoLine", "Read ST-Bridge file and display", "StbHopper", "StbHopper") {
        }

        public override void ClearData() {
            base.ClearData();
            RhinoNodes.Clear();
            RhinoNodeIDs.Clear();
            RhinoColumns.Clear();
            RhinoGirders.Clear();
            RhinoPosts.Clear();
            RhinoBeams.Clear();
            RhinoBraces.Clear();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddTextParameter("path", "path", "input ST-Bridge file path", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddPointParameter("StbNodes", "StbNodes", "output StbNodes to point3d", GH_ParamAccess.list);
            pManager.AddLineParameter("StbColumns", "StbColumns", "output StbColumns to Line", GH_ParamAccess.list);
            pManager.AddLineParameter("StbGirders", "StbGirders", "output StbGirders to Line", GH_ParamAccess.list);
            pManager.AddLineParameter("StbPosts", "StbPosts", "output StbPosts to Line", GH_ParamAccess.list);
            pManager.AddLineParameter("StbBeams", "StbBeams", "output StbBeams to Line", GH_ParamAccess.list);
            pManager.AddLineParameter("StbBraces", "StbBraces", "output StbBraces to Line", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 対象の stb の pathを取得
            if (!DA.GetData("path", ref path)) { return; }

            var xdoc = XDocument.Load(path);

            // StbNode の取得
            var xNodes = xdoc.Root.Descendants("StbNode");
            foreach ( var xNode in xNodes) {
                xPos = (double)xNode.Attribute("x");
                yPos = (double)xNode.Attribute("y");
                zPos = (double)xNode.Attribute("z");
                NodeID = (int)xNode.Attribute("id");

                RhinoNodes.Add(new Point3d(xPos, yPos, zPos));
                RhinoNodeIDs.Add(NodeID);
            }

            // StbColumns の取得
            var xColumns = xdoc.Root.Descendants("StbColumn");
            foreach (var xColumn in xColumns) {
                xNode_start = (int)xColumn.Attribute("idNode_bottom");
                xNode_end = (int)xColumn.Attribute("idNode_top");

                Nodeindex_start = RhinoNodeIDs.IndexOf(xNode_start);
                Nodeindex_end = RhinoNodeIDs.IndexOf(xNode_end);

                RhinoColumns.Add(new Line(RhinoNodes[Nodeindex_start], RhinoNodes[Nodeindex_end]));
            }

            // StbGirder の取得
            var xGirders = xdoc.Root.Descendants("StbGirder");
            foreach (var xGirder in xGirders) {
                xNode_start = (int)xGirder.Attribute("idNode_start");
                xNode_end = (int)xGirder.Attribute("idNode_end");

                Nodeindex_start = RhinoNodeIDs.IndexOf(xNode_start);
                Nodeindex_end = RhinoNodeIDs.IndexOf(xNode_end);

                RhinoGirders.Add(new Line(RhinoNodes[Nodeindex_start], RhinoNodes[Nodeindex_end]));
            }

            // StbPosts の取得
            var xPosts = xdoc.Root.Descendants("StbPost");
            foreach (var xPost in xPosts) {
                xNode_start = (int)xPost.Attribute("idNode_bottom");
                xNode_end = (int)xPost.Attribute("idNode_top");

                Nodeindex_start = RhinoNodeIDs.IndexOf(xNode_start);
                Nodeindex_end = RhinoNodeIDs.IndexOf(xNode_end);

                RhinoPosts.Add(new Line(RhinoNodes[Nodeindex_start], RhinoNodes[Nodeindex_end]));
            }

            // StbBeam の取得
            var xBeams = xdoc.Root.Descendants("StbBeam");
            foreach (var xBeam in xBeams) {
                xNode_start = (int)xBeam.Attribute("idNode_start");
                xNode_end = (int)xBeam.Attribute("idNode_end");

                Nodeindex_start = RhinoNodeIDs.IndexOf(xNode_start);
                Nodeindex_end = RhinoNodeIDs.IndexOf(xNode_end);

                RhinoBeams.Add(new Line(RhinoNodes[Nodeindex_start], RhinoNodes[Nodeindex_end]));
            }

            // StbBrace の取得
            var xBraces = xdoc.Root.Descendants("StbBrace");
            foreach (var xBrace in xBraces) {
                xNode_start = (int)xBrace.Attribute("idNode_start");
                xNode_end = (int)xBrace.Attribute("idNode_end");

                Nodeindex_start = RhinoNodeIDs.IndexOf(xNode_start);
                Nodeindex_end = RhinoNodeIDs.IndexOf(xNode_end);

                RhinoBraces.Add(new Line(RhinoNodes[Nodeindex_start], RhinoNodes[Nodeindex_end]));
            }

            DA.SetDataList(0, RhinoNodes);
            DA.SetDataList(1, RhinoColumns);
            DA.SetDataList(2, RhinoGirders);
            DA.SetDataList(3, RhinoPosts);
            DA.SetDataList(4, RhinoBeams);
            DA.SetDataList(5, RhinoBraces);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("7d2f0c4e-4888-4607-8548-592104f6f06d"); }
        }
    }

    public class StbHopperBrep : GH_Component {

        private string path, Beam_shape, xBeam_kind, BeamType;
        private int NodeID, Nodeindex_i, Nodeindex_j, Nodeindex_k, Nodeindex_l, Nodeindex_start, Nodeindex_end,
                    xNode_start, xNode_end, xBeam_id_section,
                    StbSecIndex, Beam_id_section, BeamHight, BeamWidth;
        private double xPos, yPos, zPos, BeamAngleY;
        private Point3d Node_start, Node_end;
        private List<Point3d> RhinoNodes = new List<Point3d>();
        private List<int> RhinoNodeIDs = new List<int>();
        private List<int> xSlabNodeIDs = new List<int>();
        private List<int> xSecRcColumn_id = new List<int>();
        private List<int> xSecRcColumn_Depth = new List<int>();
        private List<int> xSecRcColumn_Width = new List<int>();
        private List<int> xSecSColumn_id = new List<int>();
        private List<string> xSecSColumn_shape = new List<string>();
        private List<int> xSecRcBeam_id = new List<int>();
        private List<int> xSecRcBeam_Depth = new List<int>();
        private List<int> xSecRcBeam_Width = new List<int>();
        private List<int> xSecSBeam_id = new List<int>();
        private List<string> xSecSBeam_shape = new List<string>();
        private List<int> xSecSBrace_id = new List<int>();
        private List<string> xSecSBrace_shape = new List<string>();
        private List<string> xStbSecSteel_name = new List<string>();
        private List<int> xStbSecSteel_A = new List<int>();
        private List<int> xStbSecSteel_B = new List<int>();
        private List<string> xStbSecSteel_type = new List<string>();
        private List<Brep> RhinoSlabs = new List<Brep>();
        private List<Brep> SteelSectionBrep = new List<Brep>();
        private Brep[] RhinoColumns, RhinoGirders, RhinoPosts, RhinoBeams, S_Braces, SteelBeamBrepArray;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StbHopperBrep()
          : base("StbToBrep", "StbToBrep", "Read ST-Bridge file and display", "StbHopper", "StbHopper") {
        }

        public override void ClearData() {
            base.ClearData();
            RhinoNodes.Clear();
            RhinoNodeIDs.Clear();
            RhinoSlabs.Clear();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddTextParameter("path", "path", "input ST-Bridge file path", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddBrepParameter("StbColumns", "StbColumns", "output StbColumns to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("StbGirders", "StbGirders", "output StbGirders to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("StbPosts", "StbPosts", "output StbPosts to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("StbBeams", "StbBeams", "output StbBeams to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("StbSlabs", "StbSlabs", "output StbSlabs to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("StbBraces", "StbBraces", "output StbBraces to Brep", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 対象の stb の pathを取得
            if (!DA.GetData("path", ref path)) { return; }

            var xdoc = XDocument.Load(path);

            // StbNode の取得
            var xNodes = xdoc.Root.Descendants("StbNode");
            foreach (var xNode in xNodes) {
                xPos = (double)xNode.Attribute("x");
                yPos = (double)xNode.Attribute("y");
                zPos = (double)xNode.Attribute("z");
                NodeID = (int)xNode.Attribute("id");

                RhinoNodes.Add(new Point3d(xPos, yPos, zPos));
                RhinoNodeIDs.Add(NodeID);
            }

            // StbSlabs の取得
            var xSlabs = xdoc.Root.Descendants("StbSlab");
            foreach (var xSlab in xSlabs) {
                var xNodeids = xSlab.Element("StbNodeid_List")
                                    .Elements("StbNodeid");
                int CountNode = 0;
                foreach (var xNodeid in xNodeids) {
                    xSlabNodeIDs.Add((int)xNodeid.Attribute("id"));

                    CountNode = CountNode + 1;
                }

                Brep SlabBrep = new Brep();
                Nodeindex_i = RhinoNodeIDs.IndexOf(xSlabNodeIDs[0]);
                Nodeindex_j = RhinoNodeIDs.IndexOf(xSlabNodeIDs[1]);
                Nodeindex_k = RhinoNodeIDs.IndexOf(xSlabNodeIDs[2]);

                // StbSlabの定義上4節点以外許容されているのか不明だが場合分けしてる
                if (CountNode == 4) {
                    Nodeindex_l = RhinoNodeIDs.IndexOf(xSlabNodeIDs[3]);
                    SlabBrep = Brep.CreateFromCornerPoints(RhinoNodes[Nodeindex_i], RhinoNodes[Nodeindex_j], RhinoNodes[Nodeindex_k], RhinoNodes[Nodeindex_l], GH_Component.DocumentTolerance());
                }
                else {
                    SlabBrep = Brep.CreateFromCornerPoints(RhinoNodes[Nodeindex_i], RhinoNodes[Nodeindex_j], RhinoNodes[Nodeindex_k], GH_Component.DocumentTolerance());
                }

                xSlabNodeIDs.Clear(); // foreachごとでListにAddし続けてるのでここで値をClear
                RhinoSlabs.Add(SlabBrep);
            }

            // StbSecColumn_RC の取得
            var xSecRcColumns = xdoc.Root.Descendants("StbSecColumn_RC");
            foreach (var xSecRcColumn in xSecRcColumns) {
                xSecRcColumn_id.Add((int)xSecRcColumn.Attribute("id"));
                var xSecFigure = xSecRcColumn.Element("StbSecFigure");

                // 子要素が StbSecRect か StbSecCircle を判定
                if (xSecFigure.Element("StbSecRect") != null) {
                    xSecRcColumn_Depth.Add((int)xSecFigure.Element("StbSecRect").Attribute("DY"));
                    xSecRcColumn_Width.Add((int)xSecFigure.Element("StbSecRect").Attribute("DX"));
                }
                else {
                    xSecRcColumn_Depth.Add((int)xSecFigure.Element("StbSecCircle").Attribute("D"));
                    xSecRcColumn_Width.Add( 0 ); // Circle と判定用に width は 0
                }
            }
            // StbSecColumn_S の取得
            var xSecSColumns = xdoc.Root.Descendants("StbSecColumn_S");
            foreach (var xSecSColumn in xSecSColumns) {
                xSecSColumn_id.Add((int)xSecSColumn.Attribute("id"));
                xSecSColumn_shape.Add((string)xSecSColumn.Element("StbSecSteelColumn").Attribute("shape"));
            }
            // StbSecBeam_RC の取得
            var xSecRcBeams = xdoc.Root.Descendants("StbSecBeam_RC");
            foreach (var xSecRcBeam in xSecRcBeams) {
                xSecRcBeam_id.Add((int)xSecRcBeam.Attribute("id"));
                var xSecFigure = xSecRcBeam.Element("StbSecFigure");
                
                // 子要素が StbSecHaunch か StbSecStraight を判定
                if (xSecFigure.Element("StbSecHaunch") != null) {
                    xSecRcBeam_Depth.Add((int)xSecFigure.Element("StbSecHaunch").Attribute("depth_center"));
                    xSecRcBeam_Width.Add((int)xSecFigure.Element("StbSecHaunch").Attribute("width_center"));
                } 
                else {
                    xSecRcBeam_Depth.Add((int)xSecFigure.Element("StbSecStraight").Attribute("depth"));
                    xSecRcBeam_Width.Add((int)xSecFigure.Element("StbSecStraight").Attribute("width"));
                }
            }
            // StbSecBeam_S の取得
            var xSecSBeams = xdoc.Root.Descendants("StbSecBeam_S");
            foreach (var xSecSBeam in xSecSBeams) {
                xSecSBeam_id.Add((int)xSecSBeam.Attribute("id"));
                xSecSBeam_shape.Add((string)xSecSBeam.Element("StbSecSteelBeam").Attribute("shape"));
            }
            // StbSecBrace_S の取得
            var xSecSBraces = xdoc.Root.Descendants("StbSecBrace_S");
            foreach (var xSecSBrace in xSecSBraces) {
                xSecSBrace_id.Add((int)xSecSBrace.Attribute("id"));
                xSecSBrace_shape.Add((string)xSecSBrace.Element("StbSecSteelBrace").Attribute("shape"));
            }

            // S断面形状の取得
            GetStbSteelSection(xdoc, "StbSecRoll-H", "H");
            GetStbSteelSection(xdoc, "StbSecBuild-H", "H");
            GetStbSteelSection(xdoc, "StbSecRoll-BOX", "BOX");
            GetStbSteelSection(xdoc, "StbSecBuild-BOX", "BOX");
            GetStbSteelSection(xdoc, "StbSecPipe", "Pipe");
            GetStbSteelSection(xdoc, "StbSecRoll-L", "L");


            // 柱の断面の生成
            RhinoColumns = MakeColumnBrep(xdoc, "StbColumn");
            // 大梁の断面の生成
            RhinoGirders = MakeBeamBrep(xdoc, "StbGirder");
            // 間柱の断面の生成
            RhinoPosts = MakeColumnBrep(xdoc, "StbPost");
            // 小梁の断面の生成
            RhinoBeams = MakeBeamBrep(xdoc, "StbBeam");
            // Sブレ―スの断面の生成
            S_Braces = MakeSteelBraceBrep(xdoc, "StbBrace");

            DA.SetDataList(0, RhinoColumns);
            DA.SetDataList(1, RhinoGirders);
            DA.SetDataList(2, RhinoPosts);
            DA.SetDataList(3, RhinoBeams);
            DA.SetDataList(4, RhinoSlabs);
            DA.SetDataList(5, S_Braces);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("7d2f0c4e-4888-4607-8548-592104f6f06f"); }
        }

        /// <summary>
        /// Get ST-Bridge Steel Section
        /// </summary>
        /// <param name="xdoc"></param>
        /// <param name="xDateTag"></param>
        /// <param name="SectionType"></param>
        public void GetStbSteelSection(XDocument xdoc, string xDateTag, string SectionType) {
            if(SectionType == "Pipe") {
                var xSteelSections = xdoc.Root.Descendants(xDateTag);
                foreach (var xSteelSection in xSteelSections) {
                    xStbSecSteel_name.Add((string)xSteelSection.Attribute("name"));
                    xStbSecSteel_A.Add((int)xSteelSection.Attribute("D"));
                    xStbSecSteel_B.Add((int)xSteelSection.Attribute("t"));
                    xStbSecSteel_type.Add(SectionType);
                }
            }
            else {
                var xSteelSections = xdoc.Root.Descendants(xDateTag);
                foreach (var xSteelSection in xSteelSections) {
                    xStbSecSteel_name.Add((string)xSteelSection.Attribute("name"));
                    xStbSecSteel_A.Add((int)xSteelSection.Attribute("A"));
                    xStbSecSteel_B.Add((int)xSteelSection.Attribute("B"));
                    xStbSecSteel_type.Add(SectionType);
                }
            }
        }

        /// <summary> 
        /// Make Beam Brep
        /// </summary>
        /// <param name="xdoc"></param>
        /// <param name="xDateTag"></param>
        /// <returns></returns>
        public Brep[] MakeBeamBrep( XDocument xdoc, string xDateTag) {
            SteelSectionBrep.Clear();

            var xBeams = xdoc.Root.Descendants(xDateTag);
            foreach (var xBeam in xBeams) {
                xNode_start = (int)xBeam.Attribute("idNode_start");
                xNode_end = (int)xBeam.Attribute("idNode_end");
                xBeam_id_section = (int)xBeam.Attribute("id_section");
                xBeam_kind = (string)xBeam.Attribute("kind_structure");

                // 始点と終点の座標取得
                Nodeindex_start = RhinoNodeIDs.IndexOf(xNode_start);
                Nodeindex_end = RhinoNodeIDs.IndexOf(xNode_end);
                Node_start = RhinoNodes[Nodeindex_start];
                Node_end = RhinoNodes[Nodeindex_end];

                if (xBeam_kind == "RC") {
                    // 断面形状名（shape）の取得
                    StbSecIndex = xSecRcBeam_id.IndexOf(xBeam_id_section);
                    BeamHight = xSecRcBeam_Depth[StbSecIndex];
                    BeamWidth = xSecRcBeam_Width[StbSecIndex];
                    BeamType = "BOX";
                }
                else if (xBeam_kind == "S") {
                    // 断面形状名（shape）の取得
                    Beam_id_section = xSecSBeam_id.IndexOf(xBeam_id_section);
                    Beam_shape = xSecSBeam_shape[Beam_id_section];

                    // 断面形状（HxB）の取得
                    StbSecIndex = xStbSecSteel_name.IndexOf(Beam_shape);
                    BeamHight = xStbSecSteel_A[StbSecIndex];
                    BeamWidth = xStbSecSteel_B[StbSecIndex];
                    BeamType = xStbSecSteel_type[StbSecIndex];
                }

                // 梁断面サーフェスの作成
                BeamAngleY = Math.Atan((Node_end.Y - Node_start.Y) / (Node_end.X - Node_start.X));
                if (BeamType == "H") {
                    // make upper flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make bottom flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make web 
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(Node_start,
                                                                        new Point3d(Node_start.X, Node_start.Y, Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X, Node_end.Y, Node_end.Z - BeamHight),
                                                                        Node_end,
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                }
                else if (BeamType == "BOX") {
                    // make upper flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make bottom flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make web 1
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                        new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make web 2
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                }
                else if (BeamType == "Pipe") {
                    LineCurve PipeCurve = new LineCurve(Node_start, Node_end);
                    SteelSectionBrep.Add(Brep.CreatePipe(PipeCurve, BeamHight / 2.0, false, 0, false, GH_Component.DocumentTolerance(), GH_Component.DocumentAngleTolerance())[0]);
                }
                else if (BeamType == "L") {
                    // make bottom flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make web
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                }
                else {
                }
            }
            SteelBeamBrepArray = Brep.JoinBreps(SteelSectionBrep, GH_Component.DocumentTolerance());
            return SteelBeamBrepArray;
        }

        public Brep[] MakeColumnBrep(XDocument xdoc, string xDateTag) {
            SteelSectionBrep.Clear();

            var xBeams = xdoc.Root.Descendants(xDateTag);
            foreach (var xBeam in xBeams) {
                xNode_start = (int)xBeam.Attribute("idNode_bottom");
                xNode_end = (int)xBeam.Attribute("idNode_top");
                xBeam_id_section = (int)xBeam.Attribute("id_section");
                xBeam_kind = (string)xBeam.Attribute("kind_structure");

                // 始点と終点の座標取得
                Nodeindex_start = RhinoNodeIDs.IndexOf(xNode_start);
                Nodeindex_end = RhinoNodeIDs.IndexOf(xNode_end);
                Node_start = RhinoNodes[Nodeindex_start];
                Node_end = RhinoNodes[Nodeindex_end];

                if (xBeam_kind == "RC") {
                    // 断面形状名（shape）の取得
                    StbSecIndex = xSecRcColumn_id.IndexOf(xBeam_id_section);
                    BeamHight = xSecRcColumn_Depth[StbSecIndex];
                    BeamWidth = xSecRcColumn_Width[StbSecIndex];
                    if (BeamWidth == 0) {
                        BeamType = "Pipe";
                    }
                    else {
                        BeamType = "BOX";
                    }
                }
                else if (xBeam_kind == "S") {
                    // 断面形状名（shape）の取得
                    Beam_id_section = xSecSColumn_id.IndexOf(xBeam_id_section);
                    Beam_shape = xSecSColumn_shape[Beam_id_section];

                    // 断面形状（HxB）の取得
                    StbSecIndex = xStbSecSteel_name.IndexOf(Beam_shape);
                    BeamHight = xStbSecSteel_A[StbSecIndex];
                    BeamWidth = xStbSecSteel_B[StbSecIndex];
                    BeamType = xStbSecSteel_type[StbSecIndex];
                }

                // 柱断面サーフェスの作成
                BeamAngleY = Math.Atan((Node_end.Z - Node_start.Z) / (Node_end.X - Node_start.X));
                if (BeamType == "H") {
                    // make upper flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamHight / 2.0), Node_start.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamHight / 2.0), Node_start.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamHight / 2.0), Node_end.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamHight / 2.0), Node_end.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make bottom flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamHight / 2.0), Node_start.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamHight / 2.0), Node_start.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamHight / 2.0), Node_end.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamHight / 2.0), Node_end.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make web 
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X, Node_start.Y + (BeamHight / 2.0), Node_start.Z),
                                                                        new Point3d(Node_start.X, Node_start.Y - (BeamHight / 2.0), Node_start.Z),
                                                                        new Point3d(Node_end.X, Node_end.Y - (BeamHight / 2.0), Node_end.Z),
                                                                        new Point3d(Node_end.X, Node_end.Y + (BeamHight / 2.0), Node_end.Z),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                }
                else if (BeamType == "BOX") {
                    // make upper flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamHight / 2.0), Node_start.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamHight / 2.0), Node_start.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamHight / 2.0), Node_end.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamHight / 2.0), Node_end.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make bottom flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamHight / 2.0), Node_start.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamHight / 2.0), Node_start.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamHight / 2.0), Node_end.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamHight / 2.0), Node_end.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make web 1
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamHight / 2.0), Node_start.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamHight / 2.0), Node_start.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamHight / 2.0), Node_end.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamHight / 2.0), Node_end.Z + (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make web 2
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamHight / 2.0), Node_start.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamHight / 2.0), Node_start.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamHight / 2.0), Node_end.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamHight / 2.0), Node_end.Z - (BeamWidth / 2.0) * Math.Cos(BeamAngleY)),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                }
                else if (BeamType == "Pipe") {
                    LineCurve PipeCurve = new LineCurve(Node_start, Node_end);
                    SteelSectionBrep.Add(Brep.CreatePipe(PipeCurve, BeamHight / 2.0, false, 0, false, GH_Component.DocumentTolerance(), GH_Component.DocumentAngleTolerance())[0]);
                }
                else if (BeamType == "L") {
                    // make bottom flange
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                    // make web
                    SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                        new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                        new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                        GH_Component.DocumentTolerance()
                                                                        ));
                }
                else {
                }
            }
            SteelBeamBrepArray = Brep.JoinBreps(SteelSectionBrep, GH_Component.DocumentTolerance());
            return SteelBeamBrepArray;
        }

        public Brep[] MakeSteelBraceBrep(XDocument xdoc, string xDateTag) {
            SteelSectionBrep.Clear();

            var xBeams = xdoc.Root.Descendants(xDateTag);
            foreach (var xBeam in xBeams) {
                xNode_start = (int)xBeam.Attribute("idNode_start");
                xNode_end = (int)xBeam.Attribute("idNode_end");
                xBeam_id_section = (int)xBeam.Attribute("id_section");
                xBeam_kind = (string)xBeam.Attribute("kind_structure");

                if (xBeam_kind == "S") {
                    // 始点と終点の座標取得
                    Nodeindex_start = RhinoNodeIDs.IndexOf(xNode_start);
                    Nodeindex_end = RhinoNodeIDs.IndexOf(xNode_end);
                    Node_start = RhinoNodes[Nodeindex_start];
                    Node_end = RhinoNodes[Nodeindex_end];

                    // 断面形状名（shape）の取得
                    Beam_id_section = xSecSBrace_id.IndexOf(xBeam_id_section);
                    Beam_shape = xSecSBrace_shape[Beam_id_section];

                    // 断面形状（HxB）の取得
                    StbSecIndex = xStbSecSteel_name.IndexOf(Beam_shape);
                    BeamHight = xStbSecSteel_A[StbSecIndex];
                    BeamWidth = xStbSecSteel_B[StbSecIndex];
                    BeamType = xStbSecSteel_type[StbSecIndex];

                    // ブレース断面サーフェスの作成
                    BeamAngleY = Math.Atan((Node_end.Y - Node_start.Y) / (Node_end.X - Node_start.X));
                    if (BeamType == "H") {
                        // make upper flange
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                         new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                         new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                        // make bottom flange
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                        // make web 
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(Node_start,
                                                                         new Point3d(Node_start.X, Node_start.Y, Node_start.Z - BeamHight),
                                                                         new Point3d(Node_end.X, Node_end.Y, Node_end.Z - BeamHight),
                                                                         Node_end,
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                    }
                    else if (BeamType == "BOX") {
                        // make upper flange
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                         new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                         new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                        // make bottom flange
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                        // make web 1
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                         new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                        // make web 2
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                         new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                    }
                    else if (BeamType == "Pipe") {
                        LineCurve PipeCurve = new LineCurve(Node_start, Node_end);
                        SteelSectionBrep.Add(Brep.CreatePipe(PipeCurve, BeamHight / 2.0, false, 0, false, GH_Component.DocumentTolerance(), GH_Component.DocumentAngleTolerance())[0]);
                    }
                    else if (BeamType == "L") {
                        // make bottom flange
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         new Point3d(Node_end.X - (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y - (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                        // make web
                        SteelSectionBrep.Add(Brep.CreateFromCornerPoints(new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z),
                                                                         new Point3d(Node_start.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_start.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_start.Z - BeamHight),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z - BeamHight),
                                                                         new Point3d(Node_end.X + (BeamWidth / 2.0) * Math.Sin(BeamAngleY), Node_end.Y + (BeamWidth / 2.0) * Math.Cos(BeamAngleY), Node_end.Z),
                                                                         GH_Component.DocumentTolerance()
                                                                         ));
                    }
                    else {
                    }
                }
            }
            SteelBeamBrepArray = Brep.JoinBreps(SteelSectionBrep, GH_Component.DocumentTolerance());
            return SteelBeamBrepArray;
        }
    }
}

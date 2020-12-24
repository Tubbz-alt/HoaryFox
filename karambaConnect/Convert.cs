﻿using System.Collections.Generic;

namespace karambaConnect
{
    public static class Convert
    {
        public static STBDotNet.Geometry.Point3 ToStb(this Karamba.Geometry.Point3 kpt)
        {
            return new STBDotNet.Geometry.Point3(kpt.X, kpt.Y, kpt.Z);
        }

        public static Karamba.Geometry.Point3 ToKaramba(this STBDotNet.Geometry.Point3 spt)
        {
            return new Karamba.Geometry.Point3(spt.X, spt.Y, spt.Z);
        }

        public static List<STBDotNet.Elements.StbModel.Node> ToStb(this List<Karamba.Nodes.Node> kNodes)
        {
            var sNodes = new List<STBDotNet.Elements.StbModel.Node>();

            foreach (Karamba.Nodes.Node kNode in kNodes)
            {
                var sNode = new STBDotNet.Elements.StbModel.Node()
                {
                    Id = kNode.ind,
                    X = kNode.pos.X,
                    Y = kNode.pos.Y,
                    Z = kNode.pos.Z
                };
                sNodes.Add(sNode);
            }

            return sNodes;
        }

        public static List<Karamba.Nodes.Node> ToKaramba(this List<STBDotNet.Elements.StbModel.Node> sNodes)
        {
            var kNodes = new List<Karamba.Nodes.Node>();

            foreach (STBDotNet.Elements.StbModel.Node sNode in sNodes)
            {
                var kNode = new Karamba.Nodes.Node
                {
                    ind = sNode.Id,
                    is_visible = true,
                    pos = sNode.Position.ToKaramba()
                };
                
                kNodes.Add(kNode);
            }

            return kNodes;
        }
    }
}
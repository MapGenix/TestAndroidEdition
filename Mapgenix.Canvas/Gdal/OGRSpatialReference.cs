using System;

namespace Mapgenix.Canvas
{
    internal partial class OgrSpatialReference
    {
        private double _fromGreenwich;

        private bool _isNormInfoSet;
        private double _toDegrees;
        private double _toMeter;
   
        internal OgrsrsNode Node;

        internal OgrSpatialReference()
        {
            Clear();
        }

        internal OgrSpatialReference(string wkt)
        {
            _isNormInfoSet = false;
            Node = null;
            if (!string.IsNullOrEmpty(wkt))
                ImportFromWkt(wkt);
        }

        internal OgrSpatialReference(OgrSpatialReference ogrOther)
        {
            _isNormInfoSet = false;
            Node = null;
            if (ogrOther.Node != null)
                Node = ogrOther.Node.Clone();
        }

        private void Clear()
        {
            Node = new OgrsrsNode();
            _isNormInfoSet = false;
            _fromGreenwich = 1.0;
            _toMeter = 1.0;
            _toDegrees = 1.0;
        }


        internal OgrsrsNode GetAttrNode(string nodePath)
        {
            var pathSequence = nodePath.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            if (pathSequence.Length < 1)
                return null;
            var targetNode = Node;

            foreach (var nodeName in pathSequence)
            {
                if (targetNode != null)
                {
                    targetNode = targetNode.GetNode(nodeName);
                }
            }
            return targetNode;
        }

        private string GetAttrValue(string nodeName)
        {
            return GetAttrValue(nodeName, 0);
        }

        private string GetAttrValue(string nodeName, int valueIndex)
        {
            if (valueIndex < 0)
                return null;
            var targetNode = GetAttrNode(nodeName);

            if (targetNode == null || valueIndex >= targetNode.ChildNodes.Count)
                return null;
            return targetNode.ChildNodes[valueIndex].Value;
        }


        internal string ExportToWkt()
        {
            if (Node == null)
                throw new NullReferenceException("Node is not initialized");

            return Node.ExportToWkt();
        }

        internal void ImportFromWkt(string wkt)
        {
            Node = new OgrsrsNode();
            var wktArray = wkt.ToCharArray();
            var processedIndex = 0;

            Node.ImportFromWkt(wktArray, ref processedIndex);
        }

        private void SetNode(string nodePath, string nodeValue)
        {
            var pathSequence = nodePath.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            if (pathSequence.Length < 1)
            {
                throw new FormatException("The nodePath is not well formatted.");
            }
            if (Node == null || !string.Equals(pathSequence[0], Node.Value, StringComparison.OrdinalIgnoreCase))
            {
                Node = new OgrsrsNode();
                Node.Value = pathSequence[0];
            }
            var currentNode = Node;
            for (var i = 1; i < pathSequence.Length; i++)
            {
                int j;
                for (j = 0; j < currentNode.ChildNodes.Count; j++)
                {
                    if (string.Equals(currentNode.ChildNodes[j].Value, pathSequence[i],
                        StringComparison.OrdinalIgnoreCase))
                    {
                        currentNode = currentNode.ChildNodes[j];
                        j = -1;
                        break;
                    }
                }
                if (j != -1)
                {
                    var newNode = new OgrsrsNode(pathSequence[i]);
                    currentNode.ChildNodes.Add(newNode);
                    currentNode = newNode;
                }
            }

            if (!string.IsNullOrEmpty(nodeValue))
            {
                if (currentNode.ChildNodes.Count > 0)
                    currentNode.ChildNodes[0].Value = nodeValue;
                else
                    currentNode.ChildNodes.Add(new OgrsrsNode(nodeValue));
            }
        }

        private void SetAngularUnits(string name, double inRadians)
        {
            OgrsrsNode csNode;
            OgrsrsNode unitsNode;

            _isNormInfoSet = false;

            csNode = GetAttrNode("GEOGCS");

            if (csNode == null)
                throw new NullReferenceException(string.Format("Can not find GEOGCS Node for {0}", name));

            var value = inRadians.ToString();

            if (csNode.FindChild("UNIT") >= 0)
            {
                unitsNode = csNode.ChildNodes[(csNode.FindChild("UNIT"))];
                unitsNode.ChildNodes[0].Value = name;
                unitsNode.ChildNodes[0].Value = value;
            }
            else
            {
                unitsNode = new OgrsrsNode("UNIT");
                unitsNode.ChildNodes.Add(new OgrsrsNode(name));
                unitsNode.ChildNodes.Add(new OgrsrsNode(value));

                csNode.ChildNodes.Add(unitsNode);
            }
        }

        private double GetAngularUnits(ref string parameterName)
        {
            var geogCsNode = GetAttrNode("GEOGCS");
            if (parameterName != null)
                parameterName = "degree";
            if (geogCsNode == null)
            {
                return GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUaDegreeConv);
            }

            foreach (var childNode in geogCsNode.ChildNodes)
            {
                if (string.Equals(childNode.Value, "UNIT", StringComparison.OrdinalIgnoreCase) &&
                    childNode.ChildNodes.Count >= 2)
                {
                    if (!string.IsNullOrEmpty(parameterName))
                        parameterName = childNode.ChildNodes[0].Value;

                    return GetDoubleOutOfString(childNode.ChildNodes[1].Value);
                }
            }
            return 1.0;
        }

        

        private void SetLinearUnits(string unitName, double inMeter)
        {
            var csNode = GetAttrNode("PROJCS");
            OgrsrsNode unitNode;
            _isNormInfoSet = false;
            if (csNode == null)
            {
                if ((csNode = GetAttrNode("LOCAL_CS")) == null)
                {
                    throw new NullReferenceException("Can not find the PROJCS node.");
                }
            }
            if (csNode.FindChild("UNIT") >= 0)
            {
                unitNode = csNode.ChildNodes[csNode.FindChild("UNIT")];
                unitNode.ChildNodes[0].Value = unitName;
                unitNode.ChildNodes[1].Value = inMeter.ToString();
                var authNodeIndex = unitNode.FindChild("AUTHORITY");
                if (authNodeIndex != -1)
                {
                    unitNode.ChildNodes.RemoveAt(authNodeIndex);
                }
            }
            else
            {
                unitNode = new OgrsrsNode("UNIT");
                unitNode.ChildNodes.Add(new OgrsrsNode(unitName));
                unitNode.ChildNodes.Add(new OgrsrsNode(inMeter.ToString()));

                csNode.ChildNodes.Add(unitNode);
            }
        }

        private double GetLinearUnits()
        {
            string parametername = null;
            return GetLinearUnits(ref parametername);
        }

        private double GetLinearUnits(ref string parameterName)
        {
            var projCsNode = GetAttrNode("PROJCS");
            if (projCsNode == null)
                projCsNode = GetAttrNode("LOCAL_CS");
            if (parameterName != null)
                parameterName = "unknown";
            if (projCsNode == null)
                return 1.0;

            foreach (var childNode in projCsNode.ChildNodes)
            {
                if (string.Equals(childNode.Value, "UNIT", StringComparison.OrdinalIgnoreCase) &&
                    childNode.ChildNodes.Count >= 2)
                {
                    if (!string.IsNullOrEmpty(parameterName))
                        parameterName = childNode.ChildNodes[0].Value;
                    return GetDoubleOutOfString(childNode.ChildNodes[1].Value);
                }
            }
            return 1.0;
        }

        private double GetPrimeMeridian()
        {
            var parameterName = string.Empty;
            return GetPrimeMeridian(ref parameterName);
        }

        private double GetPrimeMeridian(ref string parameterName)
        {
            var primemNode = GetAttrNode("PRIMEM");

            if (primemNode != null && primemNode.ChildNodes.Count >= 2)
            {
                if (!string.IsNullOrEmpty(parameterName))
                    parameterName = primemNode.ChildNodes[0].Value;
                return GetDoubleOutOfString(primemNode.ChildNodes[1].Value);
            }
            if (!string.IsNullOrEmpty(parameterName))
                parameterName = OgrCoreDefinitiaons.SrsPmGreenwich;
            return 0.0;
        }

        internal void SetGeogCS(string geogName, string datumName, string spheroidName, double semiMajor,
            double invFlattening)
        {
            SetGeogCS(geogName, datumName, spheroidName, semiMajor, invFlattening, null, 0, null, 0);
        }

        internal void SetGeogCS(string geogName, string datumName, string spheroidName, double semiMajor,
            double invFlattening, string pmName, double pmOffset)
        {
            SetGeogCS(geogName, datumName, spheroidName, semiMajor, invFlattening, pmName, pmOffset, null, 0);
        }

        internal void SetGeogCS(string geogName, string datumName, string spheroidName, double semiMajor,
            double invFlattening, string pmName, double pmOffset, string angularUnits, double convertToRadians)
        {
            _isNormInfoSet = false;

            if (GetAttrNode("GEOGCS") != null)
            {
                OgrsrsNode projCsNode;
                if (string.Equals(Node.Value, "GEOGCS"))
                    Clear();
                else if ((projCsNode = GetAttrNode("PROJCS")) != null && projCsNode.FindChild("GEOGCS") != -1)
                {
                    projCsNode.ChildNodes.RemoveAt(projCsNode.FindChild("GEOGCS"));
                }
                else
                    throw new NullReferenceException("Can not find node for GEOGCS");
            }

           

            if (string.IsNullOrEmpty(geogName))
                geogName = "unnamed";
            if (pmName == null)
                pmName = OgrCoreDefinitiaons.SrsPmGreenwich;
            if (datumName == null)
                datumName = "unknown";
            if (spheroidName == null)
                spheroidName = "unnamed";
            if (angularUnits == null)
            {
                angularUnits = OgrCoreDefinitiaons.SrsUaDegree;
                convertToRadians = GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUaDegreeConv);
            }

           

           

            OgrsrsNode geogCsNode, spheroidNode, datumNode, pmNode, unitsNode;
            geogCsNode = new OgrsrsNode("GEOGCS");
            geogCsNode.ChildNodes.Add(new OgrsrsNode(geogName));

            string doubleValueString;
            spheroidNode = new OgrsrsNode("SPHEROID");
            spheroidNode.ChildNodes.Add(new OgrsrsNode(spheroidName));
            doubleValueString = semiMajor.ToString();
            spheroidNode.ChildNodes.Add(new OgrsrsNode(doubleValueString));
            doubleValueString = invFlattening.ToString();
            spheroidNode.ChildNodes.Add(new OgrsrsNode(doubleValueString));

            
            datumNode = new OgrsrsNode("DATUM");
            datumNode.ChildNodes.Add(new OgrsrsNode(datumName));
            datumNode.ChildNodes.Add(spheroidNode);
   
            if (pmOffset == 0.0)
                doubleValueString = "0";
            else
                doubleValueString = pmOffset.ToString();
            pmNode = new OgrsrsNode("PRIMEM");
            pmNode.ChildNodes.Add(new OgrsrsNode(pmName));
            pmNode.ChildNodes.Add(new OgrsrsNode(doubleValueString));

            doubleValueString = convertToRadians.ToString();
            unitsNode = new OgrsrsNode("UNIT");
            unitsNode.ChildNodes.Add(new OgrsrsNode(angularUnits));
            unitsNode.ChildNodes.Add(new OgrsrsNode(doubleValueString));

            geogCsNode.ChildNodes.Add(datumNode);
            geogCsNode.ChildNodes.Add(pmNode);
            geogCsNode.ChildNodes.Add(unitsNode);

            if (Node != null && string.Equals(Node.Value, "PROJCS", StringComparison.OrdinalIgnoreCase))
                Node.ChildNodes.Insert(1, geogCsNode);
            else
                Node = geogCsNode;

           
        }

        private void SetWellKnownGeogCs(string nameValue)
        {
            var oSrs2 = new OgrSpatialReference();
            
            if (nameValue.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase))
            {
                oSrs2.ImportFromEpsg(GetIntOutOfString(nameValue.Substring(5, nameValue.Length - 5)));
                if (!oSrs2.IsGeographic())
                    throw new InvalidOperationException(string.Format("Failed to set WellKnownGeogCS for {0}", nameValue));
                CopyGeogCsFrom(oSrs2);
                return;
            }
                 
            if (nameValue.StartsWith("EPSGA:", StringComparison.OrdinalIgnoreCase))
            {
                oSrs2.ImportFromEpsga(GetIntOutOfString(nameValue.Substring(6, nameValue.Length - 6)));
                if (!oSrs2.IsGeographic())
                    throw new InvalidOperationException(string.Format("Failed to set WellKnownGeogCS for {0}", nameValue));
                CopyGeogCsFrom(oSrs2);
                return;
            }
           
            string wkt;
            if (string.Equals(nameValue, "WGS84") || string.Equals(nameValue, "CRS84"))
                wkt = OgrCoreDefinitiaons.SrsWktWgs84;
            else if (string.Equals(nameValue, "WGS72"))
                wkt =
                    "GEOGCS[\"WGS 72\",DATUM[\"WGS_1972\",SPHEROID[\"WGS 72\",6378135,298.26,AUTHORITY[\"EPSG\",\"7043\"]],TOWGS84[0,0,4.5,0,0,0.554,0.2263],AUTHORITY[\"EPSG\",\"6322\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9108\"]],AUTHORITY[\"EPSG\",\"4322\"]]";
            else if (string.Equals(nameValue, "NAD27") || string.Equals(nameValue, "CRS27"))
                wkt =
                    "GEOGCS[\"NAD27\",DATUM[\"North_American_Datum_1927\",SPHEROID[\"Clarke 1866\",6378206.4,294.978698213898,AUTHORITY[\"EPSG\",\"7008\"]],TOWGS84[-3,142,183,0,0,0,0],AUTHORITY[\"EPSG\",\"6267\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9108\"]],AUTHORITY[\"EPSG\",\"4267\"]]";
            else if (string.Equals(nameValue, "NAD83") || string.Equals(nameValue, "CRS83"))
                wkt =
                    "GEOGCS[\"NAD83\",DATUM[\"North_American_Datum_1983\",SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[\"EPSG\",\"6269\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9108\"]],AUTHORITY[\"EPSG\",\"4269\"]]";
            else
                throw new InvalidOperationException(string.Format("Failed to set WellKnownGeogCS for {0}", nameValue));
            oSrs2.ImportFromWkt(wkt);
            CopyGeogCsFrom(oSrs2);
        }

        private void CopyGeogCsFrom(OgrSpatialReference other)
        {
            OgrsrsNode geogCsNode;
            _isNormInfoSet = false;

            if (GetAttrNode("GEOGCS") != null)
            {
                OgrsrsNode projCsNode;
                if (string.Equals(Node.Value, "GEOGCS", StringComparison.OrdinalIgnoreCase))
                    Clear();
                else if ((projCsNode = GetAttrNode("PROJCS")) != null && projCsNode.FindChild("GEOGCS") != -1)
                    projCsNode.ChildNodes.RemoveAt(projCsNode.FindChild("GEOGCS"));
                else
                    throw new Exception("Failed to Set WellKnownGeogCS from other Reference.");
            }

            geogCsNode = other.GetAttrNode("GEOGCS");

            if (geogCsNode == null)
                throw new Exception("Failed to Set WellKnownGeogCS from other Reference.");

            if (Node != null && string.Equals(Node.Value, "PROJCS", StringComparison.OrdinalIgnoreCase))
                Node.ChildNodes.Insert(1, geogCsNode.Clone());
            else
                Node = geogCsNode.Clone();
        }

        private double GetSemiMajor()
        {
            var spheroidNode = GetAttrNode("SPHEROID");
            if (spheroidNode == null)
            {
                throw new NullReferenceException("Node for Spheroid cannot be found!");
            }

            if (spheroidNode.ChildNodes.Count >= 3)
            {
                return GetDoubleOutOfString(spheroidNode.ChildNodes[1].Value);
            }
            return OgrCoreDefinitiaons.SrsWgs84Semimajor;
        }

        private double GetInvFlattening()
        {
            var spheroidNode = GetAttrNode("SPHEROID");
            if (spheroidNode == null)
            {
                throw new NullReferenceException("Node for Spheroid cannot be found!");
            }
            if (spheroidNode.ChildNodes.Count >= 3)
            {
                return GetDoubleOutOfString(spheroidNode.ChildNodes[2].Value);
            }
            return OgrCoreDefinitiaons.SrsWgs84Invflattening;
        }

        private double GetSemiMinor()
        {
            double invFlattening, semiMajor;

            semiMajor = GetSemiMajor();
            invFlattening = GetInvFlattening();

            if (Math.Abs(invFlattening) < 0.000000000001)
                return semiMajor;
            return semiMajor*(1.0 - 1.0/invFlattening);
        }

        

        private void SetProjection(string projection)
        {
            OgrsrsNode geogCsNode = null;
            if (Node != null && string.Equals(Node.Value, "GEOGCS", StringComparison.OrdinalIgnoreCase))
            {
                geogCsNode = Node;
            }
            if (GetAttrNode("PROJCS") == null)
                SetNode("PROJCS", "unnamed");
            SetNode("PROJCS|PROJECTION", projection);
            if (geogCsNode != null)
            {
                Node.ChildNodes.Insert(1, geogCsNode);
            }
        }

        private void SetProjParm(string parmName, double defaultValue)
        {
            var projCsNode = GetAttrNode("PROJCS");
            if (projCsNode == null)
            {
                throw new NullReferenceException("Could not find node PROJCS");
            }

            foreach (var childNode in projCsNode.ChildNodes)
            {
                if (string.Equals(childNode.Value, "PARAMETER", StringComparison.OrdinalIgnoreCase) &&
                    childNode.ChildNodes.Count == 2 &&
                    string.Equals(childNode.ChildNodes[0].Value, parmName, StringComparison.OrdinalIgnoreCase))
                {
                    childNode.ChildNodes[1].Value = defaultValue.ToString();
                    return;
                }
            }

            var newNode = new OgrsrsNode("PARAMETER");
            newNode.ChildNodes.Add(new OgrsrsNode(parmName));
            newNode.ChildNodes.Add(new OgrsrsNode(defaultValue.ToString()));

            projCsNode.ChildNodes.Add(newNode);
        }

        private int FindProjParm(string parameterName)
        {
            return FindProjParm(parameterName, null);
        }

        private int FindProjParm(string parameterName, OgrsrsNode projCSnode)
        {
            OgrsrsNode parameterNode;
            if (projCSnode == null)
                projCSnode = GetAttrNode("PROJCS");
            if (projCSnode == null)
                return -1;

            for (var i = 0; i < projCSnode.ChildNodes.Count; i++)
            {
                parameterNode = projCSnode.ChildNodes[i];
                if (string.Equals(parameterNode.Value, "PARAMETER", StringComparison.OrdinalIgnoreCase) &&
                    parameterNode.ChildNodes.Count == 2 &&
                    string.Equals(projCSnode.ChildNodes[i].ChildNodes[0].Value, parameterName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            var indexOfChild = -1;
            if (string.Equals(parameterName, OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin,
                StringComparison.OrdinalIgnoreCase))
                indexOfChild = FindProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, projCSnode);
            else if (string.Equals(parameterName, OgrCoreDefinitiaons.SrsPpCentralMeridian,
                StringComparison.OrdinalIgnoreCase))
            {
                indexOfChild = FindProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, projCSnode);
                if (indexOfChild == -1)
                {
                    indexOfChild = FindProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfOrigin, projCSnode);
                }
            }
            return indexOfChild;
        }

        private double GetProjParm(string parameterName, double defaultValue)
        {
            bool isSucceed;
            return GetProjParm(parameterName, defaultValue, out isSucceed);
        }

        private double GetProjParm(string parameterName, double defaultValue, out bool isSucceed)
        {
            isSucceed = false;
            var projCsNode = GetAttrNode("PROJCS");
            var indexOfChild = FindProjParm(parameterName, projCsNode);
            if (indexOfChild != -1)
            {
                isSucceed = true;
                return GetDoubleOutOfString(projCsNode.ChildNodes[indexOfChild].ChildNodes[1].Value);
            }
            return defaultValue;
        }

        private double GetNormProjParm(string parameterName, double defaultValue)
        {
            double rawResult;
            GetNormInfo();
            bool isSucceed;

            rawResult = GetProjParm(parameterName, defaultValue, out isSucceed);
            if (!isSucceed)
            {
                return rawResult;
            }

            if (_toDegrees != 1.0 && IsAngularParameter(parameterName))
                rawResult *= _toDegrees;
            if (_toMeter != 1.0 && IsLinearParameter(parameterName))
                rawResult *= _toMeter;

            

            return rawResult;
        }

        private void SetNormProjParm(string name, double value)
        {
            GetNormInfo();
            if ((_toDegrees != 1.0 || _fromGreenwich != 0.0) && IsAngularParameter(name))
            {
                value /= _toDegrees;
            }
            else if (_toMeter != 1.0 && IsLinearParameter(name))
                value /= _toMeter;

            SetProjParm(name, value);
        }

        private void SetTm(double dfCenterLat, double dfCenterLong, double dfScale, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtTransverseMercator);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }


        private void SetTmso(double dfCenterLat, double dfCenterLong, double dfScale, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtTransverseMercatorSouthOriented);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetTped(double dfLat1, double dfLong1, double dfLat2, double dfLong2, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtTwoPointEquidistant);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOf_1StPoint, dfLat1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOf_1StPoint, dfLong1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOf_2NdPoint, dfLat2);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOf_2NdPoint, dfLong2);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetTmg(double dfCenterLat, double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtTunisiaMiningGrid);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetAcea(double dfStdP1, double dfStdP2, double dfCenterLat, double dfCenterLong,
            double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtAlbersConicEqualArea);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, dfStdP1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel2, dfStdP2);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetAe(double dfCenterLat, double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtAzimuthalEquidistant);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetBonne(double stdP1, double centralMeridian, double falseEasting, double falseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtBonne);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, stdP1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, centralMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, falseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, falseNorthing);
        }

        private void SetCea(double dfStdP1, double dfCentralMeridian, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtCylindricalEqualArea);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, dfStdP1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCentralMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetCs(double centerLat, double centerLong, double falseEasting, double falseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtCassiniSoldner);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, centerLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, centerLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, falseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, falseNorthing);
        }

        private void SetEc(double dfStdP1, double dfStdP2, double dfCenterLat, double dfCenterLong,
            double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtEquidistantConic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, dfStdP1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel2, dfStdP2);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetEckert(int nVariation, double dfCentralMeridian, double dfFalseEasting, double dfFalseNorthing)
        {
            if (nVariation == 1)
                SetProjection(OgrCoreDefinitiaons.SrsPtEckertI);
            else if (nVariation == 2)
                SetProjection(OgrCoreDefinitiaons.SrsPtEckertIi);
            else if (nVariation == 3)
                SetProjection(OgrCoreDefinitiaons.SrsPtEckertIii);
            else if (nVariation == 4)
                SetProjection(OgrCoreDefinitiaons.SrsPtEckertIv);
            else if (nVariation == 5)
                SetProjection(OgrCoreDefinitiaons.SrsPtEckertV);
            else if (nVariation == 6)
                SetProjection(OgrCoreDefinitiaons.SrsPtEckertVi);
            else
                throw new NotSupportedException(
                    string.Format(
                        "The Format of Eckert is not supported, Variantion = {0}, CentralMeridian = {1}, FalseEasting ={2}, FasleNorthing = {3}",
                        nVariation, dfCentralMeridian, dfFalseEasting, dfFalseNorthing));

            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCentralMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetEquirectangular(double dfCenterLat, double dfCenterLong, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtEquirectangular);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetEquirectangular2(double dfCenterLat, double dfCenterLong, double dfStdParallel1,
            double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtEquirectangular);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, dfStdParallel1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetGs(double dfCentralMeridian, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtGallStereographic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCentralMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetGh(double dfCentralMeridian, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtGoodeHomolosine);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCentralMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetGeos(double dfCentralMeridian, double dfSatelliteHeight, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtGeostationarySatellite);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCentralMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpSatelliteHeight, dfSatelliteHeight);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetGaussSchreiberTMercator(double dfCenterLat, double dfCenterLong, double dfScale,
            double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtGaussschreibertmercator);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetGnomonic(double dfCenterLat, double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtGnomonic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetHom(double dfCenterLat, double dfCenterLong, double dfAzimuth, double dfRectToSkew,
            double dfScale, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtHotineObliqueMercator);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpAzimuth, dfAzimuth);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpRectifiedGridAngle, dfRectToSkew);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetIwmPolyconic(double dfLat1, double dfLat2, double dfCenterLong, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtImwPolyconic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOf_1StPoint, dfLat1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOf_2NdPoint, dfLat2);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetKrovak(double dfCenterLat, double dfCenterLong, double dfAzimuth, double dfPseudoStdParallel1,
            double dfScale, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtKrovak);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpAzimuth, dfAzimuth);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpPseudoStdParallel1, dfPseudoStdParallel1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetLaea(double dfCenterLat, double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtLambertAzimuthalEqualArea);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetLcc(double dfStdP1, double dfStdP2, double dfCenterLat, double dfCenterLong,
            double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtLambertConformalConic_2Sp);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, dfStdP1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel2, dfStdP2);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetLcc1Sp(double dfCenterLat, double dfCenterLong, double dfScale, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtLambertConformalConic_1Sp);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetLccb(double dfStdP1, double dfStdP2, double dfCenterLat, double dfCenterLong,
            double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtLambertConformalConic_2SpBelgium);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, dfStdP1);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel2, dfStdP2);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetMc(double dfCenterLat, double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtMillerCylindrical);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetMercator(double dfCenterLat, double dfCenterLong, double dfScale, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtMercator_1Sp);
            if (dfCenterLat != 0.0)
                SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetMercator2Sp(double dfStdP1, double dfCenterLat, double dfCenterLong, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtMercator_2Sp);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, dfStdP1);
            if (dfCenterLat != 0.0)
                SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetMollweide(double dfCentralMeridian, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtMollweide);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCentralMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetNzmg(double dfCenterLat, double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtNewZealandMapGrid);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetOs(double dfOriginLat, double dfCMeridian, double dfScale, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtObliqueStereographic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfOriginLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetOrthographic(double dfCenterLat, double dfCenterLong, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtOrthographic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetPolyconic(double dfCenterLat, double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtPolyconic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetPs(double dfCenterLat, double dfCenterLong, double dfScale, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtPolarStereographic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetRobinson(double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtRobinson);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetSinusoidal(double dfCenterLong, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtSinusoidal);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, dfCenterLong);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetStereographic(double dfOriginLat, double dfCMeridian, double dfScale, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtStereographic);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfOriginLat);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, dfScale);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetSoc(double dfLatitudeOfOrigin, double dfCentralMeridian, double dfFalseEasting,
            double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtSwissObliqueCylindrical);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, dfLatitudeOfOrigin);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCentralMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetVdg(double dfCMeridian, double dfFalseEasting, double dfFalseNorthing)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtVandergrinten);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, dfCMeridian);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        private void SetUtm(int nZone, int isNorth)
        {
            SetProjection(OgrCoreDefinitiaons.SrsPtTransverseMercator);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, nZone*6 - 183);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 0.9996);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 500000.0);
            var iszoneNorth = (isNorth > 0);
            if (iszoneNorth)
                SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0);
            else
                SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 10000000);

            if (string.Equals(GetAttrValue("PROJCS"), "unnamed"))
            {
                string utmName;

                if (iszoneNorth)
                    utmName = string.Format("UTM Zone {0}, Northern Hemisphere", nZone);
                else
                    utmName = string.Format("UTM Zone {0}, Southern Hemisphere", nZone);

                SetNode("PROJCS", utmName);
            }

            SetLinearUnits(OgrCoreDefinitiaons.SrsUlMeter, 1.0);
        }

        private int GetUtmZone(out bool isNorth)
        {
            isNorth = false;
            var projectionName = GetAttrValue("PROJECTION");
            if (string.IsNullOrEmpty(projectionName) ||
                !string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtTransverseMercator,
                    StringComparison.OrdinalIgnoreCase))
                return 0;
            if (GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0) != 0.0)
                return 0;
            if (GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 1.0) != 0.9996)
                return 0;
            if (Math.Abs(GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0) - 500000.0) > 0.001)
                return 0;

            var falseNothing = GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0);
            if (falseNothing != 0.0 && Math.Abs(falseNothing - 10000000.0) > 0.001)
                return 0;

            isNorth = falseNothing == 0;

            var centerlMeridian = GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0);
            var zone = (centerlMeridian + 183)/6.0 + 0.000000001;

            if (Math.Abs(zone - (int) zone) > 0.00001 || centerlMeridian < -177.00001 || centerlMeridian > 177.00001)
                return 0;
            return (int) zone;
        }

        private void SetWagner(int nVariation, double dfCenterLat, double dfFalseEasting, double dfFalseNorthing)
        {
            if (nVariation == 1)
                SetProjection(OgrCoreDefinitiaons.SrsPtWagnerI);
            else if (nVariation == 2)
                SetProjection(OgrCoreDefinitiaons.SrsPtWagnerIi);
            else if (nVariation == 3)
            {
                SetProjection(OgrCoreDefinitiaons.SrsPtWagnerIii);
                SetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, dfCenterLat);
            }
            else if (nVariation == 4)
                SetProjection(OgrCoreDefinitiaons.SrsPtWagnerIv);
            else if (nVariation == 5)
                SetProjection(OgrCoreDefinitiaons.SrsPtWagnerV);
            else if (nVariation == 6)
                SetProjection(OgrCoreDefinitiaons.SrsPtWagnerVi);
            else if (nVariation == 7)
                SetProjection(OgrCoreDefinitiaons.SrsPtWagnerVii);
            else
                throw new NotSupportedException(string.Format("Unsupported Wagner variation {0}", nVariation));

            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, dfFalseEasting);
            SetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, dfFalseNorthing);
        }

        internal void SetAuthority(string targetKey, string authority, int code)
        {
            var targetNode = GetAttrNode(targetKey);
            if (targetNode == null)
                throw new NullReferenceException(string.Format("Cannot find Authority node under the path: {0}",
                    targetNode));

            for (var i = 0; i < targetNode.ChildNodes.Count; i++)
            {
                if (targetNode.ChildNodes[i].Value == "AUTHORITY")
                {
                    targetNode.ChildNodes.RemoveAt(i);
                    break;
                }
            }
            var authNode = new OgrsrsNode("AUTHORITY");
            authNode.ChildNodes.Add(new OgrsrsNode(authority));
            authNode.ChildNodes.Add(new OgrsrsNode(code.ToString()));
        }

        private string GetAuthorityCode(string targetKey)
        {
            var targetNode = string.IsNullOrEmpty(targetKey) ? Node : GetAttrNode(targetKey);
            if (targetNode == null)
                return null;

            var nodeIndex = targetNode.FindChild(targetKey);
            if (nodeIndex == -1)
                return null;
            targetNode = targetNode.ChildNodes[nodeIndex];

            if (targetNode.ChildNodes.Count < 2)
                return null;
            return targetNode.ChildNodes[1].Value;
        }

        private string GetAuthorityName(string targetKey)
        {
            var targetNode = string.IsNullOrEmpty(targetKey) ? Node : GetAttrNode(targetKey);
            if (targetNode == null)
                return null;

            var nodeIndex = targetNode.FindChild(targetKey);
            if (nodeIndex == -1)
                return null;
            targetNode = targetNode.ChildNodes[nodeIndex];

            if (targetNode.ChildNodes.Count < 2)
                return null;
            return targetNode.ChildNodes[0].Value;
        }

        private bool IsProjected()
        {
            if (Node == null)
                return false;
            if (string.Equals(Node.Value, "PROJCS", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(Node.Value, "COMPD_CS", StringComparison.OrdinalIgnoreCase))
                return GetAttrNode("PROJCS") == null;
            return false;
        }

        private bool IsGeographic()
        {
            if (Node.Value == "GEOGCS")
                return true;
            if (Node.Value == "COMPD_CS")
                return GetAttrNode("GEOGCS") != null && GetAttrNode("PROJCS") == null;
            return false;
        }

        private bool IsLocal()
        {
            if (Node == null)
                return false;
            return string.Equals(Node.Value, "LOCAL_CS", StringComparison.OrdinalIgnoreCase);
        }


        private void SetTOWGS84(double x, double y, double z)
        {
            SetTOWGS84(x, y, z, 0, 0, 0, 0);
        }

        private void SetTOWGS84(double x, double y, double z, double eX, double eY, double eZ, double ppm)
        {
            OgrsrsNode datumNode, toWgs84Node;
            int position;
            datumNode = GetAttrNode("DATUM");
            if (datumNode == null)
                throw new NullReferenceException("Cannot find DATUM Node in current Node");

            if (datumNode.FindChild("TOWGS84") != -1)
                datumNode.ChildNodes.RemoveAt(datumNode.FindChild("TOWGS84"));

            position = datumNode.ChildNodes.Count;
            if (datumNode.FindChild("AUTHORITY") != -1)
            {
                position = datumNode.FindChild("AUTHORITY");
            }

            toWgs84Node = new OgrsrsNode("TOWGS84");
            toWgs84Node.ChildNodes.Add(new OgrsrsNode(x.ToString()));
            toWgs84Node.ChildNodes.Add(new OgrsrsNode(y.ToString()));
            toWgs84Node.ChildNodes.Add(new OgrsrsNode(z.ToString()));
            toWgs84Node.ChildNodes.Add(new OgrsrsNode(eX.ToString()));
            toWgs84Node.ChildNodes.Add(new OgrsrsNode(eY.ToString()));
            toWgs84Node.ChildNodes.Add(new OgrsrsNode(eZ.ToString()));
            toWgs84Node.ChildNodes.Add(new OgrsrsNode(ppm.ToString()));
            datumNode.ChildNodes.Insert(position, toWgs84Node);
        }

        private bool IsAngularParameter(string parameterName)
        {
            if (parameterName.StartsWith("long", StringComparison.OrdinalIgnoreCase)
                || parameterName.StartsWith("lati", StringComparison.OrdinalIgnoreCase)
                || parameterName.StartsWith("standard_parallel", StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(parameterName, OgrCoreDefinitiaons.SrsPpCentralMeridian,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(parameterName, OgrCoreDefinitiaons.SrsPpAzimuth, StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(parameterName, OgrCoreDefinitiaons.SrsPpRectifiedGridAngle,
                    StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private static bool IsLinearParameter(string parameterName)
        {
            if (parameterName.StartsWith("false_", StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(parameterName, OgrCoreDefinitiaons.SrsPpSatelliteHeight,
                    StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private void GetNormInfo()
        {
            if (_isNormInfoSet)
                return;
            _isNormInfoSet = true;

            string parameterName = null;
            _fromGreenwich = GetPrimeMeridian(ref parameterName);
            _toMeter = GetLinearUnits(ref parameterName);
            _toDegrees = GetAngularUnits(ref parameterName)/GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUaDegreeConv);

            if (Math.Abs(_toDegrees - 1.0d) < 0.000000001)
                _toDegrees = 1.0;
        }

        private void FixupOrdering()
        {
            if (Node != null)
                Node.FixupOrdering();
            else
                throw new NullReferenceException("Node is not initialized, please check.");
        }

        private string GetExtension(string targetKey, string extensionName, string defaultValue)
        {
            OgrsrsNode targetNode;
            if (string.IsNullOrEmpty(targetKey))
                targetNode = Node;
            else
                targetNode = GetAttrNode(targetKey);

            if (targetNode == null)
                return null;

            for (var i = targetNode.ChildNodes.Count - 1; i >= 0; i--)
            {
                var childNode = targetNode.ChildNodes[i];
                if (string.Equals(childNode.Value, "EXTENSION", StringComparison.OrdinalIgnoreCase) &&
                    childNode.ChildNodes.Count >= 2)
                {
                    if (string.Equals(childNode.ChildNodes[0].Value, extensionName))
                        return childNode.ChildNodes[1].Value;
                }
            }
            return defaultValue;
        }

        private void SetExtension(string targetKey, string name, string value)
        {
            OgrsrsNode targetNode;
            if (string.IsNullOrEmpty(targetKey))
                targetNode = Node;
            else if ((targetNode = GetAttrNode(targetKey)) == null)
                throw new NullReferenceException(string.Format("Cannot find node under the path {0}", targetKey));
            for (var i = targetNode.ChildNodes.Count - 1; i >= 0; i--)
            {
                var child = targetNode.ChildNodes[i];
                if (string.Equals(child.Value, "EXTENSION", StringComparison.OrdinalIgnoreCase) &&
                    child.ChildNodes.Count >= 2)
                {
                    if (string.Equals(child.ChildNodes[0].Value, name, StringComparison.OrdinalIgnoreCase))
                    {
                        child.ChildNodes[1].Value = value;
                        return;
                    }
                }
            }

            var authNode = new OgrsrsNode("EXTENSION");
            authNode.ChildNodes.Add(new OgrsrsNode(name));
            authNode.ChildNodes.Add(new OgrsrsNode(value));

            targetNode.ChildNodes.Add(authNode);
        }


        internal void SetAxes(string targetKey, string xAxisName, OgrAxisOrientation xAxisOrientation, string yAxisName,
            OgrAxisOrientation yAxisOrientation)
        {
            OgrsrsNode targetNode;
            if (string.IsNullOrEmpty(targetKey))
                targetNode = Node;
            else
                targetNode = GetAttrNode(targetKey);
            if (targetNode == null)
                throw new NullReferenceException(string.Format("Cannot find not under the path :{0}", targetKey));

            int nIndex;
            while ((nIndex = targetNode.FindChild("AXIS")) >= 0)
            {
                targetNode.ChildNodes.RemoveAt(nIndex);
            }

            var axisNode = new OgrsrsNode("AXIS");
            axisNode.ChildNodes.Add(new OgrsrsNode(xAxisName));
            axisNode.ChildNodes.Add(new OgrsrsNode(xAxisOrientation.ToString()));
            targetNode.ChildNodes.Add(axisNode);

            axisNode = new OgrsrsNode("AXIS");
            axisNode.ChildNodes.Add(new OgrsrsNode(yAxisName));
            axisNode.ChildNodes.Add(new OgrsrsNode(yAxisOrientation.ToString()));
            targetNode.ChildNodes.Add(axisNode);
        }

        private void Fixup()
        {
            var csNode = GetAttrNode("PROJCS");
            if (csNode == null)
            {
                csNode = GetAttrNode("LOCAL_CS");
            }
            if (csNode != null && csNode.FindChild("UNIT") == -1)
            {
                SetLinearUnits(OgrCoreDefinitiaons.SrsUlMeter, 1.0);
            }

            csNode = GetAttrNode("GEOGCS");
            if (csNode != null && csNode.FindChild("UNIT") == -1)
            {
                SetAngularUnits(OgrCoreDefinitiaons.SrsUaDegree,
                    GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUaDegreeConv));
            }
            FixupOrdering();
        }

        private void StripCTParms()
        {
            StripCTParms(null);
        }

        private void StripCTParms(OgrsrsNode currentNode)
        {
            if (currentNode == null)
            {
                StripVertical();
                currentNode = Node;
            }
            if (currentNode == null)
                return;
            if (currentNode == Node && string.Equals(currentNode.Value, "LOCAL_CS", StringComparison.OrdinalIgnoreCase))
            {
                Node = null;
                return;
            }

            currentNode.StripNodes("AUTHORITY");
            currentNode.StripNodes("TOWGS84");
            currentNode.StripNodes("AXIS");
            currentNode.StripNodes("EXTENSION");
        }

        private void StripVertical()
        {
            if (Node == null || !string.Equals(Node.Value, "COMPD_CS", StringComparison.OrdinalIgnoreCase))
                return;

            var horizontalCsNode = Node.ChildNodes[1];
            if (horizontalCsNode != null)
                horizontalCsNode = horizontalCsNode.Clone();
            Node = horizontalCsNode;
        }

       

        private static double GetDoubleOutOfString(string valueString)
        {
            var returnValue = double.MaxValue;
            if (double.TryParse(valueString, out returnValue))
                return returnValue;
            return 0.0;
        }

        private static int GetIntOutOfString(string valueString)
        {
            var returnValue = int.MaxValue;
            if (int.TryParse(valueString, out returnValue))
                return returnValue;
            return 0;
        }

       
    }
}
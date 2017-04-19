using System;
using System.Globalization;
using System.IO;

namespace Mapgenix.Canvas
{
    internal partial class OgrSpatialReference
    {
        private const double Pi = 3.14159265358979323846;
        private const int NatOriginLat = 8801;
        private const int NatOriginScaleFactor = 8805;
        private const int AngleRectifiedToSkewedGrid = 8814;
        private const int InitialLineScaleFactor = 8815;
        private const int PseudoStdParallelScaleFactor = 8819;
        private const int NatOriginLong = 8802;
        private const int FalseEasting = 8806;
        private const int FalseNorthing = 8807;
        private const int ProjCenterLat = 8811;
        private const int ProjCenterLong = 8812;
        private const int Azimuth = 8813;
        private const int ProjCenterEasting = 8816;
        private const int ProjCenterNorthing = 8817;
        private const int PseudoStdParallelLat = 8818;
        private const int FalseOriginLat = 8821;
        private const int FalseOriginLong = 8822;
        private const int StdParallel1Lat = 8823;
        private const int StdParallel2Lat = 8824;
        private const int FalseOriginEasting = 8826;
        private const int FalseOriginNorthing = 8827;
        private const int SphericalOriginLat = 8828;
        private const int SphericalOriginLong = 8829;
        private const int PolarLatStdParallel = 8832;
        private const int PolarLongOrigin = 8833;

        private static readonly string[] DatumEquiv =
        {
            "Militar_Geographische_Institut",
            "Militar_Geographische_Institute",
            "World_Geodetic_System_1984",
            "WGS_1984",
            "WGS_72_Transit_Broadcast_Ephemeris",
            "WGS_1972_Transit_Broadcast_Ephemeris",
            "World_Geodetic_System_1972",
            "WGS_1972",
            "European_Terrestrial_Reference_System_89",
            "European_Reference_System_1989",
            null
        };

        private static void OgrepsgDatumNameMassage(ref string datum)
        {
            if (string.IsNullOrEmpty(datum))
                return;

            var datumCharArray = datum.ToCharArray();
            for (var i = 0; i < datumCharArray.Length; i++)
            {
                if (!char.IsLetterOrDigit(datumCharArray[i]))
                {
                    datumCharArray[i] = '_';
                }
            }

            datum = new string(datumCharArray);
            datum = datum.Replace("__", "_");
            datum = datum.Trim('_');

            for (var i = 0; DatumEquiv[i] != null; i += 2)
            {
                if (string.Equals(datum, DatumEquiv[i]))
                {
                    datum = DatumEquiv[i + 1];
                    break;
                }
            }
        }

        private static double EpsgAngleStringToDd(string angleValue, int uomAngle)
        {
            double dAngle;
            if (uomAngle == 9110) 
            {
                var decimalIndex = angleValue.IndexOf('.');
                dAngle = (int) Math.Abs(GetDoubleOutOfString(angleValue));
                if (decimalIndex > 0 && angleValue.Substring(decimalIndex).Length > 1)
                {
                    var angleValueArray = angleValue.ToCharArray();
                    string seconds;
                    var minutes = string.Format("{0}{1}", angleValueArray[decimalIndex + 1],
                        (decimalIndex + 2 < angleValueArray.Length && angleValueArray[decimalIndex + 2] >= '0' &&
                         angleValueArray[decimalIndex + 2] <= '9')
                            ? angleValueArray[decimalIndex + 2]
                            : '0');
                    dAngle += GetIntOutOfString(minutes)/60.0;

                    if (angleValue.Substring(decimalIndex).Length > 3)
                    {
                        seconds = angleValue[decimalIndex + 3].ToString();
                        if (decimalIndex + 4 < angleValueArray.Length && angleValueArray[decimalIndex + 4] >= '0' &&
                            angleValueArray[decimalIndex + 4] <= '9')
                        {
                            seconds += angleValueArray[decimalIndex + 4] + ".";
                            seconds += angleValue.Substring(decimalIndex + 5);
                        }
                        else
                        {
                            seconds += "0";
                        }
                        dAngle += GetDoubleOutOfString(seconds)/3600;
                    }
                }
                if (angleValue.StartsWith("-"))
                    dAngle *= -1;
            }
            else if (uomAngle == 9105 || uomAngle == 9106) 
            {
                dAngle = 180*(GetDoubleOutOfString(angleValue)/200);
            }
            else if (uomAngle == 9101) 
            {
                dAngle = 180*(GetDoubleOutOfString(angleValue)/Pi);
            }
            else if (uomAngle == 9103) 
            {
                dAngle = GetDoubleOutOfString(angleValue)/60;
            }
            else if (uomAngle == 9104) 
            {
                dAngle = GetDoubleOutOfString(angleValue)/3600;
            }
            else
            {
                dAngle = GetDoubleOutOfString(angleValue);
            }
            return dAngle;
        }

        private static bool EpsgGetUomAngleInfo(int nUomAngleCode, ref string ppszUomName, ref double pdfInDegrees)
        {
            var inDegrees = 1.0;
            var fileName = CsvFileHelper.GetCsvFilename("unit_of_measure.csv");
            if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, fileName)))
            {
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot find {0} under GSuitePrjHelper folder.", fileName));
            }
            var searcheKey = nUomAngleCode.ToString();

            var uomName = CsvFileHelper.CsvGetField(fileName, "UOM_CODE", searcheKey, CsvCompareCriteria.Integer,
                "UNIT_OF_MEAS_NAME");
            if (!string.IsNullOrEmpty(uomName))
            {
                var factorB =
                    GetDoubleOutOfString(CsvFileHelper.CsvGetField(fileName, "UOM_CODE", searcheKey,
                        CsvCompareCriteria.Integer, "FACTOR_B"));
                var factorC =
                    GetDoubleOutOfString(CsvFileHelper.CsvGetField(fileName, "UOM_CODE", searcheKey,
                        CsvCompareCriteria.Integer, "FACTOR_C"));

                if (factorC != 0.0)
                    inDegrees = (factorB/factorC)*(180/Pi);
                if (nUomAngleCode == 9102 || nUomAngleCode == 9107 || nUomAngleCode == 9108 || nUomAngleCode == 9110 ||
                    nUomAngleCode == 9122)
                {
                    uomName = "degree";
                }
                if (nUomAngleCode == 9015)
                    inDegrees = 180.0/200.0;
            }
            else
            {
                switch (nUomAngleCode)
                {
                    case 9101:
                        uomName = "radian";
                        inDegrees = 180.0/Pi;
                        break;

                    case 9102:
                    case 9107:
                    case 9108:
                    case 9110:
                    case 9122:
                        uomName = "degree";
                        inDegrees = 1.0;
                        break;

                    case 9103:
                        uomName = "arc-minute";
                        inDegrees = 1/60.0;
                        break;

                    case 9104:
                        uomName = "arc-second";
                        inDegrees = 1/3600.0;
                        break;

                    case 9105:
                        uomName = "grad";
                        inDegrees = 180.0/200.0;
                        break;

                    case 9106:
                        uomName = "gon";
                        inDegrees = 180.0/200.0;
                        break;

                    case 9109:
                        uomName = "microradian";
                        inDegrees = 180.0/(3.14159265358979*1000000.0);
                        break;

                    default:
                        return false;
                }
            }

            if (ppszUomName != null)
                ppszUomName = uomName;
            if (pdfInDegrees != -1)
                pdfInDegrees = inDegrees;

            return true;
        }

        private static bool EpsgGetUomLengthInfo(int uomLengthCode, ref string uomName, ref double inMeters)
        {
            var searchKey = uomLengthCode.ToString();
            var uomFileName = CsvFileHelper.GetCsvFilename("unit_of_measure.csv");
            if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, uomFileName)))
            {
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot find {0} under GSuitePrjHelper folder.", uomFileName));
            }
            var iNameFiled = -1;
            if (uomLengthCode == 9001)
            {
                if (uomName != null)
                    uomName = "metre";
                if (inMeters != -1)
                    inMeters = 1.0;
                return true;
            }

            var unitsRecord = CsvFileHelper.CsvScanFileByName(uomFileName, "UOM_CODE", searchKey,
                CsvCompareCriteria.Integer);
            if (unitsRecord == null)
                return false;

            if (uomName != null)
            {
                iNameFiled = CsvFileHelper.CsvGetFileFieldId(uomFileName, "UNIT_OF_MEAS_NAME");
                uomName = unitsRecord[iNameFiled];
            }

            if (inMeters != -1)
            {
                var bFactorField = CsvFileHelper.CsvGetFileFieldId(uomFileName, "FACTOR_B");
                var cFactorField = CsvFileHelper.CsvGetFileFieldId(uomFileName, "FACTOR_C");

                if (GetDoubleOutOfString(unitsRecord[cFactorField]) > 0)
                    inMeters = GetDoubleOutOfString(unitsRecord[bFactorField])/
                               GetDoubleOutOfString(unitsRecord[cFactorField]);
                else
                    inMeters = 0;
            }

            return true;
        }

        internal static bool EpsgGetWgs84Transform(int geogCs, double[] padTransform)
        {
            int methodCode, indexDxField, iField;
            string filename;
            string code;
            string[] records = null;

            filename = CsvFileHelper.GetCsvFilename("gcs.override.csv");
            code = geogCs.ToString();
            records = CsvFileHelper.CsvScanFileByName(filename, "COORD_REF_SYS_CODE", code, CsvCompareCriteria.Integer);
            if (records == null)
            {
                filename = CsvFileHelper.GetCsvFilename("gcs.csv");
                if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, filename)))
                    throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                        "Cannot find {0} under GSuitePrjHelper folder.", filename));
                records = CsvFileHelper.CsvScanFileByName(filename, "COORD_REF_SYS_CODE", code,
                    CsvCompareCriteria.Integer);
            }

            if (records == null)
                return false;

            methodCode = GetIntOutOfString(records[CsvFileHelper.CsvGetFileFieldId(filename, "COORD_OP_METHOD_CODE")]);
            if (methodCode != 9603 && methodCode != 9607 && methodCode != 9606)
                return false;

            indexDxField = CsvFileHelper.CsvGetFileFieldId(filename, "DX");

            for (iField = 0; iField < 7; iField++)
                padTransform[iField] = GetDoubleOutOfString(records[indexDxField + iField]);

            if (methodCode == 9607)
            {
                padTransform[3] *= -1;
                padTransform[4] *= -1;
                padTransform[5] *= -1;
            }
            return true;
        }

        private static bool EpsgGetPmInfo(int pmCode, ref string name, ref double offset)
        {
            int uomAngle;
            var searchKey = pmCode.ToString();
            var pmFileName = CsvFileHelper.GetCsvFilename("prime_meridian.csv");
            if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, pmFileName)))
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot find {0} under GSuitePrjHelper folder.", pmFileName));
            if (pmCode == 7022)
            {
                if (offset == double.MinValue)
                    offset = 0.0;
                if (string.IsNullOrEmpty(name))
                    name = "Greenwich";
                return true;
            }
            if (
                (uomAngle =
                    GetIntOutOfString(CsvFileHelper.CsvGetField(pmFileName, "PRIME_MERIDIAN_CODE", searchKey,
                        CsvCompareCriteria.Integer, "UOM_CODE"))) < 1)
                return false;

            if (offset != double.MinValue)
            {
                offset =
                    EpsgAngleStringToDd(
                        CsvFileHelper.CsvGetField(pmFileName, "PRIME_MERIDIAN_CODE", searchKey,
                            CsvCompareCriteria.Integer, "GREENWICH_LONGITUDE"), uomAngle);
            }
            if (name != null)
                name = CsvFileHelper.CsvGetField(pmFileName, "PRIME_MERIDIAN_CODE", searchKey,
                    CsvCompareCriteria.Integer, "PRIME_MERIDIAN_NAME");
            return true;
        }

       

        internal static bool EpsgGetGcsInfo(int code, ref string refName, ref int refDatum, ref string refDatumName,
            ref int refPm, ref int refEllipsoid, ref int refUomAngle, ref int refCoordSysCode)
        {
            var isGcsCode = false;
            int datum, pm, ellipsoid, uomAngle;

            var fileName = CsvFileHelper.GetCsvFilename("gcs.override.csv");
            var searchKey = code.ToString();
            datum =
                GetIntOutOfString(CsvFileHelper.CsvGetField(fileName, "COORD_REF_SYS_CODE", searchKey,
                    CsvCompareCriteria.Integer, "DATUM_CODE"));
            if (datum < 1)
            {
                fileName = CsvFileHelper.GetCsvFilename("gcs.csv");
                if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, fileName)))
                    throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                        "Cannot find {0} under GSuitePrjHelper folder.", fileName));
                datum =
                    GetIntOutOfString(CsvFileHelper.CsvGetField(fileName, "COORD_REF_SYS_CODE", searchKey,
                        CsvCompareCriteria.Integer, "DATUM_CODE"));
            }
            if (datum < 1)
                return isGcsCode;

            if (refDatum != int.MinValue)
                refDatum = datum;

            if (
                (pm =
                    GetIntOutOfString(CsvFileHelper.CsvGetField(fileName, "COORD_REF_SYS_CODE", searchKey,
                        CsvCompareCriteria.Integer, "PRIME_MERIDIAN_CODE"))) < 1)
                return isGcsCode;

            if (refPm != int.MinValue)
                refPm = pm;

            if (
                (ellipsoid =
                    GetIntOutOfString(CsvFileHelper.CsvGetField(fileName, "COORD_REF_SYS_CODE", searchKey,
                        CsvCompareCriteria.Integer, "ELLIPSOID_CODE"))) < 1)
                return isGcsCode;

            if (refEllipsoid != int.MinValue)
                refEllipsoid = ellipsoid;

            if (
                (uomAngle =
                    GetIntOutOfString(CsvFileHelper.CsvGetField(fileName, "COORD_REF_SYS_CODE", searchKey,
                        CsvCompareCriteria.Integer, "UOM_CODE"))) < 1)
                return isGcsCode;

            if (refUomAngle != int.MinValue)
                refUomAngle = uomAngle;

            isGcsCode = true;

            if (refName != null)
                refName = CsvFileHelper.CsvGetField(fileName, "COORD_REF_SYS_CODE", searchKey,
                    CsvCompareCriteria.Integer, "COORD_REF_SYS_NAME");

            if (refDatumName != null)
                refDatumName = CsvFileHelper.CsvGetField(fileName, "COORD_REF_SYS_CODE", searchKey,
                    CsvCompareCriteria.Integer, "DATUM_NAME");

            if (refCoordSysCode != int.MinValue)
                refCoordSysCode =
                    GetIntOutOfString(CsvFileHelper.CsvGetField(fileName, "COORD_REF_SYS_CODE", searchKey,
                        CsvCompareCriteria.Integer, "COORD_SYS_CODE"));

            return isGcsCode;
        }

        private static void OsrGetEllipsoidInfo(int code, ref string ellipseName, ref double semiMajor,
            ref double invFlattening)
        {
            var searchKey = code.ToString();
            double dfSemiMajor, dfToMeters = 1.0;
            int uomLength;

            if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, "ellipsoid.csv")))
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot find {0} under GSuitePrjHelper folder.", "ellipsoid.csv"));

            dfSemiMajor =
                GetDoubleOutOfString(CsvFileHelper.CsvGetField(CsvFileHelper.GetCsvFilename("ellipsoid.csv"),
                    "ELLIPSOID_CODE", searchKey, CsvCompareCriteria.Integer, "SEMI_MAJOR_AXIS"));
            if (dfSemiMajor == 0.0)
                throw new NotSupportedException(string.Format("Error: Unsupported SRS, ELLIPSOID_CODE: {0}", searchKey));

            uomLength =
                GetIntOutOfString(CsvFileHelper.CsvGetField(CsvFileHelper.GetCsvFilename("ellipsoid.csv"),
                    "ELLIPSOID_CODE", searchKey, CsvCompareCriteria.Integer, "UOM_CODE"));
            string uomName = null;
            EpsgGetUomLengthInfo(uomLength, ref uomName, ref dfToMeters);

            dfSemiMajor *= dfToMeters;

            if (semiMajor != double.MinValue)
            {
                semiMajor = dfSemiMajor;
            }

            if (invFlattening != double.MinValue)
            {
                invFlattening =
                    GetDoubleOutOfString(CsvFileHelper.CsvGetField(CsvFileHelper.GetCsvFilename("ellipsoid.csv"),
                        "ELLIPSOID_CODE", searchKey, CsvCompareCriteria.Integer, "INV_FLATTENING"));

                if (invFlattening == 0.0)
                {
                    double dfSemiMinor;

                    dfSemiMinor =
                        GetDoubleOutOfString(CsvFileHelper.CsvGetField(CsvFileHelper.GetCsvFilename("ellipsoid.csv"),
                            "ELLIPSOID_CODE", searchKey, CsvCompareCriteria.Integer, "SEMI_MINOR_AXIS"))*dfToMeters;

                    if (dfSemiMajor != 0.0 && dfSemiMajor != dfSemiMinor)
                        invFlattening = -1.0/(dfSemiMinor/dfSemiMajor - 1.0);
                    else
                        invFlattening = 0.0;
                }
            }

            if (ellipseName != null)
            {
                ellipseName = CsvFileHelper.CsvGetField(CsvFileHelper.GetCsvFilename("ellipsoid.csv"), "ELLIPSOID_CODE",
                    searchKey, CsvCompareCriteria.Integer, "ELLIPSOID_NAME");
            }
        }

        private static bool EpsgGetProjTrfInfo(int pcsCode, ref int projMethod, int[] parmIds, double[] projParms)
        {
            int nProjMethod, i;
            var adfProjParms = new double[7];
            var trfCode = string.Empty;
            var filename = string.Empty;

            filename = CsvFileHelper.GetCsvFilename("pcs.override.csv");
            trfCode = pcsCode.ToString();
            nProjMethod =
                GetIntOutOfString(CsvFileHelper.CsvGetField(filename, "COORD_REF_SYS_CODE", trfCode,
                    CsvCompareCriteria.Integer, "COORD_OP_METHOD_CODE"));
            if (nProjMethod == 0)
            {
                filename = CsvFileHelper.GetCsvFilename("pcs.csv");

                if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, filename)))
                    throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                        "Cannot find {0} under GSuitePrjHelper folder.", filename));

                trfCode = pcsCode.ToString();
                nProjMethod =
                    GetIntOutOfString(CsvFileHelper.CsvGetField(filename, "COORD_REF_SYS_CODE", trfCode,
                        CsvCompareCriteria.Integer, "COORD_OP_METHOD_CODE"));
            }

            if (nProjMethod == 0)
                return false;

            for (i = 0; i < 7; i++)
            {
                string paramUomid, paramValueId, paramCodeId;
                string value;
                int uom;

                paramCodeId = string.Format("PARAMETER_CODE_{0}", (i + 1));
                paramUomid = string.Format("PARAMETER_UOM_{0}", (i + 1));
                paramValueId = string.Format("PARAMETER_VALUE_{0}", (i + 1));

                if (parmIds != null)
                    parmIds[i] =
                        GetIntOutOfString(CsvFileHelper.CsvGetField(filename, "COORD_REF_SYS_CODE", trfCode,
                            CsvCompareCriteria.Integer, paramCodeId));

                uom =
                    GetIntOutOfString(CsvFileHelper.CsvGetField(filename, "COORD_REF_SYS_CODE", trfCode,
                        CsvCompareCriteria.Integer, paramUomid));
                value = CsvFileHelper.CsvGetField(filename, "COORD_REF_SYS_CODE", trfCode, CsvCompareCriteria.Integer,
                    paramValueId);

                if ((parmIds[i] == NatOriginScaleFactor
                     || parmIds[i] == InitialLineScaleFactor
                     || parmIds[i] == PseudoStdParallelScaleFactor)
                    && uom < 9200)
                    uom = 9201;

                if (uom >= 9100 && uom < 9200)
                    adfProjParms[i] = EpsgAngleStringToDd(value, uom);
                else if (uom > 9000 && uom < 9100)
                {
                    var dfInMeters = 0.0;
                    string uomName = null;
                    if (!EpsgGetUomLengthInfo(uom, ref uomName, ref dfInMeters))
                        dfInMeters = 1.0;
                    adfProjParms[i] = GetDoubleOutOfString(value)*dfInMeters;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    adfProjParms[i] = 0.0;
                }
                else
                {
                    if (uom != 9201)
                    {
                    }
                    adfProjParms[i] = GetDoubleOutOfString(value);
                }
            }

            if (projMethod != int.MinValue)
                projMethod = nProjMethod;

            if (projParms != null)
            {
                for (i = 0; i < 7; i++)
                    projParms[i] = adfProjParms[i];
            }

            return true;
        }

        internal static bool EpsgGetPcsInfo(int pcsCode, ref string epsgName, ref int uomLengthCode,
            ref int uomAngleCode, ref int geogCs, ref int pnTrfCode, ref int pnCoordSysCode)
        {
            string[] record = null;
            var searchKey = string.Empty;
            string filename;

            filename = CsvFileHelper.GetCsvFilename("pcs.csv");
            searchKey = pcsCode.ToString();
            record = CsvFileHelper.CsvScanFileByName(filename, "COORD_REF_SYS_CODE", searchKey,
                CsvCompareCriteria.Integer);

            if (record == null)
            {
                filename = CsvFileHelper.GetCsvFilename("pcs.override.csv");

                if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, filename)))
                    throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                        "Cannot find {0} under GSuitePrjHelper folder.", filename));

                record = CsvFileHelper.CsvScanFileByName(filename, "COORD_REF_SYS_CODE", searchKey,
                    CsvCompareCriteria.Integer);
            }

            if (record == null)
                return false;

            if (epsgName != null)
            {
                var osPcsName = record[CsvFileHelper.CsvGetFileFieldId(filename, "COORD_REF_SYS_NAME")];

                var pszDeprecated = record[CsvFileHelper.CsvGetFileFieldId(filename, "DEPRECATED")];

                if (pszDeprecated != null && pszDeprecated[0] == '1')
                    osPcsName += " (deprecated)";

                epsgName = osPcsName;
            }
            var dataItemIndex = -1;
            if (uomLengthCode != int.MinValue)
            {
                dataItemIndex = CsvFileHelper.CsvGetFileFieldId(filename, "UOM_CODE");
                if (dataItemIndex >= 0)
                    uomLengthCode = GetIntOutOfString(record[dataItemIndex]);
                else
                    uomLengthCode = 0;
            }

            if (uomAngleCode != int.MinValue)
            {
                dataItemIndex = CsvFileHelper.CsvGetFileFieldId(filename, "UOM_ANGLE_CODE");
                if (dataItemIndex >= 0)
                    uomAngleCode = GetIntOutOfString(record[dataItemIndex]);
                else
                    uomAngleCode = 0;
            }

            if (geogCs != int.MinValue)
            {
                dataItemIndex = CsvFileHelper.CsvGetFileFieldId(filename, "SOURCE_GEOGCRS_CODE");
                if (dataItemIndex >= 0)
                    geogCs = GetIntOutOfString(record[dataItemIndex]);
                else
                    geogCs = 0;
            }

            if (pnTrfCode != int.MinValue)
            {
                dataItemIndex = CsvFileHelper.CsvGetFileFieldId(filename, "COORD_OP_CODE");
                if (dataItemIndex >= 0)
                    pnTrfCode = GetIntOutOfString(record[dataItemIndex]);
                else
                    pnTrfCode = 0;
            }

            var nCsc =
                GetIntOutOfString(CsvFileHelper.CsvGetField(filename, "COORD_REF_SYS_CODE", searchKey,
                    CsvCompareCriteria.Integer, "COORD_SYS_CODE"));

            if (pnCoordSysCode != int.MinValue)
                pnCoordSysCode = nCsc;

            return true;
        }

        private static void SetEpsgAxisInfo(OgrSpatialReference srs, string targetKey, int cooridSysCode)
        {
            if (cooridSysCode >= 4400 && cooridSysCode <= 4410)
            {
                srs.SetAxes(targetKey, "Easting", OgrAxisOrientation.East, "Northing", OgrAxisOrientation.North);
                return;
            }

            if (cooridSysCode >= 6400 && cooridSysCode <= 6423)
            {
                srs.SetAxes(targetKey, "Latitude", OgrAxisOrientation.North, "Longitude", OgrAxisOrientation.East);
                return;
            }

            #region Get the definition from the coordinate_axis.csv file.

            string[] axis1 = null, axis2 = null;
            var searchKey = cooridSysCode.ToString();
            var fileName = CsvFileHelper.GetCsvFilename("coordinate_axis.csv");

            if (!File.Exists(Path.Combine(CsvFileHelper.CsvFileFolder, fileName)))
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot find {0} under GSuitePrjHelper folder.", fileName));

            var records = CsvFileHelper.CsvScanFileByName(fileName, "COORD_SYS_CODE", searchKey,
                CsvCompareCriteria.Integer);
            if (records != null)
            {
                axis1 = new string[records.Length];
                records.CopyTo(axis1, 0);
                records = CsvFileHelper.CsvGetNextLine(fileName);
                if (records.Length > 0 && string.Equals(records[0], axis1[0], StringComparison.OrdinalIgnoreCase))
                {
                    axis2 = new string[records.Length];
                    records.CopyTo(axis2, 0);
                }
            }
            if (axis2 == null)
                throw new NotSupportedException(
                    string.Format("Failed to find entries for COORD_SYS_CODE {0} in coordinate_axis.csv", cooridSysCode));

            #endregion

            #region Confirm the records are complete, and work out which columns are which.

            var axisOrientationFieldIndex = CsvFileHelper.CsvGetFileFieldId(fileName, "coord_axis_orientation");
            var axisAbbrevFieldIndex = CsvFileHelper.CsvGetFileFieldId(fileName, "coord_axis_abbreviation");
            var axisOrderFieldIndex = CsvFileHelper.CsvGetFileFieldId(fileName, "coord_axis_order");

            if (axis1.Length < axisOrderFieldIndex + 1 || axis2.Length < axisOrderFieldIndex + 1)
                throw new NotSupportedException(
                    string.Format("Axis records appear incomplete for COORD_SYS_CODE %d in coordinate_axis.csv",
                        cooridSysCode));

            #endregion

           

            if (int.Parse(axis2[axisOrderFieldIndex]) < int.Parse(axis1[axisOrderFieldIndex]))
            {
                records = axis1;
                axis1 = axis2;
                axis2 = records;
            }



            OgrAxisOrientation oAxis1 = OgrAxisOrientation.Other, oAxis2 = OgrAxisOrientation.Other;

            for (var i = 0; i < 6; i++)
            {
                if (string.Equals(axis1[axisOrientationFieldIndex], ((OgrAxisOrientation) i).ToString(),
                    StringComparison.OrdinalIgnoreCase))
                    oAxis1 = (OgrAxisOrientation) i;
                if (string.Equals(axis2[axisOrientationFieldIndex], ((OgrAxisOrientation) i).ToString(),
                    StringComparison.OrdinalIgnoreCase))
                    oAxis2 = (OgrAxisOrientation) i;
            }

          

           

            var axisName = new string[2];
            axisName[0] = axis1[axisAbbrevFieldIndex];
            axisName[1] = axis2[axisAbbrevFieldIndex];
            for (var i = 0; i < 2; i++)
            {
                if (string.Equals(axisName[i], "N", StringComparison.OrdinalIgnoreCase))
                    axisName[i] = "Northing";
                else if (string.Equals(axisName[i], "E", StringComparison.OrdinalIgnoreCase))
                    axisName[i] = "Easting";
                else if (string.Equals(axisName[i], "S", StringComparison.OrdinalIgnoreCase))
                    axisName[i] = "Southing";
                else if (string.Equals(axisName[i], "W", StringComparison.OrdinalIgnoreCase))
                    axisName[i] = "Westing";
            }

          

            srs.SetAxes(targetKey, axisName[0], oAxis1, axisName[1], oAxis2);
        }

        private static double OgrFetchParm(double[] padfProjParms, int[] panParmIds, int nTargetId,
            double dfFromGreenwich)
        {
            int i;
            double dfResult;

            switch (nTargetId)
            {
                case NatOriginScaleFactor:
                case InitialLineScaleFactor:
                case PseudoStdParallelScaleFactor:
                    dfResult = 1.0;
                    break;

                case AngleRectifiedToSkewedGrid:
                    dfResult = 90.0;
                    break;
                default:
                    dfResult = 0.0;
                    break;
            }

            for (i = 0; i < 7; i++)
            {
                if (panParmIds[i] == nTargetId)
                {
                    dfResult = padfProjParms[i];
                    break;
                }
            }
            return dfResult;
        }

        internal static void SetEpsgGeogCs(OgrSpatialReference srs, int geogCs)
        {
            int datumCode = 0, pmCode = 0, uomAngle = 0, ellipsoidCode = 0, csc = 0;
            string geogCsName = "", datumName = "", ellipsoidName = "", pmName = "", angleName = "";
            double pmOffset = 0;
            double semiMajor = 0, invFlattening = 0, angleInDegrees = 0, angleInRadians = 0;
            var bursaTransform = new double[7];

            if (
                !EpsgGetGcsInfo(geogCs, ref geogCsName, ref datumCode, ref datumName, ref pmCode, ref ellipsoidCode,
                    ref uomAngle, ref csc))
                throw new NotSupportedException(
                    string.Format("The specified SRS format is not supported! GEOGCS value :{0}", geogCs));
            if (!EpsgGetPmInfo(pmCode, ref pmName, ref pmOffset))
                throw new NotSupportedException(
                    string.Format("The specified SRS format is not supported! GEOGCS value :{0}", geogCs));
            OgrepsgDatumNameMassage(ref datumName);
            OsrGetEllipsoidInfo(ellipsoidCode, ref ellipsoidName, ref semiMajor, ref invFlattening);

            if (!EpsgGetUomAngleInfo(uomAngle, ref angleName, ref angleInDegrees))
            {
                angleName = "degree";
                angleInDegrees = 1.0;
                uomAngle = -1;
            }

            if (angleInDegrees == 1.0)
                angleInRadians = GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUaDegreeConv);
            else
                angleInRadians = GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUaDegreeConv)*angleInDegrees;

            srs.SetGeogCS(geogCsName, datumName, ellipsoidName, semiMajor, invFlattening, pmName, pmOffset, angleName,
                angleInRadians);

            if (EpsgGetWgs84Transform(geogCs, bursaTransform))
            {
                var wgs84Node = new OgrsrsNode("TOWGS84");
                foreach (var item in bursaTransform)
                {
                    var childeNode = new OgrsrsNode(item.ToString());
                    wgs84Node.ChildNodes.Add(childeNode);
                }
                srs.GetAttrNode("DATUM").ChildNodes.Add(wgs84Node);
            }
            srs.SetAuthority("GEOGCS", "EPSG", geogCs);
            srs.SetAuthority("DATUM", "EPSG", datumCode);
            srs.SetAuthority("SPHEROID", "EPSG", ellipsoidCode);
            srs.SetAuthority("PRIMEM", "EPSG", pmCode);

            if (uomAngle > 0)
                srs.SetAuthority("GEOGCS|UNIT", "EPSG", uomAngle);
            if (csc > 0)
                SetEpsgAxisInfo(srs, "GEOGCS", csc);
        }

        internal static void SetEpsgProjCs(OgrSpatialReference srs, int pcsCode)
        {
            int gcsCode = 0, uomAngleCode = 0, uomLength = 0, trfCode = 0, projMethod = 0;
            var csc = 0;
            var parmIds = new int[7];
            string pcsName = "", uomLengthName = "";
            double dfInMeters = 0.0, dfFromGreenwich = 0.0;
            var projParms = new double[7];
            OgrsrsNode poNode;

            if (
                !EpsgGetPcsInfo(pcsCode, ref pcsName, ref uomLength, ref uomAngleCode, ref gcsCode, ref trfCode, ref csc))
                throw new NotSupportedException(string.Format("Error: Unsupported srs, PCSCode is {0}", pcsCode));
            srs.SetNode("PROJCS", pcsName);

            SetEpsgGeogCs(srs, gcsCode);

            dfFromGreenwich = srs.GetPrimeMeridian();

            if (!EpsgGetUomLengthInfo(uomLength, ref uomLengthName, ref dfInMeters))
                throw new NotSupportedException(string.Format("Error: Unsupported srs, PCSCode is {0}", pcsCode));

            srs.SetLinearUnits(uomLengthName, dfInMeters);
            srs.SetAuthority("PROJCS|UNIT", "EPSG", uomLength);

            if (!EpsgGetProjTrfInfo(pcsCode, ref projMethod, parmIds, projParms))
                throw new NotSupportedException(string.Format("Error: Unsupported srs, PCSCode is {0}", pcsCode));

            switch (projMethod)
            {
                case 9801:
                case 9817: 
                    srs.SetLcc1Sp(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9802:
                    srs.SetLcc(
                        OgrFetchParm(projParms, parmIds, StdParallel1Lat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, StdParallel2Lat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginNorthing, dfFromGreenwich));
                    break;

                case 9803:
                    srs.SetLccb(
                        OgrFetchParm(projParms, parmIds, StdParallel1Lat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, StdParallel2Lat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginNorthing, dfFromGreenwich));
                    break;

                case 9804:
                case 9805: 
                case 9841:
                case 1024: 
                    srs.SetMercator(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    if (projMethod == 1024 || projMethod == 9841) 
                    {
                        srs.SetExtension("PROJCS", "PROJ4",
                            "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext  +no_defs");
                    }
                    break;

                case 9806:
                    srs.SetCs(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9807:
                    srs.SetTm(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9808:
                    srs.SetTmso(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9809:
                    srs.SetOs(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9810:
                    srs.SetPs(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich)
                        );
                    break;

                case 9811:
                    srs.SetNzmg(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9812:
                case 9813:
                    srs.SetHom(
                        OgrFetchParm(projParms, parmIds, ProjCenterLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, ProjCenterLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, Azimuth, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, AngleRectifiedToSkewedGrid, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, InitialLineScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));

                    poNode = srs.GetAttrNode("PROJECTION").ChildNodes[0];
                    if (projMethod == 9813)
                        poNode.Value = OgrCoreDefinitiaons.SrsPtLabordeObliqueMercator;
                    break;

                case 9814:
                    srs.SetSoc(
                        OgrFetchParm(projParms, parmIds, ProjCenterLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, ProjCenterLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9815:
                    srs.SetHom(
                        OgrFetchParm(projParms, parmIds, ProjCenterLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, ProjCenterLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, Azimuth, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, AngleRectifiedToSkewedGrid, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, InitialLineScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, ProjCenterEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, ProjCenterNorthing, dfFromGreenwich));
                    break;

                case 9816:
                    srs.SetTmg(
                        OgrFetchParm(projParms, parmIds, FalseOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginNorthing, dfFromGreenwich));
                    break;

                case 9818:
                    srs.SetPolyconic(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9819:
                {
                    var dfCenterLong = OgrFetchParm(projParms, parmIds, ProjCenterLong, dfFromGreenwich);

                    if (dfCenterLong == 0.0)
                        dfCenterLong = OgrFetchParm(projParms, parmIds, PolarLongOrigin, dfFromGreenwich);

                    srs.SetKrovak(
                        OgrFetchParm(projParms, parmIds, ProjCenterLat, dfFromGreenwich),
                        dfCenterLong,
                        OgrFetchParm(projParms, parmIds, Azimuth, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, PseudoStdParallelLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, PseudoStdParallelScaleFactor, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, ProjCenterEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, ProjCenterNorthing, dfFromGreenwich));
                }
                    break;

                case 9820:
                    srs.SetLaea(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9821:
                    srs.SetLaea(
                        OgrFetchParm(projParms, parmIds, SphericalOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, SphericalOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9822:
                    srs.SetAcea(
                        OgrFetchParm(projParms, parmIds, StdParallel1Lat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, StdParallel2Lat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseOriginNorthing, dfFromGreenwich));
                    break;

                case 9823:
                    srs.SetEquirectangular(
                        OgrFetchParm(projParms, parmIds, NatOriginLat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        0.0,
                        0.0);
                    break;

                case 9829: 
                    srs.SetPs(
                        OgrFetchParm(projParms, parmIds, PolarLatStdParallel, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, PolarLongOrigin, dfFromGreenwich),
                        1.0,
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                case 9834: 
                    srs.SetCea(
                        OgrFetchParm(projParms, parmIds, StdParallel1Lat, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, NatOriginLong, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseEasting, dfFromGreenwich),
                        OgrFetchParm(projParms, parmIds, FalseNorthing, dfFromGreenwich));
                    break;

                default:
                    throw new NotSupportedException(string.Format("EPSG:No WKT support for projection method {0}.",
                        projMethod));
            }

            srs.SetAuthority("PROJCS", "EPSG", pcsCode);
            if (csc > 0)
            {
                SetEpsgAxisInfo(srs, "PROJCS", csc);
            }
        }

        private bool ImportFromEpsg(int code)
        {
            var isImportFromEpsgaSucceed = ImportFromEpsga(code);
            if (isImportFromEpsgaSucceed)
            {
                var geogCsNode = GetAttrNode("GEOGCS");
                if (geogCsNode != null)
                    geogCsNode.StripNodes("AXIS");
            }
            return isImportFromEpsgaSucceed;
        }


        private bool ImportFromEpsga(int code)
        {
            var isImportSucceeded = false;
            _isNormInfoSet = false;
            if (Node != null)
            {
                Node = null;
            }
            if (
                CsvFileHelper.CsvScanFileByName(CsvFileHelper.GetCsvFilename("gcs.csv"), "COORD_REF_SYS_CODE", "4269",
                    CsvCompareCriteria.Integer) == null)
            {
                throw new FileNotFoundException("");
            }
            string refName = null, refDatumName = null;
            int refDatum = int.MinValue,
                refPm = int.MinValue,
                refEllipsoid = int.MinValue,
                refUomAngle = int.MinValue,
                refCoordSysCode = int.MinValue;
            try
            {
                if (EpsgGetGcsInfo(code, ref refName, ref refDatum, ref refDatumName, ref refPm, ref refEllipsoid,
                    ref refUomAngle, ref refCoordSysCode))
                    SetEpsgGeogCs(this, code);
                else
                    SetEpsgProjCs(this, code);
                isImportSucceeded = true;
            }
            catch
            {
                if (!(isImportSucceeded = ImportFromDict("epsg.wkt", code)))
                {
                    var wrkDef = string.Format(CultureInfo.InvariantCulture, "+init=epsg:{0}", code);
                    if (wrkDef.IndexOf("proj=") >= 0)
                    {
                        ImportFromProj4(wrkDef);
                    }
                }
            }
            var pszAuthName = string.Empty;
            if (IsProjected())
                pszAuthName = GetAuthorityName("PROJCS");
            else
                pszAuthName = GetAuthorityName("GEOGCS");

            if (isImportSucceeded && string.IsNullOrEmpty(pszAuthName))
            {
                if (IsProjected())
                    SetAuthority("PROJCS", "EPSG", code);
                else if (IsGeographic())
                    SetAuthority("GEOGCS", "EPSG", code);

                FixupOrdering();
            }

            return isImportSucceeded;
        }

        private bool ImportFromDict(string fileName, int code)
        {
            var codeString = code.ToString();
            var isImported = false;

            var fileFullName = CsvFileHelper.CsvFileFolder + fileName;

            if (File.Exists(fileFullName))
            {
                using (var fileStream = new FileStream(fileFullName, FileMode.Open, FileAccess.Read))
                {
                    var fileContentLine = string.Empty;
                    using (TextReader tr = new StreamReader(fileStream))
                    {
                        while (!string.IsNullOrEmpty(fileContentLine = tr.ReadLine()))
                        {
                            if (fileContentLine.StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
                            {
                            }
                            if (fileContentLine.StartsWith("include", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (ImportFromDict(fileContentLine.Substring(8), code))
                                    break;
                            }
                            else if (fileContentLine.IndexOf(',') < 0)
                            {
                            }
                            else if (fileContentLine.StartsWith(codeString) && fileContentLine[codeString.Length] == ',')
                            {
                                var wkt = fileContentLine.Replace(codeString, "").Substring(1);
                                ImportFromWkt(wkt);
                                isImported = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot find {0} under GSuite Prj Helper Folder.", fileName));
            }
            return isImported;
        }

        
    }
}
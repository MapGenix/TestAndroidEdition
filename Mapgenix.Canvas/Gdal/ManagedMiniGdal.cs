using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Mapgenix.Canvas
{
    internal static class ManagedMiniGdal
    {
        internal static string EsriWktToProj4(string esriWkt)
        {
            try
            {
                var sr = new OgrSpatialReference();
                sr.ImportFromEsri(esriWkt);
                return sr.ExportToProj4();
            }
            catch (FileNotFoundException exp)
            {
                throw exp;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static string Proj4ToWkt(string proj4)
        {
            GetBaseFolder();
            try
            {
                var sr = new OgrSpatialReference();
                sr.ImportFromProj4(proj4);
                sr.MorphToEsri();
                return sr.ExportToWkt();
            }
            catch (FileNotFoundException exp)
            {
                throw new FileNotFoundException(
                    string.Format(CultureInfo.InvariantCulture,
                        "EPSG",
                        CsvFileHelper.CsvFileFolder), exp);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void GetBaseFolder()
        {
            var fullName = Assembly.GetExecutingAssembly().FullName;
            var version = fullName.Split(',')[1].Trim().Substring(8);
            var numbers = version.Split('.');
            var majorNumber = numbers[0];
            var secondNumber = numbers[1];

            CsvFileHelper.CsvFileFolder =
                string.Format(Environment.SystemDirectory +
                              string.Format(CultureInfo.InvariantCulture,
                                  "\\..\\{0}\\GSuite {1}.{2}\\GSuitePrjHelper\\",
                                  IntPtr.Size == 4 ? "System32" : "SysWOW64", majorNumber, secondNumber));
        }
    }
}
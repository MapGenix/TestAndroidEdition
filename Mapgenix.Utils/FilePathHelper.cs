using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Mapgenix.Utils
{
    /// <summary>
    /// Helper for resolve abosulte path from relative path
    /// </summary>
    public class FilePathHelper
    {

        #region public methods

        /// <summary>
        /// Return absolute path based on the <paramref name="path"/> parameter
        /// </summary>
        /// <param name="path">Absolute or relative path</param>
        /// <returns>
        ///     if path start with "..\" then get absolute physical path
        ///     if path start with ".\" then get absolute physical path
        ///     if path start with "../" then get absolute web path
        ///     if path start with "./" then get absolute web path
        ///     if path start with "/" then get absolute web path
        ///     if path start with "~" then get absolute web path
        ///     if path not contain root and contains Path.GetDirectoryName(path) + "\" then get absolute physical path
        ///     if path not contain root and contains Path.GetDirectoryName(path) + "/" then get absolute web path
        ///     else get path
        /// </returns>
        ///
        public static string GetFileAbsolutePath(string path)
        {
            string absolutePath = "";
            // destop paths
            if (path.Substring(0, 1) == @"\")
                absolutePath = AbsolutePhysicalPath(path, true);
            else if (path.Substring(0, 1) == "/")
                absolutePath = AbsoluteWebPath(path);
            else if (path.Substring(0, 1) == "~")
                absolutePath = AbsoluteWebPath(path, true);
            else if (path.Substring(0, 3) == @"..\")
                absolutePath = AbsolutePhysicalPath(path);
            else if (path.Substring(0, 2) == @".\")
                absolutePath = AbsolutePhysicalPath("." + path);
            // Web paths
            else if (path.Substring(0, 3) == "../")
                absolutePath = AbsoluteWebPath(path);
            else if (path.Substring(0, 3) == "./")
                absolutePath = AbsoluteWebPath("." + path);
            // paths start without identifier
            else if (!Path.IsPathRooted(path))
                if (path.IndexOf(Path.GetDirectoryName(path) + @"\") != -1)
                    absolutePath = AbsolutePhysicalPath(path);
                else if (path.IndexOf(Path.GetDirectoryName(path) + @"/") != -1)
                    absolutePath = AbsoluteWebPath(path);
                else
                    absolutePath = path;
            else
                absolutePath = path;

            return HandleSpecialCharacters(absolutePath);
        }

        /// <summary>
        /// Return absolute path from method GetFileAbsolutePath
        /// </summary>
        /// <param name="key">AppSetting key for get path</param>
        /// <returns>
        ///     return GetFileAbsolutePath()
        /// </returns>
        ///
        public static string GetPathFromAppSettings(string key)
        {
            string path = System.Configuration.ConfigurationManager.AppSettings[key].ToString();

            if (path == null)
                throw new System.Configuration.ConfigurationErrorsException(String.Format("Key {0} not found in application config file", key));

            return GetFileAbsolutePath(path);

        }

        /// <summary>
        /// Resolve relative path in connection string
        /// </summary>
        /// <param name="nameOrConnectionString">Name connection string in app|web.config or full connection string</param>
        /// <returns>
        /// if <paramref name="nameOrConnectionString"/> is name of connection string then refresh app|web.config runtime references and return <paramref name="nameOrConnectionString"/>
        /// if <paramref name="nameOrConnectionString"/> is a full connection string then return connection string with resolve absolutes path
        /// </returns>
        ///
        public static string GetPathFromConnectionString(string nameOrConnectionString)
        {
            string dataSource = nameOrConnectionString;
            if (nameOrConnectionString.IndexOf("Data Source=") == -1 || nameOrConnectionString.IndexOf("User Id=") == -1)
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[nameOrConnectionString];
                if(connectionString != null)
                {
                    dataSource = connectionString.ConnectionString;
                }
                else
                    throw new System.Configuration.ConfigurationErrorsException(String.Format("Connection String {0} not found in application config file", nameOrConnectionString));
            }

            string path = dataSource.Split(';').Where(r => r.IndexOf("Data Source=") != -1).Select(r => r).ToList()[0].Replace("Data Source=", "");

            return dataSource.Replace(path, GetFileAbsolutePath(path));
        }

        #endregion

        #region private methods

        /// <summary>
        /// Return absolute physical path based on the <paramref name="path"/> parameter
        /// <param name="path">Absolute or relative physical path</param>
        /// <param name="relativeToRoot">true when path is relative to root, false when is relative to current path</param>
        /// </summary>
        private static string AbsolutePhysicalPath(string path, bool relativeToRoot = false)
        {
            Uri baseURI = null;

            if (relativeToRoot)
                baseURI = new Uri(Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory));
            else 
                baseURI = new Uri(AppDomain.CurrentDomain.BaseDirectory);

            Uri absoluteUri = new Uri(baseURI, path);

            return absoluteUri.AbsolutePath;
        }

        /// <summary>
        /// Return absolute web path based on the <paramref name="path"/> parameter
        /// <param name="path">Absolute or relative physical path</param>
        /// <param name="serverPath">true when use current server mapPath, false when use current url</param>
        /// </summary>
        private static string AbsoluteWebPath(string path, bool serverPath = false)
        {
            Uri absoluteUri = null;

            if (serverPath)
            { 
                absoluteUri = new Uri(HttpContext.Current.Server.MapPath(path));
            }
            else
            {
                absoluteUri = new Uri(HttpContext.Current.Request.Url, path);
            }

            return absoluteUri.AbsolutePath;

        }

        /// <summary>
        /// Replace Url Encode Characters for Decode Characters base on <paramref name="path"/> parameter
        /// <param name="path">Absolute or relative physical path</param>
        /// </summary>
        private static string HandleSpecialCharacters(string path)
        {
            return HttpUtility.UrlDecode(path);
        }

        public static string GetBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        #endregion

    }
}

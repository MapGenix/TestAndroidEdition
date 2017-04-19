using Portable.Licensing;
using Portable.Licensing.Validation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Mapgenix.Utils
{
    /// <summary>
    /// Implements IValidationFailure to handle personalized license validations
    /// </summary>
    public class LicenseValidationFailure : IValidationFailure
    {
        public string HowToResolve { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Custom exception to comunicate license validations exceptions
    /// </summary>
    public class LicenseFailureException : Exception
    {
        public LicenseFailureException()
        {
        }

        public LicenseFailureException(string message)
            : base(message)
        {
        }

        public LicenseFailureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }



    /// <summary>
    /// Contains the methods needed to validate the license
    /// </summary>
    public class LicenseValidation
    {

        public bool isInitialized { get; set; }
        public bool isLicensed { get; set; }
        public bool isExpired { get; set; }
        public int daysLeft { get; set; }

        private string outputFilePath;

        private Dictionary<string, string> mapEditions = new Dictionary<string, string>()
        {
            {"WINFORMS", "1"},
            {"WPF", "2"},
            {"WEBFORMS", "3"},
            {"MVC", "4"},
            {"WEBAPI", "5"},
            {"ROUTING", "6"},
            {"PRODUCTION", "7" },
            {"SAAS", "8" }
        };


        public void ValidateLicense(string edition, string licenseFilePath = "")
        {
            bool licenseFileExist = true;


            //Defines path to file
            if (licenseFilePath == "")
            {
                outputFilePath = "../../App_Data/License.lic";
                licenseFilePath = "../../App_Data/";
            }
            else
                outputFilePath = String.Format("{0}\\License.lic", licenseFilePath).ToString();

            // See if this file not exists in the same directory|
            if (!File.Exists(outputFilePath) || edition == "Production" || edition == "SaaS")
            {
                licenseFileExist = GetLicense(licenseFilePath, edition).Result;
            }

            if (licenseFileExist)
            {
                // Load the license from the xml data file.
                Portable.Licensing.License license;
                using (var streamReader = new StreamReader(outputFilePath))
                {
                    license = Portable.Licensing.License.Load(streamReader);
                }

                //Load public key from license file
                string publicKey = license.AdditionalAttributes.Get("PublicKey");

                IValidationFailure idValidationFailure = new LicenseValidationFailure();
                idValidationFailure.Message = "The license file does not match with your product key";
                idValidationFailure.HowToResolve = "Use the appropiate license for this product ";

                IValidationFailure productValidationFailure = new LicenseValidationFailure();
                productValidationFailure.Message = "This product can not be run under this license";
                productValidationFailure.HowToResolve = "Use the appropiate license for this product ";

                IValidationFailure MACValidationFailure = new LicenseValidationFailure();
                MACValidationFailure.Message = "This license was not issue for this particular hardware";
                MACValidationFailure.HowToResolve = "Use the appropiate license for this product ";

                string key = ConfigurationManager.AppSettings["MapgenixKey"];
                switch (edition)
                {
                    case "routing":
                        key = ConfigurationManager.AppSettings["RoutingExtensionKey"];
                        break;
                    case "Production":
                    case "SaaS":
                        key = ConfigurationManager.AppSettings["ProductionServerKey"];
                        break;
                }

                var validationFailures = license.Validate()
                                                .ExpirationDate()
                                                    .When(lic => lic.Type == LicenseType.Trial)
                                                .And()
                                                .AssertThat(lic => lic.Id == new Guid(key), idValidationFailure)
                                                .And()
                                                .AssertThat(lic => lic.ProductFeatures.Get("Product") == mapEditions[edition.ToUpper()], productValidationFailure)
                                                .And()
                                                .AssertThat(lic => lic.AdditionalAttributes.Get("MACAddress") == GetMACAddress(), MACValidationFailure)
                                                .And()
                                                .Signature(publicKey)
                                                .AssertValidLicense();

                try
                {
                    //Convert to List
                    List<IValidationFailure> listFailure = validationFailures.ToList();


                    if (listFailure.Count > 0)
                    {
                        isLicensed = false;
                        isExpired = true;
                        //throw new LicenseFailureException(listFailure.First().Message);                    
                    }
                    else
                    {
                        if (license.Type == LicenseType.Standard)
                        {
                            isLicensed = true;
                        }
                        else
                        {
                            daysLeft = Convert.ToInt32((license.Expiration - DateTime.Now).TotalDays);

                            if (daysLeft <= 0)
                            {
                                isExpired = true;
                                isLicensed = false;
                                //throw new LicenseFailureException("Your license has expired");
                            }
                            else
                            {
                                isExpired = false;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //throw;
                    throw new LicenseFailureException("License Validation failed");
                }

            }
            else
            {
                isLicensed = false;
                isExpired = true;
            }
        }

        //TODO: To really be an async task, the heavy lifting of the method must be async, in this case use HttpWebRequest.BeginGetResponse/EndGetResponse
        private async Task<bool> GetLicense(string licenseFilePath, string edition)
        {
            try
            {
                //TODO: Implement some sort of error log
                //Load product key from web config file
                string ProductKey = ConfigurationManager.AppSettings["MapgenixKey"];
                switch (edition)
                {
                    case "routing":
                        ProductKey = ConfigurationManager.AppSettings["RoutingExtensionKey"];
                        break;
                    case "Production":
                    case "SaaS":
                        ProductKey = ConfigurationManager.AppSettings["ProductionServerKey"];
                        break;
                }

                var outputFilePath = String.Format("{0}\\License.lic", licenseFilePath);
                //TODO: Change this to get the hardware ID to avoid the isse with several network interfaces
                string macAddress = GetMACAddress();
                string uri = string.Format("http://license.mapgenix.com//api/license/GetLicense?productKey={0}&macAddress={1}", ProductKey, macAddress);
                //string uri = string.Format("http://localhost:11355/api/license/GetLicense?productKey={0}&macAddress={1}", ProductKey, macAddress);
                HttpWebRequest webRequest = WebRequest.Create(uri) as HttpWebRequest;
                HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
                string statusCode = webResponse.StatusCode.ToString();
                if (statusCode != "OK")
                {
                    //TODO: Log scenario
                    return false;
                }
                else
                {
                    Encoding enc = System.Text.Encoding.GetEncoding(1252);
                    StreamReader responseStream = new StreamReader(webResponse.GetResponseStream(), enc);
                    string Response = responseStream.ReadToEnd();

                    if (!Directory.Exists(licenseFilePath))
                        Directory.CreateDirectory(licenseFilePath);

                    //Stream fileContent = Response.ReadAsStreamAsync().Result;
                    File.WriteAllText(outputFilePath, Response);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;

            foreach (NetworkInterface adapter in nics)
            {
                // only return MAC Address from first card  
                if (sMacAddress == String.Empty)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }

            return sMacAddress;
        }

        public void RegisterActivityProductionServer(string iPAddress)
        {
            string ProductKey = (ConfigurationManager.AppSettings["ProductionServerKey"] == null) ? ConfigurationManager.AppSettings["MapgenixKey"] : ConfigurationManager.AppSettings["ProductionServerKey"];
            string uri = string.Format("http://license.mapgenix.com/api/License/GetRegisterIP?IPAddress={0}&productKey={1}", iPAddress, ProductKey);
            //string uri = string.Format("http://localhost:11355/api/License/GetRegisterIP?IPAddress={0}&productKey={1}", iPAddress, ProductKey);
            HttpWebRequest webRequest = WebRequest.Create(uri) as HttpWebRequest;
            HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
            string statusCode = webResponse.StatusCode.ToString();
            if (statusCode != "OK")
            {
                throw new Exception(webResponse.StatusDescription);
            }
        }

    }
}

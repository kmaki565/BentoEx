using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace BentoEx.Model
{
    class MyPassword
    {
        public string CompanyCode { get; private set; }
        public string UserId { get; private set; }
        public string Password { get; private set; }

        const string keyName = @"HKCU:\Software\BentoEx";
        const string keyPath = @"Software\BentoEx";

        public MyPassword()
        {
            CompanyCode = GetCompanyCode();
            UserId = GetUserId();
            Password = GetPassword();
        }

        string GetCompanyCode()
        {
            return "lenovo-y";
        }
        /// <summary>
        /// Read encrypted user id in the registry and return the clear text.
        /// </summary>
        /// <returns>clear text API key, or null if failed</returns>
        string GetUserId()
        {
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(keyPath, false);

            return (string)regkey?.GetValue("UserId");
        }
        /// <summary>
        /// Read encrypted password in the registry and return the clear text.
        /// </summary>
        /// <returns>clear text Secret, or null if failed</returns>
        string GetPassword()
        {
            return Get_A_Password("Password");
        }

        /// <summary>
        /// Read a registry value in the key and convert it to a clear text.
        /// </summary>
        /// <param name="Regvalue">Registry value name</param>
        /// <returns>clear text password, or null if failed</returns>
        string Get_A_Password(string Regvalue)
        {
            string psScript = "(Get-ItemProperty " + keyName + ")." + Regvalue + 
                @"| ConvertTo-SecureString | %{[System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($_)} | %{[System.Runtime.InteropServices.Marshal]::PtrToStringAuto($_)}";

            using (var invoker = new RunspaceInvoke())
            {
                var results = invoker.Invoke(psScript, new object[] { });

                if (results.Count == 1)
                    return results[0].BaseObject.ToString();
            }

            return null;
        }
    }
}

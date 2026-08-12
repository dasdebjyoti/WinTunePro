using System;
using System.Reflection;
using System.Windows.Forms;

namespace WinTunePro.Utils
{
    public static class AppInfo
    {
        public static string Name
        {
            get
            {
                // Prefer Application.ProductName (set from AssemblyProduct), fallback to entry assembly name or literal
                var prod = Application.ProductName;
                if (!string.IsNullOrWhiteSpace(prod)) return prod;

                var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var attr = asm.GetCustomAttribute<AssemblyProductAttribute>();
                if (attr != null && !string.IsNullOrWhiteSpace(attr.Product)) return attr.Product;

                return asm.GetName().Name ?? "WinTunePro";
            }
        }
    }
}
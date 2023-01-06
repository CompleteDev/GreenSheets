using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;


namespace GreenSheetCreator
{
    public class MyFontResolver : IFontResolver
    {

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Ignore case of font names.
            var name = familyName.ToLower().TrimEnd('#');

            // Deal with the fonts we know.
            switch (name)
            {
                case "fre3of9x":
                    return new FontResolverInfo("fre3of9x#");
                case "arial":
                    if (isBold)
                    {
                        return new FontResolverInfo("Arial#b");
                    }
                    return new FontResolverInfo("Arial#");
            }

            // We pass all other font requests to the default handler.
            // When running on a web server without sufficient permission, you can return a default font at this stage.
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        /// <summary>
        /// Return the font data for the fonts.
        /// </summary>
        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "fre3of9x#":
                    return FontHelper.fre3of9x;
                case "Arial#":
                    return FontHelper.Arial;

                case "Arial#b":
                    return FontHelper.arialbd;

            }

            return null;
        }


        internal static MyFontResolver OurGlobalFontResolver = null;

        /// <summary>
        /// Ensure the font resolver is only applied once (or an exception is thrown)
        /// </summary>
        internal static void Apply()
        {
            if (OurGlobalFontResolver == null || GlobalFontSettings.FontResolver == null)
            {
                if (OurGlobalFontResolver == null)
                    OurGlobalFontResolver = new MyFontResolver();

                GlobalFontSettings.FontResolver = OurGlobalFontResolver;
            }
        }
    }


    /// <summary>
    /// Helper class that reads font data from embedded resources.
    /// </summary>
    public static class FontHelper
    {
        public static byte[] fre3of9x
        {
            get { return LoadFontData("GreenSheetCreator.Fonts.fre3of9x.ttf"); }
        }

        public static byte[] Arial
        {
            get { return LoadFontData("GreenSheetCreator.Fonts.Arial.ttf"); }
        }
        public static byte[] arialbd
        {
            get { return LoadFontData("GreenSheetCreator.Fonts.arialbd.ttf"); }
        }

        /// <summary>
        /// Returns the specified font from an embedded resource.
        /// </summary>
        static byte[] LoadFontData(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Test code to find the names of embedded fonts
            //var ourResources = assembly.GetManifestResourceNames();

            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new ArgumentException("No resource with name " + name);

                int count = (int)stream.Length;
                byte[] data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }
    }
}

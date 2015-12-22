
using Android.Content;
using System.IO;
using System;

namespace Com.Krkadoni.Utils
{
    public static class FileSystem
    {

        public static string GetTmpDir(Context context)
        {
            if (IsExternalStorageReadable() && IsExternalStorageWritable())
            {
                return context.ExternalCacheDir.AbsolutePath;
            }
            return context.CacheDir.AbsolutePath;
        }

        /// Checks if external storage is available for read and write
        public static bool IsExternalStorageWritable()
        {
            string state = Android.OS.Environment.ExternalStorageState;
            if (Android.OS.Environment.MediaMounted.Equals(state))
            {
                return true;
            }
            return false;
        }

        /// Checks if external storage is available to at least read
        public static bool IsExternalStorageReadable()
        {
            string state = Android.OS.Environment.ExternalStorageState;
            if (Android.OS.Environment.MediaMounted.Equals(state) ||
                Android.OS.Environment.MediaMountedReadOnly.Equals(state))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the album storage dir.
        /// </summary>
        /// <returns>The album storage dir.</returns>
        public static Java.IO.File GetAlbumStorageDir(string albumName)
        {

            // Get the directory for the app's private pictures directory. 
            Java.IO.File file = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), albumName);
            if (!file.Exists())
            {
                if (!file.Mkdirs())
                {
                    System.Diagnostics.Debug.Write(string.Format("Directory {0} not created!", file.AbsolutePath));
                }
            }

            if (file != null)
            {
                file.SetReadable(true);
                file.SetWritable(true);
            }

            return file;
        }
            
        public static string ReadStringAsset(Context context, string assetFileName)
        {
            if (assetFileName == null)
                throw new ArgumentNullException("assetFileName");
            
            string content = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(context.Assets.Open(assetFileName)))
                {
                    content = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine( string.Format("Failed to read {0}! {1}"), assetFileName,  ex.Message);
            }
            return content;
        }

        public static byte[] ReadByteAsset(Context context, string assetFileName)
        {
            if (assetFileName == null)
                throw new ArgumentNullException("assetFileName");

            byte[] content = null;
            try
            {
                using (var sr = context.Assets.Open(assetFileName))
                {
                    byte[] buffer = new byte[16*1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = sr.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine( string.Format("Failed to read {0}! {1}"), assetFileName,  ex.Message);
            }
            return content;
        }

    }
}


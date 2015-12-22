using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Util;
using Android.Graphics.Drawables;
using Android.Graphics;
using Com.Krkadoni.App.SignalMeter.Model;
using Krkadoni.Enigma.Enums;
using System;
using System.IO;
using Android.Content;
using Android.Provider;
using Com.Krkadoni.Utils;


namespace Com.Krkadoni.App.SignalMeter.Layout
{
    [Activity(Name = "com.krkadoni.app.signalmeter.layout.ScreenShotActivity", Label = "ScreenShot", ParentActivity = typeof(MainActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "com.krkadoni.app.signalmeter.layout.MainActivity")]
    public class ScreenShotActivity : AppCompatActivity
    {
        ImageView iView;
        ProgressBar pbScreenShot;
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private const string TAG = "ScreenShotActivity";
        private const string albumName = "SignalMeter";
        private const string tmpFileName = "screenshot.jpg";
        private const string cachedImage = "CACHED_SCREENSHOT_PATH";
        private Drawable defaultImage;
        private LinearLayout layoutPb = null;
        private IMenuItem screenshotAll;
        private IMenuItem screenshotOSD;
        private IMenuItem screenshotPicture;
        private IMenuItem saveMenu;
        private Android.Support.V7.Widget.Toolbar bottomToolbar;

        // int SHOT_TYPE;

        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.screenshot_activity);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);       
            SetSupportActionBar(mToolbar);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = GetString(Resource.String.action_screenshot);
            iView = FindViewById<ImageView>(Resource.Id.ivScreenShot);
            iView.Click += (sender, e) =>
            {
               
                var content = GetBytesFromDrawable(iView.Drawable);

                //make sure we have something to show in the first place
                if (content != null && iView.Drawable != defaultImage)
                {
                        
                    //there is no external storage available, Gallery cannot show internal cache image
                    if (FileSystem.GetTmpDir(this) == CacheDir.AbsolutePath)
                        return;
                       
                    try
                    {
                        if (SaveFile(GetTmpFileName(), content))
                        {
                            Intent intent = new Intent();
                            intent.SetAction(Intent.ActionView);
                            intent.SetDataAndType(Android.Net.Uri.Parse("file://" + GetTmpFileName()), "image/*");
                            StartActivity(intent);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(TAG, string.Format("Error launching image view intent! {0}", ex.Message));
                    }
                }
            };
            pbScreenShot = FindViewById<ProgressBar>(Resource.Id.pbScreenShot);
            defaultImage = iView.Drawable;
            layoutPb = FindViewById<LinearLayout>(Resource.Id.layoutPb);
            bottomToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar_bottom);
            bottomToolbar.InflateMenu(Resource.Menu.menu_screenshot);
            screenshotAll = bottomToolbar.Menu.FindItem(Resource.Id.action_screenshot_all);
            screenshotOSD = bottomToolbar.Menu.FindItem(Resource.Id.action_screenshot_osd);
            screenshotPicture = bottomToolbar.Menu.FindItem(Resource.Id.action_screenshot_picture);
            saveMenu = bottomToolbar.Menu.FindItem(Resource.Id.action_save);
            saveMenu.SetVisible(FileSystem.IsExternalStorageWritable());
            bottomToolbar.MenuItemClick += (sender, e) =>
            {
                if (e.Item.ItemId == Resource.Id.action_screenshot_all)
                {
                    TakeScreenshot(ScreenshotType.All);
                    e.Handled = true;
                }
                else if (e.Item.ItemId == Resource.Id.action_screenshot_osd)
                {
                    TakeScreenshot(ScreenshotType.Osd);
                    e.Handled = true;
                }
                else if (e.Item.ItemId == Resource.Id.action_screenshot_picture)
                {
                    TakeScreenshot(ScreenshotType.Picture);
                    e.Handled = true;
                }
                else if (e.Item.ItemId == Resource.Id.action_save)
                {
                    SaveToDisk();
                    e.Handled = true;
                }
            };
            //bundle = this.Intent.Extras;
            //SHOT_TYPE = bundle.GetInt("SHOT_TYPE");
            if (bundle != null)
            {
                var cachedScreenshotPath = bundle.GetString(cachedImage);
                if (!string.IsNullOrEmpty(cachedScreenshotPath))
                {
                    var cachedScreenShot = LoadTmpFile(); 
                    if (cachedScreenShot != null)
                    {
                        layoutPb.Visibility = ViewStates.Gone;
                        iView.Visibility = ViewStates.Visible; 
                        var bitmap = CreateBitmapFromBytes(cachedScreenShot);
                        iView.SetImageBitmap(bitmap);
                    }
                }
            }
            else
            {
                TakeScreenshot(ScreenshotType.All);
            }   
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            if (iView.Drawable != null)
                outState.PutString(cachedImage, GetTmpFileName());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SaveFile(GetTmpFileName(), GetBytesFromDrawable(iView.Drawable));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                NavUtils.NavigateUpFromSameTask(this);            
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
            

        private bool screenshotPending;

        private async void TakeScreenshot(ScreenshotType screenshotType)
        {
            if (screenshotPending)
                return;

            layoutPb.Visibility = ViewStates.Visible;
            iView.Visibility = ViewStates.Gone;
            screenshotAll.SetEnabled(false);
            screenshotOSD.SetEnabled(false);
            screenshotPicture.SetEnabled(false);
            saveMenu.SetEnabled(false);

            screenshotPending = true;
            var response = await ConnectionManager.GetScreenShotOfCurrentService(screenshotType);

            layoutPb.Visibility = ViewStates.Gone;
            iView.Visibility = ViewStates.Visible; 
            screenshotAll.SetEnabled(true);
            screenshotOSD.SetEnabled(true);
            screenshotPicture.SetEnabled(true);
            saveMenu.SetEnabled(true);

            if (response == null)
            {
                screenshotPending = false;
                iView.SetScaleType(ImageView.ScaleType.Center);
                iView.SetImageDrawable(defaultImage);
                return;
            }

            iView.SetScaleType(ImageView.ScaleType.FitCenter);
            iView.SetImageBitmap(CreateBitmapFromBytes(response.Screenshot));
            screenshotPending = false;
        }

        private  bool SaveFile(string path, byte[] content)
        {
            if (content == null)
                return false;
            
            try
            {
                System.IO.File.WriteAllBytes(path, content);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("Failed to save bitmap to CacheDir! {0}", ex.Message));
                return false;
            }
        }

        private byte[] LoadTmpFile()
        {
            if (!System.IO.File.Exists(GetTmpFileName()))
                return null;
            try
            {
                return System.IO.File.ReadAllBytes(GetTmpFileName());
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("Failed to load bitmap from CacheDir! {0}", ex.Message));
                return null;
            }
           
        }

        private bool DeleteTmpFile()
        {
            if (!System.IO.File.Exists(GetTmpFileName()))
                return true;
            try
            {
                System.IO.File.Delete(GetTmpFileName());
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("Failed to delete bitmap from CacheDir! {0}", ex.Message));
                return false;
            }
        }

        private Bitmap CreateBitmapFromBytes(byte[] content)
        {
            if (content == null)
                return null;
            
            try
            {
                return BitmapFactory.DecodeByteArray(content, 0, content.Length);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("Failed to create bitmap instance from byte array! {0}", ex.Message));
                return null;
            }
        }

        private byte[] GetBytesFromDrawable(Drawable drawable)
        {
            if (drawable == null)
                return null;
            
            try
            {
                using (BitmapDrawable bitmapDrawable = ((BitmapDrawable)drawable))
                {
                    Bitmap bitmap = bitmapDrawable.Bitmap;
                    using (var stream = new MemoryStream())
                    {
                        bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("Failed to get byte array from drawable! {0}", ex.Message));
                return null;
            }
        }


        private string GetTmpFileName()
        {
            return System.IO.Path.Combine(FileSystem.GetTmpDir(this), tmpFileName);
        }

        private string GenerateScreenShotFileName()
        {
            return (Java.Lang.JavaSystem.CurrentTimeMillis() / 1000).ToString() + ".jpg";
        }

        private void SaveToDisk()
        {
            
            if (iView.Drawable != null && iView.Drawable != defaultImage)
            {
                var content = GetBytesFromDrawable(iView.Drawable);
                if (content == null)
                    return;
                
                var fileName = GenerateScreenShotFileName();
                var fullFileName = System.IO.Path.Combine(FileSystem.GetAlbumStorageDir(albumName).AbsolutePath, fileName);
                if (SaveFile(fullFileName, content))
                {
                    MediaStore.Images.Media.InsertImage(ContentResolver, fullFileName, fileName, fileName);
                    SendBroadcast(new Intent(Intent.ActionMediaScannerScanFile, Android.Net.Uri.FromFile(new Java.IO.File(fullFileName))));
                    saveMenu.SetEnabled(false);
                    Toast.MakeText(this, string.Format(GetString(Resource.String.inf_saved_filename), fileName), ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.err_save_failed), ToastLength.Short).Show();
                }
            }
        }
    }
}


using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using System;
using Android.Gms.Vision.Texts;
using Android.Views;
using Java.Lang;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using Android.Runtime;
using Android.Util;
using Android.Content;
using Java.IO;
using Android.Support.Compat;
using Android.Support.V4.Content;
using Android.Graphics;
using Android.Gms.Vision;
using System.Text.RegularExpressions;

namespace TestOcrCamera
{
    [Activity(Label = "TestOcrCamera", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, View.IOnClickListener
    {

        private const string LOG_TAG = "Text API";
        private static int PHOTO_REQUEST = 10;
        private TextView scanResults;
        private Android.Net.Uri imageUri;
        private TextRecognizer detector;
        private const int RequestWritePermission = 20;
        private const string SAVED_INSTANCE_URI = "uri";
        private const string SAVED_INSTANCE_RESULT = "result";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            Button button = (Button)FindViewById(Resource.Id.button);
            scanResults = (TextView)FindViewById(Resource.Id.results);
            if (savedInstanceState != null)
            {
                imageUri = Android.Net.Uri.Parse(savedInstanceState.GetString(SAVED_INSTANCE_URI));
                scanResults.Text = savedInstanceState.GetString(SAVED_INSTANCE_RESULT);
            }


            detector = new TextRecognizer.Builder(ApplicationContext).Build();
            button.SetOnClickListener(this);
        }

        public void OnClick(View v)
        {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, RequestWritePermission);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestWritePermission:
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        TakePicture();
                    }
                    else
                    {
                        //Toast.makeText(MainActivity.this, "Permission Denied!", Toast.LENGTH_SHORT).show();
                        Log.Error("Main Activity", "Permissão negada!");
                    }
                    break;
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == PHOTO_REQUEST && resultCode == Result.Ok)
            {
                LaunchMediaScanIntent();
                try
                {
                    Bitmap bitmap = DecodeBitmapUri(ApplicationContext, imageUri);
                    if (detector.IsOperational && bitmap != null)
                    {
                        Frame frame = new Frame.Builder().SetBitmap(bitmap).Build();
                        SparseArray textBlocks = detector.Detect(frame);
                        System.String blocks = "";
                        System.String lines = "";
                        System.String words = "";
                        var regexMoney = new Regex(@"\$ ?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9][0-9])?$*");
                        for (int index = 0; index < textBlocks.Size(); index++)
                        {
                            //extract scanned text blocks here
                            TextBlock tBlock = (TextBlock)textBlocks.ValueAt(index);
                            blocks = blocks + tBlock.Value + " " + "\n";
                            var text = Regex.Replace(blocks.ToString(), "[A-Za-z ]", " ");
                            Match match = regexMoney.Match(text);
                            if (match.Success)
                            {
                                scanResults.Text = match.Value;
                                break;
                            }
                            else
                            {
                                scanResults.Text = "No Price.";
                            }

                            //    foreach (IText line in tBlock.Components)
                            //    {
                            //        //extract scanned text lines here
                            //        lines = lines + line.Value + "\n";
                            //        foreach (IText element in line.Components)
                            //        {
                            //            //extract scanned text words here
                            //            words = words + element.Value + ", ";
                            //        }
                            //    }
                            //}
                            //if (textBlocks.Size() == 0)
                            //{
                            //    scanResults.Text = "Scan Failed: Found nothing to scan";
                            //}
                            //else
                            //{



                            //    scanResults.Text = "Assim funcaaaaaaaaaaa";
                            //    //scanResults.Text = scanResults.Text + "Blocks: " + "\n";
                            //    //scanResults.Text = scanResults.Text + blocks + "\n";
                            //    //scanResults.Text = scanResults.Text + "---------" + "\n";
                            //    //scanResults.Text = scanResults.Text + "Lines: " + "\n";
                            //    //scanResults.Text = scanResults.Text + lines + "\n";
                            //    //scanResults.Text = scanResults.Text + "---------" + "\n";
                            //    //scanResults.Text = scanResults.Text + "Words: " + "\n";
                            //    //scanResults.Text = scanResults.Text + words + "\n";
                            //    //scanResults.Text = scanResults.Text + "---------" + "\n";
                            //}
                        }
                        //else
                        //{
                        //    scanResults.Text = "Could not set up the detector!";
                        //}
                    }
                }
                catch (System.Exception e)
                {

                    Log.Error("OnActivityResult", e.StackTrace);
                }
            }
        }

        private Bitmap DecodeBitmapUri(Context ctx, Android.Net.Uri uri)
        {
            int targetW = 600;
            int targetH = 600;
            BitmapFactory.Options bmOptions = new BitmapFactory.Options();
            bmOptions.InJustDecodeBounds = true;
            BitmapFactory.DecodeStream(ctx.ContentResolver.OpenInputStream(uri), null, bmOptions);
            int photoW = bmOptions.OutWidth;
            int photoH = bmOptions.OutHeight;

            int scaleFactor = System.Math.Min(photoW / targetW, photoH / targetH);
            bmOptions.InJustDecodeBounds = false;
            bmOptions.InSampleSize = scaleFactor;

            return BitmapFactory.DecodeStream(ctx.ContentResolver.OpenInputStream(uri), null, bmOptions);
        }


        private void LaunchMediaScanIntent()
        {
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            mediaScanIntent.SetData(imageUri);
            this.SendBroadcast(mediaScanIntent);
        }




        private void TakePicture()
        {
            Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
            File photo = new File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "picture.jpg");

            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.N)
                imageUri = FileProvider.GetUriForFile(this, BuildConfig.ApplicationId + ".provider", photo);
            else
                imageUri = Android.Net.Uri.FromFile(photo);


            intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, imageUri);
            StartActivityForResult(intent, PHOTO_REQUEST);
        }
    }
}


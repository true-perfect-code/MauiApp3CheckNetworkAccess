//using Android.App;
//using Android.Content.PM;
//using Android.OS;

//namespace MauiApp3CheckNetworkAccess
//{
//    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
//    public class MainActivity : MauiAppCompatActivity
//    {
//    }
//}


using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Extensions.DependencyInjection;
using MauiApp3CheckNetworkAccess.Platforms.Android;
using Android.Util;
using System;

namespace MauiApp3CheckNetworkAccess
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private const string AutostartCheckedKey = "AutostartChecked";
        private ISharedPreferences sharedPreferences;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CreateNotificationFromIntent(Intent);

            // Initialisieren Sie die SharedPreferences
            sharedPreferences = GetSharedPreferences("AppPreferences", FileCreationMode.Private);

            // Überprüfen Sie die Autostart-Berechtigung und leiten Sie den Benutzer zu den Einstellungen, falls erforderlich
            CheckAutostartPermission();
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            CreateNotificationFromIntent(intent);
        }

        static void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string identifier = intent.GetStringExtra(LocalNotification.IdentifierKey);

                if (!string.IsNullOrEmpty(identifier))
                {
                    //// Speichern Sie das Startargument in einer globalen, zugänglichen Klasse
                    //Shared.Utility.AppLaunchArguments.LaunchArguments = identifier;

                    //// Optional: Logging für Debugging
                    //Console.WriteLine($"Notification clicked with Identifier: {identifier}");
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Berechtigungen für Benachrichtigungen und exakte Alarme anfordern
            var notificationManager = new LocalNotification();
            notificationManager.RequestNotificationPermission();
            notificationManager.RequestExactAlarmPermission();
        }

        private void CheckAutostartPermission()
        {
            bool autostartChecked = sharedPreferences.GetBoolean(AutostartCheckedKey, false);
            if (!autostartChecked)
            {
                ShowAutostartPermissionDialog();
            }
        }

        private void ShowAutostartPermissionDialog()
        {
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("Autostart-Berechtigung erforderlich");
            builder.SetMessage("Um Benachrichtigungen zu empfangen, müssen Sie die Autostart-Berechtigung für diese App aktivieren. Möchten Sie jetzt zu den Einstellungen gehen?");
            builder.SetPositiveButton("Einstellungen öffnen", (sender, args) =>
            {
                OpenAutostartSettings();
            });
            builder.SetNegativeButton("Später", (sender, args) => { });
            builder.Show();
        }

        private void OpenAutostartSettings()
        {
            try
            {
                Intent intent = new Intent();
                string manufacturer = Build.Manufacturer.ToLower();
                if ("xiaomi".Equals(manufacturer, StringComparison.OrdinalIgnoreCase))
                {
                    intent.SetComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity"));
                }
                else if ("oppo".Equals(manufacturer, StringComparison.OrdinalIgnoreCase))
                {
                    intent.SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity"));
                }
                else if ("vivo".Equals(manufacturer, StringComparison.OrdinalIgnoreCase))
                {
                    intent.SetComponent(new ComponentName("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity"));
                }
                else if ("letv".Equals(manufacturer, StringComparison.OrdinalIgnoreCase))
                {
                    intent.SetComponent(new ComponentName("com.letv.android.letvsafe", "com.letv.android.letvsafe.AutobootManageActivity"));
                }
                else if ("honor".Equals(manufacturer, StringComparison.OrdinalIgnoreCase))
                {
                    intent.SetComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.optimize.process.ProtectActivity"));
                }

                var list = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
                if (list.Count > 0)
                {
                    StartActivity(intent);
                    var editor = sharedPreferences.Edit();
                    editor.PutBoolean(AutostartCheckedKey, true);
                    editor.Apply();
                }
            }
            catch (Exception e)
            {
                Log.Error("exc", e.ToString());
            }
        }

        private void ResetAutostartChecked()
        {
            var editor = sharedPreferences.Edit();
            editor.PutBoolean(AutostartCheckedKey, false);
            editor.Apply();
        }
    }
}

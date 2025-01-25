//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MauiApp3CheckNetworkAccess.Platforms.Android
//{
//    internal class LocalNotification
//    {
//    }
//}


using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using Android.Provider;

namespace MauiApp3CheckNetworkAccess.Platforms.Android
{
    public class LocalNotification //: ILocalNotification
    {
        const string channelId = "default_channel_MauiApp3CheckNetworkAccess";
        const string channelName = "Default Channel MauiApp3CheckNetworkAccess";
        const string channelDescription = "The default channel for notifications.";

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string IdentifierKey = "identifier";

        bool channelInitialized = false;
        int pendingIntentId = 0;

        NotificationManagerCompat compatManager;

        public static LocalNotification Instance { get; private set; }
        public string NotificationIdentifier { get; set; }

        public LocalNotification()
        {
            if (Instance == null)
            {
                CreateNotificationChannel();
                compatManager = NotificationManagerCompat.From(Platform.AppContext);
                Instance = this;
            }
        }

        public Task<bool> RequestNotificationPermissionAsync()
        {
            // Leere Implementierung für Kompatibilität
            return Task.FromResult(true);
        }

        public async void RequestNotificationPermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                var status = await Permissions.CheckStatusAsync<NotificationPermission>();
                if (status != PermissionStatus.Granted)
                {
                    await Permissions.RequestAsync<NotificationPermission>();
                }
            }
        }

        public void ScheduleNotification(string identifier, string title, string body, DateTime? dateTime)
        {
            if (!channelInitialized)
            {
                CreateNotificationChannel();
            }

            if (dateTime == null)
                dateTime = DateTime.Now;

            // Prüfen der Berechtigung für exakte Alarme
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                AlarmManager alarmManager2 = Platform.AppContext.GetSystemService(Context.AlarmService) as AlarmManager;
                if (!alarmManager2.CanScheduleExactAlarms())
                {
                    // Hier sollten Sie den Benutzer zu den Einstellungen leiten
                    return;
                }
            }

            Intent intent = new Intent(Platform.AppContext, typeof(AlarmHandler));
            intent.SetAction("com.yourdomain.MauiApp3CheckNetworkAccess.ALARM_TRIGGER");  // Explizite Action
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, body);
            intent.PutExtra(IdentifierKey, identifier);
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop | ActivityFlags.IncludeStoppedPackages);

            var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;

            PendingIntent pendingIntent = PendingIntent.GetBroadcast(
                Platform.AppContext,
                identifier.GetHashCode(),
                intent,
                pendingIntentFlags
            );

            long triggerTime = GetNotifyTime(dateTime.Value);
            AlarmManager alarmManager = Platform.AppContext.GetSystemService(Context.AlarmService) as AlarmManager;

            // SetAlarmClock statt SetExactAndAllowWhileIdle verwenden
            var info = new AlarmManager.AlarmClockInfo(triggerTime, pendingIntent);
            alarmManager?.SetAlarmClock(info, pendingIntent);
        }

        public void Show(string title, string message, string identifier)
        {
            Intent intent = new Intent(Platform.AppContext, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            intent.PutExtra(IdentifierKey, identifier);
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;

            PendingIntent pendingIntent = PendingIntent.GetActivity(
                Platform.AppContext,
                identifier.GetHashCode(),
                intent,
                pendingIntentFlags
            );

            NotificationCompat.Builder builder = new NotificationCompat.Builder(Platform.AppContext, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(MauiApp3CheckNetworkAccess.Resource.Drawable.test)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetDefaults(NotificationCompat.DefaultAll);

            Notification notification = builder.Build();
            compatManager.Notify(identifier.GetHashCode(), notification);
        }

        public void RequestExactAlarmPermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                var alarmManager = (AlarmManager)Platform.AppContext.GetSystemService(Context.AlarmService);
                if (!alarmManager.CanScheduleExactAlarms())
                {
                    var intent = new Intent(Settings.ActionRequestScheduleExactAlarm);
                    intent.SetPackage(Platform.AppContext.PackageName);
                    intent.AddFlags(ActivityFlags.NewTask);
                    //Platform.AppContext.StartActivity(intent);
                    Platform.AppContext.StartForegroundService(intent);
                }
            }
        }

        //public void RemovePendingNotification(string identifier)
        //{
        //    Intent intent = new Intent(Platform.AppContext, typeof(AlarmHandler));

        //    var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        //        ? PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable
        //        : PendingIntentFlags.CancelCurrent;

        //    PendingIntent pendingIntent = PendingIntent.GetBroadcast(
        //        Platform.AppContext,
        //        identifier.GetHashCode(),
        //        intent,
        //        pendingIntentFlags
        //    );

        //    AlarmManager alarmManager = Platform.AppContext.GetSystemService(Context.AlarmService) as AlarmManager;
        //    alarmManager?.Cancel(pendingIntent);
        //}
        public void RemovePendingNotification(string identifier)
        {
            // Erstellen eines Intents, der auf den AlarmHandler verweist
            Intent intent = new Intent(Platform.AppContext, typeof(AlarmHandler));

            // Setzen der PendingIntentFlags basierend auf der Android-Version
            var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.CancelCurrent;

            // Erstellen eines PendingIntents mit dem Intent und dem Identifier
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(
                Platform.AppContext,
                identifier.GetHashCode(),
                intent,
                pendingIntentFlags
            );

            // Abrufen des AlarmManagers und Stornieren des PendingIntents
            AlarmManager alarmManager = Platform.AppContext.GetSystemService(Context.AlarmService) as AlarmManager;
            alarmManager?.Cancel(pendingIntent);
        }

        public void RemoveAllPendingNotifications()
        {
            // Android bietet keine direkte API zum Löschen aller geplanten Alarme
        }

        public void RemoveDeliveredNotification(string identifier)
        {
            compatManager.Cancel(identifier.GetHashCode());
        }

        public void RemoveAllDeliveredNotifications()
        {
            compatManager.CancelAll();
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.High)
                {
                    Description = channelDescription
                };

                var notificationManager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);
                channelInitialized = true;
            }
        }

        long GetNotifyTime(DateTime notifyTime)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            double epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
            return utcAlarmTime; // milliseconds
        }
    }

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class AlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(LocalNotification.TitleKey);
                string message = intent.GetStringExtra(LocalNotification.MessageKey);
                string identifier = intent.GetStringExtra(LocalNotification.IdentifierKey);

                LocalNotification manager = LocalNotification.Instance ?? new LocalNotification();
                manager.Show(title, message, identifier);
            }
        }
    }

    [BroadcastReceiver(Enabled = true, Label = "Reboot complete receiver", Exported = false)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class BootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //if (intent.Action == "android.intent.action.BOOT_COMPLETED")
            //{
            //    // Hier können Sie die Alarme nach einem Neustart neu erstellen
            //    // Sie müssen dafür die geplanten Benachrichtigungen persistent speichern
            //}
            if (intent.Action == Intent.ActionBootCompleted)
            {
                // Add your boot completed logic here
            }
        }
    }

    public class NotificationPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                var result = new List<(string androidPermission, bool isRuntime)>();
                if (OperatingSystem.IsAndroidVersionAtLeast(33))
                    result.Add((Manifest.Permission.PostNotifications, true));
                return result.ToArray();
            }
        }
    }
}










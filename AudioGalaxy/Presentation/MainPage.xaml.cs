namespace AudioGalaxy.Presentation;

#if __IOS__
using AVFoundation;
using Foundation;
using UIKit;
#endif

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    public void RecordAudioClicked(object sender, RoutedEventArgs e)
    {
#if __IOS__
        if(_audioRecorder is not null && _audioRecorder.Recording)
        {
            StopRecording();
            return;
        }

        _filePath = GetFilePath();

        if(AVAudioSession.SharedInstance() is {} session)
        {
            session.SetActive(true);
            session.SetCategory(AVAudioSessionCategory.PlayAndRecord);
            session.RequestRecordPermission(HandleMicPermissionCallback);
        }
#endif
    }

#if __IOS__
    private AVAudioRecorder? _audioRecorder;
    private string? _filePath;

    private string GetFilePath()
    {
        var fileName = string.IsNullOrEmpty(RecordingName.Text)
                    ? "MyAudioCat"
                    : RecordingName.Text;

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            $"{fileName}.m4a");        
    }

    private void HandleMicPermissionCallback(bool granted)
    {
        CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>  //older version do not require using UI thread
        {
            if(!granted)
            {
                ReportStatus("Mic Permission denied");
                return;
            }

            ReportStatus("Mic Permission granted");
            StartRecording();
        });
    }

    private void StopRecording()
    {
        if(_audioRecorder is { Recording: true })
        {
            ReportStatus("Stoping recording");
            _audioRecorder.Stop();
            RecordAudioButton.Content = "Start Recording";
            ReportStatus("Recording completed");
        }
    }

    private void ReportStatus(string message)
    {
        Console.WriteLine(message);
        RecordingStatus.Text += $"{message}\n";
    }

    private void StartRecording()
    {
        var settings = new AudioSettings
        {
            Format = AudioToolbox.AudioFormatType.MPEG4AAC,
            SampleRate = 12000,
            NumberChannels = 1,
            AudioQuality = AVAudioQuality.High
        };

        _audioRecorder = AVAudioRecorder.Create(NSUrl.FromFilename(_filePath), settings, out var error);

        if(error is not null || _audioRecorder is null)
        {
            Console.WriteLine($"Error creating recorder: {error?.LocalizedDescription}");
            return;
        }

        _audioRecorder.Delegate = new LocalAudioRecorderDelegate();

        ReportStatus($"Recording file on path {_filePath}");
        _audioRecorder.Record();
        RecordAudioButton.Content = "Stop Recording";
    }
#endif       
}

#if __IOS__
public class LocalAudioRecorderDelegate : AVAudioRecorderDelegate
{
    public override void FinishedRecording(AVAudioRecorder recorder, bool flag)
    {
        Console.WriteLine($"Finished recording. Successful: {flag}");
    }
}
#endif
